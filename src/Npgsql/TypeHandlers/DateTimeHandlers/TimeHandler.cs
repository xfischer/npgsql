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
    [TypeMapping("time", EDBDbType.Time, new[] { DbType.Time })]
    class TimeHandler : SimpleTypeHandler<TimeSpan>
    {
        /// <summary>
        /// A deprecated compile-time option of PostgreSQL switches to a floating-point representation of some date/time
        /// fields.  EnterpriseDB.EDBClient (currently) does not support this mode.
        /// </summary>
        readonly bool _integerFormat;

        public TimeHandler(PostgresType postgresType, TypeHandlerRegistry registry)
            : base(postgresType)
        {
            // Check for the legacy floating point timestamps feature, defaulting to integer timestamps
            _integerFormat = !registry.Connector.BackendParams.TryGetValue("integer_datetimes", out var s) || s == "on";
        }

        public override TimeSpan Read(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            if (!_integerFormat)
                throw new NotSupportedException("Old floating point representation for timestamps not supported");

            // PostgreSQL time resolution == 1 microsecond == 10 ticks
            return new TimeSpan(buf.ReadInt64() * 10);
        }

        public override int ValidateAndGetLength(object value, EDBParameter parameter = null)
        {
            var asString = value as string;
            if (asString != null)
            {
                var converted = TimeSpan.Parse(asString);
                if (parameter == null)
                    throw CreateConversionButNoParamException(value.GetType());
                parameter.ConvertedValue = converted;
            }
            else if (!(value is TimeSpan))
                throw CreateConversionException(value.GetType());
            return 8;
        }

        protected override void Write(object value, WriteBuffer buf, EDBParameter parameter = null)
        {
            if (parameter?.ConvertedValue != null)
                value = parameter.ConvertedValue;

            buf.WriteInt64(((TimeSpan)value).Ticks / 10);
        }
    }
}
