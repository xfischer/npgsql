using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.Internal.TypeMapping;
using EnterpriseDB.EDBClient.NameTranslation;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    sealed class GlobalTypeMapper : TypeMapperBase
    {
        public static GlobalTypeMapper Instance { get; }

        internal List<TypeHandlerResolverFactory> ResolverFactories { get; } = new();
        public ConcurrentDictionary<string, IUserTypeMapping> UserTypeMappings { get; } = new();

        readonly ConcurrentDictionary<Type, TypeMappingInfo> _mappingsByClrType = new();

        /// <summary>
        /// A counter that is incremented whenever a global mapping change occurs.
        /// Used to invalidate bound type mappers.
        /// </summary>
        internal int ChangeCounter => _changeCounter;

        internal ReaderWriterLockSlim Lock { get; }
            = new(LockRecursionPolicy.SupportsRecursion);

        int _changeCounter;

        static GlobalTypeMapper()
            => Instance = new GlobalTypeMapper();

        GlobalTypeMapper() : base(new EDBSnakeCaseNameTranslator())
            => Reset();

        #region Mapping management

        public override IEDBTypeMapper MapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(typeof(TEnum), nameTranslator);

            Lock.EnterWriteLock();
            try
            {
                UserTypeMappings[pgName] = new UserEnumTypeMapping<TEnum>(pgName, nameTranslator);
                RecordChange();
                return this;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override bool UnmapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(typeof(TEnum), nameTranslator);

            Lock.EnterWriteLock();
            try
            {
                if (UserTypeMappings.TryRemove(pgName, out _))
                {
                    RecordChange();
                    return true;
                }

                return false;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override IEDBTypeMapper MapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(typeof(T), nameTranslator);

            Lock.EnterWriteLock();
            try
            {
                UserTypeMappings[pgName] = new UserCompositeTypeMapping<T>(pgName, nameTranslator);
                RecordChange();
                return this;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override IEDBTypeMapper MapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(clrType, nameTranslator);

            Lock.EnterWriteLock();
            try
            {
                UserTypeMappings[pgName] =
                    (IUserTypeMapping)Activator.CreateInstance(typeof(UserCompositeTypeMapping<>).MakeGenericType(clrType),
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                        new object[] { clrType, nameTranslator }, null)!;

                RecordChange();

                return this;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override bool UnmapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            => UnmapComposite(typeof(T), pgName, nameTranslator);

        public override bool UnmapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(clrType, nameTranslator);

            Lock.EnterWriteLock();
            try
            {
                if (UserTypeMappings.TryRemove(pgName, out _))
                {
                    RecordChange();
                    return true;
                }

                return false;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public override void AddTypeResolverFactory(TypeHandlerResolverFactory resolverFactory)
        {
            Lock.EnterWriteLock();
            try
            {
                // Since EFCore.PG plugins (and possibly other users) repeatedly call EDBConnection.GlobalTypeMapped.UseNodaTime,
                // we replace an existing resolver of the same CLR type.
                var type = resolverFactory.GetType();

                if (ResolverFactories[0].GetType() == type)
                    ResolverFactories[0] = resolverFactory;
                else
                {
                    for (var i = 0; i < ResolverFactories.Count; i++)
                        if (ResolverFactories[i].GetType() == type)
                            ResolverFactories.RemoveAt(i);

                    ResolverFactories.Insert(0, resolverFactory);
                }

                RecordChange();
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
                ResolverFactories.Clear();
                ResolverFactories.Add(new BuiltInTypeHandlerResolverFactory());

                UserTypeMappings.Clear();

                RecordChange();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        internal void RecordChange()
        {
            _mappingsByClrType.Clear();
            Interlocked.Increment(ref _changeCounter);
        }

        #endregion Mapping management

        #region EDBDbType/DbType inference for EDBParameter

        [RequiresUnreferencedCodeAttribute("ToEDBDbType uses interface-based reflection and isn't trimming-safe")]
        internal bool TryResolveMappingByValue(object value, [NotNullWhen(true)] out TypeMappingInfo? typeMapping)
        {
            Lock.EnterReadLock();
            try
            {
                // We resolve as follows:
                // 1. Cached by-type lookup (fast path). This will work for almost all types after the very first resolution.
                // 2. Value-dependent type lookup (e.g. DateTime by Kind) via the resolvers. This includes complex types (e.g. array/range
                //    over DateTime), and the results cannot be cached.
                // 3. Uncached by-type lookup (for the very first resolution of a given type)

                var type = value.GetType();
                if (_mappingsByClrType.TryGetValue(type, out typeMapping))
                    return true;

                foreach (var resolverFactory in ResolverFactories)
                    if ((typeMapping = resolverFactory.GetMappingByValueDependentValue(value)) is not null)
                        return true;

                return TryResolveMappingByClrType(value.GetType(), out typeMapping);
            }
            finally
            {
                Lock.ExitReadLock();
            }

            bool TryResolveMappingByClrType(Type clrType, [NotNullWhen(true)] out TypeMappingInfo? typeMapping)
            {
                if (_mappingsByClrType.TryGetValue(clrType, out typeMapping))
                    return true;

                foreach (var resolverFactory in ResolverFactories)
                {
                    if ((typeMapping = resolverFactory.GetMappingByClrType(clrType)) is not null)
                    {
                        _mappingsByClrType[clrType] = typeMapping;
                        return true;
                    }
                }

                if (clrType.IsArray)
                {
                    if (TryResolveMappingByClrType(clrType.GetElementType()!, out var elementMapping))
                    {
                        _mappingsByClrType[clrType] = typeMapping = new(
                            EDBDbType.Array | elementMapping.EDBDbType,
                            elementMapping.DataTypeName + "[]");
                        return true;
                    }

                    typeMapping = null;
                    return false;
                }

                var typeInfo = clrType.GetTypeInfo();

                var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x =>
                    x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
                if (ilist != null)
                {
                    if (TryResolveMappingByClrType(ilist.GetGenericArguments()[0], out var elementMapping))
                    {
                        _mappingsByClrType[clrType] = typeMapping = new(
                            EDBDbType.Array | elementMapping.EDBDbType,
                            elementMapping.DataTypeName + "[]");
                        return true;
                    }

                    typeMapping = null;
                    return false;
                }

                if (typeInfo.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(EDBRange<>))
                {
                    if (TryResolveMappingByClrType(clrType.GetGenericArguments()[0], out var elementMapping))
                    {
                        _mappingsByClrType[clrType] = typeMapping = new(
                            EDBDbType.Range | elementMapping.EDBDbType,
                            dataTypeName: null);
                        return true;
                    }

                    typeMapping = null;
                    return false;
                }

                typeMapping = null;
                return false;
            }
        }

        #endregion EDBDbType/DbType inference for EDBParameter

        #region Static translation tables

        public static string? EDBDbTypeToDataTypeName(EDBDbType npgsqlDbType)
            => npgsqlDbType switch
            {
                // Numeric types
                EDBDbType.Smallint => "smallint",
                EDBDbType.Integer  => "integer",
                EDBDbType.Bigint   => "bigint",
                EDBDbType.Real     => "real",
                EDBDbType.Double   => "double precision",
                EDBDbType.Numeric  => "numeric",
                EDBDbType.Money    => "money",

                // Text types
                EDBDbType.Text      => "text",
                EDBDbType.Xml       => "xml",
                EDBDbType.Varchar   => "character varying",
                EDBDbType.Char      => "character",
                EDBDbType.Name      => "name",
                EDBDbType.Refcursor => "refcursor",
                EDBDbType.Citext    => "citext",
                EDBDbType.Jsonb     => "jsonb",
                EDBDbType.Json      => "json",
                EDBDbType.JsonPath  => "jsonpath",

                // Date/time types
                EDBDbType.Timestamp   => "timestamp without time zone",
                EDBDbType.TimestampTz => "timestamp with time zone",
                EDBDbType.Date        => "date",
                EDBDbType.Time        => "time without time zone",
                EDBDbType.TimeTz      => "time with time zone",
                EDBDbType.Interval    => "interval",

                // Network types
                EDBDbType.Cidr     => "cidr",
                EDBDbType.Inet     => "inet",
                EDBDbType.MacAddr  => "macaddr",
                EDBDbType.MacAddr8 => "macaddr8",

                // Full-text search types
                EDBDbType.TsQuery   => "tsquery",
                EDBDbType.TsVector  => "tsvector",

                // Geometry types
                EDBDbType.Box     => "box",
                EDBDbType.Circle  => "circle",
                EDBDbType.Line    => "line",
                EDBDbType.LSeg    => "lseg",
                EDBDbType.Path    => "path",
                EDBDbType.Point   => "point",
                EDBDbType.Polygon => "polygon",

                // LTree types
                EDBDbType.LQuery    => "lquery",
                EDBDbType.LTree     => "ltree",
                EDBDbType.LTxtQuery => "ltxtquery",

                // UInt types
                EDBDbType.Oid       => "oid",
                EDBDbType.Xid       => "xid",
                EDBDbType.Xid8      => "xid8",
                EDBDbType.Cid       => "cid",
                EDBDbType.Regtype   => "regtype",
                EDBDbType.Regconfig => "regconfig",

                // Misc types
                EDBDbType.Boolean => "bool",
                EDBDbType.Bytea   => "bytea",
                EDBDbType.Uuid    => "uuid",
                EDBDbType.Varbit  => "bit varying",
                EDBDbType.Bit     => "bit",
                EDBDbType.Hstore  => "hstore",

                EDBDbType.Geometry  => "geometry",
                EDBDbType.Geography => "geography",

                // Built-in range types
                EDBDbType.IntegerRange     => "int4range",
                EDBDbType.BigIntRange      => "int8range",
                EDBDbType.NumericRange     => "numrange",
                EDBDbType.TimestampRange   => "tsrange",
                EDBDbType.TimestampTzRange => "tstzrange",
                EDBDbType.DateRange        => "daterange",

                // Built-in multirange types
                EDBDbType.IntegerMultirange     => "int4multirange",
                EDBDbType.BigIntMultirange      => "int8multirange",
                EDBDbType.NumericMultirange     => "nummultirange",
                EDBDbType.TimestampMultirange   => "tsmultirange",
                EDBDbType.TimestampTzMultirange => "tstzmultirange",
                EDBDbType.DateMultirange        => "datemultirange",

                // Internal types
                EDBDbType.Int2Vector   => "int2vector",
                EDBDbType.Oidvector    => "oidvector",
                EDBDbType.PgLsn        => "pg_lsn",
                EDBDbType.Tid          => "tid",
                EDBDbType.InternalChar => "char",

                // Special types
                EDBDbType.Unknown => "unknown",

                _ => npgsqlDbType.HasFlag(EDBDbType.Array)
                    ? EDBDbTypeToDataTypeName(npgsqlDbType & ~EDBDbType.Array) + "[]"
                    : null // e.g. ranges
            };

        internal static EDBDbType? DbTypeToEDBDbType(DbType dbType)
            => dbType switch
            {
                DbType.AnsiString            => EDBDbType.Text,
                DbType.Binary                => EDBDbType.Bytea,
                DbType.Byte                  => EDBDbType.Smallint,
                DbType.Boolean               => EDBDbType.Boolean,
                DbType.Currency              => EDBDbType.Money,
                DbType.Date                  => EDBDbType.Date,
                DbType.DateTime              => LegacyTimestampBehavior ? EDBDbType.Timestamp : EDBDbType.TimestampTz,
                DbType.Decimal               => EDBDbType.Numeric,
                DbType.VarNumeric            => EDBDbType.Numeric,
                DbType.Double                => EDBDbType.Double,
                DbType.Guid                  => EDBDbType.Uuid,
                DbType.Int16                 => EDBDbType.Smallint,
                DbType.Int32                 => EDBDbType.Integer,
                DbType.Int64                 => EDBDbType.Bigint,
                DbType.Single                => EDBDbType.Real,
                DbType.String                => EDBDbType.Text,
                DbType.Time                  => EDBDbType.Time,
                DbType.AnsiStringFixedLength => EDBDbType.Text,
                DbType.StringFixedLength     => EDBDbType.Text,
                DbType.Xml                   => EDBDbType.Xml,
                DbType.DateTime2             => EDBDbType.Timestamp,
                DbType.DateTimeOffset        => EDBDbType.TimestampTz,

                DbType.Object                => null,
                DbType.SByte                 => null,
                DbType.UInt16                => null,
                DbType.UInt32                => null,
                DbType.UInt64                => null,

                _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null)
            };

        internal static DbType EDBDbTypeToDbType(EDBDbType npgsqlDbType)
            => npgsqlDbType switch
            {
                // Numeric types
                EDBDbType.Smallint    => DbType.Int16,
                EDBDbType.Integer     => DbType.Int32,
                EDBDbType.Bigint      => DbType.Int64,
                EDBDbType.Real        => DbType.Single,
                EDBDbType.Double      => DbType.Double,
                EDBDbType.Numeric     => DbType.Decimal,
                EDBDbType.Money       => DbType.Currency,

                // Text types
                EDBDbType.Text        => DbType.String,
                EDBDbType.Xml         => DbType.Xml,
                EDBDbType.Varchar     => DbType.String,
                EDBDbType.Char        => DbType.String,
                EDBDbType.Name        => DbType.String,
                EDBDbType.Refcursor   => DbType.String,
                EDBDbType.Citext      => DbType.String,
                EDBDbType.Jsonb       => DbType.Object,
                EDBDbType.Json        => DbType.Object,
                EDBDbType.JsonPath    => DbType.String,

                // Date/time types
                EDBDbType.Timestamp   => LegacyTimestampBehavior ? DbType.DateTime : DbType.DateTime2,
                EDBDbType.TimestampTz => LegacyTimestampBehavior ? DbType.DateTimeOffset : DbType.DateTime,
                EDBDbType.Date        => DbType.Date,
                EDBDbType.Time        => DbType.Time,

                // Misc data types
                EDBDbType.Bytea       => DbType.Binary,
                EDBDbType.Boolean     => DbType.Boolean,
                EDBDbType.Uuid        => DbType.Guid,

                EDBDbType.Unknown     => DbType.Object,

                _ => DbType.Object
            };

        #endregion Static translation tables
    }
}
