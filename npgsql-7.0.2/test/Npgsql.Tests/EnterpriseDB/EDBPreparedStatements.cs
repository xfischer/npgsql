using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests.EntepriseDB
{
#pragma warning disable CS8602
    /// <summary>
    /// Summary description for PreparedStatements.
    /// </summary>
    [TestFixture] 
	public class EDBPreparedStatements : TestBase
    {	
		EDBConnection? conn = null;

		[SetUp]
		public void Init()
		{	
			conn = OpenConnection();
			
		}
		protected void TearDown()
		{
			if (conn.State != ConnectionState.Closed)
				conn.Close();
		}

		[Test]
		public void testprepaed_statemant1()
		{
			try
			{
				string updateQuery  = "update emp set ename = :Name where empno = :ID";
				
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
				Prepared_command.CommandType = CommandType.Text;
			
				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));
			
				Prepared_command.Prepare();
			
				Prepared_command.Parameters[0].Value = 7369;
				Prepared_command.Parameters[1].Value = "Mark";
				
				Prepared_command.ExecuteNonQuery();

				string updateQuery1  = "update emp set ename = :Name where empno = :ID";
				
				EDBCommand Prepared_command1 = new EDBCommand(updateQuery1, conn);
				Prepared_command1.CommandType = CommandType.Text;
			
				Prepared_command1.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command1.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));
			
				Prepared_command1.Prepare();
			
				Prepared_command1.Parameters[0].Value = 7369;
				Prepared_command1.Parameters[1].Value = "SMITH";
				
				Prepared_command1.ExecuteNonQuery();
				
				
							
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
			}

		}
		[Test]
		public void testprepared_statemant2()
        {
			try
			{
				string updateQuery  = "select ename from emp where  empno = :ID";
							
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
				Prepared_command.CommandType = CommandType.Text;
			
				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command.Prepare();
			
				Prepared_command.Parameters[0].Value = 7369;
				EDBDataReader reader = Prepared_command.ExecuteReader();
				while(reader.Read())
				{
					Assert.AreEqual("SMITH",reader.GetValue(0).ToString().ToUpper());
				
				}
				reader.Close();
			}
			
						
			catch(EDBException exp)
			{
							
				Console.WriteLine(exp.ToString()); 
				
			}	 
			 
		}
		[Test]
		public void testprepaed_statemant3()
		{
			try
			{			
				string updateQuery  = "select * from emp where  empno = :ID";
							
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
				Prepared_command.CommandType = CommandType.Text;
			
				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command.Prepare();
			
				Prepared_command.Parameters[0].Value = 7369;
				EDBDataReader reader = Prepared_command.ExecuteReader();
				reader.Read();

				Assert.AreEqual("7369",reader.GetValue(0).ToString());
				Assert.AreEqual("SMITH",reader.GetValue(1).ToString().ToUpper());
				Assert.AreEqual("CLERK",reader.GetValue(2).ToString().ToUpper());
				Assert.AreEqual("7902",reader.GetValue(3).ToString());	
				Assert.AreEqual("800.00",reader.GetValue(5).ToString());
				
				Console.WriteLine("Success...");
				reader.Close();
			
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
			}
		
		}
		[Test]
		public void testprepaed_statemant4()
		{
			try
			{
				
				string updateQuery  = "select * from emp ,dept  where dept.deptno = emp.deptno and empno = :ID";
				
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
				Prepared_command.CommandType = CommandType.Text;
			
				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command.Prepare();
			
				Prepared_command.Parameters[0].Value = 7369;
				EDBDataReader reader = Prepared_command.ExecuteReader();
				reader.Read();			
			
				Assert.AreEqual("7369",reader.GetValue(0).ToString());
				Assert.AreEqual("SMITH",reader.GetValue(1).ToString().ToUpper());
				Assert.AreEqual("CLERK",reader.GetValue(2).ToString().ToUpper());
				Assert.AreEqual("7902",reader.GetValue(3).ToString());	
				Assert.AreEqual("800.00",reader.GetValue(5).ToString());
				reader.Close();
				Console.WriteLine("Success...");
			
			}
			
						
			catch(EDBException exp)
			{
							
				Console.WriteLine(exp.ToString()); 
			
				
			}
		
		}

		[Test]
		public void testprepaed_statemant5()
		{
			try
			{
				string updateQuery  = "select * from emp ,dept  where dept.deptno = emp.deptno and empno = :ID and dept.deptno = :deptno";
							
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
				Prepared_command.CommandType = CommandType.Text;
			
				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command.Parameters.Add(new EDBParameter("deptno", EDBTypes.EDBDbType.Integer));
				Prepared_command.Prepare();
			
				Prepared_command.Parameters[0].Value = 7369;
				Prepared_command.Parameters[1].Value = 20;
				EDBDataReader reader = Prepared_command.ExecuteReader();
				reader.Read();
				
				Assert.AreEqual("7369",reader.GetValue(0).ToString());
				Assert.AreEqual("SMITH",reader.GetValue(1).ToString().ToUpper());
				Assert.AreEqual("CLERK",reader.GetValue(2).ToString().ToUpper());
				Assert.AreEqual("7902",reader.GetValue(3).ToString());	
				Assert.AreEqual("800.00",reader.GetValue(5).ToString());
				Assert.AreEqual("20",reader.GetValue(7).ToString());
				reader.Close();
				
				Console.WriteLine("Success...");
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
				
			}
		}
		[Test]
		public void testprepaed_statemant6()
		{
		
			try
			{
				string updateQuery  = "select * from emp ,dept  where dept.deptno = emp.deptno and empno = :ID and dept.deptno = :deptno and dname = :dname";
							
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
				Prepared_command.CommandType = CommandType.Text;
			
				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
				Prepared_command.Parameters.Add(new EDBParameter("deptno", EDBTypes.EDBDbType.Integer));
				Prepared_command.Parameters.Add(new EDBParameter("dname", EDBTypes.EDBDbType.Varchar));
				Prepared_command.Prepare();
			
				Prepared_command.Parameters[0].Value = 7369;
				Prepared_command.Parameters[1].Value = 20;
				Prepared_command.Parameters[2].Value = "RESEARCH";
				
				EDBDataReader reader = Prepared_command.ExecuteReader();
				reader.Read();
				Assert.AreEqual("7369",reader.GetValue(0).ToString());
				Assert.AreEqual("SMITH",reader.GetValue(1).ToString().ToUpper());
				Assert.AreEqual("CLERK",reader.GetValue(2).ToString().ToUpper());
				Assert.AreEqual("7902",reader.GetValue(3).ToString());	
				Assert.AreEqual("800.00",reader.GetValue(5).ToString());
				Assert.AreEqual("20",reader.GetValue(7).ToString());
				reader.Close();
				Console.WriteLine("Success...");
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
				
			}
		
		}
		
		[Test]
		public void testmultiple_statemant1()
		{
			string CreateTableQuery  = "create table test1 (a varchar);create table test2(a varchar);create table test3(a varchar)";
			EDBCommand createcommand = new EDBCommand();
			createcommand.CommandType = CommandType.Text;
			createcommand.CommandText = CreateTableQuery;
			createcommand.Connection = conn;
			createcommand.ExecuteNonQuery();
		}
		[Test]
		public void testmultiple_statemant2()
		{
			try
			{		
				string InsertTableQuery  = "insert into  test1 values('EnterpriseDB');insert into test2 values ('Islamabad');insert into test3 values('Pakistan');";				
				EDBCommand createcommand = new EDBCommand();
				createcommand.CommandType = CommandType.Text;
				createcommand.CommandText = InsertTableQuery;
				createcommand.Connection = conn;
				createcommand.ExecuteNonQuery();

				Console.WriteLine("Success...");
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
				
			}
		
		}
		[Test]
		public void testmultiple_statemant3()
		{
			try
			{
				string CreateTableQuery  = "drop table test1;drop table test2;drop table test3;";
				EDBCommand createcommand = new EDBCommand();
				createcommand.CommandType = CommandType.Text;
				createcommand.CommandText = CreateTableQuery;
				createcommand.Connection = conn;
				createcommand.ExecuteNonQuery();
				
				Console.WriteLine("Success..."); 
			}
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString()); 
				
			}
		}
	}
#pragma warning restore CS8602
}
