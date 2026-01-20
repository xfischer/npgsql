using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types;

/// <summary>
/// Tests for EDBCircle
/// </summary>
/// 
[TestFixture]
[NonParallelizable]
public class EDBCircleTest : TestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void Init()
    {
        //write setup for following test cases
        con = OpenConnection();

        var command = new EDBCommand("create table EDBCircleTest(id serial, f1 circle);", con);
        var result = command.ExecuteNonQuery();
        Console.WriteLine("create table returned " + result);
    }

    private void Check(EDBCircle circle, double x, double y, double r)
    {
        Assert.That(circle.Center.X, Is.EqualTo(x));
        Assert.That(circle.Center.Y, Is.EqualTo(y));
        Assert.That(circle.Radius, Is.EqualTo(r));
    }

    [Test]
    public void CreateFromStringInt()
    {
        var circle = EDBCircle.Parse("<(4,3),5>");
        Check(circle, 4, 3, 5);
    }

    [Test]
    public void CreateFromStringNegativeInt()
    {
        var circle = EDBCircle.Parse("<(-4,3),5>");
        Check(circle, -4, 3, 5);
    }

    [Test]
    //[ExpectedException(typeof(FormatException))]
    public void CreateFromStringInvalid()
    {
        Assert.Throws<FormatException>(() => EDBCircle.Parse("(-4,3),5"));
    }

    [Test]
    public void CreateFromStringDouble()
    {
        var circle = EDBCircle.Parse("<(4.0,3.2),5.12>");
        Check(circle, 4, 3.2, 5.12);
    }

    [Test]
    public void CreateFromInt()
    {
        var circle = new EDBCircle(4, 3, 5);
        Check(circle, 4, 3, 5);
    }

    [Test]
    public void CreateFromDouble()
    {
        var circle = new EDBCircle(4.2, 3.3, 5.4);
        Check(circle, 4.2, 3.3, 5.4);
    }

    [Test]
    public void CreateFromCenterPoint()
    {
        var circle = new EDBCircle(new EDBPoint(4, 3), 5);
        Check(circle, 4, 3, 5);
        var line = new EDBLine(1, 3, 4);
        Console.WriteLine(line.ToString());
    }

    [Test]
    public void TestToString()
    {
        var circle = new EDBCircle(4, 3, 5);
        Assert.That(circle.ToString(), Is.EqualTo("<(4,3),5>"));

        circle = new EDBCircle(-4, 3, 5);
        Assert.That(circle.ToString(), Is.EqualTo("<(-4,3),5>"));

        circle = new EDBCircle(4, 3.3, 5.6);
        Assert.That(circle.ToString(), Is.EqualTo("<(4,3.3),5.6>"));
    }

    [Test]
    public void TestEqual()
    {
        var c1 = new EDBCircle(new EDBPoint(4, 3), 5);
        var c2 = new EDBCircle(new EDBPoint(4, 3), 5);
        var c3 = new EDBCircle(new EDBPoint(4, 3.2), 6);

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
        var inCircle = new EDBCircle(4.0, 3.0, 15);
        var command = new EDBCommand("insert into EDBCircleTest values (1, :b)", con);
        command.Parameters.Add(new EDBParameter("b", EDBDbType.Circle));
        command.Parameters[0].Value = inCircle;

        var rowsAdded = command.ExecuteNonQuery();
        Assert.That(rowsAdded, Is.EqualTo(1));

        // Retrieve
        command = new EDBCommand("select f1 from EDBCircleTest;", con);
        var circle = (EDBCircle)command.ExecuteScalar()!;
        Check(circle, 4, 3, 15);

        // Update
        inCircle = new EDBCircle(4.0, 33.0, 1);
        command = new EDBCommand("Update EDBCircleTest set f1 = :b where id = 1", con);
        command.Parameters.Add(new EDBParameter("b", EDBDbType.Circle));
        command.Parameters[0].Value = inCircle;

        rowsAdded = command.ExecuteNonQuery();
        Assert.That(rowsAdded, Is.EqualTo(1));

        command = new EDBCommand("select f1 from EDBCircleTest;", con);
        circle = (EDBCircle)command.ExecuteScalar()!;
        Check(circle, 4, 33, 1);

        // Delete
        command = new EDBCommand("Delete from EDBCircleTest where id = 1", con);

        rowsAdded = command.ExecuteNonQuery();
        Assert.That(rowsAdded, Is.EqualTo(1));

        command = new EDBCommand("select f1 from EDBCircleTest;", con);
        Assert.That(command.ExecuteNonQuery(), Is.EqualTo(-1));
    }

    [TearDown]
    public void Dispose()
    {
        var command = new EDBCommand("drop table EDBCircleTest;", con);
        var result = command.ExecuteNonQuery();
        Console.WriteLine("drop table returned " + result);
        TestUtil.closeDB(con);
        con?.Dispose();

    }
}
