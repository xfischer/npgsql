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


			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayVarchar(Name IN VARCHAR, Age IN INT, "+
				"     		Sal IN  INT, WhoAmI IN OUT VARCHAR,CheckOut OUT INT) return VARCHAR[]  "+ 
				"   IS "+
				"    Temp1 VARCHAR;"+
				"    Temp2 VARCHAR; "+
				"    Test_RefCursor REFCURSOR; "+
				"    Ret VARCHAR[];  "+
				"	    BEGIN "+
				"	      CheckOut:=100;  "+
				"	        OPEN Test_RefCursor FOR SELECT c2 FROM tblTest;  "+
				"		FETCH Test_RefCursor INTO Temp2;  "+
				"		 Temp1:=Temp2;  "+
				"		  LOOP  "+
				" 		     FETCH Test_RefCursor INTO Temp2;  "+
				"		     EXIT WHEN Test_RefCursor%NOTFOUND;  "+
				"		     Temp1:=Temp1 || ',' || Temp2;  "+
				"		  END LOOP;"+
				"		 Ret:='{' || Temp1 || '}';  "+
				"		return Ret;  "+
				"	      END;", con);
			Command.ExecuteNonQuery();


			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayNumeric(a numeric, b IN numeric, "+
			 "     		c IN numeric, d IN OUT numeric,e OUT numeric) return NUMERIC[]  " +
			 "   IS "+
			 "    Temp1 NUMERIC[];"+
			 "    Temp2 NUMERIC[]; "+
			 "    Test_RefCursor REFCURSOR; "+
			 "    Ret NUMERIC[];  "+
			 "	    BEGIN "+
			 "	      e:=100;  "+
			 "	        OPEN Test_RefCursor FOR SELECT f2 FROM tblTest;  "+
			 "	 	FETCH Test_RefCursor INTO Temp2;  "+
			 "		 Temp1:=Temp2;  "+
			 "		  LOOP  "+
			 " 		     FETCH Test_RefCursor INTO Temp2;  "+
			 "		     EXIT WHEN Test_RefCursor%NOTFOUND;  "+
			 "		     Temp1:= Temp2;  "+
			 "		  END LOOP;"+
			 "		 Ret:= Temp1 ;  "+
			 "		return Ret;  "+
			 "	      END;", con);
			Command.ExecuteNonQuery();

			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayInteger(a integer, b IN integer, "+
			 "     		c IN integer) return INTEGER[]  " +
			 "   IS "+
			 "    Temp1 INTEGER[];"+
			 "    Temp2 INTEGER[]; "+
			 "    Test_RefCursor REFCURSOR; "+
			 "    Ret INTEGER[];  "+
			 "	    BEGIN "+
			 "	        OPEN Test_RefCursor FOR SELECT f2 FROM tblTest2;  "+
			 "	 	FETCH Test_RefCursor INTO Temp2;  "+
			 "		 Temp1:=Temp2;  "+
			 "		  LOOP  "+
			 " 		     FETCH Test_RefCursor INTO Temp2;  "+
			 "		     EXIT WHEN Test_RefCursor%NOTFOUND;  "+
			 "		     Temp1:= Temp2;  "+
			 "		  END LOOP;"+
			 "		 Ret:= Temp1 ;  "+
			 "		return Ret;  "+
			 "	      END;", con);
			Command.ExecuteNonQuery();


			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayFloat(a float, b IN OUT float)  "+  
			 "  return FLOAT[] "+
			 "   IS "+
			 "    Temp1 FLOAT[];"+
			 "    Temp2 FLOAT[]; "+
			 "    Test_RefCursor REFCURSOR; "+
			 "    Ret FLOAT[];  "+
			 "	    BEGIN "+
			 "	        OPEN Test_RefCursor FOR SELECT f2 FROM tblTest3;  "+
			 "	 	FETCH Test_RefCursor INTO Temp2;  "+
			 "		 Temp1:=Temp2;  "+
			 "		  LOOP  "+
			 " 		     FETCH Test_RefCursor INTO Temp2;  "+
			 "		     EXIT WHEN Test_RefCursor%NOTFOUND;  "+
			 "		     Temp1:= Temp2;  "+
			 "		  END LOOP;"+
			 "		 Ret:= Temp1 ;  "+
			 "		return Ret;  "+
			 "	      END;", con);
			Command.ExecuteNonQuery();

			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayDoublePrecision(a double precision, b IN OUT double precision)  " + 
			 "  return double precision[] "+
			 "   IS "+
			 "    Temp1 double precision[];"+
			 "    Temp2 double precision[]; "+
			 "    Test_RefCursor REFCURSOR; "+
			 "    Ret double precision[];  "+
			 "	    BEGIN "+
			 "	        OPEN Test_RefCursor FOR SELECT f2 FROM tblTest4;  "+
			 "	 	FETCH Test_RefCursor INTO Temp2;  "+
			 "		 Temp1:=Temp2;  "+
			 "		  LOOP  "+
			 " 		     FETCH Test_RefCursor INTO Temp2;  "+
			 "		     EXIT WHEN Test_RefCursor%NOTFOUND;  "+
			 "		     Temp1:= Temp2;  "+
			 "		  END LOOP;"+
			 "		 Ret:= Temp1 ;  "+
			 "		return Ret;  "+
			 "	      END;", con);
			Command.ExecuteNonQuery();


			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayBigInt(a bigint, b IN OUT bigint)  " + 
			 "  return bigint[] "+
			 "   IS "+
			 "    Temp1 bigint[];"+
			 "    Temp2 bigint[]; "+
			 "    Test_RefCursor REFCURSOR; "+
			 "    Ret BigInt[];  "+
			 "	    BEGIN "+
			 "	        OPEN Test_RefCursor FOR SELECT f2 FROM tblTest5;  "+
			 "	 	FETCH Test_RefCursor INTO Temp2;  "+
			 "		 Temp1:=Temp2;  "+
			 "		  LOOP  "+
			 " 		     FETCH Test_RefCursor INTO Temp2;  "+
			 "		     EXIT WHEN Test_RefCursor%NOTFOUND;  "+
			 "		     Temp1:= Temp2;  "+
			 "		  END LOOP;"+
			 "		 Ret:= Temp1 ;  "+
			 "		return Ret;  "+
			 "	      END;", con);
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

			Command.CommandText="DROP procedure GETREFCURSORSIVPROC;";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP procedure GETSYSREFCURSORSIVPROC;";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP procedure GETREFCURSORSIIPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP procedure GETSYSREFCURSORSIIPROC";
			Command.ExecuteNonQuery();

			Command.CommandText="DROP Package REFCURSOR_PKG";
			Command.ExecuteNonQuery();

			Command.CommandText="drop FUNCTION FuncReturningArrayVarchar(VARCHAR,INT,INT,VARCHAR,INT);";
			Command.ExecuteNonQuery();

			Command.CommandText="drop FUNCTION FuncReturningArrayNumeric(numeric,numeric,numeric,numeric,numeric)";
			Command.ExecuteNonQuery();

			Command.CommandText="drop FUNCTION FuncReturningArrayInteger(integer,integer,integer)";
			Command.ExecuteNonQuery();

			Command.CommandText="drop FUNCTION FuncReturningArrayFloat(float,float)";
			Command.ExecuteNonQuery();

			Command.CommandText="drop FUNCTION FuncReturningArrayDoublePrecision(double precision,double precision)";
			Command.ExecuteNonQuery();

			Command.CommandText="drop FUNCTION FuncReturningArrayBigInt(bigint, bigint)";
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


		[Test]
		public void DefaultInBindAsReturn()
		{
		
			
			EDBCommand command = new EDBCommand("public.DEFAULTINRETURNFUNC(:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,55)); 

			
			command.Prepare();

			command.ExecuteReader();
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

			command.ExecuteReader();
		int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==33)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);

			
		
			
		}	
	

		
		[Test]
		public void PACKDefaultInBindAsReturn()
		{
		
			
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.DEFAULTINRETURNFUNC(:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,55)); 

			
			command.Prepare();

			command.ExecuteReader();
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

			command.ExecuteReader();
			int a= int.Parse(command.Parameters[0].Value.ToString());
			if(a==33)
				Assert.IsTrue(true);
			else
				Assert.IsTrue(false);

			
		
			
		}	
	

		[Test]
		public void RefcursorsIV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSORSIVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.AreEqual("",rst1);
			
			Console.WriteLine(rst1);
			rst.Close();
		
			tran.Commit();
			
		}	


		[Test]
		public void RefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			string rst =  command.Parameters[0].Value.ToString();
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.AreEqual("",rst);
			Assert.AreEqual("",rst1);
			
		
			tran.Commit();
			
		}	


		[Test]
		public void PACKRefcursorsIV()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSIVPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.AreEqual("",rst1);
			
			Console.WriteLine(rst1);
			rst.Close();
		
			tran.Commit();
			
		}
	
		[Test]
		public void PACKRefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.AreEqual("",rst1);
			
			Console.WriteLine(rst1);
			rst.Close();
		
			tran.Commit();
			
		}	


		[Test]
		public void SYSRefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("public.GETSYSREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
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
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			EDBDataReader rst = (EDBDataReader) command.Parameters[0].Value;
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.IsNotNull(rst);
			Assert.IsTrue(rst.Read());
			Assert.AreEqual("V1",rst.GetValue(0).ToString());
			Assert.IsTrue(rst.Read());
			
			if(2==int.Parse(rst.GetValue(1).ToString()))
				
				Assert.IsTrue(true);
			else
				Assert.IsFalse(false);

			Assert.AreEqual("",rst1);
			
			Console.WriteLine(rst1);
			rst.Close();
		
			tran.Commit();
			
		}
	
		[Test]
		public void PACKSYSRefcursorsII()
		{
			EDBTransaction tran=con.BeginTransaction();
			EDBCommand command = new EDBCommand("REFCURSOR_PKG.GETSYSREFCURSORSIIPROC(:param1,:param0)", con); 
			command.CommandType = CommandType.StoredProcedure; 
			command.Transaction=tran;
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.RefCursor,10,"param1",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.RefCursor,10,"param0",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,null)); 
			
			command.Prepare();

			command.ExecuteReader();
			string rst =  command.Parameters[0].Value.ToString();
			string rst1 = command.Parameters[1].Value.ToString();

			Assert.AreEqual("",rst);
			Assert.AreEqual("",rst1);
			
		
			tran.Commit();
			
		}	

		[Test]
		public void FuncReturningArrayVarchar()
		{
			
			
			

			EDBCommand command = new EDBCommand("CREATE TABLE tblTest (c1 VARCHAR, c2 INT); ", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest VALUES ('Ahmar',100);INSERT INTO tblTest VALUES ('Nauman',200);", con);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO tblTest VALUES ('Testing',300);INSERT INTO tblTest VALUES ('DOTNET',400);", con);
			command.ExecuteNonQuery();
					
			command.CommandText="public.FuncReturningArrayVarchar(:param0,:param1,:param2,:param3,:param4)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Varchar,10,"param",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Varchar,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,"VALUE")); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,23)); 
			command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,1000)); 
			command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar,10,"param3",ParameterDirection.InputOutput,false,2,2,System.Data.DataRowVersion.Current,"4")); 
			command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer,10,"param4",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,3)); 
			
			command.Prepare();
			EDBDataReader Reader=command.ExecuteReader();
			
			string rst =  command.Parameters[0].Value.ToString();
			
			
			Reader.Close();
			
			Assert.AreEqual("{100,200,300,400}",rst);	
			command=new EDBCommand("drop table tblTest;",con);
			
			
			command.ExecuteNonQuery();

			
		
			
		}



		[Test]
		public void FuncReturningArrayNumeric()
		{
			
			
			

			EDBCommand command = new EDBCommand("CREATE TABLE tblTest (f1 Numeric[10],f2 numeric[]);  ", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest VALUES ('{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}','{132.654,897.2563}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayNumeric(:param0,:param1,:param2,:param3,:param4)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Numeric,10,"param",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Numeric,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,110)); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric,10,"param1",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,200.25)); 
			command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric,10,"param2",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,1000)); 
			command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric,10,"param3",ParameterDirection.InputOutput,false,2,2,System.Data.DataRowVersion.Current,60)); 
			command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Numeric,10,"param4",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,400)); 
			
			command.Prepare();
			EDBDataReader Reader=command.ExecuteReader();
			
			string rst =  command.Parameters[0].Value.ToString();
			
			Console.WriteLine(rst);
			Reader.Close();
			
			Assert.AreEqual("{132.654,897.2563}",rst);	
			command=new EDBCommand("drop table tblTest;",con);
			
			
			command.ExecuteNonQuery();

			
		
			
		}


		[Test]
		public void FuncReturningArrayInteger()
		{
			
			
			

			EDBCommand command = new EDBCommand("CREATE TABLE tblTest2 (f1 integer[10],f2 integer[]);  ", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest2 VALUES ('{120,1234,2,3123,42342,5355,652,7,8,94}','{132,897}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayInteger(:param0,:param1,:param2)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Numeric,10,"param",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Integer,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,110)); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer,10,"param1",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,200)); 
			command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer,10,"param2",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,1000)); 
			
			command.Prepare();
			EDBDataReader Reader=command.ExecuteReader();
			
			string rst =  command.Parameters[0].Value.ToString();
			
			Console.WriteLine(rst);
			Reader.Close();
			
			Assert.AreEqual("{132,897}",rst);	
			command=new EDBCommand("drop table tblTest2;",con);
			
			
			command.ExecuteNonQuery();

			
		
			
		}


		[Test]
		public void FuncReturningArrayFloat()
		{
			
			
			

			EDBCommand command = new EDBCommand("CREATE TABLE tblTest3 (d1 float[10],f2 float[]);  ", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest3 VALUES ('{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}','{132.654,897.2563}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayFLOAT(:param0,:param1)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Float,10,"param",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Float,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,110.345)); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Float,10,"param1",ParameterDirection.InputOutput,false,2,2,System.Data.DataRowVersion.Current,200.123)); 
			
			command.Prepare();
			EDBDataReader Reader=command.ExecuteReader();
			
			string rst =  command.Parameters[0].Value.ToString();
			
			Console.WriteLine(rst);
			Reader.Close();
			
			Assert.AreEqual("{132.654,897.2563}",rst);	
			command=new EDBCommand("drop table tblTest3;",con);
			
			
			command.ExecuteNonQuery();

			
		
			
		}


		[Test]
		public void FuncReturningArrayDoublePrecision()
		{
			
			
			

			EDBCommand command = new EDBCommand("CREATE TABLE tblTest4 (d1 double precision[],f2 double precision[]);", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest4 VALUES ('{122.323423453,230.32131231322,123342.2323324}','{555.43534543233,344654.34534439785}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayDoublePrecision(:param0,:param1)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Double,10,"param",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Float,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,110.345)); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Float,10,"param1",ParameterDirection.InputOutput,false,2,2,System.Data.DataRowVersion.Current,200.123)); 
			
			command.Prepare();
			EDBDataReader Reader=command.ExecuteReader();
			
			string rst =  command.Parameters[0].Value.ToString();
			
			Console.WriteLine(rst);
			Reader.Close();
			
			Assert.AreEqual("{555.43534543233,344654.345344398}",rst);	
			command=new EDBCommand("drop table tblTest4;",con);
			
			
			command.ExecuteNonQuery();

			
		
			
		}


		[Test]
		public void FuncReturningArrayBigInt()
		{
			
			
			

			EDBCommand command = new EDBCommand("CREATE TABLE tblTest5 (d1 bigint[],f2 bigint[]);", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest5 VALUES ('{122323423453,23032131231322,1233422323324}','{55543534543233,34465434534439785}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayBigInt(:param0,:param1)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Bigint,10,"param",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,1)); 
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Bigint,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,110)); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Bigint,10,"param1",ParameterDirection.InputOutput,false,2,2,System.Data.DataRowVersion.Current,200)); 
			
			command.Prepare();
			EDBDataReader Reader=command.ExecuteReader();
			
			string rst =  command.Parameters[0].Value.ToString();
			
			Console.WriteLine(rst);
			Reader.Close();
			
			Assert.AreEqual("{55543534543233,34465434534439785}",rst);	
			command=new EDBCommand("drop table tblTest5;",con);
			
			
			command.ExecuteNonQuery();

			
		
			
		}





	}
}
