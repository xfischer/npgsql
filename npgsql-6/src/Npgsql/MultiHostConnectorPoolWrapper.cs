using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Util;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace EnterpriseDB.EDBClient
{
    sealed class MultiHostConnectorPoolWrapper : ConnectorSource
    {
        internal override bool OwnsConnectors => false;

        readonly MultiHostConnectorPool _wrappedSource;

        public MultiHostConnectorPoolWrapper(EDBConnectionStringBuilder settings, string connString, MultiHostConnectorPool source) : base(settings, connString)
            => _wrappedSource = source;

        internal override (int Total, int Idle, int Busy) Statistics => _wrappedSource.Statistics;

        internal override void Clear() => _wrappedSource.Clear();
        internal override ValueTask<EDBConnector> Get(EDBConnection conn, EDBTimeout timeout, bool async, CancellationToken cancellationToken)
            => _wrappedSource.Get(conn, timeout, async, cancellationToken);
        internal override bool TryGetIdleConnector([NotNullWhen(true)] out EDBConnector? connector)
            => throw new EDBException("EDB bug: trying to get an idle connector from " + nameof(MultiHostConnectorPoolWrapper));
        internal override ValueTask<EDBConnector?> OpenNewConnector(EDBConnection conn, EDBTimeout timeout, bool async, CancellationToken cancellationToken)
            => throw new EDBException("EDB bug: trying to open a new connector from " + nameof(MultiHostConnectorPoolWrapper));
        internal override void Return(EDBConnector connector)
            => _wrappedSource.Return(connector);

        internal override void AddPendingEnlistedConnector(EDBConnector connector, Transaction transaction)
            => _wrappedSource.AddPendingEnlistedConnector(connector, transaction);
        internal override bool TryRemovePendingEnlistedConnector(EDBConnector connector, Transaction transaction)
            => _wrappedSource.TryRemovePendingEnlistedConnector(connector, transaction);
        internal override bool TryRentEnlistedPending(Transaction transaction, EDBConnection connection,
            [NotNullWhen(true)] out EDBConnector? connector)
            => _wrappedSource.TryRentEnlistedPending(transaction, connection, out connector);
    }
}
