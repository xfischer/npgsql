using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
using EDBTypes;
using System.Collections.Generic;

namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8600
#pragma warning disable CS8604
#pragma warning disable CS8602
#nullable disable
    /// <summary>
    /// Summary description for MiscProcTest.
    /// </summary>
    [TestFixture]
	public class EDBMiscProcTest : TestBase
	{
		EDBConnection con = null;

        #region Setup / Tear Down
        [SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
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

			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETREFCURSORSIVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETSYSREFCURSORSIVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
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

			Command.CommandText="CREATE OR REPLACE	PROCEDURE GETSYSREFCURSORSIIPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"		BEGIN\n"+
				"			A:=NULL;\n"+
				"			B:=NULL;\n"+
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
				"PROCEDURE GETREFCURSORSIVPROC(A OUT REFCURSOR,B OUT REFCURSOR);\n"+
				"PROCEDURE GETREFCURSORSIIPROC(A OUT REFCURSOR,B OUT REFCURSOR);\n"+
				"PROCEDURE GETSYSREFCURSORSIVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR);\n"+
				"PROCEDURE GETSYSREFCURSORSIIPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR);\n"+
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
				"PROCEDURE GETREFCURSORSIVPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"	TEST_REF REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;"+
				"PROCEDURE GETREFCURSORSIIPROC(A OUT REFCURSOR,B OUT REFCURSOR) AS\n"+
				"		BEGIN\n"+
				"			A:=NULL;\n"+
				"			B:=NULL;\n"+
				"		END;"+
				"PROCEDURE GETSYSREFCURSORSIIPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"		BEGIN\n"+
				"			A:=NULL;\n"+
				"			B:=NULL;\n"+
				"		END;"+
				"PROCEDURE GETSYSREFCURSORSIVPROC(A OUT SYS_REFCURSOR,B OUT SYS_REFCURSOR) AS\n"+
				"	TEST_REF SYS_REFCURSOR;\n"+
				"		BEGIN\n"+
				"			OPEN TEST_REF FOR SELECT * FROM TESTTAB;\n"+
				"			A:=TEST_REF;\n"+
				"		END;"+
				"END REFCURSOR_PKG;\n";
			Command.ExecuteNonQuery();
		
		}

		[TearDown] 
		public void Dispose()
		{
			
			//EDBCommand Command=new EDBCommand("",con);
			//Command.CommandText="DROP Table TESTTAB";
			//Command.CommandType=CommandType.Text;
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP FUNCTION GETREFCURSOR";
			//Command.ExecuteNonQuery();

   //         Command.CommandText = "DROP FUNCTION GETREFCURSOR_OUT(R OUT REFCURSOR)";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE GETREFCURSORPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE GETREFCURSORSVVPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE GETREFCURSORSVPROC";
			//Command.ExecuteNonQuery();

   //         Command.CommandText = "DROP FUNCTION DEFAULTINRETURNFUNC(IN INT4 )";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE DEFAULTINPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP FUNCTION GETSYSREFCURSOR";
			//Command.ExecuteNonQuery();

   //         Command.CommandText = "DROP FUNCTION  GETSYSREFCURSOR_OUT(OUT SYS_REFCURSOR)";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE GETSYSREFCURSORPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE GETSYSREFCURSORSVVPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP PROCEDURE GETSYSREFCURSORSVPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP procedure GETREFCURSORSIVPROC;";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP procedure GETSYSREFCURSORSIVPROC;";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP procedure GETREFCURSORSIIPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP procedure GETSYSREFCURSORSIIPROC";
			//Command.ExecuteNonQuery();

			//Command.CommandText="DROP Package REFCURSOR_PKG";
			//Command.ExecuteNonQuery();

			TestUtil.closeDB(con);
		}
        #endregion

        [Test, Ignore("Investiage Prompt")]
		public void RefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.GETREFCURSOR", con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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
				EDBCommand command=new EDBCommand("public.getrefcursor",con);
				command.CommandType=CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
				command.Prepare();
                command.ExecuteNonQuery();
                String cursorName = command.Parameters[0].Value.ToString();
                command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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

			catch(EDBException )
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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst1);
			
			rst1.Close();
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
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
				command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
				command.Prepare();
                command.ExecuteNonQuery();
                String cursorName1 = command.Parameters[0].Value.ToString();
                String cursorName2 = command.Parameters[1].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
				Assert.IsNotNull(rst);
				Assert.IsTrue(rst.Read());
				Assert.AreEqual("1",rst.GetValue(0).ToString());
				Assert.IsNotNull(rst);
				rst.GetValue(2);
                rst.Close();

                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

                Assert.IsNotNull(rst1);
                Assert.IsTrue(rst1.Read());
                Assert.IsNotNull(rst1);
                rst1.Close();
				
			}
	
			catch(EDBException )
			{
				ex=true;
			}

			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
		}
		[Test, Ignore("Investigate")]
		public void RefCursorProc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.getrefcursorproc(:param1)",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();

            String cursorName = command.Parameters[0].Value.ToString();
            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
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
			
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
				command.Prepare();
                command.ExecuteNonQuery();
                String cursorName = command.Parameters[0].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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

			catch(EDBException )
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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst);
            Assert.IsTrue(rst.Read());
            Assert.IsTrue(rst.Read());
            rst.Close();
            
            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsFalse(rst.Read());
			Assert.IsFalse(rst1.Read());

			rst1.Close();
			tran.Commit();
			
		}	

