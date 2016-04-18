#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using  EnterpriseDB.EDBClient.Logging;
using  EnterpriseDB.EDBClient.TypeHandlers;
using EDBTypes;
using System.Diagnostics.Contracts;

namespace  EnterpriseDB.EDBClient
{
    internal class TypeHandlerRegistry
    {
        #region Members

        internal EDBConnector Connector { get; private set; }
        internal TypeHandler UnrecognizedTypeHandler { get; private set; }

        readonly Dictionary<uint, TypeHandler> _oidIndex;
        readonly Dictionary<DbType, TypeHandler> _byDbType;
        readonly Dictionary<EDBDbType, TypeHandler> _byEDBDbType;
        readonly Dictionary<Type, TypeHandler> _byType;
        Dictionary<Type, TypeHandler> _byEnumTypeAsArray;
        List<BackendType> _backendTypes;

        static internal readonly Dictionary<string, TypeAndMapping> HandlerTypes;
        static readonly Dictionary<EDBDbType, DbType> EDBDbTypeToDbType;
        static readonly Dictionary<DbType, EDBDbType> DbTypeToEDBDbType;
        static readonly Dictionary<Type, EDBDbType> TypeToEDBDbType;
        static readonly Dictionary<Type, DbType> TypeToDbType;

        /// <summary>
        /// Caches, for each connection string, the results of the backend type query in the form of a list of type
        /// info structs keyed by the PG name.
        /// Repeated connections to the same connection string reuse the query results and avoid an additional
        /// roundtrip at open-time.
        /// </summary>
        static readonly ConcurrentDictionary<string, List<BackendType>> BackendTypeCache = new ConcurrentDictionary<string, List<BackendType>>();

        static ConcurrentDictionary<string, TypeHandler> _globalEnumRegistrations;

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        #endregion

        #region Initialization and Loading

        static internal void Setup(EDBConnector connector)
        {
            connector.TypeHandlerRegistry = new TypeHandlerRegistry(connector);

            List<BackendType> types;
            if (!BackendTypeCache.TryGetValue(connector.ConnectionString, out types)) {
                types = BackendTypeCache[connector.ConnectionString] = LoadBackendTypes(connector);
            }

            connector.TypeHandlerRegistry.RegisterTypes(types);
        }

        TypeHandlerRegistry(EDBConnector connector)
        {
            Connector = connector;
            UnrecognizedTypeHandler = new UnrecognizedTypeHandler();
            _oidIndex = new Dictionary<uint, TypeHandler>();
            _byDbType = new Dictionary<DbType, TypeHandler>();
            _byEDBDbType = new Dictionary<EDBDbType, TypeHandler>();
            _byType = new Dictionary<Type, TypeHandler>();
            _byType[typeof(DBNull)] = UnrecognizedTypeHandler;
            _byEDBDbType[EDBDbType.Unknown] = UnrecognizedTypeHandler;
        }

