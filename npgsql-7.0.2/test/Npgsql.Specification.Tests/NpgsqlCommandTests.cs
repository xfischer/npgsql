using AdoNet.Specification.Tests;

namespace EnterpriseDB.EDBClient.Specification.Tests;

public sealed class EDBCommandTests : CommandTestBase<EDBDbFactoryFixture>
{
    public EDBCommandTests(EDBDbFactoryFixture fixture)
        : base(fixture)
    {
    }

    // PostgreSQL only supports a single transaction on a given connection at a given time. As a result,
    // EDB completely ignores DbCommand.Transaction.
    public override void ExecuteReader_throws_when_transaction_required() {}
    public override void ExecuteReader_throws_when_transaction_mismatched() {}
    public override void ExecuteReader_throws_when_reader_open() { }

}
