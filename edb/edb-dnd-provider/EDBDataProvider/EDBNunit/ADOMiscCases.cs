using System;
using NUnit.Framework;
using System.Data;
using System.Globalization;
using EDBTypes;
using EnterpriseDB.EDBClient;
using System.Net;

namespace ADO
{
	/// <summary>
	/// Summary description for ADOConnectionTest.
	/// </summary>
	
	[TestFixture]
	public class ADOMiscCases
	{
		private ADOCOM.Connection Conn=null;
		private string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
		[SetUp]
		protected void SetUp()
		{ 
			Conn=new ADOCOM.Connection();
			Conn.Open(DBConnection,"edb","edb",-1); 
			object RecordsAffected=null;
			Conn.Execute("CREATE TABLE TableWithAllTypesWithSynonyms(c1 BIGINT,c2 INT8,c3 BIT,c4 BYTEA,c5 BINARY,c6 BLOB,c7 BYTE,c8 IMAGE,c9 LONG,c10 LONG RAW,c11 RAW(10),c12 VARBINARY,c13 CHAR(10),c14 CHARACTER(10),c15 DATE,c16 DOUBLE PRECISION,c17 FLOAT,c18 FLOAT(10),c19 INTEGER,c20 INT,c21 NUMERIC(10,2),c22 DEC(10,2),c23 DECIMAL(10,2),c24 MONEY,c25 NUMBER(10,2),c26 SMALLMONEY,c27 YEAR,c28 REAL,c30 SMALLFLOAT,c31 SMALLINT,c32 TINYINT,c33 TEXT,c34 CLOB,c35 LONG,c36 LONG VARCHAR,c37 LONGTEXT,c38 LVARCHAR,c39 MEDIUMTEXT,c40 TIMESTAMP,c41 TIMESTAMP(2),c42 DATETIME,c43 SMALLDATETIME,c44 VARCHAR(10),c45 CHAR VARYING(10),c46 CHARACTER VARYING(10),c47 TINYTEXT,c48 VARCHAR2(10));",out RecordsAffected,-1);
		
		}	

		[TearDown]
		protected void TearDown()
		{
			object RecordsAffected=null;
			Conn.Execute("DROP TABLE TableWithAllTypesWithSynonyms",out RecordsAffected,-1);
			Conn.Close();  			
		}

		
		[Test]
		public void ADOBigIntAboRan()
		{
			
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c1) VALUES(9223372036854775808);",out RecordsAffected,-1);
				Assert.Fail("value above range inserted");
			}

			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
			
		}

		[Test]
		public void ADOBigIntBelRan()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c1) VALUES(-9223372036854775809);",out RecordsAffected,-1);
				Assert.Fail("value below range inserted");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}


		[Test]
		public void ADOBIGINTOnMaxNegRange()
		{
			
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c1) VALUES(-9223372036854775808);",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				
			}

			catch(Exception exp)
			{
				
				Assert.Fail("Could not insert value within the BIGINT range");
			}
			
		}

		[Test]
		public void ADOBIGINTOnMaxPositiveRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c1) VALUES(9223372036854775807);",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
			}
			
			catch(Exception exp)
			{
				Assert.Fail("Could not insert value within the BIGINT range");
				
			}
		}

		[Test]
		public void ADOBigIntWrongValue()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c1) VALUES('asdasd');",out RecordsAffected,-1);
				Assert.Fail("Wrong value inserted into BigInt inserted");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADOBitWrongField()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c4) VALUES(falsee);",out RecordsAffected,-1);
				Assert.Fail("Wrong value inserted into Bit inserted");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADOBooleanWrongField()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c3) VALUES(truee);",out RecordsAffected,-1);
				Assert.Fail("Wrong value inserted into Boolean inserted");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADOCharacterAboveRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c14) VALUES('12345678900');",out RecordsAffected,-1);
				Assert.Fail("Value above range inserted into character");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADODateOnMinRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c15) VALUES('January 1,4713 BC');",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
			}
			
			catch(Exception exp)
			{
				Assert.Fail("Could not insert value within the Date range");
				
			}
		}

		[Test]
		public void ADODoublePrecisionAboveRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c16) VALUES(1E+309);",out RecordsAffected,-1);
				Assert.Fail("Value above range inserted into doube precision");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADODoublePrecisionBelowRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c16) VALUES(1E-308);",out RecordsAffected,-1);
				Assert.Fail("Value below range inserted into doube precision");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(0,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADODoublePrecisionOnMinAndMaxRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c16) VALUES(1E+308);",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c16) VALUES(1E-307)",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			
			catch(Exception exp)
			{
				Assert.Fail("Could not insert value within the double precision range");
				
			}
		}

		[Test]
		public void ADOFloatBelowRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c17) VALUES(1E-308);",out RecordsAffected,-1);
				Assert.Fail("Value below range inserted into float");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(0,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADODateAboveRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c15) VALUES('January 1,5874898 AD');",out RecordsAffected,-1);
				Assert.Fail("Value above range inserted into Date");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADODateBelowRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c15) VALUES('January 1,4714 BC');",out RecordsAffected,-1);
				Assert.Fail("Value below range inserted into Date");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADODateOnMaxRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c15) VALUES('January 1,9999 AD');",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
			}
			
			catch(Exception exp)
			{
				Assert.Fail("Value within the range not inserted into Date");
				
			}
		}

		[Test]
		public void ADOFloatAboveRange()
		{
			object RecordsAffected=null;
			try
			{
				Conn.Execute("INSERT INTO TableWithAllTypesWithSynonyms(c15) VALUES('January 1,4714 BC');",out RecordsAffected,-1);
				Assert.Fail("Value below range inserted into Date");
			}
			
			catch(Exception exp)
			{
				Assert.AreEqual(1,Conn.Errors.Count);
			}
		}

		[Test]
		public void ADOExecuteSingleTable()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE TABLE TAB1(A INT4);",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP TABLE TAB1;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteMultipleTable()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE TABLE TAB1(A INT4);CREATE TABLE TAB2(A INT4);",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP TABLE TAB1;DROP TABLE TAB2;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteSingleView()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP VIEW vista;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteMultipleViews()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;CREATE VIEW vistb AS SELECT text 'Hi Man' AS hi;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP VIEW vista;DROP VIEW vistb;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteSingleProcedure()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE PROCEDURE P1 AS \rBEGIN\rNULL;\rEND;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP PROCEDURE p1;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteMultipleProcedures()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE PROCEDURE P1 IS \rBEGIN\rNULL;\rEND;CREATE PROCEDURE P2 AS \rBEGIN\rNULL;\rEND;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute(" DROP PROCEDURE p1;DROP PROCEDURE p2;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteSingleSPLFunc()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE FUNCTION P1 RETURN VOID AS \rBEGIN\rNULL;\rEND;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP FUNCTION p1",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}

		[Test]
		public void ADOExecuteMultipleSPLFunc()
		{
			object RecordsAffected=null;

			try
			{
				Conn.Execute("CREATE FUNCTION P1 RETURN VOID AS \rBEGIN\rNULL;\rEND;CREATE FUNCTION P2 RETURN VOID AS \rBEGIN\rNULL;\rEND;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);
				Conn.Execute("DROP FUNCTION p1;DROP FUNCTION p2;",out RecordsAffected,-1);
				Assert.AreEqual(0,Conn.Errors.Count);

			}
			catch (Exception exp)
			{
				Assert.Fail(exp.Message);
			}
		}
	}
}
