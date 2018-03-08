#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EnterpriseDB.EDBClient Development Team
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
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    [TypeMapping("date", EDBDbType.Date, DbType.Date, typeof(EDBDate))]
    class DateHandler : SimpleTypeHandlerWithPsv<DateTime, EDBDate>
    {
        internal const int PostgresEpochJdate = 2451545; // == date2j(2000, 1, 1)
        internal const int MonthsPerYear = 12;

        /// <summary>
        /// Whether to convert positive and negative infinity values to DateTime.{Max,Min}Value when
        /// a DateTime is requested
        /// </summary>
        readonly bool _convertInfinityDateTime;

        public DateHandler(PostgresType postgresType, TypeHandlerRegistry registry)
            : base(postgresType)
        {
            _convertInfinityDateTime = registry.Connector.ConvertInfinityDateTime;
        }

        public override DateTime Read(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            // TODO: Convert directly to DateTime without passing through EDBDate?
            var EDBDate = ((ISimpleTypeHandler<EDBDate>) this).Read(buf, len, fieldDescription);
            try {
                if (EDBDate.IsFinite)
                    return (DateTime)EDBDate;
                if (!_convertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite date values to DateTime");
                if (EDBDate.IsInfinity)
                    return DateTime.MaxValue;
                return DateTime.MinValue;
            } catch (Exception e) {
                throw new SafeReadException(e);
            }
        }

        /// <remarks>
        /// Copied wholesale from Postgresql backend/utils/adt/datetime.c:j2date
        /// </remarks>
        internal override EDBDate ReadPsv(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
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

        public override int ValidateAndGetLength(object value, [CanBeNull] EDBParameter parameter)
        {
            if (!(value is DateTime) && !(value is EDBDate))
            {
                var converted = Convert.ToDateTime(value);
                if (parameter == null)
                    throw CreateConversionButNoParamException(value.GetType());
                parameter.ConvertedValue = converted;
            }
            return 4;
        }

        protected override void Write(object value, WriteBuffer buf, [CanBeNull] EDBParameter parameter)
        {
            if (parameter?.ConvertedValue != null)
                value = parameter.ConvertedValue;

            EDBDate date;
            if (value is EDBDate)
                date = (EDBDate)value;
            else if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (_convertInfinityDateTime)
                {
                    if (dt == DateTime.MaxValue)
                        date = EDBDate.Infinity;
                    else if (dt == DateTime.MinValue)
                        date = EDBDate.NegativeInfinity;
                    else
                        date = new EDBDate(dt);
                }
                else
                    date = new EDBDate(dt);
            }
            else
                throw new InvalidOperationException("Internal EnterpriseDB.EDBClient bug, please report.");

            if (date == EDBDate.NegativeInfinity)
                buf.WriteInt32(int.MinValue);
            else if (date == EDBDate.Infinity)
                buf.WriteInt32(int.MaxValue);
            else
                buf.WriteInt32(date.DaysSinceEra - 730119);
        }
    }
}
