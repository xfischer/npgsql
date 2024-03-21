// EDBTypes.EDBTypesHelper.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Text;
using System.IO;
using EnterpriseDB.EDBClient;

namespace EDBTypes
{
    /// <summary>
    ///    This class contains helper methods for type conversion between
    /// the .Net type system and postgresql.
    /// </summary>
    internal static class EDBTypesHelper
    {
        // Logging related values
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

        // This is used by the test suite to test both text and binary encodings on version 3 connections.
        // See EDBTests.BaseClassTests.TestFixtureSetup() and InitBinaryBackendSuppression().
        // If this field is changed or removed, some tests will become partially non-functional, and an error will be issued.
        internal static bool SuppressBinaryBackendEncoding = false;

        private struct MappingKey : IEquatable<MappingKey>
        {
            public readonly Version Version;
            public readonly bool UseExtendedTypes;

            public MappingKey(EDBConnector conn)
            {
                Version = conn.ServerVersion;
                UseExtendedTypes = conn.UseExtendedTypes;
            }

            public bool Equals(MappingKey other)
            {
                return UseExtendedTypes.Equals(other.UseExtendedTypes) && Version.Equals(other.Version);
            }

            public override bool Equals(object obj)
            {
                //Note that Dictionary<T, U> will call IEquatable<T>.Equals() when possible.
                //This is included for completeness (that and second-guessing Mono while coding on .NET!).
                return obj != null && obj is MappingKey && Equals((MappingKey) obj);
            }

            public override int GetHashCode()
            {
                return UseExtendedTypes ? ~Version.GetHashCode() : Version.GetHashCode();
            }
        }

        /// <summary>
        /// A cache of basic datatype mappings keyed by server version.  This way we don't
        /// have to load the basic type mappings for every connection.
        /// </summary>
        private static readonly Dictionary<MappingKey, EDBBackendTypeMapping> BackendTypeMappingCache =
            new Dictionary<MappingKey, EDBBackendTypeMapping>();

        private static readonly EDBNativeTypeMapping NativeTypeMapping = PrepareDefaultTypesMap();

        private static readonly Version EDB207 = new Version("2.0.7");

        private static readonly Dictionary<string, EDBBackendTypeInfo> DefaultBackendInfoMapping = PrepareDefaultBackendInfoMapping();

        private static Dictionary<string, EDBBackendTypeInfo> PrepareDefaultBackendInfoMapping()
        {
            Dictionary<string, EDBBackendTypeInfo> NameIndex = new Dictionary<string, EDBBackendTypeInfo>();

            foreach (EDBBackendTypeInfo TypeInfo in TypeInfoList(false, new Version("1000.0.0.0")))
            {
                NameIndex.Add(TypeInfo.Name, TypeInfo);

                //do the same for the equivalent array type.
                NameIndex.Add("_" + TypeInfo.Name, ArrayTypeInfo(TypeInfo));

            }

            return NameIndex;
        }

        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given EDBDbType.
        /// </summary>
        public static bool TryGetBackendTypeInfo(String BackendTypeName, out EDBBackendTypeInfo TypeInfo)
        {
            return DefaultBackendInfoMapping.TryGetValue(BackendTypeName, out TypeInfo);

        }

        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given EDBDbType.
        /// </summary>
        public static bool TryGetNativeTypeInfo(EDBDbType dbType, out EDBNativeTypeInfo typeInfo)
        {
            return NativeTypeMapping.TryGetValue(dbType, out typeInfo);
        }

        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given DbType.
        /// </summary>
        public static bool TryGetNativeTypeInfo(DbType dbType, out EDBNativeTypeInfo typeInfo)
        {
            return NativeTypeMapping.TryGetValue(dbType, out typeInfo);
        }

