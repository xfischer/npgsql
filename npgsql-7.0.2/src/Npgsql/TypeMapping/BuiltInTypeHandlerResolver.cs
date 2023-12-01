using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text.Json;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.FullTextSearchHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.GeometricHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.InternalTypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.LTreeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.NetworkHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient.TypeMapping;

sealed class BuiltInTypeHandlerResolver : TypeHandlerResolver
{
    readonly EDBConnector _connector;
    readonly EDBDatabaseInfo _databaseInfo;

    static readonly Type ReadOnlyIPAddressType = IPAddress.Loopback.GetType();

    static readonly Dictionary<string, TypeMappingInfo> Mappings = new()
    {
        // Numeric types
        { "smallint",         new(EDBDbType.Smallint, "smallint",         typeof(short), typeof(byte), typeof(sbyte)) },
        { "integer",          new(EDBDbType.Integer,  "integer",          typeof(int)) },
        { "int",              new(EDBDbType.Integer,  "integer",          typeof(int)) },
        { "bigint",           new(EDBDbType.Bigint,   "bigint",           typeof(long)) },
        { "real",             new(EDBDbType.Real,     "real",             typeof(float)) },
        { "double precision", new(EDBDbType.Double,   "double precision", typeof(double)) },
        { "numeric",          new(EDBDbType.Numeric,  "numeric",          typeof(decimal), typeof(BigInteger)) },
        { "decimal",          new(EDBDbType.Numeric,  "numeric",          typeof(decimal), typeof(BigInteger)) },
        { "money",            new(EDBDbType.Money,    "money") },

        // Text types
        { "text",              new(EDBDbType.Text,      "text", typeof(string), typeof(char[]), typeof(char), typeof(ArraySegment<char>)) },
        { "xml",               new(EDBDbType.Xml,       "xml") },
        { "character varying", new(EDBDbType.Varchar,   "character varying") },
        { "varchar",           new(EDBDbType.Varchar,   "character varying") },
        { "character",         new(EDBDbType.Char,      "character") },
        { "name",              new(EDBDbType.Name,      "name") },
        { "refcursor",         new(EDBDbType.Refcursor, "refcursor") },
        { "citext",            new(EDBDbType.Citext,    "citext") },
        { "jsonb",             new(EDBDbType.Jsonb,     "jsonb", typeof(JsonDocument)) },
        { "json",              new(EDBDbType.Json,      "json") },
        { "jsonpath",          new(EDBDbType.JsonPath,  "jsonpath") },

        // Date/time types
        { "timestamp without time zone", new(EDBDbType.Timestamp,   "timestamp without time zone", typeof(DateTime)) },
        { "timestamp",                   new(EDBDbType.Timestamp,   "timestamp without time zone", typeof(DateTime)) },
        { "timestamp with time zone",    new(EDBDbType.TimestampTz, "timestamp with time zone",    typeof(DateTimeOffset)) },
        { "timestamptz",                 new(EDBDbType.TimestampTz, "timestamp with time zone",    typeof(DateTimeOffset)) },
        { "date",                        new(EDBDbType.Date,        "date"
#if NET6_0_OR_GREATER
            , typeof(DateOnly)
#endif
        ) },
        { "time without time zone",      new(EDBDbType.Time,        "time without time zone"
#if NET6_0_OR_GREATER
            , typeof(TimeOnly)
#endif
        ) },
        { "time",                        new(EDBDbType.Time,        "time without time zone"
#if NET6_0_OR_GREATER
            , typeof(TimeOnly)
#endif
        ) },
        { "time with time zone",         new(EDBDbType.TimeTz,      "time with time zone") },
        { "timetz",                      new(EDBDbType.TimeTz,      "time with time zone") },
        { "interval",                    new(EDBDbType.Interval,    "interval", typeof(TimeSpan)) },

        { "timestamp without time zone[]", new(EDBDbType.Array | EDBDbType.Timestamp,   "timestamp without time zone[]") },
        { "timestamp with time zone[]",    new(EDBDbType.Array | EDBDbType.TimestampTz, "timestamp with time zone[]") },

        { "int4range",                     new(EDBDbType.IntegerRange,     "int4range") },
        { "int8range",                     new(EDBDbType.BigIntRange,      "int8range") },
        { "numrange",                      new(EDBDbType.NumericRange,     "numrange") },
        { "daterange",                     new(EDBDbType.DateRange,        "daterange") },
        { "tsrange",                       new(EDBDbType.TimestampRange,   "tsrange") },
        { "tstzrange",                     new(EDBDbType.TimestampTzRange, "tstzrange") },

        { "int4multirange",                new(EDBDbType.IntegerMultirange,     "int4range") },
        { "int8multirange",                new(EDBDbType.BigIntMultirange,      "int8range") },
        { "nummultirange",                 new(EDBDbType.NumericMultirange,     "numrange") },
        { "datemultirange",                new(EDBDbType.DateMultirange,        "datemultirange") },
        { "tsmultirange",                  new(EDBDbType.TimestampMultirange,   "tsmultirange") },
        { "tstzmultirange",                new(EDBDbType.TimestampTzMultirange, "tstzmultirange") },

        // Network types
        { "cidr",      new(EDBDbType.Cidr,     "cidr") },
#pragma warning disable 618
        { "inet",      new(EDBDbType.Inet,     "inet", typeof(IPAddress), typeof((IPAddress Address, int Subnet)), typeof(EDBInet), ReadOnlyIPAddressType) },
#pragma warning restore 618
        { "macaddr",   new(EDBDbType.MacAddr,  "macaddr", typeof(PhysicalAddress)) },
        { "macaddr8",  new(EDBDbType.MacAddr8, "macaddr8") },

        // Full-text search types
        { "tsquery",   new(EDBDbType.TsQuery,  "tsquery",
            typeof(EDBTsQuery), typeof(EDBTsQueryAnd), typeof(EDBTsQueryEmpty), typeof(EDBTsQueryFollowedBy),
            typeof(EDBTsQueryLexeme), typeof(EDBTsQueryNot), typeof(EDBTsQueryOr), typeof(EDBTsQueryBinOp)
        ) },
        { "tsvector",  new(EDBDbType.TsVector, "tsvector", typeof(EDBTsVector)) },

        // Geometry types
        { "box",      new(EDBDbType.Box,     "box",     typeof(EDBBox)) },
        { "circle",   new(EDBDbType.Circle,  "circle",  typeof(EDBCircle)) },
        { "line",     new(EDBDbType.Line,    "line",    typeof(EDBLine)) },
        { "lseg",     new(EDBDbType.LSeg,    "lseg",    typeof(EDBLSeg)) },
        { "path",     new(EDBDbType.Path,    "path",    typeof(EDBPath)) },
        { "point",    new(EDBDbType.Point,   "point",   typeof(EDBPoint)) },
        { "polygon",  new(EDBDbType.Polygon, "polygon", typeof(EDBPolygon)) },

        // LTree types
        { "lquery",     new(EDBDbType.LQuery,    "lquery") },
        { "ltree",      new(EDBDbType.LTree,     "ltree") },
        { "ltxtquery",  new(EDBDbType.LTxtQuery, "ltxtquery") },

        // UInt types
        { "oid",        new(EDBDbType.Oid,       "oid") },
        { "xid",        new(EDBDbType.Xid,       "xid") },
        { "xid8",       new(EDBDbType.Xid8,      "xid8") },
        { "cid",        new(EDBDbType.Cid,       "cid") },
        { "regtype",    new(EDBDbType.Regtype,   "regtype") },
        { "regconfig",  new(EDBDbType.Regconfig, "regconfig") },

        // Misc types
        { "boolean",     new(EDBDbType.Boolean, "boolean", typeof(bool)) },
        { "bool",        new(EDBDbType.Boolean, "boolean", typeof(bool)) },
        { "bytea",       new(EDBDbType.Bytea,   "bytea", typeof(byte[]), typeof(ArraySegment<byte>)
#if !(NETSTANDARD2_0 || NETFRAMEWORK) // EnterpriseDB (NETFRAMEWORK)
            , typeof(ReadOnlyMemory<byte>), typeof(Memory<byte>)
#endif
        ) },
        { "uuid",        new(EDBDbType.Uuid,    "uuid", typeof(Guid)) },
        { "bit varying", new(EDBDbType.Varbit,  "bit varying", typeof(BitArray), typeof(BitVector32)) },
        { "varbit",      new(EDBDbType.Varbit,  "bit varying", typeof(BitArray), typeof(BitVector32)) },
        { "bit",         new(EDBDbType.Bit,     "bit") },
        { "hstore",      new(EDBDbType.Hstore,  "hstore", typeof(Dictionary<string, string?>), typeof(IDictionary<string, string?>)
#if !(NETSTANDARD2_0 || NETSTANDARD2_1 || NETFRAMEWORK) // EnterpriseDB (NETFRAMEWORK)
            , typeof(ImmutableDictionary<string, string?>)
#endif
        ) },

        // Internal types
        { "int2vector",  new(EDBDbType.Int2Vector,   "int2vector") },
        { "oidvector",   new(EDBDbType.Oidvector,    "oidvector") },
        { "pg_lsn",      new(EDBDbType.PgLsn,        "pg_lsn", typeof(EDBLogSequenceNumber)) },
        { "tid",         new(EDBDbType.Tid,          "tid", typeof(EDBTid)) },
        { "char",        new(EDBDbType.InternalChar, "char") },

        // Special types
        { "unknown",  new(EDBDbType.Unknown, "unknown") },
    };

