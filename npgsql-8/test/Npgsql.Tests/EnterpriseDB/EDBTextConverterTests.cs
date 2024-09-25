using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    [TestFixture]
    public class EDBTextConverterTests
    {
        [Test]
        [TestCaseSource("BackendDataCases")]
        public void TestArrayParsing(string data, int numRows, int numTupleColumns)
        {
            ArrayList result = ArrayBackendToNativeTypeConverter.ToArrayList(data, null, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(numRows, result.Count);

            if (numTupleColumns == 1)
            {
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(string));
            }
            else
            {
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ArrayList));
                foreach (var subItem in result)
                {
                    var tupleArray = ((ArrayList)subItem);

                    Assert.AreEqual(numTupleColumns, tupleArray.Count);
                    CollectionAssert.AllItemsAreInstancesOfType(tupleArray, typeof(string));
                }
            }

        }

        [Test]
        [TestCaseSource("BackendDataCases")]
        public void TestFieldEnumeration(string data, int numRows, int numTupleColumns)
        {
            var result = BackendTextEnumerator.EnumerateTokens(data).ToList();

            Assert.AreEqual(numRows * numTupleColumns, result.Count);

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
    }
}

