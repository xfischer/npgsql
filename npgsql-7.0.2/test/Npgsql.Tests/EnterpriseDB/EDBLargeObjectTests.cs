using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8604
#pragma warning disable CS8602
    /// <summary>
    /// Tests for EDBLine
    /// </summary>
    /// 
    [TestFixture]
    public class EDBLargeObjectTests : EPASTestBase
    {
        EDBConnection? con = null;
        string testPath = @"C:\Windows\media\Windows Background.wav";

        [SetUp]
        public void Init()
        {
            //write setup for following test cases
            con = OpenConnection();

            EDBCommand command = new EDBCommand("CREATE TABLE LOTest(id serial, f1 oid);", con);
            int result = command.ExecuteNonQuery();
            Console.WriteLine("CREATE TABLE returned " + result);
        }

        [Test, Ignore("MERGE_NEED_TO_EXPLORE")]
        public void LOCreateTest()
        {
            // Retrieve a Large Object Manager for this connection
            var manager = new EDBLargeObjectManager(con);

            // Create a new empty file, returning the identifier to later access it
            uint oid = manager.Create();
            EDBCommand command = new EDBCommand("INSERT INTO LOTest VALUES(1, " + oid.ToString() + "); ", con);

            int rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            command = new EDBCommand("select f1 from LOTest;", con);
            uint oid2 = (uint)command.ExecuteScalar();
            Assert.True(0 != oid, "Invalid OID value");
        }

        [Test]
        public void LOImportTest()
        {
            // Retrieve a Large Object Manager for this connection
            var manager = new EDBLargeObjectManager(con);

            // Create a new empty file, returning the identifier to later access it
            //uint oid = manager.Create();
            EDBCommand command = new EDBCommand("INSERT INTO LOTest VALUES(1, lo_import('" + testPath + "')); ", con);

            int rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            command = new EDBCommand("select f1 from LOTest;", con);
            uint oid = (uint)command.ExecuteScalar();
            Assert.True(0 != oid, "Invalid OID value");
        }

        [Test, /*Ignore("MERGE_NEED_TO_EXPLORE")*/]
        public void CreateTest()
        {
            try
            {
                // Retrieve a Large Object Manager for this connection
                var manager = new EDBLargeObjectManager(con);

                // Create a new empty file, returning the identifier to later access it
                uint oid = manager.Create();

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
                        stream.Read(buf2, 0, buf2.Length);

                        // buf2 now contains 1, 2, 3
                    }
                    // Save the changes to the object
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        [TearDown]
        public void Dispose()
        {
            EDBCommand command = new EDBCommand("DROP TABLE LOTest;", con);
            int result = command.ExecuteNonQuery();
            Console.WriteLine("DROP TABLE returned " + result);

            TestUtil.closeDB(con);
        }
    }
#pragma warning restore CS8604
#pragma warning restore CS8602
}
