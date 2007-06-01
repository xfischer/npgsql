using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;


namespace DOTNET
{
	/// <summary>
	/// Summary description for LoginTimeOutTest.
	/// </summary>
	/// 
	[TestFixture]
	public class LoginTimeOutTest
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{
							

		}

		[TearDown] 
		public void Dispose()
		{
						
		}

		[Test]
		public void testIntTimeout()  
		{
			try
			{
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=enterprisedb;Password=edb;Database=edb;Connect Timeout=45;"); 
				con.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
			
		[Test]
		public void testFloatTimeout()  
		{
			try
			{
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=enterprisedb;Password=edb;Database=edb;Connect Timeout=45.0;"); 
				con.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void testZeroTimeout()  
		{
			try
			{
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=enterprisedb;Password=edb;Database=edb;Connect Timeout=0;"); 
				con.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
		

		[Test]
		public void testNegativeTimeout()  
		{
			try
			{
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=enterprisedb;Password=edb;Database=edb;Connect Timeout=-1;"); 
				con.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void testBadTimeout()  
		{
			try
			{
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=enterprisedb;Password=edb;Database=edb;Connect Timeout=zzzz;"); 
				con.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
	}
}