        public static EDBNativeTypeInfo GetNativeTypeInfo(DbType DbType)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBNativeTypeInfo ret = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return TryGetNativeTypeInfo(DbType, out ret) ? ret : null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private static bool TestTypedEnumerator(Type type, out Type typeOut)
        {
            if (type.IsArray)
            {
                typeOut = type.GetElementType();
                return true;
            }
            //We can only work out the element type for IEnumerable<T> not for IEnumerable
            //so we are looking for IEnumerable<T> for any value of T.
            //So we want to find an interface type where GetGenericTypeDefinition == typeof(IEnumerable<>);
            //And we can only safely call GetGenericTypeDefinition() if IsGenericType is true, but if it's false
            //then the interface clearly isn't an IEnumerable<T>.
            foreach (Type iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition().Equals(typeof (IEnumerable<>)))
                {
                    typeOut = iface.GetGenericArguments()[0];
                    return true;
                }
            }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            typeOut = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return false;
        }

        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given System.Type.
        /// </summary>
        public static bool TryGetNativeTypeInfo(Type type, out EDBNativeTypeInfo typeInfo)
        {
            if (NativeTypeMapping.TryGetValue(type, out typeInfo))
            {
                return true;
            }
            // At this point there is no direct mapping, so we see if we have an array or IEnumerable<T>.
            // Note that we checked for a direct mapping first, so if there is a direct mapping of a class
            // which implements IEnumerable<T> we will use that (currently this is only string, which
            // implements IEnumerable<char>.

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Type elementType = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBNativeTypeInfo elementTypeInfo = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (TestTypedEnumerator(type, out elementType) && TryGetNativeTypeInfo(elementType, out elementTypeInfo))
            {
                typeInfo = EDBNativeTypeInfo.ArrayOf(elementTypeInfo);
                return true;
            }
            return false;
        }

        public static EDBNativeTypeInfo GetNativeTypeInfo(Type Type)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBNativeTypeInfo ret = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return TryGetNativeTypeInfo(Type, out ret) ? ret : null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static bool DefinedType(Type type)

        {
            return NativeTypeMapping.ContainsType(type);
        }

        public static bool DefinedType(object item)

        {
            return DefinedType(item.GetType());
        }

        ///<summary>
        /// This method is responsible to convert the byte[] received from the backend
        /// to the corresponding EDBType.
        /// The given TypeInfo is called upon to do the conversion.
        /// If no TypeInfo object is provided, no conversion is performed.
        /// </summary>
        public static Object ConvertBackendBytesToSystemType(EDBBackendTypeInfo TypeInfo, Byte[] data, Int32 fieldValueSize,
                                                             Int32 typeModifier)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");

