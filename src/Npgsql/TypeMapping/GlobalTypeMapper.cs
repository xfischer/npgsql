using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using EnterpriseDB.EDBClient.NameTranslation;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    class GlobalTypeMapper : TypeMapperBase
    {
        public static GlobalTypeMapper Instance { get; }

        /// <summary>
        /// A counter that is incremented whenever a global mapping change occurs.
        /// Used to invalidate bound type mappers.
        /// </summary>
        internal int ChangeCounter => _changeCounter;

        internal ReaderWriterLockSlim Lock { get; }
            = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        int _changeCounter;

        static GlobalTypeMapper()
        {
            var instance = new GlobalTypeMapper();
            instance.SetupGlobalTypeMapper();
            Instance = instance;
        }

        internal GlobalTypeMapper() : base(new EDBSnakeCaseNameTranslator()) {}

        #region Mapping management

        public override IEDBTypeMapper AddMapping(EDBTypeMapping mapping)
        {
            Lock.EnterWriteLock();
            try
            {
                base.AddMapping(mapping);
                RecordChange();

                if (mapping.EDBDbType.HasValue)
                {
                    foreach (var dbType in mapping.DbTypes)
                        _dbTypeToEDBDbType[dbType] = mapping.EDBDbType.Value;

                    if (mapping.InferredDbType.HasValue)
                        _EDBDbTypeToDbType[mapping.EDBDbType.Value] = mapping.InferredDbType.Value;

                    foreach (var clrType in mapping.ClrTypes)
                        _typeToEDBDbType[clrType] = mapping.EDBDbType.Value;
                }

                if (mapping.InferredDbType.HasValue)
                    foreach (var clrType in mapping.ClrTypes)
                        _typeToDbType[clrType] = mapping.InferredDbType.Value;

                return this;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override bool RemoveMapping(string pgTypeName)
        {
            Lock.EnterWriteLock();
            try
            {
                var result = base.RemoveMapping(pgTypeName);
                RecordChange();
                return result;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override void Reset()
        {
            Lock.EnterWriteLock();
            try
            {
                Mappings.Clear();
                SetupGlobalTypeMapper();
                RecordChange();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        internal void RecordChange() => Interlocked.Increment(ref _changeCounter);

        #endregion Mapping management

        #region EDBDbType/DbType inference for EDBParameter

        readonly Dictionary<EDBDbType, DbType> _EDBDbTypeToDbType = new Dictionary<EDBDbType, DbType>();
        readonly Dictionary<DbType, EDBDbType> _dbTypeToEDBDbType = new Dictionary<DbType, EDBDbType>();
        readonly Dictionary<Type, EDBDbType> _typeToEDBDbType = new Dictionary<Type, EDBDbType>();
        readonly Dictionary<Type, DbType> _typeToDbType = new Dictionary<Type, DbType>();

        internal DbType ToDbType(EDBDbType EDBDbType)
            => _EDBDbTypeToDbType.TryGetValue(EDBDbType, out var dbType) ? dbType : DbType.Object;

        internal EDBDbType ToEDBDbType(DbType dbType)
        {
            if (!_dbTypeToEDBDbType.TryGetValue(dbType, out var EDBDbType))
                throw new NotSupportedException($"The parameter type DbType.{dbType} isn't supported by PostgreSQL or EnterpriseDB.EDBClient");
            return EDBDbType;
        }

        internal DbType ToDbType(Type type)
            => _typeToDbType.TryGetValue(type, out var dbType) ? dbType : DbType.Object;

        internal EDBDbType ToEDBDbType(Type type)
        {
            if (_typeToEDBDbType.TryGetValue(type, out var EDBDbType))
                return EDBDbType;

            if (type.IsArray)
            {
                if (type == typeof(byte[]))
                    return EDBDbType.Bytea;
                return EDBDbType.Array | ToEDBDbType(type.GetElementType()!);
            }

            var typeInfo = type.GetTypeInfo();

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            if (ilist != null)
                return EDBDbType.Array | ToEDBDbType(ilist.GetGenericArguments()[0]);

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(EDBRange<>))
                return EDBDbType.Range | ToEDBDbType(type.GetGenericArguments()[0]);

            if (type == typeof(DBNull))
                return EDBDbType.Unknown;

            throw new NotSupportedException("Can't infer EDBDbType for type " + type);
        }


        #endregion EDBDbType/DbType inference for EDBParameter

        #region Setup for built-in handlers

        void SetupGlobalTypeMapper()
        {
            // Look for TypeHandlerFactories with mappings in our assembly, set them up
            foreach (var t in typeof(TypeMapperBase).GetTypeInfo().Assembly.GetTypes().Where(t => typeof(EDBTypeHandlerFactory).IsAssignableFrom(t.GetTypeInfo())))
            {
                var mappingAttributes = t.GetTypeInfo().GetCustomAttributes(typeof(TypeMappingAttribute), false);
                if (!mappingAttributes.Any())
                    continue;

                var factory = (EDBTypeHandlerFactory)Activator.CreateInstance(t)!;

                foreach (TypeMappingAttribute m in mappingAttributes)
                {
                    // TODO: Duplication between TypeMappingAttribute and TypeMapping. Look at this later.
                    AddMapping(new EDBTypeMappingBuilder
                    {
                        PgTypeName = m.PgName,
                        EDBDbType = m.EDBDbType,
                        DbTypes = m.DbTypes,
                        ClrTypes = m.ClrTypes,
                        InferredDbType = m.InferredDbType,
                        TypeHandlerFactory = factory,
                    }.Build());
                }
            }

            // Look for EDBTypeHandler classes with mappings in our assembly, set them up with the DefaultTypeHandlerFactory.
            // This is a shortcut that allows us to not specify a factory for each and every type handler
            foreach (var t in typeof(TypeMapperBase).GetTypeInfo().Assembly.GetTypes().Where(t => t.GetTypeInfo().IsSubclassOf(typeof(EDBTypeHandler))))
            {
                var mappingAttributes = t.GetTypeInfo().GetCustomAttributes(typeof(TypeMappingAttribute), false);
                if (!mappingAttributes.Any())
                    continue;

                var factory = new DefaultTypeHandlerFactory(t);

                foreach (TypeMappingAttribute m in mappingAttributes)
                {
                    // TODO: Duplication between TypeMappingAttribute and TypeMapping. Look at this later.
                    AddMapping(new EDBTypeMappingBuilder
                    {
                        PgTypeName = m.PgName,
                        EDBDbType = m.EDBDbType,
                        DbTypes = m.DbTypes,
                        ClrTypes = m.ClrTypes,
                        InferredDbType = m.InferredDbType,
                        TypeHandlerFactory = factory
                    }.Build());
                }
            }

            // This is an extremely ugly hack to support ReadOnlyIPAddress, which as an internal subclass of IPAddress
            // added to .NET Core 3.0 (see https://github.com/dotnet/corefx/issues/33373)
            if (_typeToEDBDbType.ContainsKey(typeof(IPAddress)) &&
                Mappings.TryGetValue("inet", out var inetMapping) &&
                typeof(IPAddress).GetNestedType("ReadOnlyIPAddress", BindingFlags.NonPublic) is Type readOnlyIpType)
            {
                _typeToEDBDbType[readOnlyIpType] = _typeToEDBDbType[typeof(IPAddress)];
                var augmentedClrType = new Type[inetMapping.ClrTypes.Length + 1];
                Array.Copy(inetMapping.ClrTypes, augmentedClrType, inetMapping.ClrTypes.Length);
                augmentedClrType[augmentedClrType.Length - 1] = readOnlyIpType;
                Mappings["inet"] = new EDBTypeMappingBuilder
                {
                    PgTypeName = "inet",
                    EDBDbType = inetMapping.EDBDbType,
                    DbTypes = inetMapping.DbTypes,
                    ClrTypes = augmentedClrType,
                    InferredDbType = inetMapping.InferredDbType,
                    TypeHandlerFactory = inetMapping.TypeHandlerFactory
                }.Build();
            }
        }

        #endregion Setup for built-in handlers
    }
}