        static List<BackendType> LoadBackendTypes(EDBConnector connector)
        {
            var byOID = new Dictionary<uint, BackendType>();

            // Select all types (base, array which is also base, enum, range).
            // Note that arrays are distinguished from primitive types through them having typreceive=array_recv.
            // Order by primitives first, container later.
            // For arrays and ranges, join in the element OID and type (to filter out arrays of unhandled
            // types).
            var query =
                @"SELECT a.typname, a.oid, " +
                @"CASE WHEN pg_proc.proname='array_recv' THEN 'a' ELSE a.typtype END AS type, " +
                @"CASE " +
                  @"WHEN pg_proc.proname='array_recv' THEN a.typelem " +
                  (connector.SupportsRangeTypes ? @"WHEN a.typtype='r' THEN rngsubtype " : "")+
                  @"ELSE 0 " +
                @"END AS elemoid, " +
                @"CASE " +
                  @"WHEN pg_proc.proname IN ('array_recv','oidvectorrecv') THEN 2 " +  // Arrays last
                  @"WHEN a.typtype='r' THEN 1 " +                                      // Ranges before
                  @"ELSE 0 " +                                                         // Base types first
                @"END AS ord " +
                @"FROM pg_type AS a " +
                @"JOIN pg_proc ON pg_proc.oid = a.typreceive " +
                @"LEFT OUTER JOIN pg_type AS b ON (b.oid = a.typelem) " +
                (connector.SupportsRangeTypes ? @"LEFT OUTER JOIN pg_range ON (pg_range.rngtypid = a.oid) " : "") +
                @"WHERE a.typtype IN ('b', 'r', 'e') AND (b.typtype IS NULL OR b.typtype IN ('b', 'r', 'e')) " +
                @"ORDER BY ord";

            var types = new List<BackendType>();
            using (var command = new EDBCommand(query, connector.Connection))
            {
                command.AllResultTypesAreUnknown = true;
                using (var dr = command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (dr.Read())
                    {
                        var backendType = new BackendType
                        {
                            Name = dr.GetString(0),
                            OID = Convert.ToUInt32(dr[1])
                        };

                        Contract.Assume(backendType.Name != null);
                        Contract.Assume(backendType.OID != 0);

                        uint elementOID;
                        var typeChar = dr.GetString(2)[0];
                        switch (typeChar)
                        {
                        case 'b':  // Normal base type
                            backendType.Type = BackendTypeType.Base;
                            break;
                        case 'a':   // Array
                            backendType.Type = BackendTypeType.Array;
                            elementOID = Convert.ToUInt32(dr[3]);
                            Contract.Assume(elementOID > 0);
                            if (!byOID.TryGetValue(elementOID, out backendType.Element)) {
                                Log.Trace(string.Format("Array type '{0}' refers to unknown element with OID {1}, skipping", backendType.Name, elementOID), connector.Id);
                                continue;
                            }
                            backendType.Element.Array = backendType;
                            break;
                        case 'e':   // Enum
                            backendType.Type = BackendTypeType.Enum;
                            break;
                        case 'r':   // Range
                            backendType.Type = BackendTypeType.Range;
                            elementOID = Convert.ToUInt32(dr[3]);
                            Contract.Assume(elementOID > 0);
                            if (!byOID.TryGetValue(elementOID, out backendType.Element)) {
                                Log.Error(String.Format("Range type '{0}' refers to unknown subtype with OID {1}, skipping", backendType.Name, elementOID), connector.Id);
                                continue;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(String.Format("Unknown typtype for type '{0}' in pg_type: {1}", backendType.Name, typeChar));
                        }

                        types.Add(backendType);
                        byOID[backendType.OID] = backendType;
                    }
                }
            }

            /*foreach (var notFound in _typeHandlers.Where(t => t.Oid == -1)) {
                _log.WarnFormat("Could not find type {0} in pg_type", notFound.PgNames[0]);
            }*/

            return types;
        }

        void RegisterTypes(List<BackendType> backendTypes)
        {
            foreach (var backendType in backendTypes)
            {
                switch (backendType.Type) {
                case BackendTypeType.Base:
                    RegisterBaseType(backendType);
                    continue;
                case BackendTypeType.Array:
                    RegisterArrayType(backendType);
                    continue;
                case BackendTypeType.Range:
                    RegisterRangeType(backendType);
                    continue;
                case BackendTypeType.Enum:
                    TypeHandler handler;
                    if (_globalEnumRegistrations != null && _globalEnumRegistrations.TryGetValue(backendType.Name, out handler)) {
                        ActivateEnumType(handler, backendType);
                    }
                    continue;
                default:
                    Log.Error("Unknown type of type encountered, skipping: " + backendType, Connector.Id);
                    continue;
                }
            }

            _backendTypes = backendTypes;
        }

        void RegisterBaseType(BackendType backendType)
        {
            TypeAndMapping typeAndMapping;
            if (!HandlerTypes.TryGetValue(backendType.Name, out typeAndMapping)) {
                // Backend type not supported by  EnterpriseDB.EDBClient
                return;
            }

            var handlerType = typeAndMapping.HandlerType;
            var mapping = typeAndMapping.Mapping;

            // Instantiate the type handler. If it has a constructor that accepts an EDBConnector, use that to allow
            // the handler to make connector-specific adjustments. Otherwise (the normal case), use the default constructor.
            var handler = (TypeHandler)(
                handlerType.GetConstructor(new[] { typeof(TypeHandlerRegistry) }) != null
                    ? Activator.CreateInstance(handlerType, this)
                    : Activator.CreateInstance(handlerType)
            );

            handler.OID = backendType.OID;
            _oidIndex[backendType.OID] = handler;
            handler.PgName = backendType.Name;

            if (mapping.EDBDbType.HasValue)
            {
                var EDBDbType = mapping.EDBDbType.Value;
                if (_byEDBDbType.ContainsKey(EDBDbType))
                    throw new Exception(String.Format("Two type handlers registered on same EDBDbType {0}: {1} and {2}",
                                        EDBDbType, _byEDBDbType[EDBDbType].GetType().Name, handlerType.Name));
                _byEDBDbType[EDBDbType] = handler;
                handler.EDBDbType = EDBDbType;
            }

            foreach (var dbType in mapping.DbTypes)
            {
                if (_byDbType.ContainsKey(dbType))
                    throw new Exception(String.Format("Two type handlers registered on same DbType {0}: {1} and {2}",
                                        dbType, _byDbType[dbType].GetType().Name, handlerType.Name));
                _byDbType[dbType] = handler;
            }

            foreach (var type in mapping.Types)
            {
                if (_byType.ContainsKey(type))
                    throw new Exception(String.Format("Two type handlers registered on same .NET type {0}: {1} and {2}",
                                        type, _byType[type].GetType().Name, handlerType.Name));
                _byType[type] = handler;
            }
        }

        #endregion

        #region Array

        void RegisterArrayType(BackendType backendType)
        {
            Contract.Requires(backendType.Element != null);

            TypeHandler elementHandler;
            if (!_oidIndex.TryGetValue(backendType.Element.OID, out elementHandler)) {
                // Array type referring to an unhandled element type
                return;
            }

            ArrayHandler arrayHandler;

            var asBitStringHandler = elementHandler as BitStringHandler;
            if (asBitStringHandler != null) {
                // BitString requires a special array handler which returns bool or BitArray
                arrayHandler = new BitStringArrayHandler(asBitStringHandler);
            } else if (elementHandler is ITypeHandlerWithPsv) {
                var arrayHandlerType = typeof(ArrayHandlerWithPsv<,>).MakeGenericType(elementHandler.GetFieldType(), elementHandler.GetProviderSpecificFieldType());
                arrayHandler = (ArrayHandler)Activator.CreateInstance(arrayHandlerType, elementHandler);
            } else {
                var arrayHandlerType = typeof(ArrayHandler<>).MakeGenericType(elementHandler.GetFieldType());
                arrayHandler = (ArrayHandler)Activator.CreateInstance(arrayHandlerType, elementHandler);
            }

            arrayHandler.PgName = "array";
            arrayHandler.OID = backendType.OID;
            _oidIndex[backendType.OID] = arrayHandler;

            if (elementHandler is IEnumHandler)
            {
                if (_byEnumTypeAsArray == null) {
                    _byEnumTypeAsArray = new Dictionary<Type, TypeHandler>();
                }
                var enumType = elementHandler.GetType().GetGenericArguments()[0];
                Contract.Assert(enumType.GetTypeInfo().IsEnum);
                _byEnumTypeAsArray[enumType] = arrayHandler;
            }
            else
            {
                _byEDBDbType[EDBDbType.Array | elementHandler.EDBDbType] = arrayHandler;
            }
        }

        #endregion

        #region Range

        void RegisterRangeType(BackendType backendType)
        {
            Contract.Requires(backendType.Element != null);

            TypeHandler elementHandler;
            if (!_oidIndex.TryGetValue(backendType.Element.OID, out elementHandler))
            {
                // Range type referring to an unhandled element type
                return;
            }

            var rangeHandlerType = typeof(RangeHandler<>).MakeGenericType(elementHandler.GetFieldType());
            var handler = (TypeHandler)Activator.CreateInstance(rangeHandlerType, elementHandler, backendType.Name);

            handler.PgName = backendType.Name;
            handler.EDBDbType = EDBDbType.Range | elementHandler.EDBDbType;
            handler.OID = backendType.OID;
            _oidIndex[backendType.OID] = handler;
            _byEDBDbType.Add(handler.EDBDbType, handler);
        }

        #endregion

        #region Enum

        internal void RegisterEnumType<TEnum>(string pgName) where TEnum : struct
        {
            var backendTypeInfo = _backendTypes.FirstOrDefault(t => t.Name == pgName);
            if (backendTypeInfo == null) {
                throw new Exception(String.Format("An enum with the name {0} was not found in the database", pgName));
            }

            var handler = new EnumHandler<TEnum>();
            ActivateEnumType(handler, backendTypeInfo);
        }

        internal static void RegisterEnumTypeGlobally<TEnum>(string pgName) where TEnum : struct
        {
            if (_globalEnumRegistrations == null) {
                _globalEnumRegistrations = new ConcurrentDictionary<string, TypeHandler>();
            }

            _globalEnumRegistrations[pgName] = new EnumHandler<TEnum>();
        }

        void ActivateEnumType(TypeHandler handler, BackendType backendType)
        {
            handler.PgName = backendType.Name;
            handler.OID = backendType.OID;
            handler.EDBDbType = EDBDbType.Enum;
            _oidIndex[backendType.OID] = handler;
            _byType[handler.GetFieldType()] = handler;

            if (backendType.Array != null) {
                RegisterArrayType(backendType.Array);
            }
        }

        #endregion

        #region Lookups

        /// <summary>
        /// Looks up a type handler by its Postgresql type's OID.
        /// </summary>
        /// <param name="oid">A Postgresql type OID</param>
        /// <returns>A type handler that can be used to encode and decode values.</returns>
        internal TypeHandler this[uint oid]
        {
            get
            {
                TypeHandler result;
                if (!_oidIndex.TryGetValue(oid, out result)) {
                    result = UnrecognizedTypeHandler;
                }
                return result;
            }
            set { _oidIndex[oid] = value; }
        }

        internal TypeHandler this[EDBDbType EDBDbType, Type enumType = null]
        {
            get
            {
                TypeHandler handler;
                if (_byEDBDbType.TryGetValue(EDBDbType, out handler)) {
                    return handler;
                }

                if (EDBDbType == EDBDbType.Enum)
                {
                    if (enumType == null) {
                        throw new InvalidCastException("Either specify EnumType along with EDBDbType.Enum, or leave both empty to infer from Value");
                    }

                    if (!_byType.TryGetValue(enumType, out handler)) {
                        throw new NotSupportedException("This enum type is not supported (have you registered it in Npsql and set the EnumType property of EDBParameter?)");
                    }
                    return handler;
                }

                if (EDBDbType == (EDBDbType.Enum | EDBDbType.Array))
                {
                    if (enumType == null) {
                        throw new InvalidCastException("Either specify EnumType along with EDBDbType.Enum, or leave both empty to infer from Value");
                    }

                    if (_byEnumTypeAsArray != null && _byEnumTypeAsArray.TryGetValue(enumType, out handler)) {
                        return handler;
                    }
                    throw new NotSupportedException("This enum array type is not supported (have you registered it in Npsql and set the EnumType property of EDBParameter?)");
                }

                throw new NotSupportedException("This EDBDbType isn't supported in  EnterpriseDB.EDBClient yet: " + EDBDbType);
            }
        }

        internal TypeHandler this[DbType dbType]
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeHandler>() != null);

