using System;
using NUnit.Framework;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable CS8602
/// <summary>
/// Summary description for PreparedStatements.
/// </summary>
[TestFixture]
[NonParallelizable]
public class EDBPreparedStatements : EPASTestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void Init()
    {
        con = OpenConnection();
    }

    [TearDown]
    protected void TearDown()
    {
        if (con.State != ConnectionState.Closed)
            con.Close();
    }

    [Test]
    public void Testprepaed_statemant1()
    {
        Assert.DoesNotThrow(() =>
        {
            var updateQuery = "update emp set ename = :Name where empno = :ID";

            var Prepared_command = new EDBCommand(updateQuery, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));

            Prepared_command.Prepare();

            Prepared_command.Parameters[0].Value = 7369;
            Prepared_command.Parameters[1].Value = "Mark";

            Prepared_command.ExecuteNonQuery();

            var updateQuery1 = "update emp set ename = :Name where empno = :ID";

            var Prepared_command1 = new EDBCommand(updateQuery1, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command1.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command1.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));

            Prepared_command1.Prepare();

            Prepared_command1.Parameters[0].Value = 7369;
            Prepared_command1.Parameters[1].Value = "SMITH";

            Prepared_command1.ExecuteNonQuery();
        });
    }
    [Test]
    public void Testprepared_statemant2()
    {
        try
        {
            var updateQuery = "select ename from emp where  empno = :ID";

            var Prepared_command = new EDBCommand(updateQuery, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command.Prepare();

            Prepared_command.Parameters[0].Value = 7369;
            var reader = Prepared_command.ExecuteReader();
            while (reader.Read())
            {
                Assert.That(reader.GetValue(0).ToString().ToUpper(), Is.EqualTo("SMITH"));

            }
            reader.Close();
        }


        catch (EDBException exp)
        {

            Console.WriteLine(exp.ToString());

        }

    }
    [Test]
    public void Testprepaed_statemant3()
    {
        try
        {
            var updateQuery = "select * from emp where  empno = :ID";

            var Prepared_command = new EDBCommand(updateQuery, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command.Prepare();

            Prepared_command.Parameters[0].Value = 7369;
            var reader = Prepared_command.ExecuteReader();
            reader.Read();

            Assert.That(reader.GetValue(0).ToString(), Is.EqualTo("7369"));
            Assert.That(reader.GetValue(1).ToString().ToUpper(), Is.EqualTo("SMITH"));
            Assert.That(reader.GetValue(2).ToString().ToUpper(), Is.EqualTo("CLERK"));
            Assert.That(reader.GetValue(3).ToString(), Is.EqualTo("7902"));
            Assert.That(reader.GetValue(5).ToString(), Is.EqualTo("800.00"));

            Console.WriteLine("Success...");
            reader.Close();

        }
        catch (EDBException exp)
        {
            Console.WriteLine(exp.ToString());
        }

    }
    [Test]
    public void Testprepaed_statemant4()
    {
        try
        {

            var updateQuery = "select * from emp ,dept  where dept.deptno = emp.deptno and empno = :ID";

            var Prepared_command = new EDBCommand(updateQuery, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command.Prepare();

            Prepared_command.Parameters[0].Value = 7369;
            var reader = Prepared_command.ExecuteReader();
            reader.Read();

            Assert.That(reader.GetValue(0).ToString(), Is.EqualTo("7369"));
            Assert.That(reader.GetValue(1).ToString().ToUpper(), Is.EqualTo("SMITH"));
            Assert.That(reader.GetValue(2).ToString().ToUpper(), Is.EqualTo("CLERK"));
            Assert.That(reader.GetValue(3).ToString(), Is.EqualTo("7902"));
            Assert.That(reader.GetValue(5).ToString(), Is.EqualTo("800.00"));
            reader.Close();
            Console.WriteLine("Success...");

        }


        catch (EDBException exp)
        {

            Console.WriteLine(exp.ToString());


        }

    }

    [Test]
    public void Testprepaed_statemant5()
    {
        try
        {
            var updateQuery = "select * from emp ,dept  where dept.deptno = emp.deptno and empno = :ID and dept.deptno = :deptno";

            var Prepared_command = new EDBCommand(updateQuery, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command.Parameters.Add(new EDBParameter("deptno", EDBTypes.EDBDbType.Integer));
            Prepared_command.Prepare();

            Prepared_command.Parameters[0].Value = 7369;
            Prepared_command.Parameters[1].Value = 20;
            var reader = Prepared_command.ExecuteReader();
            reader.Read();

            Assert.That(reader.GetValue(0).ToString(), Is.EqualTo("7369"));
            Assert.That(reader.GetValue(1).ToString().ToUpper(), Is.EqualTo("SMITH"));
            Assert.That(reader.GetValue(2).ToString().ToUpper(), Is.EqualTo("CLERK"));
            Assert.That(reader.GetValue(3).ToString(), Is.EqualTo("7902"));
            Assert.That(reader.GetValue(5).ToString(), Is.EqualTo("800.00"));
            Assert.That(reader.GetValue(7).ToString(), Is.EqualTo("20"));
            reader.Close();

            Console.WriteLine("Success...");
        }
        catch (EDBException exp)
        {
            Console.WriteLine(exp.ToString());

        }
    }
    [Test]
    public void Testprepaed_statemant6()
    {

        try
        {
            var updateQuery = "select * from emp ,dept  where dept.deptno = emp.deptno and empno = :ID and dept.deptno = :deptno and dname = :dname";

            var Prepared_command = new EDBCommand(updateQuery, con)
            {
                CommandType = CommandType.Text
            };

            Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
            Prepared_command.Parameters.Add(new EDBParameter("deptno", EDBTypes.EDBDbType.Integer));
            Prepared_command.Parameters.Add(new EDBParameter("dname", EDBTypes.EDBDbType.Varchar));
            Prepared_command.Prepare();

            Prepared_command.Parameters[0].Value = 7369;
            Prepared_command.Parameters[1].Value = 20;
            Prepared_command.Parameters[2].Value = "RESEARCH";

            var reader = Prepared_command.ExecuteReader();
            reader.Read();
            Assert.That(reader.GetValue(0).ToString(), Is.EqualTo("7369"));
            Assert.That(reader.GetValue(1).ToString().ToUpper(), Is.EqualTo("SMITH"));
            Assert.That(reader.GetValue(2).ToString().ToUpper(), Is.EqualTo("CLERK"));
            Assert.That(reader.GetValue(3).ToString(), Is.EqualTo("7902"));
            Assert.That(reader.GetValue(5).ToString(), Is.EqualTo("800.00"));
            Assert.That(reader.GetValue(7).ToString(), Is.EqualTo("20"));
            reader.Close();
            Console.WriteLine("Success...");
        }
        catch (EDBException exp)
        {
            Console.WriteLine(exp.ToString());

        }

    }

    [Test]
    public void Testmultiple_statemant1()
    {
        Assert.DoesNotThrow(() =>
        {
            var CreateTableQuery = "create table test1 (a varchar);create table test2(a varchar);create table test3(a varchar)";
            var createcommand = new EDBCommand
            {
                CommandType = CommandType.Text,
                CommandText = CreateTableQuery,
                Connection = con
            };
            createcommand.ExecuteNonQuery();
        });
    }
    [Test]
    public void Testmultiple_statemant2()
    {

        Assert.DoesNotThrow(() =>
        {
            var InsertTableQuery = "insert into  test1 values('EnterpriseDB');insert into test2 values ('Islamabad');insert into test3 values('Pakistan');";
            var createcommand = new EDBCommand
            {
                CommandType = CommandType.Text,
                CommandText = InsertTableQuery,
                Connection = con
            };
            createcommand.ExecuteNonQuery();

            Console.WriteLine("Success...");
        });

    }
    [Test]
    public void Testmultiple_statemant3()
    {
        Assert.DoesNotThrow(() =>
        {
            var CreateTableQuery = "drop table test1;drop table test2;drop table test3;";
            var createcommand = new EDBCommand
            {
                CommandType = CommandType.Text,
                CommandText = CreateTableQuery,
                Connection = con
            };
            createcommand.ExecuteNonQuery();

            Console.WriteLine("Success...");
        });
    }
}
#pragma warning restore CS8602

