using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
	/// <summary>
	/// Summary description for NumericDataTypeTest.
	/// </summary>
	/// 
	[TestFixture]
    [NonParallelizable]
    public class EDBNumericDataTypeTest : EPASTestBase
    {
		EDBConnection? con = null;

        [SetUp]
        public void Init()
        {
            con = OpenConnection();
        }

        [TearDown] 
		public void Dispose()
		{
			
			TestUtil.dropTable(con,"NumericTAB");
			
			TestUtil.closeDB(con);
		}


		//////////////////////////////////////////////
		///

		[Test]

		public void testNumericDataValid_3_0() 
		 {
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(410)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");
		
		}
    
		[Test]
		public void testNumericDataNegative_Valid_3_0()
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-410)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");
		}

		[Test]
		public void testNumericDataInValid_3_0() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,0))";
			Command.ExecuteNonQuery();

			 
				
			try
			{
			
				Command.CommandText="insert into NumericTAB values(4101.6)";
				Command.ExecuteNonQuery();
			
				Assert.Fail("Expecting Numeric field overflow error");
			}

			catch(EDBException )
			{
			}
			
		}

		[Test]
		public void testNumericDataValid_3_2() 
		{
		
	
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,2))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(4.15)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test]
		public void testNumericDataNegative_Valid_3_2()
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,2))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-4.15)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

		}
		
		[Test]
		public void testNumericDataInValid_3_2() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,2))";
			Command.ExecuteNonQuery();

    	
			try
			{

				Command.CommandText="insert into NumericTAB values(24.15)";
				Command.ExecuteNonQuery();

				Assert.Fail("Expecting Numeric field overflow error");
			}
			catch(Exception )
			{
			}
    	
    		TestUtil.dropTable(con,"NumericTAB");
		}
   
		[Test]
		public void testNumericDataValid_3_3()
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,3))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(0.123)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");
 
	
		}
    
		[Test]
		public void testNumericDataNegative_Valid_3_3()
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,3))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-0.123)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");;
		}
  
		[Test]
		public void testNumericDataInValid_3_3()
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(3,3))";
			Command.ExecuteNonQuery();
			try
			{

				Command.CommandText="insert into NumericTAB values(6.157)";
				Command.ExecuteNonQuery();

			Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
			}
    	
    	
			TestUtil.dropTable(con,"NumericTAB");

		}
    
		[Test]
  		public void testNumericDataValid_4_4() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(4,4))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(0.4585)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");
			

		}

		[Test]
    	public void testNumericDataNegative_Valid_4_4()
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(4,4))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-0.4585)";
			Command.ExecuteNonQuery();
				
			TestUtil.dropTable(con,"NumericTAB");
		}
  
		
		[Test]
		public void testNumericDataInvalid_4_4() 
		{

  
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(4,4))";
			Command.ExecuteNonQuery();
			try
			{

			Command.CommandText="insert into NumericTAB values(9.4585)";
			Command.ExecuteNonQuery();

			Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
			}
    	
    	
			TestUtil.dropTable(con,"NumericTAB");

  

		}
  
		[Test]
		public void testNumericDataValid_8_0() 
		{

 
 
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(8,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(45856987)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");
	

		}
 
		[Test]
		public void testNumericDataNegative_Valid_8_0()
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(8,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-45856987)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");
		}


		[Test]
		public void testNumericDataInValid_8_0() 
		{


			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(8,0))";
			Command.ExecuteNonQuery();

    	
			try
			{
				Command.CommandText="insert into NumericTAB values(945856987.25)";
				Command.ExecuteNonQuery();

				Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
    		
			}
    	//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
    
		[Test]
		public void testNumericDataValid_8_7() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(8,7))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(1.56987)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

		}
   		
		//////////////////////////////////////////////
		///

		[Test]
		public void testNumericDataNegative_Valid_8_7() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(8,7))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-4.56987)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test] 
		public void testNumericDataInValid_8_7() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(8,7))";
			Command.ExecuteNonQuery();

    	
			try
			{

				Command.CommandText="insert into NumericTAB values(68458.56987)";
				Command.ExecuteNonQuery();
		

				Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
				    	
			}
			//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
		public void testNumericDataValid_18_0() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(18,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(123456789124563587)";
			Command.ExecuteNonQuery();
				
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
		public void testNumericDataNegative_Valid_18_0()
		{	

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(18,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-123456789124563587)";
			Command.ExecuteNonQuery();
				
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test]
		public void testNumericDataInValid_18_0() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(18,0))";
			Command.ExecuteNonQuery();

    	
			try
			{

				Command.CommandText="insert into NumericTAB values(12345678912456358756)";
				Command.ExecuteNonQuery();

				Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
			    	
			}
			//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test] 
		public void testNumericDataValid_18_7() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(18,7))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(12345678912.4563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test]
		public void testNumericDataNegative_Valid_18_7() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(18,7))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-12345678912.4563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test]
		public void testNumericDataInValid_18_7() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(18,7))";
			Command.ExecuteNonQuery();

			try
			{

				Command.CommandText="insert into NumericTAB values(912345678912.4563587)";
				Command.ExecuteNonQuery();
		

				Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
    	
			}
			//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test] 
		public void testNumericDataValid_19_0() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(9123456789124563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
		public void testNumericDataNegativeValid_19_0() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,0))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-9123456789124563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test] 	
		public void testNumericDataInValid_19_0() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,0))";
			Command.ExecuteNonQuery();
    	
			try
			{

				Command.CommandText="insert into NumericTAB values(19123456789124563587.25)";
				Command.ExecuteNonQuery();

				Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
    	
			}
			//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
		public void testNumericDataValid_19_9() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,9))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(9123456789.124563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

   

		}
		[Test]
		public void testNumericDataNegativeValid_19_9() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,9))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-9123456789.124563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test]
		public void testNumericDataInValid_19_9() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,9))";
			Command.ExecuteNonQuery();
    	
			try
			{
				Command.CommandText="insert into NumericTAB values(19123456789.124563587)";
				Command.ExecuteNonQuery();
		

				Assert.Fail("Expecting Numeric field overflow error");
			}
			catch(Exception )
			{
    	
			}
			
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
		public void testNumericDataValid_19_19() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,19))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(0.9123456789124563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}

		[Test]		
		public void testNumericDataNegative_Valid_19_19()
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,19))";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-0.9123456789124563587)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

		}

		[Test]
		public void testNumericDataInValid_19_19() 
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric(19,19))";
			Command.ExecuteNonQuery();

    	
			try
			{

				Command.CommandText="insert into NumericTAB values(59.9123456789124563587)";
				Command.ExecuteNonQuery();

				Assert.Fail("Expecting Numeric field overflow error");
			}
    	
			catch(Exception )
			{
    	
			}
			//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
		public void testNumericDataValid() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric)";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(1111111111122222222222222222222222222222222588888888888888888888888888888888888888888888888888888888888888666666666666666666666666666666666666666666666666666664444444444444444444444444444)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");


		}
		[Test]
		public void testNumericDataNegative_Valid() 
		{

			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric)";
			Command.ExecuteNonQuery();

			Command.CommandText="insert into NumericTAB values(-1111111111122222222222222222222222222222222588888888888888888888888888888888888888888888888888888888888888666666666666666666666666666666666666666666666666666664444444444444444444444444444)";
			Command.ExecuteNonQuery();
		
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test] 
		public void testNumericDataGreaterThan1000Digits() 
   
		{
			var Command=new EDBCommand("",con);
			Command.CommandText="CREATE TABLE NumericTAB(A Numeric)";
			Command.ExecuteNonQuery();

    	
    	
			try
			{


				Command.CommandText="insert into NumericTAB values(00123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890)";
				Command.ExecuteNonQuery();

			}
    	
			catch(Exception )
			{
				Assert.Fail("Expecting Numeric field overflow error");
			}
    
			//stmt.execute("DROP TABLE NumericTAB");
			TestUtil.dropTable(con,"NumericTAB");

		}
		[Test]
    
		public void testCreateTableWithZeroZero() 
    
		{
			var Command=new EDBCommand("",con);
    	
			try
			{
		
				Command.CommandText="CREATE TABLE NumericTAB(A Numeric(0,0))";
				Command.ExecuteNonQuery();
				Assert.Fail("Precision must be a non zreo non negative number");
				TestUtil.dropTable(con,"NumericTAB");


			}
    	
			catch(Exception )
			{
    	
			}
    	
    	
		}
    
		[Test] 
		public void testCreateTableWithNegativeZero()
    
		{
			var Command=new EDBCommand("",con);
    	
			try
			{
		
				Command.CommandText="CREATE TABLE NumericTAB(A Numeric(-1,0))";
				Command.ExecuteNonQuery();
				Assert.Fail("Precision must be a non zreo non negative number");
				TestUtil.dropTable(con,"NumericTAB");


			}
    	
			catch(Exception )
			{
    	
			}
    	
		}
		[Test]
		public void testCreateTableWithNegativeNegative() 
 
		{
			var Command=new EDBCommand("",con);
    	
			try
			{
		
				Command.CommandText="CREATE TABLE NumericTAB(A Numeric(-1,-1))";
				Command.ExecuteNonQuery();
				Assert.Fail("Precision must be a non zreo non negative number");
				TestUtil.dropTable(con,"NumericTAB");


			}
    	
			catch(Exception )
			{
    	
			}
		}


		[Test]
		public void testCreateTableWithZeroNegative() 
 
		{
			var Command=new EDBCommand("",con);
    	
			try
			{
		
				Command.CommandText="CREATE TABLE NumericTAB(A Numeric(0,-1))";
				Command.ExecuteNonQuery();
				Assert.Fail("Precision must be a non zreo non negative number");
				TestUtil.dropTable(con,"NumericTAB");


			}
    	
			catch(Exception )
			{
    	
			}
 	
 	
		}


	}
}
