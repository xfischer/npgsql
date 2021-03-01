using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests
{
    [NonParallelizable]
    class PoolManagerTests : TestBase
    {
        [Test]
        public void WithCanonicalConnString()
        {
            var connString = new EDBConnectionStringBuilder(ConnectionString).ToString();
            using (var conn = new EDBConnection(connString))
                conn.Open();
            var connString2 = new EDBConnectionStringBuilder(ConnectionString)
            {
                ApplicationName = "Another connstring"
            }.ToString();
            using (var conn = new EDBConnection(connString2))
                conn.Open();
        }

#if DEBUG
        [Test]
        public void ManyPools()
        {
            PoolManager.Reset();
            for (var i = 0; i < PoolManager.InitialPoolsSize + 1; i++)
            {
                var connString = new EDBConnectionStringBuilder(ConnectionString)
                {
                    ApplicationName = "App" + i
                }.ToString();
                using (var conn = new EDBConnection(connString))
                    conn.Open();
            }
            PoolManager.Reset();
        }
#endif

        [Test]
        public void ClearAll()
        {
            using (OpenConnection()) {}
            // Now have one connection in the pool
            Assert.That(PoolManager.TryGetValue(ConnectionString, out var pool), Is.True);
            Assert.That(pool!.Statistics.Idle, Is.EqualTo(1));

            EDBConnection.ClearAllPools();
            Assert.That(pool.Statistics.Idle, Is.Zero);
            Assert.That(pool.Statistics.Total, Is.Zero);
        }

        [Test]
        public void ClearAllWithBusy()
        {
            ConnectorPool? pool;
            using (OpenConnection())
            {
                using (OpenConnection()) { }
                // We have one idle, one busy

                EDBConnection.ClearAllPools();
                Assert.That(PoolManager.TryGetValue(ConnectionString, out pool), Is.True);
                Assert.That(pool!.Statistics.Idle, Is.Zero);
                Assert.That(pool.Statistics.Total, Is.EqualTo(1));
            }
            Assert.That(pool.Statistics.Idle, Is.Zero);
            Assert.That(pool.Statistics.Total, Is.Zero);
        }

        [SetUp]
        public void Setup() => PoolManager.Reset();

        [TearDown]
        public void Teardown() => PoolManager.Reset();
    }
}
