// EDBTypes.EDBTypesHelper.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The EDB Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Globalization;
using System.Data;
using System.Net;
using System.Text;
using System.Resources;
using EnterpriseDB.EDBClient;


namespace EDBTypes
{
    /// <summary>
    ///	This class contains helper methods for type conversion between
    /// the .Net type system and postgresql.
    /// </summary>
    internal abstract class EDBTypesHelper
    {
        // Logging related values
        private static readonly String CLASSNAME = "EDBTypesHelper";
        private static ResourceManager resman = new ResourceManager(typeof(EDBTypesHelper));

        /// <summary>
        /// A cache of basic datatype mappings keyed by server version.  This way we don't
        /// have to load the basic type mappings for every connection.
        /// </summary>
        private static Hashtable BackendTypeMappingCache = new Hashtable();
        private static EDBNativeTypeMapping NativeTypeMapping = null;


        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given EDBDbType.
        /// </summary>
        public static EDBNativeTypeInfo GetNativeTypeInfo(EDBDbType EDBDbType)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromEDBDbType");

            VerifyDefaultTypesMap();
            return NativeTypeMapping[EDBDbType];
        }
        
        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given DbType.
        /// </summary>
        public static EDBNativeTypeInfo GetNativeTypeInfo(DbType DbType)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromEDBDbType");

