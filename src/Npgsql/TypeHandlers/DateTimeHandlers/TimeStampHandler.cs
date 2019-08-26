#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    [TypeMapping("timestamp", EDBDbType.Timestamp, new[] { DbType.DateTime, DbType.DateTime2 }, new[] { typeof(EDBDateTime), typeof(DateTime) }, DbType.DateTime)]
    class TimestampHandlerFactory : EDBTypeHandlerFactory<DateTime>
    {
        // Check for the legacy floating point timestamps feature
        protected override EDBTypeHandler<DateTime> Create(EDBConnection conn)
            => new TimestampHandler(conn.HasIntegerDateTimes, conn.Connector.ConvertInfinityDateTime);
    }

    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    class TimestampHandler : EDBSimpleTypeHandlerWithPsv<DateTime, EDBDateTime>
    {
        internal const uint TypeOID = 1114;

        /// <summary>
        /// A deprecated compile-time option of PostgreSQL switches to a floating-point representation of some date/time
        /// fields. Some PostgreSQL-like databases (e.g. CrateDB) use floating-point representation by default and do not
        /// provide the option of switching to integer format.
        /// </summary>
        readonly bool _integerFormat;

        /// <summary>
        /// Whether to convert positive and negative infinity values to DateTime.{Max,Min}Value when
        /// a DateTime is requested
        /// </summary>
        protected readonly bool ConvertInfinityDateTime;

        internal TimestampHandler(bool integerFormat, bool convertInfinityDateTime)
        {
            // Check for the legacy floating point timestamps feature, defaulting to integer timestamps
            _integerFormat = integerFormat;
            ConvertInfinityDateTime = convertInfinityDateTime;
        }

        #region Read

        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.ToDateTime();
                if (!ConvertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamp values to DateTime");
                if (ts.IsInfinity)
                    return DateTime.MaxValue;
                return DateTime.MinValue;
            }
            catch (Exception e)
            {
                throw new EDBSafeReadException(e);
            }
        }

        protected override EDBDateTime ReadPsv(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => ReadTimeStamp(buf, len, fieldDescription);

#pragma warning disable CA1801 // Review unused parameters
        protected EDBDateTime ReadTimeStamp(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => _integerFormat
                ? ReadInteger(buf)
                : ReadDouble(buf);
#pragma warning restore CA1801 // Review unused parameters

        EDBDateTime ReadInteger(EDBReadBuffer buf)
        {
            var value = buf.ReadInt64();
            if (value == long.MaxValue)
                return EDBDateTime.Infinity;
            if (value == long.MinValue)
                return EDBDateTime.NegativeInfinity;
            if (value >= 0) {
                var date = (int)(value / 86400000000L);
                var time = value % 86400000000L;

                date += 730119; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= 10; // To 100ns

                return new EDBDateTime(new EDBDate(date), new TimeSpan(time));
            } else {
                value = -value;
                var date = (int)(value / 86400000000L);
                var time = value % 86400000000L;
                if (time != 0) {
                    ++date;
                    time = 86400000000L - time;
                }
                date = 730119 - date; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= 10; // To 100ns

                return new EDBDateTime(new EDBDate(date), new TimeSpan(time));
            }
        }

        EDBDateTime ReadDouble(EDBReadBuffer buf)
        {
            var value = buf.ReadDouble();
            if (double.IsPositiveInfinity(value))
                return EDBDateTime.Infinity;
            if (double.IsNegativeInfinity(value))
                return EDBDateTime.NegativeInfinity;
            if (value >= 0d) {
                var date = (int)(value / 86400d);
                var time = value % 86400d;

                date += 730119; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= TimeSpan.TicksPerSecond; // seconds to Ticks

                return new EDBDateTime(new EDBDate(date), new TimeSpan((long)time));
            } else {
                value = -value;
                var date = (int)(value / 86400d);
                var time = value % 86400d;
                if (time != 0d)
                {
                    ++date;
                    time = 86400d - time;
                }

                date = 730119 - date; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= TimeSpan.TicksPerSecond; // seconds to Ticks

                return new EDBDateTime(new EDBDate(date), new TimeSpan((long)time));
            }
        }

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(DateTime value, EDBParameter parameter)
            => 8;

        public override int ValidateAndGetLength(EDBDateTime value, EDBParameter parameter)
            => 8;

        public override void Write(EDBDateTime value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            if (_integerFormat)
                WriteInteger(value, buf);
            else
                WriteDouble(value, buf);
        }

        void WriteInteger(EDBDateTime value, EDBWriteBuffer buf)
        {
            if (value.IsInfinity)
            {
                buf.WriteInt64(long.MaxValue);
                return;
            }

            if (value.IsNegativeInfinity)
            {
                buf.WriteInt64(long.MinValue);
                return;
            }

            var uSecsTime = value.Time.Ticks / 10;

            if (value >= new EDBDateTime(2000, 1, 1, 0, 0, 0))
            {
                var uSecsDate = (value.Date.DaysSinceEra - 730119) * 86400000000L;
                buf.WriteInt64(uSecsDate + uSecsTime);
            }
            else
            {
                var uSecsDate = (730119 - value.Date.DaysSinceEra) * 86400000000L;
                buf.WriteInt64(-(uSecsDate - uSecsTime));
            }
        }

        void WriteDouble(EDBDateTime value, EDBWriteBuffer buf)
        {
            if (value.IsInfinity)
            {
                buf.WriteDouble(double.PositiveInfinity);
                return;
            }

            if (value.IsNegativeInfinity)
            {
                buf.WriteDouble(double.NegativeInfinity);
                return;
            }

            var dSecsTime = value.Time.TotalSeconds;

            if (value >= new EDBDateTime(2000, 1, 1, 0, 0, 0))
            {
                var dSecsDate = (value.Date.DaysSinceEra - 730119d) * 86400d;
                buf.WriteDouble(dSecsDate + dSecsTime);
            }
            else
            {
                var dSecsDate = (730119d - value.Date.DaysSinceEra) * 86400d;
                buf.WriteDouble(-(dSecsDate - dSecsTime));
            }
        }

        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            if (ConvertInfinityDateTime)
            {
                if (value == DateTime.MaxValue)
                {
                    if (_integerFormat)
                        buf.WriteInt64(long.MaxValue);
                    else
                        buf.WriteDouble(double.PositiveInfinity);
                    return;
                }
                if (value == DateTime.MinValue)
                {
                    if (_integerFormat)
                        buf.WriteInt64(long.MinValue);
                    else
                        buf.WriteDouble(double.NegativeInfinity);
                    return;
                }
            }
            Write(new EDBDateTime(value), buf, parameter);
        }

        #endregion Write
    }
}
