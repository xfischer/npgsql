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

namespace EnterpriseDB.EDBClient.TypeMapping;

sealed class GlobalTypeMapper : IEDBTypeMapper
{
    public static GlobalTypeMapper Instance { get; }

    public IEDBNameTranslator DefaultNameTranslator { get; set; } = new EDBSnakeCaseNameTranslator();

    internal List<TypeHandlerResolverFactory> ResolverFactories { get; } = new();
    public ConcurrentDictionary<string, IUserTypeMapping> UserTypeMappings { get; } = new();

    readonly ConcurrentDictionary<Type, TypeMappingInfo> _mappingsByClrType = new();

    internal ReaderWriterLockSlim Lock { get; }
        = new(LockRecursionPolicy.SupportsRecursion);

    static GlobalTypeMapper()
        => Instance = new GlobalTypeMapper();

    GlobalTypeMapper()
        => Reset();

    #region Mapping management

    public IEDBTypeMapper MapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum
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

    public bool UnmapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum
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

    [RequiresUnreferencedCode("Composite type mapping currently isn't trimming-safe.")]
    public IEDBTypeMapper MapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
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

    [RequiresUnreferencedCode("Composite type mapping currently isn't trimming-safe.")]
    public IEDBTypeMapper MapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
    {
        var openMethod = typeof(GlobalTypeMapper).GetMethod(nameof(MapComposite), new[] { typeof(string), typeof(IEDBNameTranslator) })!;
        var method = openMethod.MakeGenericMethod(clrType);
        method.Invoke(this, new object?[] { pgName, nameTranslator });

        return this;
    }

    [RequiresUnreferencedCode("Composite type mapping currently isn't trimming-safe.")]
    public bool UnmapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        => UnmapComposite(typeof(T), pgName, nameTranslator);

    [RequiresUnreferencedCode("Composite type mapping currently isn't trimming-safe.")]
    public bool UnmapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
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

    public void AddTypeResolverFactory(TypeHandlerResolverFactory resolverFactory)
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

    public void Reset()
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
        => _mappingsByClrType.Clear();

    static string GetPgName(Type clrType, IEDBNameTranslator nameTranslator)
        => clrType.GetCustomAttribute<PgNameAttribute>()?.PgName
           ?? nameTranslator.TranslateTypeName(clrType.Name);

    #endregion Mapping management

    #region EDBDbType/DbType inference for EDBParameter

    [RequiresUnreferencedCode("ToEDBDbType uses interface-based reflection and isn't trimming-safe")]
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
            EDBDbType.Boolean => "boolean",
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

    public static EDBDbType DataTypeNameToEDBDbType(string typeName)
    {
        // Strip any facet information (length/precision/scale)
        var parenIndex = typeName.IndexOf('(');
        if (parenIndex > -1)
            typeName = typeName.Substring(0, parenIndex);

        return typeName switch
        {
            // Numeric types
            "smallint" => EDBDbType.Smallint,
            "integer" or "int" => EDBDbType.Integer,
            "bigint" => EDBDbType.Bigint,
            "real" => EDBDbType.Real,
            "double precision" => EDBDbType.Double,
            "numeric" => EDBDbType.Numeric,
            "money" => EDBDbType.Money,

            // Text types
            "text" => EDBDbType.Text,
            "xml" => EDBDbType.Xml,
            "character varying" or "varchar" => EDBDbType.Varchar,
            "character" => EDBDbType.Char,
            "name" => EDBDbType.Name,
            "refcursor" => EDBDbType.Refcursor,
            "citext" => EDBDbType.Citext,
            "jsonb" => EDBDbType.Jsonb,
            "json" => EDBDbType.Json,
            "jsonpath" => EDBDbType.JsonPath,

            // Date/time types
            "timestamp without time zone" or "timestamp" => EDBDbType.Timestamp,
            "timestamp with time zone" or "timestamptz" => EDBDbType.TimestampTz,
            "date" => EDBDbType.Date,
            "time without time zone" or "timetz" => EDBDbType.Time,
            "time with time zone" or "time" => EDBDbType.TimeTz,
            "interval" => EDBDbType.Interval,

            // Network types
            "cidr" => EDBDbType.Cidr,
            "inet" => EDBDbType.Inet,
            "macaddr" => EDBDbType.MacAddr,
            "macaddr8" => EDBDbType.MacAddr8,

            // Full-text search types
            "tsquery" => EDBDbType.TsQuery,
            "tsvector" => EDBDbType.TsVector,

            // Geometry types
            "box" => EDBDbType.Box,
            "circle" => EDBDbType.Circle,
            "line" => EDBDbType.Line,
            "lseg" => EDBDbType.LSeg,
            "path" => EDBDbType.Path,
            "point" => EDBDbType.Point,
            "polygon" => EDBDbType.Polygon,

            // LTree types
            "lquery" => EDBDbType.LQuery,
            "ltree" => EDBDbType.LTree,
            "ltxtquery" => EDBDbType.LTxtQuery,

            // UInt types
            "oid" => EDBDbType.Oid,
            "xid" => EDBDbType.Xid,
            "xid8" => EDBDbType.Xid8,
            "cid" => EDBDbType.Cid,
            "regtype" => EDBDbType.Regtype,
            "regconfig" => EDBDbType.Regconfig,

            // Misc types
            "boolean" or "bool" => EDBDbType.Boolean,
            "bytea" => EDBDbType.Bytea,
            "uuid" => EDBDbType.Uuid,
            "bit varying" or "varbit" => EDBDbType.Varbit,
            "bit" => EDBDbType.Bit,
            "hstore" => EDBDbType.Hstore,

            "geometry" => EDBDbType.Geometry,
            "geography" => EDBDbType.Geography,

            // Built-in range types
            "int4range" => EDBDbType.IntegerRange,
            "int8range" => EDBDbType.BigIntRange,
            "numrange" => EDBDbType.NumericRange,
            "tsrange" => EDBDbType.TimestampRange,
            "tstzrange" => EDBDbType.TimestampTzRange,
            "daterange" => EDBDbType.DateRange,

            // Built-in multirange types
            "int4multirange" => EDBDbType.IntegerMultirange,
            "int8multirange" => EDBDbType.BigIntMultirange,
            "nummultirange" => EDBDbType.NumericMultirange,
            "tsmultirange" => EDBDbType.TimestampMultirange,
            "tstzmultirange" => EDBDbType.TimestampTzMultirange,
            "datemultirange" => EDBDbType.DateMultirange,

            // Internal types
            "int2vector" => EDBDbType.Int2Vector,
            "oidvector" => EDBDbType.Oidvector,
            "pg_lsn" => EDBDbType.PgLsn,
            "tid" => EDBDbType.Tid,
            "char" => EDBDbType.InternalChar,

            _ => typeName.EndsWith("[]", StringComparison.Ordinal) &&
                 DataTypeNameToEDBDbType(typeName.Substring(0, typeName.Length - 2)) is { } elementEDBDbType &&
                 elementEDBDbType != EDBDbType.Unknown
                ? elementEDBDbType | EDBDbType.Array
                : EDBDbType.Unknown // e.g. ranges
        };
    }

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