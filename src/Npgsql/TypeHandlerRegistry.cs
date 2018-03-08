#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.Logging;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandlers;
using EDBTypes;

namespace EnterpriseDB.EDBClient
{
    class TypeHandlerRegistry
    {
        #region Members

        internal EDBConnector Connector { get; }
        internal TypeHandler UnrecognizedTypeHandler { get; }

        internal Dictionary<uint, TypeHandler> ByOID { get; }
        internal Dictionary<DbType, TypeHandler> ByDbType { get; }
        internal Dictionary<EDBDbType, TypeHandler> ByEDBDbType { get; }

        /// <summary>
        /// Maps CLR types to their type handlers.
        /// </summary>
        internal Dictionary<Type, TypeHandler> ByType { get; }

        /// <summary>
        /// Maps CLR types to their array handlers.
        /// </summary>
        [CanBeNull]
        internal Dictionary<Type, TypeHandler> ArrayHandlerByType { get; set; }

        internal AvailablePostgresTypes PostgresTypes { get; private set; }

        /// <summary>
        /// A counter that is updated when this registry activates its global mappings.
        /// Tracks <see cref="_globalMappingChangeCounter"/>, allows us to know when a pooled
        /// connection's mappings are no longer up to date because a global mapping change has
        /// occurred.
        /// </summary>
        int _globalMappingActivationCounter = -1;

        /// <summary>
        /// A counter that is incremented whenever a global mapping change occurs (e.g.
        /// <see cref="MapEnumGlobally{T}"/>, <see cref="UnmapCompositeGlobally{T}"/>.
        /// <seealso cref="_globalMappingActivationCounter"/>
        /// </summary>
        static int _globalMappingChangeCounter;

        internal static readonly Dictionary<string, TypeAndMapping> HandlerTypes;
        static readonly Dictionary<EDBDbType, TypeAndMapping> HandlerTypesByNpsgqlDbType;
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
        static readonly ConcurrentDictionary<string, AvailablePostgresTypes> BackendTypeCache = new ConcurrentDictionary<string, AvailablePostgresTypes>();

        static readonly ConcurrentDictionary<string, IEnumHandlerFactory> _globalEnumMappings;
        static readonly ConcurrentDictionary<string, ICompositeHandlerFactory> _globalCompositeMappings;

        internal static IDictionary<string, IEnumHandlerFactory> GlobalEnumMappings => _globalEnumMappings;
        internal static IDictionary<string, ICompositeHandlerFactory> GlobalCompositeMappings => _globalCompositeMappings;

        static readonly IEDBNameTranslator DefaultNameTranslator = new EDBSnakeCaseNameTranslator();
        static readonly AvailablePostgresTypes EmptyPostgresTypes = new AvailablePostgresTypes();

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        #endregion

        #region Initialization and Loading

        internal static async Task Setup(EDBConnector connector, EDBTimeout timeout, bool async)
        {
            // Note that there's a chicken and egg problem here - LoadBackendTypes below needs a functional
            // connector to load the types, hence the strange initialization code here
            connector.TypeHandlerRegistry = new TypeHandlerRegistry(connector);

            if (!BackendTypeCache.TryGetValue(connector.ConnectionString, out var types))
                types = BackendTypeCache[connector.ConnectionString] = await LoadBackendTypes(connector, timeout, async);

            connector.TypeHandlerRegistry.PostgresTypes = types;
            connector.TypeHandlerRegistry.ActivateGlobalMappings();
        }

        TypeHandlerRegistry(EDBConnector connector)
        {
            Connector = connector;
            PostgresTypes = EmptyPostgresTypes;
            UnrecognizedTypeHandler = new UnknownTypeHandler(this);
            ByOID = new Dictionary<uint, TypeHandler>();
            ByDbType = new Dictionary<DbType, TypeHandler>();
            ByEDBDbType = new Dictionary<EDBDbType, TypeHandler>();
            ByType = new Dictionary<Type, TypeHandler> { [typeof(DBNull)] = UnrecognizedTypeHandler };
            ByEDBDbType[EDBDbType.Unknown] = UnrecognizedTypeHandler;
        }

