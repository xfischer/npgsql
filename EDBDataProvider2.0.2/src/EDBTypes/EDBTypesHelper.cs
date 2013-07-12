// EDBTypes.EDBTypesHelper.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The EDB Development Team
//	EDB-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/EDB/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
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
using EnterpriseDB.EDBClient;

namespace EDBTypes
{
	/// <summary>
	///	This class contains helper methods for type conversion between
	/// the .Net type system and postgresql.
	/// </summary>
	internal static class EDBTypesHelper
	{
		// Logging related values
		private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;
		private static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);

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

            foreach (EDBBackendTypeInfo TypeInfo in TypeInfoList(false, new Version("10.0.0.0")))
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
			EDBNativeTypeInfo ret = null;
			return TryGetNativeTypeInfo(DbType, out ret) ? ret : null;
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
			typeOut = null;
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

			Type elementType = null;
			EDBNativeTypeInfo elementTypeInfo = null;
			if (TestTypedEnumerator(type, out elementType) && TryGetNativeTypeInfo(elementType, out elementTypeInfo))
			{
				typeInfo = EDBNativeTypeInfo.ArrayOf(elementTypeInfo);
				return true;
			}
			return false;
		}

		public static EDBNativeTypeInfo GetNativeTypeInfo(Type Type)
		{
			EDBNativeTypeInfo ret = null;
			return TryGetNativeTypeInfo(Type, out ret) ? ret : null;
		}


		public static bool DefinedType(Type type)

		{
			return NativeTypeMapping.ContainsType(type);
		}


		public static bool DefinedType(object item)

		{
			return DefinedType(item.GetType());
		}

		// CHECKME
		// Not sure what to do with this one.  I don't believe we ever ask for a binary
		// formatting, so this shouldn't even be used right now.
		// At some point this will need to be merged into the type converter system somehow?
		public static Object ConvertBackendBytesToSystemType(EDBBackendTypeInfo TypeInfo, Byte[] data, Int32 fieldValueSize,
		                                                     Int32 typeModifier)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");


			// We are never guaranteed to know about every possible data type the server can send us.
			// When we encounter an unknown type, we punt and return the data without modification.
			if (TypeInfo == null)
			{
				return data;
			}

			switch (TypeInfo.EDBDbType)
			{
				case EDBDbType.Bytea:
					return data;
					/*case EDBDbType.Boolean:
                return BitConverter.ToBoolean(data, 0);
            case EDBDbType.DateTime:
                return DateTime.MinValue.AddTicks(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0)));

            case EDBDbType.Int16:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
            case EDBDbType.Int32:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
            case EDBDbType.Int64:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0));
            case EDBDbType.String:
            case EDBDbType.AnsiString:
            case EDBDbType.StringFixedLength:
                return encoding.GetString(data, 0, fieldValueSize);*/
				default:
					throw new InvalidCastException("Type not supported in binary format");
			}
		}

		///<summary>
		/// This method is responsible to convert the string received from the backend
		/// to the corresponding EDBType.
		/// The given TypeInfo is called upon to do the conversion.
		/// If no TypeInfo object is provided, no conversion is performed.
		/// </summary>
		public static Object ConvertBackendStringToSystemType(EDBBackendTypeInfo TypeInfo, String data, Int16 typeSize,
		                                                      Int32 typeModifier)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

			if (TypeInfo != null)
			{
				return TypeInfo.ConvertToNative(data, typeSize, typeModifier);
			}
			else
			{
				return data;
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


            nativeTypeMapping.AddType("name", EDBDbType.Name, DbType.String, true, null);

			nativeTypeMapping.AddType("oidvector", EDBDbType.Oidvector, DbType.String, true, null);

			// Conflicting types should have mapped first the non default mappings.
			// For example, char, varchar and text map to DbType.String. As the most 
			// common is to use text with string, it has to be the last mapped, in order
			// to type mapping has the last entry, in this case, text, as the map value
			// for DbType.String.

			nativeTypeMapping.AddType("refcursor", EDBDbType.RefCursor, DbType.String, true, null);

			nativeTypeMapping.AddType("char", EDBDbType.Char, DbType.String, true, null);
			
			nativeTypeMapping.AddTypeAlias("char", typeof (Char));

			nativeTypeMapping.AddType("varchar", EDBDbType.Varchar, DbType.String, true, null);
            nativeTypeMapping.AddType("varchar2", EDBDbType.Varchar2, DbType.String, true, null);

			nativeTypeMapping.AddType("text", EDBDbType.Text, DbType.String, true, null);

			nativeTypeMapping.AddDbTypeAlias("text", DbType.StringFixedLength);
			nativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiString);
			nativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiStringFixedLength);
			
			nativeTypeMapping.AddTypeAlias("text", typeof (String));


			nativeTypeMapping.AddType("bytea", EDBDbType.Bytea, DbType.Binary, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBinary));

			nativeTypeMapping.AddTypeAlias("bytea", typeof (Byte[]));

			nativeTypeMapping.AddType("bit", EDBDbType.Bit, DbType.Object, false,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBit));
			
			nativeTypeMapping.AddTypeAlias("bit", typeof(BitString));

			nativeTypeMapping.AddType("bool", EDBDbType.Boolean, DbType.Boolean, false,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBoolean));

			nativeTypeMapping.AddTypeAlias("bool", typeof (Boolean));

			nativeTypeMapping.AddType("int2", EDBDbType.Smallint, DbType.Int16, false, BasicNativeToBackendTypeConverter.ToBasicType<short>);

            nativeTypeMapping.AddTypeAlias("int2", typeof (UInt16));
            
		    nativeTypeMapping.AddTypeAlias("int2", typeof (Int16));

			nativeTypeMapping.AddDbTypeAlias("int2", DbType.Byte);

			nativeTypeMapping.AddTypeAlias("int2", typeof (Byte));

			nativeTypeMapping.AddType("int4", EDBDbType.Integer, DbType.Int32, false, BasicNativeToBackendTypeConverter.ToBasicType<int>);

			nativeTypeMapping.AddTypeAlias("int4", typeof (Int32));

			nativeTypeMapping.AddType("int8", EDBDbType.Bigint, DbType.Int64, false, BasicNativeToBackendTypeConverter.ToBasicType<long>);

			nativeTypeMapping.AddTypeAlias("int8", typeof (Int64));

            nativeTypeMapping.AddType("float4", EDBDbType.Float, DbType.Single, true, BasicNativeToBackendTypeConverter.ToBasicType<float>);

			nativeTypeMapping.AddTypeAlias("float4", typeof (Single));

			//nativeTypeMapping.AddType("float8", EDBDbType.Double, DbType.Double, true, BasicNativeToBackendTypeConverter.ToBasicType<double>);
			nativeTypeMapping.AddType("float8", EDBDbType.Double, DbType.Double, true, new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToSingleDouble));
			
			nativeTypeMapping.AddTypeAlias("float8", typeof (Double));

			nativeTypeMapping.AddType("numeric", EDBDbType.Numeric, DbType.Decimal, true, BasicNativeToBackendTypeConverter.ToBasicType<decimal>);

			nativeTypeMapping.AddTypeAlias("numeric", typeof (Decimal));

			nativeTypeMapping.AddType("money", EDBDbType.Money, DbType.Currency, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToMoney));

			nativeTypeMapping.AddType("date", EDBDbType.Date, DbType.Date, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDate));

			nativeTypeMapping.AddTypeAlias("date", typeof (EDBDate));

			nativeTypeMapping.AddType("timetz", EDBDbType.TimeTZ, DbType.Time, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeTZ));

			nativeTypeMapping.AddTypeAlias("timetz", typeof (EDBTimeTZ));

			nativeTypeMapping.AddType("time", EDBDbType.Time, DbType.Time, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToTime));

			nativeTypeMapping.AddTypeAlias("time", typeof (EDBTime));

            nativeTypeMapping.AddType("timestamptz", EDBDbType.TimestampTZ, DbType.DateTime, true,
                                      new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeStamp));

            nativeTypeMapping.AddTypeAlias("timestamptz", typeof(EDBTimeStampTZ));



            nativeTypeMapping.AddDbTypeAlias("timestamptz", DbType.DateTimeOffset);



            nativeTypeMapping.AddTypeAlias("timestamptz", typeof(DateTimeOffset));


            nativeTypeMapping.AddType("abstime", EDBDbType.Abstime, DbType.DateTime, true,
                                      new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeStamp));

			nativeTypeMapping.AddType("timestamp", EDBDbType.Timestamp, DbType.DateTime, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeStamp));
            
			nativeTypeMapping.AddTypeAlias("timestamp", typeof (DateTime));
			nativeTypeMapping.AddTypeAlias("timestamp", typeof (EDBTimeStamp));

			

			nativeTypeMapping.AddType("point", EDBDbType.Point, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPoint));

			nativeTypeMapping.AddTypeAlias("point", typeof (EDBPoint));

			nativeTypeMapping.AddType("box", EDBDbType.Box, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToBox));

			nativeTypeMapping.AddTypeAlias("box", typeof (EDBBox));

			nativeTypeMapping.AddType("lseg", EDBDbType.LSeg, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToLSeg));

			nativeTypeMapping.AddTypeAlias("lseg", typeof (EDBLSeg));

			nativeTypeMapping.AddType("path", EDBDbType.Path, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPath));

			nativeTypeMapping.AddTypeAlias("path", typeof (EDBPath));

			nativeTypeMapping.AddType("polygon", EDBDbType.Polygon, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPolygon));

			nativeTypeMapping.AddTypeAlias("polygon", typeof (EDBPolygon));

			nativeTypeMapping.AddType("circle", EDBDbType.Circle, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToCircle));

			nativeTypeMapping.AddTypeAlias("circle", typeof (EDBCircle));

			nativeTypeMapping.AddType("inet", EDBDbType.Inet, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToIPAddress));

			nativeTypeMapping.AddTypeAlias("inet", typeof (IPAddress));
			nativeTypeMapping.AddTypeAlias("inet", typeof (EDBInet));

            nativeTypeMapping.AddType("macaddr", EDBDbType.MacAddr, DbType.Object, true,
                                      new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToMacAddress));

            nativeTypeMapping.AddTypeAlias("macaddr", typeof(PhysicalAddress));
            nativeTypeMapping.AddTypeAlias("macaddr", typeof(EDBMacAddress));

			nativeTypeMapping.AddType("uuid", EDBDbType.Uuid, DbType.Guid, true, null);
			nativeTypeMapping.AddTypeAlias("uuid", typeof (Guid));

			nativeTypeMapping.AddType("xml", EDBDbType.Xml, DbType.Xml, true, null);

			nativeTypeMapping.AddType("interval", EDBDbType.Interval, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToInterval));

			nativeTypeMapping.AddTypeAlias("interval", typeof (EDBInterval));
			nativeTypeMapping.AddTypeAlias("interval", typeof (TimeSpan));
			
			nativeTypeMapping.AddDbTypeAlias("text", DbType.Object);
			
			
			return nativeTypeMapping;
		}

		private static IEnumerable<EDBBackendTypeInfo> TypeInfoList(bool useExtendedTypes, Version compat)
		{
            yield return new EDBBackendTypeInfo(0, "oidvector", EDBDbType.Text, DbType.String, typeof (String), null);

			yield return new EDBBackendTypeInfo(0, "unknown", EDBDbType.Text, DbType.String, typeof (String), null);

            yield return new EDBBackendTypeInfo(0, "refcursor", EDBDbType.RefCursor, DbType.String, typeof (String), null);

			yield return new EDBBackendTypeInfo(0, "char", EDBDbType.Char, DbType.String, typeof (String), null);

			yield return new EDBBackendTypeInfo(0, "bpchar", EDBDbType.Text, DbType.String, typeof (String), null);

			yield return new EDBBackendTypeInfo(0, "varchar", EDBDbType.Varchar, DbType.String, typeof (String), null);
            yield return new EDBBackendTypeInfo(0, "varchar2", EDBDbType.Varchar2, DbType.String, typeof(String), null);

			yield return new EDBBackendTypeInfo(0, "text", EDBDbType.Text, DbType.String, typeof (String), null);

			yield return new EDBBackendTypeInfo(0, "name", EDBDbType.Name, DbType.String, typeof (String), null);

			yield return
				new EDBBackendTypeInfo(0, "bytea", EDBDbType.Bytea, DbType.Binary, typeof (Byte[]),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBinary));

			yield return
				new EDBBackendTypeInfo(0, "bit", EDBDbType.Bit, DbType.Object, typeof (BitString),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBit));

			yield return
				new EDBBackendTypeInfo(0, "bool", EDBDbType.Boolean, DbType.Boolean, typeof (Boolean),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBoolean));

			yield return new EDBBackendTypeInfo(0, "int2", EDBDbType.Smallint, DbType.Int16, typeof (Int16), null);

			yield return new EDBBackendTypeInfo(0, "int4", EDBDbType.Integer, DbType.Int32, typeof (Int32), null);

			yield return new EDBBackendTypeInfo(0, "int8", EDBDbType.Bigint, DbType.Int64, typeof (Int64), null);

			yield return new EDBBackendTypeInfo(0, "oid", EDBDbType.Bigint, DbType.Int64, typeof (Int64), null);

			yield return new EDBBackendTypeInfo(0, "float4", EDBDbType.Float, DbType.Single, typeof (Single), null);

			yield return new EDBBackendTypeInfo(0, "float8", EDBDbType.Double, DbType.Double, typeof (Double), null);

			yield return new EDBBackendTypeInfo(0, "numeric", EDBDbType.Numeric, DbType.Decimal, typeof (Decimal), null);

			yield return
				new EDBBackendTypeInfo(0, "inet", EDBDbType.Inet, DbType.Object, typeof (EDBInet),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToInet),
                                          typeof(IPAddress),
                                          ipaddress => (IPAddress)(EDBInet)ipaddress, 
                                          EDBinet => (EDBinet is IPAddress ? (EDBInet)(IPAddress) EDBinet : EDBinet));
            yield return
                new EDBBackendTypeInfo(0, "macaddr", EDBDbType.MacAddr, DbType.Object, typeof(EDBMacAddress),
                                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToMacAddress),
                                          typeof(PhysicalAddress),
                                          macAddress => (PhysicalAddress)(EDBMacAddress)macAddress,
                                          EDBmacaddr => (EDBmacaddr is PhysicalAddress ? (EDBMacAddress)(PhysicalAddress)EDBmacaddr : EDBmacaddr));

			yield return
				new EDBBackendTypeInfo(0, "money", EDBDbType.Money, DbType.Currency, typeof (Decimal),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToMoney));

			yield return
				new EDBBackendTypeInfo(0, "point", EDBDbType.Point, DbType.Object, typeof (EDBPoint),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPoint));

			yield return
				new EDBBackendTypeInfo(0, "lseg", EDBDbType.LSeg, DbType.Object, typeof (EDBLSeg),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToLSeg));

			yield return
				new EDBBackendTypeInfo(0, "path", EDBDbType.Path, DbType.Object, typeof (EDBPath),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPath));

			yield return
				new EDBBackendTypeInfo(0, "box", EDBDbType.Box, DbType.Object, typeof (EDBBox),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToBox));

			yield return
				new EDBBackendTypeInfo(0, "circle", EDBDbType.Circle, DbType.Object, typeof (EDBCircle),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToCircle));

			yield return
				new EDBBackendTypeInfo(0, "polygon", EDBDbType.Polygon, DbType.Object, typeof (EDBPolygon),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPolygon));

			yield return new EDBBackendTypeInfo(0, "uuid", EDBDbType.Uuid, DbType.Guid, typeof (Guid), new
ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToGuid));

			yield return new EDBBackendTypeInfo(0, "xml", EDBDbType.Xml, DbType.Xml, typeof (String), null);

            if (useExtendedTypes)
            {
                yield return
                    new EDBBackendTypeInfo(0, "interval", EDBDbType.Interval, DbType.Object, typeof(EDBInterval),
                                  new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToInterval));
                yield return
                    new EDBBackendTypeInfo(0, "date", EDBDbType.Date, DbType.Date, typeof(EDBDate),
                                  new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToDate));
                yield return
                    new EDBBackendTypeInfo(0, "time", EDBDbType.Time, DbType.Time, typeof(EDBTime),
                                              new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTime));
                yield return
                    new EDBBackendTypeInfo(0, "timetz", EDBDbType.TimeTZ, DbType.Time, typeof(EDBTimeTZ),
                                              new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeTZ));
                yield return
                    new EDBBackendTypeInfo(0, "timestamp", EDBDbType.Timestamp, DbType.DateTime, typeof(EDBTimeStamp),
                                              new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeStamp));
                yield return
                    new EDBBackendTypeInfo(0, "abstime", EDBDbType.Abstime , DbType.DateTime, typeof(EDBTimeStampTZ),
                                              new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeStampTZ));
                yield return
                    new EDBBackendTypeInfo(0, "timestamptz", EDBDbType.TimestampTZ, DbType.DateTime, typeof(EDBTimeStampTZ),
                                              new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeStampTZ));
            }
			else
            {
                if (compat <= EDB207)
                {
                    // In 2.0.7 and earlier, intervals were returned as the native type.
                    // later versions return a CLR type and rely on provider specific api for EDBInterval
                    yield return
                        new EDBBackendTypeInfo(0, "interval", EDBDbType.Interval, DbType.Object, typeof(EDBInterval),
                                                  new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToInterval));
                }
                else
                {
                    yield return
                        new EDBBackendTypeInfo(0, "interval", EDBDbType.Interval, DbType.Object, typeof(EDBInterval),
                                                  new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToInterval),
                                                  typeof(TimeSpan), interval => (TimeSpan)(EDBInterval)interval, intervalEDB => (intervalEDB is TimeSpan ? (EDBInterval)(TimeSpan) intervalEDB : intervalEDB));
                }

				yield return
					new EDBBackendTypeInfo(0, "date", EDBDbType.Date, DbType.Date, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDate));

				yield return
					new EDBBackendTypeInfo(0, "time", EDBDbType.Time, DbType.Time, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime));

				yield return
					new EDBBackendTypeInfo(0, "timetz", EDBDbType.TimeTZ, DbType.Time, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime));

				yield return
					new EDBBackendTypeInfo(0, "timestamp", EDBDbType.Timestamp, DbType.DateTime, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime));

				yield return
					new EDBBackendTypeInfo(0, "timestamptz", EDBDbType.TimestampTZ, DbType.DateTime, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime));
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
		/// effect another connection.</returns>
		public static EDBBackendTypeMapping CreateAndLoadInitialTypesMapping(EDBConnector conn)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "LoadTypesMapping");

		    MappingKey key = new MappingKey(conn);
			// Check the cache for an initial types map.
			EDBBackendTypeMapping oidToNameMapping = null;

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
		    EDBBackendTypeMapping mappingCheck = null;
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
			return
				new EDBBackendTypeInfo(0, "_" + elementInfo.Name, EDBDbType.Array | elementInfo.EDBDbType, DbType.Object,
				                          elementInfo.Type.MakeArrayType(),
				                          new ConvertBackendToNativeHandler(
				                          	new ArrayBackendToNativeTypeConverter(elementInfo).ToArray));
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

	/// <summary>
	/// Delegate called to convert the given backend data to its native representation.
	/// </summary>
	internal delegate Object ConvertBackendToNativeHandler(
		EDBBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier);

	/// <summary>
	/// Delegate called to convert the given native data to its backand representation.
	/// </summary>
	internal delegate String ConvertNativeToBackendHandler(EDBNativeTypeInfo TypeInfo, Object NativeData, Boolean ForExtendedQuery);

    internal delegate object ConvertProviderTypeToFrameworkTypeHander(object value);

    internal delegate object ConvertFrameworkTypeToProviderTypeHander(object value);

	/// <summary>
	/// Represents a backend data type.
	/// This class can be called upon to convert a backend field representation to a native object.
	/// </summary>
	internal class EDBBackendTypeInfo
	{
		private readonly ConvertBackendToNativeHandler _ConvertBackendToNative;
        private readonly ConvertProviderTypeToFrameworkTypeHander _convertProviderToFramework;
        private readonly ConvertFrameworkTypeToProviderTypeHander _convertFrameworkToProvider;

		internal Int32 _OID;
		private readonly String _Name;
		private readonly EDBDbType _EDBDbType;
		private readonly DbType _DbType;
		private readonly Type _Type;
        private readonly Type _frameworkType;


		/// <summary>
		/// Construct a new EDBTypeInfo with the given attributes and conversion handlers.
		/// </summary>
		/// <param name="OID">Type OID provided by the backend server.</param>
		/// <param name="Name">Type name provided by the backend server.</param>
		/// <param name="EDBDbType">EDBDbType</param>
		/// <param name="Type">System type to convert fields of this type to.</param>
		/// <param name="ConvertBackendToNative">Data conversion handler.</param>
		public EDBBackendTypeInfo(Int32 OID, String Name, EDBDbType EDBDbType, DbType DbType, Type Type,
		                             ConvertBackendToNativeHandler ConvertBackendToNative)
		{
            if (Type == null)
                throw new ArgumentNullException("Type");
			_OID = OID;
			_Name = Name;
			_EDBDbType = EDBDbType;
			_DbType = DbType;
			_Type = Type;
            _frameworkType = Type;
			_ConvertBackendToNative = ConvertBackendToNative;
		}

        public EDBBackendTypeInfo(Int32 OID, String Name, EDBDbType EDBDbType, DbType DbType, Type Type,
                                     ConvertBackendToNativeHandler ConvertBackendToNative, Type frameworkType,
                                     ConvertProviderTypeToFrameworkTypeHander convertProviderToFramework,
                                     ConvertFrameworkTypeToProviderTypeHander convertFrameworkToProvider)
            : this(OID, Name, EDBDbType, DbType, Type, ConvertBackendToNative)
        {
            _frameworkType = frameworkType;
            _convertProviderToFramework = convertProviderToFramework;
            _convertFrameworkToProvider = convertFrameworkToProvider;
        }

		/// <summary>
		/// Type OID provided by the backend server.
		/// </summary>
		public Int32 OID
		{
			get { return _OID; }
		}

		/// <summary>
		/// Type name provided by the backend server.
		/// </summary>
		public String Name
		{
			get { return _Name; }
		}

		/// <summary>
		/// EDBDbType.
		/// </summary>
		public EDBDbType EDBDbType
		{
			get { return _EDBDbType; }
		}

		/// <summary>
		/// EDBDbType.
		/// </summary>
		public DbType DbType
		{
			get { return _DbType; }
		}

		/// <summary>
		/// Provider type to convert fields of this type to.
		/// </summary>
		public Type Type
		{
			get { return _Type; }
		}

        /// <summary>
        /// System type to convert fields of this type to.
        /// </summary>
        public Type FrameworkType
        {
            get { return _frameworkType; }
        }

		/// <summary>
		/// Perform a data conversion from a backend representation to 
		/// a native object.
		/// </summary>
		/// <param name="BackendData">Data sent from the backend.</param>
		/// <param name="TypeModifier">Type modifier field sent from the backend.</param>
		public Object ConvertToNative(String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			if (_ConvertBackendToNative != null)
			{
				return _ConvertBackendToNative(this, BackendData, TypeSize, TypeModifier);
			}
			else
			{
				try
				{
					return Convert.ChangeType(BackendData, Type, CultureInfo.InvariantCulture);
				}
				catch
				{
					return BackendData;
				}
			}
		}

        internal object ConvertToFrameworkType(object providerValue)
        {
            if (providerValue == DBNull.Value)
            {
                return providerValue;
            }
            else if (_convertProviderToFramework != null)
            {
                return _convertProviderToFramework(providerValue);
            }
            else if (Type != FrameworkType)
            {
                try
                {
                    return Convert.ChangeType(providerValue, FrameworkType, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return providerValue;
                }
            }
            return providerValue;
        }

        internal object ConvertToProviderType(object frameworkValue)
        {
            if (frameworkValue == DBNull.Value)
            {
                return frameworkValue;
            }
            else if (_convertFrameworkToProvider!= null)
            {
                return _convertFrameworkToProvider(frameworkValue);
            }
            
            return frameworkValue;
        }
    
    }

    

	/// <summary>
	/// Represents a backend data type.
	/// This class can be called upon to convert a native object to its backend field representation,
	/// </summary>
	internal class EDBNativeTypeInfo
	{
		private static readonly NumberFormatInfo ni;

		private readonly ConvertNativeToBackendHandler _ConvertNativeToBackend;

		private readonly String _Name;
		private readonly string _CastName;
		private readonly EDBDbType _EDBDbType;
		private readonly DbType _DbType;
		private readonly Boolean _Quote;
		private readonly Boolean _UseSize;
		private Boolean _IsArray = false;

		/// <summary>
		/// Returns an EDBNativeTypeInfo for an array where the elements are of the type
		/// described by the EDBNativeTypeInfo supplied.
		/// </summary>
		public static EDBNativeTypeInfo ArrayOf(EDBNativeTypeInfo elementType)

		{
			if (elementType._IsArray)
				//we've an array of arrays. It's the inner most elements whose type we care about, so the type we have is fine.
			{
				return elementType;
			}

			EDBNativeTypeInfo copy =
				new EDBNativeTypeInfo("_" + elementType.Name, EDBDbType.Array | elementType.EDBDbType, elementType.DbType,
				                         false,
				                         new ConvertNativeToBackendHandler(
				                         	new ArrayNativeToBackendTypeConverter(elementType).FromArray));

			copy._IsArray = true;

			return copy;
		}


		static EDBNativeTypeInfo()
		{
			ni = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
			ni.NumberDecimalDigits = 15;
		}

        internal static NumberFormatInfo NumberFormat
        {
            get { return ni; }
        }

		/// <summary>
		/// Construct a new EDBTypeInfo with the given attributes and conversion handlers.
		/// </summary>
		/// <param name="Name">Type name provided by the backend server.</param>
		/// <param name="EDBDbType">EDBDbType</param>
		/// <param name="ConvertNativeToBackend">Data conversion handler.</param>
		public EDBNativeTypeInfo(String Name, EDBDbType EDBDbType, DbType DbType, Boolean Quote,
		                            ConvertNativeToBackendHandler ConvertNativeToBackend)
		{
			_Name = Name;
			_CastName = Name.StartsWith("_") ? Name.Substring(1) + "[]" : Name;
			_EDBDbType = EDBDbType;
			_DbType = DbType;
			_Quote = Quote;
			_ConvertNativeToBackend = ConvertNativeToBackend;


			// The only parameters types which use length currently supported are char and varchar. Check for them.

			if ((EDBDbType == EDBDbType.Char) || (EDBDbType == EDBDbType.Varchar))
			{
				_UseSize = true;
			}
			else
			{
				_UseSize = false;
			}
		}

		/// <summary>
		/// Type name provided by the backend server.
		/// </summary>
		public String Name
		{
			get { return _Name; }
		}

		public string CastName

		{
			get { return _CastName; }
		}

		public bool IsArray

		{
			get { return _IsArray; }
		}

		/// <summary>
		/// EDBDbType.
		/// </summary>
		public EDBDbType EDBDbType
		{
			get { return _EDBDbType; }
		}

		/// <summary>
		/// DbType.
		/// </summary>
		public DbType DbType
		{
			get { return _DbType; }
		}


		/// <summary>
		/// Apply quoting.
		/// </summary>
		public Boolean Quote
		{
			get { return _Quote; }
		}

		/// <summary>
		/// Use parameter size information.
		/// </summary>
		public Boolean UseSize
		{
			get { return _UseSize; }
		}


		/// <summary>
		/// Perform a data conversion from a native object to
		/// a backend representation.
		/// DBNull and null values are handled differently depending if a plain query is used
		/// When 
		/// </summary>
		/// <param name="NativeData">Native .NET object to be converted.</param>
		/// <param name="ForExtendedQuery">Flag indicating if the conversion has to be done for 
		/// plain queries or extended queries</param>
		public String ConvertToBackend(Object NativeData, Boolean ForExtendedQuery)
		{
			if (ForExtendedQuery)
			{
				return ConvertToBackendExtendedQuery(NativeData);
			}
			else
			{
				return ConvertToBackendPlainQuery(NativeData);
			}
		}

		private String ConvertToBackendPlainQuery(Object NativeData)
		{
			if ((NativeData == DBNull.Value) || (NativeData == null))
			{
				return "NULL"; // Plain queries exptects null values as string NULL. 
			}

			if (_ConvertNativeToBackend != null)
			{
				return
					(this.Quote ? QuoteString(_ConvertNativeToBackend(this, NativeData, false)) : _ConvertNativeToBackend(this, NativeData, false));
			}
			else
			{
				if (NativeData is Enum)
				{
					// Do a special handling of Enum values.
					// Translate enum value to its underlying type. 
					return
						QuoteString(
							(String)
							Convert.ChangeType(Enum.Format(NativeData.GetType(), NativeData, "d"), typeof (String),
							                   CultureInfo.InvariantCulture));
				}
				else if (NativeData is IFormattable)
				{
					return
						(this.Quote
						 	? QuoteString(((IFormattable) NativeData).ToString(null, ni).Replace("'", "''").Replace("\\", "\\\\"))
						 	: ((IFormattable) NativeData).ToString(null, ni).Replace("'", "''").Replace("\\", "\\\\"));
				}

				// Do special handling of strings when in simple query. Escape quotes and backslashes.
				return
					(this.Quote
					 	? QuoteString(NativeData.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace("\0", "\\0"))
					 	: NativeData.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace("\0", "\\0"));
			}
		}

		private String ConvertToBackendExtendedQuery(Object NativeData)
		{
			if ((NativeData == DBNull.Value) || (NativeData == null))
			{
				return null; // Extended query expects null values be represented as null.
			}

			if (_ConvertNativeToBackend != null)
			{
				return _ConvertNativeToBackend(this, NativeData, true);
			}
			else
			{
				if (NativeData is Enum)
				{
					// Do a special handling of Enum values.
					// Translate enum value to its underlying type. 
					return
						(String)
						Convert.ChangeType(Enum.Format(NativeData.GetType(), NativeData, "d"), typeof (String),
						                   CultureInfo.InvariantCulture);
				}
				else if (NativeData is IFormattable)
				{
					return ((IFormattable) NativeData).ToString(null, ni);
				}

				return NativeData.ToString();
			}
		}

		internal static String QuoteString(String S)
		{
			return String.Format("'{0}'", S);
		}
	}

	/// <summary>
	/// Provide mapping between type OID, type name, and a EDBBackendTypeInfo object that represents it.
	/// </summary>
	internal class EDBBackendTypeMapping
	{
		private readonly Dictionary<int, EDBBackendTypeInfo> OIDIndex;
		private readonly Dictionary<string, EDBBackendTypeInfo> NameIndex;

		/// <summary>
		/// Construct an empty mapping.
		/// </summary>
		public EDBBackendTypeMapping()
		{
			OIDIndex = new Dictionary<int, EDBBackendTypeInfo>();
			NameIndex = new Dictionary<string, EDBBackendTypeInfo>();
		}

		/// <summary>
		/// Copy constuctor.
		/// </summary>
		private EDBBackendTypeMapping(EDBBackendTypeMapping Other)
		{
			OIDIndex = new Dictionary<int, EDBBackendTypeInfo>(Other.OIDIndex);
			NameIndex = new Dictionary<string, EDBBackendTypeInfo>(Other.NameIndex);
		}

		/// <summary>
		/// Add the given EDBBackendTypeInfo to this mapping.
		/// </summary>
		public void AddType(EDBBackendTypeInfo T)
		{
			if (OIDIndex.ContainsKey(T.OID))
			{
				throw new Exception("Type already mapped");
			}

			OIDIndex[T.OID] = T;
			NameIndex[T.Name] = T;
		}

		/// <summary>
		/// Add a new EDBBackendTypeInfo with the given attributes and conversion handlers to this mapping.
		/// </summary>
		/// <param name="OID">Type OID provided by the backend server.</param>
		/// <param name="Name">Type name provided by the backend server.</param>
		/// <param name="EDBDbType">EDBDbType</param>
		/// <param name="Type">System type to convert fields of this type to.</param>
		/// <param name="BackendConvert">Data conversion handler.</param>
		public void AddType(Int32 OID, String Name, EDBDbType EDBDbType, DbType DbType, Type Type,
		                    ConvertBackendToNativeHandler BackendConvert)
		{
			AddType(new EDBBackendTypeInfo(OID, Name, EDBDbType, DbType, Type, BackendConvert));
		}

		/// <summary>
		/// Get the number of type infos held.
		/// </summary>
		public Int32 Count
		{
			get { return NameIndex.Count; }
		}

		public bool TryGetValue(int oid, out EDBBackendTypeInfo value)
		{
			return OIDIndex.TryGetValue(oid, out value);
		}

		/// <summary>
		/// Retrieve the EDBBackendTypeInfo with the given backend type OID, or null if none found.
		/// </summary>
		public EDBBackendTypeInfo this[Int32 OID]
		{
			get
			{
				EDBBackendTypeInfo ret = null;
				return TryGetValue(OID, out ret) ? ret : null;
			}
		}

		/// <summary>
		/// Retrieve the EDBBackendTypeInfo with the given backend type name, or null if none found.
		/// </summary>
		public EDBBackendTypeInfo this[String Name]
		{
			get
			{
				EDBBackendTypeInfo ret = null;
				return NameIndex.TryGetValue(Name, out ret) ? ret : null;
			}
		}

		/// <summary>
		/// Make a shallow copy of this type mapping.
		/// </summary>
		public EDBBackendTypeMapping Clone()
		{
			return new EDBBackendTypeMapping(this);
		}

		/// <summary>
		/// Determine if a EDBBackendTypeInfo with the given backend type OID exists in this mapping.
		/// </summary>
		public Boolean ContainsOID(Int32 OID)
		{
			return OIDIndex.ContainsKey(OID);
		}

		/// <summary>
		/// Determine if a EDBBackendTypeInfo with the given backend type name exists in this mapping.
		/// </summary>
		public Boolean ContainsName(String Name)
		{
			return NameIndex.ContainsKey(Name);
		}
	}


	/// <summary>
	/// Provide mapping between type Type, EDBDbType and a EDBNativeTypeInfo object that represents it.
	/// </summary>
	internal class EDBNativeTypeMapping
	{
		private readonly Dictionary<string, EDBNativeTypeInfo> NameIndex = new Dictionary<string, EDBNativeTypeInfo>();

		private readonly Dictionary<EDBDbType, EDBNativeTypeInfo> EDBDbTypeIndex =
			new Dictionary<EDBDbType, EDBNativeTypeInfo>();

		private readonly Dictionary<DbType, EDBNativeTypeInfo> DbTypeIndex = new Dictionary<DbType, EDBNativeTypeInfo>();
		private readonly Dictionary<Type, EDBNativeTypeInfo> TypeIndex = new Dictionary<Type, EDBNativeTypeInfo>();

		/// <summary>
		/// Add the given EDBNativeTypeInfo to this mapping.
		/// </summary>
		public void AddType(EDBNativeTypeInfo T)
		{
			if (NameIndex.ContainsKey(T.Name))
			{
				throw new Exception("Type already mapped");
			}

			NameIndex[T.Name] = T;
			EDBDbTypeIndex[T.EDBDbType] = T;
			DbTypeIndex[T.DbType] = T;
			if (!T.IsArray)

			{
				EDBNativeTypeInfo arrayType = EDBNativeTypeInfo.ArrayOf(T);
				NameIndex[arrayType.Name] = arrayType;

				NameIndex[arrayType.CastName] = arrayType;
				EDBDbTypeIndex[arrayType.EDBDbType] = arrayType;
			}
		}

		/// <summary>
		/// Add a new EDBNativeTypeInfo with the given attributes and conversion handlers to this mapping.
		/// </summary>
		/// <param name="Name">Type name provided by the backend server.</param>
		/// <param name="EDBDbType">EDBDbType</param>
		/// <param name="NativeConvert">Data conversion handler.</param>
		public void AddType(String Name, EDBDbType EDBDbType, DbType DbType, Boolean Quote,
		                    ConvertNativeToBackendHandler NativeConvert)
		{
			AddType(new EDBNativeTypeInfo(Name, EDBDbType, DbType, Quote, NativeConvert));
		}

		public void AddEDBDbTypeAlias(String Name, EDBDbType EDBDbType)
		{
			if (EDBDbTypeIndex.ContainsKey(EDBDbType))
			{
				throw new Exception("EDBDbType already aliased");
			}

			EDBDbTypeIndex[EDBDbType] = NameIndex[Name];
		}

		public void AddDbTypeAlias(String Name, DbType DbType)
		{
			/*if (DbTypeIndex.ContainsKey(DbType))
			{
				throw new Exception("DbType already aliased");
			}*/

			DbTypeIndex[DbType] = NameIndex[Name];
		}

		public void AddTypeAlias(String Name, Type Type)
		{
			if (TypeIndex.ContainsKey(Type))
			{
				throw new Exception("Type already aliased");
			}

			TypeIndex[Type] = NameIndex[Name];
		}

		/// <summary>
		/// Get the number of type infos held.
		/// </summary>
		public Int32 Count
		{
			get { return NameIndex.Count; }
		}

		public bool TryGetValue(string name, out EDBNativeTypeInfo typeInfo)
		{
			return NameIndex.TryGetValue(name, out typeInfo);
		}

		/// <summary>
		/// Retrieve the EDBNativeTypeInfo with the given EDBDbType.
		/// </summary>
		public bool TryGetValue(EDBDbType dbType, out EDBNativeTypeInfo typeInfo)
		{
			return EDBDbTypeIndex.TryGetValue(dbType, out typeInfo);
		}

		/// <summary>
		/// Retrieve the EDBNativeTypeInfo with the given DbType.
		/// </summary>
		public bool TryGetValue(DbType dbType, out EDBNativeTypeInfo typeInfo)
		{
			return DbTypeIndex.TryGetValue(dbType, out typeInfo);
		}


		/// <summary>
		/// Retrieve the EDBNativeTypeInfo with the given Type.
		/// </summary>
		public bool TryGetValue(Type type, out EDBNativeTypeInfo typeInfo)
		{
			return TypeIndex.TryGetValue(type, out typeInfo);
		}

		/// <summary>
		/// Determine if a EDBNativeTypeInfo with the given backend type name exists in this mapping.
		/// </summary>
		public Boolean ContainsName(String Name)
		{
			return NameIndex.ContainsKey(Name);
		}

		/// <summary>
		/// Determine if a EDBNativeTypeInfo with the given EDBDbType exists in this mapping.
		/// </summary>
		public Boolean ContainsEDBDbType(EDBDbType EDBDbType)
		{
			return EDBDbTypeIndex.ContainsKey(EDBDbType);
		}

		/// <summary>
		/// Determine if a EDBNativeTypeInfo with the given Type name exists in this mapping.
		/// </summary>
		public Boolean ContainsType(Type Type)
		{
			return TypeIndex.ContainsKey(Type);
		}
	}
    
    internal static class ExpectedTypeConverter
    {
        internal static object ChangeType(object value, Type expectedType)
        {
            if (value == null)
                return null;
            Type currentType = value.GetType();
            if (value is DBNull || currentType == expectedType)
                return value;
#if NET35
            if (expectedType == typeof(DateTimeOffset))
            {
                if (currentType == typeof(EDBDate))
                {
                    return new DateTimeOffset((DateTime)(EDBDate)value);
                }
                else if (currentType == typeof(EDBTime))
                {
                    return new DateTimeOffset((DateTime)(EDBTime)value);
                }
                else if (currentType == typeof(EDBTimeTZ))
                {
                    EDBTimeTZ timetz = (EDBTimeTZ)value;
                    return new DateTimeOffset(timetz.Ticks, new TimeSpan(timetz.TimeZone.Hours, timetz.TimeZone.Minutes, timetz.TimeZone.Seconds));
                }
                else if (currentType == typeof(EDBTimeStamp))
                {
                    return new DateTimeOffset((DateTime)(EDBTimeStamp)value);
                }
                else if (currentType == typeof(EDBTimeStampTZ))
                {
                    EDBTimeStampTZ timestamptz = (EDBTimeStampTZ)value;
                    return new DateTimeOffset(timestamptz.Ticks, new TimeSpan(timestamptz.TimeZone.Hours, timestamptz.TimeZone.Minutes, timestamptz.TimeZone.Seconds));
                }
                else if (currentType == typeof(EDBInterval))
                {
                    return new DateTimeOffset(((TimeSpan)(EDBInterval)value).Ticks, TimeSpan.FromSeconds(0));
                }
                else if (currentType == typeof(DateTime))
                {
                    return new DateTimeOffset((DateTime)value);
                }
                else if (currentType == typeof(TimeSpan))
                {
                    return new DateTimeOffset(((TimeSpan)value).Ticks, TimeSpan.FromSeconds(0));
                }
                else
                {
                    return DateTimeOffset.Parse(value.ToString(), CultureInfo.InvariantCulture);
                }
            }
            else
#endif
            if (expectedType == typeof(TimeSpan))
            {
                if (currentType == typeof(EDBDate))
                {
                    return new TimeSpan(((DateTime)(EDBDate)value).Ticks);
                }
                else if (currentType == typeof(EDBTime))
                {
                    return new TimeSpan(((EDBTime)value).Ticks);
                }
                else if (currentType == typeof(EDBTimeTZ))
                {
                    return new TimeSpan(((EDBTimeTZ)value).UTCTime.Ticks);
                }
                else if (currentType == typeof(EDBTimeStamp))
                {
                    return new TimeSpan(((EDBTimeStamp)value).Ticks);
                }
                else if (currentType == typeof(EDBTimeStampTZ))
                {
                    return new TimeSpan(((DateTime)(EDBTimeStampTZ)value).ToUniversalTime().Ticks);
                }
                else if (currentType == typeof(EDBInterval))
                {
                    return (TimeSpan)(EDBInterval)value;
                }
                else if (currentType == typeof(DateTime))
                {
                    return new TimeSpan(((DateTime)value).ToUniversalTime().Ticks);
                }
                else if (currentType == typeof(DateTimeOffset))
                {
                    return new TimeSpan(((DateTimeOffset)value).Ticks);
                }
                else
                {
#if NET40
                    return TimeSpan.Parse(value.ToString(), CultureInfo.InvariantCulture);
#else
                    return TimeSpan.Parse(value.ToString());
#endif
                }
            }
            else if (expectedType == typeof(string))
            {
                return value.ToString();
            }
            else if (expectedType == typeof(Guid))
            {
                if (currentType == typeof(byte[]))
                {
                    return new Guid((byte[])value);
                }
                else
                {
                    return new Guid(value.ToString());
                }
            }
            else if (expectedType == typeof(DateTime))
            {
                if (currentType == typeof(EDBDate))
                {
                    return (DateTime)(EDBDate)value;
                }
                else if (currentType == typeof(EDBTime))
                {
                    return (DateTime)(EDBTime)value;
                }
                else if (currentType == typeof(EDBTimeTZ))
                {
                    return (DateTime)(EDBTimeTZ)value;
                }
                else if (currentType == typeof(EDBTimeStamp))
                {
                    return (DateTime)(EDBTimeStamp)value;
                }
                else if (currentType == typeof(EDBTimeStampTZ))
                {
                    return (DateTime)(EDBTimeStampTZ)value;
                }
                else if (currentType == typeof(EDBInterval))
                {
                    return new DateTime(((TimeSpan)(EDBInterval)value).Ticks);
                }
#if NET35
                else if (currentType == typeof(DateTimeOffset))
                {
                    return ((DateTimeOffset)value).LocalDateTime;
                }
#endif
                else if (currentType == typeof(TimeSpan))
                {
                    return new DateTime(((TimeSpan)value).Ticks);
                }
                else
                {
                    return DateTime.Parse(value.ToString(), CultureInfo.InvariantCulture);
                }
            }
            else if (expectedType == typeof(byte[]))
            {
                if (currentType == typeof(Guid))
                {
                    return ((Guid)value).ToByteArray();
                }
                else if (value is Array)
                {
                    Array valueArray = (Array)value;
                    int byteLength = Buffer.ByteLength(valueArray);
                    byte[] bytes = new byte[byteLength];
                    Buffer.BlockCopy(valueArray, 0, bytes, 0, byteLength);
                    return bytes;
                }
                else
                {
                    // expect InvalidCastException from this call
                    return Convert.ChangeType(value, expectedType);
                }
            }
            else // long, int, short, double, float, decimal, byte, sbyte, bool, and other unspecified types
            {
                // ChangeType supports the conversions we want for above expected types
                return Convert.ChangeType(value, expectedType);
            }
        }

	}
}
