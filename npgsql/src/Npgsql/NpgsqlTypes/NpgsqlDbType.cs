using System;
using System.Data;
using EnterpriseDB.EDBClient;
using EnterpriseDB.EDBClient.Internal.Postgres;
using static EnterpriseDB.EDBClient.Util.Statics;

#pragma warning disable CA1720

// ReSharper disable once CheckNamespace
namespace EDBTypes;

/// <summary>
/// Represents a PostgreSQL data type that can be written or read to the database.
/// Used in places such as <see cref="EDBParameter.EDBDbType"/> to unambiguously specify
/// how to encode or decode values.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype.html.
/// </remarks>
// Source for PG OIDs: <see href="https://github.com/postgres/postgres/blob/master/src/include/catalog/pg_type.dat" />
public enum EDBDbType
{
    // Note that it's important to never change the numeric values of this enum, since user applications
    // compile them in.

    #region Numeric Types

    /// <summary>
    /// Corresponds to the PostgreSQL 8-byte "bigint" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    Bigint = 1,

    /// <summary>
    /// Corresponds to the PostgreSQL 8-byte floating-point "double" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    Double = 8,

    /// <summary>
    /// Corresponds to the PostgreSQL 4-byte "integer" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    Integer = 9,

    /// <summary>
    /// Corresponds to the PostgreSQL arbitrary-precision "numeric" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    Numeric = 13,

    /// <summary>
    /// Corresponds to the PostgreSQL floating-point "real" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    Real = 17,

    /// <summary>
    /// Corresponds to the PostgreSQL 2-byte "smallint" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    Smallint = 18,

    /// <summary>
    /// Corresponds to the PostgreSQL "money" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-money.html</remarks>
    Money = 12,

    #endregion

    #region Boolean Type

    /// <summary>
    /// Corresponds to the PostgreSQL "boolean" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-boolean.html</remarks>
    Boolean = 2,

    #endregion

    #region Geometric types

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "box" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    Box = 3,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "circle" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    Circle = 5,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "line" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    Line = 10,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "lseg" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    LSeg = 11,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "path" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    Path = 14,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "point" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    Point = 15,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "polygon" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    Polygon = 16,

    /// <summary>
    /// Corresponds to the PostgreSQL "cube" type, a geometric type representing multi-dimensional cubes.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/cube.html</remarks>
    Cube = 63, // Extension type

    #endregion

    #region Character Types

    /// <summary>
    /// Corresponds to the PostgreSQL "char(n)" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    Char = 6,

    /// <summary>
    /// Corresponds to the PostgreSQL "text" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    Text = 19,

    /// <summary>
    /// Corresponds to the PostgreSQL "varchar" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    Varchar = 22,

    /// <summary>
    /// Corresponds to the PostgreSQL internal "name" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    Name = 32,

    /// <summary>
    /// Corresponds to the PostgreSQL "citext" type for the citext module.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/citext.html</remarks>
    Citext = 51,   // Extension type

    /// <summary>
    /// Corresponds to the PostgreSQL "char" type.
    /// </summary>
    /// <remarks>
    /// This is an internal field and should normally not be used for regular applications.
    ///
    /// See https://www.postgresql.org/docs/current/static/datatype-text.html
    /// </remarks>
    InternalChar = 38,

    #endregion

    #region Binary Data Types

    /// <summary>
    /// Corresponds to the PostgreSQL "bytea" type, holding a raw byte string.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-binary.html</remarks>
    Bytea = 4,

    #endregion

    #region Date/Time Types

    /// <summary>
    /// Corresponds to the PostgreSQL "date" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    Date = 7,

    /// <summary>
    /// Corresponds to the PostgreSQL "time" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    Time = 20,

    /// <summary>
    /// Corresponds to the PostgreSQL "timestamp" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    Timestamp = 21,

    /// <summary>
    /// Corresponds to the PostgreSQL "timestamp with time zone" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    TimestampTz = 26,

    /// <summary>
    /// Corresponds to the PostgreSQL "interval" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    Interval = 30,