    #region Cached handlers

    // Numeric types
    readonly Int16Handler _int16Handler;
    readonly Int32Handler _int32Handler;
    readonly Int64Handler _int64Handler;
    SingleHandler? _singleHandler;
    readonly DoubleHandler _doubleHandler;
    readonly NumericHandler _numericHandler;
    MoneyHandler? _moneyHandler;

    // Text types
    readonly TextHandler _textHandler;
    TextHandler? _xmlHandler;
    TextHandler? _varcharHandler;
    TextHandler? _charHandler;
    TextHandler? _nameHandler;
    TextHandler? _refcursorHandler;
    TextHandler? _citextHandler;
    JsonHandler? _jsonbHandler; // Note that old version of PG (and Redshift) don't have jsonb
    JsonHandler? _jsonHandler;
    JsonPathHandler? _jsonPathHandler;

    // Date/time types
    readonly TimestampHandler _timestampHandler;
    readonly TimestampTzHandler _timestampTzHandler;
    readonly DateHandler _dateHandler;
    TimeHandler? _timeHandler;
    TimeTzHandler? _timeTzHandler;
    IntervalHandler? _intervalHandler;

    // Network types
    CidrHandler? _cidrHandler;
    InetHandler? _inetHandler;
    MacaddrHandler? _macaddrHandler;
    MacaddrHandler? _macaddr8Handler;

