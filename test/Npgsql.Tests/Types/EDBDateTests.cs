using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using EDBTypes;


namespace EnterpriseDB.EDBClient.Tests.Types
{
    /// <summary>
    /// Summary description for DateTest.
    /// </summary>

    [TestFixture]
    public class DateTest : TestBase
    {
        EDBConnection? con;

        [SetUp]
        public void Init()
        {
            con = OpenConnection();
            TestUtil.createTempTable(con, "testdate", "dt date");
        }

        [TearDown]
        public void Dispose()
        {
            TestUtil.dropTable(con, "testdate");
            TestUtil.closeDB(con);
        }

        [Test]
        public void testGetDate()
        {

            EDBCommand Command = new EDBCommand("", con);

            //Statement stmt = con.createStatement();
            Command.CommandText = "INSERT INTO testdate values('1950-02-07')";
            int a = Command.ExecuteNonQuery();
            Assert.AreEqual(1, a);

            Command.CommandText = "INSERT INTO testdate values('1970-06-02')";
            a = Command.ExecuteNonQuery();
            Assert.AreEqual(1, a);

            Command.CommandText = "INSERT INTO testdate values('1999-08-11')";
            a = Command.ExecuteNonQuery();
            Assert.AreEqual(1, a);

            Command.CommandText = "INSERT INTO testdate values('2001-02-13')";
            a = Command.ExecuteNonQuery();
            Assert.AreEqual(1, a);

            Command.CommandText = "INSERT INTO testdate values('1950-04-02')";
            a = Command.ExecuteNonQuery();
            Assert.AreEqual(1, a);

            Command.CommandText = "INSERT INTO testdate values('1934-02-28')";
            a = Command.ExecuteNonQuery();
            Assert.AreEqual(1, a);

            Command.CommandText = "DELETE FROM " + "testdate";
            a = Command.ExecuteNonQuery();
            Assert.AreEqual(6, a);
        }

        [Test]
        public void ParseStringTest()
        {
            EDBDate date = EDBDate.Parse("2019-11-01");
            Assert.AreEqual(2019, date.Year);
            Assert.AreEqual(11, date.Month);
            Assert.AreEqual(1, date.Day);

            try
            {
                EDBDate.Parse("No Date");
                Assert.Fail("Exception was expected with invalid date format");
            }
            catch (FormatException)
            {
            }

            EDBDate newDate = new EDBDate();
            Assert.True(EDBDate.TryParse("2019-11-01", out newDate));
            Assert.True(EDBDate.TryParse("2017-11-01", out newDate));

            Assert.False(EDBDate.TryParse("11-01", out newDate));
            Assert.False(EDBDate.TryParse("Total Random String", out newDate));

            Assert.True(EDBDate.Infinity == EDBDate.Parse("infinity"));
            Assert.True(EDBDate.NegativeInfinity == EDBDate.Parse("-infinity"));
        }

        [Test]
        public void ArithmeticTest()
        {
            EDBDate date = new EDBDate(2019, 11, 1);
            Assert.AreEqual("2019-11-01", date.Add(new EDBTimeSpan(0, 0, 0)).ToString());
            Assert.AreEqual("2019-12-01", date.Add(new EDBTimeSpan(1, 0, 0)).ToString());
            Assert.AreEqual("2019-12-03", date.Add(new EDBTimeSpan(1, 2, 0)).ToString());
            Assert.AreEqual("2020-01-01", date.Add(new EDBTimeSpan(2, 0, 0)).ToString());
            Assert.AreEqual("2020-01-01", date.Add(new EDBTimeSpan(1, 31, 0)).ToString());

            Assert.AreEqual("2020-01-01", date.AddDays(61).ToString());
            Assert.AreEqual("2020-11-01", date.AddYears(1).ToString());
            Assert.AreEqual("2019-12-01", date.AddMonths(1).ToString());

            EDBDate date2 = new EDBDate(2012, 2, 1);
            Assert.AreEqual("2012-02-01", date2.AddDays(0).ToString());
            Assert.AreEqual("2012-02-29", date2.AddDays(28).ToString());
            Assert.AreEqual("2012-03-01", date2.AddDays(29).ToString());

            Assert.AreEqual("2019-12-01", (date + new EDBTimeSpan(1, 0, 0)).ToString());
            Assert.AreEqual("2019-10-01", (date - new EDBTimeSpan(1, 0, 0)).ToString());
        }

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("2019-12-01", new EDBDate(2019, 12, 1).ToString());
            Assert.AreEqual("2019-12-12", new EDBDate(2019, 12, 12).ToString());

            Assert.AreEqual("infinity", EDBDate.Infinity.ToString());
            Assert.AreEqual("-infinity", EDBDate.NegativeInfinity.ToString());
        }

        [Test]
        public void EqualTest()
        {
            EDBDate date1 = new EDBDate(2019, 12, 1);
            EDBDate date2 = new EDBDate(2019, 12, 1);
            EDBDate date3 = new EDBDate(2019, 11, 1);

            Assert.AreEqual(date1, date2);
            Assert.True(date1 == date2);
            Assert.True(date1 != date3);
            Assert.True(date1.Equals(date2));

            Assert.AreNotEqual(date1, date3);
            Assert.False(date1.Equals(date3));

            Assert.AreEqual(0, date1.CompareTo(date2));
            Assert.AreEqual(0, date1.Compare(date1, date2));
        }

        [Test]
        public void ComparisonTest()
        {
            EDBDate date1 = new EDBDate(2019, 12, 1);
            EDBDate date2 = new EDBDate(2019, 12, 1);
            EDBDate date3 = new EDBDate(2019, 11, 1);

            Assert.True(date1 >= date2);
            Assert.True(date1 <= date2);

            Assert.True(date1 > date3);
            Assert.True(date1 >= date3);

            Assert.True(date3 < date1);
            Assert.True(date3 <= date1);
        }

        [Test]
        public void IsItToday()
        {
            Assert.True(EDBDate.Today > EDBDate.Yesterday);
            Assert.True(EDBDate.Today < EDBDate.Tomorrow);
        }

        [Test]
        public void InfinityTest()
        {
            EDBDate infinity = EDBDate.Infinity;
            Assert.True(infinity == infinity.Add(new EDBTimeSpan(1,0,0)) );
            Assert.True(infinity == infinity.AddYears(1));
            Assert.True(infinity == infinity.AddMonths(1));
            Assert.True(infinity == infinity.AddDays(1));
        }

        [Test]
        public void NagetiveInfinityTest()
        {
            EDBDate nInfinity = EDBDate.NegativeInfinity;
            Assert.True(nInfinity == nInfinity.Add(new EDBTimeSpan(1, 0, 0)));
            Assert.True(nInfinity == nInfinity.AddYears(1));
            Assert.True(nInfinity == nInfinity.AddMonths(1));
            Assert.True(nInfinity == nInfinity.AddDays(1));
        }
    }
}
