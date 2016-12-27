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
    [TypeMapping("timestamptz", EDBDbType.TimestampTZ, DbType.DateTimeOffset, typeof(DateTimeOffset))]
    internal class TimeStampTzHandler : TimeStampHandler, ISimpleTypeHandler<DateTimeOffset>
    {
        public TimeStampTzHandler(IBackendType backendType, TypeHandlerRegistry registry)
            : base(backendType, registry) {}

        public override DateTime Read(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.DateTime.ToLocalTime();
                if (!_convertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamptz values to DateTime");
                if (ts.IsInfinity)
                    return DateTime.MaxValue;
                return DateTime.MinValue;
            } catch (Exception e) {
                throw new SafeReadException(e);
            }
        }

        internal override EDBDateTime ReadPsv(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            return new EDBDateTime(ts.Date, ts.Time, DateTimeKind.Utc).ToLocalTime();
        }

        DateTimeOffset ISimpleTypeHandler<DateTimeOffset>.Read(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            try
            {
                return new DateTimeOffset(ReadTimeStamp(buf, len, fieldDescription).DateTime, TimeSpan.Zero);
            } catch (Exception e) {
                throw new SafeReadException(e);
            }
        }

        public override void Write(object value, WriteBuffer buf, EDBParameter parameter)
        {
            if (parameter != null && parameter.ConvertedValue != null) {
                value = parameter.ConvertedValue;
            }

            if (value is EDBDateTime)
            {
                var ts = (EDBDateTime)value;
                switch (ts.Kind)
                {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Utc:
                    break;
                case DateTimeKind.Local:
                    ts = ts.ToUniversalTime();
                    break;
                default:
                    throw PGUtil.ThrowIfReached();
                }
                base.Write(ts, buf, parameter);
                return;
            }

            if (value is DateTime)
            {
                var dt = (DateTime)value;
                switch (dt.Kind)
                {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Utc:
                    break;
                case DateTimeKind.Local:
                    dt = dt.ToUniversalTime();
                    break;
                default:
                    throw PGUtil.ThrowIfReached();
                }
                base.Write(dt, buf, parameter);
                return;
            }

            if (value is DateTimeOffset)
            {
                base.Write(((DateTimeOffset)value).ToUniversalTime(), buf, parameter);
                return;
            }

            throw PGUtil.ThrowIfReached();
        }
    }
}