    // Full-text search types
    TsQueryHandler? _tsQueryHandler;
    TsVectorHandler? _tsVectorHandler;

    // Geometry types
    BoxHandler? _boxHandler;
    CircleHandler? _circleHandler;
    LineHandler? _lineHandler;
    LineSegmentHandler? _lineSegmentHandler;
    PathHandler? _pathHandler;
    PointHandler? _pointHandler;
    PolygonHandler? _polygonHandler;

    // LTree types
    LQueryHandler? _lQueryHandler;
    LTreeHandler? _lTreeHandler;
    LTxtQueryHandler? _lTxtQueryHandler;

    // UInt types
    UInt32Handler? _oidHandler;
    UInt32Handler? _xidHandler;
    UInt64Handler? _xid8Handler;
    UInt32Handler? _cidHandler;
    UInt32Handler? _regtypeHandler;
    UInt32Handler? _regconfigHandler;

    // Misc types
    readonly BoolHandler _boolHandler;
    ByteaHandler? _byteaHandler;
    UuidHandler? _uuidHandler;
    BitStringHandler? _bitVaryingHandler;
    BitStringHandler? _bitHandler;
    RecordHandler? _recordHandler;
    VoidHandler? _voidHandler;
    HstoreHandler? _hstoreHandler;

    // Internal types
    Int2VectorHandler? _int2VectorHandler;
    OIDVectorHandler? _oidVectorHandler;
    PgLsnHandler? _pgLsnHandler;
    TidHandler? _tidHandler;
    InternalCharHandler? _internalCharHandler;

    // Special types
    UnknownTypeHandler? _unknownHandler;

    // Complex type handlers over timestamp/timestamptz (because DateTime is value-dependent)
    EDBTypeHandler? _timestampArrayHandler;
    EDBTypeHandler? _timestampTzArrayHandler;
    EDBTypeHandler? _timestampRangeHandler;
    EDBTypeHandler? _timestampTzRangeHandler;
    EDBTypeHandler? _timestampMultirangeHandler;
    EDBTypeHandler? _timestampTzMultirangeHandler;

    #endregion Cached handlers

    internal BuiltInTypeHandlerResolver(EDBConnector connector)
    {
        _connector = connector;
        _databaseInfo = connector.DatabaseInfo;

        // Eagerly instantiate some handlers for very common types so we don't need to check later
        _int16Handler = new Int16Handler(PgType("smallint"));
        _int32Handler = new Int32Handler(PgType("integer"));
        _int64Handler = new Int64Handler(PgType("bigint"));
        _doubleHandler = new DoubleHandler(PgType("double precision"));
        _numericHandler = new NumericHandler(PgType("numeric"));
        _textHandler ??= new TextHandler(PgType("text"), _connector.TextEncoding);
        _timestampHandler ??= new TimestampHandler(PgType("timestamp without time zone"));
        _timestampTzHandler ??= new TimestampTzHandler(PgType("timestamp with time zone"));
        _dateHandler ??= new DateHandler(PgType("date"));
        _boolHandler ??= new BoolHandler(PgType("boolean"));
    }

