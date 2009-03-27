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

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION surname(a IN INTEGER, b IN VARCHAR2) RETURN VARCHAR2 \n" +
                "   IS\n" +
                "   BEGIN" +
                "	RETURN ('Chief Justice: ' || b || ' Choudhry');" +
                "   END;";

            Command.ExecuteNonQuery();
        }

        [TearDown]
        public void Dispose()
        {
            EDBCommand Command = new EDBCommand("", con);
            Command.CommandType = CommandType.Text;
            Command.CommandText = "DROP FUNCTION surname(integer, varchar2)";
            Command.ExecuteNonQuery();
            TestUtil.closeDB(con);
        }

        [Test]
        public void FB_11665()
        {
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
        }


    }

}
