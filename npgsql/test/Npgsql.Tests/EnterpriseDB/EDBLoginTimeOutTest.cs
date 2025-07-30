using System;
using NUnit.Framework;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

/// <summary>
/// Summary description for LoginTimeOutTest.
/// </summary>
/// 
[TestFixture]
public class EDBLoginTimeOutTest
{

    [Test]
    public void TestIntTimeout()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=45;");
            con.Close();
        });
    }

    [Test]
    public void TestFloatTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=45.2;");
            con.Close();
        });
    }

    [Test]
    public void TestZeroTimeout()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=0;");
            con.Close();
        });
    }


    [Test]
    public void TestNegativeTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=-45;");
            con.Close();
        });
    }

    [Test]
    public void TestBadTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=abc;");
            con.Close();
        });
    }
}
