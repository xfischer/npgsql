using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types
{
	/// <summary>
	/// Test for EDBBOX
	/// </summary>
	/// 
	[TestFixture]
    [NonParallelizable]
	public class EDBBoxTest : TestBase
    {
		EDBConnection? con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();

			EDBCommand command = new EDBCommand("create table EDBBoxTest(id serial, f1 box);", con);
			int result = command.ExecuteNonQuery();
			Console.WriteLine("create table returned " + result);
		}

		private void Check(EDBBox box, double upperX, double upperY, double lowerX, double lowerY)
		{
			Assert.AreEqual(upperX, box.UpperRight.X);
			Assert.AreEqual(upperY, box.UpperRight.Y);
			Assert.AreEqual(lowerX, box.LowerLeft.X);
			Assert.AreEqual(lowerY, box.LowerLeft.Y);
		}

		[Test]
		public void CreateFromStringInt()
		{
			EDBBox box = EDBBox.Parse("(4,3),(5,4)");
			Check(box, 5, 4, 4, 3);
		}

		[Test]
		public void CreateFromStringNegativeInt()
		{
			EDBBox box = EDBBox.Parse("(-4,3),(5,-4)");
			Check(box, 5, 3, -4, -4);
		}

		[Test]
		public void CreateFromStringDouble()
		{
			EDBBox box = EDBBox.Parse("(4.0,3.0),(5.0,4.0)");
			Check(box, 5, 4, 4, 3);
		}

		[Test]
		//[ExpectedException(typeof(FormatException))]
		public void CreateFromStringInvalid()
		{
			Assert.Throws<FormatException>(() => EDBBox.Parse("(-4,3,2,5"));
		}

		[Test]
		public void CreateFromStringEmpty()
		{
			EDBBox box = EDBBox.Parse("(4.0,3.0),(4.0,4.0)");
			Check(box, 4, 4, 4, 3);

			Assert.True(box.IsEmpty);
		}

		[Test]
		public void CreateFromTwoPoint()
		{
			EDBBox box = new EDBBox(new EDBPoint(4.0, 3.0), new EDBPoint(4.0, 13.0));
			Check(box, 4, 13, 4, 3);

			Assert.AreEqual(13, box.Top);
			Assert.AreEqual(4, box.Right);
			Assert.AreEqual(3, box.Bottom);
			Assert.AreEqual(4, box.Left);

			Assert.True(box.IsEmpty);
		}

		[Test]
		public void CreateFromFourPoint()
		{
			EDBBox box = new EDBBox(4.0,3.0,4.0,13.0);
			Check(box, 13, 4, 3, 4);

			Assert.AreEqual(4, box.Top);
			Assert.AreEqual(13, box.Right);
			Assert.AreEqual(4, box.Bottom);
			Assert.AreEqual(3, box.Left);

			Assert.True(box.IsEmpty);
		}

		[Test]
		public void TestEqual()
		{
			EDBBox box1 = new EDBBox(4.0, 3.0, 4.0, 13.0);
			EDBBox box2 = new EDBBox(4.0, 3.0, 4.0, 13.0);
			EDBBox box3 = new EDBBox(4.0, 3.0, 5.0, 13.0);

			Assert.True(box1 == box2);
			Assert.True(box1.Equals(box2));

			Assert.False(box1.Equals(box3));
			Assert.False(box1 == box3);
			Assert.True(box1 != box3);

			String s = "Hello";
			Assert.False(box1.Equals((object)s));

		}

		
		[Test]
		public void TestInsert()
		{
			EDBBox box1 = new EDBBox(4.0, 3.0, 4.0, 3.0);
			EDBCommand command = new EDBCommand("insert into EDBBoxTest values (1, :b)", con);

			command.Parameters.Add(new EDBParameter("b", EDBDbType.Box));
			command.Parameters[0].Value = box1;


			Int32 rowsAdded = command.ExecuteNonQuery();

			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBBoxTest;", con);
			
			EDBBox box = (EDBBox)command.ExecuteScalar();
			Check(box, 3, 4, 3, 4);

			// Update
			box1 = new EDBBox(5.0, 2.0, 5.0, 2.0);
			command = new EDBCommand("Update EDBBoxTest set f1 = :b where id = 1", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Box));
			command.Parameters[0].Value = box1;

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBBoxTest;", con);
			box = (EDBBox)command.ExecuteScalar();
			Check(box, 2, 5, 2, 5);

			// Delete
			command = new EDBCommand("Delete from EDBBoxTest where id = 1", con);

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBBoxTest;", con);
			Assert.AreEqual(-1, command.ExecuteNonQuery());
		}
		
		[TearDown] 
		public void Dispose()
		{
			EDBCommand command = new EDBCommand("drop table EDBBoxTest;", con);
			int result = command.ExecuteNonQuery();
			Console.WriteLine("drop table returned " + result);
			TestUtil.closeDB(con);
		}
	}
}
