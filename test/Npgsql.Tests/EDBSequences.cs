using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8600
    /// <summary>
    /// Summary description for Sequences.
    /// </summary>
    /// 
    [TestFixture]
	public class EDBSequences : TestBase
    {
			EDBConnection? con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
			
						

		}

		[TearDown] 
		public void Dispose()
		{
			/*EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();*/
			TestUtil.closeDB(con);
		}

		/// <summary>
		/// creates a user definrd sequence with all default properties
		/// </summary>
		[Test]
		public void CreateSequenceSimple()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceSimple";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

            Command.CommandText = "select CreateSequenceSimple.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsNotNull(Reader.Read());
			//Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

            Command.CommandText = "DROP SEQUENCE CreateSequenceSimple";
			Command.ExecuteNonQuery();

		}
		/// <summary>
		/// createa user definedd sequence to test START WITH functionality
		/// </summary>
		[Test]
		public void CreateSequenceStartWith()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceStartWith START WITH 300";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

            Command.CommandText = "select CreateSequenceStartWith.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Reader.Read();
			Assert.AreEqual("300",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

            Command.CommandText = "DROP SEQUENCE CreateSequenceStartWith";
			Command.ExecuteNonQuery();

		}

		/// <summary>
		/// create a user defined sequence with a positive number as user defined increment value
		/// </summary>
		[Test]
		public void CreateSequencePositiveIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequencePositiveIncrementBy INCREMENT BY 2";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

            Command.CommandText = "select CreateSequencePositiveIncrementBy.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
            Reader.Close();
            Command.CommandText = "select CreateSequencePositiveIncrementBy.NextVal from dual";

			Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("3",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

            Command.CommandText = "DROP SEQUENCE CreateSequencePositiveIncrementBy";
			Command.ExecuteNonQuery();

		}

		/// <summary>
		/// create a user defined sequence with a negaitve number as user defined increment value
		/// </summary>
		[Test]
		public void CreateSequenceNegativeIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceNegativeIncrementBy INCREMENT BY -2";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

            Command.CommandText = "select CreateSequenceNegativeIncrementBy.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("-1",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
            Reader.Close();
            Command.CommandText = "select CreateSequenceNegativeIncrementBy.NextVal from dual";

			Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("-3",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

            Command.CommandText = "DROP SEQUENCE CreateSequenceNegativeIncrementBy";
				Command.ExecuteNonQuery();

		}


/// <summary>
/// create a user defined sequence with user defined MAXVALUE and START WITH parameters
/// </summary>
		[Test]
		public void CreateSequenceMaxValPositiveIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceMaxValPositiveIncrementBy MAXVALUE 2 START WITH 1";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

            Command.CommandText = "select CreateSequenceMaxValPositiveIncrementBy.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
            Reader.Close();
            Command.CommandText = "select CreateSequenceMaxValPositiveIncrementBy.NextVal from dual";
            try
            {
                Reader = Command.ExecuteReader();
                Assert.IsTrue(Reader.Read());
                Assert.AreEqual("2", Reader.GetValue(0).ToString());
                Console.WriteLine(Reader.GetValue(0).ToString());
                Reader.Close();
            }
            catch (EDBException )
            {
            }
			/*try
			{
				Command.CommandText="select TestSequence.NextVal from dual";
				Reader=Command.ExecuteReader();
				Assert.Fail("Error: Sequence reached its maximum value");

			}

			catch(EDBException exp)
			{
			}

			
			Reader.Close();*/

            Command.CommandText = "DROP SEQUENCE CreateSequenceMaxValPositiveIncrementBy";
				Command.ExecuteNonQuery();

		}
/// <summary>
	/// create a user defined sequence with user defined MAXVALUE and START WITH parameters and negative increment value
/// </summary>
		[Test]
		public void CreateSequenceMaxValNegativeIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceMaxValNegativeIncrementBy MAXVALUE 150 START WITH 150 INCREMENT BY -5";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

            Command.CommandText = "select CreateSequenceMaxValNegativeIncrementBy.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("150",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

            Reader.Close();
            Command.CommandText = "select CreateSequenceMaxValNegativeIncrementBy.NextVal from dual";

			Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("145",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			
			
			Reader.Close();

            Command.CommandText = "DROP SEQUENCE CreateSequenceMaxValNegativeIncrementBy";
				Command.ExecuteNonQuery();

		}
		