    public override EDBTypeHandler? ResolveByDataTypeName(string typeName)
        => typeName switch
        {
            // Numeric types
            "smallint"             => _int16Handler,
            "integer" or "int"     => _int32Handler,
            "bigint"               => _int64Handler,
            "real"                 => SingleHandler(),
            "double precision"     => _doubleHandler,
            "numeric" or "decimal" => _numericHandler,
            "money"                => MoneyHandler(),

            // Text types
            "text"                           => _textHandler,
            "xml"                            => XmlHandler(),
            "varchar" or "character varying" => VarcharHandler(),
            "character"                      => CharHandler(),
            "name"                           => NameHandler(),
            "refcursor"                      => RefcursorHandler(),
            "citext"                         => CitextHandler(),
            "jsonb"                          => JsonbHandler(),
            "json"                           => JsonHandler(),
            "jsonpath"                       => JsonPathHandler(),

            // Date/time types
            "timestamp" or "timestamp without time zone" => _timestampHandler,
            "timestamptz" or "timestamp with time zone"  => _timestampTzHandler,
            "date"                                       => _dateHandler,
            "time without time zone"                     => TimeHandler(),
            "time with time zone"                        => TimeTzHandler(),
            "interval"                                   => IntervalHandler(),

            // Network types
            "cidr"     => CidrHandler(),
            "inet"     => InetHandler(),
            "macaddr"  => MacaddrHandler(),
            "macaddr8" => Macaddr8Handler(),

            // Full-text search types
            "tsquery"  => TsQueryHandler(),
            "tsvector" => TsVectorHandler(),

            // Geometry types
            "box"     => BoxHandler(),
            "circle"  => CircleHandler(),
            "line"    => LineHandler(),
            "lseg"    => LineSegmentHandler(),
            "path"    => PathHandler(),
            "point"   => PointHandler(),
            "polygon" => PolygonHandler(),

            // LTree types
            "lquery"    => LQueryHandler(),
            "ltree"     => LTreeHandler(),
            "ltxtquery" => LTxtHandler(),

            // UInt types
            "oid"       => OidHandler(),
            "xid"       => XidHandler(),
            "xid8"      => Xid8Handler(),
            "cid"       => CidHandler(),
            "regtype"   => RegtypeHandler(),
            "regconfig" => RegconfigHandler(),

            // Misc types
            "bool" or "boolean"       => _boolHandler,
            "bytea"                   => ByteaHandler(),
            "uuid"                    => UuidHandler(),
            "bit varying" or "varbit" => BitVaryingHandler(),
            "bit"                     => BitHandler(),
            "hstore"                  => HstoreHandler(),

            // Internal types
            "int2vector" => Int2VectorHandler(),
            "oidvector"  => OidVectorHandler(),
            "pg_lsn"     => PgLsnHandler(),
            "tid"        => TidHandler(),
            "char"       => InternalCharHandler(),
            "record"     => RecordHandler(),
            "void"       => VoidHandler(),

            "unknown"    => UnknownHandler(),

            _ => null
        };

    public override EDBTypeHandler? ResolveByClrType(Type type)
    {
        if (!ClrTypeToDataTypeNameTable.TryGetValue(type, out var dataTypeName))
        {
            if (!type.IsSubclassOf(typeof(Stream)))
                return null;

            dataTypeName = "bytea";
        }

        return ResolveByDataTypeName(dataTypeName);
    }

    static readonly Dictionary<Type, string> ClrTypeToDataTypeNameTable;

