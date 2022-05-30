using System.Collections;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    [TestFixture(MultiplexingMode.NonMultiplexing, false)]
    [TestFixture(MultiplexingMode.NonMultiplexing, true)]
    [TestFixture(MultiplexingMode.Multiplexing, false)]
    [TestFixture(MultiplexingMode.Multiplexing, true)]
    public sealed class TsQueryTests : TypeHandlerTestBase<EDBTsQuery>
    {
        public TsQueryTests(MultiplexingMode multiplexingMode, bool useTypeName) : base(
            multiplexingMode,
            useTypeName ? null : EDBDbType.TsQuery,
            useTypeName ? "tsquery" : null)
        { }

        public static IEnumerable TestCases() => new[]
        {
            new object[]
            {
                "$$'a'$$::tsquery",
                new EDBTsQueryLexeme("a")
            },
            new object[]
            {
                "$$!'a'$$::tsquery",
                new EDBTsQueryNot(
                    new EDBTsQueryLexeme("a"))
            },
            new object[]
            {
                "$$'a' | 'b'$$::tsquery",
                new EDBTsQueryOr(
                    new EDBTsQueryLexeme("a"),
                    new EDBTsQueryLexeme("b"))
            },
            new object[]
            {
                "$$'a' & 'b'$$::tsquery",
                new EDBTsQueryAnd(
                    new EDBTsQueryLexeme("a"),
                    new EDBTsQueryLexeme("b"))
            },
            new object[]
            {
                "$$'a' <-> 'b'$$::tsquery",
                new EDBTsQueryFollowedBy(
                    new EDBTsQueryLexeme("a"), 1, new EDBTsQueryLexeme("b"))
            },
            new object[]
            {
                "$$('a' & !('c' | 'd')) & (!!'a' & 'b') | 'ä' | 'x' <-> 'y' | 'x' <10> 'y' | 'd' <0> 'e' | 'f'$$::tsquery",
                new EDBTsQueryOr(
                    new EDBTsQueryOr(
                        new EDBTsQueryOr(
                            new EDBTsQueryOr(
                                new EDBTsQueryOr(
                                    new EDBTsQueryAnd(
                                        new EDBTsQueryAnd(
                                            new EDBTsQueryLexeme("a"),
                                            new EDBTsQueryNot(
                                                new EDBTsQueryOr(
                                                    new EDBTsQueryLexeme("c"),
                                                    new EDBTsQueryLexeme("d")))),
                                        new EDBTsQueryAnd(
                                            new EDBTsQueryNot(
                                                new EDBTsQueryNot(
                                                    new EDBTsQueryLexeme("a"))),
                                            new EDBTsQueryLexeme("b"))),
                                    new EDBTsQueryLexeme("ä")),
                                new EDBTsQueryFollowedBy(
                                    new EDBTsQueryLexeme("x"), 1, new EDBTsQueryLexeme("y"))),
                            new EDBTsQueryFollowedBy(
                                new EDBTsQueryLexeme("x"), 10, new EDBTsQueryLexeme("y"))),
                        new EDBTsQueryFollowedBy(
                            new EDBTsQueryLexeme("d"), 0, new EDBTsQueryLexeme("e"))),
                    new EDBTsQueryLexeme("f"))
            }
        };
    }
}
