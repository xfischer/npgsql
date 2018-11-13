using System;
using System.Data;

using EnterpriseDB.EDBClient;
using NUnit.Framework;

namespace NpgsqlTests
{
    /// <summary>
    /// Summary description for PrepareTest.
    /// </summary>
    [TestFixture]
    public class PrepareTest : BaseClassTests
    {
        protected override EDBConnection TheConnection {
            get { return _conn; }
        }
        protected override EDBTransaction TheTransaction {
            get { return _t; }
            set { _t = value; }
        }
        protected override void SetUp()
        {
            base.SetUp();
            string sql = @"	CREATE TABLE public.preparetest
                         (
                         testid serial NOT NULL,
                         varchar_notnull varchar(100) NOT NULL,
                         varchar_null varchar(100),
                         integer_notnull int4 NOT NULL,
                         integer_null int4,
                         bigint_notnull int8 NOT NULL,
                         bigint_null int8
                         ) WITHOUT OIDS;";
            EDBCommand cmd = new EDBCommand(sql, TheConnection);
            cmd.ExecuteNonQuery();
            CommitTransaction = true;
        }


        
        protected override void TearDown()
        {
            
            string sql = @"	DROP TABLE public.preparetest;";
            EDBCommand cmd = new EDBCommand(sql, TheConnection);

            cmd.ExecuteNonQuery();
            CommitTransaction = true;
            base.TearDown();
        }
        

        [Test]
        public void TestInt8Null()
        {
            EDBCommand cmd = GetCommand();


            // Default params work OK
            cmd.ExecuteNonQuery();

            cmd.Parameters[5].Value = System.DBNull.Value;
            //cmd.Parameters[5].Value = null;

            // This too
            cmd.ExecuteNonQuery();

            cmd.Prepare();

            // This will fail
            cmd.ExecuteNonQuery();
        }

        [Test]
        public void TestInt4Null()
        {
            EDBCommand cmd = GetCommand();


            // Default params work OK
            cmd.ExecuteNonQuery();

            cmd.Parameters[3].Value = System.DBNull.Value;

            // This too
            cmd.ExecuteNonQuery();

            cmd.Prepare();

            // This will fail
            cmd.ExecuteNonQuery();
        }

        [Test]
        public void TestVarcharNull()
        {
            EDBCommand cmd = GetCommand();


            // Default params work OK
            cmd.ExecuteNonQuery();

            cmd.Parameters[1].Value = System.DBNull.Value;

            // This too
            cmd.ExecuteNonQuery();

            cmd.Prepare();

            // This inserts a string with the value 'NULL'
            cmd.ExecuteNonQuery();
        }

        private EDBCommand GetCommand()
        {
            string sql = @"	INSERT INTO preparetest(varchar_notnull, varchar_null, integer_notnull, integer_null, bigint_notnull, bigint_null)
                         VALUES(:param1, :param2, :param3, :param4, :param5, :param6)";
            EDBCommand cmd = new EDBCommand(sql, TheConnection);

            EDBParameter p1 = new EDBParameter("param1", DbType.String, 100);
            p1.Value = "One";
            cmd.Parameters.Add(p1);
            EDBParameter p2 = new EDBParameter("param2", DbType.String, 100);
            p2.Value = "Two";
            cmd.Parameters.Add(p2);
            EDBParameter p3 = new EDBParameter("param3", DbType.Int32);
            p3.Value = 3;
            cmd.Parameters.Add(p3);
            EDBParameter p4 = new EDBParameter("param4", DbType.Int32);
            p4.Value = 4;
            cmd.Parameters.Add(p4);
            EDBParameter p5 = new EDBParameter("param5", DbType.Int64);
            p5.Value = 5;
            cmd.Parameters.Add(p5);
            EDBParameter p6 = new EDBParameter("param6", DbType.Int64);
            p6.Value = 6;
            cmd.Parameters.Add(p6);

            return cmd;
        }

        [Test]
        public void TestSubquery()
        {
            string sql = "SELECT testid FROM preparetest WHERE :p1 IN (SELECT varchar_notnull FROM preparetest)";
            EDBCommand cmd = new EDBCommand(sql, TheConnection);
            EDBParameter p1 = new EDBParameter(":p1", DbType.String);
            p1.Value = "blahblah";
            cmd.Parameters.Add(p1);


            cmd.ExecuteNonQuery(); // Succeeds

            cmd.Prepare(); // Fails

            cmd.ExecuteNonQuery();
        }
    }
    [TestFixture]
    public class PrepareTestV2 : PrepareTest
    {
        protected override EDBConnection TheConnection {
            get { return _connV2; }
        }
        protected override EDBTransaction TheTransaction {
            get { return _tV2; }
        }
    }
}