            VerifyDefaultTypesMap();
            return NativeTypeMapping[DbType];
        }
        
        

        /// <summary>
        /// Find a EDBNativeTypeInfo in the default types map that can handle objects
        /// of the given System.Type.
        /// </summary>
        public static EDBNativeTypeInfo GetNativeTypeInfo(Type Type)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromEDBDbType");

            VerifyDefaultTypesMap();
            return NativeTypeMapping[Type];
        }

        // CHECKME
        // Not sure what to do with this one.  I don't believe we ever ask for a binary
        // formatting, so this shouldn't even be used right now.
        // At some point this will need to be merged into the type converter system somehow?
        public static Object ConvertBackendBytesToSystemType(EDBBackendTypeInfo TypeInfo, Byte[] data, Encoding encoding, Int32 fieldValueSize, Int32 typeModifier)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");

            /*
            // We are never guaranteed to know about every possible data type the server can send us.
            // When we encounter an unknown type, we punt and return the data without modification.
            if (TypeInfo == null)
                return data;

            switch (TypeInfo.EDBDbType)
            {
            case EDBDbType.Binary:
                return data;
            case EDBDbType.Boolean:
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
                return encoding.GetString(data, 0, fieldValueSize);
            default:
                throw new InvalidCastException("Type not supported in binary format");
            }*/
            
            return null;
        }

        ///<summary>
        /// This method is responsible to convert the string received from the backend
        /// to the corresponding EDBType.
        /// The given TypeInfo is called upon to do the conversion.
        /// If no TypeInfo object is provided, no conversion is performed.
        /// </summary>
        public static Object ConvertBackendStringToSystemType(EDBBackendTypeInfo TypeInfo, String data, Int16 typeSize, Int32 typeModifier)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

            if (TypeInfo != null) {
                return TypeInfo.ConvertToNative(data, typeSize, typeModifier);
            } else {
                return data;
            }
        }

        /// <summary>
        /// Create the one and only native to backend type map.
        /// This map is used when formatting native data
        /// types to backend representations.
        /// </summary>
        private static void VerifyDefaultTypesMap()
        {
            lock(CLASSNAME) {
                if (NativeTypeMapping != null) {
                    return;
                }


                NativeTypeMapping = new EDBNativeTypeMapping();
                

                NativeTypeMapping.AddType("text", EDBDbType.Text, DbType.String, true, null);

                NativeTypeMapping.AddDbTypeAlias("text", DbType.StringFixedLength);
                NativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiString);
//                NativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiStringFixedLength);
//                NativeTypeMapping.AddTypeAlias("text", typeof(String));
//				
			    //EnterpriseDB Team Adding Char support.
			    NativeTypeMapping.AddType("char", EDBDbType.Char, DbType.String, true, null);
                NativeTypeMapping.AddType("bytea", EDBDbType.Bytea, DbType.Binary, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBinary));

                NativeTypeMapping.AddTypeAlias("bytea", typeof(Byte[]));
				//SA:Added, to support Array, Start
				NativeTypeMapping.AddType("_bool", EDBDbType.BooleanArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_bool", typeof(bool[]));

				NativeTypeMapping.AddType("_int4", EDBDbType.IntegerArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_int4", typeof(int[]));

				NativeTypeMapping.AddType("_int2", EDBDbType.SmallintArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_int2", typeof(short[]));

				NativeTypeMapping.AddType("_int8", EDBDbType.LongArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_int8", typeof(long[]));

				NativeTypeMapping.AddType("_float4", EDBDbType.FloatArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_float4", typeof(float[]));

				NativeTypeMapping.AddType("_float8", EDBDbType.DoubleArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_float8", typeof(double[]));

				NativeTypeMapping.AddType("_char", EDBDbType.CharArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_char", typeof(char[]));

				NativeTypeMapping.AddType("_varchar", EDBDbType.StringArray, DbType.Binary, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));
				NativeTypeMapping.AddTypeAlias("_varchar", typeof(string[]));
				//SA:Added, to support Array, End

                NativeTypeMapping.AddType("bool", EDBDbType.Boolean, DbType.Boolean, false,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBoolean));

                NativeTypeMapping.AddTypeAlias("bool", typeof(Boolean));
                                
                NativeTypeMapping.AddType("int2", EDBDbType.Smallint, DbType.Int16, false,
                null);

                NativeTypeMapping.AddTypeAlias("int2", typeof(Int16));
                
                NativeTypeMapping.AddDbTypeAlias("int2", DbType.Byte);

                NativeTypeMapping.AddType("int4", EDBDbType.Integer, DbType.Int32, false,
                null);

                NativeTypeMapping.AddTypeAlias("int4", typeof(Int32));

                NativeTypeMapping.AddType("int8", EDBDbType.Bigint, DbType.Int64, false,
                null);

                NativeTypeMapping.AddTypeAlias("int8", typeof(Int64));

                NativeTypeMapping.AddType("float4", EDBDbType.Float, DbType.Single, false,
                null);

                NativeTypeMapping.AddTypeAlias("float4", typeof(Single));

                NativeTypeMapping.AddType("float8", EDBDbType.Double, DbType.Double, false,
                null);

                NativeTypeMapping.AddTypeAlias("float8", typeof(Double));

                NativeTypeMapping.AddType("numeric", EDBDbType.Numeric, DbType.Decimal, false,
                null);

                NativeTypeMapping.AddTypeAlias("numeric", typeof(Decimal));

                NativeTypeMapping.AddType("currency", EDBDbType.Money, DbType.Currency, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToMoney));

                NativeTypeMapping.AddType("date", EDBDbType.Date, DbType.Date, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDate));

                NativeTypeMapping.AddType("time", EDBDbType.Time, DbType.Time, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToTime));

                NativeTypeMapping.AddType("timestamp", EDBDbType.Timestamp, DbType.DateTime, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDateTime));

                //NativeTypeMapping.AddTypeAlias("timestamp", typeof(DateTime)); // 09 nov

                NativeTypeMapping.AddType("point", EDBDbType.Point, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPoint));

                NativeTypeMapping.AddTypeAlias("point", typeof(EDBPoint));
                
                NativeTypeMapping.AddType("box", EDBDbType.Box, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToBox));

                NativeTypeMapping.AddTypeAlias("box", typeof(EDBBox));
                
                NativeTypeMapping.AddType("lseg", EDBDbType.LSeg, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToLSeg));

                NativeTypeMapping.AddTypeAlias("lseg", typeof(EDBLSeg));

                NativeTypeMapping.AddType("path", EDBDbType.Path, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPath));

                NativeTypeMapping.AddTypeAlias("path", typeof(EDBPath));

                NativeTypeMapping.AddType("polygon", EDBDbType.Polygon, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPolygon));

                NativeTypeMapping.AddTypeAlias("polygon", typeof(EDBPolygon));

                NativeTypeMapping.AddType("circle", EDBDbType.Circle, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToCircle));

                NativeTypeMapping.AddTypeAlias("circle", typeof(EDBCircle));
				//EDB Team

				NativeTypeMapping.AddType("varchar", EDBDbType.Varchar, DbType.Binary, true,
					null);
				//NativeTypeMapping.AddTypeAlias("varchar", typeof(String));

				//EDB TEAM :19 OCT 05  Mapping of varchar2 and datetime F# 1184
				NativeTypeMapping.AddType("varchar2", EDBDbType.Varchar2, DbType.String, true,
					null);
				NativeTypeMapping.AddTypeAlias("varchar2", typeof(String));

				NativeTypeMapping.AddType("datetime", EDBDbType.DateTime, DbType.DateTime, true,
					new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDateTime));
				NativeTypeMapping.AddTypeAlias("datetime", typeof(DateTime));
				//Add mapping for RefCursor
				NativeTypeMapping.AddType("refcursor", EDBDbType.RefCursor, DbType.String, true, null);

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

            // [TODO] Verify another way to get higher concurrency.
            lock(CLASSNAME)
            {
                // Check the cache for an initial types map.
                EDBBackendTypeMapping oidToNameMapping = (EDBBackendTypeMapping) BackendTypeMappingCache[conn.ServerVersion];

                if (oidToNameMapping != null)
                {
                    return oidToNameMapping;
                }

                // Not in cache, create a new one.
                oidToNameMapping = new EDBBackendTypeMapping();

                // Create a list of all natively supported postgresql data types.
                EDBBackendTypeInfo[] TypeInfoList = new EDBBackendTypeInfo[]
                {
                    new EDBBackendTypeInfo(0, "unknown", EDBDbType.Text, DbType.String, typeof(String),
                        null),
					
                    new EDBBackendTypeInfo(0, "char", EDBDbType.Text, DbType.String, typeof(String),
                        null),

                    new EDBBackendTypeInfo(0, "bpchar", EDBDbType.Text, DbType.String, typeof(String),
                        null),

                    new EDBBackendTypeInfo(0, "varchar", EDBDbType.Text, DbType.String, typeof(String),
                        null),

//EDB Team
//					new EDBBackendTypeInfo(0, "varchar2", EDBDbType.Text, DbType.String, typeof(String),
//					null),
//End 

                   new EDBBackendTypeInfo(0, "text", EDBDbType.Text, DbType.String, typeof(String),
                        null),
                        
                    new EDBBackendTypeInfo(0, "name", EDBDbType.Text, DbType.String, typeof(String),
                        null),

                    new EDBBackendTypeInfo(0, "bytea", EDBDbType.Bytea, DbType.Binary, typeof(Byte[]),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBinary)),


                    new EDBBackendTypeInfo(0, "bool", EDBDbType.Boolean, DbType.Boolean, typeof(Boolean),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBoolean)),


                    new EDBBackendTypeInfo(0, "int2", EDBDbType.Smallint, DbType.Int16, typeof(Int16),
                        null),

                    new EDBBackendTypeInfo(0, "int4", EDBDbType.Integer, DbType.Int32, typeof(Int32),
                        null),

                    new EDBBackendTypeInfo(0, "int8", EDBDbType.Bigint, DbType.Int64, typeof(Int64),
                        null),

                    new EDBBackendTypeInfo(0, "oid", EDBDbType.Bigint, DbType.Int64, typeof(Int64),
                        null),