        internal void ActivateGlobalMappings()
        {
            if (_globalMappingActivationCounter == _globalMappingChangeCounter)
                return;

            foreach (var kv in _globalEnumMappings)
            {
                var backendType = GetBackendTypeByName(kv.Key);
                var backendEnumType = backendType as PostgresEnumType;
                if (backendEnumType == null)
                {
                    Log.Warn($"While attempting to activate global enum mappings, PostgreSQL type {kv.Key} was found but is not an enum. Skipping it.", Connector.Id);
                    continue;
                }
                backendEnumType.Activate(this, kv.Value);
            }

            foreach (var kv in _globalCompositeMappings)
            {
                try
                {
                    GetCompositeType(kv.Key).Activate(this, kv.Value);
                }
                catch (Exception e)
                {
                    Log.Warn("Caught an exception while attempting to activate global composite mappings", e, Connector.Id);
                }
            }

            _globalMappingActivationCounter = _globalMappingChangeCounter;
        }

        static readonly string TypesQueryWithRange = GenerateTypesQuery(true);
        static readonly string TypesQueryWithoutRange = GenerateTypesQuery(false);

        static string GenerateTypesQuery(bool withRange)
        {
            // Select all types (base, array which is also base, enum, range, composite).
            // Note that arrays are distinguished from primitive types through them having typreceive=array_recv.
            // Order by primitives first, container later.
            // For arrays and ranges, join in the element OID and type (to filter out arrays of unhandled
            // types).
            return
$@"SELECT ns.nspname, a.typname, a.oid, a.typrelid, a.typbasetype,
CASE WHEN pg_proc.proname='array_recv' THEN 'a' ELSE a.typtype END AS type,
CASE
  WHEN pg_proc.proname='array_recv' THEN a.typelem
  {(withRange ? "WHEN a.typtype='r' THEN rngsubtype" : "")}
  ELSE 0
END AS elemoid,
CASE
  WHEN pg_proc.proname IN ('array_recv','oidvectorrecv') THEN 3    /* Arrays last */
  WHEN a.typtype='r' THEN 2                                        /* Ranges before */
  WHEN a.typtype='d' THEN 1                                        /* Domains before */
  ELSE 0                                                           /* Base types first */
END AS ord
FROM pg_type AS a
JOIN pg_namespace AS ns ON (ns.oid = a.typnamespace)
JOIN pg_proc ON pg_proc.oid = a.typreceive
LEFT OUTER JOIN pg_type AS b ON (b.oid = a.typelem)
{(withRange ? "LEFT OUTER JOIN pg_range ON (pg_range.rngtypid = a.oid) " : "")}
WHERE
  (
    a.typtype IN ('b', 'r', 'e', 'd') AND
    (b.typtype IS NULL OR b.typtype IN ('b', 'r', 'e', 'd'))  /* Either non-array or array of supported element type */
  ) OR
  (a.typname IN ('record', 'void') AND a.typtype = 'p')
ORDER BY ord";
        }

        static async Task<AvailablePostgresTypes> LoadBackendTypes(EDBConnector connector, EDBTimeout timeout, bool async)
        {
            var commandTimeout = 0;  // Default to infinity
            if (timeout.IsSet)
            {
                commandTimeout = (int)timeout.TimeLeft.TotalSeconds;
                if (commandTimeout <= 0)
                    throw new TimeoutException();
            }

            var types = new AvailablePostgresTypes();
            using (var command = new EDBCommand(connector.SupportsRangeTypes ? TypesQueryWithRange : TypesQueryWithoutRange, connector.Connection))
            {
                command.CommandTimeout = commandTimeout;
                command.AllResultTypesAreUnknown = true;
                using (var reader = async ? await command.ExecuteReaderAsync() : command.ExecuteReader())
                {
                    while (async ? await reader.ReadAsync() : reader.Read())
                    {
                        timeout.Check();
                        LoadBackendType(reader, types, connector);
                    }
                }
            }
            return types;
        }

