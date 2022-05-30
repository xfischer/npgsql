using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers
{
    public partial class MultirangeHandler<TSubtype> : EDBTypeHandler<EDBRange<TSubtype>[]>,
        IEDBTypeHandler<List<EDBRange<TSubtype>>>
    {
        /// <summary>
        /// The type handler for the range that this multirange type holds
        /// </summary>
        protected RangeHandler<TSubtype> RangeHandler { get; }

        /// <inheritdoc />
        public MultirangeHandler(PostgresMultirangeType pgMultirangeType, RangeHandler<TSubtype> rangeHandler)
            : base(pgMultirangeType)
            => RangeHandler = rangeHandler;

        public override ValueTask<EDBRange<TSubtype>[]> Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
            => ReadMultirangeArray<TSubtype>(buf, len, async, fieldDescription);

        protected async ValueTask<EDBRange<TAnySubtype>[]> ReadMultirangeArray<TAnySubtype>(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var numRanges = buf.ReadInt32();
            var multirange = new EDBRange<TAnySubtype>[numRanges];

            for (var i = 0; i < numRanges; i++)
            {
                await buf.Ensure(4, async);
                var rangeLen = buf.ReadInt32();
                multirange[i] = await RangeHandler.ReadRange<TAnySubtype>(buf, rangeLen, async, fieldDescription);
            }

            return multirange;
        }

        ValueTask<List<EDBRange<TSubtype>>> IEDBTypeHandler<List<EDBRange<TSubtype>>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadMultirangeList<TSubtype>(buf, len, async, fieldDescription);

        protected async ValueTask<List<EDBRange<TAnySubtype>>> ReadMultirangeList<TAnySubtype>(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var numRanges = buf.ReadInt32();
            var multirange = new List<EDBRange<TAnySubtype>>(numRanges);

            for (var i = 0; i < numRanges; i++)
            {
                await buf.Ensure(4, async);
                var rangeLen = buf.ReadInt32();
                multirange.Add(await RangeHandler.ReadRange<TAnySubtype>(buf, rangeLen, async, fieldDescription));
            }

            return multirange;
        }

        public override int ValidateAndGetLength(EDBRange<TSubtype>[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

        public int ValidateAndGetLength(List<EDBRange<TSubtype>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

        protected int ValidateAndGetLengthMultirange<TAnySubtype>(
            IList<EDBRange<TAnySubtype>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        {
            lengthCache ??= new EDBLengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get();

            // Leave empty slot for the entire array length, and go ahead an populate the element slots
            var pos = lengthCache.Position;
            lengthCache.Set(0);

            var sum = 4 + 4 * value.Count;
            for (var i = 0; i < value.Count; i++)
                sum += RangeHandler.ValidateAndGetLength(value[i], ref lengthCache, parameter);

            lengthCache.Lengths[pos] = sum;
            return sum;
        }

        public override Task Write(
            EDBRange<TSubtype>[] value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
            => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

        public Task Write(
            List<EDBRange<TSubtype>> value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
            => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

        public async Task WriteMultirange<TAnySubtype>(
            IList<EDBRange<TAnySubtype>> value,
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
                await RangeHandler.WriteWithLength(value[i], buf, lengthCache, parameter: null, async, cancellationToken);
        }
    }

    public class MultirangeHandler<TSubtype1, TSubtype2> : MultirangeHandler<TSubtype1>,
        IEDBTypeHandler<EDBRange<TSubtype2>[]>, IEDBTypeHandler<List<EDBRange<TSubtype2>>>
    {
        /// <inheritdoc />
        public MultirangeHandler(PostgresMultirangeType pgMultirangeType, RangeHandler<TSubtype1, TSubtype2> rangeHandler)
            : base(pgMultirangeType, rangeHandler) {}

        ValueTask<EDBRange<TSubtype2>[]> IEDBTypeHandler<EDBRange<TSubtype2>[]>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadMultirangeArray<TSubtype2>(buf, len, async, fieldDescription);

        ValueTask<List<EDBRange<TSubtype2>>> IEDBTypeHandler<List<EDBRange<TSubtype2>>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadMultirangeList<TSubtype2>(buf, len, async, fieldDescription);

        public int ValidateAndGetLength(List<EDBRange<TSubtype2>> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBRange<TSubtype2>[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthMultirange(value, ref lengthCache, parameter);

        public Task Write(
            List<EDBRange<TSubtype2>> value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
            => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

        public Task Write(
            EDBRange<TSubtype2>[] value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
            => WriteMultirange(value, buf, lengthCache, parameter, async, cancellationToken);

        public override int ValidateObjectAndGetLength(object? value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => value switch
            {
                EDBRange<TSubtype1>[] converted => ((IEDBTypeHandler<EDBRange<TSubtype1>[]>)this).ValidateAndGetLength(converted, ref lengthCache, parameter),
                EDBRange<TSubtype2>[] converted => ((IEDBTypeHandler<EDBRange<TSubtype2>[]>)this).ValidateAndGetLength(converted, ref lengthCache, parameter),
                List<EDBRange<TSubtype1>> converted => ((IEDBTypeHandler<List<EDBRange<TSubtype1>>>)this).ValidateAndGetLength(converted, ref lengthCache, parameter),
                List<EDBRange<TSubtype2>> converted => ((IEDBTypeHandler<List<EDBRange<TSubtype2>>>)this).ValidateAndGetLength(converted, ref lengthCache, parameter),

                DBNull => 0,
                null => 0,
                _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type RangeHandler<TElement>")
            };

        public override Task WriteObjectWithLength(object? value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => value switch
            {
                EDBRange<TSubtype1>[] converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),
                EDBRange<TSubtype2>[] converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),
                List<EDBRange<TSubtype1>> converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),
                List<EDBRange<TSubtype2>> converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),

                DBNull => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
                null => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
                _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type RangeHandler<TElement>")
            };
    }
}
