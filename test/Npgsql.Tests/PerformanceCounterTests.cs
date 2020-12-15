#if NET461 || NET472 || NET48
using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests
{
    [NonParallelizable]
    [Explicit]
    public class PerformanceCounterTests : TestBase
    {
        [Test]
        public void HardConnectsAndDisconnectsPerSecond()
        {
            var hardConnectCounter = Counters.HardConnectsPerSecond.DiagnosticsCounter;
            var hardDisconnectCounter = Counters.HardDisconnectsPerSecond.DiagnosticsCounter;
            var softConnectCounter = Counters.SoftConnectsPerSecond.DiagnosticsCounter;
            var softDisconnectCounter = Counters.SoftDisconnectsPerSecond.DiagnosticsCounter;

            var nonPooledConnString = new EDBConnectionStringBuilder(ConnectionString)
            {
                Pooling = false
            }.ToString();
            using (var pooledConn = new EDBConnection(ConnectionString))
            using (var nonPooledConn = new EDBConnection(nonPooledConnString))
            {
                Thread.Sleep(2000);  // Let the counter reset
                Assert.That(hardConnectCounter!.RawValue, Is.Zero);
                Assert.That(hardDisconnectCounter!.RawValue, Is.Zero);
                Assert.That(softConnectCounter!.RawValue, Is.Zero);
                Assert.That(softDisconnectCounter!.RawValue, Is.Zero);
                pooledConn.Open();   // Pool is empty so this is a hard connect
                nonPooledConn.Open();
                Assert.That(hardConnectCounter.RawValue, Is.EqualTo(2));
                Assert.That(hardDisconnectCounter.RawValue, Is.Zero);
                Assert.That(softConnectCounter.RawValue, Is.EqualTo(1));
                Assert.That(softDisconnectCounter.RawValue, Is.Zero);
                pooledConn.Close();
                nonPooledConn.Close();
                // The non-pooled was a hard disconnect, the pooled was a soft
                Assert.That(hardConnectCounter.RawValue, Is.EqualTo(2));
                Assert.That(hardDisconnectCounter.RawValue, Is.EqualTo(1));
                Assert.That(softConnectCounter.RawValue, Is.EqualTo(1));
                Assert.That(softDisconnectCounter.RawValue, Is.EqualTo(1));
                pooledConn.Open();
                Assert.That(hardConnectCounter.RawValue, Is.EqualTo(2));
                Assert.That(hardDisconnectCounter.RawValue, Is.EqualTo(1));
                Assert.That(softConnectCounter.RawValue, Is.EqualTo(2));
                Assert.That(softDisconnectCounter.RawValue, Is.EqualTo(1));
                pooledConn.Close();
                Assert.That(hardConnectCounter.RawValue, Is.EqualTo(2));
                Assert.That(hardDisconnectCounter.RawValue, Is.EqualTo(1));
                Assert.That(softConnectCounter.RawValue, Is.EqualTo(2));
                Assert.That(softDisconnectCounter.RawValue, Is.EqualTo(2));
            }
        }

        [Test]
        public void NumberOfNonPooledConnections()
        {
            var counter = Counters.NumberOfNonPooledConnections.DiagnosticsCounter;

            var nonPooledConnString = new EDBConnectionStringBuilder(ConnectionString)
            {
                Pooling = false
            }.ToString();
            using (var pooledConn = new EDBConnection(ConnectionString))
            using (var nonPooledConn = new EDBConnection(nonPooledConnString))
            {
                Assert.That(counter!.RawValue, Is.Zero);
                nonPooledConn.Open();
                Assert.That(counter.RawValue, Is.EqualTo(1));
                pooledConn.Open();
                Assert.That(counter.RawValue, Is.EqualTo(1));
                nonPooledConn.Close();
                Assert.That(counter.RawValue, Is.Zero);
                pooledConn.Close();
                Assert.That(counter.RawValue, Is.Zero);
            }
        }

        [Test]
        public void PooledConnections()
        {
            var totalCounter = Counters.NumberOfPooledConnections.DiagnosticsCounter;
            var activeCounter = Counters.NumberOfActiveConnections.DiagnosticsCounter;
            var freeCounter = Counters.NumberOfFreeConnections.DiagnosticsCounter;

            var nonPooledConnString = new EDBConnectionStringBuilder(ConnectionString)
            {
                Pooling = false
            }.ToString();
            using (var pooledConn = new EDBConnection(ConnectionString))
            using (var nonPooledConn = new EDBConnection(nonPooledConnString))
            {
                Assert.That(totalCounter!.RawValue, Is.Zero);
                Assert.That(activeCounter!.RawValue, Is.Zero);
                Assert.That(freeCounter!.RawValue, Is.Zero);
                nonPooledConn.Open();
                Assert.That(totalCounter.RawValue, Is.Zero);
                Assert.That(activeCounter.RawValue, Is.Zero);
                Assert.That(freeCounter.RawValue, Is.Zero);
                pooledConn.Open();
                Assert.That(totalCounter.RawValue, Is.EqualTo(1));
                Assert.That(activeCounter.RawValue, Is.EqualTo(1));
                Assert.That(freeCounter.RawValue, Is.Zero);
                nonPooledConn.Close();
                Assert.That(totalCounter.RawValue, Is.EqualTo(1));
                Assert.That(activeCounter.RawValue, Is.EqualTo(1));
                Assert.That(freeCounter.RawValue, Is.Zero);
                pooledConn.Close();
                Assert.That(totalCounter.RawValue, Is.EqualTo(1));
                Assert.That(activeCounter.RawValue, Is.Zero);
                Assert.That(freeCounter.RawValue, Is.EqualTo(1));
            }
            EDBConnection.ClearAllPools();
            Assert.That(totalCounter.RawValue, Is.Zero);
            Assert.That(activeCounter.RawValue, Is.Zero);
            Assert.That(freeCounter.RawValue, Is.Zero);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var countersExist = false;
            try
            {
                countersExist = PerformanceCounterCategory.Exists(Counter.DiagnosticsCounterCategory);
            }
            catch (UnauthorizedAccessException) {}

            Assert.That(countersExist, "The EDB performance counters haven't been created");

            var perfCtrSwitch = new TraceSwitch("ConnectionPoolPerformanceCounterDetail", "level of detail to track with connection pool performance counters");
            var verboseCounters = perfCtrSwitch.Level == TraceLevel.Verbose;

            if (!verboseCounters)
                Assert.Fail("Verbose performance counters not enabled");
        }

        [SetUp]
        public void Setup()
        {
            EDBConnection.ClearAllPools();
        }
    }
}
#endif
