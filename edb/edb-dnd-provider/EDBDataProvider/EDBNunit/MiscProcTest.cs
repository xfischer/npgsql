using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

namespace NUnit
{
	/// <summary>
	/// Summary description for MiscProcTest.
	/// </summary>
	[TestFixture]
	public class MiscProcTest
	{
		EDBConnection con = null;


		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = TestUtil.openDB();
			TestUtil.createTempTable(con,"TESTTAB","a VARCHAR, b INT4");
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="INSERT INTO TESTTAB VALUES('V1',1)";
			Command.ExecuteNonQuery();
			Command.CommandText="INSERT INTO TESTTAB VALUES('V2',2)";
			Command.ExecuteNonQuery();
			

			Command.CommandText="CREATE OR REPLACE	FUNCTION GETREFCURSOR RETURN REFCURSOR AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETREFCURSORPROC(A OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE FUNCTION GETREFCURSOR_OUT(R OUT REFCURSOR) RETURN REFCURSOR AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"	TEST_REF2 REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT b FROM TESTTAB;\n"+
				"			R:=TEST_REF2;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();


			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETREFCURSORSVVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"	TEST_REF2 REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=TEST_REF2;\n"+
				"		END;";
			Command.ExecuteNonQuery();


			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETREFCURSORSIVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();


			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETREFCURSORSIIPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"		BEGIN\n"+
				"			A:=NULL;\n"+
				"			B:=NULL;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETREFCURSORSVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=A;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE	FUNCTION DEFAULTINRETURNFUNC(A IN INT4 DEFAULT 29) RETURN INT4 AS\n"+
				"		BEGIN\n"+
				"			RETURN A;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE	PROCEDURE DEFAULTINPROC(B OUT INT4,A IN INT4 DEFAULT 29) AS\n"+
				"		BEGIN\n"+
				"			B:=A;\n"+
				"		END;";
			Command.ExecuteNonQuery();
	
		}

		[TearDown] 
		public void Dispose()
		{
			
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="DROP Table TESTTAB";
			Command.CommandType=CommandType.Text;
			Command.ExecuteNonQuery();

			Command.CommandText="DROP FUNCTION GETREFCURSOR";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP FUNCTION GETREFCURSOR_OUT(REFCURSOR)";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE GETREFCURSORPROC";
			Command.ExecuteNonQuery();


			Command.CommandText="DROP PROCEDURE GETREFCURSORSVVPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE GETREFCURSORSIVPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE GETREFCURSORSIIPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE GETREFCURSORSVPROC";
			Command.ExecuteNonQuery();


			Command.CommandText="DROP FUNCTION DEFAULTINRETURNFUNC(INT4)";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE DEFAULTINPROC";
			Command.ExecuteNonQuery();

			TestUtil.closeDB(con);
		}

		[Test]
		public void RefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.getrefcursor()",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			rst.Close();
			tran.Commit();
			
		}

		[Test]
		public void RefCursorFuncOut()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSOR_OUT(:param1)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsNotNull(rst1);
			
			rst.Close();
			tran.Commit();
			
		}

		[Test]
		public void RefCursorInvalidFuncOut()
		{
				EDBTransaction tran=con.BeginTransaction();
		
				EDBCommand command = new EDBCommand("public.GETREFCURSOR_OUT(:param1)", con); 
				command.CommandType = CommandType.StoredProcedure; 
				command.Transaction=tran;
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
				command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
				command.Prepare();

				command.ExecuteReader();
				EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
				EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

				Assert.IsNotNull(rst);
				Assert.IsNotNull(rst1);
				Assert.IsTrue(rst.Read());
				Assert.IsTrue(rst1.Read());
				Assert.AreEqual("1",rst.GetValue(0).ToString());
				Assert.IsNotNull(rst);
				Assert.IsNotNull(rst1);
		
				rst.Close();
				rst1.Close();
				tran.Commit();
			

		}
	}
}
