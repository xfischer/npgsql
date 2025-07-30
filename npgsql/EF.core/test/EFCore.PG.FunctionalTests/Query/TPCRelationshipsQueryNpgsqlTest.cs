// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class TPCRelationshipsQueryNpgsqlTest
    : TPCRelationshipsQueryTestBase<TPCRelationshipsQueryNpgsqlTest.TPCRelationshipsQueryNpgsqlFixture>
{
    public TPCRelationshipsQueryNpgsqlTest(
        TPCRelationshipsQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class TPCRelationshipsQueryNpgsqlFixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
