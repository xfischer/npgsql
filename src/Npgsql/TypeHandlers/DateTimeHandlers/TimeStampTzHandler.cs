#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    [TypeMapping("timestamp with time zone", EDBDbType.TimestampTz, DbType.DateTimeOffset, typeof(DateTimeOffset))]
    class TimestampTzHandlerFactory : EDBTypeHandlerFactory<DateTime>
    {
        // Check for the legacy floating point timestamps feature
        protected override EDBTypeHandler<DateTime> Create(EDBConnection conn)
            => new TimestampTzHandler(conn.HasIntegerDateTimes, conn.Connector.ConvertInfinityDateTime);
    }

    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    class TimestampTzHandler : TimestampHandler, IEDBSimpleTypeHandler<DateTimeOffset>
    {
        public TimestampTzHandler(bool integerFormat, bool convertInfinityDateTime)
            : base(integerFormat, convertInfinityDateTime) {}

        #region Read

        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.ToDateTime().ToLocalTime();
                if (!ConvertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamptz values to DateTime");
                if (ts.IsInfinity)
                    return DateTime.MaxValue;
                return DateTime.MinValue;
            } catch (Exception e) {
                throw new EDBSafeReadException(e);
            }
        }

        protected override EDBDateTime ReadPsv(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            return new EDBDateTime(ts.Date, ts.Time, DateTimeKind.Utc).ToLocalTime();
        }

        DateTimeOffset IEDBSimpleTypeHandler<DateTimeOffset>.Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.ToDateTime().ToLocalTime();
                if (!ConvertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamptz values to DateTime");
                if (ts.IsInfinity)
                    return DateTimeOffset.MaxValue;
                return DateTimeOffset.MinValue;
            } catch (Exception e) {
                throw new EDBSafeReadException(e);
            }
        }

        #endregion Read

        #region Write

        public int ValidateAndGetLength(DateTimeOffset value, EDBParameter parameter)
            => 8;

        public override void Write(EDBDateTime value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            switch (value.Kind)
            {
            case DateTimeKind.Unspecified:
            case DateTimeKind.Utc:
                break;
            case DateTimeKind.Local:
                value = value.ToUniversalTime();
                break;
            default:
                throw new InvalidOperationException($"Internal EnterpriseDB.EDBClient bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
            }
            base.Write(value, buf, parameter);
        }

        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            switch (value.Kind)
            {
            case DateTimeKind.Unspecified:
            case DateTimeKind.Utc:
                break;
            case DateTimeKind.Local:
                value = value.ToUniversalTime();
                break;
            default:
                throw new InvalidOperationException($"Internal EnterpriseDB.EDBClient bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
            }
            base.Write(value, buf, parameter);
        }

        public void Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter parameter)
            => base.Write(value.ToUniversalTime().DateTime, buf, parameter);

        #endregion Write
    }
}
