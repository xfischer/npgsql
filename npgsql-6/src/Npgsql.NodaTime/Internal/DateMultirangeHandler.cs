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

namespace EnterpriseDB.EDBClient.NodaTime.Internal
{
    public partial class DateMultirangeHandler : MultirangeHandler<LocalDate>,
        IEDBTypeHandler<DateInterval[]>, IEDBTypeHandler<List<DateInterval>>
    {
        readonly IEDBTypeHandler<DateInterval> _dateIntervalHandler;

        public DateMultirangeHandler(PostgresMultirangeType multirangePostgresType, DateRangeHandler rangeHandler)
            : base(multirangePostgresType, rangeHandler)
            => _dateIntervalHandler = rangeHandler;

        public override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(DateInterval[]);
        public override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(DateInterval[]);

        public override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async,
            FieldDescription? fieldDescription = null)
            => (await Read<DateInterval[]>(buf, len, async, fieldDescription))!;

        async ValueTask<DateInterval[]> IEDBTypeHandler<DateInterval[]>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        {
            await buf.Ensure(4, async);
            var numRanges = buf.ReadInt32();
            var multirange = new DateInterval[numRanges];

            for (var i = 0; i < multirange.Length; i++)
            {
                await buf.Ensure(4, async);
                var rangeLen = buf.ReadInt32();
                multirange[i] = await _dateIntervalHandler.Read(buf, rangeLen, async, fieldDescription);
            }

            return multirange;
        }

        async ValueTask<List<DateInterval>> IEDBTypeHandler<List<DateInterval>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        {
            await buf.Ensure(4, async);
            var numRanges = buf.ReadInt32();
            var multirange = new List<DateInterval>(numRanges);

            for (var i = 0; i < numRanges; i++)
            {
                await buf.Ensure(4, async);
                var rangeLen = buf.ReadInt32();
                multirange.Add(await _dateIntervalHandler.Read(buf, rangeLen, async, fieldDescription));
            }

            return multirange;
        }

        public int ValidateAndGetLength(DateInterval[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthCore(value, ref lengthCache);

        public int ValidateAndGetLength(List<DateInterval> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthCore(value, ref lengthCache);

        int ValidateAndGetLengthCore(IList<DateInterval> value, ref EDBLengthCache? lengthCache)
        {
            lengthCache ??= new EDBLengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get();

            var sum = 4 + 4 * value.Count;
            for (var i = 0; i < value.Count; i++)
                sum += _dateIntervalHandler.ValidateAndGetLength(value[i], ref lengthCache, parameter: null);

            return lengthCache!.Set(sum);
        }

        public async Task Write(
            DateInterval[] value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 4)
                await buf.Flush(async, cancellationToken);

            buf.WriteInt32(value.Length);

            for (var i = 0; i < value.Length; i++)
                await RangeHandler.WriteWithLength(value[i], buf, lengthCache, parameter: null, async, cancellationToken);
        }

        public async Task Write(
            List<DateInterval> value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 4)
                await buf.Flush(async, cancellationToken);

            buf.WriteInt32(value.Count);

            for (var i = 0; i < value.Count; i++)
            {
                var interval = value[i];
                await RangeHandler.WriteWithLength(
                    new EDBRange<LocalDate>(interval.Start, interval.End), buf, lengthCache, parameter: null, async, cancellationToken);
            }
        }
    }
}
