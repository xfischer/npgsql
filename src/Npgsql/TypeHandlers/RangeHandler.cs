#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    public abstract class RangeHandler : EDBTypeHandler
    {
        public override RangeHandler CreateRangeHandler(PostgresType rangeBackendType)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Type handler for PostgreSQL range types
    /// </summary>
    /// <remarks>
    /// Introduced in PostgreSQL 9.2.
    /// http://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    /// <typeparam name="TElement">the range subtype</typeparam>
    public class RangeHandler<TElement> : RangeHandler, IEDBTypeHandler<EDBRange<TElement>>
    {
        /// <summary>
        /// The type handler for the element that this range type holds
        /// </summary>
        readonly EDBTypeHandler _elementHandler;

        public RangeHandler(EDBTypeHandler elementHandler)
            => _elementHandler = elementHandler;

        /// <inheritdoc />
        public override ArrayHandler CreateArrayHandler(PostgresType arrayBackendType)
            => new ArrayHandler<EDBRange<TElement>>(this) { PostgresType = arrayBackendType };

        internal override Type GetFieldType(FieldDescription fieldDescription = null) => typeof(EDBRange<TElement>);
        internal override Type GetProviderSpecificFieldType(FieldDescription fieldDescription = null) => typeof(EDBRange<TElement>);

        #region Read

        internal override TAny Read<TAny>(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => Read<TAny>(buf, len, false, fieldDescription).Result;

        protected internal override ValueTask<TAny> Read<TAny>(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
        {
            if (this is IEDBTypeHandler<TAny> typedHandler)
                return typedHandler.Read(buf, len, async, fieldDescription);

            buf.Skip(len); // Perform this in sync for performance
            throw new EDBSafeReadException(new InvalidCastException(fieldDescription == null
                ? $"Can't cast database type to {typeof(TAny).Name}"
                : $"Can't cast database type {fieldDescription.Handler.PgDisplayName} to {typeof(TAny).Name}"
            ));
        }

        internal override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
            => await Read(buf, len, async, fieldDescription);

        internal override object ReadAsObject(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => Read(buf, len, false, fieldDescription).Result;

        public async ValueTask<EDBRange<TElement>> Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            await buf.Ensure(1, async);

            var flags = (RangeFlags)buf.ReadByte();
            if ((flags & RangeFlags.Empty) != 0)
                return EDBRange<TElement>.Empty;

            var lowerBound = default(TElement);
            var upperBound = default(TElement);

            if ((flags & RangeFlags.LowerBoundInfinite) == 0)
                lowerBound = await _elementHandler.ReadWithLength<TElement>(buf, async);

            if ((flags & RangeFlags.UpperBoundInfinite) == 0)
                upperBound = await _elementHandler.ReadWithLength<TElement>(buf, async);

            return new EDBRange<TElement>(lowerBound, upperBound, flags);
        }

        #endregion

        #region Write

        protected internal override int ValidateAndGetLength<TAny>(TAny value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => this is IEDBTypeHandler<TAny> typedHandler
                ? typedHandler.ValidateAndGetLength(value, ref lengthCache, parameter)
                : throw new InvalidCastException($"Can't write CLR type {typeof(TAny)} to database type {PgDisplayName}");

        protected internal override int ValidateObjectAndGetLength(object value, ref EDBLengthCache lengthCache, EDBParameter parameter = null)
            => ValidateAndGetLength((EDBRange<TElement>)value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBRange<TElement> value, ref EDBLengthCache lengthCache, EDBParameter parameter)
        {
            var totalLen = 1;
            var lengthCachePos = lengthCache?.Position ?? 0;
            if (!value.IsEmpty)
            {
                if (!value.LowerBoundInfinite)
                    totalLen += 4 + _elementHandler.ValidateAndGetLength(value.LowerBound, ref lengthCache, null);

                if (!value.UpperBoundInfinite)
                    totalLen += 4 + _elementHandler.ValidateAndGetLength(value.UpperBound, ref lengthCache, null);
            }

            // If we're traversing an already-populated length cache, rewind to first element slot so that
            // the elements' handlers can access their length cache values
            if (lengthCache != null && lengthCache.IsPopulated)
                lengthCache.Position = lengthCachePos;

            return totalLen;
        }

        internal override Task WriteWithLengthInternal<TAny>(TAny value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            if (buf.WriteSpaceLeft < 4)
                return WriteWithLengthLong();

            if (value == null || typeof(TAny) == typeof(DBNull))
            {
                buf.WriteInt32(-1);
                return PGUtil.CompletedTask;
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
        protected internal override Task WriteObjectWithLength(object value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => value == null || value is DBNull
                ? WriteWithLengthInternal(DBNull.Value, buf, lengthCache, parameter, async)
                : WriteWithLengthInternal((EDBRange<TElement>)value, buf, lengthCache, parameter, async);

        public async Task Write(EDBRange<TElement> value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
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
