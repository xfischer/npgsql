using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

public partial class TimestampTzMultirangeHandler : MultirangeHandler<Instant>,
    IEDBTypeHandler<Interval[]>, IEDBTypeHandler<List<Interval>>,
    IEDBTypeHandler<EDBRange<ZonedDateTime>[]>, IEDBTypeHandler<List<EDBRange<ZonedDateTime>>>,
    IEDBTypeHandler<EDBRange<OffsetDateTime>[]>, IEDBTypeHandler<List<EDBRange<OffsetDateTime>>>,
    IEDBTypeHandler<EDBRange<DateTime>[]>, IEDBTypeHandler<List<EDBRange<DateTime>>>,
    IEDBTypeHandler<EDBRange<DateTimeOffset>[]>, IEDBTypeHandler<List<EDBRange<DateTimeOffset>>>
{
    readonly IEDBTypeHandler<Interval> _intervalHandler;

    public override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(Interval[]);
    public override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(Interval[]);

    public TimestampTzMultirangeHandler(PostgresMultirangeType pgMultirangeType, TimestampTzRangeHandler rangeHandler)
        : base(pgMultirangeType, rangeHandler)
        => _intervalHandler = rangeHandler;

    public override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async,
        FieldDescription? fieldDescription = null)
        => (await Read<Interval[]>(buf, len, async, fieldDescription))!;

    async ValueTask<Interval[]> IEDBTypeHandler<Interval[]>.Read(EDBReadBuffer buf, int len, bool async,
        FieldDescription? fieldDescription)
    {
        await buf.Ensure(4, async);
        var numRanges = buf.ReadInt32();
        var multirange = new Interval[numRanges];

        for (var i = 0; i < multirange.Length; i++)
        {
            await buf.Ensure(4, async);
            var rangeLen = buf.ReadInt32();
            multirange[i] = await _intervalHandler.Read(buf, rangeLen, async, fieldDescription);
        }

        return multirange;
    }

    async ValueTask<List<Interval>> IEDBTypeHandler<List<Interval>>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
    {
        await buf.Ensure(4, async);
        var numRanges = buf.ReadInt32();
        var multirange = new List<Interval>(numRanges);

        for (var i = 0; i < numRanges; i++)
        {
            await buf.Ensure(4, async);
            var rangeLen = buf.ReadInt32();
            multirange.Add(await _intervalHandler.Read(buf, rangeLen, async, fieldDescription));
        }

        return multirange;
    }

    public int ValidateAndGetLength(List<Interval> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthCore(value, ref lengthCache);

    public int ValidateAndGetLength(Interval[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthCore(value, ref lengthCache);

    int ValidateAndGetLengthCore(IList<Interval> value, ref EDBLengthCache? lengthCache)
    {
        lengthCache ??= new EDBLengthCache(1);
        if (lengthCache.IsPopulated)
            return lengthCache.Get();

        var sum = 4 + 4 * value.Count;
        for (var i = 0; i < value.Count; i++)
            sum += _intervalHandler.ValidateAndGetLength(value[i], ref lengthCache, parameter: null);

        return lengthCache!.Set(sum);
    }

    public async Task Write(Interval[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
    {
        if (buf.WriteSpaceLeft < 4)
            await buf.Flush(async, cancellationToken);

        buf.WriteInt32(value.Length);

        for (var i = 0; i < value.Length; i++)
            await RangeHandler.WriteWithLength(value[i], buf, lengthCache, parameter: null, async, cancellationToken);
    }

    public async Task Write(List<Interval> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
    {
        if (buf.WriteSpaceLeft < 4)
            await buf.Flush(async, cancellationToken);

        buf.WriteInt32(value.Count);

        for (var i = 0; i < value.Count; i++)
            await RangeHandler.WriteWithLength(value[i], buf, lengthCache, parameter: null, async, cancellationToken);
    }

    #region Boilerplate

    ValueTask<EDBRange<ZonedDateTime>[]> IEDBTypeHandler<EDBRange<ZonedDateTime>[]>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeArray<ZonedDateTime>(buf, len, async, fieldDescription);

    ValueTask<List<EDBRange<ZonedDateTime>>> IEDBTypeHandler<List<EDBRange<ZonedDateTime>>>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeList<ZonedDateTime>(buf, len, async, fieldDescription);

    ValueTask<EDBRange<OffsetDateTime>[]> IEDBTypeHandler<EDBRange<OffsetDateTime>[]>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeArray<OffsetDateTime>(buf, len, async, fieldDescription);

    ValueTask<List<EDBRange<OffsetDateTime>>> IEDBTypeHandler<List<EDBRange<OffsetDateTime>>>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeList<OffsetDateTime>(buf, len, async, fieldDescription);

    ValueTask<EDBRange<DateTime>[]> IEDBTypeHandler<EDBRange<DateTime>[]>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeArray<DateTime>(buf, len, async, fieldDescription);

    ValueTask<List<EDBRange<DateTime>>> IEDBTypeHandler<List<EDBRange<DateTime>>>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeList<DateTime>(buf, len, async, fieldDescription);

    ValueTask<EDBRange<DateTimeOffset>[]> IEDBTypeHandler<EDBRange<DateTimeOffset>[]>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeArray<DateTimeOffset>(buf, len, async, fieldDescription);

    ValueTask<List<EDBRange<DateTimeOffset>>> IEDBTypeHandler<List<EDBRange<DateTimeOffset>>>.Read(
        EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadMultirangeList<DateTimeOffset>(buf, len, async, fieldDescription);

    public int ValidateAndGetLength(EDBRange<ZonedDateTime>[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(List<EDBRange<ZonedDateTime>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(EDBRange<OffsetDateTime>[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(List<EDBRange<OffsetDateTime>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(EDBRange<DateTime>[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(List<EDBRange<DateTime>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(EDBRange<DateTimeOffset>[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public int ValidateAndGetLength(List<EDBRange<DateTimeOffset>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

    public Task Write(EDBRange<ZonedDateTime>[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(List<EDBRange<ZonedDateTime>> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(EDBRange<OffsetDateTime>[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(List<EDBRange<OffsetDateTime>> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(EDBRange<DateTime>[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(List<EDBRange<DateTime>> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(EDBRange<DateTimeOffset>[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    public Task Write(List<EDBRange<DateTimeOffset>> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache,
        EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

    #endregion Boilerplate
}