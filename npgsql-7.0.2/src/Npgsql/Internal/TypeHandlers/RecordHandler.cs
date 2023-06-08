using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers;

/// <summary>
/// Type handler for PostgreSQL record types. Defaults to returning object[], but can also return <see cref="ValueTuple" /> or <see cref="Tuple"/>.
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/static/datatype-pseudo.html
///
/// Encoding (identical to composite):
/// A 32-bit integer with the number of columns, then for each column:
/// * An OID indicating the type of the column
/// * The length of the column(32-bit integer), or -1 if null
/// * The column data encoded as binary
/// </remarks>
sealed partial class RecordHandler : EDBTypeHandler<object[]>
{
    readonly TypeMapper _typeMapper;

    public RecordHandler(PostgresType postgresType, TypeMapper typeMapper)
        : base(postgresType)
        => _typeMapper = typeMapper;

    #region Read

    protected internal override async ValueTask<T> ReadCustom<T>(
        EDBReadBuffer buf,
        int len,
        bool async,
        FieldDescription? fieldDescription)
    {
        if (typeof(T) == typeof(object[]))
            return (T)(object)await Read(buf, len, async, fieldDescription);

        if (typeof(T).FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true ||
            typeof(T).FullName?.StartsWith("System.Tuple`", StringComparison.Ordinal) == true)
        {
            var asArray = await Read(buf, len, async, fieldDescription);
            if (typeof(T).GenericTypeArguments.Length != asArray.Length)
                throw new InvalidCastException($"Cannot read record type with {asArray.Length} fields as {typeof(T)}");

            var constructor = typeof(T).GetConstructors().Single(c => c.GetParameters().Length == asArray.Length);
            return (T)constructor.Invoke(asArray);
        }

        return await base.ReadCustom<T>(buf, len, async, fieldDescription);
    }

    public override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        => await Read(buf, len, async, fieldDescription);

    public override async ValueTask<object[]> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
    {
        await buf.Ensure(4, async);
        var fieldCount = buf.ReadInt32();
        var result = new object[fieldCount];

        for (var i = 0; i < fieldCount; i++)
        {
            await buf.Ensure(8, async);
            var typeOID = buf.ReadUInt32();
            var fieldLen = buf.ReadInt32();
            if (fieldLen == -1)  // Null field, simply skip it and leave at default
                continue;
            result[i] = await _typeMapper.ResolveByOID(typeOID).ReadAsObject(buf, fieldLen, async);
        }

        return result;
    }

    /// <inheritdoc />
    public override EDBTypeHandler CreateRangeHandler(PostgresType pgRangeType)
        => throw new NotSupportedException();

    /// <inheritdoc />
    public override EDBTypeHandler CreateMultirangeHandler(PostgresMultirangeType pgMultirangeType)
        => throw new NotSupportedException();

    #endregion

    #region Write (unsupported)

    public override int ValidateAndGetLength(object[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => throw new NotSupportedException("Can't write record types");

    public override Task Write(
        object[] value,
        EDBWriteBuffer buf,
        EDBLengthCache? lengthCache,
        EDBParameter? parameter,
        bool async,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Can't write record types");

    #endregion
}