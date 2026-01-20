using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types;

/// <summary>
/// Tests for EDBPolygon
/// </summary>
/// 
[TestFixture]
[NonParallelizable]
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

        var command = new EDBCommand("create table EDBPolygonTest(id serial, f1 polygon);", con);
        var result = command.ExecuteNonQuery();
        Console.WriteLine("create table returned " + result);
    }

    private void Check(EDBPolygon polygon, EDBPoint[] points)
    {
        for (var i = 0; i < polygon.Count; i++)
        {
            Assert.That(polygon[i], Is.EqualTo(points[i]));
        }
    }

    [Test]
    public void CreateFromStringInt()
    {
        var polygon = EDBPolygon.Parse("((1,2),(3,4),(5,6))");
        Check(polygon, testPoints);
    }

    [Test]
    public void CreateFromStringNegativeInt()
    {
        var polygon = EDBPolygon.Parse("((7,0.1),(3,4.4),(8,-6))");
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
        var lst = new System.Collections.Generic.List<EDBPoint>();
        lst.Add(testPoints[0]);
        lst.Add(testPoints[1]);
        lst.Add(testPoints[2]);

        var polygon = new EDBPolygon(lst);
        Check(polygon, testPoints);
    }

    [Test]
    public void CreateFromCapacity()
    {
        var polygon = new EDBPolygon(1);
        polygon.Add(testPoints[0]);
        polygon.Add(testPoints[1]);
        polygon.Add(testPoints[2]);
    }

    [Test]
    public void TestToString()
    {
        var polygon = new EDBPolygon(testPoints);
        Assert.That(polygon.ToString(), Is.EqualTo("((1,2),(3,4),(5,6))"));

        polygon = new EDBPolygon(testPoints2);
        Assert.That(polygon.ToString(), Is.EqualTo("((7,0.1),(3,4.4),(8,-6))"));
    }

    [Test]
    public void TestEqual()
    {
        var c1 = new EDBPolygon(testPoints);
        var c2 = new EDBPolygon(testPoints);
        var c3 = new EDBPolygon(testPoints2);

        Assert.That(c1 == c2);
        Assert.That(c1.Equals(c2));

        Assert.That(c1.Equals(c3), Is.False);
        Assert.That(c1 == c3, Is.False);
        Assert.That(c1 != c3);

        var s = "Hello";
        Assert.That(c1.Equals((object)s), Is.False);
    }

    [Test]
    public void TestCRUD()
    {
        // Create 
        var inCircle = new EDBPolygon(testPoints);
        var command = new EDBCommand("insert into EDBPolygonTest values (1, :b)", con);
        command.Parameters.Add(new EDBParameter("b", EDBDbType.Polygon));
        command.Parameters[0].Value = inCircle;

        var rowsAdded = command.ExecuteNonQuery();
        Assert.That(rowsAdded, Is.EqualTo(1));

        // Retrieve
        command = new EDBCommand("select f1 from EDBPolygonTest;", con);
        var polygon = (EDBPolygon)command.ExecuteScalar()!;
        Check(polygon, testPoints);

        // Update
        inCircle = new EDBPolygon(testPoints2);
        command = new EDBCommand("Update EDBPolygonTest set f1 = :b where id = 1", con);
        command.Parameters.Add(new EDBParameter("b", EDBDbType.Polygon));
        command.Parameters[0].Value = inCircle;

        rowsAdded = command.ExecuteNonQuery();
        Assert.That(rowsAdded, Is.EqualTo(1));

        command = new EDBCommand("select f1 from EDBPolygonTest;", con);
        polygon = (EDBPolygon)command.ExecuteScalar()!;
        Check(polygon, testPoints2);

        // Delete
        command = new EDBCommand("Delete from EDBPolygonTest where id = 1", con);

        rowsAdded = command.ExecuteNonQuery();
        Assert.That(rowsAdded, Is.EqualTo(1));

        command = new EDBCommand("select f1 from EDBPolygonTest;", con);
        Assert.That(command.ExecuteNonQuery(), Is.EqualTo(-1));
    }

    [TearDown]
    public void Dispose()
    {
        var command = new EDBCommand("drop table EDBPolygonTest;", con);
        var result = command.ExecuteNonQuery();
        Console.WriteLine("drop table returned " + result);
        TestUtil.closeDB(con);
        con?.Dispose();
    }
}
