using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;

#pragma warning disable 618 // EDBDate is obsolete, remove in 7.0

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL date data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class DateHandler : EDBSimpleTypeHandlerWithPsv<DateTime, EDBDate>,
        IEDBSimpleTypeHandler<int>
#if NET6_0_OR_GREATER
        , IEDBSimpleTypeHandler<DateOnly>
#endif
    {
        /// <summary>
        /// Constructs a <see cref="DateHandler"/>
        /// </summary>
        public DateHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var npgsqlDate = ReadPsv(buf, len, fieldDescription);

            if (npgsqlDate.IsFinite)
                return (DateTime)npgsqlDate;
            if (DisableDateTimeInfinityConversions)
                throw new InvalidCastException("Can't convert infinite date values to DateTime");
            if (npgsqlDate.IsInfinity)
                return DateTime.MaxValue;
            return DateTime.MinValue;
        }

        /// <remarks>
        /// Copied wholesale from Postgresql backend/utils/adt/datetime.c:j2date
        /// </remarks>
        protected override EDBDate ReadPsv(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var binDate = buf.ReadInt32();

            return binDate switch
            {
                int.MaxValue => EDBDate.Infinity,
                int.MinValue => EDBDate.NegativeInfinity,
                _            => new EDBDate(binDate + 730119)
            };
        }

        int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => buf.ReadInt32();

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBDate value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public int ValidateAndGetLength(int value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (!DisableDateTimeInfinityConversions)
            {
                if (value == DateTime.MaxValue)
                {
                    Write(EDBDate.Infinity, buf, parameter);
                    return;
                }

                if (value == DateTime.MinValue)
                {
                    Write(EDBDate.NegativeInfinity, buf, parameter);
                    return;
                }
            }

            Write(new EDBDate(value), buf, parameter);
        }

        /// <inheritdoc />
        public override void Write(EDBDate value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (value == EDBDate.NegativeInfinity)
                buf.WriteInt32(int.MinValue);
            else if (value == EDBDate.Infinity)
                buf.WriteInt32(int.MaxValue);
            else
                buf.WriteInt32(value.DaysSinceEra - 730119);
        }

        /// <inheritdoc />
        public void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteInt32(value);

        #endregion Write

#if NET6_0_OR_GREATER
        DateOnly IEDBSimpleTypeHandler<DateOnly>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            var npgsqlDate = ReadPsv(buf, len, fieldDescription);

            if (npgsqlDate.IsFinite)
                return (DateOnly)npgsqlDate;
            if (DisableDateTimeInfinityConversions)
                throw new InvalidCastException("Can't convert infinite date values to DateOnly");
            if (npgsqlDate.IsInfinity)
                return DateOnly.MaxValue;
            return DateOnly.MinValue;
        }

        public int ValidateAndGetLength(DateOnly value, EDBParameter? parameter) => 4;

        public void Write(DateOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (!DisableDateTimeInfinityConversions)
            {
                if (value == DateOnly.MaxValue)
                {
                    Write(EDBDate.Infinity, buf, parameter);
                    return;
                }

                if (value == DateOnly.MinValue)
                {
                    Write(EDBDate.NegativeInfinity, buf, parameter);
                    return;
                }
            }

            Write(new EDBDate(value), buf, parameter);
        }

        public override EDBTypeHandler CreateRangeHandler(PostgresType pgRangeType)
            => new RangeHandler<DateTime, DateOnly>(pgRangeType, this);

        public override EDBTypeHandler CreateMultirangeHandler(PostgresMultirangeType pgRangeType)
            => new MultirangeHandler<DateTime, DateOnly>(pgRangeType, new RangeHandler<DateTime, DateOnly>(pgRangeType, this));
#endif
    }
}
