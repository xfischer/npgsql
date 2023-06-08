using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8604
#pragma warning disable CS8602
    /// <summary>
    /// Testing Procedures with Different combination of parameters
    /// </summary>
    [TestFixture]
    public class EDBPackageTest : TestBase
	{
		EDBConnection? con = null;

        #region Setup / Tear Down
        [SetUp]
		public void Init()
		{
			con = OpenConnection();

			EDBCommand com = new EDBCommand("",con);
			com.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE PKG_INVOKE_exec_pro IS\n"
								+ "PROCEDURE exec_pro(namein IN VARCHAR2,nameout OUT VARCHAR2);\n"
							    + "END PKG_INVOKE_exec_pro;\n";
			com.CommandText = strSql;
			com.ExecuteNonQuery();
			strSql = "CREATE OR REPLACE PACKAGE BODY PKG_INVOKE_exec_pro IS\n"
						+ "PROCEDURE local(namein VARCHAR2, nameout OUT VARCHAR2)\n"
						+ "IS\n"
						+ "BEGIN\n"
						+ "nameout := TRANSLATE(namein,'AEIOUaeiou','EIOUAeioua');\n"
						+ "END local;\n"
						+ "PROCEDURE exec_pro(namein IN VARCHAR2,nameout OUT VARCHAR2)\n"
						+ "IS\n"
						+ "countX NUMBER;\n"
						+ "BEGIN\n"
						+ "local(namein, nameout);\n"
						+ "END exec_pro;\n"
						+ "END PKG_INVOKE_exec_pro;\n";
			com.CommandText = strSql;
			com.ExecuteNonQuery();
            strSql = "CREATE OR REPLACE PACKAGE PKG_Variable_Test IS\n"
                        + "alpha   varchar(20);\n"
                        + "beta    numeric;\n"
                        + "PROCEDURE proc(aa OUT varchar,bb OUT numeric);\n"
                        + "END PKG_Variable_Test;\n";
            com.CommandText = strSql;
            com.ExecuteNonQuery();
            
            strSql = "CREATE OR REPLACE PACKAGE BODY PKG_Variable_Test IS\n"
						+ "PROCEDURE proc(aa OUT varchar,bb OUT numeric) IS\n"
						+ "BEGIN\n"
						+ "alpha := 'alpha';\n"
						+ "aa := alpha;\n"
						+ "beta := 2;\n"
						+ "bb := beta;\n"
						+ "END proc;\n"
						+ "END PKG_Variable_Test;\n";
			com.CommandText = strSql;
			com.ExecuteNonQuery();
            			
		}

		[TearDown] 
		public void Dispose()
		{
			//EDBCommand com = new EDBCommand("",con);
			//com.CommandType = CommandType.Text;

			//com.CommandText = "DROP PACKAGE PKG_INVOKE_exec_pro;";
			//com.ExecuteNonQuery();
			//com.CommandText = "DROP PACKAGE PKG_Variable_Test;";
			//com.ExecuteNonQuery();
	
			TestUtil.closeDB(con);
		}

        #endregion
        /// <summary>
        ////////////////////////////Scenerio//////////////
        //////////////////////////Create package spec without body
        //////////////////////////After Execution No error should occur
        /// </summary>
        [Test]
		public void testPackageWithoutBody()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;
			try
			{
				string strSql = "CREATE TABLE Test_Table( c1 CHAR(10))";///////";
				command.CommandText = strSql;
				command.ExecuteNonQuery();
				strSql = "INSERT INTO Test_Table VALUES ('Sarim');INSERT INTO Test_Table VALUES ('IS');INSERT INTO Test_Table VALUES ('Testing');INSERT INTO Test_Table VALUES ('Something')";///////";
				command.CommandText = strSql;
				command.ExecuteNonQuery();
				strSql = "CREATE OR REPLACE PACKAGE check_package IS   FUNCTION get_c1 (      p_c1        CHAR(10)   )    RETURN CHAR(10); END check_package";
				command.CommandText = strSql;
				command.ExecuteNonQuery();
				

				//////////tear down
				///
				command.Dispose();
				command = new EDBCommand("",con);
				command.CommandText = "Drop TABLE Test_Table";
				command.ExecuteNonQuery();
				command.CommandText = "Drop Package check_package";
				command.ExecuteNonQuery();
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

		}

        #region Procedures with in Packages

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument INT type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureINTWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePI1  IS  procedure get_c1(p_in in int,p_inout inout int,p_out out int) ;   END check_packagePI1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePI1  IS procedure get_c1(p_in in int,p_inout inout int,p_out out int)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePI1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePI1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	    EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Integer,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Prepare();
				
				command.ExecuteNonQuery();
				Assert.AreEqual(1,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1,int.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2,int.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePI1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument INT4 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureINT4WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePI2  IS  procedure get_c1(p_in in int4,p_inout inout int4,p_out out int4) ;   END check_packagePI2; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePI2  IS procedure get_c1(p_in in int4,p_inout inout int4,p_out out int4)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePI2;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePI2.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Integer,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Prepare();
	
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePI2;";
			command.ExecuteNonQuery();
		}
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument INT8 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureINT8WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePI3  IS  procedure get_c1(p_in in int8,p_inout inout int8,p_out out int8) ;   END check_packagePI3; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePI3  IS procedure get_c1(p_in in int8,p_inout inout int8,p_out out int8)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePI3;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePI3.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Bigint,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Bigint,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Bigint,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1", command.Parameters[0].Value.ToString());
				Assert.AreEqual("1", command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2", command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePI3;";
			command.ExecuteNonQuery();
		}
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument NUMERIC type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureNUMERICWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePN1  IS  procedure get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) ;   END check_packagePN1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePN1  IS procedure get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePN1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePN1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

				
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Numeric,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Numeric,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Numeric,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePN1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument FLOAT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureFLOATWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePF1  IS  procedure get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT) ;   END check_packagePF1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePF1  IS procedure get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePF1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePF1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Double, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Double, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Double, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 4.4));
				command.Prepare();

				command.ExecuteNonQuery();
				Assert.AreEqual("1.1", command.Parameters[0].Value.ToString());
				Assert.AreEqual("1.1", command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2.2", command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePF1;";
			command.ExecuteNonQuery();
		}
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument REAL type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureREALWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePR1  IS  procedure get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL) ;   END check_packagePR1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePR1  IS procedure get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePR1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePR1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Real, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Real, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Real, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 4.4));
				command.Prepare();

				command.ExecuteNonQuery();
				Assert.AreEqual("1.1", command.Parameters[0].Value.ToString());
				Assert.AreEqual("1.1", command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2.2", command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePR1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument CHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePC1  IS  procedure get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR) ;   END check_packagePC1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePC1  IS procedure get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePC1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePC1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Char,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Char,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Char,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Prepare();
				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePC1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument CHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureCHARACTERWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE PackageProcedureCharacter  IS  procedure get_c1(p_in in CHARACTER,p_inout inout CHARACTER,p_out out CHARACTER) ;   END PackageProcedureCharacter; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY PackageProcedureCharacter  IS procedure get_c1(p_in in CHARACTER,p_inout inout CHARACTER,p_out out CHARACTER)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END PackageProcedureCharacter;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("PackageProcedureCharacter.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Char,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Char,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Char,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package PackageProcedureCharacter;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument VARCHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureVARCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePVC1  IS  procedure get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR) ;   END check_packagePVC1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePVC1  IS procedure get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePVC1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePVC1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;
		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Varchar,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Varchar,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Varchar,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Prepare();
				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
                Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePVC1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument TEXT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageProcedureTEXTCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packagePTC1  IS  procedure get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT) ;   END check_packagePTC1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packagePTC1  IS procedure get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_packagePTC1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packagePTC1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Text,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Text,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Text,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Prepare();

				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packagePTC1;";
			command.ExecuteNonQuery();
		}

        #endregion

        #region Functions with in Packages

        /// <summary>
        /// ////////////////////////Calling a Function within a package with argument INT type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionINTWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageI3  IS  Function get_c1(p_in in int,p_inout inout int,p_out out int) return int;   END check_packageI3; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packageI3  IS Function get_c1(p_in in int,p_inout inout int,p_out out int) return int  IS a int :=3;  BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_packageI3;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageI3.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Integer,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Integer,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0));
				
				command.Prepare();

                command.ExecuteNonQuery();
				Assert.AreEqual(1,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1,int.Parse(command.Parameters[1].Value.ToString()));
				Assert.AreEqual(2,int.Parse(command.Parameters[2].Value.ToString()));	
				Assert.AreEqual(3,int.Parse(command.Parameters[3].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageI3;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument INT4 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionINT4WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageI2  IS  function get_c1(p_in in int4,p_inout inout int4,p_out out int4) return int4;   END check_packageI2; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_packageI2  IS function get_c1(p_in in int4,p_inout inout int4,p_out out int4) return int4  IS  a int4:=3; BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_packageI2;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageI2.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Integer,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Integer,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0));
				command.Prepare();
				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());
				Assert.AreEqual("3",command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageI2;";
			command.ExecuteNonQuery();
		}
		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument INT8 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionINT8WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageI1  IS  function get_c1(p_in in int8,p_inout inout int8,p_out out int8) return int8;   END check_packageI1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packageI1  IS function get_c1(p_in in int8,p_inout inout int8,p_out out int8) return int8  IS  a int8:=3; BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_packageI1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageI1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;
			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Bigint,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Bigint,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Bigint,10,"v_out",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Bigint,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0));
				command.Prepare();
				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());
				Assert.AreEqual("3",command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageI1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument NUMERIC type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionNUMERICWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;
			string strSql = "CREATE OR REPLACE PACKAGE check_packageN1  IS  function get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) return NUMERIC;   END check_packageN1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_packageN1  IS function get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) return NUMERIC  IS a NUMERIC:=3;  BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_packageN1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageN1.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Numeric,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Numeric,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Numeric,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Numeric,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());
				Assert.AreEqual("3",command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageN1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument FLOAT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionFLOATWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;
			string strSql = "CREATE OR REPLACE PACKAGE check_packageF1  IS  function get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT) return FLOAT;   END check_packageF1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_packageF1  IS function get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT)  return FLOAT IS  a FLOAT:=3.3; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_packageF1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();

            command.Dispose();
			//////////////code
			try
			{
                command = new EDBCommand("check_packageF1.get_c1(:v_in,:v_inout,:v_out)",con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Double, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Double, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Double, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 4.4));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Double, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 0.0));
				command.Prepare();
				
				command.ExecuteNonQuery();
				Assert.AreEqual("1.1", command.Parameters[0].Value.ToString());
				Assert.AreEqual("1.1", command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2.2", command.Parameters[2].Value.ToString());
				Assert.AreEqual("3.3", command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			//command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageF1;";
			command.ExecuteNonQuery();
		}
		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument REAL type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
        public void testPackageFunctionREALWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageR1  IS  function get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL) return REAL;   END check_packageR1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packageR1  IS function get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL) return REAL  IS  a REAL := 3.3; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_packageR1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageR1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Real, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Real, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Real, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 4.4));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Real, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 0.0));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1.1", command.Parameters[0].Value.ToString());
				Assert.AreEqual("1.1", command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2.2", command.Parameters[2].Value.ToString());	
				Assert.AreEqual("3.3", command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageR1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument CHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageC1  IS  function get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR) return CHAR;   END check_packageC1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packageC1  IS function get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR) return CHAR  IS  a CHAR := '3'; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_packageC1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageC1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Char,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Char,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Char,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Char,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,"0"));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
				Assert.AreEqual("3",command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageC1;";
			command.ExecuteNonQuery();
		}
		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument VARCHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionVARCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageVC1  IS  function get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR) return VARCHAR;   END check_packageVC1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packageVC1  IS function get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR) return VARCHAR  IS  a VARCHAR := '3'; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_packageVC1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageVC1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Varchar,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Varchar,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Varchar,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Varchar,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,"0"));
				command.Prepare();
	

				
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
				Assert.AreEqual("3",command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageVC1;";
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument TEXT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test, /*Ignore("Investigate")*/]
		public void testPackageFunctionTEXTWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			string strSql = "CREATE OR REPLACE PACKAGE check_packageT1  IS  function get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT) return TEXT;   END check_packageT1; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql = "CREATE OR REPLACE PACKAGE BODY check_packageT1  IS function get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT) return TEXT  IS  a TEXT := '3'; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_packageT1;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
			//////////////code
			try
			{
				command = new EDBCommand("check_packageT1.get_c1(:v_in,:v_inout,:v_out)", con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Text,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"1"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Text,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"2"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Text,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Text,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,"0"));
				command.Prepare();
	
				command.ExecuteNonQuery();
				Assert.AreEqual("1",command.Parameters[0].Value.ToString());
				Assert.AreEqual("1",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("2",command.Parameters[2].Value.ToString());	
				Assert.AreEqual("3",command.Parameters[3].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_packageT1;";
			command.ExecuteNonQuery();
		}

        #endregion

        ////////

        //		[Test]
        //		public void testProcedures()
        //		{
        //			/*try 
        //			{
        //				EDBCommand command = new EDBCommand("PKG_INVOKE_exec_pro.exec_pro(:namein,:nameout)",con);
        //				command.CommandType = CommandType.StoredProcedure;
        //
        //				command.Parameters.Add(new EDBParameter("namein",EDBTypes.EDBDbType.Varchar2,50));
        //				command.Parameters[0].Value = "Eminent";
        //
        //				command.Parameters.Add(new EDBParameter("nameout", 
        //										EDBTypes.EDBDbType.Varchar2,50,"nameout",
        //										ParameterDirection.Output,false, 10, 10,
        //										DataRowVersion.Current,""));
        //
        //				command.Prepare();
        //				command.ExecuteNonQuery();
        //
        //				Assert.AreEqual("Imonint",command.Parameters[1].Value.ToString());	
        //			}
        //			catch(EDBException e)
        //			{			
        //				throw new Exception(e.ToString());
        //			}*/
        //		}
        //		[Test]
        //		public void testVariables()
        //		{
        //			try 
        //			{
        //				EDBCommand command = new EDBCommand("PKG_Variable_Test.proc(:aa,:bb)",con);
        //				command.CommandType = CommandType.StoredProcedure;
        //
        //				command.Parameters.Add(new EDBParameter("aa", 
        //					EDBTypes.EDBDbType.Varchar,10,"aa",
        //					ParameterDirection.Output,false ,2,2,
        //					System.Data.DataRowVersion.Current,1));
        //
        //				command.Parameters.Add(new EDBParameter("bb", 
        //					EDBTypes.EDBDbType.Numeric,10,"bb",
        //					ParameterDirection.Output,false ,2,2,
        //					System.Data.DataRowVersion.Current,1));
        //
        //				command.Prepare();
        //				//command.ExecuteNonQuery();
        //				
        //				EDBDataReader ab = command.ExecuteReader();
        //				while(ab.Read())
        //				{
        //					Console.WriteLine(ab.GetValue(1).ToString());
        //					Console.WriteLine(ab.GetValue(0).ToString());
        //				}
        //
        ////				Assert.AreEqual("alpha",ab.GetValue(0).ToString());
        ////				Assert.AreEqual("2",int.Parse(ab.GetValue(1).ToString()));
        //			}
        //			catch(EDBException e)
        //			{			
        //				throw new Exception(e.ToString());
        //			}
        //		}

        #region TERSE Tests

        [Test]
        public void TERSE_PKG_PROC_NATIVE_INPUT_TYPES()

        {
            try
            {
                EDBCommand Command;

                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    Command.ExecuteNonQuery();
                    Command.Dispose();

                }

                catch (EDBException )

                {
                }

                Command = new EDBCommand("create or replace package terse_pkg1 is " +
                                         "  procedure terse_p1( a integer, b integer ); " +
                                         "end terse_pkg1;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg1 is " +
                                         "  procedure terse_p1( a integer, b integer ) is " +
                                         "  begin " +
                                         "      dbms_output.put_line('a = ' || a); " +
                                         "      dbms_output.put_line('b = ' || b); " +
                                         "  end; " +
                                         "end terse_pkg1;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("terse_pkg1.terse_p1(:a,:b)", con);

                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));

                Command.Parameters[0].Value = 50;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));

                Command.Parameters[1].Value = 51;

                Command.Prepare();

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("END;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

            }

            catch (EDBException exp)

            {
                throw new Exception(exp.ToString());

            }

        }

        [Test]
        public void TERSE_PKG_PROC_NATIVE_OUTPUT_TYPES()

        {
            try
            {
                EDBCommand Command;

                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("create or replace package terse_pkg2 is " +
                                         "  procedure terse_p1( a out integer, b out integer ); " +
                                         "end terse_pkg2;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg2 is " +
                                         "  procedure terse_p1( a out integer, b out integer ) is " +
                                         "  begin " +
                                         "      a := 10; " +
                                         "      b := 20; " +
                                         "  end; " +
                                         "end terse_pkg2;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    Command.ExecuteNonQuery();
                    Command.Dispose();

                }

                catch (EDBException )

                {
                }

                Command = new EDBCommand("terse_pkg2.terse_p1(:a,:b)", con);

                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));

                Command.Parameters[0].Direction = ParameterDirection.Output;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));

                Command.Parameters[1].Direction = ParameterDirection.Output;

                Command.Prepare();

                Command.ExecuteNonQuery();
                Assert.AreEqual(10, int.Parse(Command.Parameters[0].Value.ToString()));

                Assert.AreEqual(20, int.Parse(Command.Parameters[1].Value.ToString()));

                Command.Dispose();

                Command = new EDBCommand("END;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

            }

            catch (EDBException exp)

            {
                throw new Exception(exp.ToString());

            }

        }

        [Test]
        public void TERSE_PKG_PROC_MIXED_NATIVE_TYPES()

        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("create or replace package terse_pkg3 is " +
                                         "  procedure multipleInOutArg_test(a IN NUMERIC, b OUT NUMERIC, c IN NUMERIC, d OUT NUMERIC); " +
                                         "end terse_pkg3;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg3 is " +
                                         "  procedure multipleInOutArg_test(a IN NUMERIC, b OUT NUMERIC, c IN NUMERIC, d OUT NUMERIC) IS " +
                                         "  begin " +
				                         "      b := a; " +
                                         "      d := c; " +
                                         "  end; " +
                                         "end terse_pkg3;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    command.ExecuteNonQuery();
                    command.Dispose();

                }

                catch (EDBException )
                {
                }

                command = new EDBCommand("terse_pkg3.multipleInOutArg_test(:a,:b,:c,:d)", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Numeric));

                command.Parameters[0].Value = 5;

                command.Parameters.Add(new EDBParameter("b",

                    EDBTypes.EDBDbType.Integer, 10, "b",

                    ParameterDirection.Output, false, 2, 2,

                    System.Data.DataRowVersion.Current, 1));

                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Numeric));

                command.Parameters[2].Value = 15;

                command.Parameters.Add(new EDBParameter("d",

                EDBTypes.EDBDbType.Integer, 10, "d",

                ParameterDirection.Output, false, 2, 2,

                    System.Data.DataRowVersion.Current, 1));

                command.Prepare();

                command.ExecuteNonQuery();
                Assert.AreEqual(5, int.Parse(command.Parameters[1].Value.ToString()));

                Assert.AreEqual(15, int.Parse(command.Parameters[3].Value.ToString()));

                command.Dispose();

                command = new EDBCommand("END;", con);

                command.ExecuteNonQuery();
                command.Dispose();

            }

            catch (EDBException e)
            {
                throw new Exception(e.ToString());

            }

        }

        [Test]
        public void TERSE_PKG_PROC_CURSOR_TYPES()
        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                EDBTransaction tran = con.BeginTransaction();

                command = new EDBCommand("create or replace package terse_pkg4 is " +
                                         "  procedure cursortest2(c_1 OUT refcursor,c_2 OUT refcursor ); " +
                                         "end terse_pkg4;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg4 is " +
                                         "  procedure cursortest2(c_1 OUT refcursor, c_2 OUT refcursor) IS " +
                                         "  BEGIN " +
                                         "      open c_1 for select * from emp order by empno; " +
                                         "      open c_2 for select * from emp order by empno; " +
                                         "  END; " +
                                         "end terse_pkg4;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    command.ExecuteNonQuery();
                    command.Dispose();

                }

                catch (EDBException )
                {
                }

                command = new EDBCommand("terse_pkg4.cursortest2(:cur1,:cur2)", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Transaction = tran;

                //REFCUSOR CommandBehavior.SequentialAccess

                command.Parameters.Add(new EDBParameter("cur1", EDBTypes.EDBDbType.Refcursor, 10, "cur1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("cur2", EDBTypes.EDBDbType.Refcursor, 10, "cur2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                command.Prepare();
                command.ExecuteNonQuery();
                string? cursorName1 = command.Parameters[0].Value.ToString();
                string? cursorName2 = command.Parameters[1].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

                
                rst.Read();

                Assert.AreEqual("7369", Convert.ToString(rst[0].ToString()));

                Assert.AreEqual("SMITH", Convert.ToString(rst[1].ToString()));

                Assert.AreEqual("CLERK", Convert.ToString(rst[2].ToString()));

                Assert.AreEqual("7902", Convert.ToString(rst[3].ToString()));

                Assert.AreEqual("800.00", Convert.ToString(rst[5].ToString()));

                rst.Close();

                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
                rst.Read();

                rst.Read();

                rst.Read();

                Assert.AreEqual("7521", Convert.ToString(rst[0].ToString()));

                Assert.AreEqual("WARD", Convert.ToString(rst[1].ToString()));

                Assert.AreEqual("SALESMAN", Convert.ToString(rst[2].ToString()));

                Assert.AreEqual("7698", Convert.ToString(rst[3].ToString()));

                Assert.AreEqual("1250.00", Convert.ToString(rst[5].ToString()));

                rst.Close();

                tran.Commit();


            }

            catch (EDBException exp)
            {
                Console.WriteLine("Exception: " + exp.ToString());

            }

        }

        [Test]
        public void TERSE_PKG_PROC_MIXED_NATIVE_CURSOR_TYPES()
        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                EDBTransaction tran = con.BeginTransaction();
                command = new EDBCommand("create or replace package terse_pkg5 is " +
                                         "  procedure refcur_callee2(c_1 OUT numeric, c_2 IN OUT refcursor, c_3 IN OUT refcursor); " +
                                         "end terse_pkg5;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg5 is " +
                                         "  procedure refcur_callee2(c_1 OUT numeric, c_2 IN OUT refcursor, c_3 IN OUT refcursor) IS " +
                                         "  begin " +
                                         "      c_1 := 100; " +
                                         "      open c_2 for SELECT * FROM emp ORDER BY empno; " +
                                         "      open c_3 for SELECT ename FROM emp WHERE empno = 7369; " +
                                         "  end; " +
                                         "end terse_pkg5;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException )
                {
                }

                command = new EDBCommand("terse_pkg5.refcur_callee2(:b,:a,:c)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Numeric, 10, "b", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Refcursor, 10, "a", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Refcursor, 10, "c", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));

                command.Prepare();
                command.Parameters[0].Value = 7369;
                command.ExecuteNonQuery();

                Assert.AreEqual("100", Convert.ToString(command.Parameters[0].Value.ToString()));

                string? cursorName1 = command.Parameters[1].Value.ToString();
                string? cursorName2 = command.Parameters[2].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();
                reader.Read();

                Assert.AreEqual("7499", Convert.ToString(reader[0].ToString()));
                Assert.AreEqual("ALLEN", Convert.ToString(reader[1].ToString()));
                Assert.AreEqual("SALESMAN", Convert.ToString(reader[2].ToString()));
                Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
                Assert.AreEqual("1600.00", Convert.ToString(reader[5].ToString()));

                reader.Close();

                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();

                Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));

                reader.Close();
                tran.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        [Test]
        public void TERSE_PKG_PROC_DEFAULT_TYPES()
        {
            try
            {
                EDBCommand Command;
                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);
                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("create or replace package terse_pkg6 is " +
                                         "  procedure terse_p2( a integer, b integer default 10); " +
                                         "end terse_pkg6;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg6 is " +
                                         "  procedure terse_p2( a integer, b integer default 10) is " +
                                         "  begin " +
                                         "      dbms_output.put_line('a = ' || a); " +
                                         "      dbms_output.put_line('b = ' || b); " +
                                         "  end; " +
                                         "end terse_pkg6;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    Command.ExecuteNonQuery();
                    Command.Dispose();
                }
                catch (EDBException )
                {
                }

                Command = new EDBCommand("terse_pkg6.terse_p2(:a)", con);
                Command.CommandType = CommandType.StoredProcedure;
                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
                Command.Parameters[0].Value = 50;

                Command.Prepare();

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("END;", con);
                Command.ExecuteNonQuery();
                Command.Dispose();
            }
            catch (EDBException exp)
            {
                throw new Exception(exp.ToString());
            }

        }

        [Test, /*Ignore("Investigate exception caught")*/]
        public void TERSE_PKG_FUNC_NATIVE_INPUT_TYPES_RETURN_INTEGER()
        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("create or replace package terse_pkg8 is " +
                                         "  Function FunconeInArg_test2(a IN NUMERIC) return INTEGER; " +
                                         "end terse_pkg8;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg8 is " +
                                         "  Function FunconeInArg_test2(a IN NUMERIC) return INTEGER is " +
                                         "      b NUMBER(2); " +
                                         "  begin " +
                                         "      b := a; " +
                                         "      return 30; " +
                                         "  end; " +
                                         "end terse_pkg8;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException)
                {
                }

                command = new EDBCommand("terse_pkg8.FunconeInArg_test2(:param1)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Prepare();

                command.Parameters[0].Value = 3;
                command.ExecuteNonQuery();

                Assert.AreEqual(3, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("30", command.Parameters[1].Value.ToString());

                command = new EDBCommand("END;", con);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }

        }

        [Test, /*Ignore("Investigate exception caught")*/]
        public void TERSE_PKG_FUNC_NATIVE_INPUT_TYPES()
        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("create or replace package terse_pkg7 is " +
                                         "  Function FunconeInArg_test(a IN NUMERIC) return varchar; " +
                                         "end terse_pkg7;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg7 is " +
                                         "  Function FunconeInArg_test(a IN NUMERIC) return varchar is " +
                                         "      b NUMBER(2); " +
                                         "  begin " +
                                         "      b := a; " +
                                         "      return 'EnterpriseDB'; " +
                                         "  end; " +
                                         "end terse_pkg7;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException )
                {
                }

                command = new EDBCommand("terse_pkg7.FunconeInArg_test(:param1)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Prepare();

                command.Parameters[0].Value = 3;
                command.ExecuteNonQuery();

                Assert.AreEqual(3, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[1].Value.ToString());

                command = new EDBCommand("END;", con);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }

        }

        [Test, Ignore("")]
        public void TERSE_PKG_FUNC_NATIVE_OUTPUT_TYPES_RETURN_VARCHAR()

        {
            try
            {
                EDBCommand Command;

                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    Command.ExecuteNonQuery();
                    Command.Dispose();

                }

                catch (EDBException)

                {
                }

                Command = new EDBCommand("create or replace package terse_pkg8 is " +
                                         "  Function terse_f1( a out integer, b out integer ) return VARCHAR; " +
                                         "end terse_pkg8;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg8 is " +
                                         "  Function terse_f1( a out integer, b out integer ) return VARCHAR IS " +
                                         "  begin " +
                                         "      a := 10; " +
                                         "      b := 20; " +
                                         "      return '30'; " +
                                         "  end; " +
                                         "end terse_pkg8;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("terse_pkg8.terse_f1(:a,:b)", con);

                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));

                Command.Parameters[0].Direction = ParameterDirection.Output;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));

                Command.Parameters[1].Direction = ParameterDirection.Output;

                //Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer, 10, "v_ret",
                //    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                //Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer, 10, "v_ret",
                //    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                //Command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer));

                //Command.Parameters[2].Direction = ParameterDirection.ReturnValue;

                Command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 10, "v_ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                Command.Prepare();

                Command.ExecuteNonQuery();
                Assert.AreEqual(10, int.Parse(Command.Parameters[0].Value.ToString()));

                Assert.AreEqual(20, int.Parse(Command.Parameters[1].Value.ToString()));

                Assert.AreEqual("30", Command.Parameters[2].Value.ToString());

                Command.Dispose();

                Command = new EDBCommand("END;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

            }

            catch (EDBException exp)

            {
                throw new Exception(exp.ToString());

            }

        }

        [Test]
        public void TERSE_PKG_FUNC_NATIVE_INOUT_TYPES()

        {
            try
            {
                EDBCommand Command;

                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    Command.ExecuteNonQuery();
                    Command.Dispose();

                }

                catch (EDBException)

                {
                }

                Command = new EDBCommand("create or replace package terse_pkg8 is " +
                                         "  Function terse_f1( a IN OUT integer, b IN OUT integer ) return integer; " +
                                         "end terse_pkg8;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg8 is " +
                                         "  Function terse_f1( a IN OUT integer, b IN OUT integer ) return integer IS " +
                                         "  begin " +
                                         "      a := 10; " +
                                         "      b := 20; " +
                                         "      return 30; " +
                                         "  end; " +
                                         "end terse_pkg8;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("terse_pkg8.terse_f1(:a,:b)", con);

                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));

                Command.Parameters[0].Direction = ParameterDirection.InputOutput;
                Command.Parameters[0].Value = 400;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));
                Command.Parameters[1].Value = 400;

                Command.Parameters[1].Direction = ParameterDirection.InputOutput;


                Command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer, 10, "v_ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                Command.Prepare();

                Command.ExecuteNonQuery();
                Assert.AreEqual(10, int.Parse(Command.Parameters[0].Value.ToString()));

                Assert.AreEqual(20, int.Parse(Command.Parameters[1].Value.ToString()));

                Assert.AreEqual(30, int.Parse(Command.Parameters[2].Value.ToString()));

                Command.Dispose();

                Command = new EDBCommand("END;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

            }

            catch (EDBException exp)

            {
                throw new Exception(exp.ToString());

            }

        }

        [Test]
        public void TERSE_PKG_FUNC_NATIVE_OUTPUT_TYPES()

        {
            try
            {
                EDBCommand Command;

                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    Command.ExecuteNonQuery();
                    Command.Dispose();

                }

                catch (EDBException )

                {
                }

                Command = new EDBCommand("create or replace package terse_pkg8 is " +
                                         "  Function terse_f1( a out integer, b out integer ) return integer; " +
                                         "end terse_pkg8;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg8 is " +
                                         "  Function terse_f1( a out integer, b out integer ) return integer IS " +
                                         "  begin " +
                                         "      a := 10; " +
                                         "      b := 20; " +
                                         "      return 30; " +
                                         "  end; " +
                                         "end terse_pkg8;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("terse_pkg8.terse_f1(:a,:b)", con);

                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));

                Command.Parameters[0].Direction = ParameterDirection.Output;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));

                Command.Parameters[1].Direction = ParameterDirection.Output;

                //Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer, 10, "v_ret",
                //    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                //Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer, 10, "v_ret",
                //    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                Command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer, 10, "v_ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                Command.Prepare();

                Command.ExecuteNonQuery();
                Assert.AreEqual(10, int.Parse(Command.Parameters[0].Value.ToString()));

                Assert.AreEqual(20, int.Parse(Command.Parameters[1].Value.ToString()));

                Assert.AreEqual(30, int.Parse(Command.Parameters[2].Value.ToString()));

                Command.Dispose();

                Command = new EDBCommand("END;", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

            }

            catch (EDBException exp)

            {
                throw new Exception(exp.ToString());

            }

        }

        [Test, Ignore("Investigate exception caught")]
        public void TERSE_PKG_FUNC_MIXED_NATIVE_TYPES()
        {
            try
            {
                EDBCommand command;
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("create or replace package terse_pkg9 is " +
                                         "  Function functionsanity(a1 OUT NUMERIC, a2 OUT NUMERIC, a3 IN NUMERIC,a4 OUT NUMERIC) return Varchar; " +
                                         "end terse_pkg9;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg9 is " +
                                         "  Function functionsanity(a1 OUT NUMERIC, a2 OUT NUMERIC, a3 IN NUMERIC,a4 OUT NUMERIC) return Varchar IS " +
                                         "  begin " +
                                         "      a1 := 100; " +
                                         "      a2 := 200; " +
                                         "      a4 := 400; " +
                                         "      RETURN 'EnterpriseDB'; " +
                                         "  end; " +
                                         "end terse_pkg9;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    command.ExecuteNonQuery();
                    command.Dispose();

                }

                catch (EDBException )

                {
                }

                command = new EDBCommand("terse_pkg9.functionsanity(:param1,:param2,:param3,:param4)", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer, 10, "param3", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();
                command.Parameters[0].Value = 1;
                command.Parameters[1].Value = null;
                command.Parameters[2].Value = 3;
                command.Parameters[3].Value = null;

                command.ExecuteNonQuery();

                Assert.AreEqual(100, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(200, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(3, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(400, int.Parse(command.Parameters[3].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[4].Value.ToString());

                command.Dispose();

                command = new EDBCommand("END;", con);
                command.ExecuteNonQuery();
                command.Dispose();

            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }

        }

        [Test]
        public void TERSE_PKG_FUNC_CURSOR_TYPES()
        {
            try
            {
                EDBCommand com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                string CursorTable = "CREATE TABLE IF NOT EXISTS TestCursorTable (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
                com.CommandText = CursorTable;
                com.ExecuteNonQuery();
                CursorTable = "CREATE OR REPLACE package terse_pkg10 is " +
                              "     Function RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR) return NUMERIC;" +
                              "end terse_pkg10;";
                com.CommandText = CursorTable;
                com.ExecuteNonQuery();
                CursorTable = "CREATE OR REPLACE PACKAGE BODY terse_pkg10 is " +
                              "     Function RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR) return NUMERIC IS " +
                              "     BEGIN " +
                              "         OPEN Test_RefCursor FOR SELECT * FROM TestCursorTable; " +
                              "         return 10; " +
                              "     END; " +
                              "end terse_pkg10;";

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
                com = new EDBCommand("set edb_stmt_level_tx to on;", con);
                com.ExecuteNonQuery();
                com.Dispose();

                EDBTransaction tran = con.BeginTransaction();

                try
                {
                    com = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    com.ExecuteNonQuery();
                    com.Dispose();
                }
                catch (EDBException )
                {
                }

                EDBCommand command = new EDBCommand("terse_pkg10.RefCursorsOUT(:v_id)", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Transaction = tran;

                command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor, 0, "v_id", ParameterDirection.Output, false, 10, 10, System.Data.DataRowVersion.Current, null));

                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));

                command.Prepare();
                command.ExecuteNonQuery();

                string? cursorName = command.Parameters[0].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
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
                
                com.CommandText = "DROP TABLE TestCursorTable;";

                com.ExecuteNonQuery();
            }

            catch (EDBException e)
            {
                EDBCommand command = new EDBCommand("DROP TABLE IF EXISTS TestCursorTable;", con);
                command.ExecuteNonQuery();
                throw new Exception(e.ToString());
            }
        }

        [Test]
        public void TERSE_PKG_FUNC_MIXED_NATIVE_CURSOR_TYPES()
        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                EDBTransaction tran = con.BeginTransaction();

                command = new EDBCommand("create or replace package terse_pkg11 is " +
                                         "  Function refcur_callee2_func( c_1 OUT numeric, " +
                                         "                                c_2 IN OUT refcursor, " +
                                         "                                c_3 IN OUT refcursor ) return numeric; " +
                                         "end terse_pkg11;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg11 is " +
                                         "  Function refcur_callee2_func( c_1 OUT numeric, " +
                                         "                                c_2 IN OUT refcursor, " +
                                         "                                c_3 IN OUT refcursor ) return numeric is " +
                                         "  begin " +
                                         "      c_1 := 100; " +
                                         "      open c_2 for select * from emp ORDER BY empno; " +
                                         "      open c_3 for select ename from emp  WHERE empno = 7369; " +
                                         "      return c_1; " +
                                         "  end; " +
                                         "end terse_pkg11;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    command.ExecuteNonQuery();
                    command.Dispose();

                }
                catch (EDBException )
                {
                }

                command = new EDBCommand("terse_pkg11.refcur_callee2_func(:b,:a,:c)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Numeric, 10, "b", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Refcursor, 10, "a", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Refcursor, 10, "c", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Numeric, 10, "ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                command.Prepare();
                command.Parameters[0].Value = 7369;

                command.ExecuteNonQuery();
                
                Assert.AreEqual("100", Convert.ToString(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("100", Convert.ToString(command.Parameters[3].Value.ToString()));

                string? cursorName1 = command.Parameters[1].Value.ToString();
                string? cursorName2 = command.Parameters[2].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();
                reader.Read();

                Assert.AreEqual("7499", Convert.ToString(reader.GetString(0)));
                Assert.AreEqual("ALLEN", Convert.ToString(reader.GetString(1)));
                Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
                Assert.AreEqual("7698", Convert.ToString(reader.GetString(3)));
                Assert.AreEqual("1600", Convert.ToString(reader.GetString(5)));

                reader.Close();

                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();

                Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));

                reader.Close();
                tran.Commit();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        [Test]
        public void TERSE_PKG_FUNC_DEFAULT_TYPES()
        {
            try
            {
                EDBCommand command;

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("create or replace package terse_pkg12 is " +
                                         "  Function terse_func_defvals( param1 integer, param2 integer default 10 ) return varchar2; " +
                                         "end terse_pkg12;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY terse_pkg12 is " +
                                         "  Function terse_func_defvals( param1 integer, param2 integer default 10 ) return varchar2 IS " +
                                         "  begin " +
                                         "      return 'EnterpriseDB'; " +
                                         "  end; " +
                                         "end terse_pkg12;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException )
                {
                }

                command = new EDBCommand("terse_pkg12.terse_func_defvals(:param1)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Prepare();

                command.Parameters[0].Value = 3;
                command.ExecuteNonQuery();

                Assert.AreEqual(3, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[1].Value.ToString());

                command = new EDBCommand("END;", con);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        #endregion
    }

#pragma warning restore CS8604
#pragma warning restore CS8602
}
