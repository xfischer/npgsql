using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.NodaTime.Internal
{
    public partial class TimestampTzRangeHandler : RangeHandler<Instant>,
        IEDBTypeHandler<Interval>, IEDBTypeHandler<EDBRange<ZonedDateTime>>, IEDBTypeHandler<EDBRange<OffsetDateTime>>,
        IEDBTypeHandler<EDBRange<DateTime>>, IEDBTypeHandler<EDBRange<DateTimeOffset>>
    {
        public override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(Interval);
        public override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(Interval);

        public TimestampTzRangeHandler(PostgresType rangePostgresType, EDBTypeHandler subtypeHandler)
            : base(rangePostgresType, subtypeHandler)
        {
        }

        public override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async,
            FieldDescription? fieldDescription = null)
            => (await Read<Interval>(buf, len, async, fieldDescription))!;

        // internal Interval ConvertRangetoInterval(EDBRange<Instant> range)
        async ValueTask<Interval> IEDBTypeHandler<Interval>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        {
            var range = await Read(buf, len, async, fieldDescription);

            // NodaTime Interval includes the start instant and excludes the end instant.
            Instant? start = range.LowerBoundInfinite
                ? null
                : range.LowerBoundIsInclusive
                    ? range.LowerBound
                    : range.LowerBound + Duration.Epsilon;
            Instant? end = range.UpperBoundInfinite
                ? null
                : range.UpperBoundIsInclusive
                    ? range.UpperBound + Duration.Epsilon
                    : range.UpperBound;
            return new(start, end);
        }

        public int ValidateAndGetLength(Interval value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(
                new EDBRange<Instant>(value.Start, true, !value.HasStart, value.End, false, !value.HasEnd), ref lengthCache, parameter);

        public Task Write(Interval value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
            EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => WriteRange(new EDBRange<Instant>(value.Start, true, !value.HasStart, value.End, false, !value.HasEnd),
                buf, lengthCache, parameter, async, cancellationToken);

        #region Boilerplate

        ValueTask<EDBRange<ZonedDateTime>> IEDBTypeHandler<EDBRange<ZonedDateTime>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadRange<ZonedDateTime>(buf, len, async, fieldDescription);

        ValueTask<EDBRange<OffsetDateTime>> IEDBTypeHandler<EDBRange<OffsetDateTime>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadRange<OffsetDateTime>(buf, len, async, fieldDescription);

        ValueTask<EDBRange<DateTime>> IEDBTypeHandler<EDBRange<DateTime>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadRange<DateTime>(buf, len, async, fieldDescription);

        ValueTask<EDBRange<DateTimeOffset>> IEDBTypeHandler<EDBRange<DateTimeOffset>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadRange<DateTimeOffset>(buf, len, async, fieldDescription);

        public int ValidateAndGetLength(EDBRange<ZonedDateTime> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBRange<OffsetDateTime> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBRange<DateTime> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBRange<DateTimeOffset> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

        public Task Write(EDBRange<ZonedDateTime> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
            EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);

        public Task Write(EDBRange<OffsetDateTime> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
            EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);

        public Task Write(EDBRange<DateTime> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
            EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);

        public Task Write(EDBRange<DateTimeOffset> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
            EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);

        #endregion Boilerplate
    }
}
