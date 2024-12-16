using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using EnterpriseDB.EDBClient.Internal;
using EDBTypes;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

public class IntervalConverter : PgStreamingConverter<Interval>
{
    readonly PgConverter<EDBRange<Instant>> _rangeConverter;

    public IntervalConverter(PgConverter<EDBRange<Instant>> rangeConverter)
        => _rangeConverter = rangeConverter;

    public override Interval Read(PgReader reader)
        => Read(async: false, reader, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask<Interval> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        => Read(async: true, reader, cancellationToken);

    async ValueTask<Interval> Read(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        var range = async
            ? await _rangeConverter.ReadAsync(reader, cancellationToken).ConfigureAwait(false)
            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
            : _rangeConverter.Read(reader);

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

    public override Size GetSize(SizeContext context, Interval value, ref object? writeState)
        => _rangeConverter.GetSize(context, IntervalToEDBRange(value), ref writeState);

    public override void Write(PgWriter writer, Interval value)
        => _rangeConverter.Write(writer, IntervalToEDBRange(value));

    public override ValueTask WriteAsync(PgWriter writer, Interval value, CancellationToken cancellationToken = default)
        => _rangeConverter.WriteAsync(writer, IntervalToEDBRange(value), cancellationToken);

    static EDBRange<Instant> IntervalToEDBRange(Interval interval)
        => new(
            interval.HasStart ? interval.Start : default, true, !interval.HasStart,
            interval.HasEnd ? interval.End : default, false, !interval.HasEnd);
}
