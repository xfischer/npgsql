using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

class EDBSqlQueryParserTests : TestBase
{

    [TestCase(true)]
    public void CreateSPLProcedureLf(bool redwoodMode)
    {
        var commandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar) \n"
                + " AS \n"
                + " BEGIN \n"
                + "    a:='HELLO'; \n"
                + " END; \n";

        var result = ParseCommand(commandText, redwoodMode);

        Assert.That(result, Has.Count.EqualTo(1));

    }

    [TestCase(true)]
    public void CreateSPLProcedureLf2(bool redwoodMode)
    {
        var commandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar) \r\n"
                + " AS \r\n"
                + " BEGIN \r\n"
                + "    a:='HELLO'; \r\n"
                + " END; \r\n";

        var result = ParseCommand(commandText, redwoodMode);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Theory]
    [TestCase(true)]
    [TestCase(false)]
    public void CreateSPLProcedureCrLf(bool redwoodMode)
    {
        var commandText = @"CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar)
                 AS
                BEGIN
                    a:='HELLO'
                END;";

        var result = ParseCommand(commandText, redwoodMode);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Theory]
    [TestCase(true)]
    [TestCase(false)]
    public void CreateSPLProcedureLowerCase(bool redwoodMode)
    {
        var commandText = @"create or replace procedure oneoutargproc_test1(a out varchar)
                 as
                begin
                    a:='hello'
                end;";

        var result = ParseCommand(commandText, redwoodMode);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Theory]
    [TestCase(true)]
    [TestCase(false)]
    public void CreatePlPgSqlProcedure(bool redwoodMode)
    {
        var commandText = @"CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar)
                                        LANGUAGE plpgsql
                                        AS $$
                                        BEGIN
                                        a:= 'HELLO';
                                    END; $$";

        var result = ParseCommand(commandText, redwoodMode);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [TestCase(false)]
    public void MultipleStatements(bool redwoodMode)
    {
        var commandText = """
CREATE FUNCTION "CustomerOrderCount" ("customerId" INTEGER) RETURNS INTEGER
AS $$ SELECT COUNT("Id")::INTEGER FROM "Orders" WHERE "CustomerId" = $1 $$
LANGUAGE SQL;

CREATE FUNCTION "StarValue" ("starCount" INTEGER, value TEXT) RETURNS TEXT
AS $$ SELECT repeat('*', $1) || $2 $$
LANGUAGE SQL;
""";

        var result = ParseCommand(commandText, redwoodMode);

        Assert.That(result, Has.Count.EqualTo(2));

    }

    [Test]
    public void UsingPackagesWithUserDefinedTypesTest()
    {
        var commandText = """
            DECLARE
                v_deptno dept.deptno%TYPE DEFAULT 30;
                v_emp_cur emp_rpt.EMP_REFCUR;
            BEGIN
                v_emp_cur := emp_rpt.open_emp_by_dept(v_deptno);
                DBMS_OUTPUT.PUT_LINE('EMPLOYEES IN DEPT #' || v_deptno ||
                    ': ' || emp_rpt.get_dept_name(v_deptno));
                emp_rpt.fetch_emp(v_emp_cur);
                DBMS_OUTPUT.PUT_LINE('**********************');
                DBMS_OUTPUT.PUT_LINE(v_emp_cur%ROWCOUNT || ' rows were retrieved');
                emp_rpt.close_refcur(v_emp_cur);
            END;
            """;
        var result = ParseCommand(commandText, redwoodMode: true);
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void BaseLineAnonymousTest()
    {
        var commandText = """
            DECLARE    
            	TYPE rec IS RECORD (x INT, y INT);
            	rec_var rec;
            	row_var db1425_t1%ROWTYPE;
            	comp_var db1425_t1;
            BEGIN    
            	rec_var = row(1000, 1000);    
            	UPDATE db1425_t1 SET ROW=rec_var WHERE a = 1;	
            	row_var.a = 2000;	
            	row_var.b = 2000;    
            	UPDATE db1425_t1 SET ROW=row_var WHERE a = 2;	
            	comp_var = row(3000, 3000);    
            	UPDATE db1425_t1 SET ROW=comp_var WHERE a = 3;
            END;
            """;
        var result = ParseCommand(commandText, redwoodMode: true);
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Theory]
    [TestCase("TEST procedure\n", "PROCEDURE", true)]
    [TestCase("TEST PROCEDUREABC", "PROCEDURE", false)]
    [TestCase("TEST procedure ABC", "PROCEDURE", true)]
    [TestCase("procedure ABC", "PROCEDURE", true)]
    [TestCase("procedure ABC", "PROCEDURE ", false)]
    public void TestSPLDetection_ContainsWord(string query, string keyword,  bool expected)
    {
        Assert.That(query.ContainsWord(keyword, StringComparison.OrdinalIgnoreCase) , Is.EqualTo(expected));
    }

    [Theory]
    [TestCase("TEST procedure\n", "PROCEDURE", false)]
    [TestCase("TEST PROCEDUREABC", "PROCEDURE", false)]
    [TestCase("procedure", "PROCEDURE", false)]
    [TestCase("procedure ABC", "PROCEDURE", true)]
    public void TestSPLDetection_StartsWithWord(string query, string keyword, bool expected)
    {
        Assert.That(query.StartsWithWord(keyword, StringComparison.OrdinalIgnoreCase), Is.EqualTo(expected));
    }

    [Test]
    public void UsingPackagesWithUserDefinedTypesRecordVariableTest()
    {
        var commandText = """
            DECLARE
                v_deptno     dept.deptno%TYPE DEFAULT 30;
                v_emp_cur    emp_rpt.EMP_REFCUR;
                r_emp        emp_rpt.EMPREC_TYP;
            BEGIN
                v_emp_cur := emp_rpt.open_emp_by_dept(v_deptno);
                DBMS_OUTPUT.PUT_LINE('EMPLOYEES IN DEPT #' || v_deptno ||
                    ': ' || emp_rpt.get_dept_name(v_deptno));
                DBMS_OUTPUT.PUT_LINE('EMPNO ENAME');
                DBMS_OUTPUT.PUT_LINE('----- -------');
                LOOP
                    FETCH v_emp_cur INTO r_emp;
                    EXIT WHEN v_emp_cur%NOTFOUND;
                    DBMS_OUTPUT.PUT_LINE(r_emp.empno || '  ' ||
                        r_emp.ename);
                END LOOP;
                DBMS_OUTPUT.PUT_LINE('**********************');
                DBMS_OUTPUT.PUT_LINE(v_emp_cur%ROWCOUNT || ' rows were retrieved');
                CLOSE v_emp_cur;
            END;
            """;
        var result = ParseCommand(commandText, redwoodMode: true);
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void ModularizingCursorOperationsTest()
    {
        var commandText = """
            DECLARE
                gen_refcur      SYS_REFCURSOR;
            BEGIN
                DBMS_OUTPUT.PUT_LINE('ALL EMPLOYEES');
                open_all_emp(gen_refcur);
                fetch_emp(gen_refcur);
                DBMS_OUTPUT.PUT_LINE('****************');

                DBMS_OUTPUT.PUT_LINE('EMPLOYEES IN DEPT #10');
                open_emp_by_dept(gen_refcur, 10);
                fetch_emp(gen_refcur);
                DBMS_OUTPUT.PUT_LINE('****************');

                DBMS_OUTPUT.PUT_LINE('DEPARTMENTS');
                fetch_dept(open_dept(gen_refcur));
                DBMS_OUTPUT.PUT_LINE('*****************');

                close_refcur(gen_refcur);
            END;
            """;
        var result = ParseCommand(commandText, redwoodMode: true);
        Assert.That(result, Has.Count.EqualTo(1));
    }

    #region Setup / Teardown / Utils

    static List<EDBBatchCommand> ParseCommand(string sql, bool redwoodMode, params EDBParameter[] parameters)
        => ParseCommand(sql, parameters, standardConformingStrings: true, redwoodMode);

    static List<EDBBatchCommand> ParseCommand(string sql, EDBParameter[] parameters, bool standardConformingStrings, bool redwoodMode)
    {
        var cmd = new EDBCommand(sql);
        cmd.Parameters.AddRange(parameters);
        var parser = new SqlQueryParser(redwoodMode);
        parser.ParseRawQuery(cmd, standardConformingStrings);
        return cmd.InternalBatchCommands;
    }

    #endregion
}
