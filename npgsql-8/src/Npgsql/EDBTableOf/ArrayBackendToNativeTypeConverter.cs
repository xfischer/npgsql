using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient;

internal static class ArrayBackendToNativeTypeConverter
{
    private struct TypeDescriptor
    {
        public bool RequiresSpecificParsing;
        public PostgresType PostgresType;
        public char TupleSeparator;
    }

    /// <summary>
    /// Creates an array list from EPAS table of represenation (array of domain/tuples/composites)
    /// If a type is a composite, we explore the fiels and recursively extract the tokens to build the final composite
    /// If it's a domain type, we process each tuple and check if a specialized parse is needed (ie point, box, line, ...)
    /// Otherwise we convert each token separately and return the native token values
    /// </summary>
    public static ArrayList ToArrayList(string BackendData, PgSerializerOptions? options, PostgresArrayType? pgType)
    {
        /* Examples :
         * Points: "{\"(0,0)\",\"(-4.2,43.5)\"}"
         * Table of real,real: "{\"(-5.2,43.5)\",\"(-5.2,43.5)\"}"
         * Array of domain types :  {7369,7499,7521}
         * Array of tuples with space-escaped ones : {\"(ACCOUNTING,\\\"NEW YORK\\\")\",\"(OPERATIONS,BOSTON)\",\"(RESEARCH,DALLAS)\",\"(SALES,CHICAGO)\"}
        */


        // If composite, call dedicated method
        if (pgType?.Element is PostgresCompositeType pgCompositeType)
        {
            using var tokenEnumerator = BackendTextEnumerator.EnumerateTokens(BackendData).GetEnumerator();
            return ToArrayList_Composite(tokenEnumerator, options, pgCompositeType, head: true);
        }

        //remove the braces on either side and work on what they contain.
        var stripBracesSpan = BackendData.AsSpan().Trim().Slice(1, BackendData.Length - 2).Trim();
        string stripBraces = stripBracesSpan.ToString();
        if (stripBraces.Length == 0)
        {
            return new();
        }

        var list = new ArrayList();
        TypeDescriptor pgTypeDescriptor = GetElementTypeDescription(options, pgType);
        if ((stripBraces.Length > 2 && stripBraces.Substring(0, 2) == "\"(")
            || pgTypeDescriptor.RequiresSpecificParsing)
        // there are tuples inside
        {
            foreach (string arrayChunk in TupleChunkEnumeration(stripBraces, pgTypeDescriptor.TupleSeparator))
            {
                if (pgTypeDescriptor.RequiresSpecificParsing)
                {
                    list.Add(StringToNativeConverter.ConvertTextToNative(arrayChunk, pgTypeDescriptor.PostgresType, options));
                }
                else
                {
                    list.Add(ToArrayList(arrayChunk, options, pgType));
                }
            }
        }
        else
        //We're either dealing with a 1-dimension array or treating a row of an n-dimension array. In either case parse the elements and put them in our ArrayList
        {
            foreach (string token in TokenEnumeration(stripBraces))
            {
                if (pgType is null || options is null) // here for EDBTextConverterTests which passes null for both
                {
                    list.Add(token);
                }
                else
                {
                    var nativeValue = StringToNativeConverter.ConvertTextToNative(token, pgType.Element, options);
                    list.Add(nativeValue);
                }
            }
        }
        return list;
    }

    private static ArrayList ToArrayList_Composite(IEnumerator<string> tokenEnumerator, PgSerializerOptions? options, PostgresCompositeType pgCompType, bool head)
    {
        /* Examples :
         * Array of tuples with space-escaped ones : {\"(ACCOUNTING,\\\"NEW YORK\\\")\",\"(OPERATIONS,BOSTON)\",\"(RESEARCH,DALLAS)\",\"(SALES,CHICAGO)\"}
        */

        if (options == null)
            throw new EDBException("PgSerializerOptions is required");

        var list = new ArrayList();
        List<object> compositeFieldValues = new(30);
        //remove the braces on either side and work on what they contain.

        bool endOfBuffer = false;

        do
        {
            foreach (var field in pgCompType.Fields)
            {
                if (field.Type is PostgresCompositeType pgNestedComposite)
                {
                    var nestedComposite = ToArrayList_Composite(tokenEnumerator, options, pgNestedComposite, head: false);
                    if (nestedComposite.Count == 0)
                    {
                        endOfBuffer = true;
                        break;
                    }
                    // Add composite. If mapped we have a single typed element
                    // otherwise we get all individual fields
                    foreach(var comp in nestedComposite)
                    {
                        compositeFieldValues.Add(comp);
                    }
                }
                else if (tokenEnumerator.MoveNext())
                {                    
                    var token = tokenEnumerator.Current;
                    var nativeValue = StringToNativeConverter.ConvertTextToNative(token, field.Type, options);
                    compositeFieldValues.Add(nativeValue);
                }
                else
                {
                    endOfBuffer = true;
                    break;
                }
            }

            if (endOfBuffer)
                break;

            // Create composite instance if converter found
            var pgCompTypeInfo = options.GetObjectOrDefaultTypeInfo(pgCompType);            
            var converter = pgCompTypeInfo?.GetResolution().Converter;
            if (converter is not null && converter is ITextFormatConverter textConverter)
            {
                var compositeInstance = textConverter.ReadFromValues(compositeFieldValues.ToArray());
                list.Add(compositeInstance);
            }
            else
            {
                // No converter found, return each field in a separate arraylist slot
                list.Add(new ArrayList(compositeFieldValues));
            }

            compositeFieldValues.Clear();
        }
        while (head && !endOfBuffer);

        return list;
    }

