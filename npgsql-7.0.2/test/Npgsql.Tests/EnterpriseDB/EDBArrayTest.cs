using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections;
using NUnit;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    /// <summary>
    /// Summary description for EDBArrayTest.
    /// </summary>

    [TestFixture]
	public class EDBArrayTest : TestBase
    {
		EDBConnection? con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();

            TestUtil.EnsureEDBAdvancedServer(con);

		}

		[TearDown] 
		public void Dispose()
		{
			TestUtil.closeDB(con);
		}

		// Following cases verify Arrays w.r.t various datatypes

		[Test]
		public void ArraysInt2()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestInt2 (i int2[10],j int2[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestInt2 VALUES ('{0,1,2,3,4,5,6,7,8,9}','{40,50,60,70,81,90,32765}');";
			Command.ExecuteNonQuery();
            int[] a = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] b = { 40, 50, 60, 70, 81, 90, 32765 };
			Command.CommandText= "SELECT * FROM arrtestInt2;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(short[])Reader.GetValue(0));
            Assert.AreEqual(b, (short[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestInt2";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysInt4()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestInt4 (i int4[10],j int4[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestInt4 VALUES ('{0,1,2,3,4,5,6,7,8,9}','{-2147483648,100,433,544,2147483647}');";
			Command.ExecuteNonQuery();

            int[] a = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] b = { -2147483648, 100, 433, 544, 2147483647 };

			Command.CommandText= "SELECT * FROM arrtestInt4;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(int[])Reader.GetValue(0));
			Assert.AreEqual(b,(int[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestInt4";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysInt8()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestInt8 (i int8[10],j int8[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestInt8 VALUES ('{1000,2000,3000,4000,50000,6000,7000,8000,9000,10000}','{65454545,32769}');";
			Command.ExecuteNonQuery();

            long[] a = { 1000, 2000, 3000, 4000, 50000, 6000, 7000, 8000, 9000, 10000 };
            long[] b = { 65454545, 32769 };

			Command.CommandText= "SELECT * FROM arrtestInt8;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(long[])Reader.GetValue(0));
            Assert.AreEqual(b, (long[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestInt8";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysFloat()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestFloat (f1 Float[10],f2 Float[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestFloat VALUES ('{2.0,4.21,6.32,3.98,4.00,5.91,6.00,7.66,8.88,9.99}','{43534.234,5534.463}');";
			Command.ExecuteNonQuery();

            double[] a = { 2, 4.21, 6.32, 3.98, 4, 5.91, 6, 7.66, 8.88, 9.99 };
            double[] b = { 43534.234, 5534.463 };
			Command.CommandText= "SELECT * FROM arrtestFloat;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(double[] )Reader.GetValue(0));
            Assert.AreEqual(b, (double[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestFloat";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysFloat4()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestFloat4 (f1 Float4[10],f2 Float4[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestFloat4 VALUES ('{65.2,23.1,56.42,334.5,46.3,532.33,69.64,75.234,8.75,92.1}','{2132.32,987.145}');";
			Command.ExecuteNonQuery();
            float[] a = { 65.2F, 23.1F, 56.42F, 334.5F, 46.3F, 532.33F, 69.64F, 75.234F, 8.75F, 92.1F };
            float[] b = { 2132.32F, 987.145F};

			Command.CommandText= "SELECT * FROM arrtestFloat4;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(float[])Reader.GetValue(0));
            Assert.AreEqual(b, (float[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestFloat4";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysFloat8()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestFloat8 (f1 Float8[10],f2 Float8[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestFloat8 VALUES ('{122.33,230.32,1342.24,28766.33,343245.234,462.33,575.323,6787.433,7004.344,865.345,983.433}','{8555.233,654.9785}');";
			Command.ExecuteNonQuery();

            double[] a = { 122.33, 230.32, 1342.24, 28766.33, 343245.234, 462.33, 575.323, 6787.433, 7004.344, 865.345, 983.433 };
            double[] b = { 8555.233, 654.9785 };
			Command.CommandText= "SELECT * FROM arrtestFloat8;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(double[])Reader.GetValue(0));
			Assert.AreEqual(b,(double[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestFloat8";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysReal()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest1 (r1 Real[10],r2 Real[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest1 VALUES ('{12.3233,13.223,265.323,30.001,4235.9,543.454,543.453,775.235,800.992,9122.12}');";
			Command.ExecuteNonQuery();

            float[] a = { 12.3233F, 13.223F, 265.323F, 30.001F, 4235.9F, 543.454F, 543.453F, 775.235F, 800.992F, 9122.12F };

			Command.CommandText="SELECT * FROM arrtest1;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(float[])Reader.GetValue(0));
			//Assert.AreEqual("{8555.233,654.9785}",Reader.GetValue(1));*/
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest1";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysNumeric()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysNumeric (n1 Numeric[10],n2 numeric[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysNumeric VALUES ('{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}','{132.654,897.2563}');";
			Command.ExecuteNonQuery();

            decimal[] a = { 120.89809M, 1234.00090M, 2.2434M, 3123.0M, 42342.22M, 53552.2M, 652.233M, 7.09M, 8.11M, 9.654M };
            decimal[] b = { 132.654M, 897.2563M };
            Command.CommandText = "SELECT * FROM ArraysNumeric;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysNumeric";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysNumericWithPrecision()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysNumericWithPrecision (n1 Numeric(5,2)[10],n2 Numeric(4,3)[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysNumericWithPrecision VALUES ('{120.89,123.90,22.334,412.40,422.22,552.21,62.22,712.09,18.11,91.65}','{1.234,2.142}');";
			Command.ExecuteNonQuery();

            decimal[] a = { 120.89M, 123.90M, 22.33M, 412.40M, 422.22M, 552.21M, 62.22M, 712.09M, 18.11M, 91.65M };
            decimal[] b = { 1.234M, 2.142M };

            Command.CommandText = "SELECT * FROM ArraysNumericWithPrecision;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysNumericWithPrecision";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysSmallInt()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysSmallInt (i smallint[10],j smallint[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysSmallInt VALUES ('{-1,-2,-3,-4,0,4,5,6,7,8}','{40,50,60,70,81,90,32765}');";
			Command.ExecuteNonQuery();

            int[] a = { -1, -2, -3, -4, 0, 4, 5, 6, 7, 8 };
            int[] b = { 40, 50, 60, 70, 81, 90, 32765 };
            Command.CommandText = "SELECT * FROM ArraysSmallInt;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(short[])Reader.GetValue(0));
            Assert.AreEqual(b, (short[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysSmallInt";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBigInt()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBigInt (i bigint[10],j bigint[]);";
			Command.ExecuteNonQuery();

			long[] a ={-100,-200,-300,-4000,-922337203685477,50000,6000,7000,8000,9000};
            long[] b ={ -9223372036854775808, 9223372036854775807};

            Command.CommandText = "INSERT INTO ArraysBigInt VALUES ('{-100,-200,-300,-4000,-922337203685477,50000,6000,7000,8000,9000}','{-9223372036854775808,9223372036854775807}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBigInt;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a, (long[])Reader.GetValue(0));
            Assert.AreEqual(b, (long[])Reader.GetValue(1));
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBigInt";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDoublePrecision()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysDoublePrecision (d1 double precision[3],d2 double precision[]);";
			Command.ExecuteNonQuery();

            double[] a = { 122.323423453, 230.32131231322, 123342.2323324 };
            double[] b = { 555.43534543233, 344654.34534439782 };

            Command.CommandText = "INSERT INTO ArraysDoublePrecision VALUES ('{122.323423453,230.32131231322,123342.2323324}','{555.43534543233,344654.34534439785}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysDoublePrecision;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a, (double[])Reader.GetValue(0));
            Assert.AreEqual(b, (double[])Reader.GetValue(1));
			//Assert.AreEqual("{122.323423453,230.32131231322,123342.2323324}",Reader.GetValue(0));
			//Assert.AreEqual("{555.43534543233,344654.345344398}",Reader.GetValue(1));

			Reader.Close();

            Command.CommandText = "DROP TABLE ArraysDoublePrecision";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysInteger()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysInteger (i integer[],j integer[2]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysInteger VALUES ('{-2147483648,2147483647}','{5,9}');";
			Command.ExecuteNonQuery();

            int[] a = { -2147483648, 2147483647 };
            int[] b = { 5, 9 };

            Command.CommandText = "SELECT * FROM ArraysInteger;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(int[])Reader.GetValue(0));
            Assert.AreEqual(b, (int[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysInteger";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysNumber()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtestNumber (n1 Number[5],n2 Number[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO arrtestNumber VALUES ('{321.255,654.233,8987,545.23,654.36}','{31.2434,23.1442}');";
			Command.ExecuteNonQuery();
            decimal[] a = { 321.255M, 654.233M, 8987M, 545.23M, 654.36M };
            decimal[] b = { 31.2434M, 23.1442M };
            Command.CommandText = "SELECT * FROM arrtestNumber;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE arrtestNumber";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDecimal()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysDecimal (n1 decimal(5,2)[10],n2 decimal(4,3)[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysDecimal VALUES ('{120.89,123.90,22.334,412.40,422.22,552.21,62.22,712.09,18.11,91.65}','{1.234,2.142}');";
			Command.ExecuteNonQuery();

            decimal[] a = { 120.89M, 123.90M, 22.33M, 412.40M, 422.22M, 552.21M, 62.22M, 712.09M, 18.11M, 91.65M };
            decimal[] b = { 1.234M, 2.142M };
            Command.CommandText = "SELECT * FROM ArraysDecimal;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysDecimal";
			Command.ExecuteNonQuery();

		}

		[Test, Ignore("Difference in decimal point value only")]
		public void ArraysMoney()
		{
            TestUtil.dropTable(con, "arrtestMoney");

			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestMoney (m1 money[],m2 money[2]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestMoney VALUES ('{-21474823123326.4128,2123432474836.247}','{2343245.571,523432.3226}');";
			Command.ExecuteNonQuery();

            decimal[] a = { new decimal((double)-21474823123326.4128), new decimal((double)2123432474836.247) };
           
            Command.CommandText= "SELECT * FROM arrtestMoney;";
			EDBDataReader Reader = Command.ExecuteReader();
			object[] test={"-$21,474,823,123,326.41","$2,123,432,474,836.25"};
			Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a, Reader.GetValue(0));
			Assert.AreEqual("{\""+test[0].ToString()+"\",\""+test[1].ToString()+"\"}",Reader.GetValue(0).ToString());
			string[] teststr={"$2,343,245.57","$523,432.32"};
			Assert.AreEqual("{\""+teststr[0]+"\",\""+teststr[1]+"\"}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestMoney";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysSmallMoney()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysSmallMoney (m1 smallmoney[],m2 smallmoney[2]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysSmallMoney VALUES ('{-474836.4128,74836.2417}','{45.1157,15.2636}');";
			Command.ExecuteNonQuery();

            decimal[] a = { -474836.4128M, 74836.2417M };
            decimal[] b = { 45.1157M, 15.2636M };
            Command.CommandText = "SELECT * FROM ArraysSmallMoney;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysSmallMoney";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysText()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books text[]);;";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO books VALUES ('{ Lord of the Rings , Suffocles}');";
			Command.ExecuteNonQuery();

            string[] a = {"Lord of the Rings","Suffocles"};

			Command.CommandText="SELECT * FROM  books;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
//			Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysVarchar()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE favourite_books( books Varchar[3]);";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO favourite_books VALUES ('{The Hitchhikers Guide to the Galaxy,Harry Potter,Kitten, Squared}');";
			Command.ExecuteNonQuery();

            string[] a = {"The Hitchhikers Guide to the Galaxy","Harry Potter","Kitten","Squared"};

			Command.CommandText="SELECT * FROM  favourite_books;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
					//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  favourite_books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTinyText()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books tinytext[]);";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO books VALUES ('{Lord of the Rings,Suffocles}');";
			Command.ExecuteNonQuery();
			
            string[] a = {"Lord of the Rings","Suffocles"};

			Command.CommandText="SELECT * FROM  books;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
			//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysVarchar2()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE favourite_books( books Varchar2[3]);";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO favourite_books VALUES ('{The Hitchikers Guide to the Galaxy,Harry Potter,Kitten, Squared}');";
			Command.ExecuteNonQuery();

            string[] a = {"The Hitchikers Guide to the Galaxy","Harry Potter","Kitten","Squared"};
			
			Command.CommandText="SELECT * FROM  favourite_books;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
			//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  favourite_books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysCharacter()
		{
            string[] a = {"1st char  ","sec char  " };
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE chartest( ch character(10)[]);";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO chartest VALUES ('{1st char,sec char}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM chartest;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE chartest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysChar()
		{
            string[] a = { "1st char","sec char" };
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE chartest( ch char(8)[]);";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO chartest VALUES ('{1st char,sec char}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM chartest;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE chartest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysLong()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books long[]);";
			Command.ExecuteNonQuery();

            string[] a = { "Lord of the War", "Suffocles" };

			Command.CommandText="INSERT INTO books VALUES ('{Lord of the War,Suffocles}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM books;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysLongText()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books longtext[]);";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO books VALUES ('{Lord of the War,Suffocles,A walk in the cloudsssss }');";
			Command.ExecuteNonQuery();

            string[] a = { "Lord of the War", "Suffocles", "A walk in the cloudsssss" };

			Command.CommandText="SELECT * FROM books;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(string[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE books";
			Command.ExecuteNonQuery();

		}

        //ZK CHECKME: Date[] to DateTime[] cast not supported in EDB
		[Test]
		public void ArraysDate()
		{
			
			var Command = new EDBCommand("",con);
				
			Command.CommandText= "CREATE TABLE arrtestDate (d1 Date[]);";
			Command.ExecuteNonQuery();

			Command.CommandText= "INSERT INTO arrtestDate VALUES ('{040506,101203}');";
			Command.ExecuteNonQuery();

            DateTime[] a = { Convert.ToDateTime("2004-05-06 00:00:00"), Convert.ToDateTime("2010-12-03 00:00:00") };

			Command.CommandText= "SELECT * FROM arrtestDate;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			//Assert.AreEqual(a,(D)Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText= "DROP TABLE arrtestDate;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTimestamp()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysTimestamp (t Timestamp[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysTimestamp VALUES ('{1999-01-08 04:05:06,December 11 04:05:06 2006}');";
			Command.ExecuteNonQuery();

            DateTime[] a = { DateTime.Parse("1999-01-08 04:05:06"), DateTime.Parse("2006-12-11 04:05:06") };

            Command.CommandText = "SELECT * FROM ArraysTimestamp;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
		//	Assert.AreEqual(a,(DateTime[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysTimestamp;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDateTime()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysDateTime (t DATETIME[]);";
			Command.ExecuteNonQuery();

            DateTime[] a = {Convert.ToDateTime("1999-01-08 04:05:06"),Convert.ToDateTime("2006-12-11 04:05:06")};
            Command.CommandText = "INSERT INTO ArraysDateTime VALUES ('{1999-01-08 04:05:06,December 11 04:05:06 2006}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysDateTime;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
		//	Assert.AreEqual(a,(DateTime[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysDateTime;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTime()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysTime (t TIME[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysTime VALUES ('{04:05:06,12:10:48 }');";
			Command.ExecuteNonQuery();

            DateTime[] a = {DateTime.Parse("04:05:06"),DateTime.Parse("12:10:48")};

            Command.CommandText = "SELECT * FROM ArraysTime;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
          //  DateTime[] data = (DateTime[])Reader.GetValue(0);

		//	Assert.AreEqual(a[0].ToShortTimeString(),data[0].ToShortTimeString());
          //  Assert.AreEqual(a[1].ToShortTimeString(), data[1].ToShortTimeString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysTime;";
			Command.ExecuteNonQuery();

		}

		public static string BitStreamToString(IEnumerable myList, int myWidth)
		{
			var sw = new System.IO.StringWriter();

			var i = myWidth;
			foreach (var obj in myList)
			{
				if (i <= 0)
				{
					i = myWidth;
					sw.WriteLine();
				}
				i--;
				sw.Write("{0,8}", obj);
			}
			sw.WriteLine();
			return sw.ToString();
		}

		public static string MakeDebugMessage(BitArray expected, BitArray actual)
		{

			return "Expected:\n" + BitStreamToString((IEnumerable)expected, 8) + "Actual:\n" + BitStreamToString((IEnumerable)actual, 8);

		}

		[Test]
		public void ArraysBoolean()
		{
            TestUtil.dropTable(con, "ArraysBoolean");
            var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBoolean (t boolean[]);";
			Command.ExecuteNonQuery();

            bool[] a =  { true, false, true, true, true, false, false, false, false, false, false, false };

            Command.CommandText = "INSERT INTO ArraysBoolean VALUES ('{t,f,t,t,t,f,f,f,f,f,f,f }');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBoolean;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(bool[])Reader.GetValue(0), MakeDebugMessage(new BitArray(a), new BitArray((bool[])Reader.GetValue(0))));

			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBoolean;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBoolTrueFalse()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBool (t bool[]);";
			Command.ExecuteNonQuery();

            bool[] a = { true, false, false, false, true, false, false, true };

			Command.CommandText = "INSERT INTO ArraysBool VALUES ('{true,false,false,false,true,false,false,true }');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBool;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(bool[])Reader.GetValue(0), MakeDebugMessage(new BitArray(a), new BitArray((bool[])Reader.GetValue(0))));

			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBool;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBoolOneZero()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBool2 (t bool[]);";
			Command.ExecuteNonQuery();

            bool[] a = { false, true, true, false, false, false, true, true, true, true, true, false };

            Command.CommandText = "INSERT INTO ArraysBool2 VALUES ('{0,1,1,0,0,0,1,1,1,1,1,0}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBool2;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(bool[])Reader.GetValue(0), MakeDebugMessage(new BitArray(a), new BitArray((bool[])Reader.GetValue(0))));
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBool2;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTimestampWithoutTimeZone()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysTimestampWithoutTimeZone (t Timestamp[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysTimestampWithoutTimeZone VALUES ('{1999-01-08 04:05:06 -8:00,2005-11-08 12:02:06 -8:00,February 10 00:04:50 2004 PST}');";
			Command.ExecuteNonQuery();

            DateTime[] a = { DateTime.Parse("1999-01-08 04:05:06"), DateTime.Parse("2005-11-08 12:02:06"), DateTime.Parse("2004-02-10 00:04:50") };

            Command.CommandText = "SELECT * FROM ArraysTimestampWithoutTimeZone;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
		//	Assert.AreEqual(a,(DateTime[])Reader.GetValue(0));
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysTimestampWithoutTimeZone;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysInterval()
		{
            TestUtil.dropTable(con, "ArraysInterval");
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysInterval (t interval[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysInterval VALUES ('{1 12:59:10,2 01:23:34}');";
			Command.ExecuteNonQuery();
            //EDBTypes.EDBTimeSpan[] a = { EDBTypes.EDBTimeSpan.Parse("1 day 12:59:10"), EDBTypes.EDBTimeSpan.Parse("2 days 01:23:34") };
            TimeSpan[] a = { new TimeSpan(1, 12, 59, 10), new TimeSpan(2, 1, 23, 34) };

            Command.CommandText = "SELECT * FROM ArraysInterval;";
			EDBDataReader Reader = Command.ExecuteReader();

            Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a,(TimeSpan[])Reader.GetValue(0));
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysInterval;";
			Command.ExecuteNonQuery();

		}

        
		[Test]
		public void ArraysInterval2()
		{
            TestUtil.dropTable(con, "ArraysInterval2");
            var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysInterval2 (t interval[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysInterval2 VALUES ('{-23:00:00,2 01:23:34,1 day -01:00:00,21 days}');";
			Command.ExecuteNonQuery();

            /*
            EDBTypes.EDBTimeSpan[] a = { EDBTypes.EDBTimeSpan.Parse("-23:00:00"),
                EDBTypes.EDBTimeSpan.Parse("2 days 01:23:34"),
                                         EDBTypes.EDBTimeSpan.Parse("1 day -01:00:00"),
                EDBTypes.EDBTimeSpan.Parse("21 days")};
            */

            TimeSpan[] a = { new TimeSpan(-23, 0, 0), new TimeSpan(2, 1, 23, 34)
            , new TimeSpan(1, -1, 0, 0), new TimeSpan(21, 0, 0, 0)};
            Command.CommandText = "SELECT * FROM ArraysInterval2;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a, (TimeSpan[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysInterval2;";
			Command.ExecuteNonQuery();

		}
        
		[Test]
		public void ArraySelect()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraySelect (a int2[],b int, c name[],e float8[],f char(5)[],g varchar(5)[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraySelect (a,b, c, e, f, g) " +
  			 " VALUES ('{100,200,300,400,500}', 101, '{}',  '{}', '{}', '{}');	";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraySelect (a, b, c, e, f, g) VALUES ('{11,12,23}',103, '{ foobar}', " +
				" '{ 3.4,  6.7}', '{abc,abcde}', '{xyz,xyzz}');";
			Command.ExecuteNonQuery();

            int[] a = { 100, 200, 300, 400, 500 };
            int[] c = {  };

            Command.CommandText = "SELECT  * FROM ArraySelect where b = 101;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(short[])Reader.GetValue(0));
            Assert.AreEqual(101, (int)Reader.GetValue(1));
						
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraySelect;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArrayUpdate()
		{
			
			var Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArrayUpdate (a int2[],b int, c name[],e float8[],f char(5)[],g varchar(5)[]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArrayUpdate (a,b, c, e, f, g) " +
				" VALUES ('{100,200,300,400,500}', 101, '{}',  '{}', '{}', '{}');	";
			Command.ExecuteNonQuery();

            Command.CommandText = "UPDATE ArrayUpdate SET e[0] = '1.10'";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArrayUpdate (a, b, c, e, f, g) VALUES ('{11,12,23}',103, '{ foobar}', " +
				" '{ 3.4,  6.7}', '{abc,abcde}', '{xyz,xyzz}');";
			Command.ExecuteNonQuery();

            int[] a = { 100, 200, 300, 400, 500 };
            Command.CommandText = "SELECT a, e[0] ,e[1]  FROM ArrayUpdate where a[2] = 200;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(short[])Reader.GetValue(0));
			Assert.AreEqual("1.1",Reader.GetValue(1).ToString());
			Assert.AreEqual("",Reader.GetValue(2).ToString());
//			//Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();
            Command.CommandText = "DROP TABLE ArrayUpdate;";
			Command.ExecuteNonQuery();

		}

    }
}
