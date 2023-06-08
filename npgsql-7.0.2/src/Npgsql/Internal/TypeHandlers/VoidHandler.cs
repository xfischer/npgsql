using System;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers;

/// <remarks>
/// https://www.postgresql.org/docs/current/static/datatype-boolean.html
/// </remarks>
sealed class VoidHandler : EDBSimpleTypeHandler<DBNull>
{
    public VoidHandler(PostgresType pgType) : base(pgType) {}

    public override DBNull Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => DBNull.Value;

    public override int ValidateAndGetLength(DBNull value, EDBParameter? parameter)
        => throw new NotSupportedException();

    public override void Write(DBNull value, EDBWriteBuffer buf, EDBParameter? parameter)
        => throw new NotSupportedException();

    public override int ValidateObjectAndGetLength(object? value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        => value switch
        {
            DBNull => 0,
            null => 0,
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type {nameof(VoidHandler)}")
        };

    public override Task WriteObjectWithLength(object? value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        => value switch
        {
            DBNull => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            null => WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken),
            _ => throw new InvalidCastException($"Can't write CLR type {value.GetType()} with handler type {nameof(VoidHandler)}")
        };
}