//EDB team ..//Real to introduce Float
//                    new EDBBackendTypeInfo(0, "float4", EDBDbType.Real, DbType.Single, typeof(Single),
//                        null),

                    
					new EDBBackendTypeInfo(0, "float4", EDBDbType.Float, DbType.Single, typeof(Single),
					null),
//END EDB Team
					
					new EDBBackendTypeInfo(0, "float8", EDBDbType.Double, DbType.Double, typeof(Double),
                        null),

                    new EDBBackendTypeInfo(0, "numeric", EDBDbType.Numeric, DbType.Decimal, typeof(Decimal),
                        null),

                    new EDBBackendTypeInfo(0, "money", EDBDbType.Money, DbType.Decimal, typeof(Decimal),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToMoney)),


                    new EDBBackendTypeInfo(0, "date", EDBDbType.Date, DbType.Date, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDate)),

                    new EDBBackendTypeInfo(0, "time", EDBDbType.Time, DbType.Time, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime)),

                    new EDBBackendTypeInfo(0, "timetz", EDBDbType.Time, DbType.Time, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime)),

                    new EDBBackendTypeInfo(0, "timestamp", EDBDbType.Timestamp, DbType.DateTime, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime)),

                    new EDBBackendTypeInfo(0, "timestamptz", EDBDbType.Timestamp, DbType.DateTime, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime)),


                    new EDBBackendTypeInfo(0, "point", EDBDbType.Point, DbType.Object, typeof(EDBPoint),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPoint)),

                    new EDBBackendTypeInfo(0, "lseg", EDBDbType.LSeg, DbType.Object, typeof(EDBLSeg),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToLSeg)),

                    new EDBBackendTypeInfo(0, "path", EDBDbType.Path, DbType.Object, typeof(EDBPath),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPath)),

                    new EDBBackendTypeInfo(0, "box", EDBDbType.Box, DbType.Object, typeof(EDBBox),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToBox)),

                    new EDBBackendTypeInfo(0, "circle", EDBDbType.Circle, DbType.Object, typeof(EDBCircle),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToCircle)),

                    new EDBBackendTypeInfo(0, "polygon", EDBDbType.Polygon, DbType.Object, typeof(EDBPolygon),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPolygon)),
                };

                // Attempt to map each type info in the list to an OID on the backend and
                // add each mapped type to the new type mapping object.
                LoadTypesMappings(conn, oidToNameMapping, TypeInfoList);

                // Add this mapping to the per-server-version cache so we don't have to
                // do these expensive queries on every connection startup.
                BackendTypeMappingCache.Add(conn.ServerVersion, oidToNameMapping);

                return oidToNameMapping;
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
        public static void LoadTypesMappings(EDBConnector conn, EDBBackendTypeMapping TypeMappings, IList TypeInfoList)
        {
            StringBuilder       InList = new StringBuilder();
            Hashtable           NameIndex = new Hashtable();

            // Build a clause for the SELECT statement.
            // Build a name->typeinfo mapping so we can match the results of the query
            /// with the list of type objects efficiently.
            foreach (EDBBackendTypeInfo TypeInfo in TypeInfoList) {
                NameIndex.Add(TypeInfo.Name, TypeInfo);
                InList.AppendFormat("{0}'{1}'", ((InList.Length > 0) ? ", " : ""), TypeInfo.Name);
            }

            if (InList.Length == 0) {
                return;
            }

            EDBCommand       command = new EDBCommand("SELECT oid, typname FROM pg_type WHERE typname IN (" + InList.ToString() + ")", conn);
            EDBDataReader    dr = command.ExecuteReader();

            while (dr.Read()) {
                EDBBackendTypeInfo TypeInfo = (EDBBackendTypeInfo)NameIndex[dr[1].ToString()];

                TypeInfo._OID = Convert.ToInt32(dr[0]);
				
                TypeMappings.AddType(TypeInfo);
				
            }
        }
    }

    /// <summary>
    /// Delegate called to convert the given backend data to its native representation.
    /// </summary>
    internal delegate Object ConvertBackendToNativeHandler(EDBBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier);
    /// <summary>
    /// Delegate called to convert the given native data to its backand representation.
    /// </summary>
    internal delegate String ConvertNativeToBackendHandler(EDBNativeTypeInfo TypeInfo, Object NativeData);

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a backend field representation to a native object.
    /// </summary>
    internal class EDBBackendTypeInfo
    {
        private ConvertBackendToNativeHandler _ConvertBackendToNative;

        internal Int32           _OID;
	//	private Int32			 _Oid;//EDB Team
		private String           _Name;
        private EDBDbType     _EDBDbType;
        private DbType           _DbType;
        private Type             _Type;

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
			_OID = OID;
			 _Name = Name;
            _EDBDbType = EDBDbType;
            _DbType = DbType;
			_Type = Type;
            _ConvertBackendToNative = ConvertBackendToNative;
        }

        /// <summary>
        /// Type OID provided by the backend server.
        /// </summary>
        public  Int32 OID
        {
          get { return _OID; }
        }

        /// <summary>
        /// Type name provided by the backend server.
        /// </summary>
        public String Name
        { 
			get 
			{
				
				return _Name; 
			} 
		}
		

        /// <summary>
        /// EDBDbType.
        /// </summary>
        public EDBDbType EDBDbType
        { get { return _EDBDbType; } }

        /// <summary>
        /// EDBDbType.
        /// </summary>
        public DbType DbType
        { get { return _DbType; } }
        
        /// <summary>
        /// System type to convert fields of this type to.
        /// </summary>
        public Type Type
        { get { return _Type; } }

        /// <summary>
        /// Perform a data conversion from a backend representation to 
        /// a native object.
        /// </summary>
        /// <param name="BackendData">Data sent from the backend.</param>
        /// <param name="TypeModifier">Type modifier field sent from the backend.</param>
        public Object ConvertToNative(String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            if (_ConvertBackendToNative != null) {
                return _ConvertBackendToNative(this, BackendData, TypeSize, TypeModifier);
            } else {
                try {
                	return Convert.ChangeType(BackendData, Type, CultureInfo.InvariantCulture);
                } catch {
                    return BackendData;
                }
            }
        }
    }

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a native object to its backend field representation,
    /// </summary>
    internal class EDBNativeTypeInfo
    {
        private static NumberFormatInfo ni;
        private ConvertNativeToBackendHandler _ConvertNativeToBackend;

        private String           _Name;
        private EDBDbType     _EDBDbType;
        private DbType           _DbType;
        private Boolean          _Quote;
        private Boolean          _UseSize;
        static EDBNativeTypeInfo()
        {
            ni = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
            ni.NumberDecimalDigits = 15;
        }

        /// <summary>
        /// Construct a new EDBTypeInfo with the given attributes and conversion handlers.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="EDBDbType">EDBDbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public EDBNativeTypeInfo(String Name, EDBDbType EDBDbType, DbType DbType, Boolean Quote,
                              ConvertNativeToBackendHandler ConvertNativeToBackend)
        {
            _Name = Name;
            _EDBDbType = EDBDbType;
            _DbType = DbType;
            _Quote = Quote;
            _ConvertNativeToBackend = ConvertNativeToBackend;
            // The only parameters types which use length currently supported are char and varchar. Check for them.
            
            if ( (EDBDbType == EDBDbType.Char)
                || (EDBDbType == EDBDbType.Varchar))
                
                _UseSize = false;
            else
                _UseSize = false;
        }

        /// <summary>
        /// Type name provided by the backend server.
        /// </summary>
        public String Name
        { get { return _Name; } }

		
        /// <summary>
        /// EDBDbType.
        /// </summary>
        public EDBDbType EDBDbType
        { get { return _EDBDbType; } }

        /// <summary>
        /// DbType.
        /// </summary>
        public DbType DbType
        { get { return _DbType; } }
        
        
        /// <summary>
        /// Apply quoting.
        /// </summary>
        public Boolean Quote
        { get { return _Quote; } }

        /// <summary>
        /// Use parameter size information.
        /// </summary>
        public Boolean UseSize
        { get { return _UseSize; } }
        
        
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
                return ConvertToBackendExtendedQuery(NativeData);
            else
                return ConvertToBackendPlainQuery(NativeData);
            
        }
		
	
	   private String ConvertToBackendPlainQuery(Object NativeData)
	   {
            if ((NativeData == DBNull.Value) || (NativeData == null))
                return "NULL";  // Plain queries exptects null values as string NULL. 
            
            if (_ConvertNativeToBackend != null)
                return (this.Quote ? QuoteString(_ConvertNativeToBackend(this, NativeData)) : _ConvertNativeToBackend(this, NativeData));
            else
            {
                
                
				if (NativeData is System.Enum)
				{
					// Do a special handling of Enum values.
					// Translate enum value to its underlying type. 
					return QuoteString((String)Convert.ChangeType(Enum.Format(NativeData.GetType(), NativeData, "d"), typeof(String), CultureInfo.InvariantCulture));
				}
				//SA:Added for Array Support, Start
				else if (NativeData is Array)  
				{
					StringBuilder strArray=new StringBuilder("{");
					foreach(Object obj in (Array)NativeData) 
					{
						strArray.Append(obj.ToString());
						strArray.Append(",");
					}
					if(strArray.Length >1) 
						strArray.Remove(strArray.Length-1,1);
					strArray.Append("}");
					return QuoteString(strArray.ToString().Replace("'", "''").Replace("\\", "\\\\"));
				}else
				//SA:Added for Array Support, End
                    // Do special handling of strings when in simple query. Escape quotes and backslashes.
                return (this.Quote ? QuoteString(NativeData.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace("\0", "\\0")) : 
                NativeData.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace("\0", "\\0"));
                
            }
    
        }
        
        private String ConvertToBackendExtendedQuery(Object NativeData)
        {
            if ((NativeData == DBNull.Value) || (NativeData == null))
                return null;    // Extended query expects null values be represented as null.
            
            if (_ConvertNativeToBackend != null)
                return _ConvertNativeToBackend(this, NativeData);
            else
            {
                if (NativeData is System.Enum)
                {
                    // Do a special handling of Enum values.
                    // Translate enum value to its underlying type. 
                    return (String)Convert.ChangeType(Enum.Format(NativeData.GetType(), NativeData, "d"), typeof(String), CultureInfo.InvariantCulture);
                }
                else if (NativeData is Double) 
                {
                    return ((Double)NativeData).ToString();
                    
                }
                else if (NativeData is Decimal) 
                {
                    return ((Decimal)NativeData).ToString("N", ni);
                } 
                    return NativeData.ToString();
                
            }
        
        }

        private static String QuoteString(String S)
        {
            return String.Format("'{0}'", S);
            
        }
    }

    /// <summary>
    /// Provide mapping between type OID, type name, and a EDBBackendTypeInfo object that represents it.
    /// </summary>
    internal class EDBBackendTypeMapping
    {
        private Hashtable       OIDIndex;
        private Hashtable       NameIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public EDBBackendTypeMapping()
        {
            OIDIndex = new Hashtable();
            NameIndex = new Hashtable();
        }

        /// <summary>
        /// Copy constuctor.
        /// </summary>
        private EDBBackendTypeMapping(EDBBackendTypeMapping Other)
        {
            OIDIndex = (Hashtable)Other.OIDIndex.Clone();
            NameIndex = (Hashtable)Other.NameIndex.Clone();
        }

        /// <summary>
        /// Add the given EDBBackendTypeInfo to this mapping.
        /// </summary>
        public void AddType(EDBBackendTypeInfo T)
        {	
		     if (OIDIndex.Contains(T.OID)) {
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
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        public void AddType(Int32 OID, String Name, EDBDbType EDBDbType, DbType DbType, Type Type,
                            ConvertBackendToNativeHandler BackendConvert)
        {
            AddType(new EDBBackendTypeInfo(OID, Name, EDBDbType, DbType, Type, BackendConvert));
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        { get { return NameIndex.Count; } }

        /// <summary>
        /// Retrieve the EDBBackendTypeInfo with the given backend type OID, or null if none found.
        /// </summary>
        public EDBBackendTypeInfo this [Int32 OID]
        {
            get
            {
                return (EDBBackendTypeInfo)OIDIndex[OID];
            }
        }

        /// <summary>
        /// Retrieve the EDBBackendTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public EDBBackendTypeInfo this [String Name]
        {
            get
            {
                return (EDBBackendTypeInfo)NameIndex[Name];
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
        private Hashtable       NameIndex;
        private Hashtable       EDBDbTypeIndex;
        private Hashtable       DbTypeIndex;
        private Hashtable       TypeIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public EDBNativeTypeMapping()
        {
            NameIndex = new Hashtable();
            EDBDbTypeIndex = new Hashtable();
            DbTypeIndex = new Hashtable();
            TypeIndex = new Hashtable();
        }

        /// <summary>
        /// Add the given EDBNativeTypeInfo to this mapping.
        /// </summary>
        public void AddType(EDBNativeTypeInfo T)
        {
            if (NameIndex.Contains(T.Name)) {
			     throw new Exception("Type already mapped");
            }

            NameIndex[T.Name] = T;
            EDBDbTypeIndex[T.EDBDbType] = T;
            DbTypeIndex[T.DbType] = T;
        }

        /// <summary>
        /// Add a new EDBNativeTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="EDBDbType">EDBDbType</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public void AddType(String Name, EDBDbType EDBDbType, DbType DbType, Boolean Quote,
                            ConvertNativeToBackendHandler NativeConvert)
        {
            AddType(new EDBNativeTypeInfo(Name, EDBDbType, DbType, Quote, NativeConvert));
        }

        public void AddEDBDbTypeAlias(String Name, EDBDbType EDBDbType)
        {
            if (EDBDbTypeIndex.Contains(EDBDbType)) {
                throw new Exception("EDBDbType already aliased");
            }

            EDBDbTypeIndex[EDBDbType] = NameIndex[Name];
        }
        
        public void AddDbTypeAlias(String Name, DbType DbType)
        {
            if (DbTypeIndex.Contains(DbType)) {
                throw new Exception("EDBDbType already aliased");
            }

            DbTypeIndex[DbType] = NameIndex[Name];
        }

        public void AddTypeAlias(String Name, Type Type)
        {
            if (TypeIndex.Contains(Type)) {
                throw new Exception("Type already aliased");
            }

            TypeIndex[Type] = NameIndex[Name];
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        { get { return NameIndex.Count; } }

        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public EDBNativeTypeInfo this [String Name]
        {
            get
            {
                return (EDBNativeTypeInfo)NameIndex[Name];
            }
        }

        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given EDBDbType, or null if none found.
        /// </summary>
        public EDBNativeTypeInfo this [EDBDbType EDBDbType]
        {
            get
            {
                return (EDBNativeTypeInfo)EDBDbTypeIndex[EDBDbType];
            }
        }
        
        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given DbType, or null if none found.
        /// </summary>
        public EDBNativeTypeInfo this [DbType DbType]
        {
            get
            {
                return (EDBNativeTypeInfo)DbTypeIndex[DbType];
            }
        }
        
        

        /// <summary>
        /// Retrieve the EDBNativeTypeInfo with the given Type, or null if none found.
        /// </summary>
        public EDBNativeTypeInfo this [Type Type]
        {
            get
            {
                return (EDBNativeTypeInfo)TypeIndex[Type];            
            }
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
}
