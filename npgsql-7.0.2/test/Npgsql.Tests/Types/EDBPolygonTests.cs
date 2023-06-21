using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types
{
	/// <summary>
	/// Tests for EDBPolygon
	/// </summary>
	/// 
	[TestFixture]
	public class EDBPolygonTest : TestBase
    {
		EDBConnection? con = null;
		EDBPoint[] testPoints = { new EDBPoint(1, 2), new EDBPoint(3, 4), new EDBPoint(5, 6) };
		EDBPoint[] testPoints2 = { new EDBPoint(7, 0.1), new EDBPoint(3, 4.4), new EDBPoint(8, -6) };

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();

			EDBCommand command = new EDBCommand("create table EDBPolygonTest(id serial, f1 polygon);", con);
			int result = command.ExecuteNonQuery();
			Console.WriteLine("create table returned " + result);
		}

		private void Check(EDBPolygon polygon, EDBPoint []points)
		{
			for (int i = 0; i < polygon.Count; i++)
			{
				Assert.AreEqual(points[i], polygon[i]);
			}
		}

		[Test]
		public void CreateFromStringInt()
		{
			EDBPolygon polygon = EDBPolygon.Parse("((1,2),(3,4),(5,6))");
			Check(polygon, testPoints);
		}

		[Test]
		public void CreateFromStringNegativeInt()
		{
			EDBPolygon polygon = EDBPolygon.Parse("((7,0.1),(3,4.4),(8,-6))");
			Check(polygon, testPoints2);
		}

		[Test]
		//[ExpectedException(typeof(FormatException))]
		public void CreateFromStringInvalid()
		{
			Assert.Throws<FormatException>(() => EDBPolygon.Parse("(5)"));
		}

		[Test]
		public void CreateFromList()
		{
			System.Collections.Generic.List<EDBPoint> lst = new System.Collections.Generic.List<EDBPoint>();
			lst.Add(testPoints[0]);
			lst.Add(testPoints[1]);
			lst.Add(testPoints[2]);

			EDBPolygon polygon = new EDBPolygon(lst);
			Check(polygon, testPoints);
		}

		[Test]
		public void CreateFromCapacity()
		{
			EDBPolygon polygon = new EDBPolygon(1);
			polygon.Add(testPoints[0]);
			polygon.Add(testPoints[1]);
			polygon.Add(testPoints[2]);
		}

		[Test]
		public void TestToString()
		{
			EDBPolygon polygon = new EDBPolygon(testPoints);
			Assert.AreEqual("((1,2),(3,4),(5,6))", polygon.ToString());

			polygon = new EDBPolygon(testPoints2);
			Assert.AreEqual("((7,0.1),(3,4.4),(8,-6))", polygon.ToString());
		}

		[Test]
		public void TestEqual()
		{
			EDBPolygon c1 = new EDBPolygon(testPoints);
			EDBPolygon c2 = new EDBPolygon(testPoints);
			EDBPolygon c3 = new EDBPolygon(testPoints2);

			Assert.True(c1 == c2);
			Assert.True(c1.Equals(c2));

			Assert.False(c1.Equals(c3));
			Assert.False(c1 == c3);
			Assert.True(c1 != c3);

			String s = "Hello";
			Assert.False(c1.Equals((object)s));
		}

		[Test]
		public void TestCRUD()
		{
			// Create 
			EDBPolygon inCircle = new EDBPolygon(testPoints);
			EDBCommand command = new EDBCommand("insert into EDBPolygonTest values (1, :b)", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Polygon));
			command.Parameters[0].Value = inCircle;

			Int32 rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			// Retrieve
			command = new EDBCommand("select f1 from EDBPolygonTest;", con);
			EDBPolygon polygon = (EDBPolygon)command.ExecuteScalar();
			Check(polygon, testPoints);

			// Update
			inCircle = new EDBPolygon(testPoints2);
			command = new EDBCommand("Update EDBPolygonTest set f1 = :b where id = 1", con);
			command.Parameters.Add(new EDBParameter("b", EDBDbType.Polygon));
			command.Parameters[0].Value = inCircle;

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBPolygonTest;", con);
			polygon = (EDBPolygon)command.ExecuteScalar();
			Check(polygon, testPoints2);

			// Delete
			command = new EDBCommand("Delete from EDBPolygonTest where id = 1", con);

			rowsAdded = command.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAdded);

			command = new EDBCommand("select f1 from EDBPolygonTest;", con);
			Assert.AreEqual( -1, command.ExecuteNonQuery());
		}
		
		[TearDown] 
		public void Dispose()
		{
			EDBCommand command = new EDBCommand("drop table EDBPolygonTest;", con);
			int result = command.ExecuteNonQuery();
			Console.WriteLine("drop table returned " + result);
			TestUtil.closeDB(con);
		}
	}
}
