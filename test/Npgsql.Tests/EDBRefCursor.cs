using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using EDBTypes;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8600
#pragma warning disable CS8602
    /// <summary>
    /// Summary description for RefCursor.
    /// </summary>

    [TestFixture]
        public class EDBRefCursor : TestBase
		{
			EDBConnection? con = null;
            
            #region Setup / Tear Down
			[SetUp]
			public void Init()
			{
				con = OpenConnection();
				
				EDBCommand com = new EDBCommand("",con);
				com.CommandType = CommandType.Text;

                string RefCurProc = "CREATE OR REPLACE Procedure RefCurProc(Test_RefCursor OUT SYS_REFCURSOR)" +
                " IS BEGIN" +
                    " OPEN Test_RefCursor FOR SELECT * FROM TestCursorTable;" +
                " END;\n";
				com.CommandText = RefCurProc;
				com.ExecuteNonQuery();

				string RefCurPackageProc = "CREATE OR REPLACE PACKAGE refcurpackproc" +
                " IS" +
                    " Procedure RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR);" +
                " END refcurpackproc;\n";
				com.CommandText = RefCurPackageProc;
				com.ExecuteNonQuery();

                string RefCurPackageProcBody = "CREATE OR REPLACE PACKAGE BODY refcurpackproc" +
                " IS" +
                    " Procedure RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR)" +
                    " IS BEGIN" +
                        " OPEN Test_RefCursor FOR SELECT * FROM PackFuncRefCursorOutParameter;" +
                    " END;" +
                " END refcurpackproc;\n";
				com.CommandText = RefCurPackageProcBody;
				com.ExecuteNonQuery();
				
				string RefCurPackagefunc = "CREATE OR REPLACE PACKAGE refcurpackfunc" +
                " IS" +
                    " FUNCTION RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR) return NUMERIC;" +
                " END refcurpackfunc;\n";
				com.CommandText = RefCurPackagefunc;
				com.ExecuteNonQuery();

				string RefCurPackagefuncBody = "CREATE OR REPLACE PACKAGE BODY refcurpackfunc" +
                " IS" +
                    " Function RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR) return NUMERIC" +
                    " IS BEGIN" +
                        " OPEN Test_RefCursor FOR SELECT * FROM TestCursorTable;" +
                        " return 10;" +
                    " END;" +
                " END refcurpackfunc;\n";
				com.CommandText = RefCurPackagefuncBody;
				com.ExecuteNonQuery();

                string RefCurfunc = "CREATE OR REPLACE Function RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR) return NUMERIC" +
                " IS BEGIN" +
                    " OPEN Test_RefCursor FOR SELECT * FROM FuncRefCursorOutParameter;" +
                    " return 10;" +
                " END;\n";
				com.CommandText = RefCurfunc;
				com.ExecuteNonQuery();

                string RefCursorsReturnFunc = "CREATE OR REPLACE Function RefCursorsReturnFunc return SYS_REFCURSOR" +
                " IS" +
                    " Test_RefCursor SYS_REFCURSOR;" +
                    " BEGIN OPEN Test_RefCursor FOR SELECT * FROM TbRefCursor;" +
                    " return TestCursorTable;" +
                " END;\n";
				com.CommandText = RefCursorsReturnFunc;
				com.ExecuteNonQuery();

				string PackFuncRefCursorReturn = "CREATE OR REPLACE PACKAGE PackFuncRefCursorReturn" +
                " IS" +
                    " Function RefCursorsReturnFunc return SYS_REFCURSOR;" +
                " END PackFuncRefCursorReturn;\n";
				com.CommandText = PackFuncRefCursorReturn;
				com.ExecuteNonQuery();

				string PackFuncRefCursorReturnBody = "CREATE OR REPLACE PACKAGE BODY PackFuncRefCursorReturn" +
                " IS" +
                    " Function RefCursorsReturnFunc return SYS_REFCURSOR" +
                    " IS" +
                        " Test_RefCursor SYS_REFCURSOR;" +
                        " BEGIN" +
                        " OPEN Test_RefCursor FOR SELECT * FROM TestCursorTable;" +
                        " return Test_RefCursor;" +
                    " END;" +
                " END PackFuncRefCursorReturn;\n";
				com.CommandText = PackFuncRefCursorReturnBody;
				com.ExecuteNonQuery();

				string strRefTwoArg = "CREATE OR REPLACE PROCEDURE public.cursortest2 (c_1 OUT    refcursor,c_2 OUT refcursor )" +
                " IS BEGIN" +
                    " open  c_1 for select * from emp order by empno;" +
                    " open  c_2 for select * from emp order by empno;" +
                " END;"	;
				com.CommandText = strRefTwoArg;
				com.ExecuteNonQuery();
				
				string strRefThreeArg = "CREATE OR REPLACE PROCEDURE public.refcur_callee2 (c_1  OUT numeric, c_2 IN OUT refcursor,c_3 IN OUT refcursor )" +
                " IS BEGIN" +
                    " c_1 :=100;" +
                    " open  c_2 for select * from emp order by empno;" +
                    " open  c_3 for select ename from emp order by empno;" +
                " END;";
				com.CommandText = strRefThreeArg;
				com.ExecuteNonQuery();
				
				string strRef4ParamWithJoin = "CREATE OR REPLACE PROCEDURE public.refcur_callee_4param_with_Join" +
                " (c_1 OUT numeric,c_2 IN OUT refcursor,c_3 IN OUT    refcursor, c_4 IN OUT refcursor)" +
                " IS BEGIN" +
                    " c_1 :=100;" +
                    " open  c_2 for select * from emp order by empno;" +
                    " open  c_3 for select ename from emp order by empno;" +
                    " open  c_4 for select  * from emp,dept where emp.deptno = dept.deptno and dept.deptno=30 order by empno;" +
                " END;";
				com.CommandText =strRef4ParamWithJoin;
				com.ExecuteNonQuery();

				string strRef5ParamWithJoin ="CREATE OR REPLACE PROCEDURE public.refcur_callee_5param_with_Join" +
                " (c_1 OUT numeric,c_2 IN OUT refcursor,c_3 IN OUT    refcursor,c_4 IN OUT refcursor,c_5  OUT varchar)" +
                " IS BEGIN" +
                    " c_1 :=100;" +
                    " open  c_2 for select * from emp order by empno;" +
                    " open  c_3 for select ename from emp order by empno;" +
                    " open c_4 for select  * from emp,dept where emp.deptno = dept.deptno and dept.deptno=30;" +
                    "c_5:='EnterpriseDB';" +
                " END;";
				com.CommandText=strRef5ParamWithJoin;
				com.ExecuteNonQuery();

				string strRef6ParamWithJoin ="CREATE OR REPLACE PROCEDURE public.refcur_callee_6param_with_Join" +
                " (c_1 OUT numeric,c_2 IN OUT refcursor,c_3 IN OUT refcursor,c_4 IN OUT refcursor,c_5  OUT varchar,c_6 IN OUT refcursor )" +
                " IS BEGIN" +
                    " c_1 :=100;" +
                    " open  c_2 for select * from emp order by empno;" +
                    " open  c_3 for select ename from emp order by empno;" +
                    " open c_4 for select  * from emp,dept where emp.deptno = dept.deptno and dept.deptno=30;" +
                    " c_5:='EnterpriseDB';" +
                    " open c_6 for select * from dept order by deptno;" +
                " END;";
				com.CommandText = strRef6ParamWithJoin;
				com.ExecuteNonQuery();

				string strRef7ParamWithJoin= "CREATE OR REPLACE PROCEDURE public.refcur_callee_7param_with_Join " +
                " (c_1 OUT numeric,c_2 IN OUT refcursor,c_3 IN OUT refcursor,c_4 IN OUT refcursor,c_5  OUT varchar,c_6 IN OUT refcursor,c_7 OUT NUMERIC  )" +
                " IS BEGIN" +
                    " c_1 :=100;" +
                    " open  c_2 for select * from emp order by empno;" +
                    " open  c_3 for select ename from emp order by empno;" +
                    " open c_4 for select  * from emp, dept where emp.deptno = dept.deptno and dept.deptno=30;" +
                    " c_5:='EnterpriseDB';" +
                    " open c_6 for select * from dept order by deptno;" +
                    " c_7 := 106;" +
                " END;";
				com.CommandText= strRef7ParamWithJoin;
				com.ExecuteNonQuery();

				string strRef8ParamWithJoin="CREATE OR REPLACE PROCEDURE public.refcur_callee_8param_with_Join " +
                    " (c_1 OUT numeric,c_2 IN OUT refcursor,c_3 IN OUT refcursor,c_4 IN OUT refcursor,c_5  OUT varchar,c_6 IN OUT refcursor,c_7 OUT NUMERIC,c_8 OUT money  )" +
                    " IS BEGIN " +
                        " c_1 :=100;" +
                        " open  c_2 for select * from emp order by empno;" +
                        " open  c_3 for select ename from emp order by empno;" +
                        " open  c_4 for select  * from emp, dept where emp.deptno = dept.deptno and dept.deptno=30;" +
                        " c_5:='EnterpriseDB';" +
                        " open c_6 for select * from dept;" +
                        " c_7:= 106;" +
                        " c_8:=99.90;" +
                    " END;";
				com.CommandText= strRef8ParamWithJoin;
				com.ExecuteNonQuery();

				string RefCurProcOutBool ="CREATE OR REPLACE PROCEDURE RefCurProcOutBool(c_1 OUT refcursor,c_2 out Boolean)" +
                " IS   BEGIN" +
                    " open  c_1 for select * from emp order by empno;" +
                    " c_2:= true;" +
                "  END;";
				com.CommandText = RefCurProcOutBool;
				com.ExecuteNonQuery();

				string RefCurProcOutBigInt ="CREATE OR REPLACE PROCEDURE RefCurProcOutBigInt(c_1 OUT refcursor,c_2 out BigInt)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 200;" +
                "  END;";
				com.CommandText = RefCurProcOutBigInt;
				com.ExecuteNonQuery();

				string RefCurProcOutChar ="CREATE OR REPLACE PROCEDURE RefCurProcOutChar(c_1 OUT refcursor,c_2 out Char)" +
                " IS   BEGIN" +
                    " open  c_1 for select * from emp order by empno;" +
                    " c_2:= 'Hashim';" +
                " END;";
				com.CommandText = RefCurProcOutChar;
				com.ExecuteNonQuery();

				string RefCurProcOutDoublePrecision ="CREATE OR REPLACE PROCEDURE RefCurProcOutDoublePrecision(c_1 OUT refcursor,c_2 out Double Precision)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 2.9863;" +
                " END;";
				com.CommandText = RefCurProcOutDoublePrecision;
				com.ExecuteNonQuery();

				string RefCurProcOutInteger ="CREATE OR REPLACE PROCEDURE RefCurProcOutInteger(c_1 OUT refcursor,c_2 out Integer)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 263;" +
                "  END;";
				com.CommandText = RefCurProcOutInteger;
				com.ExecuteNonQuery();

				string RefCurProcOutNumeric ="CREATE OR REPLACE PROCEDURE RefCurProcOutNumeric(c_1 OUT refcursor,c_2 out Numeric)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 263000;" +
                "  END;";
				com.CommandText = RefCurProcOutNumeric;
				com.ExecuteNonQuery();

				string RefCurProcOutNumeric2 ="CREATE OR REPLACE PROCEDURE RefCurProcOutNumeric2(c_1 OUT refcursor,c_2 out Numeric(10,2))" +
                " IS   BEGIN" +
                    " open  c_1 for select * from emp order by empno;" +
                    " c_2:= 263000.24598;" +
                "  END;";
				com.CommandText = RefCurProcOutNumeric2;
				com.ExecuteNonQuery();

				string RefCurProcOutReal ="CREATE OR REPLACE PROCEDURE RefCurProcOutReal(c_1 OUT refcursor,c_2 out Real)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 263001.24598;" +
                "  END;";
				com.CommandText = RefCurProcOutReal;
				com.ExecuteNonQuery();
				
				string RefCurProcOutSmallint ="CREATE OR REPLACE PROCEDURE RefCurProcOutSmallint(c_1 OUT refcursor,c_2 out Smallint)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 26301;" +
                "  END;";
				com.CommandText = RefCurProcOutSmallint;
				com.ExecuteNonQuery();

				string RefCurProcOutText ="CREATE OR REPLACE PROCEDURE RefCurProcOutText(c_1 OUT refcursor,c_2 out Text)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 'Hashim';" +
                "  END;";
				com.CommandText = RefCurProcOutText;
				com.ExecuteNonQuery();

				string RefCurProcOutVarchar ="CREATE OR REPLACE PROCEDURE RefCurProcOutVarchar(c_1 OUT refcursor,c_2 out Varchar)" +
                " IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno;" +
                    " c_2:= 'Hashim';" +
                "  END;";
				com.CommandText = RefCurProcOutVarchar;
				com.ExecuteNonQuery();

				string RefCursorPackage = "CREATE OR REPLACE PACKAGE RefCursorPackage" +
                " IS " +
                    "PROCEDURE RefCurProcOutBool(c_1 OUT refcursor,c_2 out Boolean);" +
                    " PROCEDURE RefCurProcOutBigInt(c_1 OUT refcursor,c_2 out BigInt);" +
                    " PROCEDURE RefCurProcOutChar(c_1 OUT refcursor,c_2 out Char);" +
                    " PROCEDURE RefCurProcOutDoublePrecision(c_1 OUT refcursor,c_2 out Double Precision);" +
                    " PROCEDURE RefCurProcOutInteger(c_1 OUT refcursor,c_2 out Integer);" +
                    " PROCEDURE RefCurProcOutNumeric(c_1 OUT refcursor,c_2 out Numeric);" +
                    " PROCEDURE RefCurProcOutNumeric2(c_1 OUT refcursor,c_2 out Numeric(10,2));" +
                    " PROCEDURE RefCurProcOutReal(c_1 OUT refcursor,c_2 out Real);" +
                    " PROCEDURE RefCurProcOutSmallint(c_1 OUT refcursor,c_2 out Smallint);" +
                    " PROCEDURE RefCurProcOutText(c_1 OUT refcursor,c_2 out Text);" +
                    " PROCEDURE RefCurProcOutVarchar(c_1 OUT refcursor,c_2 out Varchar);" +
                " END RefCursorPackage;\n";
				com.CommandText = RefCursorPackage;
				com.ExecuteNonQuery();

				string RefCursorPackageBody = "CREATE OR REPLACE PACKAGE BODY RefCursorPackage" +
                " IS" +
                    " PROCEDURE RefCurProcOutBool(c_1 OUT refcursor,c_2 out Boolean) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= true;  END;" +
                    " PROCEDURE RefCurProcOutBigInt(c_1 OUT refcursor,c_2 out BigInt) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 200;  END;" +
                    " PROCEDURE RefCurProcOutChar(c_1 OUT refcursor,c_2 out Char) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 'Hashim';  END;" +
                    " PROCEDURE RefCurProcOutDoublePrecision(c_1 OUT refcursor,c_2 out Double Precision) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 2.9863;  END;" +
                    " PROCEDURE RefCurProcOutInteger(c_1 OUT refcursor,c_2 out Integer) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 263;  END;" +
                    " PROCEDURE RefCurProcOutNumeric(c_1 OUT refcursor,c_2 out Numeric) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 263000; END;" +
                    " PROCEDURE RefCurProcOutNumeric2(c_1 OUT refcursor,c_2 out Numeric(10,2)) IS   BEGIN" +
                    " open  c_1 for select * from emp order by empno; c_2:= 263000.24598;  END;" +
                    " PROCEDURE RefCurProcOutReal(c_1 OUT refcursor,c_2 out Real) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 263001.24598;  END;" +
                    " PROCEDURE RefCurProcOutSmallint(c_1 OUT refcursor,c_2 out Smallint) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 26301;  END;" +
                    " PROCEDURE RefCurProcOutText(c_1 OUT refcursor,c_2 out Text) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 'Hashim';  END;" +
                    " PROCEDURE RefCurProcOutVarchar(c_1 OUT refcursor,c_2 out Varchar) IS   BEGIN" +
                    "  open  c_1 for select * from emp order by empno; c_2:= 'Hashim';  END;" +
                " END RefCursorPackage;\n";
				com.CommandText = RefCursorPackageBody;
				com.ExecuteNonQuery();
			}
			
			[TearDown] 
			public void Dispose()
			{
                // On Fetch Cursor command in test, transaction might not close on server side. ( In case of any assert in test before Trasaction commit )
                // Following extra Close() open sequence will make sure pending transactions are rolled back.
				if(con.State != ConnectionState.Closed)
					con.Close();
                con.Open();

            EDBCommand com = new EDBCommand("", con);
            com.CommandType = CommandType.Text;

            com.CommandText = "DROP PROCEDURE cursortest2";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE refcur_callee2";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE refcur_callee_4param_with_Join";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE refcur_callee_5param_with_Join ";

            com.CommandText = "DROP PROCEDURE refcur_callee_6param_with_Join";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE refcur_callee_7param_with_Join";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE refcur_callee_8param_with_Join";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProc;";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PACKAGE refcurpackproc;";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PACKAGE RefCursorPackage;";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutBool";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutBigint";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutDoublePrecision";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutNumeric";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutNumeric2";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutSmallInt";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE RefCurProcOutInteger";
            com.ExecuteNonQuery();

            if (con.State != ConnectionState.Closed)
					con.Close();
			}

            #endregion

            [Test]
			public void testonecursor()
			{			
				try
				{	
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					string strSqlEmptyArg = "CREATE OR REPLACE PROCEDURE public.cursortest1(c_1  OUT  refcursor) IS BEGIN open  c_1 for select * from emp order by empno; END;\n";
					com.CommandText = strSqlEmptyArg;
					com.ExecuteNonQuery();

					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("cursortest1", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					//REFCUSOR
					command.Parameters.Add(new EDBParameter("c_1", EDBTypes.EDBDbType.Refcursor,10, "c_1", ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Prepare();
                    command.ExecuteNonQuery();
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
                    rst.Read();
					Assert.AreEqual("7369",Convert.ToString(rst[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(rst[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(rst[2].ToString()));
                    Assert.AreEqual("7902", Convert.ToString(rst[3].ToString()));
					Assert.AreEqual("800.00", Convert.ToString(rst[5].ToString()));

                    rst.Close();
					tran.Commit();
                
				}
				catch(EDBException ex)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}
				
			}				
			
			[Test]
			public void refCuror2Param()
			{
				try
				{	
					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("cursortest2(:cur1,:cur2)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					//REFCUSOR CommandBehavior.SequentialAccess
					command.Parameters.Add(new EDBParameter("cur1",EDBTypes.EDBDbType.Refcursor,10,"cur1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("cur2",EDBTypes.EDBDbType.Refcursor,10,"cur2",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Prepare();
                    command.ExecuteNonQuery();
                    string cursorName1 = command.Parameters[0].Value.ToString();
                    string cursorName2 = command.Parameters[1].Value.ToString();

                    command.CommandText = "FETCH ALL IN \""+cursorName1+"\"";
                    command.CommandType = CommandType.Text;
					EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
					rst.Read();
                
                    Assert.AreEqual("7369", Convert.ToString(rst[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(rst.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(rst.GetString(2)));
					Assert.AreEqual("7902",Convert.ToString(rst[3].ToString()));
					Assert.AreEqual("800.00", Convert.ToString(rst[5].ToString()));
                    rst.Close();

                    command.CommandText = "FETCH ALL IN \""+cursorName2+"\"";
                    command.CommandType = CommandType.Text;
					rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
					rst.Read();
					rst.Read();
					rst.Read();
                    Assert.AreEqual("7521", Convert.ToString(rst[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(rst.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(rst.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(rst[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(rst[5].ToString()));
				
					rst.Close();
					tran.Commit();
				}
				catch(EDBException ex	)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}
				
			}
			[Test]	
			public void RefCur3Params()
			{			
				
				try
				{
					EDBTransaction tran = con.BeginTransaction();
				
					EDBCommand command = new EDBCommand("refcur_callee2(:b,:a,:c)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					
					command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric,10,"b",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,10));
					command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Refcursor,10,"a",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Refcursor,10,"c",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					
					command.Prepare();
					command.Parameters[0].Value = 7369; 
                    command.ExecuteNonQuery();
					Assert.AreEqual("100",Convert.ToString(command.Parameters[0].Value.ToString()));

                    string cursorName1 = command.Parameters[1].Value.ToString();
                    string cursorName2 = command.Parameters[2].Value.ToString();

                    command.CommandText = "FETCH ALL IN \""+cursorName1+"\"";
                    command.CommandType = CommandType.Text;
					EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					reader.Read();
					reader.Read();

                    Assert.AreEqual("7521", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
                    reader.Close();
                
                    command.CommandText = "FETCH ALL IN \""+cursorName2+"\"";
                    command.CommandType = CommandType.Text;
					EDBDataReader reader2 = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader2.Read();
					reader2.Read();
					Assert.AreEqual("ALLEN", Convert.ToString(reader2.GetString(0)));
					reader2.Close();
					tran.Commit();
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}
			}

			[Test]
			public void refcur_callee_4param_with_Join()
			{			
				try
				{					
					EDBTransaction tran = con.BeginTransaction();
				
					EDBCommand command = new EDBCommand("refcur_callee_4param_with_Join(:b,:a,:c,:d)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					
					command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric,10,"b",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,10));
					command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Refcursor,10,"a",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Refcursor,10,"c",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("d",EDBTypes.EDBDbType.Refcursor,10,"d",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));

					command.Prepare();
					command.Parameters[0].Value = 7369;
                    command.ExecuteNonQuery();
					
					Assert.AreEqual("100",Convert.ToString(command.Parameters[0].Value.ToString()));
                    string cursorName1 = command.Parameters[1].Value.ToString();
                    string cursorName2 = command.Parameters[2].Value.ToString();
                    string cursorName3 = command.Parameters[3].Value.ToString();
                
                    command.CommandText = "FETCH ALL IN \""+cursorName1+"\"";
                    command.CommandType = CommandType.Text;
					EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					reader.Read();
					reader.Read();

                    Assert.AreEqual("7521", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
                    reader.Close();

                    command.CommandText = "FETCH ALL IN \""+cursorName2+"\"";
                    command.CommandType = CommandType.Text;
					reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					
					Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));
					reader.Close();

                    command.CommandText = "FETCH ALL IN \""+cursorName3+"\"";
                    command.CommandType = CommandType.Text;
					reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();

					Assert.AreEqual("7521", Convert.ToString(reader.GetString(0)));
					Assert.AreEqual("WARD", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250", Convert.ToString(reader.GetString(5)));
					reader.Close();

					tran.Commit();
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}				
			}

			[Test]
			public void refcur_callee_5param_with_Join()
			{
			
				try
				{
					EDBTransaction tran = con.BeginTransaction();
				
					EDBCommand command = new EDBCommand("refcur_callee_5param_with_Join(:b,:a,:c,:d,:e)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					
					command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric,10,"b",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,10));
					command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Refcursor,10,"a",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Refcursor,10,"c",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("d",EDBTypes.EDBDbType.Refcursor,10,"d",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("e",EDBTypes.EDBDbType.Varchar,10,"e",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Prepare();
					command.Parameters[0].Value = 7369;
                    command.ExecuteNonQuery();
					
					Assert.AreEqual("100",Convert.ToString(command.Parameters[0].Value.ToString()));
					Assert.AreEqual("EnterpriseDB",command.Parameters[4].Value.ToString());

                    string cursorName1 = command.Parameters[1].Value.ToString();
                    string cursorName2 = command.Parameters[2].Value.ToString();
                    string cursorName3 = command.Parameters[3].Value.ToString();
                
                    command.CommandText = "FETCH ALL IN \""+cursorName1+"\"";
                    command.CommandType = CommandType.Text;
					EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("7521", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
                    reader.Close();
                
                    command.CommandText = "FETCH ALL IN \""+cursorName2+"\"";
                    command.CommandType = CommandType.Text;
					reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					reader.Read();
					
					Assert.AreEqual("ALLEN", Convert.ToString(reader.GetString(0)));
                    reader.Close();
                
                    command.CommandText = "FETCH ALL IN \""+cursorName3+"\"";
                    command.CommandType = CommandType.Text;
					reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					reader.Read();

					Assert.AreEqual("7521", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
					reader.Close();
					tran.Commit();
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}	
			}

			[Test]
			public void refcur_callee_6param_with_Join()
			{
			
				try
				{
					EDBTransaction tran = con.BeginTransaction();
				
					EDBCommand command = new EDBCommand("refcur_callee_6param_with_Join(:b,:a,:c,:d,:e,:f)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					
					command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric,10,"b",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,10));
					command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Refcursor,10,"a",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Refcursor,10,"c",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("d",EDBTypes.EDBDbType.Refcursor,10,"d",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("e",EDBTypes.EDBDbType.Varchar,10,"e",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("f",EDBTypes.EDBDbType.Refcursor,10,"f",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));

                    command.Prepare();
					command.Parameters[0].Value = 7369;

                    command.ExecuteNonQuery();

                    string cursorName1 = command.Parameters[1].Value.ToString();
                    string cursorName2 = command.Parameters[2].Value.ToString();
                    string cursorName3 = command.Parameters[3].Value.ToString();
                    string cursorName4 = command.Parameters[5].Value.ToString();

					Assert.AreEqual("100",Convert.ToString(command.Parameters[0].Value.ToString()));
					Assert.AreEqual("EnterpriseDB",command.Parameters[4].Value.ToString());

                    command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();

					Assert.AreEqual("7499", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1600.00", Convert.ToString(reader[5].ToString()));
                
                    reader.Close();

                    command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					
					Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));
					
                    reader.Close();
					
                    command.CommandText = "FETCH ALL IN \"" + cursorName3 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("7654", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("MARTIN", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
                    reader.Close();
					
                    command.CommandText = "FETCH ALL IN \"" + cursorName4 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("30", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("SALES", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("CHICAGO", Convert.ToString(reader.GetString(2)));
					reader.Close();

				    tran.Commit();
			
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}
			}

            [Test]
			public void refcur_callee_7param_with_Join()
			{
				try
				{
					EDBTransaction tran = con.BeginTransaction();
				
					EDBCommand command = new EDBCommand("refcur_callee_7param_with_Join(:b,:a,:c,:d,:e,:f,:g)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					
					command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric,10,"b",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Refcursor,10,"a",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Refcursor,10,"c",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("d",EDBTypes.EDBDbType.Refcursor,10,"d",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("e",EDBTypes.EDBDbType.Varchar,10,"e",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("f",EDBTypes.EDBDbType.Refcursor,10,"f",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("g",EDBTypes.EDBDbType.Numeric,10,"g",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Prepare();
					command.Parameters[0].Value = 7369;
                
                    command.ExecuteNonQuery();

                    string cursorName1 = command.Parameters[1].Value.ToString();
                    string cursorName2 = command.Parameters[2].Value.ToString();
                    string cursorName3 = command.Parameters[3].Value.ToString();
                    string cursorName4 = command.Parameters[5].Value.ToString();

					Assert.AreEqual("100",Convert.ToString(command.Parameters[0].Value.ToString()));
					Assert.AreEqual("EnterpriseDB",command.Parameters[4].Value.ToString());
					Assert.AreEqual("106",command.Parameters[6].Value.ToString());

                    command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
				
					reader.Read();
					reader.Read();

					Assert.AreEqual("7499", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1600.00", Convert.ToString(reader[5].ToString()));
					reader.Close();
                
                    command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
					reader.Read();
					
					Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));
					reader.Close();
		
                    command.CommandText = "FETCH ALL IN \"" + cursorName3 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("7654", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("MARTIN", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
					reader.Close();
					
                    command.CommandText = "FETCH ALL IN \"" + cursorName4 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("30", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("SALES", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("CHICAGO", Convert.ToString(reader.GetString(2)));
					
					reader.Close();
					tran.Commit();
			
			
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message.ToString() + "\n" + ex.StackTrace);
				}
			
			}

            [Test]
			public void refcur_callee_8param_with_Join()
			{

            try
				{
					EDBTransaction tran = con.BeginTransaction();
				
					EDBCommand command = new EDBCommand("refcur_callee_8param_with_Join(:b,:a,:c,:d,:e,:f,:g,:h)", con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					
					command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric,10,"b",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Refcursor,10,"a",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Refcursor,10,"c",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("d",EDBTypes.EDBDbType.Refcursor,10,"d",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("e",EDBTypes.EDBDbType.Varchar,10,"e",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("f",EDBTypes.EDBDbType.Refcursor,10,"f",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("g",EDBTypes.EDBDbType.Numeric,10,"g",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("h",EDBTypes.EDBDbType.Money,10,"h",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
					command.Prepare();
					command.Parameters[0].Value = 7369;
                    command.ExecuteNonQuery();

                    string cursorName1 = command.Parameters[1].Value.ToString();
                    string cursorName2 = command.Parameters[2].Value.ToString();
                    string cursorName3 = command.Parameters[3].Value.ToString();
                    string cursorName4 = command.Parameters[5].Value.ToString();

					Assert.AreEqual("100",Convert.ToString(command.Parameters[0].Value.ToString()));
					Assert.AreEqual("EnterpriseDB",command.Parameters[4].Value.ToString());
					Assert.AreEqual("106",command.Parameters[6].Value.ToString());
					Assert.AreEqual("99.90",command.Parameters[7].Value.ToString());

                    command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                    reader.Read();
					reader.Read();

					Assert.AreEqual("7499", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1600.00", Convert.ToString(reader[5].ToString()));
					reader.Close();
                
                    command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					
					Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));
					reader.Close();
					
                    command.CommandText = "FETCH ALL IN \"" + cursorName3 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("7654", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("MARTIN", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
					Assert.AreEqual("1250.00", Convert.ToString(reader[5].ToString()));
					reader.Close();
                
                    command.CommandText = "FETCH ALL IN \"" + cursorName4 + "\"";
                    command.CommandType = CommandType.Text;
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

					reader.Read();
					reader.Read();
					reader.Read();

					Assert.AreEqual("30", Convert.ToString(reader[0].ToString()));
					Assert.AreEqual("SALES", Convert.ToString(reader.GetString(1)));
					Assert.AreEqual("CHICAGO", Convert.ToString(reader.GetString(2)));
					reader.Close();

                    tran.Commit();
			
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message.ToString());
				}				
			}
			
			[Test]
			public void ProcRefCursorOutParameter() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					string CursorTable = "CREATE TABLE TestCursorTable (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
					com.CommandText = CursorTable;
					com.ExecuteNonQuery();

					string CursorInsert1 = "INSERT INTO TestCursorTable VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
					com.CommandText = CursorInsert1;
					com.ExecuteNonQuery();

					string CursorInsert2 = "INSERT INTO TestCursorTable VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
					com.CommandText = CursorInsert2;
					com.ExecuteNonQuery();

					string CursorInsert3 = "INSERT INTO TestCursorTable VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
					com.CommandText = CursorInsert3;
					com.ExecuteNonQuery();

					string CursorInsert4 = "INSERT INTO TestCursorTable VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
					com.CommandText = CursorInsert4;
					com.ExecuteNonQuery();

					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("RefCurProc(:v_id)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Prepare();
					command.ExecuteNonQuery();

                    string cursorName = command.Parameters[0].Value.ToString();

                    command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
					cur.Read();
					Assert.AreEqual("1", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("a", Convert.ToString(cur.GetString(3)));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.1", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Shehzad", Convert.ToString(cur.GetString(11)));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Hashim", Convert.ToString(cur.GetString(13)));
					
					cur.Read();
					Assert.AreEqual("2", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("b", Convert.ToString(cur.GetString(3)));
					Assert.AreEqual("10/10/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.2", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("3.30", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("3.3", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("EnterpriseDB", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("2/3/2005 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Great", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("3", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("c", Convert.ToString(cur.GetString(3)));
					Assert.AreEqual("11/1/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.3", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.10", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Islamabad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Sirsyed", Convert.ToString(cur[13].ToString()));

					cur.Read();
					Assert.AreEqual("4", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("d", Convert.ToString(cur.GetString(3)));
					Assert.AreEqual("2/3/1997 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.4", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("4", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("5", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Pakistan", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Endnews", Convert.ToString(cur[13].ToString()));
                    cur.Close();

					tran.Commit();	

					com.CommandText = "DROP TABLE TestCursorTable;";
					com.ExecuteNonQuery();
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void PackProcRefCursorOutParameter() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

                    string CursorTable = "CREATE TABLE IF NOT EXISTS PackFuncRefCursorOutParameter (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
					com.CommandText = CursorTable;
					com.ExecuteNonQuery();

                    string CursorInsert1 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
					com.CommandText = CursorInsert1;
					com.ExecuteNonQuery();

                    string CursorInsert2 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
					com.CommandText = CursorInsert2;
					com.ExecuteNonQuery();

                    string CursorInsert3 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
					com.CommandText = CursorInsert3;
					com.ExecuteNonQuery();

                    string CursorInsert4 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
					com.CommandText = CursorInsert4;
					com.ExecuteNonQuery();

					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("refcurpackproc.RefCursorsOUT(:v_id)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Prepare();
                    command.ExecuteNonQuery();
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);
					
					cur.Read();
					Assert.AreEqual("1", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("a", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.1", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Shehzad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Hashim", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("2", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("b", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("10/10/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.2", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("3.30", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("3.3", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("EnterpriseDB", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("2/3/2005 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Great", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("3", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("c", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("11/1/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.3", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.10", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Islamabad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Sirsyed", Convert.ToString(cur[13].ToString()));

					cur.Read();
					Assert.AreEqual("4", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("d", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("2/3/1997 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.4", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("4", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("5", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Pakistan", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Endnews", Convert.ToString(cur[13].ToString()));
                    cur.Close();

					tran.Commit();

                    com.CommandText = "DROP TABLE PackFuncRefCursorOutParameter;";
					com.ExecuteNonQuery();
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void ProcRefCursorOutWithBoolean() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutBool(:v_id,:v_bool)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_bool",	EDBTypes.EDBDbType.Boolean,10,"v_bool",ParameterDirection.Output,false, 8, 8,DataRowVersion.Current,true));
					command.Prepare();
					command.ExecuteNonQuery();
		
					Assert.AreEqual("True",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void ProcRefCursorOutWithBigInt() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutBigInt(:v_id,:v_bigint)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_bigint",	EDBTypes.EDBDbType.Bigint,10,"v_bigint",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,400));
					command.Prepare();
					command.ExecuteNonQuery();
					
					Assert.AreEqual("200",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void ProcRefCursorOutWithChar() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutChar(:v_id,:v_char)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_char",	EDBTypes.EDBDbType.Char,10,"v_char",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"Hashim"));
					command.Prepare();
					command.ExecuteNonQuery();
					Assert.AreEqual("Hashim",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();

                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithDoublePrecision() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutDoublePrecision(:v_id,:v_doublePrecision)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_doublePrecision",	EDBTypes.EDBDbType.Double,10,"v_doublePrecision",ParameterDirection.Output,false, 8, 8,DataRowVersion.Current,4.4009));
					command.Prepare();
					command.ExecuteNonQuery();
					
					Assert.AreEqual("2.9863",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();
					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithInteger() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutInteger(:v_id,:v_integer)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_integer", EDBTypes.EDBDbType.Integer,10,"v_integer",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
					command.Prepare();
					command.ExecuteNonQuery();

					Assert.AreEqual("263",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithNumeric() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutNumeric(:v_id,:v_numeric)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_numeric", EDBTypes.EDBDbType.Numeric,10,"v_numeric",ParameterDirection.Output,false,4,4,System.Data.DataRowVersion.Current,1)); 
					command.Prepare();
					command.ExecuteNonQuery();

					Assert.AreEqual("263000",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithNumeric2() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutNumeric2(:v_id,:v_numeric)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_numeric", EDBTypes.EDBDbType.Numeric,10,"v_numeric",ParameterDirection.Output,false,4,4,System.Data.DataRowVersion.Current,1)); 
					command.Prepare();
					command.ExecuteNonQuery();
					
					Assert.AreEqual("263000.24598", Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

                tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithReal() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutReal(:v_id,:v_real)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_real",	EDBTypes.EDBDbType.Double,10,"v_real",ParameterDirection.Output,false, 15, 15,DataRowVersion.Current,4.4));
					command.Prepare();
					command.ExecuteNonQuery();
					
					double start = 263001.25;
		//			Assert.AreEqual("263001.25",Convert.ToString(command.Parameters[1].Value.ToString()));
					Assert.AreEqual(start, command.Parameters[1].Value);
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

                tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithSmallInt() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutSmallInt(:v_id,:v_smallint)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_smallint",	EDBTypes.EDBDbType.Smallint,10,"v_smallint",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,400));
					command.Prepare();
					command.ExecuteNonQuery();

					Assert.AreEqual("26301",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("MANAGER", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutWithText() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutText(:v_id,:v_text)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_text",	EDBTypes.EDBDbType.Text,10,"v_text",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("Hashim",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("MANAGER", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void ProcRefCursorOutTimeStamp() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					string CursorTable = "CREATE TABLE TestCursorTable (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
					com.CommandText = CursorTable;
					com.ExecuteNonQuery();

					string CursorInsert1 = "INSERT INTO TestCursorTable VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
					com.CommandText = CursorInsert1;
					com.ExecuteNonQuery();

					string CursorInsert2 = "INSERT INTO TestCursorTable VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
					com.CommandText = CursorInsert2;
					com.ExecuteNonQuery();

					string CursorInsert3 = "INSERT INTO TestCursorTable VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
					com.CommandText = CursorInsert3;
					com.ExecuteNonQuery();

					string CursorInsert4 = "INSERT INTO TestCursorTable VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
					com.CommandText = CursorInsert4;
					com.ExecuteNonQuery();

					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("RefCurProc(:v_id)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Prepare();
					command.ExecuteNonQuery();

                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					cur.Read();
					Assert.AreEqual("2/3/2005 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					cur.Read();
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					cur.Read();
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
                    cur.Close();
					tran.Commit();	

					com.CommandText = "DROP TABLE TestCursorTable;";
					com.ExecuteNonQuery();
				}
				catch(EDBException e)
				{
                    Console.WriteLine(e.StackTrace);
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void ProcRefCursorOutWithVarchar() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCurProcOutVarchar(:v_id,:v_varchar)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_varchar",	EDBTypes.EDBDbType.Varchar,10,"v_varchar",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,"4"));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("Hashim",Convert.ToString(command.Parameters[1].Value.ToString()));
                
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

                tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithBoolean() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutBool(:v_id,:v_bool)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_bool",	EDBTypes.EDBDbType.Boolean,10,"v_bool",ParameterDirection.Output,false, 8, 8,DataRowVersion.Current,true));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("True",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("MANAGER", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void PackProcRefCursorOutWithBigInt() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutBigInt(:v_id,:v_bigint)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_bigint",	EDBTypes.EDBDbType.Bigint,10,"v_bigint",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,400));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("200",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void PackProcRefCursorOutWithChar() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutChar(:v_id,:v_char)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_char",	EDBTypes.EDBDbType.Char,10,"v_char",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"Hashim"));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("Hashim",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithDoublePrecision() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutDoublePrecision(:v_id,:v_doublePrecision)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
                    command.Parameters.Add(new EDBParameter("v_doublePrecision", EDBTypes.EDBDbType.Double, 10, "v_doublePrecision", ParameterDirection.Output, false, 8, 8, DataRowVersion.Current, 4.4009));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("2.9863",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("MANAGER", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithInteger() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutInteger(:v_id,:v_integer)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_integer", EDBTypes.EDBDbType.Integer,10,"v_integer",ParameterDirection.Output,false,2,2,System.Data.DataRowVersion.Current,1)); 
					command.Prepare();
					command.ExecuteNonQuery();

					Assert.AreEqual("263",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithNumeric() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutNumeric(:v_id,:v_numeric)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_numeric", EDBTypes.EDBDbType.Numeric,10,"v_numeric",ParameterDirection.Output,false,4,4,System.Data.DataRowVersion.Current,1)); 
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("263000",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithNumeric2() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutNumeric2(:v_id,:v_numeric)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_numeric", EDBTypes.EDBDbType.Numeric,10,"v_numeric",ParameterDirection.Output,false,4,4,System.Data.DataRowVersion.Current,1)); 
					command.Prepare();
					command.ExecuteNonQuery();
					
					Assert.AreEqual("263000.24598", Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
					Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithReal() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutReal(:v_id,:v_real)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_real",	EDBTypes.EDBDbType.Real,10,"v_real",ParameterDirection.Output,false, 8, 8,DataRowVersion.Current,4.4));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("263001.3",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
                    Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("SMITH", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("CLERK", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("ALLEN", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("WARD", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("SALESMAN", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
                    cur.Read();
                    Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
                    Assert.AreEqual("JONES", Convert.ToString(cur.GetString(1)));
                    Assert.AreEqual("MANAGER", Convert.ToString(cur.GetString(2)));
                    Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

                tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithSmallInt() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutSmallInt(:v_id,:v_smallint)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_smallint",	EDBTypes.EDBDbType.Smallint,10,"v_smallint",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,400));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("26301",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("MANAGER", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void PackProcRefCursorOutWithText() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

					EDBTransaction tran = con.BeginTransaction();

					EDBCommand command = new EDBCommand("RefCursorPackage.RefCurProcOutText(:v_id,:v_text)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_text",	EDBTypes.EDBDbType.Text,10,"v_text",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
					command.Prepare();
					command.ExecuteNonQuery();
                
					Assert.AreEqual("Hashim",Convert.ToString(command.Parameters[1].Value.ToString()));
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

					cur.Read();
					Assert.AreEqual("7369", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("SMITH", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("CLERK", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7902", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7499", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("ALLEN", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7521", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("WARD", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("SALESMAN", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7698", Convert.ToString(cur[3].ToString()));
					cur.Read();
					Assert.AreEqual("7566", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("JONES", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("MANAGER", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("7839", Convert.ToString(cur[3].ToString()));
                    cur.Close();

					tran.Commit();	
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			}

			[Test]
			public void FuncRefCursorOutParameter() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;

                    string CursorTable = "CREATE TABLE IF NOT EXISTS FuncRefCursorOutParameter (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
					com.CommandText = CursorTable;
					com.ExecuteNonQuery();

                    string CursorInsert1 = "INSERT INTO FuncRefCursorOutParameter VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
					com.CommandText = CursorInsert1;
					com.ExecuteNonQuery();

                    string CursorInsert2 = "INSERT INTO FuncRefCursorOutParameter VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
					com.CommandText = CursorInsert2;
					com.ExecuteNonQuery();

                    string CursorInsert3 = "INSERT INTO FuncRefCursorOutParameter VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
					com.CommandText = CursorInsert3;
					com.ExecuteNonQuery();

                    string CursorInsert4 = "INSERT INTO FuncRefCursorOutParameter VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
					com.CommandText = CursorInsert4;
					com.ExecuteNonQuery();

					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("RefCursorsOUT(:v_id)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,"Test"));
					command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric,10,"v_ret",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,100)); 
					command.Prepare();
					command.ExecuteNonQuery();
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);
					
					cur.Read();
					Assert.AreEqual("1", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("a", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.1", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Shehzad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Hashim", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("2", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("b", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("10/10/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.2", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("3.30", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("3.3", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("EnterpriseDB", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("2/3/2005 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Great", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("3", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("c", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("11/1/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.3", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.10", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Islamabad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Sirsyed", Convert.ToString(cur[13].ToString()));

					cur.Read();
					Assert.AreEqual("4", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("d", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("2/3/1997 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.4", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("4", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("5", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Pakistan", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Endnews", Convert.ToString(cur[13].ToString()));
                    cur.Close();

					tran.Commit();

                    com.CommandText = "DROP TABLE FuncRefCursorOutParameter;";
					com.ExecuteNonQuery();
				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}

			[Test]
			public void PackFuncRefCursorOutParameter() 
			{
				try 
				{
					EDBCommand com = new EDBCommand("",con);
					com.CommandType = CommandType.Text;


                    string CursorTable = "CREATE TABLE IF NOT EXISTS TestCursorTable (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
                    com.CommandText = CursorTable;
                    com.ExecuteNonQuery();

                    string CursorInsert1 = "INSERT INTO TestCursorTable VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
                    com.CommandText = CursorInsert1;
                    com.ExecuteNonQuery();

                    string CursorInsert2 = "INSERT INTO TestCursorTable VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
                    com.CommandText = CursorInsert2;
                    com.ExecuteNonQuery();

                    string CursorInsert3 = "INSERT INTO TestCursorTable VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
                    com.CommandText = CursorInsert3;
                    com.ExecuteNonQuery();

                    string CursorInsert4 = "INSERT INTO TestCursorTable VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
                    com.CommandText = CursorInsert4;
                    com.ExecuteNonQuery();

                    string CursorTable5 = "CREATE TABLE PackFuncRefCursorOutParameter (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
                    com.CommandText = CursorTable5;
					com.ExecuteNonQuery();

                    string CursorInsert6 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
                    com.CommandText = CursorInsert6;
					com.ExecuteNonQuery();

                    string CursorInsert8 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
                    com.CommandText = CursorInsert8;
					com.ExecuteNonQuery();

                    string CursorInsert9 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
                    com.CommandText = CursorInsert9;
					com.ExecuteNonQuery();

                    string CursorInsert10 = "INSERT INTO PackFuncRefCursorOutParameter VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
                    com.CommandText = CursorInsert10;
					com.ExecuteNonQuery();

					EDBTransaction tran = con.BeginTransaction();
					EDBCommand command = new EDBCommand("refcurpackfunc.RefCursorsOUT(:v_id)",con);
					command.CommandType = CommandType.StoredProcedure;
					command.Transaction = tran;
					command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor,0,"v_id", ParameterDirection.Output,false ,10,10,	System.Data.DataRowVersion.Current,null));
					command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric,10,"v_ret",ParameterDirection.ReturnValue,false,2,2,System.Data.DataRowVersion.Current,100)); 
					command.Prepare();
					command.ExecuteNonQuery();
                    string cursorName = command.Parameters[0].Value.ToString();
                    command.CommandText = "FETCH ALL IN \""+cursorName+"\"";
                    command.CommandType = CommandType.Text;
                    EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);
					
					cur.Read();
					Assert.AreEqual("1", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("a", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.1", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Shehzad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Hashim", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("2", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("b", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("10/10/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.2", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("3.30", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("3.3", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("2", Convert.ToString(cur[10].ToString()));     
					Assert.AreEqual("EnterpriseDB", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("2/3/2005 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Great", Convert.ToString(cur[13].ToString()));
					
					cur.Read();
					Assert.AreEqual("3", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("True", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("c", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("11/1/2007 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.3", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("3", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.10", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Islamabad", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Sirsyed", Convert.ToString(cur[13].ToString()));

					cur.Read();
					Assert.AreEqual("4", Convert.ToString(cur[0].ToString()));
					Assert.AreEqual("False", Convert.ToString(cur[1].ToString()));
					Assert.AreEqual("System.Byte[]", Convert.ToString(cur[2].ToString()));
					Assert.AreEqual("d", Convert.ToString(cur[3].ToString()));
					Assert.AreEqual("2/3/1997 12:00:00 AM", Convert.ToString(cur[4].ToString()));
					Assert.AreEqual("1.4", Convert.ToString(cur[5].ToString()));
					Assert.AreEqual("4", Convert.ToString(cur[6].ToString()));
					Assert.AreEqual("5", Convert.ToString(cur[7].ToString()));
					Assert.AreEqual("2.20", Convert.ToString(cur[8].ToString()));
					Assert.AreEqual("2.2", Convert.ToString(cur[9].ToString()));
					Assert.AreEqual("1", Convert.ToString(cur[10].ToString()));
					Assert.AreEqual("Pakistan", Convert.ToString(cur[11].ToString()));
					Assert.AreEqual("1/1/2006 12:00:00 AM", Convert.ToString(cur[12].ToString()));
					Assert.AreEqual("Endnews", Convert.ToString(cur[13].ToString()));
                    cur.Close();
                
					tran.Commit();

                    com.CommandText = "DROP TABLE PackFuncRefCursorOutParameter;";
					com.ExecuteNonQuery();
                    com.CommandText = "DROP TABLE TestCursorTable;";
                    com.ExecuteNonQuery();


				}
				catch(EDBException e)
				{
					throw new Exception(e.Message.ToString());
				}
			
			}
		}
#pragma warning restore CS8600
#pragma warning restore CS8602
}
