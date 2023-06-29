using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers;

/// <summary>
/// A type handler for the PostgreSQL timestamp data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class TimestampHandler : EDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<long>
#if NET6_0_OR_GREATER
    , IEDBSimpleTypeHandler<DateOnly> // EnterpriseDB Team : EPAS date data type in redwood mode serves as an emulation of the Oracle date data type. As the Oracle date type stores both date and time information, we have mapped the EPAS date data type to timestamp without timezone. 
#endif
{
    /// <summary>
    /// Constructs a <see cref="TimestampHandler"/>.
    /// </summary>
    public TimestampHandler(PostgresType postgresType) : base(postgresType) {}

    #region Read

    /// <inheritdoc />
    public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => DateTimeUtils.ReadDateTime(buf, DateTimeKind.Unspecified);

    long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => buf.ReadInt64();

    #endregion Read

    #region Write

    /// <inheritdoc />
    public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter)
        => value.Kind != DateTimeKind.Utc || LegacyTimestampBehavior
            ? 8
            : throw new InvalidCastException(
                "Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone', " +
                "consider using 'timestamp with time zone'. " +
                "Note that it's not possible to mix DateTimes with different Kinds in an array/range. " +
                "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

    /// <inheritdoc />
    public int ValidateAndGetLength(long value, EDBParameter? parameter) => 8;

    /// <inheritdoc />
    public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        => DateTimeUtils.WriteTimestamp(value, buf);

    /// <inheritdoc />
    public void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
        => buf.WriteInt64(value);

    #endregion Write

    // EnterpriseDB Team
#if NET6_0_OR_GREATER

    DateOnly IEDBSimpleTypeHandler<DateOnly>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => DateTimeUtils.ReadDateOnly(buf, len, fieldDescription);

    public int ValidateAndGetLength(DateOnly value, EDBParameter? parameter) => 4;

    public void Write(DateOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        DateTimeUtils.WriteDateOnly(value, buf, parameter);
    }

#endif
}
