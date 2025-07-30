using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types;

	/// <summary>
	/// Tests for EDBPath
	/// </summary>
	/// 
	[TestFixture]
[NonParallelizable]
public class EDBPathTest : TestBase
{
		EDBConnection? con = null;
		EDBPoint[] testPoints = { new EDBPoint(1, 2), new EDBPoint(3, 4), new EDBPoint(5, 6) };
		EDBPoint[] testPoints2 = { new EDBPoint(7, 0.1), new EDBPoint(3, 4.4), new EDBPoint(8, -6) };

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();

			var command = new EDBCommand("create table EDBPathTest(id serial, f1 path);", con);
			var result = command.ExecuteNonQuery();
			Console.WriteLine("create table returned " + result);
		}

		private void Check(EDBPath path, EDBPoint []points)
		{
			for (var i = 0; i < path.Count; i++)
			{
				Assert.AreEqual(points[i], path[i]);
			}
		}

		[Test]
		public void CreateFromStringInt()
		{
			var path = EDBPath.Parse("((1,2),(3,4),(5,6))");
			Check(path, testPoints);
			Assert.False(path.Open);
		}

		[Test]
		public void CreateFromStringIntClosed()
		{
			var path = EDBPath.Parse("[(1,2),(3,4),(5,6)]");
			Check(path, testPoints);
			Assert.True(path.Open);
		}

		[Test]
		public void CreateFromStringNegativeInt()
		{
			var path = EDBPath.Parse("((7,0.1),(3,4.4),(8,-6))");
			Check(path, testPoints2);
		}

		[Test]
		//[ExpectedException(typeof(FormatException))]
		public void CreateFromStringInvalid()
		{
			Assert.Throws<FormatException>(() => EDBPath.Parse("(5)"));
		}

		[Test]
		public void CreateFromList()
		{
			var lst = new System.Collections.Generic.List<EDBPoint>();
			lst.Add(testPoints[0]);
			lst.Add(testPoints[1]);
			lst.Add(testPoints[2]);

			var path = new EDBPath(lst);
			Check(path, testPoints);
		}

		[Test]
		public void CreateFromCapacity()
		{
			var path = new EDBPath(1);
			path.Add(testPoints[0]);
			path.Add(testPoints[1]);
			path.Add(testPoints[2]);
		}

		[Test]
		public void TestToString()
		{
			var path = new EDBPath(testPoints);
			Assert.AreEqual("((1,2),(3,4),(5,6))", path.ToString());

			path = new EDBPath(testPoints2);
			Assert.AreEqual("((7,0.1),(3,4.4),(8,-6))", path.ToString());
		}

		[Test]
		public void TestEqual()
		{
			var c1 = new EDBPath(testPoints);
			var c2 = new EDBPath(testPoints);
			var c3 = new EDBPath(testPoints2);

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
			var inCircle = new EDBPath(testPoints);
			var command = new EDBCommand("insert into EDBPathTest values (1, :b)", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Path));
			command.Parameters[0].Value = inCircle;

			var rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			// Retrieve
			command = new EDBCommand("select f1 from EDBPathTest;", con);
			var path = (EDBPath)command.ExecuteScalar();
			Check(path, testPoints);

			// Update
			inCircle = new EDBPath(testPoints2);
			command = new EDBCommand("Update EDBPathTest set f1 = :b where id = 1", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Path));
			command.Parameters[0].Value = inCircle;

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBPathTest;", con);
			path = (EDBPath)command.ExecuteScalar();
			Check(path, testPoints2);

			// Delete
			command = new EDBCommand("Delete from EDBPathTest where id = 1", con);

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBPathTest;", con);
			Assert.AreEqual( -1, command.ExecuteNonQuery());
		}
		
		[TearDown] 
		public void Dispose()
		{
			var command = new EDBCommand("drop table EDBPathTest;", con);
			var result = command.ExecuteNonQuery();
			Console.WriteLine("drop table returned " + result);
			TestUtil.closeDB(con);
		}
	}
