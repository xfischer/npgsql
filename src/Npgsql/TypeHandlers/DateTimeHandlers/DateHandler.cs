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
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    [TypeMapping("date", EDBDbType.Date, DbType.Date, typeof(EDBDate))]
    class DateHandlerFactory : EDBTypeHandlerFactory<DateTime>
    {
        protected override EDBTypeHandler<DateTime> Create(EDBConnection conn)
            => new DateHandler(conn.Connector.ConvertInfinityDateTime);
    }

    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    class DateHandler : EDBSimpleTypeHandlerWithPsv<DateTime, EDBDate>
    {
        internal const int PostgresEpochJdate = 2451545; // == date2j(2000, 1, 1)
        internal const int MonthsPerYear = 12;

        /// <summary>
        /// Whether to convert positive and negative infinity values to DateTime.{Max,Min}Value when
        /// a DateTime is requested
        /// </summary>
        readonly bool _convertInfinityDateTime;

        public DateHandler(bool convertInfinityDateTime)
        {
            _convertInfinityDateTime = convertInfinityDateTime;
        }

        #region Read

        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
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
        protected override EDBDate ReadPsv(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            var binDate = buf.ReadInt32();

            switch (binDate)
            {
            case int.MaxValue:
                return EDBDate.Infinity;
            case int.MinValue:
                return EDBDate.NegativeInfinity;
            default:
                return new EDBDate(binDate + 730119);
            }
        }

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(DateTime value, EDBParameter parameter)
            => 4;

        public override int ValidateAndGetLength(EDBDate value, EDBParameter parameter)
            => 4;

        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter parameter)
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

        public override void Write(EDBDate value, EDBWriteBuffer buf, EDBParameter parameter)
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
