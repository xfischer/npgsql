using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    /// <summary>
    /// Summary description for EDBFunctionWithArray.
    /// </summary>
    [TestFixture]
	public class EDBFunctionWithArray : TestBase
	{
		EDBConnection? con = null;

        #region Setup / Tear Down
        [SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(con);
            EDBCommand Command=new EDBCommand("",con);
            
			Command = new EDBCommand("CREATE OR REPLACE FUNCTION FuncReturningArrayVarchar(Name IN VARCHAR, Age IN INT, "+
				"     		Sal IN  INT, WhoAmI IN OUT VARCHAR,CheckOut OUT INT) return VARCHAR[]  "+ 
				"   IS "+
				"    Temp1 VARCHAR;"+
				"    Temp2 VARCHAR; "+
				"    Test_RefCursor REFCURSOR; "+
				"    Ret VARCHAR[];  "+
				"	    BEGIN "+
				"	      CheckOut:=100;  "+
				"	        OPEN Test_RefCursor FOR SELECT c2 FROM tblTest1;  "+
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
		}

		[TearDown] 
		public void Dispose()
		{
			
			EDBCommand Command=new EDBCommand("",con);

            Command.CommandText = "drop FUNCTION FuncReturningArrayVarchar( IN VARCHAR,  IN INT, IN  INT,  IN OUT VARCHAR, OUT INT) ;";
			Command.ExecuteNonQuery();

            Command.CommandText = "drop FUNCTION FuncReturningArrayNumeric( numeric,  IN numeric,  IN numeric,  IN OUT numeric, OUT numeric)";
			Command.ExecuteNonQuery();

            Command.CommandText = "drop FUNCTION FuncReturningArrayInteger(IN integer, IN integer, IN integer)";
			Command.ExecuteNonQuery();

            Command.CommandText = "drop FUNCTION FuncReturningArrayFloat( float,  IN OUT float) ";
			Command.ExecuteNonQuery();

            Command.CommandText = "drop FUNCTION FuncReturningArrayDoublePrecision( double precision,  IN OUT double precision)";
			Command.ExecuteNonQuery();

            Command.CommandText = "drop FUNCTION FuncReturningArrayBigInt( bigint,  IN OUT bigint)";
			Command.ExecuteNonQuery();

			TestUtil.closeDB(con);
		}
        #endregion

        #region Function Return Array

        [Test, Ignore("Fix Array test")]
		public void FuncReturningArrayVarchar()
		{

            string[] a = { "100", "200", "300", "400" };
			EDBCommand command = new EDBCommand("CREATE TABLE tblTest1 (c1 VARCHAR, c2 INT); ", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest1 VALUES ('Ahmar',100);INSERT INTO tblTest1 VALUES ('Nauman',200);", con);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO tblTest1 VALUES ('Testing',300);INSERT INTO tblTest1 VALUES ('DOTNET',400);", con);
			command.ExecuteNonQuery();

            command.CommandText = "public.FuncReturningArrayVarchar(:param0,:param1,:param2,:param3,:param4)";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Varchar, 10, "param0", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "VALUE"));
            command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 23));
            command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1000));
            command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "4"));
            command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 3));
            command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Array | EDBTypes.EDBDbType.Text, 10, "param", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

            command.Prepare();
            command.ExecuteNonQuery();

            try
            {
                Object rst = command.Parameters[5].Value;

                Assert.AreEqual(a, (string[])rst);
            }
            finally
            {
                command = new EDBCommand("drop table tblTest1;", con);

                command.ExecuteNonQuery();
            }
		}

		[Test, Ignore("Fix Array test")]
		public void FuncReturningArrayNumeric()
		{

            decimal[] a = { 132.654M, 897.2563M };
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
            command.ExecuteNonQuery();

            Object rst = command.Parameters[0].Value;
			
			Console.WriteLine(rst);
			
			Assert.AreEqual(a,(decimal[])rst);	
			command=new EDBCommand("drop table tblTest;",con);

			command.ExecuteNonQuery();

		}

		[Test, Ignore("Fix Array test")]
		public void FuncReturningArrayInteger()
		{
            int[] a = {132,897};
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
            command.ExecuteNonQuery();

            Object rst = command.Parameters[0].Value;
			
			Console.WriteLine(rst);
			
			Assert.AreEqual(a , (int[])rst);	
			command=new EDBCommand("drop table tblTest2;",con);

			command.ExecuteNonQuery();

		}

		[Test, Ignore("Fix Array test")]
		public void FuncReturningArrayFloat()
		{

            double[] a = { 132.654, 897.2563 };
			EDBCommand command = new EDBCommand("CREATE TABLE tblTest3 (d1 float[10],f2 float[]);  ", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest3 VALUES ('{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}','{132.654,897.2563}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayFLOAT(:param0,:param1)";
			command.CommandType=CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("param0", /*EDBTypes.EDBDbType.Array | */EDBTypes.EDBDbType.Double, 10, "param0", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 110.345));
            command.Parameters.Add(new EDBParameter("param1", /*EDBTypes.EDBDbType.Array | */EDBTypes.EDBDbType.Double, 10, "param1", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 200.123));
            command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Array | EDBTypes.EDBDbType.Double, 10, "param", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1)); 
			
			command.Prepare();
            command.ExecuteNonQuery();

            Object rst = command.Parameters[2].Value;
			
			Console.WriteLine(rst);
			
			Assert.AreEqual(a,(double[])rst);	
			command=new EDBCommand("drop table tblTest3;",con);

			command.ExecuteNonQuery();

		}

		[Test, Ignore("Fix Array test")]
		public void FuncReturningArrayDoublePrecision()
		{

            double[] a = { 555.43534543233, 344654.345344398 };
			EDBCommand command = new EDBCommand("CREATE TABLE tblTest4 (d1 double precision[],f2 double precision[]);", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest4 VALUES ('{122.323423453,230.32131231322,123342.2323324}','{555.43534543233,344654.34534439785}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayDoublePrecision(:param0,:param1)";
			command.CommandType=CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Double, 10, "param0", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 110.345));
            command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Double, 10, "param1", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 200.123));
            command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Double, 10, "param", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1)); 
			
			command.Prepare();
            command.ExecuteNonQuery();

            Object rst = command.Parameters[2].Value;
			
			Console.WriteLine(rst);
			
			Assert.AreEqual(a,(double[])rst);	
			command=new EDBCommand("drop table tblTest4;",con);
			
			command.ExecuteNonQuery();
		}

		[Test, Ignore("Fix Array test")]
		public void FuncReturningArrayBigInt()
		{

            long[] a = { 55543534543233, 34465434534439785 };
			EDBCommand command = new EDBCommand("CREATE TABLE tblTest5 (d1 bigint[],f2 bigint[]);", con);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO tblTest5 VALUES ('{122323423453,23032131231322,1233422323324}','{55543534543233,34465434534439785}');", con);
			command.ExecuteNonQuery();
			
			command.CommandText="public.FuncReturningArrayBigInt(:param0,:param1)";
			command.CommandType=CommandType.StoredProcedure;
			
			command.Parameters.Add(new EDBParameter("param0", EDBTypes.EDBDbType.Bigint,10,"param0",ParameterDirection.Input,false,2,2,System.Data.DataRowVersion.Current,110)); 
			command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Bigint,10,"param1",ParameterDirection.InputOutput,false,2,2,System.Data.DataRowVersion.Current,200));
            command.Parameters.Add(new EDBParameter("param", EDBTypes.EDBDbType.Bigint, 10, "param", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1)); 
			
			command.Prepare();
            EDBDataReader Reader = command.ExecuteReader();

            Object rst = command.Parameters[2].Value;

            Console.WriteLine(rst);
			Reader.Close();
			
			Assert.AreEqual(a,(long)rst);	

			command=new EDBCommand("drop table tblTest5;",con);
			command.ExecuteNonQuery();

		}

        #endregion

    }
}
