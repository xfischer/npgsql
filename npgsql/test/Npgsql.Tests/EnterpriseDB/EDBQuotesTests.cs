#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Data;

#pragma warning disable CS8602
namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

[TestFixture]
[NonParallelizable]
public class EDBQuotesTests : EPASTestBase
{

    [OneTimeSetUp]
    public void Setup()
    {
        using var con = OpenConnection();
        DropTableQuote(con);
        con.Close();
    }

    private static void DropTableQuote(EDBConnection con)
    {
        var com = new EDBCommand("", con)
        {
            CommandText = "DROP TABLE Quote"
        };
        try
        {
            com.ExecuteNonQuery();
        }
        catch (Exception)
        {
            // swallow
        }
    }

    [Test]
    public void QuoteHandling1()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows);
        while (Reader.Read())
        {
            Assert.That(Reader.GetDecimal(0), Is.InstanceOf<decimal>());
        }
        Reader.Close();
        con.Close();
    }

    [Test]
    public void QuoteHandling2()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar));
        com.Parameters[0].Value = "t";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows);
        while (Reader.Read())
        {
            Assert.That(Reader.GetDecimal(0), Is.InstanceOf<decimal>());
        }
        Reader.Close();
        DropTableQuote(con);
    }

    [Test]
    public void QuoteHandling3()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, 5));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        con.Close();
    }

    [Test]
    public void QuoteHandling4()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, 1));
        com.Parameters[0].Value = "t";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }

    [Test]
    public void QuoteHandling5()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Char, 5));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetDecimal(0), Is.InstanceOf<decimal>());
        }
        Reader.Close();
        con.Close();
    }

    [Test]
    public void QuoteHandling6()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        EDBDataReader? Reader = null;
        com.CommandText = "select id from Quote where b= :No";


        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char, 1));
        com.Parameters[0].Value = "t";
        Reader = com.ExecuteReader();


        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }


    [Test]
    public void QuoteHandling7()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        EDBDataReader? Reader = null;
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char));
        com.Parameters[0].Value = "t";
        Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }

    [Test]
    public void QuoteHandling8()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        EDBDataReader? Reader = null;
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar));
        com.Parameters[0].Value = "t";
        Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }

        Reader.Close();
        DropTableQuote(con);
    }

    [Test]
    public void QuoteHandling9()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        EDBDataReader? Reader = null;
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char, -1));
        com.Parameters[0].Value = "t";
        Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }

    [Test]
    public void QuoteHandling10()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Char, -4));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        con.Close();
    }


    [Test]
    public void QuoteHandling11()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, -4));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetDecimal(0), Is.InstanceOf<decimal>());
        }
        Reader.Close();
        con.Close();
    }

    [Test]
    public void QuoteHandling12()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, -1));
        com.Parameters[0].Value = "t";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }

    
    [Test]
    public void QuoteHandling15()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char, 0));
        com.Parameters[0].Value = "t";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }

    [Test]
    public void QuoteHandling16()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Char, 5));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        con.Close();
    }


    [Test]
    public void QuoteHandling17()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "select empno from emp where ename= :Name"
        };
        com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, 5));
        com.Parameters[0].Value = "SMITH";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetDecimal(0), Is.InstanceOf<decimal>());
        }
        Reader.Close();
        con.Close();
    }

    [Test]
    public void QuoteHandling18()
    {
        using var con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE Quote(id int4, b char)"
        };
        com.ExecuteNonQuery();
        com.CommandText = "INSERT INTO Quote values(1, 't')";
        com.ExecuteNonQuery();
        com.CommandText = "select id from Quote where b= :No";
        com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, 0));
        com.Parameters[0].Value = "t";
        var Reader = com.ExecuteReader();

        Assert.That(Reader.HasRows, "Expected rows were not returned.");
        while (Reader.Read())
        {
            Assert.That(Reader.GetInt32(0), Is.InstanceOf<int>());
        }
        Reader.Close();
        DropTableQuote(con);
    }
}