//		[Test]
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

	//	[Test]
		public void DefaultInAsOutProc()
		{
			
			EDBCommand command = new EDBCommand("public.DEFAULTINPROC(:param1)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

			EDBDataReader Reader=  command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==29)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);

            Reader.Close();
		}	

		[Test, Ignore("Investiage Prompt")]
		public void SYSRefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("public.getsysrefcursor()",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsFalse(rst1.Read());

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			
			rst.Close();
            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst1);

            rst1.Close();
            tran.Commit();
			
		}

		[Test, Ignore("Investiage Prompt")]
		public void PACKAGERefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.GETREFCURSOR", con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();

            String cursorName = command.Parameters[0].Value.ToString();
            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			rst.Close();
			tran.Commit();
			
		}

		[Test, Ignore("Investigate")]
		public void PACKAGERefCursorProc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.getrefcursorproc(:param1)",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());

			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
						
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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst1);
            
            rst1.Close();
			tran.Commit();
			
		}

		[Test]
		public void PACKAGERefcursorsVV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSVVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsFalse(rst1.Read());

			rst1.Close();
			tran.Commit();
			
		}	

		[Test]//, Ignore("Investigate default params failure")]
		public void PACKAGEDefaultInAsReturn()
		{
			
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.DEFAULTINRETURNFUNC()", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			
			command.Prepare();

            command.ExecuteNonQuery();
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

			EDBDataReader reader=  command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==29)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
            reader.Close();
			
		}	

		[Test, Ignore("Investiage Prompt")]
		public void PACKSYSRefCursorFunc()
		{
			EDBTransaction tran=con.BeginTransaction();
			
			EDBCommand command=new EDBCommand("REFCURSOR_PKG.getsysrefcursor()",con);
			command.CommandType=CommandType.StoredProcedure;
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName = command.Parameters[0].Value.ToString();
            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.IsTrue(rst.Read());
			Assert.IsFalse(rst.Read());
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsFalse(rst1.Read());

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			if(2==int.Parse(rst.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst1);
			Assert.IsTrue(rst1.Read());
			Assert.AreEqual("V1",rst1.GetValue(0).ToString());
			Assert.IsTrue(rst1.Read());
			if(2==int.Parse(rst1.GetValue(1).ToString()))
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);
            rst.Close();

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.IsNotNull(rst1);
			
            rst1.Close();
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
			
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
				command.Prepare();
                command.ExecuteNonQuery();
                String cursorName = command.Parameters[0].Value.ToString();
                command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

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

			catch(EDBException )
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
				
				command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
				command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
				command.Prepare();
                command.ExecuteNonQuery();
                String cursorName1 = command.Parameters[0].Value.ToString();
                String cursorName2 = command.Parameters[1].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

                Assert.IsNotNull(rst);
				Assert.IsTrue(rst.Read());
				Assert.AreEqual("1",rst.GetValue(0).ToString());
				Assert.IsNotNull(rst);
				rst.GetValue(2);
				
				rst.Close();
				
                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst1 = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
                Assert.IsNotNull(rst1);
                Assert.IsTrue(rst1.Read());
                Assert.IsNotNull(rst1);

                rst1.Close();

            }
	
			catch(EDBException )
			{
				ex=true;
			}

			if(!ex) 
				Assert.Fail("Expected an exception. Cursor should be invalid");
		}

		[Ignore("Investigate default params failure")]
		public void DefaultInBindAsReturn()
		{
			EDBCommand command = new EDBCommand("public.DEFAULTINRETURNFUNC(:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,55)); 
            
			command.Prepare();
            command.ExecuteNonQuery();

            Console.WriteLine(command.Parameters[1].Value.ToString());
			int a= int.Parse(command.Parameters[1].Value.ToString());
			
			if(a==55)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
            
		}

		[Test]
		public void DefaultInBindAsOutProc()
		{
			
			EDBCommand command = new EDBCommand("public.DEFAULTINPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,33)); 

			command.Prepare();

			EDBDataReader reader= command.ExecuteReader();
		int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==33)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);

            reader.Close();
		}	
		[Test, Ignore("Investigate default params failure")]
		public void PACKDefaultInBindAsReturn()
		{
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.DEFAULTINRETURNFUNC(:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,55));
            command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

            command.Prepare();
            command.ExecuteNonQuery();

            Console.WriteLine(command.Parameters[1].Value.ToString());
			int a= int.Parse(command.Parameters[1].Value.ToString());
			
			if(a==55)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);

		}

		[Test]
		public void PACKDefaultInBindAsOutProc()
		{
			
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.DEFAULTINPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,33)); 

			command.Prepare();

		  EDBDataReader reader = command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==33)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);
            reader.Close();
		}
		[Test]
		public void RefcursorsIV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSORSIVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
            rst.Close();
            
            Assert.AreEqual("", cursorName2);

			tran.Commit();
			
		}	

		[Test]
		public void RefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

			Assert.AreEqual("", cursorName2);
			Assert.AreEqual("", cursorName2);
			tran.Commit();
			
		}	

		[Test]
		public void PACKRefcursorsIV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSIVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			rst.Close();
		
			tran.Commit();

            Assert.AreEqual("", cursorName2);
            Console.WriteLine(cursorName2);

        }
	
		[Test]
		public void PACKRefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            string rst =  command.Parameters[0].Value.ToString();
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.AreEqual("",rst);
			Assert.AreEqual("",rst1);
			tran.Commit();
			
		}

		[Test]
		public void SYSRefcursorsIV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETSYSREFCURSORSIVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);
            rst.Close();

            Assert.AreEqual("",cursorName2);
			
			Console.WriteLine(cursorName2);
		
			tran.Commit();
			
		}	

		[Test, Ignore("Investigate")]
		public void SYSRefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETSYSREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			string rst =  command.Parameters[0].Value.ToString();
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.AreEqual("",rst);
			Assert.AreEqual("",rst1);
			tran.Commit();
			
		}	

		[Test]
		public void PACKSYSRefcursorsIV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETSYSREFCURSORSIVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();
            String cursorName1 = command.Parameters[0].Value.ToString();
            String cursorName2 = command.Parameters[1].Value.ToString();

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
            
			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			rst.Close();
			tran.Commit();

            Console.WriteLine(cursorName2);
            Assert.AreEqual("", cursorName2);

        }
	
		[Test, Ignore("Investigate")]
		public void PACKSYSRefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETSYSREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Refcursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();
            command.ExecuteNonQuery();

            string rst =  command.Parameters[0].Value.ToString();
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.AreEqual("",rst);
			Assert.AreEqual("",rst1);
			tran.Commit();
			
		}

    }
