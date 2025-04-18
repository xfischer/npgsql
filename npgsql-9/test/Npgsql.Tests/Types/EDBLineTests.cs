using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types;

	/// <summary>
	/// Tests for EDBLine
	/// </summary>
	/// 
	[TestFixture]
[NonParallelizable]
	public class EDBLineTest : TestBase
{
		EDBConnection? con = null;
		
		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();

			var command = new EDBCommand("create table EDBLineTest(id serial, f1 line);", con);
			var result = command.ExecuteNonQuery();
			Console.WriteLine("create table returned " + result);
		}

		private void Check(EDBLine line, double a, double b, double c)
		{
			Assert.AreEqual(a, line.A);
			Assert.AreEqual(b, line.B);
			Assert.AreEqual(c, line.C);
		}

		[Test]
		public void CreateFromStringInt()
		{
			var line = EDBLine.Parse("{4,3,5}");
			Check(line, 4, 3, 5);
		}

		[Test]
		public void CreateFromStringNegativeInt()
		{
			var line = EDBLine.Parse("{-4,3,5}");
			Check(line, -4, 3, 5);
		}

		[Test]
		//[ExpectedException(typeof(FormatException))]
		public void CreateFromStringInvalid()
		{
			Assert.Throws<FormatException>(() => EDBLine.Parse("(5"));
		}

		[Test]
		public void CreateFromStringDouble()
		{
			var line = EDBLine.Parse("{4.0,3.2,5.12}");
			Check(line, 4, 3.2, 5.12);
		}

		[Test]
		public void CreateFromInt()
		{
			var line = new EDBLine(4, 3, 5);
			Check(line, 4, 3, 5);
		}

		[Test]
		public void CreateFromDouble()
		{
			var line = new EDBLine(4.2, 3.2, 5.1);
			Check(line, 4.2, 3.2, 5.1);
		}

		[Test]
		public void TestToString()
		{
			var line = new EDBLine(4, 3, 5);
			Assert.AreEqual("{4,3,5}", line.ToString());

			line = new EDBLine(-4, 3, 5);
			Assert.AreEqual("{-4,3,5}", line.ToString());

			line = new EDBLine(4, 3.3, 5.6);
			Assert.AreEqual("{4,3.3,5.6}", line.ToString());
		}

		[Test]
		public void TestEqual()
		{
			var c1 = new EDBLine(4, 3, 5);
			var c2 = new EDBLine(4, 3, 5);
			var c3 = new EDBLine(4, 3.2, 6);

			Assert.True(c1 == c2);
			Assert.True(c1.Equals(c2));

			Assert.False(c1.Equals(c3));
			Assert.False(c1 == c3);
			Assert.True(c1 != c3);

			var s = "Hello";
			Assert.False(c1.Equals((object)s));
		}

		[Test]
		public void TestCRUD()
		{
			// Create 
			var inCircle = new EDBLine(4.0, 3.0, 15);
			var command = new EDBCommand("insert into EDBLineTest values (1, :b)", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Line));
			command.Parameters[0].Value = inCircle;

			var rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			// Retrieve
			command = new EDBCommand("select f1 from EDBLineTest;", con);
			var line = (EDBLine)command.ExecuteScalar();
			Check(line, 4, 3, 15);

			// Update
			inCircle = new EDBLine(4.0, 33.0, 1);
			command = new EDBCommand("Update EDBLineTest set f1 = :b where id = 1", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Line));
			command.Parameters[0].Value = inCircle;

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBLineTest;", con);
			line = (EDBLine)command.ExecuteScalar();
			Check(line, 4, 33, 1);

			// Delete
			command = new EDBCommand("Delete from EDBLineTest where id = 1", con);

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBLineTest;", con);
			Assert.AreEqual( -1, command.ExecuteNonQuery());
		}
		
		[TearDown] 
		public void Dispose()
		{
			var command = new EDBCommand("drop table EDBLineTest;", con);
			var result = command.ExecuteNonQuery();
			Console.WriteLine("drop table returned " + result);
			TestUtil.closeDB(con);
		}
	}
