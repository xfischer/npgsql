using System;
using NUnit.Framework;
using System.Data;
using NUnit;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

/// <summary>
/// Summary description for NumericDataTypeTest.
/// </summary>
/// 
[TestFixture]
[NonParallelizable]
public class EDBNumericDataTypeTest : EPASTestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void Init()
    {
        con = OpenConnection();
    }

    [TearDown]
    public void Dispose()
    {

        TestUtil.dropTable(con, "NumericTAB");

        TestUtil.closeDB(con);
    }


    //////////////////////////////////////////////
    ///

    [Test]

    public void TestNumericDataValid_3_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(3,0))"
            };
            Command.ExecuteNonQuery();
            Command.CommandText = "insert into NumericTAB values(410)";
            Command.ExecuteNonQuery();
            TestUtil.dropTable(con, "NumericTAB");

            Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(3,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(410)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_3_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(3,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-410)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_3_0()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(3,0))"
        };
        Command.ExecuteNonQuery();

        try
        {

            Command.CommandText = "insert into NumericTAB values(4101.6)";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");
    }

    [Test]
    public void TestNumericDataValid_3_2()
    {
        Assert.DoesNotThrow(() =>
            {

                var Command = new EDBCommand("", con)
                {
                    CommandText = "CREATE TABLE NumericTAB(A Numeric(3,2))"
                };
                Command.ExecuteNonQuery();

                Command.CommandText = "insert into NumericTAB values(4.15)";
                Command.ExecuteNonQuery();

                TestUtil.dropTable(con, "NumericTAB");
            });
    }

    [Test]
    public void TestNumericDataNegative_Valid_3_2()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(3,2))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-4.15)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_3_2()
    {
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(3,2))"
        };
        Command.ExecuteNonQuery();

        try
        {

            Command.CommandText = "insert into NumericTAB values(24.15)";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");
    }

    [Test]
    public void TestNumericDataValid_3_3()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(3,3))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(0.123)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");

        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_3_3()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(3,3))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-0.123)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_3_3()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(3,3))"
        };
        Command.ExecuteNonQuery();
        try
        {

            Command.CommandText = "insert into NumericTAB values(6.157)";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");
    }

    [Test]
    public void TestNumericDataValid_4_4()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(4,4))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(0.4585)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });

    }

    [Test]
    public void TestNumericDataNegative_Valid_4_4()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(4,4))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-0.4585)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }


    [Test]
    public void TestNumericDataInvalid_4_4()
    {

        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(4,4))"
        };
        Command.ExecuteNonQuery();
        try
        {

            Command.CommandText = "insert into NumericTAB values(9.4585)";
            Command.ExecuteNonQuery();

        }

        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");
    }

    [Test]
    public void TestNumericDataValid_8_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(8,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(45856987)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_8_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(8,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-45856987)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }


    [Test]
    public void TestNumericDataInValid_8_0()
    {

        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(8,0))"
        };
        Command.ExecuteNonQuery();


        try
        {
            Command.CommandText = "insert into NumericTAB values(945856987.25)";
            Command.ExecuteNonQuery();

            Assert.Fail("Expecting Numeric field overflow error");
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");

    }

    [Test]
    public void TestNumericDataValid_8_7()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(8,7))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(1.56987)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");

        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_8_7()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(8,7))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-4.56987)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");

        });
    }
    [Test]
    public void TestNumericDataInValid_8_7()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(8,7))"
        };
        Command.ExecuteNonQuery();

        try
        {

            Command.CommandText = "insert into NumericTAB values(68458.56987)";
            Command.ExecuteNonQuery();


            Assert.Fail("Expecting Numeric field overflow error");
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");

    }

    [Test]
    public void TestNumericDataValid_18_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(18,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(123456789124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_18_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(18,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-123456789124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }
    [Test]
    public void TestNumericDataInValid_18_0()
    {

        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(18,0))"
        };
        Command.ExecuteNonQuery();

        try
        {

            Command.CommandText = "insert into NumericTAB values(12345678912456358756)";
            Command.ExecuteNonQuery();

        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");

    }

    [Test]
    public void TestNumericDataValid_18_7()
    {
        Assert.DoesNotThrow(() =>
        {

            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(18,7))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(12345678912.4563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_18_7()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(18,7))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-12345678912.4563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_18_7()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(18,7))"
        };
        Command.ExecuteNonQuery();

        try
        {

            Command.CommandText = "insert into NumericTAB values(912345678912.4563587)";
            Command.ExecuteNonQuery();


            Assert.Fail("Expecting Numeric field overflow error");
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Expecting Numeric field overflow error");

    }

    [Test]
    public void TestNumericDataValid_19_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(19,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(9123456789124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegativeValid_19_0()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(19,0))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-9123456789124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_19_0()
    {

        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(19,0))"
        };
        Command.ExecuteNonQuery();

        try
        {

            Command.CommandText = "insert into NumericTAB values(19123456789124563587.25)";
            Command.ExecuteNonQuery();

            Assert.Fail("Expecting Numeric field overflow error");
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }
    }

    [Test]
    public void TestNumericDataValid_19_9()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(19,9))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(9123456789.124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegativeValid_19_9()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(19,9))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-9123456789.124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_19_9()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(19,9))"
        };
        Command.ExecuteNonQuery();

        try
        {
            Command.CommandText = "insert into NumericTAB values(19123456789.124563587)";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

    }
    [Test]
    public void TestNumericDataValid_19_19()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(19,19))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(0.9123456789124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegative_Valid_19_19()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric(19,19))"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-0.9123456789124563587)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataInValid_19_19()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric(19,19))"
        };
        Command.ExecuteNonQuery();


        try
        {

            Command.CommandText = "insert into NumericTAB values(59.9123456789124563587)";
            Command.ExecuteNonQuery();

            Assert.Fail("Expecting Numeric field overflow error");
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

    }

    [Test]
    public void TestNumericDataValid()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric)"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(1111111111122222222222222222222222222222222588888888888888888888888888888888888888888888888888888888888888666666666666666666666666666666666666666666666666666664444444444444444444444444444)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataNegative_Valid()
    {
        Assert.DoesNotThrow(() =>
        {
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE NumericTAB(A Numeric)"
            };
            Command.ExecuteNonQuery();

            Command.CommandText = "insert into NumericTAB values(-1111111111122222222222222222222222222222222588888888888888888888888888888888888888888888888888888888888888666666666666666666666666666666666666666666666666666664444444444444444444444444444)";
            Command.ExecuteNonQuery();

            TestUtil.dropTable(con, "NumericTAB");
        });
    }

    [Test]
    public void TestNumericDataGreaterThan1000Digits()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE NumericTAB(A Numeric)"
        };
        Command.ExecuteNonQuery();

        try
        {
            Command.CommandText = "insert into NumericTAB values(00123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890)";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22003")
        {
            Assert.Pass("Numeric field overflow error");
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }
    }

    [Test]

    public void TestCreateTableWithZeroZero()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con);

        try
        {

            Command.CommandText = "CREATE TABLE NumericTAB(A Numeric(0,0))";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22023")
        {
            Assert.Pass("Precision must be a non zero non negative number");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Precision must be a non zero non negative number");
    }

    [Test]
    public void TestCreateTableWithNegativeZero()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con);

        try
        {

            Command.CommandText = "CREATE TABLE NumericTAB(A Numeric(-1,0))";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22023")
        {
            Assert.Pass("Precision must be a non zero non negative number");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Precision must be a non zero non negative number");

    }
    [Test]
    public void TestCreateTableWithNegativeNegative()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con);

        try
        {

            Command.CommandText = "CREATE TABLE NumericTAB(A Numeric(-1,-1))";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22023")
        {
            Assert.Pass("Precision must be a non zero non negative number");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Precision must be a non zero non negative number");
    }


    [Test]
    public void TestCreateTableWithZeroNegative()
    {
        TestUtil.dropTable(con, "NumericTAB");
        var Command = new EDBCommand("", con);

        try
        {

            Command.CommandText = "CREATE TABLE NumericTAB(A Numeric(0,-1))";
            Command.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "22023")
        {
            Assert.Pass("Precision must be a non zero non negative number");
            return;
        }
        finally
        {
            TestUtil.dropTable(con, "NumericTAB");
        }

        Assert.Fail("Precision must be a non zero non negative number");
    }
}