#pragma warning restore CS8602
#pragma warning restore CS8604
#pragma warning restore CS8600
#nullable restore

    [TestFixture]
    public class EC2114_Fix_Tests : TestBase
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        EDBConnection con = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        [SetUp]
        public void Init()
        {
            //write setup for following test cases
            con = OpenConnection();
            var Command = new EDBCommand("", con);
            Command.CommandText = "CREATE OR REPLACE PACKAGE pkgTest Is\n" +
            "Type rsOut      Is Ref Cursor;\n" +

            "Error_Msg Exception;\n" +
            "gstrErrorMessage Varchar2(1000);\n" +

            "Procedure proGetTestData(ParamData In Clob, ErrorData Out Clob, ProcessData Out Clob, curResult Out rsOut);\n" +

            "End pkgTest;";

            Command.ExecuteNonQuery();

            Command.CommandText = "CREATE OR REPLACE PACKAGE BODY pkgTest\n" +
                "Is\n" +
                    "Procedure proGetTestData(ParamData In Clob, ErrorData Out Clob, ProcessData Out Clob, curResult Out rsOut)\n" +
                    "Is\n" +
                    "Begin\n" +
                        "Open curResult For\n" +
                        "Select 1 As EmpID, 'Kiran' As EmpName From Dual\n" +
                        "Union\n" +
                        "Select 2 As EmpID, 'Peter' As EmpName From Dual\n" +
                        "Union\n" +
                        "Select 3 As EmpID, 'Monica' As EmpName From Dual;\n" +
                        "ProcessData:= 'Success';\n" +
                    "Exception\n" +
                        "When Others Then\n" +
                         "Open curResult For Select 1 From Dual;\n" +
                        "ErrorData:= Sqlerrm;\n" +
                    "End proGetTestData;\n" +
                    "End pkgTest;";
            Command.ExecuteNonQuery();

            Command.CommandText = "CREATE OR REPLACE PACKAGE pkgTest2 Is\n" +
            "Type rsOut      Is Ref Cursor;\n" +

            "Error_Msg Exception;\n" +
            "gstrErrorMessage Varchar2(1000);\n" +

            "Procedure proGetTestData(ParamData In Clob, curResult1 Out rsOut, curResult2 Out rsOut);\n" +

            "End pkgTest2;";

            Command.ExecuteNonQuery();

            Command.CommandText = "CREATE OR REPLACE PACKAGE BODY pkgTest2\n" +
                "Is\n" +
                    "Procedure proGetTestData(ParamData In Clob, curResult1 Out rsOut, curResult2 Out rsOut)\n" +
                    "Is\n" +
                    "Begin\n" +
                        "Open curResult1 For\n" +
                        "Select 1 As EmpID, 'Kiran1' As EmpName From Dual\n" +
                        "Union\n" +
                        "Select 2 As EmpID, 'Peter1' As EmpName From Dual\n" +
                        "Union\n" +
                        "Select 3 As EmpID, 'Monica1' As EmpName From Dual;\n" +

                        "Open curResult2 For\n" +
                        "Select 4 As EmpID, 'Kiran2' As EmpName From Dual\n" +
                        "Union\n" +
                        "Select 5 As EmpID, 'Peter2' As EmpName From Dual\n" +
                        "Union\n" +
                        "Select 6 As EmpID, 'Monica2' As EmpName From Dual;\n" +
                    "Exception\n" +
                        "When Others Then\n" +
                         "Open curResult1 For Select 1 From Dual;\n" +
                         "Open curResult2 For Select 2 From Dual;\n" +
                    "End proGetTestData;\n" +
                    "End pkgTest2;";
            Command.ExecuteNonQuery();
        }
        [TearDown]
        public void Dispose()
        {
            var Command = new EDBCommand("", con);

            Command.CommandText = "DROP PACKAGE BODY pkgTest";
            Command.ExecuteNonQuery();

            Command.CommandText = "DROP PACKAGE pkgTest";
            Command.ExecuteNonQuery();

            Command.CommandText = "DROP PACKAGE BODY pkgTest2";
            Command.ExecuteNonQuery();

            Command.CommandText = "DROP PACKAGE pkgTest2";
            Command.ExecuteNonQuery();
        }

        private void RunCustomerCase(string query)
        {
            EDBTransaction tran = con.BeginTransaction();
            var command = new EDBCommand(query, con);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = tran;
            command.Parameters.Add(new EDBParameter("param_data", EDBTypes.EDBDbType.Varchar, 12, "param_data", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "Test"));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            command.Parameters.Add(new EDBParameter("e_data", EDBTypes.EDBDbType.Varchar, 12, "e_data", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            command.Parameters.Add(new EDBParameter("proc_data", EDBTypes.EDBDbType.Varchar, 12, "proc_data", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            command.Parameters.Add(new EDBParameter("v_cur", EDBTypes.EDBDbType.Refcursor, 12, "v_cur", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            command.Prepare();
            command.ExecuteNonQuery();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var cursorName = command.Parameters[3].Value.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

            //We are not sure about the order in which the cursor will return values.
            //So we put these values in lists and later verify the values exist in list.
            var items1 = new List<string>();
            var items2 = new List<string>();
            while (cur.Read())
            {
#pragma warning disable CS8604 // Possible null reference argument.
                items1.Add(cur[0].ToString());
                items2.Add(cur[1].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            }

            cur.Close();
            tran.Commit();

            //We should have three values.
            Assert.AreEqual(3, items1.Count);
            Assert.AreEqual(3, items1.Count);

            //These values should exist in the lists.
            Assert.Contains("1", items1);
            Assert.Contains("2", items1);
            Assert.Contains("3", items1);

            Assert.Contains("Kiran", items2);
            Assert.Contains("Peter", items2);
            Assert.Contains("Monica", items2);
        }

        [Test]
        public void CustomerCase79248_Exact()
        {
            RunCustomerCase("pkgTest.proGetTestData");
        }

        [Test]
        public void CustomerCase79248_EDBWay()
        {
            RunCustomerCase("pkgTest.proGetTestData(:param_data,:e_data,:proc_data,:v_cur)");
        }

        private void RunPkgProcTwoOutCursors(string query)
        {
            EDBTransaction tran = con.BeginTransaction();
            var command = new EDBCommand(query, con);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = tran;
            command.Parameters.Add(new EDBParameter("param_data", EDBTypes.EDBDbType.Varchar, 12, "param_data", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "Test"));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            command.Parameters.Add(new EDBParameter("v_cur1", EDBTypes.EDBDbType.Refcursor, 12, "v_cur1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            command.Parameters.Add(new EDBParameter("v_cur2", EDBTypes.EDBDbType.Refcursor, 12, "v_cur2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            command.Prepare();
            command.ExecuteNonQuery();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var cursorName1 = command.Parameters[1].Value.ToString();
            var cursorName2 = command.Parameters[2].Value.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader cur1 = command.ExecuteReader(CommandBehavior.SequentialAccess);

            //We are not sure about the order in which the cursor will return values.
            //So we put these values in lists and later verify the values exist in list.
            var items1 = new List<string>();
            var items2 = new List<string>();
            while (cur1.Read())
            {
#pragma warning disable CS8604 // Possible null reference argument.
                items1.Add(cur1[0].ToString());
                items2.Add(cur1[1].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            }

            cur1.Close();

            //We should have three values.
            Assert.AreEqual(3, items1.Count);
            Assert.AreEqual(3, items2.Count);

            //These values should exist in the lists.
            Assert.Contains("1", items1);
            Assert.Contains("2", items1);
            Assert.Contains("3", items1);

            Assert.Contains("Kiran1", items2);
            Assert.Contains("Peter1", items2);
            Assert.Contains("Monica1", items2);

            command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
            command.CommandType = CommandType.Text;
            EDBDataReader cur2 = command.ExecuteReader(CommandBehavior.SequentialAccess);

            //We are not sure about the order in which the cursor will return values.
            //So we put these values in lists and later verify the values exist in list.
            var items3 = new List<string>();
            var items4 = new List<string>();
            while (cur2.Read())
            {
#pragma warning disable CS8604 // Possible null reference argument.
                items3.Add(cur2[0].ToString());
                items4.Add(cur2[1].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            }

            cur2.Close();
            tran.Commit();

            //We should have three values.
            Assert.AreEqual(3, items3.Count);
            Assert.AreEqual(3, items4.Count);

            //These values should exist in the lists.
            Assert.Contains("4", items3);
            Assert.Contains("5", items3);
            Assert.Contains("6", items3);

            Assert.Contains("Kiran2", items4);
            Assert.Contains("Peter2", items4);
            Assert.Contains("Monica2", items4);
        }

        [Test]
        public void PkgProcTwoOutCursors_ProcNameONLY()
        {
            RunPkgProcTwoOutCursors("pkgTest2.proGetTestData");
        }

        [Test]
        public void PkgProcTwoOutCursors_EDBWay()
        {
            RunPkgProcTwoOutCursors("pkgTest2.proGetTestData(:param_data,:v_cur1,:v_cur2)");
        }
    }
}
