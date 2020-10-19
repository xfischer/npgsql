using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// Base class for all type handlers which handle PostgreSQL ranges.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/rangetypes.html
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public abstract class RangeHandler : EDBTypeHandler
    {
        /// <inheritdoc />
        protected RangeHandler(PostgresType rangePostgresType) : base(rangePostgresType) {}

        /// <inheritdoc />
        public override RangeHandler CreateRangeHandler(PostgresRangeType rangeBackendType)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// A type handler for PostgreSQL range types.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/rangetypes.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    /// <typeparam name="TElement">the range subtype</typeparam>
    public class RangeHandler<TElement> : RangeHandler, IEDBTypeHandler<EDBRange<TElement>>
    {
        /// <summary>
        /// The type handler for the element that this range type holds
        /// </summary>
        readonly EDBTypeHandler _elementHandler;

        /// <inheritdoc />
        public RangeHandler(PostgresType rangePostgresType, EDBTypeHandler elementHandler)
            : base(rangePostgresType) => _elementHandler = elementHandler;

        /// <inheritdoc />
        public override ArrayHandler CreateArrayHandler(PostgresArrayType arrayBackendType)
            => new ArrayHandler<EDBRange<TElement>>(arrayBackendType, this);

        internal override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(EDBRange<TElement>);
        internal override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(EDBRange<TElement>);

        #region Read

        /// <inheritdoc />
        public override TAny Read<TAny>(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => Read<TAny>(buf, len, false, fieldDescription).Result;

        /// <inheritdoc />
        protected internal override ValueTask<TAny> Read<TAny>(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            if (this is IEDBTypeHandler<TAny> typedHandler)
                return typedHandler.Read(buf, len, async, fieldDescription);

            buf.Skip(len); // Perform this in sync for performance
            throw new EDBSafeReadException(new InvalidCastException(fieldDescription == null
                ? $"Can't cast database type to {typeof(TAny).Name}"
                : $"Can't cast database type {fieldDescription.Handler.PgDisplayName} to {typeof(TAny).Name}"
            ));
        }

        internal override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
            => await Read(buf, len, async, fieldDescription);

        internal override object ReadAsObject(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => Read(buf, len, false, fieldDescription).Result;

        /// <inheritdoc />
        public async ValueTask<EDBRange<TElement>> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(1, async);

            var flags = (RangeFlags)buf.ReadByte();
            if ((flags & RangeFlags.Empty) != 0)
                return EDBRange<TElement>.Empty;

            var lowerBound = flags.HasFlag(RangeFlags.LowerBoundInfinite)
                ? default
                : await _elementHandler.ReadWithLength<TElement>(buf, async);

            var upperBound = flags.HasFlag(RangeFlags.UpperBoundInfinite)
                ? default
                : await _elementHandler.ReadWithLength<TElement>(buf, async);

            return new EDBRange<TElement>(lowerBound, upperBound, flags);
        }

        #endregion

        #region Write

        /// <inheritdoc />
        protected internal override int ValidateAndGetLength<TAny>(TAny value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => this is IEDBTypeHandler<TAny> typedHandler
                ? typedHandler.ValidateAndGetLength(value, ref lengthCache, parameter)
                : throw new InvalidCastException($"Can't write CLR type {typeof(TAny)} to database type {PgDisplayName}");

        /// <inheritdoc />
        protected internal override int ValidateObjectAndGetLength(object value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBRange<TElement>)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBRange<TElement> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
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

        internal override Task WriteWithLengthInternal<TAny>([AllowNull] TAny value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
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
                    await buf.Flush(async);

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
                    return typedHandler.Write(value, buf, lengthCache, parameter, async);
                }
                else
                    throw new InvalidCastException($"Can't write CLR type {typeof(TAny)} to database type {PgDisplayName}");
            }
        }

        // The default WriteObjectWithLength casts the type handler to IEDBTypeHandler<T>, but that's not sufficient for
        // us (need to handle many types of T, e.g. int[], int[,]...)
        /// <inheritdoc />
        protected internal override Task WriteObjectWithLength(object value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
            => value is DBNull
                ? WriteWithLengthInternal(DBNull.Value, buf, lengthCache, parameter, async)
                : WriteWithLengthInternal((EDBRange<TElement>)value, buf, lengthCache, parameter, async);

        /// <inheritdoc />
        public async Task Write(EDBRange<TElement> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
        {
            if (buf.WriteSpaceLeft < 1)
                await buf.Flush(async);

            buf.WriteByte((byte)value.Flags);

            if (value.IsEmpty)
                return;

            if (!value.LowerBoundInfinite)
                await _elementHandler.WriteWithLengthInternal(value.LowerBound, buf, lengthCache, null, async);

            if (!value.UpperBoundInfinite)
                await _elementHandler.WriteWithLengthInternal(value.UpperBound, buf, lengthCache, null, async);
        }

        #endregion
    }
}