    private static TypeDescriptor GetElementTypeDescription(PgSerializerOptions? options, PostgresArrayType? pgType)
    {
        var result = new TypeDescriptor();
        result.RequiresSpecificParsing = false;
        result.TupleSeparator = ',';
        result.PostgresType = null!;

        if (pgType is null || options is null)
        {
            return result;
        }

        if (pgType.Element is PostgresBaseType pgBaseTypeInstance
            && StringToNativeConverter.ShouldProcessWholeTuple(pgBaseTypeInstance))
        {
            result.TupleSeparator = StringToNativeConverter.GetTupleSeparator(pgBaseTypeInstance);
            result.PostgresType = pgBaseTypeInstance;
            result.RequiresSpecificParsing = true;
            return result;
        }

        return result;
    }

    /// <summary>
    /// Takes a string representation of a pg 1-dimensional array
    /// (or a 1-dimensional row within an n-dimensional array)
    /// and allows enumeration of the string represenations of each items.
    /// </summary>
    private static IEnumerable<string> TokenEnumeration(string source)
    {
        bool inQuoted = false;
        StringBuilder sb = new StringBuilder(source.Length);
        //We start of not in a quoted section, with an empty StringBuilder.
        //We iterate through each character. Generally we add that character
        //to the string builder, but in the case of special characters we
        //have different handling. When we reach a comma that isn't in a quoted
        //section we yield return the string we have built and then clear it
        //to continue with the next.
        int idx = 0;
        while (idx < source.Length)
        {
            char c = source[idx];
            switch (c)
            {
            case '"': //entering of leaving a quoted string
                inQuoted = !inQuoted;
                break;
            case ',': //ending this item, unless we're in a quoted string.
                if (inQuoted)
                {
                    sb.Append(',');
                }
                else
                {
                    yield return sb.ToString().Trim('"');
                    sb = new StringBuilder(source.Length - idx);
                }
                break;
            case '\\': //next char is an escaped character, grab it, ignore the \ we are on now.
                sb.Append(source[++idx]);
                break;
            default:
                sb.Append(c);
                break;
            }
            idx++;
        }
        yield return sb.ToString();
    }

    /// <summary>
    /// Takes a string representation of a pg n-dimensional array
    /// and allows enumeration of the string represenations of the next
    /// lower level of rows (which in turn can be taken as (n-1)-dimensional arrays.
    /// </summary>
    private static IEnumerable<string> TupleChunkEnumeration(string source, char tupleSeparator)
    {
        char lastEscaped = '\0';
        bool inTuple = false;
        int escapeLevel = 0;
        int parenthesisDepth = 0;
        StringBuilder sb = new StringBuilder(source.Length);

        int idx = 0;
        while (idx < source.Length)
        {
            char c = source[idx];
            if (c == tupleSeparator) //ending this item, unless we're in a quoted string.
            {
                if (inTuple)
                {
                    sb.Append(c);
                }
                else
                {
                    yield return sb.ToString();
                    sb = new StringBuilder(source.Length - idx);
                    lastEscaped = '\0';
                    escapeLevel = 0;
                }
            }
            else
            {
                switch (c)
                {
                case '"': //entering of leaving a quoted string
                    if (inTuple)
                    {
                        sb.Append(c);
                    }

                    break;
                case '\\': //next char is an escaped character, grab it, ignore the \ we are on now.
                    if (source[idx + 1] == lastEscaped)
                    {
                        escapeLevel--;
                        idx++;
                    }
                    else
                    {
                        escapeLevel++;
                        lastEscaped = source[++idx];
                    }
                    if (inTuple)
                        sb.Append(lastEscaped);
                    break;
                case '(':
                    if (!inTuple)
                        inTuple = true;

                    parenthesisDepth++;

                    sb.Append(c);

                    break;
                case ')':
                    parenthesisDepth--;

                    if (inTuple && parenthesisDepth == 0 && escapeLevel == 0)
                        inTuple = false;

                    sb.Append(c);
                    break;

                default:
                    sb.Append(c);
                    break;
                }
            }
            idx++;
        }
        yield return sb.ToString();

    }
}