            if (TypeInfo != null)
            {
                return TypeInfo.ConvertBackendBinaryToNative(data, fieldValueSize, typeModifier);
            }
            else
            {
                return data;
            }
        }

        ///<summary>
        /// This method is responsible to convert the string received from the backend
        /// to the corresponding EDBType.
        /// The given TypeInfo is called upon to do the conversion.
        /// If no TypeInfo object is provided, no conversion is performed.
        /// </summary>
        public static Object ConvertBackendStringToSystemType(EDBBackendTypeInfo TypeInfo, Byte[] data, Int16 typeSize,
                                                              Int32 typeModifier)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

            if (TypeInfo != null)
            {
                return TypeInfo.ConvertBackendTextToNative(data, typeSize, typeModifier);
            }
            else
            {
                return BackendEncoding.UTF8Encoding.GetString(data);
            }
        }

        /// <summary>
        /// Create the one and only native to backend type map.
        /// This map is used when formatting native data
        /// types to backend representations.
        /// </summary>
        private static EDBNativeTypeMapping PrepareDefaultTypesMap()
        {
            EDBNativeTypeMapping nativeTypeMapping = new EDBNativeTypeMapping();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            nativeTypeMapping.AddType("name", EDBDbType.Name, DbType.String, true, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            nativeTypeMapping.AddType("oidvector", EDBDbType.Oidvector, DbType.String, true, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Conflicting types should have mapped first the non default mappings.
            // For example, char, varchar and text map to DbType.String. As the most
            // common is to use text with string, it has to be the last mapped, in order
            // to type mapping has the last entry, in this case, text, as the map value
            // for DbType.String.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            nativeTypeMapping.AddType("refcursor", EDBDbType.RefCursor, DbType.String, true, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            nativeTypeMapping.AddType("char", EDBDbType.Char, DbType.String, false,
                                            BasicNativeToBackendTypeConverter.StringToTextText,
                                            BasicNativeToBackendTypeConverter.StringToTextBinary);

            nativeTypeMapping.AddTypeAlias("char", typeof(Char));

            nativeTypeMapping.AddType("varchar", EDBDbType.Varchar, DbType.String, false,
                                            BasicNativeToBackendTypeConverter.StringToTextText,
                                            BasicNativeToBackendTypeConverter.StringToTextBinary);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            nativeTypeMapping.AddType("varchar2", EDBDbType.Varchar2, DbType.String, true, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Dummy type that facilitates non-binary string conversions for types that are treated as
            // text but which are not really text.  Those types cause problems if they are encoded as binary.
            // The mapping EDBDbType.Text => text_nonbinary is removed when text is mapped.
            // DBType.Object will be re-mapped to this type at the end.
            nativeTypeMapping.AddType("unknown", EDBDbType.Text, DbType.Object, true);

            nativeTypeMapping.AddType("text", EDBDbType.Text, DbType.String, false,
                                            BasicNativeToBackendTypeConverter.StringToTextText,
                                            BasicNativeToBackendTypeConverter.StringToTextBinary);

            nativeTypeMapping.AddDbTypeAlias("text", DbType.StringFixedLength);
            nativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiString);
            nativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiStringFixedLength);

            nativeTypeMapping.AddTypeAlias("text", typeof(String));

            nativeTypeMapping.AddType("bytea", EDBDbType.Bytea, DbType.Binary, false,
                                            BasicNativeToBackendTypeConverter.ByteArrayToByteaText,
                                            BasicNativeToBackendTypeConverter.ByteArrayToByteaBinary);

            nativeTypeMapping.AddTypeAlias("bytea", typeof(Byte[]));

            nativeTypeMapping.AddType("bit", EDBDbType.Bit, DbType.Object, false,
                                            BasicNativeToBackendTypeConverter.ToBit);

            nativeTypeMapping.AddTypeAlias("bit", typeof(BitString));

            nativeTypeMapping.AddType("bool", EDBDbType.Boolean, DbType.Boolean, false,
                                            BasicNativeToBackendTypeConverter.BooleanToBooleanText,
                                            BasicNativeToBackendTypeConverter.BooleanToBooleanBinary);

            nativeTypeMapping.AddTypeAlias("bool", typeof(Boolean));

            nativeTypeMapping.AddType("int2", EDBDbType.Smallint, DbType.Int16, false,
                                            BasicNativeToBackendTypeConverter.ToBasicType<short>,
                                            BasicNativeToBackendTypeConverter.Int16ToInt2Binary);

            nativeTypeMapping.AddTypeAlias("int2", typeof(UInt16));

            nativeTypeMapping.AddTypeAlias("int2", typeof(Int16));

            nativeTypeMapping.AddDbTypeAlias("int2", DbType.Byte);

            nativeTypeMapping.AddTypeAlias("int2", typeof(Byte));

            nativeTypeMapping.AddType("int4", EDBDbType.Integer, DbType.Int32, false,
                                            BasicNativeToBackendTypeConverter.ToBasicType<int>,
                                            BasicNativeToBackendTypeConverter.Int32ToInt4Binary);

            nativeTypeMapping.AddTypeAlias("int4", typeof(Int32));

            nativeTypeMapping.AddType("int8", EDBDbType.Bigint, DbType.Int64, false,
                                            BasicNativeToBackendTypeConverter.ToBasicType<long>,
                                            BasicNativeToBackendTypeConverter.Int64ToInt8Binary);

            nativeTypeMapping.AddTypeAlias("int8", typeof(Int64));

         /*   TODO ZK 
          * 
           nativeTypeMapping.AddType("float4", EDBDbType.Real, DbType.Single, false,
                                            BasicNativeToBackendTypeConverter.SingleToFloat4Text,
                                            BasicNativeToBackendTypeConverter.SingleToFloat4Binary);
          */

            nativeTypeMapping.AddType("float4", EDBDbType.Float, DbType.Single, false,
                                       BasicNativeToBackendTypeConverter.SingleToFloat4Text,
                                       BasicNativeToBackendTypeConverter.SingleToFloat4Binary);

            
            nativeTypeMapping.AddTypeAlias("float4", typeof(Single));

            nativeTypeMapping.AddType("float8", EDBDbType.Double, DbType.Double, false,
                                            BasicNativeToBackendTypeConverter.DoubleToFloat8Text,
                                            BasicNativeToBackendTypeConverter.DoubleToFloat8Binary);

            nativeTypeMapping.AddTypeAlias("float8", typeof(Double));

            nativeTypeMapping.AddType("numeric", EDBDbType.Numeric, DbType.Decimal, false,
                                            BasicNativeToBackendTypeConverter.ToBasicType<decimal>);

            nativeTypeMapping.AddTypeAlias("numeric", typeof (Decimal));

            nativeTypeMapping.AddType("money", EDBDbType.Money, DbType.Currency, true,
                                            BasicNativeToBackendTypeConverter.ToMoney);

            nativeTypeMapping.AddType("date", EDBDbType.Date, DbType.Date, true,
                                            BasicNativeToBackendTypeConverter.ToDate);

            nativeTypeMapping.AddTypeAlias("date", typeof (EDBDate));

            nativeTypeMapping.AddType("timetz", EDBDbType.TimeTZ, DbType.Time, true,
                                            ExtendedNativeToBackendTypeConverter.ToTimeTZ);

            nativeTypeMapping.AddTypeAlias("timetz", typeof (EDBTimeTZ));

            nativeTypeMapping.AddType("time", EDBDbType.Time, DbType.Time, true,
                                            BasicNativeToBackendTypeConverter.ToTime);

            nativeTypeMapping.AddTypeAlias("time", typeof (EDBTime));

            nativeTypeMapping.AddType("timestamptz", EDBDbType.TimestampTZ, DbType.DateTime, true,
                                            ExtendedNativeToBackendTypeConverter.ToTimeStamp);

            nativeTypeMapping.AddTypeAlias("timestamptz", typeof(EDBTimeStampTZ));

            nativeTypeMapping.AddDbTypeAlias("timestamptz", DbType.DateTimeOffset);

            nativeTypeMapping.AddTypeAlias("timestamptz", typeof(DateTimeOffset));

            nativeTypeMapping.AddType("abstime", EDBDbType.Abstime, DbType.DateTime, true,
                                            ExtendedNativeToBackendTypeConverter.ToTimeStamp);

            nativeTypeMapping.AddType("timestamp", EDBDbType.Timestamp, DbType.DateTime, true,
                                            ExtendedNativeToBackendTypeConverter.ToTimeStamp);

            nativeTypeMapping.AddTypeAlias("timestamp", typeof (DateTime));
            nativeTypeMapping.AddTypeAlias("timestamp", typeof (EDBTimeStamp));

            nativeTypeMapping.AddType("point", EDBDbType.Point, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToPoint);

            nativeTypeMapping.AddTypeAlias("point", typeof (EDBPoint));

            nativeTypeMapping.AddType("box", EDBDbType.Box, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToBox);

            nativeTypeMapping.AddTypeAlias("box", typeof (EDBBox));

            nativeTypeMapping.AddType("lseg", EDBDbType.LSeg, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToLSeg);

            nativeTypeMapping.AddTypeAlias("lseg", typeof (EDBLSeg));

            nativeTypeMapping.AddType("path", EDBDbType.Path, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToPath);

            nativeTypeMapping.AddTypeAlias("path", typeof (EDBPath));

            nativeTypeMapping.AddType("polygon", EDBDbType.Polygon, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToPolygon);

            nativeTypeMapping.AddTypeAlias("polygon", typeof (EDBPolygon));

            nativeTypeMapping.AddType("circle", EDBDbType.Circle, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToCircle);

            nativeTypeMapping.AddTypeAlias("circle", typeof (EDBCircle));

            nativeTypeMapping.AddType("inet", EDBDbType.Inet, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToIPAddress);

            nativeTypeMapping.AddTypeAlias("inet", typeof (IPAddress));
            nativeTypeMapping.AddTypeAlias("inet", typeof (EDBInet));

            nativeTypeMapping.AddType("macaddr", EDBDbType.MacAddr, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToMacAddress);

            nativeTypeMapping.AddTypeAlias("macaddr", typeof(PhysicalAddress));
            nativeTypeMapping.AddTypeAlias("macaddr", typeof(EDBMacAddress));

            nativeTypeMapping.AddType("uuid", EDBDbType.Uuid, DbType.Guid, true);
            nativeTypeMapping.AddTypeAlias("uuid", typeof (Guid));

            nativeTypeMapping.AddType("xml", EDBDbType.Xml, DbType.Xml, false,
                                            BasicNativeToBackendTypeConverter.StringToTextText,
                                            BasicNativeToBackendTypeConverter.StringToTextBinary);

            nativeTypeMapping.AddType("interval", EDBDbType.Interval, DbType.Object, true,
                                            ExtendedNativeToBackendTypeConverter.ToInterval);

            nativeTypeMapping.AddTypeAlias("interval", typeof (EDBInterval));
            nativeTypeMapping.AddTypeAlias("interval", typeof (TimeSpan));

            nativeTypeMapping.AddType("json", EDBDbType.Json, DbType.Object, false,
                BasicNativeToBackendTypeConverter.StringToTextText,
                BasicNativeToBackendTypeConverter.StringToTextBinary);

            nativeTypeMapping.AddType("jsonb", EDBDbType.Jsonb, DbType.Object, false,
                BasicNativeToBackendTypeConverter.StringToTextText,
                BasicNativeToBackendTypeConverter.StringToTextBinary);

            nativeTypeMapping.AddType("hstore", EDBDbType.Hstore, DbType.Object, false,
                BasicNativeToBackendTypeConverter.StringToTextText,
                BasicNativeToBackendTypeConverter.StringToTextBinary);

            nativeTypeMapping.AddDbTypeAlias("unknown", DbType.Object);

            return nativeTypeMapping;
        }

        private static IEnumerable<EDBBackendTypeInfo> TypeInfoList(bool useExtendedTypes, Version compat)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "oidvector", EDBDbType.Text, DbType.String, typeof (String), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "unknown", EDBDbType.Text, DbType.String, typeof (String),
                                            null,
                                            BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "refcursor", EDBDbType.RefCursor, DbType.String, typeof (String),  null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "char", EDBDbType.Char, DbType.String, typeof(String),
                                            null,
                                            BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "bpchar", EDBDbType.Text, DbType.String, typeof(String),
                                            null,
                                            BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "varchar", EDBDbType.Varchar, DbType.String, typeof(String),
                                            null,
                                            BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "varchar2", EDBDbType.Varchar2, DbType.String, typeof(String),
                                         null,
                                         BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.


