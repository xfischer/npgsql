using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
	/// <summary>
	/// Summary description for LoginTimeOutTest.
	/// </summary>
	/// 
	[TestFixture]
	public class EDBLoginTimeOutTest
    {
		EDBConnection? con = null;

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
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=edb;Password=edb;Database=edb;Timeout=45;"); 
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
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=edb;Password=edb;Database=edb;Timeout=45;"); 
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
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=edb;Password=edb;Database=edb;Timeout=0;"); 
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
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=edb;Password=edb;Database=edb;Timeout=1;"); 
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
				con=new EDBConnection("Server=127.0.0.1;Port=5444;UserId=edb;Password=edb;Database=edb;Timeout=10;"); 
				con.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
	}
}