    static BuiltInTypeHandlerResolver()
    {
        ClrTypeToDataTypeNameTable = new()
        {
            // Numeric types
            { typeof(byte),       "smallint" },
            { typeof(short),      "smallint" },
            { typeof(int),        "integer" },
            { typeof(long),       "bigint" },
            { typeof(float),      "real" },
            { typeof(double),     "double precision" },
            { typeof(decimal),    "decimal" },
            { typeof(BigInteger), "decimal" },

            // Text types
            { typeof(string),             "text" },
            { typeof(char[]),             "text" },
            { typeof(char),               "text" },
            { typeof(ArraySegment<char>), "text" },
            { typeof(JsonDocument),       "jsonb" },

            // Date/time types
            // The DateTime entry is for LegacyTimestampBehavior mode only. In regular mode we resolve through
            // ResolveValueDependentValue below
            { typeof(DateTime),       "timestamp without time zone" },
            { typeof(DateTimeOffset), "timestamp with time zone" },
#if NET6_0_OR_GREATER
            { typeof(DateOnly),       "date" },
            { typeof(TimeOnly),       "time without time zone" },
#endif
            { typeof(TimeSpan),       "interval" },
            { typeof(EDBInterval), "interval" },

            // Network types
            { typeof(IPAddress),                       "inet" },
            // See ReadOnlyIPAddress below
            { typeof((IPAddress Address, int Subnet)), "inet" },
#pragma warning disable 618
            { typeof(EDBInet),                      "inet" },
#pragma warning restore 618
            { typeof(PhysicalAddress),                 "macaddr" },

            // Full-text types
            { typeof(EDBTsVector),          "tsvector" },
            { typeof(EDBTsQueryLexeme),     "tsquery" },
            { typeof(EDBTsQueryAnd),        "tsquery" },
            { typeof(EDBTsQueryOr),         "tsquery" },
            { typeof(EDBTsQueryNot),        "tsquery" },
            { typeof(EDBTsQueryEmpty),      "tsquery" },
            { typeof(EDBTsQueryFollowedBy), "tsquery" },

            // Geometry types
            { typeof(EDBBox),     "box" },
            { typeof(EDBCircle),  "circle" },
            { typeof(EDBLine),    "line" },
            { typeof(EDBLSeg),    "lseg" },
            { typeof(EDBPath),    "path" },
            { typeof(EDBPoint),   "point" },
            { typeof(EDBPolygon), "polygon" },

            // Misc types
            { typeof(bool),                 "boolean" },
            { typeof(byte[]),               "bytea" },
            { typeof(ArraySegment<byte>),   "bytea" },
#if !(NETSTANDARD2_0 || NETFRAMEWORK) // EnterpriseDB (NETFRAMEWORK)
            { typeof(ReadOnlyMemory<byte>), "bytea" },
            { typeof(Memory<byte>),         "bytea" },
#endif
            { typeof(Guid),                                "uuid" },
            { typeof(BitArray),                            "bit varying" },
            { typeof(BitVector32),                         "bit varying" },
            { typeof(Dictionary<string, string>),          "hstore" },
#if !NETSTANDARD2_0 && !NETSTANDARD2_1
            { typeof(ImmutableDictionary<string, string>), "hstore" },
#endif

            // Internal types
            { typeof(EDBLogSequenceNumber), "pg_lsn" },
            { typeof(EDBTid),               "tid" },
            { typeof(DBNull),                  "unknown" },

            // Built-in range types
            { typeof(EDBRange<int>), "int4range" },
            { typeof(EDBRange<long>), "int8range" },
            { typeof(EDBRange<decimal>), "numrange" },
#if NET6_0_OR_GREATER
            { typeof(EDBRange<DateOnly>), "daterange" },
#endif

            // Built-in multirange types
            { typeof(EDBRange<int>[]), "int4multirange" },
            { typeof(List<EDBRange<int>>), "int4multirange" },
            { typeof(EDBRange<long>[]), "int8multirange" },
            { typeof(List<EDBRange<long>>), "int8multirange" },
            { typeof(EDBRange<decimal>[]), "nummultirange" },
            { typeof(List<EDBRange<decimal>>), "nummultirange" },
#if NET6_0_OR_GREATER
            { typeof(EDBRange<DateOnly>[]), "datemultirange" },
            { typeof(List<EDBRange<DateOnly>>), "datemultirange" },
#endif
        };

        // Recent versions of .NET Core have an internal ReadOnlyIPAddress type (returned e.g. for IPAddress.Loopback)
        // But older versions don't have it
        if (ReadOnlyIPAddressType != typeof(IPAddress))
            ClrTypeToDataTypeNameTable[ReadOnlyIPAddressType] = "inet";

        if (LegacyTimestampBehavior)
            ClrTypeToDataTypeNameTable[typeof(DateTime)] = "timestamp without time zone";
    }

