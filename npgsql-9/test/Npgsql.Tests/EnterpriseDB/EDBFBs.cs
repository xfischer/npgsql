using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
using EnterpriseDB.EDBClient.Tests.Support;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable CS8604
/// <summary>
/// Summary description for FBs.
/// </summary>
[TestFixture]
public class EDBFBs : EPASTestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void Init()
    {
        //write setup for following test cases
        con = OpenConnection();
        EDBCommand Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION surname1(a IN INTEGER, b IN VARCHAR2) RETURN VARCHAR2 \n" +
                "   IS\n" +
                "   BEGIN" +
                "	RETURN ('Chief Justice: ' || b || ' Choudhry');" +
                "   END;"
        };

        Console.WriteLine("CREATE surname1 status: " + Command.ExecuteNonQuery());

        Command.CommandText = "CREATE TABLE IF NOT EXISTS Quote(id int4, b char)";

        Command.ExecuteNonQuery();


    }

    [TearDown]
    public void Dispose()
    {
        EDBCommand Command = new EDBCommand("", con)
        {
            CommandText = "DROP FUNCTION surname1(integer, varchar2)"
        };
        Command.ExecuteNonQuery();
        Command.CommandText = "DROP TABLE IF EXISTS Quote";
        Command.ExecuteNonQuery();

        TestUtil.closeDB(con);
    }

    [Test, EDBExplicit("Needs to be fixed")]
    public void FB_11665()
    {
        Assert.DoesNotThrow(() =>
        {
            EDBCommand edbFunctionCmd = new EDBCommand("surname1", con)
            {
                CommandType = CommandType.StoredProcedure
            };

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




            //      Assert.IsNotNull(edbFunctionCmd.Parameters[2].Value);
            //    Assert.AreEqual("Chief Justice: Iftikhar Choudhry", edbFunctionCmd.Parameters[2].Value.ToString());
            result.Close();
        });

    }


    [Test]
    public void FB_12481()
    {
        Assert.DoesNotThrow(() =>
        {
            EDBCommand Command = new EDBCommand("", con)
            {
                CommandText = "create or replace procedure quoteproc(abc in integer)\n"
                    + "is\n"
                    + "declare\n"
                    + "i integer:=0;\n"
                    + "begin\n"
                    + "while i < abc loop\n"
                    + "INSERT INTO Quote values(1, 't');\n"
                    + "i := i+1;\n"
                    + "end loop;\n"
                    + "end;\n"
            };

            Command.ExecuteNonQuery();

            EDBCommand com;

            com = new EDBCommand("quoteproc(:a)", con)
            {
                CommandType = CommandType.StoredProcedure,

                /*
                 * Intentionally provided a short TimeOut value so that exception type 
                 * is to be verified. The right exception is EDBException.
                 */
                CommandTimeout = 2
            };

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 200;
            com.Prepare();


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
            Console.WriteLine("Values selected");
            com = new EDBCommand("drop procedure quoteproc", con);
            com.ExecuteNonQuery();

        });
    }

}
#pragma warning restore CS8604

