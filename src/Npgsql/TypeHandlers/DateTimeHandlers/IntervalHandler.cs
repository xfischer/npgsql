#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
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
    [TypeMapping("interval", EDBDbType.Interval, typeof(EDBTimeSpan))]
    internal class IntervalHandler : TypeHandlerWithPsv<TimeSpan, EDBTimeSpan>,
        ISimpleTypeReader<TimeSpan>, ISimpleTypeReader<EDBTimeSpan>, ISimpleTypeWriter
    {
        /// <summary>
        /// A deprecated compile-time option of PostgreSQL switches to a floating-point representation of some date/time
        /// fields.  EnterpriseDB.EDBClient (currently) does not support this mode.
        /// </summary>
        readonly bool _integerFormat;

        public IntervalHandler(TypeHandlerRegistry registry)
        {
            _integerFormat = registry.Connector.BackendParams["integer_datetimes"] == "on";
        }

        public TimeSpan Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return (TimeSpan)((ISimpleTypeReader<EDBTimeSpan>)this).Read(buf, len, fieldDescription);
        }

        EDBTimeSpan ISimpleTypeReader<EDBTimeSpan>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            if (!_integerFormat) {
                throw new NotSupportedException("Old floating point representation for timestamps not supported");
            }
            var ticks = buf.ReadInt64();
            var day = buf.ReadInt32();
            var month = buf.ReadInt32();
            return new EDBTimeSpan(month, day, ticks * 10);
        }

        public int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            if (!_integerFormat) {
                throw new NotSupportedException("Old floating point representation for timestamps not supported");
            }

            var asString = value as string;
            if (asString != null)
            {
                var converted = EDBTimeSpan.Parse(asString);
                if (parameter == null) {
                    throw CreateConversionButNoParamException(value.GetType());
                }
                parameter.ConvertedValue = converted;
            }
            else if (!(value is TimeSpan) && !(value is EDBTimeSpan))
            {
                throw CreateConversionException(value.GetType());
            }

            return 16;
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            if (parameter != null && parameter.ConvertedValue != null) {
                value = parameter.ConvertedValue;
            }

            var interval = (value is TimeSpan)
                ? ((EDBTimeSpan)(TimeSpan)value)
                : ((EDBTimeSpan)value);

            buf.WriteInt64(interval.Ticks / 10); // TODO: round?
            buf.WriteInt32(interval.Days);
            buf.WriteInt32(interval.Months);
        }
    }
}