    public override EDBTypeHandler? ResolveValueDependentValue(object value)
    {
        // In LegacyTimestampBehavior, DateTime isn't value-dependent, and handled above in ClrTypeToDataTypeNameTable like other types
        if (LegacyTimestampBehavior)
            return null;

        return value switch
        {
            DateTime dateTime => dateTime.Kind == DateTimeKind.Utc ? _timestampTzHandler : _timestampHandler,

            // For arrays/lists, return timestamp or timestamptz based on the kind of the first DateTime; if the user attempts to
            // mix incompatible Kinds, that will fail during validation. For empty arrays it doesn't matter.
            IList<DateTime> array => ArrayHandler(array.Count == 0 ? DateTimeKind.Unspecified : array[0].Kind),

            EDBRange<DateTime> range => RangeHandler(!range.LowerBoundInfinite ? range.LowerBound.Kind :
                !range.UpperBoundInfinite ? range.UpperBound.Kind : DateTimeKind.Unspecified),

            EDBRange<DateTime>[] multirange => MultirangeHandler(GetMultirangeKind(multirange)),
            List<EDBRange<DateTime>> multirange => MultirangeHandler(GetMultirangeKind(multirange)),

            _ => null
        };

        EDBTypeHandler ArrayHandler(DateTimeKind kind)
            => kind == DateTimeKind.Utc
                ? _timestampTzArrayHandler ??= _timestampTzHandler.CreateArrayHandler(
                    (PostgresArrayType)PgType("timestamp with time zone[]"), _connector.Settings.ArrayNullabilityMode)
                : _timestampArrayHandler ??= _timestampHandler.CreateArrayHandler(
                    (PostgresArrayType)PgType("timestamp without time zone[]"), _connector.Settings.ArrayNullabilityMode);

        EDBTypeHandler RangeHandler(DateTimeKind kind)
            => kind == DateTimeKind.Utc
                ? _timestampTzRangeHandler ??= _timestampTzHandler.CreateRangeHandler((PostgresRangeType)PgType("tstzrange"))
                : _timestampRangeHandler ??= _timestampHandler.CreateRangeHandler((PostgresRangeType)PgType("tsrange"));

        EDBTypeHandler MultirangeHandler(DateTimeKind kind)
            => kind == DateTimeKind.Utc
                ? _timestampTzMultirangeHandler ??= _timestampTzHandler.CreateMultirangeHandler((PostgresMultirangeType)PgType("tstzmultirange"))
                : _timestampMultirangeHandler ??= _timestampHandler.CreateMultirangeHandler((PostgresMultirangeType)PgType("tsmultirange"));
    }

    static DateTimeKind GetRangeKind(EDBRange<DateTime> range)
        => !range.LowerBoundInfinite
            ? range.LowerBound.Kind
            : !range.UpperBoundInfinite
                ? range.UpperBound.Kind
                : DateTimeKind.Unspecified;

    static DateTimeKind GetMultirangeKind(IList<EDBRange<DateTime>> multirange)
    {
        for (var i = 0; i < multirange.Count; i++)
            if (!multirange[i].IsEmpty)
                return GetRangeKind(multirange[i]);

        return DateTimeKind.Unspecified;
    }

    internal static string? ValueDependentValueToDataTypeName(object value)
    {
        // In LegacyTimestampBehavior, DateTime isn't value-dependent, and handled above in ClrTypeToDataTypeNameTable like other types
        if (LegacyTimestampBehavior)
            return null;

        return value switch
        {
            DateTime dateTime => dateTime.Kind == DateTimeKind.Utc ? "timestamp with time zone" : "timestamp without time zone",

            // For arrays/lists, return timestamp or timestamptz based on the kind of the first DateTime; if the user attempts to
            // mix incompatible Kinds, that will fail during validation. For empty arrays it doesn't matter.
            IList<DateTime> array => array.Count == 0
                ? "timestamp without time zone[]"
                : array[0].Kind == DateTimeKind.Utc ? "timestamp with time zone[]" : "timestamp without time zone[]",

            EDBRange<DateTime> range => GetRangeKind(range) == DateTimeKind.Utc ? "tstzrange" : "tsrange",

            EDBRange<DateTime>[] multirange => GetMultirangeKind(multirange) == DateTimeKind.Utc ? "tstzmultirange" : "tsmultirange",

            _ => null
        };
    }

