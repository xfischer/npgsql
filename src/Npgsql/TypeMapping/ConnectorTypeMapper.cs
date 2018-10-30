#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.Logging;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandlers;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    class ConnectorTypeMapper : TypeMapperBase
    {
        /// <summary>
        /// The connector to which this type mapper belongs.
        /// </summary>
        [CanBeNull]
        readonly EDBConnector _connector;

        /// <summary>
        /// Type information for the database of this mapper. Null for the global mapper.
        /// </summary>
        internal EDBDatabaseInfo DatabaseInfo { get; set; }

        internal EDBTypeHandler UnrecognizedTypeHandler { get; }

        readonly Dictionary<uint, EDBTypeHandler> _byOID = new Dictionary<uint, EDBTypeHandler>();
        readonly Dictionary<EDBDbType, EDBTypeHandler> _byEDBDbType = new Dictionary<EDBDbType, EDBTypeHandler>();
        readonly Dictionary<DbType, EDBTypeHandler> _byDbType = new Dictionary<DbType, EDBTypeHandler>();
        readonly Dictionary<string, EDBTypeHandler> _byTypeName = new Dictionary<string, EDBTypeHandler>();

        /// <summary>
        /// Maps CLR types to their type handlers.
        /// </summary>
        readonly Dictionary<Type, EDBTypeHandler> _byClrType= new Dictionary<Type, EDBTypeHandler>();

        /// <summary>
        /// Maps CLR types to their array handlers.
        /// </summary>
        readonly Dictionary<Type, EDBTypeHandler> _arrayHandlerByClrType = new Dictionary<Type, EDBTypeHandler>();

        /// <summary>
        /// Copy of <see cref="GlobalTypeMapper.ChangeCounter"/> at the time when this
        /// mapper was created, to detect mapping changes. If changes are made to this connection's
        /// mapper, the change counter is set to -1.
        /// </summary>
        internal int ChangeCounter { get; private set; }

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        #region Construction

        internal ConnectorTypeMapper(EDBConnector connector)
        {
            _connector = connector;
            UnrecognizedTypeHandler = new UnknownTypeHandler(_connector.Connection);
            ClearBindings();
            ResetMappings();
            DefaultNameTranslator = GlobalTypeMapper.Instance.DefaultNameTranslator;
        }

        #endregion Constructors

        #region Type handler lookup

        /// <summary>
        /// Looks up a type handler by its PostgreSQL type's OID.
        /// </summary>
        /// <param name="oid">A PostgreSQL type OID</param>
        /// <returns>A type handler that can be used to encode and decode values.</returns>
        internal EDBTypeHandler GetByOID(uint oid)
            => TryGetByOID(oid, out var result) ? result : UnrecognizedTypeHandler;

        internal bool TryGetByOID(uint oid, out EDBTypeHandler handler)
            => _byOID.TryGetValue(oid, out handler);

        internal EDBTypeHandler GetByEDBDbType(EDBDbType EDBDbType)
            => _byEDBDbType.TryGetValue(EDBDbType, out var handler)
                ? handler
                : throw new EDBException($"The EDBDbType '{EDBDbType}' isn't present in your database. " +
                                             "You may need to install an extension or upgrade to a newer version.");


        internal EDBTypeHandler GetByDbType(DbType dbType)
            => _byDbType.TryGetValue(dbType, out var handler)
                ? handler
                : throw new NotSupportedException("This DbType is not supported in EnterpriseDB.EDBClient: " + dbType);

        internal EDBTypeHandler GetByDataTypeName(string typeName)
            => _byTypeName.TryGetValue(typeName, out var handler)
                ? handler
                : throw new NotSupportedException("Could not find PostgreSQL type " + typeName);

        internal EDBTypeHandler GetByClrType(Type type)
        {
            if (_byClrType.TryGetValue(type, out var handler))
                return handler;

            // Try to see if it is an array type
            var arrayElementType = GetArrayElementType(type);
            if (arrayElementType != null)
            {
                if (_arrayHandlerByClrType.TryGetValue(arrayElementType, out var elementHandler))
                    return elementHandler;
                throw new NotSupportedException($"The CLR array type {type} isn't supported by EnterpriseDB.EDBClient or your PostgreSQL. " +
                                                "If you wish to map it to an  PostgreSQL composite type array you need to register it before usage, please refer to the documentation.");
            }

            // Nothing worked
            if (type.GetTypeInfo().IsEnum)
                throw new NotSupportedException($"The CLR enum type {type.Name} must be registered with EnterpriseDB.EDBClient before usage, please refer to the documentation.");

            if (typeof(IEnumerable).IsAssignableFrom(type))
                throw new NotSupportedException("EnterpriseDB.EDBClient 3.x removed support for writing a parameter with an IEnumerable value, use .ToList()/.ToArray() instead");

            throw new NotSupportedException($"The CLR type {type} isn't natively supported by EnterpriseDB.EDBClient or your PostgreSQL. " +
                                            $"To use it with a PostgreSQL composite you need to specify {nameof(EDBParameter.DataTypeName)} or to map it, please refer to the documentation.");
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

        #endregion Type handler lookup

        #region Mapping management

        public override IEDBTypeMapper AddMapping(EDBTypeMapping mapping)
        {
            CheckReady();

            base.AddMapping(mapping);
            BindType(mapping, _connector, true);
            ChangeCounter = -1;
            return this;
        }

        public override bool RemoveMapping(string pgTypeName)
        {
            CheckReady();

            var removed = base.RemoveMapping(pgTypeName);
            if (!removed)
                return false;

            // Rebind everything. We redo rather than trying to update the
            // existing dictionaries because it's complex to remove arrays, ranges...
            ClearBindings();
            BindTypes();
            ChangeCounter = -1;
            return true;
        }

        void CheckReady()
        {
            if (_connector.State != ConnectorState.Ready)
                throw new InvalidOperationException("Connection must be open and idle to perform registration");
        }

        void ResetMappings()
        {
            var globalMapper = GlobalTypeMapper.Instance;
            globalMapper.Lock.EnterReadLock();
            try
            {
                Mappings = new Dictionary<string, EDBTypeMapping>(globalMapper.Mappings);
            }
            finally
            {
                globalMapper.Lock.ExitReadLock();
            }
            ChangeCounter = GlobalTypeMapper.Instance.ChangeCounter;
        }

        void ClearBindings()
        {
            _byOID.Clear();
            _byEDBDbType.Clear();
            _byDbType.Clear();
            _byClrType.Clear();
            _arrayHandlerByClrType.Clear();

            _byEDBDbType[EDBDbType.Unknown] = UnrecognizedTypeHandler;
            _byClrType[typeof(DBNull)] = UnrecognizedTypeHandler;
        }

        public override void Reset()
        {
            ClearBindings();
            ResetMappings();
            BindTypes();
        }

        #endregion Mapping management

        #region Binding

        internal void Bind(EDBDatabaseInfo databaseInfo)
        {
            DatabaseInfo = databaseInfo;
            BindTypes();
        }

        void BindTypes()
        {
            foreach (var mapping in Mappings.Values)
                BindType(mapping, _connector, false);

            // Enums
            var enumFactory = new UnmappedEnumTypeHandlerFactory(DefaultNameTranslator);
            foreach (var e in DatabaseInfo.EnumTypes.Where(e => !_byOID.ContainsKey(e.OID)))
                BindType(enumFactory.Create(e, _connector.Connection), e);

            // Wire up any domains we find to their base type mappings, this is important
            // for reading domain fields of composites
            foreach (var domain in DatabaseInfo.DomainTypes)
                if (_byOID.TryGetValue(domain.BaseType.OID, out var baseTypeHandler))
                {
                    _byOID[domain.OID] = baseTypeHandler;
                    if (domain.Array != null)
                        BindType(baseTypeHandler.CreateArrayHandler(domain.Array), domain.Array);
                }

            // Composites
            var dynamicCompositeFactory = new UnmappedCompositeTypeHandlerFactory(DefaultNameTranslator);
            foreach (var compType in DatabaseInfo.CompositeTypes.Where(e => !_byOID.ContainsKey(e.OID)))
                BindType(dynamicCompositeFactory.Create(compType, _connector.Connection), compType);
        }

        void BindType(EDBTypeMapping mapping, EDBConnector connector, bool throwOnError)
        {
            // Binding can occur at two different times:
            // 1. When a user adds a mapping for a specific connection (and exception should bubble up to them)
            // 2. When binding the global mappings, in which case we want to log rather than throw
            // (i.e. missing database type for some unused defined binding shouldn't fail the connection)

            var pgName = mapping.PgTypeName;
            var found = pgName.IndexOf('.') == -1
                ? DatabaseInfo.ByName.TryGetValue(pgName, out var pgType)  // No dot, partial type name
                : DatabaseInfo.ByFullName.TryGetValue(pgName, out pgType); // Full type name with namespace

            if (!found)
            {
                var msg = $"A PostgreSQL type with the name {mapping.PgTypeName} was not found in the database";
                if (throwOnError)
                    throw new ArgumentException(msg);
                Log.Debug(msg);
                return;
            }
            else if (pgType == null)
            {
                var msg = $"More than one PostgreSQL type was found with the name {mapping.PgTypeName}, please specify a full name including schema";
                if (throwOnError)
                    throw new ArgumentException(msg);
                Log.Debug(msg);
                return;
            }
            else if (pgType is PostgresDomainType)
            {
                var msg = "Cannot add a mapping to a PostgreSQL domain type";
                if (throwOnError)
                    throw new NotSupportedException(msg);
                Log.Debug(msg);
                return;
            }

            var handler = mapping.TypeHandlerFactory.Create(pgType, connector.Connection);
            BindType(handler, pgType, mapping.EDBDbType, mapping.DbTypes, mapping.ClrTypes);
        }

        void BindType(EDBTypeHandler handler, PostgresType pgType, EDBDbType? EDBDbType = null, DbType[] dbTypes = null, Type[] clrTypes = null)
        {
            _byOID[pgType.OID] = handler;
            _byTypeName[pgType.FullName] = handler;
            _byTypeName[pgType.Name] = handler;

            if (EDBDbType.HasValue)
            {
                var value = EDBDbType.Value;
                if (_byEDBDbType.ContainsKey(value))
                    throw new InvalidOperationException($"Two type handlers registered on same EDBDbType '{EDBDbType}': {_byEDBDbType[value].GetType().Name} and {handler.GetType().Name}");
                _byEDBDbType[EDBDbType.Value] = handler;
            }

            if (dbTypes != null)
            {
                foreach (var dbType in dbTypes)
                {
                    if (_byDbType.ContainsKey(dbType))
                        throw new InvalidOperationException($"Two type handlers registered on same DbType {dbType}: {_byDbType[dbType].GetType().Name} and {handler.GetType().Name}");
                    _byDbType[dbType] = handler;
                }
            }

            if (clrTypes != null)
            {
                foreach (var type in clrTypes)
                {
                    if (_byClrType.ContainsKey(type))
                        throw new InvalidOperationException($"Two type handlers registered on same .NET type '{type}': {_byClrType[type].GetType().Name} and {handler.GetType().Name}");
                    _byClrType[type] = handler;
                }
            }

            if (pgType.Array != null)
                BindArrayType(handler, pgType.Array, EDBDbType, clrTypes);

            if (pgType.Range != null)
                BindRangeType(handler, pgType.Range, EDBDbType, clrTypes);
        }

        void BindArrayType(EDBTypeHandler elementHandler, PostgresArrayType pgArrayType, EDBDbType? elementEDBDbType, Type[] elementClrTypes)
        {
            var arrayHandler = elementHandler.CreateArrayHandler(pgArrayType);

            var arrayEDBDbType = elementEDBDbType.HasValue
                ? EDBDbType.Array | elementEDBDbType.Value
                : (EDBDbType?)null;

            BindType(arrayHandler, pgArrayType, arrayEDBDbType);

            // Note that array handlers aren't registered in ByClrType like base types, because they handle all
            // dimension types and not just one CLR type (e.g. int[], int[,], int[,,]).
            // So the by-type lookup is special and goes via _arrayHandlerByClrType, see this[Type type]
            // TODO: register single-dimensional in _byType as a specific optimization? But do PSV as well...
            if (elementClrTypes != null)
            {
                foreach (var elementType in elementClrTypes)
                {
                    if (_arrayHandlerByClrType.ContainsKey(elementType))
                        throw new Exception(
                            $"Two array type handlers registered on same .NET type {elementType}: {_arrayHandlerByClrType[elementType].GetType().Name} and {arrayHandler.GetType().Name}");
                    _arrayHandlerByClrType[elementType] = arrayHandler;
                }
            }
        }

        void BindRangeType(EDBTypeHandler elementHandler, PostgresRangeType pgRangeType, EDBDbType? elementEDBDbType, Type[] elementClrTypes)
        {
            var rangeHandler = elementHandler.CreateRangeHandler(pgRangeType);

            var rangeEDBDbType = elementEDBDbType.HasValue
                ? EDBDbType.Range | elementEDBDbType.Value
                : (EDBDbType?)null;


            Type[] clrTypes = null;
            if (elementClrTypes != null)
            {
                // Somewhat hacky. Although the element may have more than one CLR mapping,
                // its range will only be mapped to the "main" one for now.
                var defaultElementType = elementHandler.GetFieldType();

                clrTypes = elementClrTypes.Contains(defaultElementType)
                    ? new[] { rangeHandler.GetFieldType() }
                    : null;
            }

            BindType(rangeHandler, pgRangeType, rangeEDBDbType, null, clrTypes);
        }

        #endregion Binding

        internal (EDBDbType? EDBDbType, PostgresType postgresType) GetTypeInfoByOid(uint oid)
        {
            if (!DatabaseInfo.ByOID.TryGetValue(oid, out var postgresType))
                throw new InvalidOperationException($"Couldn't find PostgreSQL type with OID {oid}");

            // Try to find the postgresType in the mappings
            if (TryGetMapping(postgresType, out var EDBTypeMapping))
                return (EDBTypeMapping.EDBDbType, postgresType);

            // Try to find the Elements' postgresType in the mappings
            if (postgresType is PostgresArrayType arrayType &&
                TryGetMapping(arrayType.Element, out var elementEDBTypeMapping))
            {
                return (elementEDBTypeMapping.EDBDbType | EDBDbType.Array, postgresType);
            }

            // It might be an unmapped enum/composite type, or some other unmapped type
            return (null, postgresType);

            bool TryGetMapping(PostgresType pgType, out EDBTypeMapping mapping)
                => (Mappings.TryGetValue(pgType.Name, out mapping) ||
                    Mappings.TryGetValue(pgType.FullName, out mapping));
        }
    }
}
