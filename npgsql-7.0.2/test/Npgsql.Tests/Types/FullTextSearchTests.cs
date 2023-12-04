using System.Collections;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

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
}