    public override EDBTypeHandler? ResolveValueTypeGenerically<T>(T value)
    {
        // This method only ever gets called for value types, and relies on the JIT specializing the method for T by eliding all the
        // type checks below.

        // Numeric types
        if (typeof(T) == typeof(byte))
            return _int16Handler;
        if (typeof(T) == typeof(short))
            return _int16Handler;
        if (typeof(T) == typeof(int))
            return _int32Handler;
        if (typeof(T) == typeof(long))
            return _int64Handler;
        if (typeof(T) == typeof(float))
            return SingleHandler();
        if (typeof(T) == typeof(double))
            return _doubleHandler;
        if (typeof(T) == typeof(decimal))
            return _numericHandler;
        if (typeof(T) == typeof(BigInteger))
            return _numericHandler;

        // Text types
        if (typeof(T) == typeof(char))
            return _textHandler;
        if (typeof(T) == typeof(ArraySegment<char>))
            return _textHandler;
        if (typeof(T) == typeof(JsonDocument))
            return JsonbHandler();

        // Date/time types
        // No resolution for DateTime, since that's value-dependent (Kind)
        if (typeof(T) == typeof(DateTimeOffset))
            return _timestampTzHandler;
#if NET6_0_OR_GREATER
        if (typeof(T) == typeof(DateOnly))
            return _dateHandler;
        if (typeof(T) == typeof(TimeOnly))
            return _timeHandler;
#endif
        if (typeof(T) == typeof(TimeSpan))
            return _intervalHandler;
        if (typeof(T) == typeof(EDBInterval))
            return _intervalHandler;

        // Network types
        if (typeof(T) == typeof(IPAddress))
            return InetHandler();
        if (typeof(T) == typeof(PhysicalAddress))
            return _macaddrHandler;
        if (typeof(T) == typeof(TimeSpan))
            return _intervalHandler;

        // Geometry types
        if (typeof(T) == typeof(EDBBox))
            return BoxHandler();
        if (typeof(T) == typeof(EDBCircle))
            return CircleHandler();
        if (typeof(T) == typeof(EDBLine))
            return LineHandler();
        if (typeof(T) == typeof(EDBLSeg))
            return LineSegmentHandler();
        if (typeof(T) == typeof(EDBPath))
            return PathHandler();
        if (typeof(T) == typeof(EDBPoint))
            return PointHandler();
        if (typeof(T) == typeof(EDBPolygon))
            return PolygonHandler();

        // Misc types
        if (typeof(T) == typeof(bool))
            return _boolHandler;
        if (typeof(T) == typeof(Guid))
            return UuidHandler();
        if (typeof(T) == typeof(BitVector32))
            return BitVaryingHandler();

        // Internal types
        if (typeof(T) == typeof(EDBLogSequenceNumber))
            return PgLsnHandler();
        if (typeof(T) == typeof(EDBTid))
            return TidHandler();
        if (typeof(T) == typeof(DBNull))
            return UnknownHandler();

        return null;
    }

    internal static string? ClrTypeToDataTypeName(Type type)
        => ClrTypeToDataTypeNameTable.TryGetValue(type, out var dataTypeName) ? dataTypeName : null;

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => DoGetMappingByDataTypeName(dataTypeName);

    internal static TypeMappingInfo? DoGetMappingByDataTypeName(string dataTypeName)
        => Mappings.TryGetValue(dataTypeName, out var mapping) ? mapping : null;

    PostgresType PgType(string pgTypeName) => _databaseInfo.GetPostgresTypeByName(pgTypeName);

    #region Handler accessors

    // Numeric types
    EDBTypeHandler SingleHandler() => _singleHandler ??= new SingleHandler(PgType("real"));
    EDBTypeHandler MoneyHandler()  => _moneyHandler ??= new MoneyHandler(PgType("money"));

    // Text types
    EDBTypeHandler XmlHandler()       => _xmlHandler ??= new TextHandler(PgType("xml"), _connector.TextEncoding);
    EDBTypeHandler VarcharHandler()   => _varcharHandler ??= new TextHandler(PgType("character varying"), _connector.TextEncoding);
    EDBTypeHandler CharHandler()      => _charHandler ??= new TextHandler(PgType("character"), _connector.TextEncoding);
    EDBTypeHandler NameHandler()      => _nameHandler ??= new TextHandler(PgType("name"), _connector.TextEncoding);
    EDBTypeHandler RefcursorHandler() => _refcursorHandler ??= new TextHandler(PgType("refcursor"), _connector.TextEncoding);
    EDBTypeHandler? CitextHandler()   => _citextHandler ??= _databaseInfo.TryGetPostgresTypeByName("citext", out var pgType)
        ? new TextHandler(pgType, _connector.TextEncoding)
        : null;
    EDBTypeHandler JsonbHandler()     => _jsonbHandler ??= new JsonHandler(PgType("jsonb"), _connector.TextEncoding, isJsonb: true);
    EDBTypeHandler JsonHandler()      => _jsonHandler ??= new JsonHandler(PgType("json"), _connector.TextEncoding, isJsonb: false);
    EDBTypeHandler JsonPathHandler()  => _jsonPathHandler ??= new JsonPathHandler(PgType("jsonpath"), _connector.TextEncoding);

    // Date/time types
    EDBTypeHandler TimeHandler()     => _timeHandler ??= new TimeHandler(PgType("time without time zone"));
    EDBTypeHandler TimeTzHandler()   => _timeTzHandler ??= new TimeTzHandler(PgType("time with time zone"));
    EDBTypeHandler IntervalHandler() => _intervalHandler ??= new IntervalHandler(PgType("interval"));

    // Network types
    EDBTypeHandler CidrHandler()     => _cidrHandler ??= new CidrHandler(PgType("cidr"));
    EDBTypeHandler InetHandler()     => _inetHandler ??= new InetHandler(PgType("inet"));
    EDBTypeHandler MacaddrHandler()  => _macaddrHandler ??= new MacaddrHandler(PgType("macaddr"));
    EDBTypeHandler Macaddr8Handler() => _macaddr8Handler ??= new MacaddrHandler(PgType("macaddr8"));

