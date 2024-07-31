using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient
{
    internal static class ArrayBackendToNativeTypeConverter
    {

        /// <summary>
        /// Creates an array list from EPAS table of represenation (array of domain or tuples)
        /// Multidimensional arrays are treated as ArrayLists of ArrayLists
        /// </summary>
        public static ArrayList ToArrayList(string BackendData, PgSerializerOptions? options, PostgresArrayType? pgType)
        {

            /* Examples :
             * 
             * Array of domain types :  {7369,7499,7521}
             * Array of tuples with space-escaped ones : {\"(ACCOUNTING,\\\"NEW YORK\\\")\",\"(OPERATIONS,BOSTON)\",\"(RESEARCH,DALLAS)\",\"(SALES,CHICAGO)\"}
            */

            ArrayList list = new ArrayList();
            //remove the braces on either side and work on what they contain.
            var stripBracesSpan = BackendData.AsSpan().Trim().Slice(1, BackendData.Length - 2).Trim();
            string stripBraces = stripBracesSpan.ToString();
            if (stripBraces.Length == 0)
            {
                return list;
            }
            if (stripBraces.Length > 2 && stripBraces.Substring(0, 2) == "\"(")
            // there are tuples inside
            {
                foreach (string arrayChunk in TupleChunkEnumeration(stripBraces))
                {
                    list.Add(ToArrayList(arrayChunk, options, pgType));
                }
            }
            else
            //We're either dealing with a 1-dimension array or treating a row of an n-dimension array. In either case parse the elements and put them in our ArrayList
            {
                int fieldIndex = 0;
                foreach (string token in TokenEnumeration(stripBraces))
                {
                    if (pgType is null || options is null) // here for EDBTextConverterTests which passes null for both
                    {
                        list.Add(token);
                        continue;
                    }

                    //Use the NpgsqlBackendTypeInfo for the element type to obtain each element.
                    if (pgType.Element is PostgresBaseType pgBaseType)
                    {
                        var boxedToken = ConvertStringToNative(options, token, pgBaseType);
                        list.Add(boxedToken);
                    }
                    else if (pgType.Element is PostgresCompositeType pgCompositeType)
                    {
                        if (pgCompositeType.MutableFields[fieldIndex++].Type is PostgresBaseType fieldType)
                        {
                            var boxedToken = ConvertStringToNative(options, token, fieldType);
                            list.Add(boxedToken);
                        }
                        else
                        {
                            throw new NotSupportedException($"Table of nested types should contain only primitive types or composite types (type found: {pgCompositeType.MutableFields[fieldIndex].Type}. Please file a bug");
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Table of nested types should contain only primitive types or composite types. Please file a bug");
                    }

                }
            }
            return list;


            static object? ConvertStringToNative(PgSerializerOptions options, string token, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]  PostgresBaseType pgBaseType)
            {
                var typeInfo = options.GetObjectOrDefaultTypeInfo(pgBaseType)
                                           ?? throw new NotSupportedException(
                                               $"Reading isn't supported for record field 0 (PG type '{pgBaseType.DisplayName}'");

                object? nativeValue;
                if (token == "NULL")
                {
                    // Slow. Fast replacement here :  https://devblogs.microsoft.com/premier-developer/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/
                    if (typeInfo.Type.IsByRef)
                    {
                        nativeValue = null;
                    }
                    else
                    {
                        nativeValue = Activator.CreateInstance(typeInfo.Type);
                    }
                }
                else
                {
                    nativeValue = Convert.ChangeType(token, typeInfo.Type);
                }

                return nativeValue;
            }
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
        private static IEnumerable<string> TupleChunkEnumeration(string source)
        {
            char lastEscaped = '\0';
            bool inTuple = false;
            int escapeLevel = 0;
            StringBuilder sb = new StringBuilder(source.Length);

            int idx = 0;
            while (idx < source.Length)
            {
                char c = source[idx];
                switch (c)
                {
                case '"': //entering of leaving a quoted string
                    if (inTuple)
                    {
                        sb.Append(c);
                    }

                    break;
                case ',': //ending this item, unless we're in a quoted string.
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

                    sb.Append(c);

                    break;
                case ')':
                    if (inTuple && escapeLevel == 0)
                        inTuple = false;

                    sb.Append(c);
                    break;

                default:
                    sb.Append(c);
                    break;
                }
                idx++;
            }
            yield return sb.ToString();

        }
    }
}
