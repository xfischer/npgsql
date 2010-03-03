using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

namespace DOTNET
{
	/// <summary>
	/// Summary description for FBs.
	/// </summary>
    [TestFixture]
    public class FBs
    {
        EDBConnection con = null;

        [SetUp]
        public void Init()
        {
            //write setup for following test cases
            con = TestUtil.openDB();
            

        }

        [TearDown]
        public void Dispose()
        {
            TestUtil.closeDB(con);
        }

        [Test]
        public void FB_11665()
        {
            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION surname(a IN INTEGER, b IN VARCHAR2) RETURN VARCHAR2 \n" +
                "   IS\n" +
                "   BEGIN" +
                "	RETURN ('Chief Justice: ' || b || ' Choudhry');" +
                "   END;";

            Command.ExecuteNonQuery();


            Command.CommandText = "create table Quote(id int4, b char)";

            Command.ExecuteNonQuery();

            Command.CommandText = "create or replace procedure quoteproc(abc in integer)\n"
                + "is\n"
                + "declare\n"
                + "i integer:=0;\n"
                + "begin\n"
                + "while i < abc loop\n"
                + "insert into Quote values(1, 't');\n"
                + "i := i+1;\n"
                + "end loop;\n"
                + "end;\n";

            Command.ExecuteNonQuery();



            EDBTransaction tran = con.BeginTransaction();

            EDBCommand edbFunctionCmd = new EDBCommand("surname(:parameter1,:parameter2)", con);
            edbFunctionCmd.CommandType = CommandType.StoredProcedure;
            edbFunctionCmd.Transaction = tran;


            //
            // Note: This line raised exception.
            // Case 11665:   EDBCommandBuilder.DeriveParameters() raises an exception.  
            //
            EDBCommandBuilder.DeriveParameters(edbFunctionCmd);

            edbFunctionCmd.Parameters.Add(new EDBParameter("p3", EDBTypes.EDBDbType.Varchar, 10, "r", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

            edbFunctionCmd.Parameters[0].Value = 10;
            edbFunctionCmd.Parameters[1].Value = "Iftikhar";


            edbFunctionCmd.Prepare();

            //
            // NOTE: The implementation is as such that the EDBDataReader will not populate the result.
            // This is only when CommandType is CommandType.StoredProcedure.
            // 
            EDBDataReader result = edbFunctionCmd.ExecuteReader();
            
            
            //while(result.Read())
            //{
            //    if (result.HasRows)
            //    {
            //        int dept = result.Depth;
            //        int count = result.FieldCount;
            //    }

            //    Console.WriteLine(result.GetValue(0).ToString());
            //    Console.WriteLine(result.GetValue(1).ToString());
            //    Console.WriteLine(result.GetValue(2).ToString());
            //}


            

            Assert.IsNotNull(edbFunctionCmd.Parameters[2].Value);
            Assert.AreEqual("Chief Justice: Iftikhar Choudhry", edbFunctionCmd.Parameters[2].Value.ToString());
            result.Close();
            tran.Commit();

            Command.CommandText = "DROP FUNCTION surname(integer, varchar2)";
            Command.ExecuteNonQuery();
            Command.CommandText = "DROP table quote";
            Command.ExecuteNonQuery();

        }


        [Test]
        public void FB_12481()
        {

            EDBCommand Command = new EDBCommand("", con);
            Command.CommandText = "CREATE OR REPLACE FUNCTION surname(a IN INTEGER, b IN VARCHAR2) RETURN VARCHAR2 \n" +
                "   IS\n" +
                "   BEGIN" +
                "	RETURN ('Chief Justice: ' || b || ' Choudhry');" +
                "   END;";

            Command.ExecuteNonQuery();
            Command.CommandText = "create table Quote(id int4, b char)";
            Command.ExecuteNonQuery();

            Command.CommandText = "create or replace procedure quoteproc(abc in integer)\n"
                + "is\n"
                + "declare\n"
                + "i integer:=0;\n"
                + "begin\n"
                + "while i < abc loop\n"
                + "insert into Quote values(1, 't');\n"
                + "i := i+1;\n"
                + "end loop;\n"
                + "end;\n";

            Command.ExecuteNonQuery();

            
            
            EDBCommand com = new EDBCommand("", con);

            com = new EDBCommand("quoteproc(:a)", con);
            com.CommandType = CommandType.StoredProcedure;

            /*
             * Intentionally provided a short TimeOut value so that exception type 
             * is to be verified. The right exception is EDBException.
             */
            com.CommandTimeout = 2;

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 200;
            com.Prepare();

            try
            {
                /*
                 * Exception is thrown here ...
                 */
                com.ExecuteNonQuery();
                Console.WriteLine("Data inserted");
                DataSet ds = new DataSet();
                Console.WriteLine("selecting data");
                EDBDataAdapter da = new EDBDataAdapter("select * from Quote", con);
                da.Fill(ds);
                Console.WriteLine("selected data");
                Console.WriteLine("filled data=" + ds.Tables[0].Rows.Count);

                Console.WriteLine("Values selected");
                com = new EDBCommand("drop procedure quoteproc", con);
                com.ExecuteNonQuery();
                GC.Collect();
                Command.CommandText = "DROP FUNCTION surname(integer, varchar2)";
                Command.ExecuteNonQuery();
                Command.CommandText = "DROP table quote";
                Command.ExecuteNonQuery();


            }
            catch (EDBException edbException)
            {
                Assert.IsTrue(true);
            }
            catch (Exception exp)
            {
                Assert.IsTrue(false);
            }
        }

    }

}
