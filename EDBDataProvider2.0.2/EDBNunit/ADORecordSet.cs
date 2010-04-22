using System;
using NUnit.Framework;
using System.Data;
using System.Globalization;
using EDBTypes;
using EnterpriseDB.EDBClient;
using System.Net;
using System.IO;

namespace ADO
{
	/// <summary>
	/// Summary description for ADOConnectionTest.
	/// </summary>
	
	[TestFixture]
	public class ADORecordSet
	{
		private ADODB.Connection Conn=null;
		private string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
		[SetUp]
		protected void SetUp()
		{ 
			Conn=new ADODB.Connection();
			Conn.Open(DBConnection,"edb","edb",-1); 
		}	

		[TearDown]
		protected void TearDown()
		{
			Conn.Close();  			
		}

		[Test]
		public void ADORecordSetTestOne()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			Assert.AreEqual("SMITH",rs.Fields[0].Value);
			rs.Close();

		}

		[Test]
		public void ADORecordSetReferenceTest()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
		
			
			rs.Close();
			
		}
		[Test]
		public void ADORecordSetUpdateBatchOptimistic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source
			rs.Update(1,"New Job");
			
			
			rs.Close();
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);      
		
			Assert.AreEqual("CLERK",rs.Fields[1].Value.ToString());
			rs.Close();
		}

		[Test]
		public void ADORecordSetUpdateReadOnly()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockReadOnly,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source
			
			try
			{
				rs.Update(1,"New Job");
				Assert.Fail("Can't update a readonly recordset");
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				
			}
			
			
		}

		[Test]
		public void ADORecordSetUpdatePessimistic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			     
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source
			
			try
			{
				rs.Update(1,"New Job");
				rs.Close();

				
				rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
				rs.Update(1,"CLERK");
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				Console.WriteLine(exp.Message);
				Assert.Fail("Could not update");
				
			}
			
			
		}

		
		[Test]
		public void ADORecordSetTestOneDynamic()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			Assert.AreEqual("SMITH",rs.Fields[0].Value);
			rs.Close();

		}

		[Test]
		public void ADORecordSetReferenceTestDynamic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
		
			
			rs.Close();
			
		}

		

		[Test]
		public void ADORecordSetUpdateBatchOptimisticDynamic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source
			rs.Update(1,"New Job");
			
			
			rs.Close();
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);      
		
			Assert.AreEqual("CLERK",rs.Fields[1].Value.ToString());
			rs.Close();
		}

		[Test]
		public void ADORecordSetUpdateReadOnlyDynamic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockReadOnly,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source
			
			try
			{
				rs.Update(1,"New Job");
				Assert.Fail("Can't update a readonly recordset");
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				
			}
			
			
		}

		[Test]
		public void ADORecordSetUpdatePessimisticDynamic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockPessimistic,1);       
            

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source
			
			try
			{
				rs.Update(1,"New Job");
				rs.Close();

				rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockPessimistic,1);  
				rs.Update(1,"CLERK");
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				Console.WriteLine(exp.Message);
				Assert.Fail("Could not update");
				
			}
			
			
		}

		[Test]
		public void ADORecordSetUpdatePessimisticDynamic1()
		{
			// sql statment
			string SQL = "select ename,mgr from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockPessimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			//string JobByName=rs.Fields["job"].Value.ToString();
			//Assert.AreEqual(ENameByIndex,ENameByName);
			//Assert.AreEqual("CLERK",JobByIndex);
			//update should not update field in the data source

			Console.WriteLine(JobByIndex);
			
			try
			{
				//rs.Update(1,2222); //old one was 7902
				rs.Close();

				rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockPessimistic,1);  
				rs.Update(1,7902);
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				Console.WriteLine(exp.Message);
				Assert.Fail("Could not update");
				
			}
			
			
		}

		[Test]
		public void ADORecordsetScrollabilityROLock()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenForwardOnly,ADODB.LockTypeEnum.adLockReadOnly,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			try
			{
				rs.MoveLast();
				Assert.Fail("Forward only should not move back");
			}

			catch (Exception exp)
			{
				rs.Close();
			}



		}

		
		[Test]
		public void ADORecordsetScrollabilityUSLock()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenForwardOnly,ADODB.LockTypeEnum.adLockUnspecified,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			try
			{
				rs.MoveLast();
				Assert.Fail("Forward only should not move back");
			}

			catch (Exception exp)
			{
				rs.Close();
			}

		}

		[Test]
		public void ADORecordsetScrollabilityOPLock()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenForwardOnly,ADODB.LockTypeEnum.adLockOptimistic,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			try
			{
				rs.MoveLast();
				Assert.Fail("Forward only should not move back");
			}

			catch (Exception exp)
			{
				rs.Close();
			}

		}

		[Test]
		public void ADORecordSetClosedAccess()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			Assert.AreEqual("SMITH",rs.Fields[0].Value);
			rs.Close();

			try
			{
				Assert.AreEqual("SMITH",rs.Fields[0].Value);
				Assert.Fail("Operation is not allowed when the object is closed.");
			}

			catch(Exception exp)
			{
				;	
			}

		}

		[Test]
		public void ADORecordSetClosedAccessWithoutRecordAccess()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			Assert.AreEqual("SMITH",rs.Fields[0].Value);
			rs.Close();

			try
			{
				Assert.AreEqual("SMITH",rs.Fields[0].Value);
				Assert.Fail("Operation is not allowed when the object is closed.");
			}

			catch(Exception exp)
			{
				;	
			}

		}

		[Test]
		public void ADORecordSet_xSaveXML()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			
			rs._xSave("Recordset.xml",ADODB.PersistFormatEnum.adPersistXML);
			
			if(File.Exists("Recordset.xml"))
			{
				File.Delete("Recordset.xml");
				Console.WriteLine("File deleted");
			}
			else
			{
				Assert.Fail("Could not save recordset in File");
			}

			rs.Close();
		}

		[Test]
		public void ADORecordSetSaveXML()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			
			rs.Save("Recordset.xml",ADODB.PersistFormatEnum.adPersistXML);
	
			if(File.Exists("Recordset.xml"))
			{
				File.Delete("Recordset.xml");
				Console.WriteLine("File deleted");
			}
			else
			{
				Assert.Fail("Could not save recordset in File");
			}

			rs.Close();
		}


		
		[Test]
		public void ADORecordSet_xSaveADTG()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			
			rs._xSave("Recordset",ADODB.PersistFormatEnum.adPersistADTG);
			
			if(File.Exists("Recordset"))
			{
				File.Delete("Recordset");
				Console.WriteLine("File deleted");
			}
			else
			{
				Assert.Fail("Could not save recordset in File");
			}

			rs.Close();
		}

		[Test]
		public void ADORecordSetSaveADTG()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
		
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			
			rs.Save("Recordset",ADODB.PersistFormatEnum.adPersistADTG);
	
			if(File.Exists("Recordset"))
			{
				File.Delete("Recordset");
				Console.WriteLine("File deleted");
			}
			else
			{
				Assert.Fail("Could not save recordset in File");
			}

			rs.Close();
		}

		[Test]
		public void ADORecordsetBookmarkForward()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenForwardOnly,ADODB.LockTypeEnum.adLockOptimistic,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			try
			{
				int a =int.Parse(rs.Bookmark.ToString());

				Console.WriteLine(rs.Fields[0].Value.ToString());
				Console.WriteLine(rs.Fields[1].Value.ToString());

				rs.MoveNext();

				Console.WriteLine(rs.Bookmark.ToString());

				Assert.Fail("Current Recordset does not support Bookmarks");
			}

			catch(Exception exp)
			{
				rs.Close();
			}
		}



		[Test]
		public void ADORecordsetBookmarkStatic()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockOptimistic,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
				int a =int.Parse(rs.Bookmark.ToString());

				Console.WriteLine(rs.Fields[0].Value.ToString());
				Console.WriteLine(rs.Fields[1].Value.ToString());

				rs.MoveNext();

				Console.WriteLine(rs.Bookmark.ToString());
				rs.Close();
			
		}


		[Test]
		public void ADORecordsetBookmarkDynamic()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockOptimistic,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			try
			{
				int a =int.Parse(rs.Bookmark.ToString());

				Console.WriteLine(rs.Fields[0].Value.ToString());
				Console.WriteLine(rs.Fields[1].Value.ToString());

				rs.MoveNext();

				Console.WriteLine(rs.Bookmark.ToString());

				Assert.Fail("Current Recordset does not support Bookmarks");
			}

			catch(Exception exp)
			{
				rs.Close();
			}
		}


		[Test]
		public void ADORecordsetBookmarkKeySet()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenForwardOnly,ADODB.LockTypeEnum.adLockOptimistic,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			//specified cursor type doest support this 
			//	int a =int.Parse(rs.Bookmark.ToString());

				Console.WriteLine(rs.Fields[0].Value.ToString());
				Console.WriteLine(rs.Fields[1].Value.ToString());

				rs.MoveNext();
			//  specified cursor type doest support this 
			//	Console.WriteLine(rs.Bookmark.ToString());
				rs.Close();
		}



		[Test]
		public void ADORecordsetBookmarkUnspecified()
		{
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenUnspecified,ADODB.LockTypeEnum.adLockOptimistic,1);

			Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			Assert.AreEqual("7499",rs.Fields[0].Value.ToString());

			rs.MoveNext();
			
			try
			{
				int a =int.Parse(rs.Bookmark.ToString());

				Console.WriteLine(rs.Fields[0].Value.ToString());
				Console.WriteLine(rs.Fields[1].Value.ToString());

				rs.MoveNext();
			
				Console.WriteLine(rs.Bookmark.ToString());

				Assert.Fail("Current Recordset does not support Bookmarks");
			}

			catch(Exception exp)
			{
				rs.Close();
			}
		}

		[Test]
		public void ADORecordSetCloning()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			//Assert.AreEqual("CLERK",JobByIndex);
		
			ADODB.Recordset rs2= rs.Clone(ADODB.LockTypeEnum.adLockBatchOptimistic);
			
			 ENameByIndex=rs2.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			 ENameByName=rs2.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex2=rs2.Fields[1].Value.ToString();
			//reference the job field by column name
			 JobByName=rs2.Fields["job"].Value.ToString();
		//	Assert.AreEqual(ENameByIndex,ENameByName);
		//	Assert.AreEqual("CLERK",JobByIndex2);
			
			rs2.Close();
			
			rs.Close();
			
		}

		[Test]
		public void ADORecordSetWrongCloning()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			//Assert.AreEqual("CLERK",JobByIndex);
		
			ADODB.Recordset rs2=null;
			try
			{

				 rs2= rs.Clone(ADODB.LockTypeEnum.adLockOptimistic);
				Assert.Fail("Arguments are of the wrong type, are out of acceptable range, or are in conflict with one another");
			}

			catch(Exception exp)
			{
				
				
				rs.Close();
			}
			
			
			
			
		}

		[Test]
		public void ADORecordSet_xCloning()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			//Assert.AreEqual("CLERK",JobByIndex);
		
			ADODB.Recordset rs2= rs._xClone();
			ENameByIndex=rs2.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			ENameByName=rs2.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			JobByIndex=rs2.Fields[1].Value.ToString();
			//reference the job field by column name
			JobByName=rs2.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			//Assert.AreEqual("CLERK",JobByIndex);
			
			rs2.Close();
			
			rs.Close();
			
		}

		[Test]
		public void ADORecordsetBookmarkDynamic1()
		{
			
			

			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockUnspecified,1);

			
			
			try
			{
			

				rs.MoveNext();

				Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

				rs.MoveNext();

				Assert.AreEqual("7499",rs.Fields[0].Value.ToString());
			}

			catch(Exception exp)
			{
				;
			}
			
		
		}

		[Test]
		public void ADORecordsetBookmarkDynamic2()
		{
			
			string SQL="SELECT * FROM EMP order by empno;";
			ADODB.Recordset rs=new ADODB.Recordset();

			rs.Open(SQL,Conn,ADODB.CursorTypeEnum.adOpenDynamic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);

			
			
			try
			{
			

				rs.MoveNext();

				Assert.AreEqual("7369",rs.Fields[0].Value.ToString());

				rs.MoveNext();

				Assert.AreEqual("7499",rs.Fields[0].Value.ToString());
			}

			catch(Exception exp)
			{
				;
			}
			
			
		}


	}
}
