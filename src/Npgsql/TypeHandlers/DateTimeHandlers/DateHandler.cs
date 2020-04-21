using System;
using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A factory for type handlers for the PostgreSQL date data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("date", EDBDbType.Date, DbType.Date, typeof(EDBDate))]
    public class DateHandlerFactory : EDBTypeHandlerFactory<DateTime>
    {
        /// <inheritdoc />
        public override EDBTypeHandler<DateTime> Create(PostgresType postgresType, EDBConnection conn)
            => new DateHandler(postgresType, conn.Connector!.ConvertInfinityDateTime);
    }

    /// <summary>
    /// A type handler for the PostgreSQL date data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public class DateHandler : EDBSimpleTypeHandlerWithPsv<DateTime, EDBDate>
    {
        /// <summary>
        /// Whether to convert positive and negative infinity values to DateTime.{Max,Min}Value when
        /// a DateTime is requested
        /// </summary>
        readonly bool _convertInfinityDateTime;

        internal DateHandler(PostgresType postgresType, bool convertInfinityDateTime)
            : base(postgresType)
            => _convertInfinityDateTime = convertInfinityDateTime;

        #region Read

        /// <inheritdoc />
        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var EDBDate = ReadPsv(buf, len, fieldDescription);
            try {
                if (EDBDate.IsFinite)
                    return (DateTime)EDBDate;
                if (!_convertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite date values to DateTime");
                if (EDBDate.IsInfinity)
                    return DateTime.MaxValue;
                return DateTime.MinValue;
            } catch (Exception e) {
                throw new EDBSafeReadException(e);
            }
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

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBDate value, EDBParameter? parameter) => 4;

        /// <inheritdoc />
        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            EDBDate value2;
            if (_convertInfinityDateTime)
            {
                if (value == DateTime.MaxValue)
                    value2 = EDBDate.Infinity;
                else if (value == DateTime.MinValue)
                    value2 = EDBDate.NegativeInfinity;
                else
                    value2 = new EDBDate(value);
            }
            else
                value2 = new EDBDate(value);

            Write(value2, buf, parameter);
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

        #endregion Write
    }
}