    /// <summary>
    /// Corresponds to the PostgreSQL "time with time zone" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    TimeTz = 31,

    /// <summary>
    /// Corresponds to the obsolete PostgreSQL "abstime" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [Obsolete("The PostgreSQL abstime time is obsolete.")]
    Abstime = 33,

    #endregion

    #region Network Address Types

    /// <summary>
    /// Corresponds to the PostgreSQL "inet" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    Inet = 24,

    /// <summary>
    /// Corresponds to the PostgreSQL "cidr" type, a field storing an IPv4 or IPv6 network.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    Cidr = 44,

    /// <summary>
    /// Corresponds to the PostgreSQL "macaddr" type, a field storing a 6-byte physical address.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    MacAddr = 34,

    /// <summary>
    /// Corresponds to the PostgreSQL "macaddr8" type, a field storing a 6-byte or 8-byte physical address.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    MacAddr8 = 54,

    #endregion

    #region Bit String Types

    /// <summary>
    /// Corresponds to the PostgreSQL "bit" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-bit.html</remarks>
    Bit = 25,

    /// <summary>
    /// Corresponds to the PostgreSQL "varbit" type, a field storing a variable-length string of bits.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-boolean.html</remarks>
    Varbit = 39,

    #endregion

    #region Text Search Types

    /// <summary>
    /// Corresponds to the PostgreSQL "tsvector" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>
    TsVector = 45,

    /// <summary>
    /// Corresponds to the PostgreSQL "tsquery" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>
    TsQuery = 46,

    /// <summary>
    /// Corresponds to the PostgreSQL "regconfig" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>
    Regconfig = 56,

    #endregion

    #region UUID Type

    /// <summary>
    /// Corresponds to the PostgreSQL "uuid" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-uuid.html</remarks>
    Uuid = 27,

    #endregion

    #region XML Type

    /// <summary>
    /// Corresponds to the PostgreSQL "xml" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-xml.html</remarks>
    Xml = 28,

    #endregion

    #region JSON Types

    /// <summary>
    /// Corresponds to the PostgreSQL "json" type, a field storing JSON in text format.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-json.html</remarks>
    /// <seealso cref="Jsonb"/>
    Json = 35,

    /// <summary>
    /// Corresponds to the PostgreSQL "jsonb" type, a field storing JSON in an optimized binary.
    /// format.
    /// </summary>
    /// <remarks>
    /// Supported since PostgreSQL 9.4.
    /// See https://www.postgresql.org/docs/current/static/datatype-json.html
    /// </remarks>
    Jsonb = 36,

    /// <summary>
    /// Corresponds to the PostgreSQL "jsonpath" type, a field storing JSON path in text format.
    /// format.
    /// </summary>
    /// <remarks>
    /// Supported since PostgreSQL 12.
    /// See https://www.postgresql.org/docs/current/datatype-json.html#DATATYPE-JSONPATH
    /// </remarks>
    JsonPath = 57,

    #endregion

    #region HSTORE Type

    /// <summary>
    /// Corresponds to the PostgreSQL "hstore" type, a dictionary of string key-value pairs.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/hstore.html</remarks>
    Hstore = 37, // Extension type

    #endregion

    #region Internal Types

    /// <summary>
    /// Corresponds to the PostgreSQL "refcursor" type.
    /// </summary>
    Refcursor = 23,

    /// <summary>
    /// Corresponds to the PostgreSQL internal "oidvector" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>
    Oidvector = 29,

    /// <summary>
    /// Corresponds to the PostgreSQL internal "int2vector" type.
    /// </summary>
    Int2Vector = 52,

    /// <summary>
    /// Corresponds to the PostgreSQL "oid" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>
    Oid = 41,

    /// <summary>
    /// Corresponds to the PostgreSQL "xid" type, an internal transaction identifier.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>
    Xid = 42,

    /// <summary>
    /// Corresponds to the PostgreSQL "xid8" type, an internal transaction identifier.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>
    Xid8 = 64,

    /// <summary>
    /// Corresponds to the PostgreSQL "cid" type, an internal command identifier.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>
    Cid = 43,

