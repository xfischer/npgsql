using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Util;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace EnterpriseDB.EDBClient;

sealed class MultiHostDataSourceWrapper(EDBMultiHostDataSource wrappedSource, TargetSessionAttributes targetSessionAttributes)
    : EDBDataSource(CloneSettingsForTargetSessionAttributes(wrappedSource.Settings, targetSessionAttributes), wrappedSource.Configuration, reportMetrics: false)
{
    internal EDBMultiHostDataSource WrappedSource { get; } = wrappedSource;

    internal override bool OwnsConnectors => false;

    public override void Clear() => WrappedSource.Clear();

    static EDBConnectionStringBuilder CloneSettingsForTargetSessionAttributes(
        EDBConnectionStringBuilder settings,
        TargetSessionAttributes targetSessionAttributes)
    {
        var clonedSettings = settings.Clone();
        clonedSettings.TargetSessionAttributesParsed = targetSessionAttributes;
        return clonedSettings;
    }

    internal override (int Total, int Idle, int Busy) Statistics => WrappedSource.Statistics;

    internal override ValueTask<EDBConnector> Get(EDBConnection conn, EDBTimeout timeout, bool async, CancellationToken cancellationToken)
        => WrappedSource.Get(conn, timeout, async, cancellationToken);
    internal override bool TryGetIdleConnector([NotNullWhen(true)] out EDBConnector? connector)
        => throw new EDBException("EDB bug: trying to get an idle connector from " + nameof(MultiHostDataSourceWrapper));
    internal override ValueTask<EDBConnector?> OpenNewConnector(EDBConnection conn, EDBTimeout timeout, bool async, CancellationToken cancellationToken)
        => throw new EDBException("EDB bug: trying to open a new connector from " + nameof(MultiHostDataSourceWrapper));
    internal override void Return(EDBConnector connector)
        => WrappedSource.Return(connector);

    internal override void AddPendingEnlistedConnector(EDBConnector connector, Transaction transaction)
        => WrappedSource.AddPendingEnlistedConnector(connector, transaction);
    internal override bool TryRemovePendingEnlistedConnector(EDBConnector connector, Transaction transaction)
        => WrappedSource.TryRemovePendingEnlistedConnector(connector, transaction);
    internal override bool TryRentEnlistedPending(Transaction transaction, EDBConnection connection,
        [NotNullWhen(true)] out EDBConnector? connector)
        => WrappedSource.TryRentEnlistedPending(transaction, connection, out connector);
}
