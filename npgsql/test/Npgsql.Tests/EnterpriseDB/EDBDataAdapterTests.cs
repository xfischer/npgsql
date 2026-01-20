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
using System.Data;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;


[TestFixture]
[NonParallelizable]
public class EDBDataAdapterTests : EPASTestBase
{

    [Test]
    public void FB8070_1()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();

            TestUtil.dropTable(con, "Quote");

            var com = new EDBCommand("", con)
            {
                CommandText = "create table Quote(id int4, b char)"
            };
            com.ExecuteNonQuery();
            com = new EDBCommand("", con)
            {
                CommandText = "create or replace procedure quoteproc(abc in integer)\n"
                    + "is\n"
                    + "declare\n"
                    + "i integer:=0;\n"
                    + "begin\n"
                    + "while i < abc loop\n"
                    + "insert into Quote values(1, 't');\n"
                    + "i := i+1;\n"
                    + "end loop;\n"
                    + "end;\n"
            };

            com.ExecuteNonQuery();

            com = new EDBCommand("quoteproc(:a)", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 2000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            var ds = new DataSet();
            Console.WriteLine("selecting data");
            var da = new EDBDataAdapter("select * from Quote", con);
            da.Fill(ds);
            Console.WriteLine("selected data");

            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote", con);
            com.ExecuteNonQuery();

            com = new EDBCommand("drop procedure quoteproc", con);
            com.ExecuteNonQuery();
            con.Close();
        });
    }

    [Test]
    public void UseDataAdapterEDBConnectionConstructor()
    {
        Assert.DoesNotThrow(() =>
        {
            using (var conn = OpenConnection())
            using (var command = new EDBCommand("SELECT 1", conn))
            {
                command.Connection = conn;
                var da = new EDBDataAdapter(command);
                var ds = new DataSet();
                da.Fill(ds);
            }
        });
    }

    [Test]
    public void UseDataAdapterStringEDBConnectionConstructor()
    {
        Assert.DoesNotThrow(() =>
        {
            using (var conn = OpenConnection())
            {
                var da = new EDBDataAdapter("SELECT 1", conn);
                var ds = new DataSet();
                da.Fill(ds);
            }
        });
    }

    [Test]
    public void UseDataAdapterStringStringConstructor()
    {
        Assert.DoesNotThrow(() =>
        {
            var da = new EDBDataAdapter("SELECT 1", ConnectionString);
            var ds = new DataSet();
            da.Fill(ds);
        });
    }

    [Test]
    public void UseDataAdapterStringStringConstructor2()
    {
        Assert.DoesNotThrow(() =>
        {
            var da = new EDBDataAdapter("SELECT 1", ConnectionString);
            var ds = new DataSet();
            da.Fill(ds);
        });
    }

    [Test]
    public void TestDSNotNull()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();
            var ds = new DataSet();
            var da = new EDBDataAdapter("select * from emp", con);

            da.Fill(ds);
            Console.WriteLine(ds.Tables[0].Rows.Count.ToString());

            Assert.That(ds, Is.Not.Null);
        });

    }

    [Test]
    public void FB8070_2()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();
            var com = new EDBCommand("", con)
            {
                CommandText = "create table if not exists Quote1(id int4, b char)"
            };
            com.ExecuteNonQuery();
            com = new EDBCommand("", con)
            {
                CommandText = "create or replace procedure quoteproc1(abc in integer)\n"
                    + "is\n"
                    + "declare\n"
                    + "i integer:=0;\n"
                    + "begin\n"
                    + "while i < abc loop\n"
                    + "insert into Quote1 values(1, 't');\n"
                    + "i := i+1;\n"
                    + "end loop;\n"
                    + "end;\n"
            };

            com.ExecuteNonQuery();

            com = new EDBCommand("quoteproc1(:a)", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 20000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            var ds = new DataSet();
            Console.WriteLine("selecting data");
            var da = new EDBDataAdapter("select * from Quote1", con);
            da.Fill(ds);
            Console.WriteLine("selected data");
            Console.WriteLine("filled data=" + ds.Tables[0].Rows.Count);

            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote1", con);
            com.ExecuteNonQuery();
            com = new EDBCommand("drop procedure quoteproc1", con);
            com.ExecuteNonQuery();
            con.Close();
        });
    }

    [Test]
    public void FB8070_3()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();
            var com = new EDBCommand("", con)
            {
                CommandText = "create table Quote2(id int4, b char)"
            };
            com.ExecuteNonQuery();
            com = new EDBCommand("", con)
            {
                CommandText = "create or replace procedure quoteproc2(abc in integer)\n"
                    + "is\n"
                    + "declare\n"
                    + "i integer:=0;\n"
                    + "begin\n"
                    + "while i < abc loop\n"
                    + "insert into Quote2 values(1, 't');\n"
                    + "i := i+1;\n"
                    + "end loop;\n"
                    + "end;\n"
            };

            com.ExecuteNonQuery();

            com = new EDBCommand("quoteproc2(:a)", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 200000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            var ds = new DataSet();
            Console.WriteLine("selecting data");
            var da = new EDBDataAdapter("select * from Quote2", con);
            da.Fill(ds);
            Console.WriteLine("selected data");
            Console.WriteLine("filled data=" + ds.Tables[0].Rows.Count);

            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote2", con);
            com.ExecuteNonQuery();
            com = new EDBCommand("drop procedure quoteproc2", con);
            com.ExecuteNonQuery();

            con.Close();
        });
    }

    [Test]
    public void FB8070_4()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();

            var com = new EDBCommand("", con)
            {
                CommandText = "create table if not exists Quote3(id int4, b char)"
            };
            com.ExecuteNonQuery();
            com = new EDBCommand("", con)
            {
                CommandText = "create or replace procedure quoteproc3(abc in integer)\n"
                    + "is\n"
                    + "declare\n"
                    + "i integer:=0;\n"
                    + "begin\n"
                    + "while i < abc loop\n"
                    + "insert into Quote3 values(1, 't');\n"
                    + "i := i+1;\n"
                    + "end loop;\n"
                    + "end;\n"
            };

            com.ExecuteNonQuery();

            com = new EDBCommand("quoteproc3(:a)", con)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 1500
            };

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 1000000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            var ds = new DataSet();
            Console.WriteLine("selecting data");
            var da = new EDBDataAdapter("select * from Quote3", con);
            da.Fill(ds);
            Console.WriteLine("selected data");
            Console.WriteLine("filled data=" + ds.Tables[0].Rows.Count);

            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote3", con);
            com.ExecuteNonQuery();
            com = new EDBCommand("drop procedure quoteproc3", con);
            com.ExecuteNonQuery();

            con.Close();
        });
    }

    [Test]
    public void FB8070_5()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();
            var com = new EDBCommand("", con)
            {
                CommandText = "create table if not exists Quote4(id int4, b char)"
            };
            com.ExecuteNonQuery();
            com = new EDBCommand("", con)
            {
                CommandText = "create or replace procedure quoteproc4(abc in integer)\n"
                    + "is\n"
                    + "declare\n"
                    + "i integer:=0;\n"
                    + "begin\n"
                    + "while i < abc loop\n"
                    + "insert into Quote4 values(1, 't');\n"
                    + "i := i+1;\n"
                    + "end loop;\n"
                    + "end;\n"
            };

            com.ExecuteNonQuery();

            com = new EDBCommand("quoteproc4(:a)", con)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 1500
            };
            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 1000000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            var ds = new DataSet();
            Console.WriteLine("selecting data");
            var da = new EDBDataAdapter("select * from Quote4", con);
            da.Fill(ds);
            Console.WriteLine("selected data");
            Console.WriteLine("filled data=" + ds.Tables[0].Rows.Count);

            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote4", con);
            com.ExecuteNonQuery();
            com = new EDBCommand("drop procedure quoteproc4", con);
            com.ExecuteNonQuery();

            con.Close();
        });
    }



    [Test]
    public void AdapFillSchemaDataTableSourceColumnNameAccess()
    {
        Assert.DoesNotThrow(() =>
        {
            var con = OpenConnection();

            var ds = new DataSet();

            var da = new EDBDataAdapter("select * from emp limit 1", con);
            try
            {
                da.FillSchema(ds, SchemaType.Source);
                var dt = new DataTable("testtab");
                da.FillSchema(dt, SchemaType.Source);

                Assert.That(dt.Columns[2].ColumnName.ToUpper(),Is.EqualTo("job".ToUpper()));
            }

            catch (Exception)
            {
                con.Close();
            }


            con.Close();
        });
    }

    [Test]
    public void AdapFillSchemaDataTableSourceColumnType()
    {
        var con = OpenConnection();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from emp limit 1", con);

        da.FillSchema(ds, SchemaType.Source);
        var dt = new DataTable("testtab");
        da.FillSchema(dt, SchemaType.Source);

        Assert.That(dt.Columns[0].DataType.FullName!.ToUpper(), Is.EqualTo("system.decimal".ToUpper()));
        Assert.That(dt.Columns[1].DataType.FullName!.ToUpper(), Is.EqualTo("system.string".ToUpper()));
        Assert.That(dt.Columns[2].DataType.FullName!.ToUpper(), Is.EqualTo("system.string".ToUpper()));
        Assert.That(dt.Columns[3].DataType.FullName!.ToUpper(), Is.EqualTo("system.decimal".ToUpper()));
        Assert.That(dt.Columns[4].DataType.FullName!.ToUpper(), Is.EqualTo("system.datetime".ToUpper()));
        Assert.That(dt.Columns[5].DataType.FullName!.ToUpper(), Is.EqualTo("system.decimal".ToUpper()));
        Assert.That(dt.Columns[6].DataType.FullName!.ToUpper(), Is.EqualTo("system.decimal".ToUpper()));
        Assert.That(dt.Columns[7].DataType.FullName!.ToUpper(), Is.EqualTo("system.decimal".ToUpper()));

        con.Close();
    }


    [Test]
    public void AdapFillSchemaDataTableSourcePrimaryKey()
    {
        var con = OpenConnection();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from emp limit 1", con);

        da.FillSchema(ds, SchemaType.Source);
        var dt = new DataTable("testtab");
        da.FillSchema(dt, SchemaType.Source);

        Assert.That(dt.PrimaryKey!.GetValue(0)!.ToString()!.ToUpper(), Is.EqualTo("empno".ToUpper()));

        con.Close();

    }
}