    // Full-text search types
    EDBTypeHandler TsQueryHandler()  => _tsQueryHandler ??= new TsQueryHandler(PgType("tsquery"));
    EDBTypeHandler TsVectorHandler() => _tsVectorHandler ??= new TsVectorHandler(PgType("tsvector"));

    // Geometry types
    EDBTypeHandler BoxHandler()         => _boxHandler ??= new BoxHandler(PgType("box"));
    EDBTypeHandler CircleHandler()      => _circleHandler ??= new CircleHandler(PgType("circle"));
    EDBTypeHandler LineHandler()        => _lineHandler ??= new LineHandler(PgType("line"));
    EDBTypeHandler LineSegmentHandler() => _lineSegmentHandler ??= new LineSegmentHandler(PgType("lseg"));
    EDBTypeHandler PathHandler()        => _pathHandler ??= new PathHandler(PgType("path"));
    EDBTypeHandler PointHandler()       => _pointHandler ??= new PointHandler(PgType("point"));
    EDBTypeHandler PolygonHandler()     => _polygonHandler ??= new PolygonHandler(PgType("polygon"));

    // LTree types
    EDBTypeHandler? LQueryHandler() => _lQueryHandler ??= _databaseInfo.TryGetPostgresTypeByName("lquery", out var pgType)
        ? new LQueryHandler(pgType, _connector.TextEncoding)
        : null;
    EDBTypeHandler? LTreeHandler()  => _lTreeHandler ??= _databaseInfo.TryGetPostgresTypeByName("ltree", out var pgType)
        ? new LTreeHandler(pgType, _connector.TextEncoding)
        : null;
    EDBTypeHandler? LTxtHandler()   => _lTxtQueryHandler ??= _databaseInfo.TryGetPostgresTypeByName("ltxtquery", out var pgType)
        ? new LTxtQueryHandler(pgType, _connector.TextEncoding)
        : null;

    // UInt types
    EDBTypeHandler OidHandler()       => _oidHandler ??= new UInt32Handler(PgType("oid"));
    EDBTypeHandler XidHandler()       => _xidHandler ??= new UInt32Handler(PgType("xid"));
    EDBTypeHandler Xid8Handler()      => _xid8Handler ??= new UInt64Handler(PgType("xid8"));
    EDBTypeHandler CidHandler()       => _cidHandler ??= new UInt32Handler(PgType("cid"));
    EDBTypeHandler RegtypeHandler()   => _regtypeHandler ??= new UInt32Handler(PgType("regtype"));
    EDBTypeHandler RegconfigHandler() => _regconfigHandler ??= new UInt32Handler(PgType("regconfig"));

    // Misc types
    EDBTypeHandler ByteaHandler()      => _byteaHandler ??= new ByteaHandler(PgType("bytea"));
    EDBTypeHandler UuidHandler()       => _uuidHandler ??= new UuidHandler(PgType("uuid"));
    EDBTypeHandler BitVaryingHandler() => _bitVaryingHandler ??= new BitStringHandler(PgType("bit varying"));
    EDBTypeHandler BitHandler()        => _bitHandler ??= new BitStringHandler(PgType("bit"));
    EDBTypeHandler? HstoreHandler()    => _hstoreHandler ??= _databaseInfo.TryGetPostgresTypeByName("hstore", out var pgType)
        ? new HstoreHandler(pgType, _textHandler)
        : null;

    // Internal types
    EDBTypeHandler Int2VectorHandler()   => _int2VectorHandler ??= new Int2VectorHandler(PgType("int2vector"), PgType("smallint"));
    EDBTypeHandler OidVectorHandler()    => _oidVectorHandler ??= new OIDVectorHandler(PgType("oidvector"), PgType("oid"));
    EDBTypeHandler PgLsnHandler()        => _pgLsnHandler ??= new PgLsnHandler(PgType("pg_lsn"));
    EDBTypeHandler TidHandler()          => _tidHandler ??= new TidHandler(PgType("tid"));
    EDBTypeHandler InternalCharHandler() => _internalCharHandler ??= new InternalCharHandler(PgType("char"));
    EDBTypeHandler RecordHandler()       => _recordHandler ??= new RecordHandler(PgType("record"), _connector.TypeMapper);
    EDBTypeHandler VoidHandler()         => _voidHandler ??= new VoidHandler(PgType("void"));

    EDBTypeHandler UnknownHandler() => _unknownHandler ??= new UnknownTypeHandler(_connector.TextEncoding);

    #endregion Handler accessors
}