        static void LoadBackendType(DbDataReader reader, AvailablePostgresTypes types, EDBConnector connector)
        {
            var ns = reader.GetString(0);
            var name = reader.GetString(1);
            var oid = Convert.ToUInt32(reader[2]);

            Debug.Assert(name != null);
            Debug.Assert(oid != 0);

            uint elementOID;
            var typeChar = reader.GetString(5)[0];
            switch (typeChar)
            {
            case 'b':  // Normal base type
                (
                    HandlerTypes.TryGetValue(name, out var typeAndMapping)
                        ? new PostgresBaseType(ns, name, oid, typeAndMapping.HandlerType, typeAndMapping.Mapping)
                        : new PostgresBaseType(ns, name, oid)  // Unsupported by EnterpriseDB.EDBClient
                ).AddTo(types);
                return;
            case 'a':   // Array
                elementOID = Convert.ToUInt32(reader[6]);
                Debug.Assert(elementOID > 0);
                if (!types.ByOID.TryGetValue(elementOID, out var elementPostgresType))
                {
                    Log.Trace($"Array type '{name}' refers to unknown element with OID {elementOID}, skipping", connector.Id);
                    return;
                }
                new PostgresArrayType(ns, name, oid, elementPostgresType).AddTo(types);
                return;
            case 'r':   // Range
                elementOID = Convert.ToUInt32(reader[6]);
                Debug.Assert(elementOID > 0);
                if (!types.ByOID.TryGetValue(elementOID, out elementPostgresType))
                {
                    Log.Trace($"Range type '{name}' refers to unknown subtype with OID {elementOID}, skipping", connector.Id);
                    return;
                }
                new PostgresRangeType(ns, name, oid, elementPostgresType).AddTo(types);
                return;
            case 'e':   // Enum
                new PostgresEnumType(ns, name, oid).AddTo(types);
                return;
            case 'd':   // Domain
                var baseTypeOID = Convert.ToUInt32(reader[4]);
                Debug.Assert(baseTypeOID > 0);
                PostgresType basePostgresType;
                if (!types.ByOID.TryGetValue(baseTypeOID, out basePostgresType))
                {
                    Log.Trace($"Domain type '{name}' refers to unknown base type with OID {baseTypeOID}, skipping", connector.Id);
                    return;
                }
                new PostgresDomainType(ns, name, oid, basePostgresType).AddTo(types);
                return;
            case 'p':   // pseudo-type (record, void)
                // Hack this as a base type
                goto case 'b';
            default:
                throw new ArgumentOutOfRangeException($"Unknown typtype for type '{name}' in pg_type: {typeChar}");
            }
        }

        #endregion

        #region Enum

        internal void MapEnum<TEnum>([CanBeNull] string pgName, [CanBeNull] IEDBNameTranslator nameTranslator) where TEnum : struct
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<TEnum>(nameTranslator);
            var backendType = GetBackendTypeByName(pgName);

            var asEnumType = backendType as PostgresEnumType;
            if (asEnumType == null)
                throw new EDBException($"A PostgreSQL type with the name {pgName} was found in the database but it isn't an enum");

            asEnumType.Activate(this, new EnumHandler<TEnum>(backendType, nameTranslator));
        }

        internal static void MapEnumGlobally<TEnum>([CanBeNull] string pgName, [CanBeNull] IEDBNameTranslator nameTranslator) where TEnum : struct
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<TEnum>(nameTranslator);

            _globalMappingChangeCounter++;
            _globalEnumMappings[pgName] = new EnumHandler<TEnum>.Factory(nameTranslator);
        }

        internal static void UnmapEnumGlobally<TEnum>([CanBeNull] string pgName, [CanBeNull] IEDBNameTranslator nameTranslator) where TEnum : struct
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<TEnum>(nameTranslator);

            _globalMappingChangeCounter++;
#pragma warning disable 168
            _globalEnumMappings.TryRemove(pgName, out var _);