    /// <summary>
    /// Corresponds to the PostgreSQL "regtype" type, a numeric (OID) ID of a type in the pg_type table.
    /// </summary>
    Regtype = 49,

    /// <summary>
    /// Corresponds to the PostgreSQL "tid" type, a tuple id identifying the physical location of a row within its table.
    /// </summary>
    Tid = 53,

    /// <summary>
    /// Corresponds to the PostgreSQL "pg_lsn" type, which can be used to store LSN (Log Sequence Number) data which
    /// is a pointer to a location in the WAL.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/datatype-pg-lsn.html and
    /// https://git.postgresql.org/gitweb/?p=postgresql.git;a=commit;h=7d03a83f4d0736ba869fa6f93973f7623a27038a
    /// </remarks>
    PgLsn = 59,

    #endregion

    #region Special

    /// <summary>
    /// A special value that can be used to send parameter values to the database without
    /// specifying their type, allowing the database to cast them to another value based on context.
    /// The value will be converted to a string and send as text.
    /// </summary>
    /// <remarks>
    /// This value shouldn't ordinarily be used, and makes sense only when sending a data type
    /// unsupported by EnterpriseDB.EDBClient.
    /// </remarks>
    Unknown = 40,

    #endregion

    #region PostGIS

    /// <summary>
    /// The geometry type for PostgreSQL spatial extension PostGIS.
    /// </summary>
    Geometry = 50,  // Extension type

    /// <summary>
    /// The geography (geodetic) type for PostgreSQL spatial extension PostGIS.
    /// </summary>
    Geography = 55, // Extension type

    #endregion

    #region Label tree types

    /// <summary>
    /// The PostgreSQL ltree type, each value is a label path "a.label.tree.value", forming a tree in a set.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/ltree.html</remarks>
    LTree = 60, // Extension type

    /// <summary>
    /// The PostgreSQL lquery type for PostgreSQL extension ltree
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/ltree.html</remarks>
    LQuery = 61, // Extension type

    /// <summary>
    /// The PostgreSQL ltxtquery type for PostgreSQL extension ltree
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/ltree.html</remarks>
    LTxtQuery = 62, // Extension type

    #endregion

    #region Range types

    /// <summary>
    /// Corresponds to the PostgreSQL "int4range" type.
    /// </summary>
    IntegerRange = Range | Integer,

    /// <summary>
    /// Corresponds to the PostgreSQL "int8range" type.
    /// </summary>
    BigIntRange = Range | Bigint,

    /// <summary>
    /// Corresponds to the PostgreSQL "numrange" type.
    /// </summary>
    NumericRange = Range | Numeric,

    /// <summary>
    /// Corresponds to the PostgreSQL "tsrange" type.
    /// </summary>
    TimestampRange = Range | Timestamp,

    /// <summary>
    /// Corresponds to the PostgreSQL "tstzrange" type.
    /// </summary>
    TimestampTzRange = Range | TimestampTz,

    /// <summary>
    /// Corresponds to the PostgreSQL "daterange" type.
    /// </summary>
    DateRange = Range | Date,

    #endregion Range types

    #region Multirange types

    /// <summary>
    /// Corresponds to the PostgreSQL "int4multirange" type.
    /// </summary>
    IntegerMultirange = Multirange | Integer,

    /// <summary>
    /// Corresponds to the PostgreSQL "int8multirange" type.
    /// </summary>
    BigIntMultirange = Multirange | Bigint,

    /// <summary>
    /// Corresponds to the PostgreSQL "nummultirange" type.
    /// </summary>
    NumericMultirange = Multirange | Numeric,

    /// <summary>
    /// Corresponds to the PostgreSQL "tsmultirange" type.
    /// </summary>
    TimestampMultirange = Multirange | Timestamp,

    /// <summary>
    /// Corresponds to the PostgreSQL "tstzmultirange" type.
    /// </summary>
    TimestampTzMultirange = Multirange | TimestampTz,

    /// <summary>
    /// Corresponds to the PostgreSQL "datemultirange" type.
    /// </summary>
    DateMultirange = Multirange | Date,