                TypeHandler handler;
                if (!_byDbType.TryGetValue(dbType, out handler)) {
                    throw new NotSupportedException("This DbType is not supported in  EnterpriseDB.EDBClient: " + dbType);
                }
                return handler;
            }
        }

        internal TypeHandler this[object value]
        {
            get
            {
                Contract.Requires(value != null);
                Contract.Ensures(Contract.Result<TypeHandler>() != null);

                if (value is DateTime)
                {
                    return ((DateTime) value).Kind == DateTimeKind.Utc
                        ? this[EDBDbType.TimestampTZ]
                        : this[EDBDbType.Timestamp];
                }

                if (value is EDBDateTime) {
                    return ((EDBDateTime)value).Kind == DateTimeKind.Utc
                        ? this[EDBDbType.TimestampTZ]
                        : this[EDBDbType.Timestamp];
                }

                return this[value.GetType()];
            }
        }

        internal TypeHandler this[Type type]
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeHandler>() != null);

                TypeHandler handler;
                if (_byType.TryGetValue(type, out handler)) {
                    return handler;
                }

                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    if (elementType.GetTypeInfo().IsEnum) {
                        if (_byEnumTypeAsArray != null && _byEnumTypeAsArray.TryGetValue(elementType, out handler)) {
                            return handler;
                        }
                        throw new Exception("Enums must be registered with  EnterpriseDB.EDBClient via Connection.RegisterEnumType or RegisterEnumTypeGlobally");
                    }

                    if (!_byType.TryGetValue(elementType, out handler)) {
                        throw new NotSupportedException("This .NET type is not supported in  EnterpriseDB.EDBClient or your PostgreSQL: " + type);
                    }
                    return this[EDBDbType.Array | handler.EDBDbType];
                }

                var typeInfo = type.GetTypeInfo();

                if (typeof(IList).IsAssignableFrom(type))
                {
                    if (typeInfo.IsGenericType)
                    {
                        if (!_byType.TryGetValue(type.GetGenericArguments()[0], out handler)) {
                            throw new NotSupportedException("This .NET type is not supported in  EnterpriseDB.EDBClient or your PostgreSQL: " + type);
                        }
                        return this[EDBDbType.Array | handler.EDBDbType];
                    }
                    throw new NotSupportedException("Non-generic IList is a supported parameter, but the EDBDbType parameter must be set on the parameter");
                }

                if (typeInfo.IsEnum) {
                    throw new Exception("Enums must be registered with  EnterpriseDB.EDBClient via Connection.RegisterEnumType or RegisterEnumTypeGlobally");
                }

                if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(EDBRange<>))
                {
                    if (!_byType.TryGetValue(type.GetGenericArguments()[0], out handler)) {
                        throw new NotSupportedException("This .NET range type is not supported in your PostgreSQL: " + type);
                    }
                    return this[EDBDbType.Range | handler.EDBDbType];
                }

                throw new NotSupportedException("This .NET type is not supported in  EnterpriseDB.EDBClient or your PostgreSQL: " + type);
            }
        }

        internal static EDBDbType ToEDBDbType(DbType dbType)
        {
            return DbTypeToEDBDbType[dbType];
        }

        internal static EDBDbType ToEDBDbType(Type type)
        {
            EDBDbType EDBDbType;
            if (TypeToEDBDbType.TryGetValue(type, out EDBDbType)) {
                return EDBDbType;
            }

            if (type.IsArray)
            {
                if (type == typeof(byte[])) {
                    return EDBDbType.Bytea;
                }
                return EDBDbType.Array | ToEDBDbType(type.GetElementType());
            }

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsEnum) {
                return EDBDbType.Enum;
            }

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(EDBRange<>)) {
                return EDBDbType.Range | ToEDBDbType(type.GetGenericArguments()[0]);
            }

            if (type == typeof(DBNull))
            {
                return EDBDbType.Unknown;
            }

            throw new NotSupportedException("Can't infer EDBDbType for type " + type);
        }

        internal static DbType ToDbType(Type type)
        {
            DbType dbType;
            return TypeToDbType.TryGetValue(type, out dbType) ? dbType : DbType.Object;
        }

        internal static DbType ToDbType(EDBDbType EDBDbType)
        {
            DbType dbType;
            return EDBDbTypeToDbType.TryGetValue(EDBDbType, out dbType) ? dbType : DbType.Object;
        }

        #endregion

        #region Type Handler Discovery

        static TypeHandlerRegistry()
        {
            HandlerTypes = new Dictionary<string, TypeAndMapping>();
            EDBDbTypeToDbType = new Dictionary<EDBDbType, DbType>();
            DbTypeToEDBDbType = new Dictionary<DbType, EDBDbType>();
            TypeToEDBDbType = new Dictionary<Type, EDBDbType>();
            TypeToDbType = new Dictionary<Type, DbType>();

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(TypeHandler))))
            {
                var mappings = t.GetCustomAttributes(typeof(TypeMappingAttribute), false);
                if (!mappings.Any())
                    continue;

                foreach (TypeMappingAttribute m in mappings)
                {
                    if (HandlerTypes.ContainsKey(m.PgName)) {
                        throw new Exception("Two type handlers registered on same PostgreSQL type name: " + m.PgName);
                    }
                    HandlerTypes[m.PgName] = new TypeAndMapping { HandlerType=t, Mapping=m };
                    if (!m.EDBDbType.HasValue) {
                        continue;
                    }
                    var EDBDbType = m.EDBDbType.Value;

                    var inferredDbType = m.InferredDbType;

                    if (inferredDbType != null) {
                        EDBDbTypeToDbType[EDBDbType] = inferredDbType.Value;
                    }
                    foreach (var dbType in m.DbTypes) {
                        DbTypeToEDBDbType[dbType] = EDBDbType;
                    }
                    foreach (var type in m.Types)
                    {
                        TypeToEDBDbType[type] = EDBDbType;
                        if (inferredDbType != null) {
                            TypeToDbType[type] = inferredDbType.Value;
                        }
                    }
                }
            }
        }

        #endregion

        #region Misc

        /// <summary>
        /// Clears the internal type cache.
        /// Useful for forcing a reload of the types after loading an extension.
        /// </summary>
        static internal void ClearBackendTypeCache()
        {
            BackendTypeCache.Clear();
        }

        /// <summary>
        /// Clears the internal type cache.
        /// Useful for forcing a reload of the types after loading an extension.
        /// </summary>
        static internal void ClearBackendTypeCache(string connectionString)
        {
            List<BackendType> types;
            BackendTypeCache.TryRemove(connectionString, out types);
        }

        #endregion

        #region Debugging / Testing
#if DEBUG
        internal Dictionary<uint, TypeHandler> OIDIndex { get { return _oidIndex; } }
#endif
        #endregion
    }

    class BackendType
    {
        internal string Name;
        internal uint OID;
        internal BackendTypeType Type;
        internal BackendType Element;
        internal BackendType Array;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }

    struct TypeAndMapping
    {
        internal Type HandlerType;
        internal TypeMappingAttribute Mapping;
    }

    /// <summary>
    /// Specifies the type of a type, as represented in the PostgreSQL typtype column of the pg_type table.
    /// See http://www.postgresql.org/docs/current/static/catalog-pg-type.html
    /// </summary>
    enum BackendTypeType
    {
        Base,
        Array,
        Range,
        Enum,
        Pseudo
    }
}
