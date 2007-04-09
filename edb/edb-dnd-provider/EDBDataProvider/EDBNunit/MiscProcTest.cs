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


			Command.CommandText="CREATE OR REPLACE	FUNCTION GETSYSREFCURSOR RETURN SYS_REFCURSOR AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();
	

			///////////
			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETSYSREFCURSORPROC(A OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE FUNCTION GETSYSREFCURSOR_OUT(R OUT SYS_REFCURSOR) RETURN SYS_REFCURSOR AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"	TEST_REF2 SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT b FROM TESTTAB;\n"+
				"			R:=TEST_REF2;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();


			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETSYSREFCURSORSVVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"	TEST_REF2 SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=TEST_REF2;\n"+
				"		END;";
			Command.ExecuteNonQuery();


			

			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETSYSREFCURSORSVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=A;\n"+
				"		END;";
			Command.ExecuteNonQuery();

				////////////
			Command.CommandText="CREATE OR REPLACE PACKAGE REFCURSOR_PKG IS\n"+
			"	FUNCTION GETREFCURSOR RETURN REFCURSOR;\n"+
				"PROCEDURE GETREFCURSORPROC(A OUT REFCURSOR);\n"+
				"FUNCTION GETREFCURSOR_OUT(R OUT REFCURSOR) RETURN REFCURSOR;\n"+
				"PROCEDURE GETREFCURSORSVVPROC(A OUT REFCURSOR,B OUT REFCURSOR);\n"+
				"PROCEDURE GETREFCURSORSVPROC(A OUT REFCURSOR,B OUT REFCURSOR);\n"+
				"FUNCTION DEFAULTINRETURNFUNC(A IN INT4 DEFAULT 29) RETURN INT4;\n"+
				"PROCEDURE DEFAULTINPROC(B OUT INT4,A IN INT4 DEFAULT 29);\n"+
				"FUNCTION GETSYSREFCURSOR RETURN SYS_REFCURSOR;\n"+
				"PROCEDURE GETSYSREFCURSORPROC(A OUT SYS_REFCURSOR);\n"+
				"FUNCTION GETSYSREFCURSOR_OUT(R OUT SYS_REFCURSOR) RETURN SYS_REFCURSOR;\n"+
				"PROCEDURE GETSYSREFCURSORSVVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR);\n"+
				"PROCEDURE GETSYSREFCURSORSVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR);\n"+
			"END REFCURSOR_PKG;";
			
			Command.ExecuteNonQuery();


			Command.CommandText="CREATE OR REPLACE PACKAGE BODY REFCURSOR_PKG IS\n"
				+ "FUNCTION GETREFCURSOR RETURN REFCURSOR AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;\n"+
				"PROCEDURE GETREFCURSORPROC(A OUT REFCURSOR) AS\n"+
			"	TEST_REF REFCURSOR;\n"+
			"		BEGIN\n"+
			"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
			"			A:=TEST_REF;\n"+
			"		END;"+
				"FUNCTION GETREFCURSOR_OUT(R OUT REFCURSOR) RETURN REFCURSOR AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"	TEST_REF2 REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT b FROM TESTTAB;\n"+
				"			R:=TEST_REF2;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;"+
				"PROCEDURE GETREFCURSORSVVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"	TEST_REF2 REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=TEST_REF2;\n"+
				"		END;"+
				"PROCEDURE GETREFCURSORSVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=A;\n"+
				"		END;"+
				"FUNCTION DEFAULTINRETURNFUNC(A IN INT4 DEFAULT 29) RETURN INT4 AS\n"+
				"		BEGIN\n"+
				"			RETURN A;\n"+
				"		END;"+
				"PROCEDURE DEFAULTINPROC(B OUT INT4,A IN INT4 DEFAULT 29) AS\n"+
				"		BEGIN\n"+
				"			B:=A;\n"+
				"		END;"+
				"FUNCTION GETSYSREFCURSOR RETURN SYS_REFCURSOR AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;"+
								
	

				"PROCEDURE GETSYSREFCURSORPROC(A OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;"+
			

				"FUNCTION GETSYSREFCURSOR_OUT(R OUT SYS_REFCURSOR) RETURN SYS_REFCURSOR AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"	TEST_REF2 SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT b FROM TESTTAB;\n"+
				"			R:=TEST_REF2;\n"+
				"			RETURN TEST_REF;\n"+
				"		END;"+

				"PROCEDURE GETSYSREFCURSORSVVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"	TEST_REF2 SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			OPEN TEST_REF2 FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=TEST_REF2;\n"+
				"		END;"+
				"PROCEDURE GETSYSREFCURSORSVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"			B:=A;\n"+
				"		END;"+
				"END REFCURSOR_PKG;\n";

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

			

			Command.CommandText="DROP PROCEDURE GETREFCURSORSVPROC";
			Command.ExecuteNonQuery();


			Command.CommandText="DROP FUNCTION DEFAULTINRETURNFUNC(INT4)";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE DEFAULTINPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP FUNCTION GETSYSREFCURSOR";
			Command.ExecuteNonQuery();


			Command.CommandText="DROP FUNCTION GETSYSREFCURSOR_OUT(REFCURSOR)";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP PROCEDURE GETSYSREFCURSORPROC";
			Command.ExecuteNonQuery();


			Command.CommandText="DROP PROCEDURE GETSYSREFCURSORSVVPROC";
			Command.ExecuteNonQuery();

			

			Command.CommandText="DROP PROCEDURE GETSYSREFCURSORSVPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP Package REFCURSOR_PKG";
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
		public void RefCursorInvalidFunc()
		{
			bool ex=false;
			try
			{
				EDBCommand command=new EDBCommand("public.getrefcursor()",con);
				command.CommandType=CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
				command.Prepare();

				command.ExecuteReader();
				EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

				Assert.IsNotNull(rst);
				Assert.IsTrue(rst.Read());
				Assert.AreEqual("V1",rst.GetValue(0).ToString());
				Assert.IsTrue(rst.Read());
			
				if(2==int.Parse(rst.GetValue(1).ToString()))
					Assert.IsTrue(true);
				else
					Assert.IsFalse(false);
						
				rst.Close();
			}

			catch(EDBException exp)
			{
				ex=true;
			}
		
			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
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
				
			bool ex=false;
			try
			{
				EDBCommand command = new EDBCommand("public.GETREFCURSOR_OUT(:param1)", con); 
				command.CommandType = CommandType.StoredProcedure; 
				
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
				rst.GetValue(2);
				
				rst.Close();
				rst1.Close();
				
			}
	
			catch(EDBException exp)
			{
				ex=true;
			}

			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
		}
		


		[Test]
		public void RefCursorProc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.getrefcursorproc(:param1)",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
			rst.Close();
			rst.Close();
			tran.Commit();
			
		}


		[Test]
		public void RefCursorInvalidProc()
		{
			
			bool ex=false;

			try
			{
				EDBCommand command=new EDBCommand("public.getrefcursorproc(:param1)",con);
				command.CommandType=CommandType.StoredProcedure;
			
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
				command.Prepare();

				command.ExecuteReader();
				EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

				Assert.IsNotNull(rst);
				Assert.IsTrue(rst.Read());
				Assert.AreEqual("V1",rst.GetValue(0).ToString());

				Assert.IsTrue(rst.Read());
				if(2==int.Parse(rst.GetValue(1).ToString()))
					Assert.IsTrue(true);
				else
					Assert.IsFalse(false);
						
				rst.Close();
				rst.Close();
			}

			catch(EDBException exp)
			{
				ex=true;
			}
			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
		}



		[Test]
		public void RefcursorsVV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSORSVVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

		
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	


		[Test]
		public void RefcursorsV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSORSVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
			Assert.IsFalse(rst1.Read());

			
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	

		[Test]
		public void DefaultInAsReturn()
		{
			
			EDBCommand command = new EDBCommand("public.DEFAULTINRETURNFUNC()", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==29)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
		
			
		}	

		[Test]
		public void DefaultInAsOutProc()
		{
			
			EDBCommand command = new EDBCommand("public.DEFAULTINPROC(:param1)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==29)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
		
			
		}	


		[Test]
		public void SYSRefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.getsysrefcursor()",con);
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
		public void SYSRefCursorProc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.getsysrefcursorproc(:param1)",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
			rst.Close();
			rst.Close();
			tran.Commit();
			
		}


		[Test]
		public void SYSRefcursorsV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETSYSREFCURSORSVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
			Assert.IsFalse(rst1.Read());

			
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	
		
		[Test]
		public void SYSRefcursorsVV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETSYSREFCURSORSVVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

		
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	


		[Test]
		public void SYSRefCursorFuncOut()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETSYSREFCURSOR_OUT(:param1)", con); 
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
		public void PACKAGERefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.getrefcursor()",con);
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
		public void PACKAGERefCursorProc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.getrefcursorproc(:param1)",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
			rst.Close();
			rst.Close();
			tran.Commit();
			
		}

	

		[Test]
		public void PACKGAERefCursorFuncOut()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSOR_OUT(:param1)", con); 
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
		public void PACKAGERefcursorsVV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSVVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

		
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}
	

		[Test]
		public void PACKAGERefcursorsV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
			Assert.IsFalse(rst1.Read());

			
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	

		[Test]
		public void PACKAGEDefaultInAsReturn()
		{
			
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.DEFAULTINRETURNFUNC()", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==29)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
		
			
		}	

		[Test]
		public void PACKAGEDefaultInAsOutProc()
		{
			
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.DEFAULTINPROC(:param1)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==29)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
		
			
		}	

		[Test]
		public void PACKSYSRefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.getsysrefcursor()",con);
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
		public void PACKSYSRefCursorProc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.getsysrefcursorproc(:param1)",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
			rst.Close();
			rst.Close();
			tran.Commit();
			
		}


		[Test]
		public void PACKSYSRefcursorsV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETSYSREFCURSORSVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
			Assert.IsFalse(rst1.Read());

			
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	
		
		[Test]
		public void PACKSYSRefcursorsVV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETSYSREFCURSORSVVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			EDBDataReader rst1 = (EDBDataReader) command.Parameters[1].Value;

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

		
			
			rst.Close();
			rst1.Close();
			tran.Commit();
			
		}	

		[Test]
		public void PACKSYSRefCursorFuncOut()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETSYSREFCURSOR_OUT(:param1)", con); 
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
		public void PACKAGEInvalidRefCursorProc()
		{
			
			bool ex=false;

			try
			{
				EDBCommand command=new EDBCommand("REFCURSOR_PKG.getrefcursorproc(:param1)",con);
				command.CommandType=CommandType.StoredProcedure;
			
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
				command.Prepare();

				command.ExecuteReader();
				EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;

				Assert.IsNotNull(rst);
				Assert.IsTrue(rst.Read());
				Assert.AreEqual("V1",rst.GetValue(0).ToString());

				Assert.IsTrue(rst.Read());
				if(2==int.Parse(rst.GetValue(1).ToString()))
					Assert.IsTrue(true);
				else
					Assert.IsFalse(false);
						
				rst.Close();
				rst.Close();
			}

			catch(EDBException exp)
			{
				ex=true;
			}
			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
			
			
		}


		[Test]
		public void PACKRefCursorInvalidFuncOut()
		{
				
			bool ex=false;
			try
			{
				EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSOR_OUT(:param1)", con); 
				command.CommandType = CommandType.StoredProcedure; 
				
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
				rst.GetValue(2);
				
				rst.Close();
				rst1.Close();
				
			}
	
			catch(EDBException exp)
			{
				ex=true;
			}

			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
		}

	

		
	}
}
