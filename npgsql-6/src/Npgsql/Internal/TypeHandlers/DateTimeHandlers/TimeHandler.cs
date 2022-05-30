using System;
using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL time data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class TimeHandler : EDBSimpleTypeHandler<TimeSpan>
#if NET6_0_OR_GREATER
        , IEDBSimpleTypeHandler<TimeOnly>
#endif
    {
        /// <summary>
        /// Constructs a <see cref="TimeHandler"/>.
        /// </summary>
        public TimeHandler(PostgresType postgresType) : base(postgresType) {}

        // PostgreSQL time resolution == 1 microsecond == 10 ticks
        /// <inheritdoc />
        public override TimeSpan Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new(buf.ReadInt64() * 10);

        /// <inheritdoc />
        public override int ValidateAndGetLength(TimeSpan value, EDBParameter? parameter) => 8;

        /// <inheritdoc />
        public override void Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteInt64(value.Ticks / 10);

#if NET6_0_OR_GREATER
        TimeOnly IEDBSimpleTypeHandler<TimeOnly>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => new(buf.ReadInt64() * 10);

        public int ValidateAndGetLength(TimeOnly value, EDBParameter? parameter) => 8;

        public void Write(TimeOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteInt64(value.Ticks / 10);
#endif
    }
}
