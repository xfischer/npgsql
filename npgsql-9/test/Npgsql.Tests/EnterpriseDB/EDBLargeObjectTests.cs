using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    /// <summary>
    /// Tests for EDBLine
    /// </summary>
    /// 
    [TestFixture]
    [NonParallelizable]
    public class EDBLargeObjectTests : EPASTestBase
    {
        EDBConnection? con = null;
        readonly string testPath = @"C:\Windows\media\Windows Background.wav";

        [SetUp]
        public void Init()
        {
            //write setup for following test cases
            con = OpenConnection();

            var command = new EDBCommand("CREATE TABLE LOTest(id serial, f1 oid);", con);
            var result = command.ExecuteNonQuery();
            Console.WriteLine("CREATE TABLE returned " + result);
        }

        [Test, Ignore("MERGE_NEED_TO_EXPLORE")]
        [Obsolete("EDBLargeObjectManager is obsoleted by community")]
        public void LOCreateTest()
        {
            // Retrieve a Large Object Manager for this connection
            var manager = new EDBLargeObjectManager(con);

            // Create a new empty file, returning the identifier to later access it
            var oid = manager.Create();
            var command = new EDBCommand("INSERT INTO LOTest VALUES(1, " + oid.ToString() + "); ", con);

            var rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            command = new EDBCommand("select f1 from LOTest;", con);
            var oid2 = (uint)command.ExecuteScalar()!;
            Assert.NotZero(oid2, "Invalid OID value");
        }

        [Test]
        [Obsolete("EDBLargeObjectManager is obsoleted by community")]
        public void LOImportTest()
        {
            // Retrieve a Large Object Manager for this connection
            var manager = new EDBLargeObjectManager(con);

            // Create a new empty file, returning the identifier to later access it
            var command = new EDBCommand("INSERT INTO LOTest VALUES(1, lo_import('" + testPath + "')); ", con);

            var rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            command = new EDBCommand("select f1 from LOTest;", con);
            var oid = (uint)command.ExecuteScalar()!;
            Assert.NotZero(oid, "Invalid OID value");
        }

        [Test]
        [Obsolete("EDBLargeObjectManager is obsoleted by community")]
        public void CreateTest()
        {
            Assert.DoesNotThrow(() =>
            {
                // Retrieve a Large Object Manager for this connection
                var manager = new EDBLargeObjectManager(con);

                // Create a new empty file, returning the identifier to later access it
                var oid = manager.Create();

                // Reading and writing Large Objects requires the use of a transaction
                using (var transaction = con.BeginTransaction())
                {
                    // Open the file for reading and writing
                    using (var stream = manager.OpenReadWrite(oid))
                    {
                        var buf = new byte[] { 1, 2, 3 };
                        stream.Write(buf, 0, buf.Length);
                        stream.Seek(0, System.IO.SeekOrigin.Begin);

                        var buf2 = new byte[buf.Length];
                        _ = stream.Read(buf2, 0, buf2.Length);

                        // buf2 now contains 1, 2, 3
                    }
                    // Save the changes to the object
                    transaction.Commit();
                }
            });
        }

        [TearDown]
        public void Dispose()
        {
            var command = new EDBCommand("DROP TABLE LOTest;", con);
            var result = command.ExecuteNonQuery();
            Console.WriteLine("DROP TABLE returned " + result);

            TestUtil.closeDB(con);
        }
    }
}
