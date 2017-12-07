#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The  EnterpriseDB.EDBClient DEVELOPMENT Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace  EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    [TypeMapping("timestamp", EDBDbType.Timestamp, new[] { DbType.DateTime, DbType.DateTime2 }, new [] { typeof(EDBDateTime), typeof(DateTime) }, DbType.DateTime)]
    class TimeStampHandler : SimpleTypeHandlerWithPsv<DateTime, EDBDateTime>
    {
        /// <summary>
        /// A deprecated compile-time option of PostgreSQL switches to a floating-point representation of some date/time
        /// fields.  EnterpriseDB.EDBClient (currently) does not support this mode.
        /// </summary>
        readonly bool _integerFormat;

        /// <summary>
        /// Whether to convert positive and negative infinity values to DateTime.{Max,Min}Value when
        /// a DateTime is requested
        /// </summary>
        protected readonly bool ConvertInfinityDateTime;

        internal TimeStampHandler(PostgresType postgresType, TypeHandlerRegistry registry)
            : base(postgresType)
        {
            // Check for the legacy floating point timestamps feature, defaulting to integer timestamps
            _integerFormat = !registry.Connector.BackendParams.TryGetValue("integer_datetimes", out var s) || s == "on";
            ConvertInfinityDateTime = registry.Connector.ConvertInfinityDateTime;
        }

        public override DateTime Read(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            // TODO: Convert directly to DateTime without passing through NpgsqlTimeStamp?
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
                throw new SafeReadException(e);
            }
        }

        internal override EDBDateTime ReadPsv(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => ReadTimeStamp(buf, len, fieldDescription);

        protected EDBDateTime ReadTimeStamp(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            if (!_integerFormat) {
                throw new NotSupportedException("Old floating point representation for timestamps not supported");
            }

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

        public override int ValidateAndGetLength(object value, EDBParameter parameter = null)
        {
            if (!(value is DateTime) && !(value is EDBDateTime) && !(value is DateTimeOffset))
            {
                var converted = Convert.ToDateTime(value);
                if (parameter == null)
                    throw CreateConversionButNoParamException(value.GetType());
                parameter.ConvertedValue = converted;
            }
            return 8;
        }

        protected override void Write(object value, WriteBuffer buf, EDBParameter parameter = null)
        {
            if (parameter?.ConvertedValue != null)
                value = parameter.ConvertedValue;

            EDBDateTime ts;
            if (value is EDBDateTime) {
                ts = (EDBDateTime)value;
                if (!ts.IsFinite)
                {
                    if (ts.IsInfinity)
                    {
                        buf.WriteInt64(long.MaxValue);
                        return;
                    }

                    if (ts.IsNegativeInfinity)
                    {
                        buf.WriteInt64(long.MinValue);
                        return;
                    }

                    throw new InvalidOperationException("Internal  EnterpriseDB.EDBClient bug, please report.");
                }
            }
            else if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (ConvertInfinityDateTime)
                {
                    if (dt == DateTime.MaxValue)
                    {
                        buf.WriteInt64(long.MaxValue);
                        return;
                    }
                    if (dt == DateTime.MinValue)
                    {
                        buf.WriteInt64(long.MinValue);
                        return;
                    }
                }
                ts = new EDBDateTime(dt);
            }
            else if (value is DateTimeOffset)
                ts = new EDBDateTime(((DateTimeOffset)value).DateTime);
            else
                throw new InvalidOperationException("Internal  EnterpriseDB.EDBClient bug, please report.");

            var uSecsTime = ts.Time.Ticks / 10;

            if (ts >= new EDBDateTime(2000, 1, 1, 0, 0, 0))
            {
                var uSecsDate = (ts.Date.DaysSinceEra - 730119) * 86400000000L;
                buf.WriteInt64(uSecsDate + uSecsTime);
            }
            else
            {
                var uSecsDate = (730119 - ts.Date.DaysSinceEra) * 86400000000L;
                buf.WriteInt64(-(uSecsDate - uSecsTime));
            }
        }
    }
}
