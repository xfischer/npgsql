using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers;

/// <summary>
/// A type handler for PostgreSQL range types.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/rangetypes.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
/// <typeparam name="TSubtype">The range subtype.</typeparam>
// NOTE: This cannot inherit from EDBTypeHandler<EDBRange<TSubtype>>, since that triggers infinite generic recursion in Native AOT
public partial class RangeHandler<TSubtype> : EDBTypeHandler, IEDBTypeHandler<EDBRange<TSubtype>>
{
    /// <summary>
    /// The type handler for the subtype that this range type holds
    /// </summary>
    protected EDBTypeHandler SubtypeHandler { get; }

    /// <inheritdoc />
    public RangeHandler(PostgresType rangePostgresType, EDBTypeHandler subtypeHandler)
        : base(rangePostgresType)
        => SubtypeHandler = subtypeHandler;

    public override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(EDBRange<TSubtype>);
    public override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(EDBRange<TSubtype>);

    /// <inheritdoc />
    public override EDBTypeHandler CreateArrayHandler(PostgresArrayType pgArrayType, ArrayNullabilityMode arrayNullabilityMode)
        => new ArrayHandler<EDBRange<TSubtype>>(pgArrayType, this, arrayNullabilityMode);

    /// <inheritdoc />
    public override EDBTypeHandler CreateRangeHandler(PostgresType pgRangeType)
        => throw new NotSupportedException();

    /// <inheritdoc />
    public override EDBTypeHandler CreateMultirangeHandler(PostgresMultirangeType pgMultirangeType)
        => throw new NotSupportedException();

    #region Read

    /// <inheritdoc />
    public ValueTask<EDBRange<TSubtype>> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        => ReadRange<TSubtype>(buf, len, async, fieldDescription);

    protected internal async ValueTask<EDBRange<TAnySubtype>> ReadRange<TAnySubtype>(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
    {
        await buf.Ensure(1, async);

        var flags = (RangeFlags)buf.ReadByte();
        if ((flags & RangeFlags.Empty) != 0)
            return EDBRange<TAnySubtype>.Empty;

        var lowerBound = default(TAnySubtype);
        var upperBound = default(TAnySubtype);

        if ((flags & RangeFlags.LowerBoundInfinite) == 0)
            lowerBound = await SubtypeHandler.ReadWithLength<TAnySubtype>(buf, async);

        if ((flags & RangeFlags.UpperBoundInfinite) == 0)
            upperBound = await SubtypeHandler.ReadWithLength<TAnySubtype>(buf, async);

        return new EDBRange<TAnySubtype>(lowerBound, upperBound, flags);
    }

    public override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        => await Read(buf, len, async, fieldDescription);

    #endregion

    #region Write

    /// <inheritdoc />
    public int ValidateAndGetLength(EDBRange<TSubtype> value, [NotNullIfNotNull("lengthCache")] ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

    protected internal int ValidateAndGetLengthRange<TAnySubtype>(EDBRange<TAnySubtype> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
    {
        var totalLen = 1;
        var lengthCachePos = lengthCache?.Position ?? 0;
        if (!value.IsEmpty)
        {
            if (!value.LowerBoundInfinite)
            {
                totalLen += 4;
                if (value.LowerBound is not null)
                    totalLen += SubtypeHandler.ValidateAndGetLength(value.LowerBound, ref lengthCache, null);
            }

            if (!value.UpperBoundInfinite)
            {
                totalLen += 4;
                if (value.UpperBound is not null)
                    totalLen += SubtypeHandler.ValidateAndGetLength(value.UpperBound, ref lengthCache, null);
            }
        }

        // If we're traversing an already-populated length cache, rewind to first element slot so that
        // the elements' handlers can access their length cache values
        if (lengthCache != null && lengthCache.IsPopulated)
            lengthCache.Position = lengthCachePos;

        return totalLen;
    }

    /// <inheritdoc />
    public Task Write(EDBRange<TSubtype> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);

    protected internal async Task WriteRange<TAnySubtype>(EDBRange<TAnySubtype> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
    {
        if (buf.WriteSpaceLeft < 1)
            await buf.Flush(async, cancellationToken);

        buf.WriteByte((byte)value.Flags);

        if (value.IsEmpty)
            return;

        if (!value.LowerBoundInfinite)
            await SubtypeHandler.WriteWithLength(value.LowerBound, buf, lengthCache, null, async, cancellationToken);

        if (!value.UpperBoundInfinite)
            await SubtypeHandler.WriteWithLength(value.UpperBound, buf, lengthCache, null, async, cancellationToken);
    }

    #endregion
}

/// <summary>
/// Type handler for PostgreSQL range types.
/// </summary>
/// <remarks>
/// Introduced in PostgreSQL 9.2.
/// https://www.postgresql.org/docs/current/static/rangetypes.html
/// </remarks>
/// <typeparam name="TSubtype1">The main range subtype.</typeparam>
/// <typeparam name="TSubtype2">An alternative range subtype.</typeparam>
public class RangeHandler<TSubtype1, TSubtype2> : RangeHandler<TSubtype1>, IEDBTypeHandler<EDBRange<TSubtype2>>
{
    /// <inheritdoc />
    public RangeHandler(PostgresType rangePostgresType, EDBTypeHandler subtypeHandler)
        : base(rangePostgresType, subtypeHandler) {}

    ValueTask<EDBRange<TSubtype2>> IEDBTypeHandler<EDBRange<TSubtype2>>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        => ReadRange<TSubtype2>(buf, len, async, fieldDescription);

    /// <inheritdoc />
    public int ValidateAndGetLength(EDBRange<TSubtype2> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

    /// <inheritdoc />
    public Task Write(EDBRange<TSubtype2> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);

    public override int ValidateObjectAndGetLength(object? value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => value switch
        {
            EDBRange<TSubtype1> converted => ((IEDBTypeHandler<EDBRange<TSubtype1>>)this).ValidateAndGetLength(converted, ref lengthCache, parameter),
            EDBRange<TSubtype2> converted => ((IEDBTypeHandler<EDBRange<TSubtype2>>)this).ValidateAndGetLength(converted, ref lengthCache, parameter),

            DBNull => 0,
            null => 0,
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type RangeHandler<TElement>")
        };

    public override Task WriteObjectWithLength(object? value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => value switch
        {
            EDBRange<TSubtype1> converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),
            EDBRange<TSubtype2> converted => WriteWithLength(converted, buf, lengthCache, parameter, async, cancellationToken),

            DBNull => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            null => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type RangeHandler<TElement>")
        };
}