using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Util;

namespace EnterpriseDB.EDBClient;

sealed class UnpooledDataSource : EDBDataSource
{
    public UnpooledDataSource(EDBConnectionStringBuilder settings, EDBDataSourceConfiguration dataSourceConfig)
        : base(settings, dataSourceConfig)
    {
    }

    volatile int _numConnectors;

    internal override (int Total, int Idle, int Busy) Statistics => (_numConnectors, 0, _numConnectors);

    internal override bool OwnsConnectors => true;

    internal override async ValueTask<EDBConnector> Get(
        EDBConnection conn, EDBTimeout timeout, bool async, CancellationToken cancellationToken)
    {
        CheckDisposed();

        var connector = new EDBConnector(this, conn);
        await connector.Open(timeout, async, cancellationToken);
        Interlocked.Increment(ref _numConnectors);
        return connector;
    }

    internal override bool TryGetIdleConnector([NotNullWhen(true)] out EDBConnector? connector)
    {
        connector = null;
        return false;
    }

    internal override ValueTask<EDBConnector?> OpenNewConnector(
        EDBConnection conn, EDBTimeout timeout, bool async, CancellationToken cancellationToken)
        => new((EDBConnector?)null);

    internal override void Return(EDBConnector connector)
    {
        Interlocked.Decrement(ref _numConnectors);
        connector.Close();
    }

    internal override void Clear() {}

    internal override bool TryRentEnlistedPending(Transaction transaction, EDBConnection connection,
        [NotNullWhen(true)] out EDBConnector? connector)
    {
        connector = null;
        return false;
    }

    internal override bool TryRemovePendingEnlistedConnector(EDBConnector connector, Transaction transaction) => false;
}