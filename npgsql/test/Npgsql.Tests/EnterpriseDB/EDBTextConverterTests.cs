using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

[TestFixture]
public class EDBTextConverterTests
{
    [Test]
    [TestCaseSource(nameof(BackendDataCases))]
    public void TestArrayParsing(string data, int numRows, int numTupleColumns)
    {
        var result = ArrayBackendToNativeTypeConverter.ToList(data, null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(numRows));

        if (numTupleColumns == 1)
        {
            Assert.That(result, Is.All.InstanceOf<string>());
        }
        else
        {
            Assert.That(result, Is.All.InstanceOf<List<object>>());
            foreach (var subItem in result)
            {
                var tupleArray = ((List<object>)subItem);

                Assert.That(tupleArray.Count, Is.EqualTo(numTupleColumns));
                Assert.That(tupleArray, Is.All.InstanceOf<string>());
            }
        }

    }

    [Test]
    [TestCaseSource(nameof(BackendDataCases))]
    public void TestFieldEnumeration(string data, int numRows, int numTupleColumns)
    {
        var result = BackendTextEnumerator.EnumerateTokens(data).ToList();

        Assert.That(result.Count, Is.EqualTo(numRows * numTupleColumns));

    }

    [Test]
    [TestCaseSource(nameof(BackendMoneyCases))]
    public void MoneyParsingTests(string data, decimal expected)
    {
        var value = StringToNativeConverter.ParseMoney(data);
        Assert.That(value, Is.EqualTo(expected));

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            var localized = expected.ToString(culture);
            value = StringToNativeConverter.ParseMoney(localized);
            Assert.That(value, Is.EqualTo(expected));
        }

        expected *= -1;

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            var localized = expected.ToString(culture);
            value = StringToNativeConverter.ParseMoney(localized);
            Assert.That(value, Is.EqualTo(expected));
        }
    }


    // Test case : backend data as test, element count (rows), and num tup
    private static IEnumerable<TestCaseData> BackendDataCases()
    {
        yield return new TestCaseData("{10,20,30,40,-1,0}"
                                    , 6, 1);
        yield return new TestCaseData("{\"(ACCOUNTING,\\\"NEW YORK\\\")\",\"(OPERATIONS,BOSTON)\",\"(RESEARCH,DALLAS)\",\"(SALES,CHICAGO)\"}"
                                    , 4, 2);
        yield return new TestCaseData("{\"(ACCOUNTING,\\\"NEW YORK\\\")\",\"(OPERATIONS,BOSTON)\",\"(RESEARCH,DALLAS)\",\"(SALES,CHICAGO)\"}"
                                    , 4, 2);
        yield return new TestCaseData("{}"
                                    , 0, 1);
        yield return new TestCaseData("{\"(ACCOUNTING,1,\\\"NEW YORK\\\")\",\"(OPERATIONS,2,BOSTON)\",\"(RESEARCH,3,DALLAS)\",\"(SALES,4,CHICAGO)\"}"
                                    , 4, 3);
        yield return new TestCaseData("{\"(7369,SMITH)\",\"(7499,ALLEN)\",\"(7521,WARD)\",\"(7566,JONES)\",\"(7654,MARTIN)\",\"(7698,BLAKE)\",\"(7782,CLARK)\",\"(7788,SCOTT)\",\"(7839,KING)\",\"(7844,TURNER)\"}"
                                    , 10, 2);
    }

    // Test case : backend data as test, element count (rows), and num tup
    private static IEnumerable<TestCaseData> BackendMoneyCases()
    {
        yield return new TestCaseData("12,30 €", 12.3m );
        yield return new TestCaseData("$ -5650.06", -5650.06m);
        yield return new TestCaseData("Rs0.00", 0m);
    }
}

