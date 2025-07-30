using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests;

[TestFixture(SyncOrAsync.Sync)]
[TestFixture(SyncOrAsync.Async)]
public abstract class SyncOrAsyncTestBase(SyncOrAsync syncOrAsync) : TestBase
{
    protected bool IsAsync => SyncOrAsync == SyncOrAsync.Async;

    protected SyncOrAsync SyncOrAsync { get; } = syncOrAsync;
}

public enum SyncOrAsync
{
    Sync,
    Async
}