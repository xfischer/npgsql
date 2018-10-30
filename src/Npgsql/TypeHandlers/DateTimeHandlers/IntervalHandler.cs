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
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    [TypeMapping("interval", EDBDbType.Interval, new[] { typeof(TimeSpan), typeof(EDBTimeSpan) })]
    class IntervalHandlerFactory : EDBTypeHandlerFactory<TimeSpan>
    {
        // Check for the legacy floating point timestamps feature
        protected override EDBTypeHandler<TimeSpan> Create(EDBConnection conn)
            => new IntervalHandler(conn.HasIntegerDateTimes);
    }

    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    class IntervalHandler : EDBSimpleTypeHandlerWithPsv<TimeSpan, EDBTimeSpan>
    {
        /// <summary>
        /// A deprecated compile-time option of PostgreSQL switches to a floating-point representation of some date/time
        /// fields. Some PostgreSQL-like databases (e.g. CrateDB) use floating-point representation by default and do not
        /// provide the option of switching to integer format.
        /// </summary>
        readonly bool _integerFormat;

        public IntervalHandler(bool integerFormat)
        {
            _integerFormat = integerFormat;
        }

        public override TimeSpan Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => (TimeSpan)((IEDBSimpleTypeHandler<EDBTimeSpan>)this).Read(buf, len, fieldDescription);

        protected override EDBTimeSpan ReadPsv(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            if (_integerFormat)
            {
                var ticks = buf.ReadInt64();
                var day = buf.ReadInt32();
                var month = buf.ReadInt32();
                return new EDBTimeSpan(month, day, ticks * 10);
            }
            else
            {
                var seconds = buf.ReadDouble();
                var day = buf.ReadInt32();
                var month = buf.ReadInt32();
                return new EDBTimeSpan(month, day, (long)(seconds * TimeSpan.TicksPerSecond));
            }
        }

        public override int ValidateAndGetLength(TimeSpan value, EDBParameter parameter)
            => 16;

        public override int ValidateAndGetLength(EDBTimeSpan value, EDBParameter parameter)
            => 16;

        public override void Write(EDBTimeSpan value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            if (_integerFormat)
                buf.WriteInt64(value.Ticks / 10); // TODO: round?
            else
                buf.WriteDouble(value.TotalSeconds - (value.Days * 86400) - (value.Months * EDBTimeSpan.DaysPerMonth * 86400));

            buf.WriteInt32(value.Days);
            buf.WriteInt32(value.Months);
        }

        // TODO: Can write directly from TimeSpan
        public override void Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write(value, buf, parameter);
    }
}
