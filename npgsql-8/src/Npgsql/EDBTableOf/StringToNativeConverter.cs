using EDBTypes;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.PostgresTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace EnterpriseDB.EDBClient
{
    internal static class StringToNativeConverter
    {
        internal static object ConvertTextToNative(string token, PostgresType fieldPgType, PgSerializerOptions options)
        {
            if (fieldPgType is PostgresBaseType pgElementBaseType)
            {
                var boxedToken = StringToNativeConverter.ConvertDomainTypeTextToNative(options, token, pgElementBaseType);
                return boxedToken!;
            }
            else if (fieldPgType is PostgresEnumType pgEnumType)
            {
                var enumTypeInfo = options.GetDefaultTypeInfo(pgEnumType);
                if (enumTypeInfo is null || !enumTypeInfo.Type.IsEnum)
                {
                    // no enum mapping found, return token as string
                    return token;
                }
                else
                {
                    var enumValue = Enum.Parse(enumTypeInfo.Type, token, ignoreCase: true);
                    return enumValue;
                }
            }
            else
            {
                throw new EDBException($"{fieldPgType.GetType().Name} not supported for TABLE OF declarations. Please contact support.");
            }
        }

        private static object? ConvertDomainTypeTextToNative(PgSerializerOptions options, string token, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] PostgresType pgBaseType)
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
                    if (typeInfo.Type == typeof(string)) return string.Empty;
                    if (typeInfo.Type == typeof(bool)) return default(bool);
                    if (typeInfo.Type == typeof(short)) return default(short);
                    if (typeInfo.Type == typeof(int)) return default(int);
                    if (typeInfo.Type == typeof(long)) return default(long);
                    if (typeInfo.Type == typeof(ushort)) return default(ushort);
                    if (typeInfo.Type == typeof(uint)) return default(uint);
                    if (typeInfo.Type == typeof(ulong)) return default(ulong);
                    if (typeInfo.Type == typeof(float)) return default(float);
                    if (typeInfo.Type == typeof(double)) return default(double);
                    if (typeInfo.Type == typeof(decimal)) return default(decimal);
                    if (typeInfo.Type == typeof(Guid)) return Guid.Empty;
                    if (typeInfo.Type == typeof(DateTime)) return DateTime.MinValue;
                    if (typeInfo.Type == typeof(DateTimeOffset)) return DateTimeOffset.MinValue;
                    if (typeInfo.Type == typeof(EDBCidr)) return new EDBCidr();
                    if (typeInfo.Type == typeof(PhysicalAddress)) return default(PhysicalAddress);

                    nativeValue = Activator.CreateInstance(typeInfo.Type);
                }
            }
            else
            {
                if (typeInfo.Type == typeof(string))
                {
                    if (typeInfo.PgTypeId != null
                        && typeInfo.PgTypeId.Value.Oid == 1042 && token.Length == 1)
                        return token[0]; // char
                    else
                        return token;
                }
                if (typeInfo.Type == typeof(bool)) return (bool)(token == "t");
                if (typeInfo.Type == typeof(short)) return short.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(int)) return int.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(long)) return long.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(ushort)) return ushort.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(uint)) return uint.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(ulong)) return ulong.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(float)) return float.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(double)) return double.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(decimal)) return pgBaseType.Name == "money" ? ParseMoney(token) : decimal.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(Guid)) return Guid.Parse(token);
                if (typeInfo.Type == typeof(DateTime)) return DateTime.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(DateTimeOffset)) return DateTimeOffset.Parse(token, CultureInfo.InvariantCulture);
                if (typeInfo.Type == typeof(EDBCidr)) return new EDBCidr(token);
                if (typeInfo.Type == typeof(IPAddress)) return new EDBInet(token);
                if (typeInfo.Type == typeof(EDBInterval)) return EDBIntervalExtensions.Parse(token);
                if (typeInfo.Type == typeof(TimeSpan)) return TimeSpan.Parse(token, CultureInfo.InvariantCulture);
#if NET5_0_OR_GREATER
                if (typeInfo.Type == typeof(PhysicalAddress)) return PhysicalAddress.Parse(token);
#else
                // see https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.physicaladdress.parse?view=netframework-4.7.2#system-net-networkinformation-physicaladdress-parse(system-string)
                if (typeInfo.Type == typeof(PhysicalAddress)) return PhysicalAddress.Parse(token.ToUpper().Replace(':', '-'));