    #endregion Multirange types

    #region Composables

    /// <summary>
    /// Corresponds to the PostgreSQL "array" type, a variable-length multidimensional array of
    /// another type. This value must be combined with another value from <see cref="EDBDbType"/>
    /// via a bit OR (e.g. EDBDbType.Array | EDBDbType.Integer)
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/arrays.html</remarks>
    Array = int.MinValue,

    /// <summary>
    /// Corresponds to the PostgreSQL "range" type, continuous range of values of specific type.
    /// This value must be combined with another value from <see cref="EDBDbType"/>
    /// via a bit OR (e.g. EDBDbType.Range | EDBDbType.Integer)
    /// </summary>
    /// <remarks>
    /// Supported since PostgreSQL 9.2.
    /// See https://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    Range = 0x40000000,

    /// <summary>
    /// Corresponds to the PostgreSQL "multirange" type, continuous range of values of specific type.
    /// This value must be combined with another value from <see cref="EDBDbType"/>
    /// via a bit OR (e.g. EDBDbType.Multirange | EDBDbType.Integer)
    /// </summary>
    /// <remarks>
    /// Supported since PostgreSQL 14.
    /// See https://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    Multirange = 0x20000000,

    #endregion
}

static class EDBDbTypeExtensions
{
    internal static EDBDbType? ToEDBDbType(this DbType dbType)
        => dbType switch
        {
            DbType.AnsiString => EDBDbType.Text,
            DbType.Binary => EDBDbType.Bytea,
            DbType.Byte => EDBDbType.Smallint,
            DbType.Boolean => EDBDbType.Boolean,
            DbType.Currency => EDBDbType.Money,
            DbType.Date => EDBDbType.Date,
            DbType.DateTime => LegacyTimestampBehavior ? EDBDbType.Timestamp : EDBDbType.TimestampTz,
            DbType.Decimal => EDBDbType.Numeric,
            DbType.VarNumeric => EDBDbType.Numeric,
            DbType.Double => EDBDbType.Double,
            DbType.Guid => EDBDbType.Uuid,
            DbType.Int16 => EDBDbType.Smallint,
            DbType.Int32 => EDBDbType.Integer,
            DbType.Int64 => EDBDbType.Bigint,
            DbType.Single => EDBDbType.Real,
            DbType.String => EDBDbType.Text,
            DbType.Time => EDBDbType.Time,
            DbType.AnsiStringFixedLength => EDBDbType.Text,
            DbType.StringFixedLength => EDBDbType.Text,
            DbType.Xml => EDBDbType.Xml,
            DbType.DateTime2 => EDBDbType.Timestamp,
            DbType.DateTimeOffset => EDBDbType.TimestampTz,

            DbType.Object => null,
            DbType.SByte => null,
            DbType.UInt16 => null,
            DbType.UInt32 => null,
            DbType.UInt64 => null,

            _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null)
        };

    public static DbType ToDbType(this EDBDbType npgsqlDbType)
        => npgsqlDbType switch
        {
            // Numeric types
            EDBDbType.Smallint => DbType.Int16,
            EDBDbType.Integer => DbType.Int32,
            EDBDbType.Bigint => DbType.Int64,
            EDBDbType.Real => DbType.Single,
            EDBDbType.Double => DbType.Double,
            EDBDbType.Numeric => DbType.Decimal,
            EDBDbType.Money => DbType.Currency,

            // Text types
            EDBDbType.Text => DbType.String,
            EDBDbType.Xml => DbType.Xml,
            EDBDbType.Varchar => DbType.String,
            EDBDbType.Char => DbType.String,
            EDBDbType.Name => DbType.String,
            EDBDbType.Citext => DbType.String,
            EDBDbType.Refcursor => DbType.Object,
            EDBDbType.Jsonb => DbType.Object,
            EDBDbType.Json => DbType.Object,
            EDBDbType.JsonPath => DbType.Object,

            // Date/time types
            EDBDbType.Timestamp => LegacyTimestampBehavior ? DbType.DateTime : DbType.DateTime2,
            EDBDbType.TimestampTz => LegacyTimestampBehavior ? DbType.DateTimeOffset : DbType.DateTime,
            EDBDbType.Date => DbType.Date,
            EDBDbType.Time => DbType.Time,

            // Misc data types
            EDBDbType.Bytea => DbType.Binary,
            EDBDbType.Boolean => DbType.Boolean,
            EDBDbType.Uuid => DbType.Guid,

            EDBDbType.Unknown => DbType.Object,

            _ => DbType.Object
        };

    /// Can return null when a custom range type is used.
    internal static string? ToUnqualifiedDataTypeName(this EDBDbType npgsqlDbType)
        => npgsqlDbType switch
        {
            // Numeric types
            EDBDbType.Smallint => "int2",
            EDBDbType.Integer  => "int4",
            EDBDbType.Bigint   => "int8",
            EDBDbType.Real     => "float4",
            EDBDbType.Double   => "float8",
            EDBDbType.Numeric  => "numeric",
            EDBDbType.Money    => "money",

            // Text types
            EDBDbType.Text      => "text",
            EDBDbType.Xml       => "xml",
            EDBDbType.Varchar   => "varchar",
            EDBDbType.Char      => "bpchar",
            EDBDbType.Name      => "name",
            EDBDbType.Refcursor => "refcursor",
            EDBDbType.Jsonb     => "jsonb",
            EDBDbType.Json      => "json",
            EDBDbType.JsonPath  => "jsonpath",

            // Date/time types
            EDBDbType.Timestamp   => "timestamp",
            EDBDbType.TimestampTz => "timestamptz",
            EDBDbType.Date        => "date",
            EDBDbType.Time        => "time",
            EDBDbType.TimeTz      => "timetz",
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
            EDBDbType.Varbit  => "varbit",
            EDBDbType.Bit     => "bit",

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

            // Plugin types
            EDBDbType.Citext    => "citext",
            EDBDbType.Cube      => "cube",
            EDBDbType.LQuery    => "lquery",
            EDBDbType.LTree     => "ltree",
            EDBDbType.LTxtQuery => "ltxtquery",
            EDBDbType.Hstore    => "hstore",
            EDBDbType.Geometry  => "geometry",
            EDBDbType.Geography => "geography",

            EDBDbType.Unknown => "unknown",

            // Unknown cannot be composed
            _ when npgsqlDbType.HasFlag(EDBDbType.Array) && (npgsqlDbType & ~EDBDbType.Array) == EDBDbType.Unknown
                => "unknown",
            _ when npgsqlDbType.HasFlag(EDBDbType.Range) && (npgsqlDbType & ~EDBDbType.Range) == EDBDbType.Unknown
                => "unknown",
            _ when npgsqlDbType.HasFlag(EDBDbType.Multirange) && (npgsqlDbType & ~EDBDbType.Multirange) == EDBDbType.Unknown
                => "unknown",

            _ => npgsqlDbType.HasFlag(EDBDbType.Array)
                ? ToUnqualifiedDataTypeName(npgsqlDbType & ~EDBDbType.Array) is { } name ? "_" + name : null
                : null // e.g. ranges
        };

    internal static string ToUnqualifiedDataTypeNameOrThrow(this EDBDbType npgsqlDbType)
        => npgsqlDbType.ToUnqualifiedDataTypeName() ?? throw new ArgumentOutOfRangeException(nameof(npgsqlDbType), npgsqlDbType, "Cannot convert EDBDbType to DataTypeName");

    /// Can return null when a plugin type or custom range type is used.
    internal static DataTypeName? ToDataTypeName(this EDBDbType npgsqlDbType)
        => npgsqlDbType switch
        {
            // Numeric types
            EDBDbType.Smallint => DataTypeNames.Int2,
            EDBDbType.Integer => DataTypeNames.Int4,
            EDBDbType.Bigint => DataTypeNames.Int8,
            EDBDbType.Real => DataTypeNames.Float4,
            EDBDbType.Double => DataTypeNames.Float8,
            EDBDbType.Numeric => DataTypeNames.Numeric,
            EDBDbType.Money => DataTypeNames.Money,

            // Text types
            EDBDbType.Text => DataTypeNames.Text,
            EDBDbType.Xml => DataTypeNames.Xml,
            EDBDbType.Varchar => DataTypeNames.Varchar,
            EDBDbType.Char => DataTypeNames.Bpchar,
            EDBDbType.Name => DataTypeNames.Name,
            EDBDbType.Refcursor => DataTypeNames.RefCursor,
            EDBDbType.Jsonb => DataTypeNames.Jsonb,
            EDBDbType.Json => DataTypeNames.Json,
            EDBDbType.JsonPath => DataTypeNames.Jsonpath,

            // Date/time types
            EDBDbType.Timestamp => DataTypeNames.Timestamp,
            EDBDbType.TimestampTz => DataTypeNames.TimestampTz,
            EDBDbType.Date => DataTypeNames.Date,
            EDBDbType.Time => DataTypeNames.Time,
            EDBDbType.TimeTz => DataTypeNames.TimeTz,
            EDBDbType.Interval => DataTypeNames.Interval,

            // Network types
            EDBDbType.Cidr => DataTypeNames.Cidr,
            EDBDbType.Inet => DataTypeNames.Inet,
            EDBDbType.MacAddr => DataTypeNames.MacAddr,
            EDBDbType.MacAddr8 => DataTypeNames.MacAddr8,

            // Full-text search types
            EDBDbType.TsQuery => DataTypeNames.TsQuery,
            EDBDbType.TsVector => DataTypeNames.TsVector,

            // Geometry types
            EDBDbType.Box => DataTypeNames.Box,
            EDBDbType.Circle => DataTypeNames.Circle,
            EDBDbType.Line => DataTypeNames.Line,
            EDBDbType.LSeg => DataTypeNames.LSeg,
            EDBDbType.Path => DataTypeNames.Path,
            EDBDbType.Point => DataTypeNames.Point,
            EDBDbType.Polygon => DataTypeNames.Polygon,

            // UInt types
            EDBDbType.Oid => DataTypeNames.Oid,
            EDBDbType.Xid => DataTypeNames.Xid,
            EDBDbType.Xid8 => DataTypeNames.Xid8,
            EDBDbType.Cid => DataTypeNames.Cid,
            EDBDbType.Regtype => DataTypeNames.RegType,
            EDBDbType.Regconfig => DataTypeNames.RegConfig,

            // Misc types
            EDBDbType.Boolean => DataTypeNames.Bool,
            EDBDbType.Bytea => DataTypeNames.Bytea,
            EDBDbType.Uuid => DataTypeNames.Uuid,
            EDBDbType.Varbit => DataTypeNames.Varbit,
            EDBDbType.Bit => DataTypeNames.Bit,

            // Built-in range types
            EDBDbType.IntegerRange => DataTypeNames.Int4Range,
            EDBDbType.BigIntRange => DataTypeNames.Int8Range,
            EDBDbType.NumericRange => DataTypeNames.NumRange,
            EDBDbType.TimestampRange => DataTypeNames.TsRange,
            EDBDbType.TimestampTzRange => DataTypeNames.TsTzRange,
            EDBDbType.DateRange => DataTypeNames.DateRange,

            // Internal types
            EDBDbType.Int2Vector => DataTypeNames.Int2Vector,
            EDBDbType.Oidvector => DataTypeNames.OidVector,
            EDBDbType.PgLsn => DataTypeNames.PgLsn,
            EDBDbType.Tid => DataTypeNames.Tid,
            EDBDbType.InternalChar => DataTypeNames.Char,

            // Special types
            EDBDbType.Unknown => DataTypeNames.Unknown,

            // Unknown cannot be composed
            _ when npgsqlDbType.HasFlag(EDBDbType.Array) && (npgsqlDbType & ~EDBDbType.Array) == EDBDbType.Unknown
                => DataTypeNames.Unknown,
            _ when npgsqlDbType.HasFlag(EDBDbType.Range) && (npgsqlDbType & ~EDBDbType.Range) == EDBDbType.Unknown
                => DataTypeNames.Unknown,
            _ when npgsqlDbType.HasFlag(EDBDbType.Multirange) && (npgsqlDbType & ~EDBDbType.Multirange) == EDBDbType.Unknown
                 => DataTypeNames.Unknown,

            // If both multirange and array are set we first remove array, so array is added to the outermost datatypename.
            _ when npgsqlDbType.HasFlag(EDBDbType.Array)
                => ToDataTypeName(npgsqlDbType & ~EDBDbType.Array)?.ToArrayName(),
            _ when npgsqlDbType.HasFlag(EDBDbType.Multirange)
                => ToDataTypeName((npgsqlDbType | EDBDbType.Range) & ~EDBDbType.Multirange)?.ToDefaultMultirangeName(),

            // Plugin types don't have a stable fully qualified name.
            _ => null
        };

    internal static EDBDbType? ToEDBDbType(this DataTypeName dataTypeName) => ToEDBDbType(dataTypeName.UnqualifiedName);
    /// Should not be used with display names, first normalize it instead.
    internal static EDBDbType? ToEDBDbType(string normalizedDataTypeName)
    {
        var unqualifiedName = normalizedDataTypeName.AsSpan();
        if (unqualifiedName.IndexOf('.') is not -1 and var index)
            unqualifiedName = unqualifiedName.Slice(index + 1);

        return unqualifiedName switch
            {
                // Numeric types
                "int2" => EDBDbType.Smallint,
                "int4" => EDBDbType.Integer,
                "int8" => EDBDbType.Bigint,
                "float4" => EDBDbType.Real,
                "float8" => EDBDbType.Double,
                "numeric" => EDBDbType.Numeric,
                "money" => EDBDbType.Money,

                // Text types
                "text" => EDBDbType.Text,
                "xml" => EDBDbType.Xml,
                "varchar" => EDBDbType.Varchar,
                "bpchar" => EDBDbType.Char,
                "name" => EDBDbType.Name,
                "refcursor" => EDBDbType.Refcursor,
                "jsonb" => EDBDbType.Jsonb,
                "json" => EDBDbType.Json,
                "jsonpath" => EDBDbType.JsonPath,

                // Date/time types
                "timestamp" => EDBDbType.Timestamp,
                "timestamptz" => EDBDbType.TimestampTz,
                "date" => EDBDbType.Date,
                "time" => EDBDbType.Time,
                "timetz" => EDBDbType.TimeTz,
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

                // UInt types
                "oid" => EDBDbType.Oid,
                "xid" => EDBDbType.Xid,
                "xid8" => EDBDbType.Xid8,
                "cid" => EDBDbType.Cid,
                "regtype" => EDBDbType.Regtype,
                "regconfig" => EDBDbType.Regconfig,

                // Misc types
                "bool" => EDBDbType.Boolean,
                "bytea" => EDBDbType.Bytea,
                "uuid" => EDBDbType.Uuid,
                "varbit" => EDBDbType.Varbit,
                "bit" => EDBDbType.Bit,

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

                // Plugin types
                "citext" => EDBDbType.Citext,
                "cube" => EDBDbType.Cube,
                "lquery" => EDBDbType.LQuery,
                "ltree" => EDBDbType.LTree,
                "ltxtquery" => EDBDbType.LTxtQuery,
                "hstore" => EDBDbType.Hstore,
                "geometry" => EDBDbType.Geometry,
                "geography" => EDBDbType.Geography,

                _ when unqualifiedName.IndexOf("unknown") != -1
                    => !unqualifiedName.StartsWith("_", StringComparison.Ordinal)
                        ? EDBDbType.Unknown
                        : null,
                _ when unqualifiedName.StartsWith("_", StringComparison.Ordinal)
					=> ToEDBDbType(unqualifiedName.Slice(1).ToString()) is { } elementEDBDbType
                    //=> ToEDBDbType(unqualifiedName.Substring(1)) is { } elementEDBDbType
                        ? elementEDBDbType | EDBDbType.Array
                        : null,
                // e.g. custom ranges, plugin types etc.
                _ => null
            };
    }
}
