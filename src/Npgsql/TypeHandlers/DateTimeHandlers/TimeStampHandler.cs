#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The  EnterpriseDB.EDBClient Development Team
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
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;

namespace  EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    [TypeMapping("timestamp", EDBDbType.Timestamp, new[] { DbType.DateTime, DbType.DateTime2 }, new [] { typeof(EDBDateTime), typeof(DateTime) }, DbType.DateTime)]
    internal class TimeStampHandler : SimpleTypeHandlerWithPsv<DateTime, EDBDateTime>
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
        protected readonly bool _convertInfinityDateTime;

        internal TimeStampHandler(IBackendType backendType, TypeHandlerRegistry registry)
            : base(backendType)
        {
            // Check for the legacy floating point timestamps feature, defaulting to integer timestamps
            string s;
            _integerFormat = !registry.Connector.BackendParams.TryGetValue("integer_datetimes", out s) || s == "on";
            _convertInfinityDateTime = registry.Connector.ConvertInfinityDateTime;
        }

        public override DateTime Read(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.DateTime;
                if (!_convertInfinityDateTime)
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

        internal override EDBDateTime ReadPsv(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            return ReadTimeStamp(buf, len, fieldDescription);
        }

        protected EDBDateTime ReadTimeStamp(ReadBuffer buf, int len, FieldDescription fieldDescription)
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
                int date = (int)(value / 86400000000L);
                long time = value % 86400000000L;

                date += 730119; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= 10; // To 100ns

                return new EDBDateTime(new EDBDate(date), new TimeSpan(time));
            } else {
                value = -value;
                int date = (int)(value / 86400000000L);
                long time = value % 86400000000L;
                if (time != 0) {
                    ++date;
                    time = 86400000000L - time;
                }
                date = 730119 - date; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= 10; // To 100ns

                return new EDBDateTime(new EDBDate(date), new TimeSpan(time));
            }
        }

        public override int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            if (!(value is DateTime) && !(value is EDBDateTime) && !(value is DateTimeOffset))
            {
                var converted = Convert.ToDateTime(value);
                if (parameter == null)
                {
                    throw CreateConversionButNoParamException(value.GetType());
                }
                parameter.ConvertedValue = converted;
            }
            return 8;
        }

        public override void Write(object value, WriteBuffer buf, EDBParameter parameter)
        {
            if (parameter != null && parameter.ConvertedValue != null) {
                value = parameter.ConvertedValue;
            }

            EDBDateTime ts;
            if (value is EDBDateTime) {
                ts = (EDBDateTime)value;
                if (!ts.IsFinite)
                {
                    if (ts.IsInfinity)
                    {
                        buf.WriteInt64(Int64.MaxValue);
                        return;
                    }

                    if (ts.IsNegativeInfinity)
                    {
                        buf.WriteInt64(Int64.MinValue);
                        return;
                    }

                    throw PGUtil.ThrowIfReached();
                }
            }
            else if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (_convertInfinityDateTime)
                {
                    if (dt == DateTime.MaxValue)
                    {
                        buf.WriteInt64(Int64.MaxValue);
                        return;
                    }
                    else if (dt == DateTime.MinValue)
                    {
                        buf.WriteInt64(Int64.MinValue);
                        return;
                    }
                }
                ts = new EDBDateTime(dt);
            }
            else if (value is DateTimeOffset)
            {
                ts = new EDBDateTime(((DateTimeOffset)value).DateTime);
            }
            else
            {
                throw PGUtil.ThrowIfReached();
            }

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
