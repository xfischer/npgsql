using System;
using System.Collections;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;
using NUnit.Framework;
using EnterpriseDB.EDBClient.Tests;
using EnterpriseDB.EDBClient;

#pragma warning disable CS0618 // EDBTsVector.Parse is obsolete

namespace EnterpriseDB.EDBClient.Tests.Types;

public class FullTextSearchTests : MultiplexingTestBase
{
    public FullTextSearchTests(MultiplexingMode multiplexingMode)
        : base(multiplexingMode) { }

    [Test]
    public Task TsVector()
        => AssertType(
            EDBTsVector.Parse("'1' '2' 'a':24,25A,26B,27,28,12345C 'b' 'c' 'd'"),
            "'1' '2' 'a':24,25A,26B,27,28,12345C 'b' 'c' 'd'",
            "tsvector",
            EDBDbType.TsVector);

    public static IEnumerable TsQueryTestCases() => new[]
    {
        new object[]
        {
            "'a'",
            new EDBTsQueryLexeme("a")
        },
        new object[]
        {
            "!'a'",
            new EDBTsQueryNot(
                new EDBTsQueryLexeme("a"))
        },
        new object[]
        {
            "'a' | 'b'",
            new EDBTsQueryOr(
                new EDBTsQueryLexeme("a"),
                new EDBTsQueryLexeme("b"))
        },
        new object[]
        {
            "'a' & 'b'",
            new EDBTsQueryAnd(
                new EDBTsQueryLexeme("a"),
                new EDBTsQueryLexeme("b"))
        },
        new object[]
        {
            "'a' <-> 'b'",
            new EDBTsQueryFollowedBy(
                new EDBTsQueryLexeme("a"), 1, new EDBTsQueryLexeme("b"))
        }
    };

    [Test]
    [TestCaseSource(nameof(TsQueryTestCases))]
    public Task TsQuery(string sqlLiteral, EDBTsQuery query)
        => AssertType(query, sqlLiteral, "tsquery", EDBDbType.TsQuery);

    [Test]
    public async Task Full_text_search_not_supported_by_default_on_EDBSlimSourceBuilder()
    {
        var errorMessage = string.Format(
            EDBStrings.FullTextSearchNotEnabled,
            nameof(EDBSlimDataSourceBuilder.EnableFullTextSearch),
            nameof(EDBSlimDataSourceBuilder));

        var dataSourceBuilder = new EDBSlimDataSourceBuilder(ConnectionString);
        await using var dataSource = dataSourceBuilder.Build();

        var exception = await AssertTypeUnsupportedRead<EDBTsQuery, InvalidCastException>("a", "tsquery", dataSource);
        Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        Assert.AreEqual(errorMessage, exception.InnerException!.Message);

        exception = await AssertTypeUnsupportedWrite<EDBTsQuery, InvalidCastException>(new EDBTsQueryLexeme("a"), pgTypeName: null, dataSource);
        Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        Assert.AreEqual(errorMessage, exception.InnerException!.Message);

        exception = await AssertTypeUnsupportedRead<EDBTsVector, InvalidCastException>("1", "tsvector", dataSource);
        Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        Assert.AreEqual(errorMessage, exception.InnerException!.Message);

        exception = await AssertTypeUnsupportedWrite<EDBTsVector, InvalidCastException>(EDBTsVector.Parse("'1'"), pgTypeName: null, dataSource);
        Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        Assert.AreEqual(errorMessage, exception.InnerException!.Message);
    }

    [Test]
    public async Task EDBSlimSourceBuilder_EnableFullTextSearch()
    {
        var dataSourceBuilder = new EDBSlimDataSourceBuilder(ConnectionString);
        dataSourceBuilder.EnableFullTextSearch();
        await using var dataSource = dataSourceBuilder.Build();

        await AssertType<EDBTsQuery>(new EDBTsQueryLexeme("a"), "'a'", "tsquery", EDBDbType.TsQuery);
        await AssertType(EDBTsVector.Parse("'1'"), "'1'", "tsvector", EDBDbType.TsVector);
    }
}
