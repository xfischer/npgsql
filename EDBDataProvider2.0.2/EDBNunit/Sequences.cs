using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

namespace DOTNET
{
	/// <summary>
	/// Summary description for Sequences.
	/// </summary>
	/// 
	[TestFixture]
	public class Sequences
	{
			EDBConnection con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = TestUtil.openDB();
			
						

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
			Command.CommandText="CREATE SEQUENCE TestSequence";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Command.CommandText="select TestSequence.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsNotNull(Reader.Read());
			//Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

			Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();

		}
		/// <summary>
		/// createa user definedd sequence to test START WITH functionality
		/// </summary>
		[Test]
		public void CreateSequenceStartWith()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 300";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Command.CommandText="select TestSequence.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Reader.Read();
			Assert.AreEqual("300",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

			Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();

		}

		/// <summary>
		/// create a user defined sequence with a positive number as user defined increment value
		/// </summary>
		[Test]
		public void CreateSequencePositiveIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence INCREMENT BY 2";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Command.CommandText="select TestSequence.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText="select TestSequence.NextVal from dual";

			Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("3",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

			Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();

		}

		/// <summary>
		/// create a user defined sequence with a negaitve number as user defined increment value
		/// </summary>
		[Test]
		public void CreateSequenceNegativeIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence INCREMENT BY -2";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Command.CommandText="select TestSequence.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("-1",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText="select TestSequence.NextVal from dual";

			Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("-3",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			Reader.Close();

				Command.CommandText="DROP SEQUENCE TestSequence";
				Command.ExecuteNonQuery();

		}


/// <summary>
/// create a user defined sequence with user defined MAXVALUE and START WITH parameters
/// </summary>
		[Test]
		public void CreateSequenceMaxValPositiveIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence MAXVALUE 2 START WITH 1";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Command.CommandText="select TestSequence.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText="select TestSequence.NextVal from dual";
            try
            {
                Reader = Command.ExecuteReader();
                Assert.IsTrue(Reader.Read());
                Assert.AreEqual("2", Reader.GetValue(0).ToString());
                Console.WriteLine(Reader.GetValue(0).ToString());
                Reader.Close();
            }
            catch (EDBException ex)
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

				Command.CommandText="DROP SEQUENCE TestSequence";
				Command.ExecuteNonQuery();

		}
/// <summary>
	/// create a user defined sequence with user defined MAXVALUE and START WITH parameters and negative increment value
/// </summary>
		[Test]
		public void CreateSequenceMaxValNegativeIncrementBy()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence MAXVALUE 150 START WITH 150 INCREMENT BY -5";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Command.CommandText="select TestSequence.NextVal from dual";

			EDBDataReader Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("150",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

            Reader.Close();
			Command.CommandText="select TestSequence.NextVal from dual";

			Reader=Command.ExecuteReader();
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("145",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());

			
			
			Reader.Close();

				Command.CommandText="DROP SEQUENCE TestSequence";
				Command.ExecuteNonQuery();

		}
		
/// <summary>
/// create a user defined sequence with user defined MAXVALUE paremeter
/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxVal()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 MAXVALUE 5";
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
				Command.CommandText="select TestSequence.NextVal from dual";

				Reader=Command.ExecuteReader();
                Reader.Close();
			
			}
			
			try
			{
				Command.CommandText="select TestSequence.NextVal from dual";

				Reader=Command.ExecuteReader();

                Reader.Close();
				Assert.Fail("expecting MaxVal reached error");
			}
			catch(EDBException exp)
			{
				//Reader.Close();
			
			}
			//Reader.Close();
			
				Command.CommandText="DROP SEQUENCE TestSequence";
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
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 MAXVALUE 5 CYCLE;";
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

			
		}

/// <summary>
/// create a user defined sequence to test NOCYCLE functionality.case fails because EDB doesnot provide NOCYCLE functionality
/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxValNoCycle()
		{
			/*EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 MAXVALUE 5 NOCYCLE;";
			try
			{
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				Assert.Fail("Error creating sequence");
				Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();

			}
			
			Command.CommandText="DROP SEQUENCE TestSequence";
			Command.ExecuteNonQuery();
			*/
			
		}

		/// <summary>
		/// create a user defined sequence to test CACHE and CYCLE functionality when used in combination
		/// </summary>
		[Test]
		public void CreateSequenceStartWithMaxValCycleCache()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 MAXVALUE 5 CYCLE CACHE 4;";
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

			
		}
/// <summary>
/// create a user defined sequence to test cache functionality
/// </summary>
		[Test]
		public void CreateSequenceCache()
		{
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence CACHE 100;";
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
			/*EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 ORDER;";
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
		/// create user defined sequence to test NOORDER functionality.case fails because EDB doesnot provide NOORDER functionality
		/// </summary>
		[Test]
		public void CreateSequenceStartWithNoOrder()
		{
			/*EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="CREATE SEQUENCE TestSequence START WITH 1 NOORDER;";
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


	}
}