#pragma warning restore 168
        }

        #endregion

        #region Composite

        internal void MapComposite<T>([CanBeNull] string pgName, [CanBeNull] IEDBNameTranslator nameTranslator) where T : new()
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<T>(nameTranslator);

            // TODO: Check if already mapped dude

            var compositeType = GetCompositeType(pgName);
            compositeType.Activate(this, new CompositeHandler<T>(compositeType, nameTranslator, compositeType.RawFields, this));
        }

        internal static void MapCompositeGlobally<T>([CanBeNull] string pgName, [CanBeNull] IEDBNameTranslator nameTranslator) where T : new()
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<T>(nameTranslator);

            _globalMappingChangeCounter++;
            _globalCompositeMappings[pgName] = new CompositeHandler<T>.Factory(nameTranslator);
        }

        internal static void UnmapCompositeGlobally<T>([CanBeNull] string pgName, [CanBeNull] IEDBNameTranslator nameTranslator) where T : new()
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<T>(nameTranslator);

            _globalMappingChangeCounter++;
#pragma warning disable 168
            _globalCompositeMappings.TryRemove(pgName, out var _);
#pragma warning restore 168
        }

        static string GenerateLoadCompositeQuery(bool withSchema) =>
$@"SELECT ns.nspname, typ.oid, typ.typtype
FROM pg_type AS typ
JOIN pg_namespace AS ns ON (ns.oid = typ.typnamespace)
WHERE (typ.typname = @name{(withSchema ? " AND ns.nspname = @schema" : "")});

SELECT att.attname, att.atttypid
FROM pg_type AS typ
JOIN pg_namespace AS ns ON (ns.oid = typ.typnamespace)
JOIN pg_attribute AS att ON (att.attrelid = typ.typrelid)
WHERE
  typ.typname = @name{(withSchema ? " AND ns.nspname = @schema" : "")} AND
  attnum > 0 AND     /* Don't load system attributes */
  NOT attisdropped;

