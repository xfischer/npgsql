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
    public class RMs
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
        public void RM_22730()
        {
           try
            {

            EDBCommand Command = new EDBCommand("", con);
            //	Create Procedure
            string procedure_string_create = "CREATE OR REPLACE PROCEDURE test(v_name IN character varying, v_desc OUT character varying) \n"
                + " AS \n"
                + " BEGIN \n"
                + "    select count(*) into v_desc from dual; \n"
                + " END; \n";
            Command.CommandText = procedure_string_create;
            Command.ExecuteNonQuery();
            
            EDBTransaction t = con.BeginTransaction();
            Call_Procedure_1(con, t);
            //This used to fail with seek exception previously. (This stream does not support seek operations.)
            Call_Procedure_2(con, t);
            t.Commit();

            //Drop Procedure
            string procedure_string_drop = "DROP PROCEDURE test";
            Command.CommandText = procedure_string_drop;
            Command.ExecuteNonQuery();
            }
            catch (EDBException e)
            {
                throw new Exception("\nCreate/Drop Procedure Incomplete!\n" + e.ToString());
            }
        }

        private void Call_Procedure_1(EDBConnection connection, EDBTransaction trans)
        {
            try
            {

                EDBCommand com = new EDBCommand("test(:a, :b)", connection);

                com.CommandType = CommandType.StoredProcedure;
                com.Transaction = trans;

                EDBParameter a = new EDBParameter("a", EDBTypes.EDBDbType.Varchar2);
                a.Direction = ParameterDirection.Input; 
                com.Parameters.Add(a);


                EDBParameter b = new EDBParameter("b", EDBTypes.EDBDbType.Varchar2);
                b.Direction = ParameterDirection.Output; 
                com.Parameters.Add(b);

                com.Prepare();

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                EDBDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(reader[0]);
                }
                reader.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                throw err;
            }
        }
        ////////////////////
        private void Call_Procedure_2(EDBConnection connection, EDBTransaction trans)
        {
            try
            {
                EDBCommand com = new EDBCommand("test(:c, :d)", connection);

                com.CommandType = CommandType.StoredProcedure;
                com.Transaction = trans;

                EDBParameter c = new EDBParameter("c", EDBTypes.EDBDbType.Varchar2);
                c.Direction = ParameterDirection.Input;
                com.Parameters.Add(c);

                EDBParameter d = new EDBParameter("d", EDBTypes.EDBDbType.Varchar2);
                d.Direction = ParameterDirection.Output; 
                com.Parameters.Add(d);

                com.Prepare();

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                EDBDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(reader[0]);
            
                }
                reader.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                throw err;
            }
        }
        ////////////////////////////////

    }

}
