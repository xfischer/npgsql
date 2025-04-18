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
    public void testIntTimeout()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=45;");
            con.Close();
        });
    }

    [Test]
    public void testFloatTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=45.2;");
            con.Close();
        });
    }

    [Test]
    public void testZeroTimeout()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=0;");
            con.Close();
        });
    }


    [Test]
    public void testNegativeTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=-45;");
            con.Close();
        });
    }

    [Test]
    public void testBadTimeout()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var con = new EDBConnection("Server=127.0.0.1;Port=5433;UserId=edb;Password=edb;Database=edb;Timeout=abc;");
            con.Close();
        });
    }
}