/// <summary>
/// create a user defined sequence with user defined MAXVALUE paremeter
/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxVal()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceStartWithMaxVal START WITH 1 MAXVALUE 5";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
			EDBDataReader Reader=null;
			for(int i=0;i<5;i++)
			{
                Command.CommandText = "select CreateSequenceStartWithMaxVal.NextVal from dual";

				Reader=Command.ExecuteReader();
                Reader.Close();
			
			}
			
			try
			{
                Command.CommandText = "select CreateSequenceStartWithMaxVal.NextVal from dual";

				Reader=Command.ExecuteReader();

                Reader.Close();
				Assert.Fail("expecting MaxVal reached error");
			}
			catch(EDBException )
			{
				//Reader.Close();
			
			}
			//Reader.Close();

            Command.CommandText = "DROP SEQUENCE CreateSequenceStartWithMaxVal";
				Command.ExecuteNonQuery();

		}

//		

/// <summary>
/// create a user defined sequence to test cycle functionality
/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxValCycle()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceStartWithMaxValCycle START WITH 1 MAXVALUE 5 CYCLE;";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException )
			{
                Command.CommandText = "DROP SEQUENCE CreateSequenceStartWithMaxValCycle";
				Command.ExecuteNonQuery();

				Assert.Fail("Error creating sequence");
			}
            Command.CommandText = "DROP SEQUENCE CreateSequenceStartWithMaxValCycle";
			Command.ExecuteNonQuery();

			
		}

/// <summary>
/// create a user defined sequence to test NOCYCLE functionality.case fails because EDB doesnot provide NOCYCLE functionality
/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxValNoCycle()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 MAXVALUE 5 NOCYCLE;";
			try
			{
				Command.ExecuteNonQuery();
				Assert.Fail(" NOCYCLE functionality.case should fails because EDB doesnot provide NOCYCLE functionality");
			}

			catch(EDBException )
			{
			}
	
		}

		/// <summary>
		/// create a user defined sequence to test CACHE and CYCLE functionality when used in combination
		/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxValCycleCache()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceStartWithMaxValCycleCache START WITH 1 MAXVALUE 5 CYCLE CACHE 4;";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException )
			{
                Command.CommandText = "DROP SEQUENCE CreateSequenceStartWithMaxValCycleCache";
				Command.ExecuteNonQuery();

				Assert.Fail("Error creating sequence");

			}

            Command.CommandText = "DROP SEQUENCE CreateSequenceStartWithMaxValCycleCache";
			Command.ExecuteNonQuery();

			
		}
/// <summary>
/// create a user defined sequence to test cache functionality
/// </summary>
		[Test]
		public void CreateSequenceCache()
		{
			EDBCommand Command=new EDBCommand("",con);
            Command.CommandText = "CREATE SEQUENCE CreateSequenceCache CACHE 100;";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException )
			{
                Command.CommandText = "DROP SEQUENCE CreateSequenceCache";
				Command.ExecuteNonQuery();

				Assert.Fail("Error creating sequence");
			}

            Command.CommandText = "DROP SEQUENCE CreateSequenceCache";
			Command.ExecuteNonQuery();

			
		}

	/// <summary>
	/// create a user defined sequence to test NOCACHE functionality.case fails because EDB doesnot provide NOCACHE functionality
	/// </summary>
		[Test]
		public void CreateSequenceNoCache()
		{
			/*EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence NOCACHE;";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
			Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();

				Assert.Fail("Error creating sequence");
			}
				Command.CommandText="DROP SEQUENCE TestSequence";
				Command.ExecuteNonQuery();
			*/
		}

		/// <summary>
		/// create user defined sequence to test ORDER functionality.case fails because EDB doesnot provide ORDER functionality
		/// </summary>
		[Test]
		public void CreateSequenceStartWithOrder()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 ORDER;";
			try
			{
				Command.ExecuteNonQuery();
				Assert.Fail("ORDER functionality.case should fails because EDB does not provide ORDER functionality");
			}

			catch(EDBException )
			{
			}
		
		}

		/// <summary>
		/// create user defined sequence to test NOORDER functionality.case fails because EDB doesnot provide NOORDER functionality
		/// </summary>
		[Test]
		public void CreateSequenceStartWithNoOrder()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 NOORDER;";
			try
			{
				Command.ExecuteNonQuery();
				Assert.Fail("NOORDER functionality.case should fails because EDB does not provide ORDER functionality");
			}

			catch(EDBException )
			{
			}
			
		}


	}
#pragma warning restore CS8600
}
