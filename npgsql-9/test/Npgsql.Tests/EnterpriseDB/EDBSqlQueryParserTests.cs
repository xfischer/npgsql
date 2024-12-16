using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

    #region Setup / Teardown / Utils

    List<EDBBatchCommand> ParseCommand(string sql, bool redwoodMode, params EDBParameter[] parameters)
        => ParseCommand(sql, parameters, standardConformingStrings: true, redwoodMode);

    List<EDBBatchCommand> ParseCommand(string sql, EDBParameter[] parameters, bool standardConformingStrings, bool redwoodMode)
    {
        var cmd = new EDBCommand(sql);
        cmd.Parameters.AddRange(parameters);
        var parser = new SqlQueryParser(redwoodMode);
        parser.ParseRawQuery(cmd, standardConformingStrings);
        return cmd.InternalBatchCommands;
    }

    #endregion
}
