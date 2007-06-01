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
	public class PackageTest
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{
			con = TestUtil.openDB();

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
						+ "END PKG_Variable_Test;\n"
						+ "CREATE OR REPLACE PACKAGE BODY PKG_Variable_Test IS\n"
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
			EDBCommand com = new EDBCommand("",con);
			com.CommandType = CommandType.Text;

			com.CommandText = "DROP PACKAGE PKG_INVOKE_exec_pro;";
			com.ExecuteNonQuery();

			com.CommandText = "DROP PACKAGE PKG_Variable_Test;";
			com.ExecuteNonQuery();
	
			TestUtil.closeDB(con);
		}
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

//////////////////////////////////////////Procedures with in Packages
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument INT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureINTWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in int,p_inout inout int,p_out out int) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in int,p_inout inout int,p_out out int)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument INT4 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureINT4WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in int4,p_inout inout int4,p_out out int4) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in int4,p_inout inout int4,p_out out int4)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument INT8 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureINT8WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in int8,p_inout inout int8,p_out out int8) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in int8,p_inout inout int8,p_out out int8)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Bigint,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Bigint,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Bigint,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument NUMERIC type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureNUMERICWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

				
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Numeric,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Numeric,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Numeric,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument FLOAT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureFLOATWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

				
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Float,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1.1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Float,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2.2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Float,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4.4));
				command.Prepare();
	

				
				command.ExecuteNonQuery();

				Assert.AreEqual(1.1,float.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1.1,float.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2.2,float.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument REAL type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureREALWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Float,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1.1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Float,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2.2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Float,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4.4));
				command.Prepare();
	

				
				command.ExecuteNonQuery();

				Assert.AreEqual(1.1,float.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1.1,float.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2.2,float.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument CHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
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
		[Test]
		public void testPackageProcedureVARCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument TEXT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageProcedureTEXTCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT) ;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}


//////////////////////////////Functions with in Packages

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument INT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionINTWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  Function get_c1(p_in in int,p_inout inout int,p_out out int) return int;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS Function get_c1(p_in in int,p_inout inout int,p_out out int) return int  IS a int :=3;  BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument INT4 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionINT4WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in int4,p_inout inout int4,p_out out int4) return int4;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in int4,p_inout inout int4,p_out out int4) return int4  IS  a int4:=3; BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument INT8 type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionINT8WithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in int8,p_inout inout int8,p_out out int8) return int8;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in int8,p_inout inout int8,p_out out int8) return int8  IS  a int8:=3; BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Bigint,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Bigint,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Bigint,10,"v_out",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Bigint,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0));
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument NUMERIC type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionNUMERICWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) return NUMERIC;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) return NUMERIC  IS a NUMERIC:=3;  BEGIN  p_out:=p_inout; p_inout:=p_in; return a;  END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Numeric,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Numeric,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Numeric,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Numeric,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0));
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument FLOAT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionFLOATWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT) return FLOAT;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT)  return FLOAT IS  a FLOAT:=3.3; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Float,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1.1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Float,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2.2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Float,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4.4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Float,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0.0));
				command.Prepare();
	

				
				command.ExecuteNonQuery();

				Assert.AreEqual(1.1,float.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1.1,float.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2.2,float.Parse(command.Parameters[2].Value.ToString()));
				Assert.AreEqual(3.3,float.Parse(command.Parameters[3].Value.ToString()));
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument REAL type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionREALWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL) return REAL;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in REAL,p_inout inout REAL,p_out out REAL) return REAL  IS  a REAL := 3.3; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Float,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1.1));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Float,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2.2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Float,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,4.4));
				command.Parameters.Add(new EDBParameter("v_ret",	EDBTypes.EDBDbType.Float,10,"v_ret",ParameterDirection.ReturnValue,false, 2, 2,DataRowVersion.Current,0.0));
				command.Prepare();
	

				
				command.ExecuteNonQuery();

				Assert.AreEqual(1.1,float.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1.1,float.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2.2,float.Parse(command.Parameters[2].Value.ToString()));	
				Assert.AreEqual(3.3,float.Parse(command.Parameters[3].Value.ToString()));
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP package check_package;";
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


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR) return CHAR;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in CHAR,p_inout inout CHAR,p_out out CHAR) return CHAR  IS  a CHAR := '3'; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument VARCHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionVARCHARWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR) return VARCHAR;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in VARCHAR,p_inout inout VARCHAR,p_out out VARCHAR) return VARCHAR  IS  a VARCHAR := '3'; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}

		/// <summary>
		/// ////////////////////////Calling a Function within a package with argument TEXT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public void testPackageFunctionTEXTWithInInoutOut()
		{
			//////prereq
			EDBCommand command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			string strSql = "CREATE OR REPLACE PACKAGE check_package  IS  function get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT) return TEXT;   END check_package; ";
			command.CommandText = strSql;
			command.ExecuteNonQuery();
				
			strSql ="CREATE OR REPLACE PACKAGE BODY check_package  IS function get_c1(p_in in TEXT,p_inout inout TEXT,p_out out TEXT) return TEXT  IS  a TEXT := '3'; BEGIN  p_out:=p_inout; p_inout:=p_in;  return a; END;END check_package;";
			command.CommandText = strSql;
			command.ExecuteNonQuery();


			//////////////code
			try
			{
				command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)",con);
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
			command.CommandText = "DROP package check_package;";
			command.ExecuteNonQuery();

		}
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
	}
}
