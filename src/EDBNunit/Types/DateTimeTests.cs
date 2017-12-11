#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DOTNET
{
    /// <summary>
    /// Tests on PostgreSQL date/time types
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    class DateTimeTests
    {
        #region Date

        [Test]
        public void Date()
        {
            using (var conn = TestUtil.openDB())
            {
                var dateTime = new DateTime(2002, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified);
                var npgsqlDate = new EDBDate(dateTime);

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn))
                {
                    var p1 = new EDBParameter("p1", EDBDbType.Date) {Value = npgsqlDate};
                    var p2 = new EDBParameter("p2", DbType.Date) {Value = npgsqlDate.ToString()};
                    var p3 = new EDBParameter {ParameterName = "p3", Value = npgsqlDate};
                    Assert.That(p3.EDBDbType, Is.EqualTo(EDBDbType.Date));
                    Assert.That(p3.DbType, Is.EqualTo(DbType.Date));
                    cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2);
                    cmd.Parameters.Add(p3);
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            // Regular type (DateTime)
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (DateTime)));
                            Assert.That(reader.GetDateTime(i), Is.EqualTo(dateTime));
                            Assert.That(reader.GetFieldValue<DateTime>(i), Is.EqualTo(dateTime));
                            Assert.That(reader[i], Is.EqualTo(dateTime));
                            Assert.That(reader.GetValue(i), Is.EqualTo(dateTime));

                            // Provider-specific type (EDBDate)
                            Assert.That(reader.GetDate(i), Is.EqualTo(npgsqlDate));
                            Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof (EDBDate)));
                            Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(npgsqlDate));
                            Assert.That(reader.GetFieldValue<EDBDate>(i), Is.EqualTo(npgsqlDate));
                        }
                    }
                }
            }
        }

        static readonly TestCaseData[] DateSpecialCases = {
            new TestCaseData(EDBDate.Infinity).SetName(nameof(DateSpecial) + "Infinity"),
            new TestCaseData(EDBDate.NegativeInfinity).SetName(nameof(DateSpecial) + "NegativeInfinity"),
            new TestCaseData(new EDBDate(-5, 3, 3)).SetName(nameof(DateSpecial) +"BC"),
        };

        [Test, TestCaseSource(nameof(DateSpecialCases))]
        public void DateSpecial(EDBDate value)
        {
            using (var conn = TestUtil.openDB())
            using (var cmd = new EDBCommand("SELECT @p", conn)) {
                cmd.Parameters.Add(new EDBParameter { ParameterName = "p", Value = value });
                using (var reader = cmd.ExecuteReader()) {
                    reader.Read();
                    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(value));
                    Assert.That(() => reader.GetDateTime(0), Throws.Exception.TypeOf<InvalidCastException>());
                }
                Assert.That(conn.ExecuteScalar("SELECT 1"), Is.EqualTo(1));
            }
        }

        [Test, Description("Makes sure that when ConvertInfinityDateTime is true, infinity values are properly converted")]
        public void DateConvertInfinity()
        {
            using (var conn = new EDBConnection(TestUtil.defaultConnectionString + ";ConvertInfinityDateTime=true"))
            {
                conn.Open();

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn)) {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.Date, DateTime.MaxValue);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.Date, DateTime.MinValue);
                    using (var reader = cmd.ExecuteReader()) {
                        reader.Read();
                        Assert.That(reader.GetFieldValue<EDBDate>(0), Is.EqualTo(EDBDate.Infinity));
                        Assert.That(reader.GetFieldValue<EDBDate>(1), Is.EqualTo(EDBDate.NegativeInfinity));
                        Assert.That(reader.GetDateTime(0), Is.EqualTo(DateTime.MaxValue));
                        Assert.That(reader.GetDateTime(1), Is.EqualTo(DateTime.MinValue));
                    }
                }
            }
        }

        #endregion

        #region Time

        [Test]
        public void Time()
        {
            using (var conn = TestUtil.openDB())
            {
                var expected = new TimeSpan(0, 10, 45, 34, 500);

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn))
                {
                    cmd.Parameters.Add(new EDBParameter("p1", EDBDbType.Time) {Value = expected});
                    cmd.Parameters.Add(new EDBParameter("p2", DbType.Time) {Value = expected});
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (TimeSpan)));
                            Assert.That(reader.GetTimeSpan(i), Is.EqualTo(expected));
                            Assert.That(reader.GetFieldValue<TimeSpan>(i), Is.EqualTo(expected));
                            Assert.That(reader[i], Is.EqualTo(expected));
                            Assert.That(reader.GetValue(i), Is.EqualTo(expected));
                        }
                    }
                }
            }
        }

        #endregion

        #region Time with timezone

        [Test]
       // [MonoIgnore]
        public void TimeTz()
        {
            using (var conn = TestUtil.openDB())
            {
                var tzOffset = TimeZoneInfo.Local.BaseUtcOffset;
                if (tzOffset == TimeSpan.Zero)
                    Assert.Ignore("Test cannot run when machine timezone is UTC");

                // Note that the date component of the below is ignored
                var dto = new DateTimeOffset(5, 5, 5, 13, 3, 45, 510, tzOffset);
                var dtUtc = new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, DateTimeKind.Utc) - tzOffset;
                var dtLocal = new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, DateTimeKind.Local);
                var dtUnspecified = new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, DateTimeKind.Unspecified);
                var ts = dto.TimeOfDay;

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4, @p5", conn))
                {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.TimeTZ, dto);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.TimeTZ, dtUtc);
                    cmd.Parameters.AddWithValue("p3", EDBDbType.TimeTZ, dtLocal);
                    cmd.Parameters.AddWithValue("p4", EDBDbType.TimeTZ, dtUnspecified);
                    cmd.Parameters.AddWithValue("p5", EDBDbType.TimeTZ, ts);
                    Assert.That(cmd.Parameters.All(p => p.DbType == DbType.Object));

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTimeOffset)));

                            Assert.That(reader.GetFieldValue<DateTimeOffset>(i), Is.EqualTo(new DateTimeOffset(1, 1, 1, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset)));
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTimeOffset)));
                            Assert.That(reader.GetFieldValue<DateTime>(i).Kind, Is.EqualTo(DateTimeKind.Local));
                            Assert.That(reader.GetFieldValue<DateTime>(i), Is.EqualTo(reader.GetFieldValue<DateTimeOffset>(i).LocalDateTime));
                            Assert.That(reader.GetFieldValue<TimeSpan>(i), Is.EqualTo(reader.GetFieldValue<DateTimeOffset>(i).LocalDateTime.TimeOfDay));
                        }
                    }
                }
            }
        }

        #endregion

        #region Timestamp

        static readonly TestCaseData[] TimeStampCases = {
            new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38)).SetName(nameof(Timestamp) + "Pre2000"),
            new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345)).SetName(nameof(Timestamp) + "Post2000"),
            new TestCaseData(new DateTime(2013, 7, 25)).SetName(nameof(Timestamp) + "DateOnly"),
        };

        [Test, TestCaseSource(nameof(TimeStampCases))]
        public void Timestamp(DateTime dateTime)
        {
            using (var conn = TestUtil.openDB())
            {
                var npgsqlTimeStamp = new EDBDateTime(dateTime.Ticks);
                var offset = TimeSpan.FromHours(2);
                var dateTimeOffset = new DateTimeOffset(dateTime, offset);

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4, @p5, @p6", conn))
                {
                    var p1 = new EDBParameter("p1", EDBDbType.Timestamp);
                    var p2 = new EDBParameter("p2", DbType.DateTime);
                    var p3 = new EDBParameter("p3", DbType.DateTime2);
                    var p4 = new EDBParameter { ParameterName = "p4", Value = npgsqlTimeStamp };
                    var p5 = new EDBParameter { ParameterName = "p5", Value = dateTime };
                    var p6 = new EDBParameter("p6", EDBDbType.Timestamp);
                    Assert.That(p4.EDBDbType, Is.EqualTo(EDBDbType.Timestamp));
                    Assert.That(p4.DbType, Is.EqualTo(DbType.DateTime));
                    Assert.That(p5.EDBDbType, Is.EqualTo(EDBDbType.Timestamp));
                    Assert.That(p5.DbType, Is.EqualTo(DbType.DateTime));
                    cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2);
                    cmd.Parameters.Add(p3);
                    cmd.Parameters.Add(p4);
                    cmd.Parameters.Add(p5);
                    cmd.Parameters.Add(p6);
                    p1.Value = p2.Value = p3.Value = npgsqlTimeStamp;
                    p6.Value = dateTimeOffset;
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            // Regular type (DateTime)
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (DateTime)));
                            Assert.That(reader.GetDateTime(i), Is.EqualTo(dateTime));
                            Assert.That(reader.GetDateTime(i).Kind, Is.EqualTo(DateTimeKind.Unspecified));
                            Assert.That(reader.GetFieldValue<DateTime>(i), Is.EqualTo(dateTime));
                            Assert.That(reader[i], Is.EqualTo(dateTime));
                            Assert.That(reader.GetValue(i), Is.EqualTo(dateTime));

                            // Provider-specific type (EDBTimeStamp)
                            Assert.That(reader.GetTimeStamp(i), Is.EqualTo(npgsqlTimeStamp));
                            Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof (EDBDateTime)));
                            Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(npgsqlTimeStamp));
                            Assert.That(reader.GetFieldValue<EDBDateTime>(i), Is.EqualTo(npgsqlTimeStamp));

                            // DateTimeOffset
                            Assert.That(() => reader.GetFieldValue<DateTimeOffset>(i), Throws.Exception.TypeOf<InvalidCastException>());
                        }
                    }
                }
            }
        }

        static readonly TestCaseData[] TimeStampSpecialCases = {
            new TestCaseData(EDBDateTime.Infinity).SetName(nameof(TimeStampSpecial) + "Infinity"),
            new TestCaseData(EDBDateTime.NegativeInfinity).SetName(nameof(TimeStampSpecial) + "NegativeInfinity"),
            new TestCaseData(new EDBDateTime(-5, 3, 3, 1, 0, 0)).SetName(nameof(TimeStampSpecial) + "BC"),
        };

        [Test, TestCaseSource(nameof(TimeStampSpecialCases))]
        public void TimeStampSpecial(EDBDateTime value)
        {
            using (var conn = TestUtil.openDB())
            using (var cmd = new EDBCommand("SELECT @p", conn)) {
                cmd.Parameters.Add(new EDBParameter { ParameterName = "p", Value = value });
                using (var reader = cmd.ExecuteReader()) {
                    reader.Read();
                    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(value));
                    Assert.That(() => reader.GetDateTime(0), Throws.Exception.TypeOf<InvalidCastException>());
                }
                Assert.That(conn.ExecuteScalar("SELECT 1"), Is.EqualTo(1));
            }
        }

        [Test, Description("Makes sure that when ConvertInfinityDateTime is true, infinity values are properly converted")]
        public void TimeStampConvertInfinity()
        {
            using (var conn = new EDBConnection(TestUtil.defaultConnectionString + ";ConvertInfinityDateTime=true"))
            {
                conn.Open();

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn))
                {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.Timestamp, DateTime.MaxValue);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.Timestamp, DateTime.MinValue);
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        Assert.That(reader.GetFieldValue<EDBDateTime>(0), Is.EqualTo(EDBDateTime.Infinity));
                        Assert.That(reader.GetFieldValue<EDBDateTime>(1), Is.EqualTo(EDBDateTime.NegativeInfinity));
                        Assert.That(reader.GetDateTime(0), Is.EqualTo(DateTime.MaxValue));
                        Assert.That(reader.GetDateTime(1), Is.EqualTo(DateTime.MinValue));
                    }
                }
            }
        }

        #endregion

        #region Timestamp with timezone

        [Test]
        public void TimestampTz()
        {
            using (var conn = TestUtil.openDB())
            {
                var tzOffset = TimeZoneInfo.Local.BaseUtcOffset;
                if (tzOffset == TimeSpan.Zero)
                    Assert.Ignore("Test cannot run when machine timezone is UTC");

                var dateTimeUtc = new DateTime(2015, 6, 27, 8, 45, 12, 345, DateTimeKind.Utc);
                var dateTimeLocal = dateTimeUtc.ToLocalTime();
                var dateTimeUnspecified = new DateTime(dateTimeUtc.Ticks, DateTimeKind.Unspecified);

                var nDateTimeUtc = new EDBDateTime(dateTimeUtc);
                var nDateTimeLocal = nDateTimeUtc.ToLocalTime();
                var nDateTimeUnspecified = new EDBDateTime(nDateTimeUtc.Ticks, DateTimeKind.Unspecified);

                var dateTimeOffset = new DateTimeOffset(dateTimeLocal, dateTimeLocal - dateTimeUtc);

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9", conn))
                {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.TimestampTZ, dateTimeUtc);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.TimestampTZ, dateTimeLocal);
                    cmd.Parameters.AddWithValue("p3", EDBDbType.TimestampTZ, dateTimeUnspecified);
                    cmd.Parameters.AddWithValue("p4", EDBDbType.TimestampTZ, nDateTimeUtc);
                    cmd.Parameters.AddWithValue("p5", EDBDbType.TimestampTZ, nDateTimeLocal);
                    cmd.Parameters.AddWithValue("p6", EDBDbType.TimestampTZ, nDateTimeUnspecified);
                    cmd.Parameters.AddWithValue("p7", dateTimeUtc);
                    Assert.That(cmd.Parameters["p7"].EDBDbType, Is.EqualTo(EDBDbType.TimestampTZ));
                    cmd.Parameters.AddWithValue("p8", nDateTimeUtc);
                    Assert.That(cmd.Parameters["p8"].EDBDbType, Is.EqualTo(EDBDbType.TimestampTZ));
                    cmd.Parameters.AddWithValue("p9", dateTimeOffset);
                    Assert.That(cmd.Parameters["p9"].EDBDbType, Is.EqualTo(EDBDbType.TimestampTZ));

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            // Regular type (DateTime)
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTime)));
                            Assert.That(reader.GetDateTime(i), Is.EqualTo(dateTimeLocal));
                            Assert.That(reader.GetFieldValue<DateTime>(i).Kind, Is.EqualTo(DateTimeKind.Local));
                            Assert.That(reader[i], Is.EqualTo(dateTimeLocal));
                            Assert.That(reader.GetValue(i), Is.EqualTo(dateTimeLocal));

                            // Provider-specific type (EDBDateTime)
                            Assert.That(reader.GetTimeStamp(i), Is.EqualTo(nDateTimeLocal));
                            Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof(EDBDateTime)));
                            Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(nDateTimeLocal));
                            Assert.That(reader.GetFieldValue<EDBDateTime>(i), Is.EqualTo(nDateTimeLocal));

                            // DateTimeOffset
                            Assert.That(reader.GetFieldValue<DateTimeOffset>(i), Is.EqualTo(dateTimeOffset.ToUniversalTime()));
                        }
                    }
                }

                Assert.AreEqual(nDateTimeUtc, nDateTimeLocal.ToUniversalTime());
                Assert.AreEqual(nDateTimeUtc, new EDBDateTime(nDateTimeLocal.Ticks, DateTimeKind.Unspecified).ToUniversalTime());
                Assert.AreEqual(nDateTimeLocal, nDateTimeUnspecified.ToLocalTime());
            }
        }

        #endregion

        #region Interval

        [Test]
        public void Interval()
        {
            using (var conn = TestUtil.openDB())
            {
                var expectedEDBInterval = new EDBTimeSpan(1, 2, 3, 4, 5);
                var expectedTimeSpan = new TimeSpan(1, 2, 3, 4, 5);

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn))
                {
                    var p1 = new EDBParameter("p1", EDBDbType.Interval);
                    var p2 = new EDBParameter("p2", expectedTimeSpan);
                    var p3 = new EDBParameter("p3", expectedEDBInterval);
                    Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Interval));
                    Assert.That(p2.DbType, Is.EqualTo(DbType.Object));
                    Assert.That(p3.EDBDbType, Is.EqualTo(EDBDbType.Interval));
                    Assert.That(p3.DbType, Is.EqualTo(DbType.Object));
                    cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2);
                    cmd.Parameters.Add(p3);
                    p1.Value = expectedEDBInterval;

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        // Regular type (TimeSpan)
                        Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof (TimeSpan)));
                        Assert.That(reader.GetTimeSpan(0), Is.EqualTo(expectedTimeSpan));
                        Assert.That(reader.GetFieldValue<TimeSpan>(0), Is.EqualTo(expectedTimeSpan));
                        Assert.That(reader[0], Is.EqualTo(expectedTimeSpan));
                        Assert.That(reader.GetValue(0), Is.EqualTo(expectedTimeSpan));

                        // Provider-specific type (EDBInterval)
                        Assert.That(reader.GetInterval(0), Is.EqualTo(expectedEDBInterval));
                        Assert.That(reader.GetProviderSpecificFieldType(0), Is.EqualTo(typeof (EDBTimeSpan)));
                        Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(expectedEDBInterval));
                        Assert.That(reader.GetFieldValue<EDBTimeSpan>(0), Is.EqualTo(expectedEDBInterval));
                    }
                }
            }
        }

        #endregion
    }
}
