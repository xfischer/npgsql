using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace DOTNET
{
	/// <summary>
	/// Testing Procedures with Different combination of parameters
	/// </summary>
	[TestFixture]
	public class CursorTest
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{
			con = TestUtil.openDB();

			EDBCommand com = new EDBCommand("",con);
			com.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PROCEDURE CUR_TEST(v_id OUT NUMERIC)\n"
							+ "IS\n"
//							+ "v_id number;\n"
							+ "v_name varchar2(20);\n"
							+ "CURSOR c_name IS SELECT EMPNO,ENAME FROM EMP;\n"
							+ "BEGIN\n"
							+ "OPEN c_name;\n"
							+ "FETCH c_name into v_id,v_name;\n"
							+ "CLOSE c_name;\n"
							+ "DBMS_OUTPUT.PUT_LINE(v_id);\n"
							+ "DBMS_OUTPUT.PUT_LINE(v_name);\n"
							+ "END;\n";
			com.CommandText = strSql;
			com.ExecuteNonQuery();

		}

		[TearDown] 
		public void Dispose()
		{
			EDBCommand com = new EDBCommand("",con);
			com.CommandType = CommandType.Text;

			com.CommandText = "DROP PROCEDURE CUR_TEST;";
			com.ExecuteNonQuery();

			TestUtil.closeDB(con);
		}
		[Test]
		public void testSelect() //Have to change the dependancy on emp table
		{
			/*try 
			{
				EDBCommand command = new EDBCommand("CUR_TEST(:v_id)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("v_id", 
					EDBTypes.EDBDbType.Numeric,10,"v_id",
					ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));

				command.Prepare();
				command.ExecuteNonQuery();

				Assert.AreEqual(7521,int.Parse(command.Parameters[0].Value.ToString()));
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}*/
		}
	}
}
