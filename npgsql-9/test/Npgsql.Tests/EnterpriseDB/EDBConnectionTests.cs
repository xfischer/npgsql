using System;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;


/// <summary>
/// This Class contains functions for unit testing of .Net Driver.D:\shared\EDBNunit\
/// </summary>
[TestFixture]
[NonParallelizable]
public class EDBConnectionTests : TestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void Init()
    {
        con = OpenConnection();
        Console.WriteLine(con.ConnectionString.ToString());
    }

    [Test]
    public void TestConnecting()
    {
        Assert.DoesNotThrow(() =>
        {
            con = OpenConnection();
        }
        , "Exception was thrown while opening connection");
    }

    [Test]
    public void ChangeDatabase()
    {

        con!.ChangeDatabase("template1");

        var command = new EDBCommand("select current_database()", con);

        var result = (string)command.ExecuteScalar()!;
        Console.WriteLine(result);
        Assert.AreEqual("template1", result);

    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(con);
    }

    //Haroon
    [Test]
    public void TestEDBCommandStatement()
    {

        var Command = new EDBCommand("", con);
        Assert.IsNotNull(Command);
        Command.Dispose();

        //Ask for Updateable ResultSets
    }


    [Test]
    public void TestIsClosed()
    {
        var Con = OpenConnection();

        // Should not say closed
        Console.WriteLine(Con.State.ToString());

        Assert.AreEqual("OPEN", Con.State.ToString().ToUpper());

        TestUtil.closeDB(Con);
        Console.WriteLine(Con.State.ToString());

        // Should now say closed
        Assert.AreEqual("CLOSED", Con.State.ToString().ToUpper());
    }

    [Test]
    public void TestDoubleClose()
    {
        Assert.DoesNotThrow(() =>
        {
            var Con = OpenConnection();
            Con.Close();
            Con.Close();
        });
    }

}
