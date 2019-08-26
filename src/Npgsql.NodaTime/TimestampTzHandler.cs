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
using NodaTime;
using NodaTime.TimeZones;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class TimestampTzHandlerFactory : EDBTypeHandlerFactory<Instant>
    {
        // Check for the legacy floating point timestamps feature
        protected override EDBTypeHandler<Instant> Create(EDBConnection conn)
            => new TimestampTzHandler(conn);
    }

    class TimestampTzHandler : EDBSimpleTypeHandler<Instant>, IEDBSimpleTypeHandler<ZonedDateTime>,
        IEDBSimpleTypeHandler<OffsetDateTime>
    {
        readonly IDateTimeZoneProvider _dateTimeZoneProvider;

        /// <summary>
        /// A deprecated compile-time option of PostgreSQL switches to a floating-point representation of some date/time
        /// fields. EDB (currently) does not support this mode.
        /// </summary>
        readonly bool _integerFormat;

        public TimestampTzHandler(EDBConnection conn)
        {
            _integerFormat = conn.HasIntegerDateTimes;
            _dateTimeZoneProvider = DateTimeZoneProviders.Tzdb;
        }

        #region Read

        public override Instant Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            if (_integerFormat) {
                var value = buf.ReadInt64();
                if (value == long.MaxValue || value == long.MinValue)
                    throw new EDBSafeReadException(new NotSupportedException("Infinity values not supported for timestamp with time zone"));
                return TimestampHandler.Decode(value);
            }
            else
            {
                var value = buf.ReadDouble();
                if (double.IsPositiveInfinity(value) || double.IsNegativeInfinity(value))
                    throw new EDBSafeReadException(new NotSupportedException("Infinity values not supported for timestamp with time zone"));
                return TimestampHandler.Decode(value);
            }
        }

        ZonedDateTime IEDBSimpleTypeHandler<ZonedDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            try
            {
                if (_integerFormat)
                {
                    var value = buf.ReadInt64();
                    if (value == long.MaxValue || value == long.MinValue)
                        throw new EDBSafeReadException(new NotSupportedException("Infinity values not supported for timestamp with time zone"));
                    return TimestampHandler.Decode(value).InZone(_dateTimeZoneProvider[buf.Connection.Timezone]);
                }
                else
                {
                    var value = buf.ReadDouble();
                    if (double.IsPositiveInfinity(value) || double.IsNegativeInfinity(value))
                        throw new EDBSafeReadException(new NotSupportedException("Infinity values not supported for timestamp with time zone"));
                    return TimestampHandler.Decode(value).InZone(_dateTimeZoneProvider[buf.Connection.Timezone]);
                }
            }
            catch (TimeZoneNotFoundException) when (string.Equals(buf.Connection.Timezone, "localtime", StringComparison.OrdinalIgnoreCase))
            {
                throw new EDBSafeReadException(
                    new TimeZoneNotFoundException(
                        "The special PostgreSQL timezone 'localtime' is not supported when reading values of type 'timestamp with time zone'. " +
                        "Please specify a real timezone in 'postgresql.conf' on the server, or set the 'PGTZ' environment variable on the client."));
            }
            catch (TimeZoneNotFoundException e)
            {
                throw new EDBSafeReadException(e);
            }
        }

        OffsetDateTime IEDBSimpleTypeHandler<OffsetDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription)
            => ((IEDBSimpleTypeHandler<ZonedDateTime>)this).Read(buf, len, fieldDescription).ToOffsetDateTime();

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(Instant value, EDBParameter parameter)
            => 8;

        int IEDBSimpleTypeHandler<ZonedDateTime>.ValidateAndGetLength(ZonedDateTime value, EDBParameter parameter)
            => 8;

        public int ValidateAndGetLength(OffsetDateTime value, EDBParameter parameter)
            => 8;

        public override void Write(Instant value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            if (_integerFormat)
                TimestampHandler.WriteInteger(value, buf);
            else
                TimestampHandler.WriteDouble(value, buf);
        }

        void IEDBSimpleTypeHandler<ZonedDateTime>.Write(ZonedDateTime value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write(value.ToInstant(), buf, parameter);

        public void Write(OffsetDateTime value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write(value.ToInstant(), buf, parameter);

        #endregion Write
    }
}