SELECT ns.nspname, a.typname, a.oid
FROM pg_type AS a
JOIN pg_type AS b ON (b.oid = a.typelem)
JOIN pg_namespace AS ns ON (ns.oid = b.typnamespace)
WHERE a.typtype = 'b' AND b.typname = @name{(withSchema ? " AND ns.nspname = @schema" : "")}";

        PostgresCompositeType GetCompositeType(string pgName)
        {
            // First check if the composite type definition has already been loaded from the database
            if (pgName.IndexOf('.') == -1
                ? PostgresTypes.ByName.TryGetValue(pgName, out var postgresType)
                : PostgresTypes.ByFullName.TryGetValue(pgName, out postgresType))
            {
                var asComposite = postgresType as PostgresCompositeType;
                if (asComposite == null)
                    throw new EDBException($"Type {pgName} was found but is not a composite");
                return asComposite;
            }

            // This is the first time the composite is mapped, the type definition needs to be loaded
            string name, schema;
            var i = pgName.IndexOf('.');
            if (i == -1)
            {
                schema = null;
                name = pgName;
            }
            else
            {
                schema = pgName.Substring(0, i);
                name = pgName.Substring(i + 1);
            }

            using (var cmd = new EDBCommand(GenerateLoadCompositeQuery(schema != null), Connector.Connection))
            {
                cmd.Parameters.AddWithValue("name", name);
                if (schema != null)
                    cmd.Parameters.AddWithValue("schema", schema);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception($"An PostgreSQL type with the name {pgName} was not found in the database");

                    // Load some info on the composite type itself, do some checks
                    var ns = reader.GetString(0);
                    Debug.Assert(schema == null || ns == schema);
                    var oid = reader.GetFieldValue<uint>(1);
                    var typeChar = reader.GetChar(2);
                    if (typeChar != 'c')
                        throw new EDBException($"Type {pgName} was found in the database but is not a composite");
                    if (reader.Read())
                    {
                        // More than one composite type matched, the user didn't specify a schema and the same name
                        // exists in more than one schema
                        Debug.Assert(schema == null);
                        var ns2 = reader.GetString(0);
                        throw new EDBException($"More than one composite types with name {name} where found (in schemas {ns} and {ns2}). Please qualify with a schema.");
                    }

                    reader.NextResult();  // Load the fields

                    var fields = new List<RawCompositeField>();
                    while (reader.Read())
                        fields.Add(new RawCompositeField { PgName = reader.GetString(0), TypeOID = reader.GetFieldValue<uint>(1) });

                    var compositeType = new PostgresCompositeType(ns, name, oid, fields);
                    compositeType.AddTo(PostgresTypes);

                    reader.NextResult();  // Load the array type

                    if (reader.Read())
                    {
                        var arrayNs = reader.GetString(0);
                        var arrayName = reader.GetString(1);
                        var arrayOID = reader.GetFieldValue<uint>(2);

                        new PostgresArrayType(arrayNs, arrayName, arrayOID, compositeType).AddTo(PostgresTypes);
                    } else
                        Log.Warn($"Could not find array type corresponding to composite {pgName}");

                    return compositeType;
                }
            }
        }

        #endregion

        #region Lookups

        /// <summary>
        /// Looks up a type handler by its PostgreSQL type's OID.
        /// </summary>
        /// <param name="oid">A PostgreSQL type OID</param>
        /// <returns>A type handler that can be used to encode and decode values.</returns>
        internal TypeHandler this[uint oid]
        {
            get => TryGetByOID(oid, out var result) ? result : UnrecognizedTypeHandler;
            set => ByOID[oid] = value;
        }

        internal bool TryGetByOID(uint oid, out TypeHandler handler)
        {
            if (ByOID.TryGetValue(oid, out handler))
                return true;
            if (!PostgresTypes.ByOID.TryGetValue(oid, out var postgresType))
                return false;

            handler = postgresType.Activate(this);
            return true;
        }

        internal TypeHandler this[EDBDbType EDBDbType, Type specificType = null]
        {
            get
            {
                if (specificType != null && (EDBDbType & EDBDbType.Enum) == 0 && (EDBDbType & EDBDbType.Composite) == 0)
                    throw new ArgumentException($"{nameof(specificType)} can only be used with {nameof(EDBDbType.Enum)} or {nameof(EDBDbType.Composite)}");

                if (ByEDBDbType.TryGetValue(EDBDbType, out var handler))
                    return handler;

                if (specificType != null)  // Enum/composite
                {
                    // Note that enums and composites are never lazily activated - they're activated at the
                    // moment of mapping (or at connection time when globally-mapped)
                    if ((EDBDbType & EDBDbType.Array) != 0)
                    {
                        // Already-activated array of enum/composite
                        if (ArrayHandlerByType != null && ArrayHandlerByType.TryGetValue(specificType, out handler))
                            return handler;
                    }

                    // For non-array enum/composite, simply delegate to type inference
                    return this[specificType];
                }

                // Couldn't find already activated type, attempt to activate

                if (EDBDbType == EDBDbType.Enum || EDBDbType == EDBDbType.Composite)
                    throw new InvalidCastException($"When specifying EDBDbType.{nameof(EDBDbType.Enum)}, {nameof(EDBParameter.SpecificType)} must be specified as well");

                // Base, range or array of base/range
                if (PostgresTypes.ByEDBDbType.TryGetValue(EDBDbType, out var postgresType))
                    return postgresType.Activate(this);

                // We don't have a backend type for this EDBDbType. This could be because it's not yet supported by
                // EnterpriseDB.EDBClient, or that the type is missing in the database (old PG, missing extension...)
                if (!HandlerTypesByNpsgqlDbType.TryGetValue(EDBDbType, out var typeAndMapping))
                    throw new NotSupportedException("This EDBDbType isn't supported in EnterpriseDB.EDBClient yet: " + EDBDbType);
                throw new EDBException($"The PostgreSQL type '{typeAndMapping.Mapping.PgName}', mapped to EDBDbType '{EDBDbType}' isn't present in your database. " +
                                           "You may need to install an extension or upgrade to a newer version.");
            }
        }

        internal TypeHandler this[DbType dbType]
        {
            get
            {
                if (ByDbType.TryGetValue(dbType, out var handler))
                    return handler;
                if (PostgresTypes.ByDbType.TryGetValue(dbType, out var postgresType))
                    return postgresType.Activate(this);
                throw new NotSupportedException("This DbType is not supported in EnterpriseDB.EDBClient: " + dbType);
            }
        }

        internal TypeHandler this[object value]
        {
            get
            {
                Debug.Assert(value != null);

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

#pragma warning disable CA1043
        internal TypeHandler this[Type type]
#pragma warning restore CA1043
        {
            get
            {
                if (ByType.TryGetValue(type, out var handler))
                    return handler;

                // Try to find the backend type by a simple lookup on the given CLR type, this will handle base types.
                if (PostgresTypes.ByClrType.TryGetValue(type, out var postgresType))
                    return postgresType.Activate(this);

                // Try to see if it is an array type
                var arrayElementType = GetArrayElementType(type);
                if (arrayElementType != null)
                {
                    if (ByType.TryGetValue(arrayElementType, out var elementHandler) &&
                        elementHandler.PostgresType.EDBDbType.HasValue &&
                        ByEDBDbType.TryGetValue(EDBDbType.Array | elementHandler.PostgresType.EDBDbType.Value, out handler))
                    {
                        return handler;
                    }

                    // Enum and composite types go through the special _arrayHandlerByType
                    if (ArrayHandlerByType != null && ArrayHandlerByType.TryGetValue(arrayElementType, out handler))
                        return handler;

                    // Unactivated array

                    // Special check for byte[] - bytea not array of int2
                    if (type == typeof(byte[]))
                    {
                        if (!PostgresTypes.ByClrType.TryGetValue(typeof(byte[]), out var byteaPostgresType))
                            throw new EDBException("The PostgreSQL 'bytea' type is missing");
                        return byteaPostgresType.Activate(this);
                    }

                    // Get the elements backend type and activate its array backend type
                    if (!PostgresTypes.ByClrType.TryGetValue(arrayElementType, out var elementPostgresType))
                    {
                        if (arrayElementType.GetTypeInfo().IsEnum)
                            throw new NotSupportedException($"The CLR enum type {arrayElementType.Name} must be mapped with EnterpriseDB.EDBClient before usage, please refer to the documentation.");
                        throw new NotSupportedException($"The CLR type {arrayElementType} isn't supported by EnterpriseDB.EDBClient or your PostgreSQL. " +
                                                         "If you wish to map it to a PostgreSQL composite type you need to register it before usage, please refer to the documentation.");
                    }

                    if (elementPostgresType == null)
                        throw new NotSupportedException($"The PostgreSQL {arrayElementType.Name} does not have an array type in the database");

                    return elementPostgresType.Array.Activate(this);
                }

                // Range type which hasn't yet been set up
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(EDBRange<>))
                {
                    if (!PostgresTypes.ByClrType.TryGetValue(type.GetGenericArguments()[0], out var subtypePostgresType) ||
                        subtypePostgresType.Range == null)
                    {
                        throw new EDBException($"The .NET range type {type.Name} isn't supported in your PostgreSQL, use CREATE TYPE AS RANGE");
                    }

                    return subtypePostgresType.Range.Activate(this);
                }

                // Nothing worked
                if (type.GetTypeInfo().IsEnum)
                    throw new NotSupportedException($"The CLR enum type {type.Name} must be registered with EnterpriseDB.EDBClient before usage, please refer to the documentation.");

                if (typeof(IEnumerable).IsAssignableFrom(type))
                    throw new NotSupportedException("EnterpriseDB.EDBClient 3.x removed support for writing a parameter with an IEnumerable value, use .ToList()/.ToArray() instead");

                throw new NotSupportedException($"The CLR type {type} isn't supported by EnterpriseDB.EDBClient or your PostgreSQL. " +
                                                 "If you wish to map it to a PostgreSQL composite type you need to register it before usage, please refer to the documentation.");
            }
        }

        [CanBeNull]
        static Type GetArrayElementType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
                return type.GetElementType();

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            if (ilist != null)
                return ilist.GetGenericArguments()[0];

            if (typeof(IList).IsAssignableFrom(type))
                throw new NotSupportedException("Non-generic IList is a supported parameter, but the EDBDbType parameter must be set on the parameter");

            return null;
        }

        internal static EDBDbType ToEDBDbType(DbType dbType)
        {
            if (!DbTypeToEDBDbType.TryGetValue(dbType, out var EDBDbType))
                throw new NotSupportedException($"The parameter type DbType.{dbType} isn't supported by PostgreSQL or EnterpriseDB.EDBClient");
            return EDBDbType;
        }

        internal static EDBDbType ToEDBDbType(object value)
        {
            if (value is DateTime)
            {
                return ((DateTime)value).Kind == DateTimeKind.Utc
                    ? EDBDbType.TimestampTZ
                    : EDBDbType.Timestamp;
            }

            if (value is EDBDateTime)
            {
                return ((EDBDateTime)value).Kind == DateTimeKind.Utc
                    ? EDBDbType.TimestampTZ
                    : EDBDbType.Timestamp;
            }

            return ToEDBDbType(value.GetType());
        }

        static EDBDbType ToEDBDbType(Type type)
        {
            if (TypeToEDBDbType.TryGetValue(type, out var EDBDbType))
                return EDBDbType;

            if (type.IsArray)
            {
                if (type == typeof(byte[]))
                    return EDBDbType.Bytea;
                return EDBDbType.Array | ToEDBDbType(type.GetElementType());
            }

            var typeInfo = type.GetTypeInfo();

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            if (ilist != null)
                return EDBDbType.Array | ToEDBDbType(ilist.GetGenericArguments()[0]);

            if (typeInfo.IsEnum)
                return EDBDbType.Enum;

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(EDBRange<>))
                return EDBDbType.Range | ToEDBDbType(type.GetGenericArguments()[0]);

            if (type == typeof(DBNull))
                return EDBDbType.Unknown;

            throw new NotSupportedException("Can't infer EDBDbType for type " + type);
        }

        internal static DbType ToDbType(Type type)
            => TypeToDbType.TryGetValue(type, out var dbType) ? dbType : DbType.Object;

        internal static DbType ToDbType(EDBDbType EDBDbType)
            => EDBDbTypeToDbType.TryGetValue(EDBDbType, out var dbType) ? dbType : DbType.Object;

        #endregion

        #region Backend types

        /// <summary>
        /// A structure holding information about all PostgreSQL types found in an actual database.
        /// Only contains <see cref="PostgresType"/> instances and not actual <see cref="TypeHandlers"/>, and is shared between
        /// all connections using the same connection string. Consulted when a type handler needs to be created.
        /// </summary>
        internal class AvailablePostgresTypes
        {
            internal Dictionary<uint, PostgresType> ByOID { get; } = new Dictionary<uint, PostgresType>();

#if !__MonoCS__
            /// <summary>
            /// Indexes backend types by their PostgreSQL name, including namespace (e.g. pg_catalog.int4).
            /// Only used for enums and composites.
            /// </summary>
#endif
            internal Dictionary<string, PostgresType> ByFullName { get; } = new Dictionary<string, PostgresType>();

#if !__MonoCS__
            /// <summary>
            /// Indexes backend types by their PostgreSQL name, not including namespace.
            /// If more than one type exists with the same name (i.e. in different namespaces) this
            /// table will contain an entry with a null value.
            /// Only used for enums and composites.
            /// </summary>
#endif
            internal Dictionary<string, PostgresType> ByName { get; } = new Dictionary<string, PostgresType>();
            internal Dictionary<EDBDbType, PostgresType> ByEDBDbType { get; } = new Dictionary<EDBDbType, PostgresType>();
            internal Dictionary<DbType, PostgresType> ByDbType { get; } = new Dictionary<DbType, PostgresType>();
            internal Dictionary<Type, PostgresType> ByClrType { get; } = new Dictionary<Type, PostgresType>();
        }

        #endregion

        #region Type Handler Discovery

        static TypeHandlerRegistry()
        {
            _globalEnumMappings = new ConcurrentDictionary<string, IEnumHandlerFactory>();
            _globalCompositeMappings = new ConcurrentDictionary<string, ICompositeHandlerFactory>();

            HandlerTypes = new Dictionary<string, TypeAndMapping>();
            HandlerTypesByNpsgqlDbType = new Dictionary<EDBDbType, TypeAndMapping>();
            EDBDbTypeToDbType = new Dictionary<EDBDbType, DbType>();
            DbTypeToEDBDbType = new Dictionary<DbType, EDBDbType>();
            TypeToEDBDbType = new Dictionary<Type, EDBDbType>();
            TypeToDbType = new Dictionary<Type, DbType>();

            foreach (var t in typeof(TypeHandlerRegistry).GetTypeInfo().Assembly.GetTypes().Where(t => t.GetTypeInfo().IsSubclassOf(typeof(TypeHandler))))
            {
                var mappings = t.GetTypeInfo().GetCustomAttributes(typeof(TypeMappingAttribute), false);
                if (!mappings.Any())
                    continue;

                foreach (TypeMappingAttribute m in mappings)
                {
                    Debug.Assert(!HandlerTypes.ContainsKey(m.PgName), "Two type handlers registered on same PostgreSQL type name: " + m.PgName);
                    var typeAndMapping = new TypeAndMapping { HandlerType = t, Mapping = m };
                    HandlerTypes[m.PgName] = typeAndMapping;

                    if (m.EDBDbType.HasValue)
                    {
                        Debug.Assert(!HandlerTypesByNpsgqlDbType.ContainsKey(m.EDBDbType.Value), "Two type handlers registered on same EDBDbType: " + m.EDBDbType);
                        HandlerTypesByNpsgqlDbType[m.EDBDbType.Value] = typeAndMapping;
                    }

                    if (!m.EDBDbType.HasValue)
                        continue;

                    var EDBDbType = m.EDBDbType.Value;
                    var inferredDbType = m.InferredDbType;

                    if (inferredDbType != null)
                        EDBDbTypeToDbType[EDBDbType] = inferredDbType.Value;
                    foreach (var dbType in m.DbTypes)
                        DbTypeToEDBDbType[dbType] = EDBDbType;
                    foreach (var type in m.ClrTypes)
                    {
                        TypeToEDBDbType[type] = EDBDbType;
                        if (inferredDbType != null)
                            TypeToDbType[type] = inferredDbType.Value;
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
        internal static void ClearBackendTypeCache()
        {
            BackendTypeCache.Clear();
        }

        /// <summary>
        /// Clears the internal type cache.
        /// Useful for forcing a reload of the types after loading an extension.
        /// </summary>
#pragma warning disable 168
        internal static void ClearBackendTypeCache(string connectionString)
            => BackendTypeCache.TryRemove(connectionString, out var _);
#pragma warning restore 168

        static string GetPgName<T>(IEDBNameTranslator nameTranslator)
        {
            var attr = typeof(T).GetTypeInfo().GetCustomAttribute<PgNameAttribute>();
            return attr == null
                ? nameTranslator.TranslateTypeName(typeof(T).Name)
                : attr.PgName;
        }

        PostgresType GetBackendTypeByName(string pgName)
        {
            PostgresType postgresType;
            var i = pgName.IndexOf('.');
            if (i == -1)
            {
                // No dot, this is a partial type name
                if (!PostgresTypes.ByName.TryGetValue(pgName, out postgresType))
                    throw new EDBException($"A PostgreSQL type with the name {pgName} was not found in the database");
                if (postgresType == null)
                    throw new EDBException($"More than one PostgreSQL type was found with the name {pgName}, please specify a full name including schema");
                return postgresType;
            }

            // Full type name with namespace
            if (!PostgresTypes.ByFullName.TryGetValue(pgName, out postgresType))
                throw new Exception($"A PostgreSQL type with the name {pgName} was not found in the database");
            return postgresType;
        }

        #endregion
    }

    struct TypeAndMapping
    {
        internal Type HandlerType;
        internal TypeMappingAttribute Mapping;
    }
}