#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "text", EDBDbType.Text, DbType.String, typeof(String),
                                            null,
                                            BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "name", EDBDbType.Name, DbType.String, typeof(String),
                                            null,
                                            BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            yield return
                new EDBBackendTypeInfo(0, "bytea", EDBDbType.Bytea, DbType.Binary, typeof(Byte[]),
                                            BasicBackendToNativeTypeConverter.ByteaTextToByteArray,
                                            BasicBackendToNativeTypeConverter.ByteaBinaryToByteArray);

            yield return
                new EDBBackendTypeInfo(0, "bit", EDBDbType.Bit, DbType.Object, typeof (BitString),
                                            BasicBackendToNativeTypeConverter.ToBit);

            yield return
                new EDBBackendTypeInfo(0, "bool", EDBDbType.Boolean, DbType.Boolean, typeof(Boolean),
                                            BasicBackendToNativeTypeConverter.BooleanTextToBoolean,
                                            BasicBackendToNativeTypeConverter.BooleanBinaryToBoolean);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "int2", EDBDbType.Smallint, DbType.Int16, typeof (Int16),
                                            null,
                                            BasicBackendToNativeTypeConverter.IntBinaryToInt);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "int4", EDBDbType.Integer, DbType.Int32, typeof (Int32),
                                            null,
                                            BasicBackendToNativeTypeConverter.IntBinaryToInt);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "int8", EDBDbType.Bigint, DbType.Int64, typeof (Int64),
                                            null,
                                            BasicBackendToNativeTypeConverter.IntBinaryToInt);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "oid", EDBDbType.Integer, DbType.Int32, typeof (Int32),
                                            null,
                                            BasicBackendToNativeTypeConverter.IntBinaryToInt);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "float4", EDBDbType.Real, DbType.Single, typeof(Single),
                                            null,
                                            BasicBackendToNativeTypeConverter.Float4Float8BinaryToFloatDouble);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "float8", EDBDbType.Double, DbType.Double, typeof(Double),
                                            null,
                                            BasicBackendToNativeTypeConverter.Float4Float8BinaryToFloatDouble);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "numeric", EDBDbType.Numeric, DbType.Decimal, typeof (Decimal), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            yield return
                new EDBBackendTypeInfo(0, "inet", EDBDbType.Inet, DbType.Object, typeof (EDBInet),
                                            ExtendedBackendToNativeTypeConverter.ToInet,
                                            typeof(IPAddress),
                                            ipaddress => (IPAddress)(EDBInet)ipaddress,
                                            npgsqlinet => (npgsqlinet is IPAddress ? (EDBInet)(IPAddress) npgsqlinet : npgsqlinet));
            yield return
                new EDBBackendTypeInfo(0, "macaddr", EDBDbType.MacAddr, DbType.Object, typeof(EDBMacAddress),
                                            ExtendedBackendToNativeTypeConverter.ToMacAddress,
                                            typeof(PhysicalAddress),
                                            macAddress => (PhysicalAddress)(EDBMacAddress)macAddress,
                                            npgsqlmacaddr => (npgsqlmacaddr is PhysicalAddress ? (EDBMacAddress)(PhysicalAddress)npgsqlmacaddr : npgsqlmacaddr));

            yield return
                new EDBBackendTypeInfo(0, "money", EDBDbType.Money, DbType.Currency, typeof (Decimal),
                                            BasicBackendToNativeTypeConverter.ToMoney);

            yield return
                new EDBBackendTypeInfo(0, "point", EDBDbType.Point, DbType.Object, typeof (EDBPoint),
                                            ExtendedBackendToNativeTypeConverter.ToPoint);

            yield return
                new EDBBackendTypeInfo(0, "lseg", EDBDbType.LSeg, DbType.Object, typeof (EDBLSeg),
                                            ExtendedBackendToNativeTypeConverter.ToLSeg);

            yield return
                new EDBBackendTypeInfo(0, "path", EDBDbType.Path, DbType.Object, typeof (EDBPath),
                                            ExtendedBackendToNativeTypeConverter.ToPath);

            yield return
                new EDBBackendTypeInfo(0, "box", EDBDbType.Box, DbType.Object, typeof (EDBBox),
                                            ExtendedBackendToNativeTypeConverter.ToBox);

            yield return
                new EDBBackendTypeInfo(0, "circle", EDBDbType.Circle, DbType.Object, typeof (EDBCircle),
                                            ExtendedBackendToNativeTypeConverter.ToCircle);

            yield return
                new EDBBackendTypeInfo(0, "polygon", EDBDbType.Polygon, DbType.Object, typeof (EDBPolygon),
                                            ExtendedBackendToNativeTypeConverter.ToPolygon);

            yield return new EDBBackendTypeInfo(0, "uuid", EDBDbType.Uuid, DbType.Guid, typeof (Guid),
                                            ExtendedBackendToNativeTypeConverter.ToGuid);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "xml", EDBDbType.Xml, DbType.Xml, typeof (String), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "json", EDBDbType.Json, DbType.Object, typeof(String),
                null,
                BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "jsonb", EDBDbType.Jsonb, DbType.Object, typeof(String),
                null,
                BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            yield return new EDBBackendTypeInfo(0, "hstore", EDBDbType.Hstore, DbType.Object, typeof(String),
                null,
                BasicBackendToNativeTypeConverter.TextBinaryToString);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            if (useExtendedTypes)
            {
                yield return
                    new EDBBackendTypeInfo(0, "interval", EDBDbType.Interval, DbType.Object, typeof(EDBInterval),
                                            ExtendedBackendToNativeTypeConverter.ToInterval);

                yield return
                    new EDBBackendTypeInfo(0, "date", EDBDbType.Date, DbType.Date, typeof(EDBDate),
                                            ExtendedBackendToNativeTypeConverter.ToDate);

                yield return
                    new EDBBackendTypeInfo(0, "time", EDBDbType.Time, DbType.Time, typeof(EDBTime),
                                            ExtendedBackendToNativeTypeConverter.ToTime);

                yield return
                    new EDBBackendTypeInfo(0, "timetz", EDBDbType.TimeTZ, DbType.Time, typeof(EDBTimeTZ),
                                            ExtendedBackendToNativeTypeConverter.ToTimeTZ);

                yield return
                    new EDBBackendTypeInfo(0, "timestamp", EDBDbType.Timestamp, DbType.DateTime, typeof(EDBTimeStamp),
                                            ExtendedBackendToNativeTypeConverter.ToTimeStamp);
                yield return
                    new EDBBackendTypeInfo(0, "abstime", EDBDbType.Abstime , DbType.DateTime, typeof(EDBTimeStampTZ),
                                            ExtendedBackendToNativeTypeConverter.ToTimeStampTZ);

                yield return
                    new EDBBackendTypeInfo(0, "timestamptz", EDBDbType.TimestampTZ, DbType.DateTime, typeof(EDBTimeStampTZ),
                                            ExtendedBackendToNativeTypeConverter.ToTimeStampTZ);
            }
            else
            {
                if (compat <= EDB207)
                {
                    // In 2.0.7 and earlier, intervals were returned as the native type.
                    // later versions return a CLR type and rely on provider specific api for EDBInterval
                    yield return
                        new EDBBackendTypeInfo(0, "interval", EDBDbType.Interval, DbType.Object, typeof(EDBInterval),
                                            ExtendedBackendToNativeTypeConverter.ToInterval);
                }
                else
                {
                    yield return
                        new EDBBackendTypeInfo(0, "interval", EDBDbType.Interval, DbType.Object, typeof(EDBInterval),
                                            ExtendedBackendToNativeTypeConverter.ToInterval,
                                            typeof(TimeSpan),
                                            interval => (TimeSpan)(EDBInterval)interval,
                                            intervalEDB => (intervalEDB is TimeSpan ? (EDBInterval)(TimeSpan) intervalEDB : intervalEDB));
                }

                yield return
                    new EDBBackendTypeInfo(0, "date", EDBDbType.Date, DbType.Date, typeof (EDBDate),
                                            ExtendedBackendToNativeTypeConverter.ToDate,
                                            typeof(DateTime),
                                            date => (DateTime)(EDBDate)date,
                                            npgsqlDate => (npgsqlDate is DateTime ? (EDBDate)(DateTime) npgsqlDate : npgsqlDate));

                yield return
                    new EDBBackendTypeInfo(0, "time", EDBDbType.Time, DbType.Time, typeof (EDBTime),
                                            ExtendedBackendToNativeTypeConverter.ToTime,
                                            typeof(DateTime),
                                            time => time is DateTime ? time : (DateTime)(EDBTime)time,
                                            npgsqlTime => (npgsqlTime is TimeSpan ? (EDBTime)(TimeSpan) npgsqlTime : npgsqlTime));

                yield return
                    new EDBBackendTypeInfo(0, "timetz", EDBDbType.TimeTZ, DbType.Time, typeof (EDBTimeTZ),
                                            ExtendedBackendToNativeTypeConverter.ToTimeTZ,
                                            typeof(DateTime),
                                            timetz => (DateTime)(EDBTimeTZ)timetz,
                                            npgsqlTimetz => (npgsqlTimetz is TimeSpan ? (EDBTimeTZ)(TimeSpan) npgsqlTimetz : npgsqlTimetz));

                yield return
                    new EDBBackendTypeInfo(0, "timestamp", EDBDbType.Timestamp, DbType.DateTime, typeof (EDBTimeStamp),
                                            ExtendedBackendToNativeTypeConverter.ToTimeStamp,
                                            typeof(DateTime),
                                            timestamp => (DateTime)(EDBTimeStamp)timestamp,
                                            npgsqlTimestamp => (npgsqlTimestamp is DateTime ? (EDBTimeStamp)(DateTime) npgsqlTimestamp : npgsqlTimestamp));

                yield return
                    new EDBBackendTypeInfo(0, "abstime", EDBDbType.Abstime, DbType.DateTime, typeof(EDBTimeStampTZ),
                                            ExtendedBackendToNativeTypeConverter.ToTimeStampTZ,
                                            typeof(DateTime),
                                            timestamp => (DateTime)(EDBTimeStampTZ)timestamp,
                                            npgsqlTimestampTZ => (npgsqlTimestampTZ is DateTime ? (EDBTimeStampTZ)(DateTime) npgsqlTimestampTZ : npgsqlTimestampTZ));

                yield return
                    new EDBBackendTypeInfo(0, "timestamptz", EDBDbType.TimestampTZ, DbType.DateTime, typeof (EDBTimeStampTZ),
                                            ExtendedBackendToNativeTypeConverter.ToTimeStampTZ,
                                            typeof(DateTime),
                                            timestamptz => ((DateTime)(EDBTimeStampTZ)timestamptz).ToLocalTime(),
                                            npgsqlTimestampTZ => (npgsqlTimestampTZ is DateTime ? (EDBTimeStampTZ)(DateTime)npgsqlTimestampTZ : npgsqlTimestampTZ is DateTimeOffset ? (EDBTimeStampTZ)(DateTimeOffset)npgsqlTimestampTZ : npgsqlTimestampTZ));
            }
        }

        ///<summary>
        /// This method creates (or retrieves from cache) a mapping between type and OID
        /// of all natively supported postgresql data types.
        /// This is needed as from one version to another, this mapping can be changed and
        /// so we avoid hardcoding them.
        /// </summary>
        /// <returns>EDBTypeMapping containing all known data types.  The mapping must be
        /// cloned before it is modified because it is cached; changes made by one connection may
        /// effect another connection.
        /// </returns>
        public static EDBBackendTypeMapping CreateAndLoadInitialTypesMapping(EDBConnector conn)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "LoadTypesMapping");

            MappingKey key = new MappingKey(conn);
            // Check the cache for an initial types map.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBBackendTypeMapping oidToNameMapping = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if(BackendTypeMappingCache.TryGetValue(key, out oidToNameMapping))
                return oidToNameMapping;

            // Not in cache, create a new one.
            oidToNameMapping = new EDBBackendTypeMapping();

            // Create a list of all natively supported postgresql data types.

            // Attempt to map each type info in the list to an OID on the backend and
            // add each mapped type to the new type mapping object.
            LoadTypesMappings(conn, oidToNameMapping, TypeInfoList(conn.UseExtendedTypes, conn.CompatVersion));

            //We hold the lock for the least time possible on the least scope possible.
            //We must lock on BackendTypeMappingCache because it will be updated by this operation,
            //and we must not just add to it, but also check that another thread hasn't updated it
            //in the meantime. Strictly just doing :
            //return BackendTypeMappingCache[key] = oidToNameMapping;
            //as the only call within the locked section should be safe and correct, but we'll assume
            //there's some subtle problem with temporarily having two copies of the same mapping and
            //ensure only one is called.
            //It is of course wasteful that multiple threads could be creating mappings when only one
            //will be used, but we aim for better overall concurrency at the risk of causing some
            //threads the extra work.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            EDBBackendTypeMapping mappingCheck = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            //First check without acquiring the lock; don't lock if we don't have to.
            if(BackendTypeMappingCache.TryGetValue(key, out mappingCheck))//Another thread built the mapping in the meantime.
                return mappingCheck;
            lock(BackendTypeMappingCache)
            {
                //Final check. We have the lock now so if this fails it'll continue to fail.
                if(BackendTypeMappingCache.TryGetValue(key, out mappingCheck))//Another thread built the mapping in the meantime.
                    return mappingCheck;
                // Add this mapping to the per-server-version cache so we don't have to
                // do these expensive queries on every connection startup.
                BackendTypeMappingCache.Add(key, oidToNameMapping);
            }
            return oidToNameMapping;
        }

        //Take a EDBBackendTypeInfo for a type and return the EDBBackendTypeInfo for
        //an array of that type.
        private static EDBBackendTypeInfo ArrayTypeInfo(EDBBackendTypeInfo elementInfo)
        {
            ArrayBackendToNativeTypeConverter converter = new ArrayBackendToNativeTypeConverter(elementInfo);

            if (elementInfo.SupportsBinaryBackendData)
            {
                return
                    new EDBBackendTypeInfo(0, "_" + elementInfo.Name, EDBDbType.Array | elementInfo.EDBDbType, DbType.Object,
                                              elementInfo.Type.MakeArrayType(),
                                              converter.ArrayTextToArray,
                                              converter.ArrayBinaryToArray);
            }
            else
            {
                return
                    new EDBBackendTypeInfo(0, "_" + elementInfo.Name, EDBDbType.Array | elementInfo.EDBDbType, DbType.Object,
                                              elementInfo.Type.MakeArrayType(),
                                              converter.ArrayTextToArray);
            }
        }

        /// <summary>
        /// Attempt to map types by issuing a query against pg_type.
        /// This function takes a list of EDBTypeInfo and attempts to resolve the OID field
        /// of each by querying pg_type.  If the mapping is found, the type info object is
        /// updated (OID) and added to the provided EDBTypeMapping object.
        /// </summary>
        /// <param name="conn">EDBConnector to send query through.</param>
        /// <param name="TypeMappings">Mapping object to add types too.</param>
        /// <param name="TypeInfoList">List of types that need to have OID's mapped.</param>
        public static void LoadTypesMappings(EDBConnector conn, EDBBackendTypeMapping TypeMappings,
                                             IEnumerable<EDBBackendTypeInfo> TypeInfoList)
        {
            StringBuilder InList = new StringBuilder();
            Dictionary<string, EDBBackendTypeInfo> NameIndex = new Dictionary<string, EDBBackendTypeInfo>();

            // Build a clause for the SELECT statement.
            // Build a name->typeinfo mapping so we can match the results of the query
            // with the list of type objects efficiently.
            foreach (EDBBackendTypeInfo TypeInfo in TypeInfoList)
            {
                NameIndex.Add(TypeInfo.Name, TypeInfo);
                InList.AppendFormat("{0}'{1}'", ((InList.Length > 0) ? ", " : ""), TypeInfo.Name);

                //do the same for the equivalent array type.

                NameIndex.Add("_" + TypeInfo.Name, ArrayTypeInfo(TypeInfo));

                InList.Append(", '_").Append(TypeInfo.Name).Append('\'');
            }

            if (InList.Length == 0)
            {
                return;
            }

            using (
                EDBCommand command =
                    new EDBCommand(string.Format("SELECT typname, oid FROM pg_type WHERE typname IN ({0})", InList), conn))
            {
                using (EDBDataReader dr = command.GetReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                {
                    while (dr.Read())
                    {
                        EDBBackendTypeInfo TypeInfo = NameIndex[dr[0].ToString()];

                        TypeInfo._OID = Convert.ToInt32(dr[1]);

                        TypeMappings.AddType(TypeInfo);
                    }
                }
            }
        }
    }
}
