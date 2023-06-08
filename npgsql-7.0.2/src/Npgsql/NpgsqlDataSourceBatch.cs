using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Properties;

namespace EnterpriseDB.EDBClient;

sealed class EDBDataSourceBatch : EDBBatch
{
    internal EDBDataSourceBatch(EDBConnection connection)
        : base(new EDBDataSourceCommand(DefaultBatchCommandsSize, connection))
    {
    }

    // The below are incompatible with batches executed directly against DbDataSource, since no DbConnection
    // is involved at the user API level and the batch owns the DbConnection.
    public override void Prepare()
        => throw new NotSupportedException(EDBStrings.NotSupportedOnDataSourceBatch);

    public override Task PrepareAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException(EDBStrings.NotSupportedOnDataSourceBatch);

    protected override DbConnection? DbConnection
    {
        get => throw new NotSupportedException(EDBStrings.NotSupportedOnDataSourceBatch);
        set => throw new NotSupportedException(EDBStrings.NotSupportedOnDataSourceBatch);
    }

    protected override DbTransaction? DbTransaction
    {
        get => throw new NotSupportedException(EDBStrings.NotSupportedOnDataSourceBatch);
        set => throw new NotSupportedException(EDBStrings.NotSupportedOnDataSourceBatch);
    }
}
