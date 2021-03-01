using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// An interface implementing by <see cref="RangeHandler{TElement}"/>, exposing the handler's supported range
    /// CLR types.
    /// </summary>
    public interface IRangeHandler
    {
        /// <summary>
        /// Exposes the range CLR types supported by this handler.
        /// </summary>
        Type[] SupportedRangeClrTypes { get; }
    }

    /// <summary>
    /// A type handler for PostgreSQL range types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/rangetypes.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    /// <typeparam name="TElement">the range subtype</typeparam>
    public class RangeHandler<TElement> : EDBTypeHandler<EDBRange<TElement>>, IRangeHandler
    {
        /// <summary>
        /// The type handler for the element that this range type holds
        /// </summary>
        readonly EDBTypeHandler _elementHandler;

        /// <inheritdoc />
        public Type[] SupportedRangeClrTypes { get; }

        /// <inheritdoc />
        public RangeHandler(PostgresType rangePostgresType, EDBTypeHandler elementHandler)
            : this(rangePostgresType, elementHandler, new[] { typeof(EDBRange<TElement>)}) {}

        /// <inheritdoc />
        protected RangeHandler(PostgresType rangePostgresType, EDBTypeHandler elementHandler, Type[] supportedElementClrTypes)
            : base(rangePostgresType)
        {
            _elementHandler = elementHandler;
            SupportedRangeClrTypes = supportedElementClrTypes;
        }

        /// <inheritdoc />
        public override ArrayHandler CreateArrayHandler(PostgresArrayType arrayBackendType, ArrayNullabilityMode arrayNullabilityMode)
            => new ArrayHandler<EDBRange<TElement>>(arrayBackendType, this, arrayNullabilityMode);

        internal override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(EDBRange<TElement>);
        internal override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(EDBRange<TElement>);

        /// <inheritdoc />
        public override IRangeHandler CreateRangeHandler(PostgresType rangeBackendType)
            => throw new NotSupportedException();

        #region Read

        /// <inheritdoc />
        public override TAny Read<TAny>(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => Read<TAny>(buf, len, false, fieldDescription).Result;

        /// <inheritdoc />
        public override ValueTask<EDBRange<TElement>> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
            => DoRead<TElement>(buf, len, async, fieldDescription);

        private protected async ValueTask<EDBRange<TAny>> DoRead<TAny>(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        {
            await buf.Ensure(1, async);

            var flags = (RangeFlags)buf.ReadByte();
            if ((flags & RangeFlags.Empty) != 0)
                return EDBRange<TAny>.Empty;

            var lowerBound = default(TAny);
            var upperBound = default(TAny);

            if ((flags & RangeFlags.LowerBoundInfinite) == 0)
                lowerBound = await _elementHandler.ReadWithLength<TAny>(buf, async);

            if ((flags & RangeFlags.UpperBoundInfinite) == 0)
                upperBound = await _elementHandler.ReadWithLength<TAny>(buf, async);

            return new EDBRange<TAny>(lowerBound, upperBound, flags);
        }

        #endregion

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBRange<TElement> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        private protected int ValidateAndGetLength<TAny>(EDBRange<TAny> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        {
            var totalLen = 1;
            var lengthCachePos = lengthCache?.Position ?? 0;
            if (!value.IsEmpty)
            {
                if (!value.LowerBoundInfinite)
                {
                    totalLen += 4;
                    if (!(value.LowerBound is null) && typeof(TElement) != typeof(DBNull))
                        totalLen += _elementHandler.ValidateAndGetLength(value.LowerBound, ref lengthCache, null);
                }

                if (!value.UpperBoundInfinite)
                {
                    totalLen += 4;
                    if (!(value.UpperBound is null) && typeof(TElement) != typeof(DBNull))
                        totalLen += _elementHandler.ValidateAndGetLength(value.UpperBound, ref lengthCache, null);
                }
            }

            // If we're traversing an already-populated length cache, rewind to first element slot so that
            // the elements' handlers can access their length cache values
            if (lengthCache != null && lengthCache.IsPopulated)
                lengthCache.Position = lengthCachePos;

            return totalLen;
        }

        internal override Task WriteWithLengthInternal<TAny>([AllowNull] TAny value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 4)
                return WriteWithLengthLong();

            if (value == null || typeof(TAny) == typeof(DBNull))
            {
                buf.WriteInt32(-1);
                return Task.CompletedTask;
            }

            return WriteWithLengthCore();

            async Task WriteWithLengthLong()
            {
                if (buf.WriteSpaceLeft < 4)
                    await buf.Flush(async, cancellationToken);

                if (value == null || typeof(TAny) == typeof(DBNull))
                {
                    buf.WriteInt32(-1);
                    return;
                }

                await WriteWithLengthCore();
            }

            Task WriteWithLengthCore()
            {
                if (this is IEDBTypeHandler<TAny> typedHandler)
                {
                    buf.WriteInt32(typedHandler.ValidateAndGetLength(value, ref lengthCache, parameter));
                    return typedHandler.Write(value, buf, lengthCache, parameter, async, cancellationToken);
                }
                else
                    throw new InvalidCastException($"Can't write CLR type {typeof(TAny)} to database type {PgDisplayName}");
            }
        }

        /// <inheritdoc />
        public override Task Write(EDBRange<TElement> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write(value, buf, lengthCache, parameter, async, cancellationToken);

        private protected async Task Write<TAny>(EDBRange<TAny> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 1)
                await buf.Flush(async, cancellationToken);

            buf.WriteByte((byte)value.Flags);

            if (value.IsEmpty)
                return;

            if (!value.LowerBoundInfinite)
                await _elementHandler.WriteWithLengthInternal(value.LowerBound, buf, lengthCache, null, async, cancellationToken);

            if (!value.UpperBoundInfinite)
                await _elementHandler.WriteWithLengthInternal(value.UpperBound, buf, lengthCache, null, async, cancellationToken);
        }

        #endregion
    }

    /// <summary>
    /// Type handler for PostgreSQL range types
    /// </summary>
    /// <remarks>
    /// Introduced in PostgreSQL 9.2.
    /// https://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    /// <typeparam name="TElement1">the main range subtype</typeparam>
    /// <typeparam name="TElement2">an alternative range subtype</typeparam>
    public class RangeHandler<TElement1, TElement2> : RangeHandler<TElement1>, IEDBTypeHandler<EDBRange<TElement2>>
    {
        /// <inheritdoc />
        public RangeHandler(PostgresType rangePostgresType, EDBTypeHandler elementHandler)
            : base(rangePostgresType, elementHandler, new[] { typeof(EDBRange<TElement1>), typeof(EDBRange<TElement2>) }) {}

        ValueTask<EDBRange<TElement2>> IEDBTypeHandler<EDBRange<TElement2>>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => DoRead<TElement2>(buf, len, async, fieldDescription);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBRange<TElement2> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength<TElement2>(value, ref lengthCache, parameter);

        /// <inheritdoc />
        public Task Write(EDBRange<TElement2> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write<TElement2>(value, buf, lengthCache, parameter, async, cancellationToken);
    }
}