#endif
                if (typeInfo.Type == typeof(EDBTsQuery)) return EDBTsQuery.Parse(token);
                if (typeInfo.Type == typeof(EDBTsVector)) return EDBTsVector.Parse(token);
                if (typeInfo.Type == typeof(EDBPoint)) return EDBPoint.Parse(token);
                if (typeInfo.Type == typeof(EDBLSeg)) return EDBLSeg.Parse(token);
                if (typeInfo.Type == typeof(EDBPath)) return EDBPath.Parse(token);
                if (typeInfo.Type == typeof(EDBPolygon)) return EDBPolygon.Parse(token);
                if (typeInfo.Type == typeof(EDBLine)) return EDBLine.Parse(token);
                if (typeInfo.Type == typeof(EDBBox)) return EDBBox.Parse(token);
                if (typeInfo.Type == typeof(EDBCircle)) return EDBCircle.Parse(token);
                if (typeInfo.Type == typeof(Dictionary<string, string>)) // hstore
                {
                    return HstoreStringToNative(token);
                }
                if (typeInfo.Type == typeof(object)
                        && typeInfo.PgTypeId != null
                        && typeInfo.PgTypeId.Value.Oid == 1560 /*bit or bit array*/)
                {
                    if (token.Length == 1) // bit
                    {
                        return token == "1";
                    }
                    else
                    {
                        return new BitArray(token.Select(c => c == '1').ToArray());
                    }
                }
                if (typeInfo.Type == typeof(object)
                        && typeInfo.PgTypeId != null
                        && typeInfo.PgTypeId.Value.Oid == 1562 /*bit varying */)
                {
                    return new BitArray(token.Select(c => c == '1').ToArray());
                }

                nativeValue = Convert.ChangeType(token, typeInfo.Type);

            }

            return nativeValue;
        }

        /// <summary>
        /// Translates any digit or decimal point to a culture invariant version and parses the resulting string to decimal
        /// </summary>
        internal static decimal ParseMoney(string token)
        {
            // Remove all non digits chars except commas and dots
            var sb = new StringBuilder(token.Length);
            int index = 0;
            foreach (var c in token)
            {
                if (c >= '0' && c <= '9')
                {
                    sb.Append(c);
                }
                else if (c == '-' || c == '−') // negative signs variations
                {
                    sb.Append(CultureInfo.InvariantCulture.NumberFormat.NegativeSign);
                }
                else if (c == '(' && index == 0)
                {
                    sb.Append(CultureInfo.InvariantCulture.NumberFormat.NegativeSign);
                }
                else if (c == ',' || c == '٫' // commas variations
                    || c == '.')
                {
                    sb.Append(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator);
                }
                index++;
            }
            var finalToken = sb.ToString();

            return decimal.Parse(finalToken, NumberStyles.Currency | NumberStyles.AllowLeadingSign | NumberStyles.AllowParentheses, CultureInfo.InvariantCulture);
        }

        private static string[] HstoreTupleSeparator = new string[] { "\", \"", "\",\"" };
        private static string[] HstoreKeyvalueSeparator = new string[] { "=>" };

        private static object HstoreStringToNative(string token)
        {
            var pairs = token.Split(HstoreTupleSeparator, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> hstore = new Dictionary<string, string>(pairs.Length);
            foreach (var pair in pairs)
            {
                var keyvalue = pair.Split(HstoreKeyvalueSeparator, StringSplitOptions.None);
                if (keyvalue.Length != 2) throw new EDBException("Hstore text parsing failed. Invalid format.");

                var key = keyvalue[0].Trim('"');
                var value = keyvalue[1].Trim('"');

                hstore.Add(key, value);
            }
            return hstore;
        }

        internal static bool ShouldProcessWholeTuple(PostgresBaseType pgBaseType)
        {
            return
                pgBaseType.OID == 600 // point
                || pgBaseType.OID == 603 // box
                || pgBaseType.OID == 604 // polygon
                ;

        }

        internal static char GetTupleSeparator(PostgresBaseType pgBaseTypeInstance) => pgBaseTypeInstance.OID switch
        {
            603 => ';', // box
            _ => ','
        };
    }
}
