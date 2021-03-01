using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    /// <summary>
    /// Tests on PostgreSQL date/time types
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/datatype-datetime.html
    /// </remarks>
    public class DateTimeTests : MultiplexingTestBase
    {
        #region Date

        [Test]
        public async Task Date()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var dateTime = new DateTime(2002, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified);
                var EDBDate = new EDBDate(dateTime);

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn))
                {
                    var p1 = new EDBParameter("p1", EDBDbType.Date) {Value = EDBDate};
                    var p2 = new EDBParameter {ParameterName = "p2", Value = EDBDate};
                    Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Date));
                    Assert.That(p2.DbType, Is.EqualTo(DbType.Date));
                    cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            // Regular type (DateTime)
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTime)));
                            Assert.That(reader.GetDateTime(i), Is.EqualTo(dateTime));
                            Assert.That(reader.GetFieldValue<DateTime>(i), Is.EqualTo(dateTime));
                            Assert.That(reader[i], Is.EqualTo(dateTime));
                            Assert.That(reader.GetValue(i), Is.EqualTo(dateTime));

                            // Provider-specific type (EDBDate)
                            Assert.That(reader.GetDate(i), Is.EqualTo(EDBDate));
                            Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof(EDBDate)));
                            Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(EDBDate));
                            Assert.That(reader.GetFieldValue<EDBDate>(i), Is.EqualTo(EDBDate));
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
        public async Task DateSpecial(EDBDate value)
        {
            using (var conn = await OpenConnectionAsync())
            using (var cmd = new EDBCommand("SELECT @p", conn)) {
                cmd.Parameters.Add(new EDBParameter { ParameterName = "p", Value = value });
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    reader.Read();
                    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(value));
                    Assert.That(() => reader.GetDateTime(0), Throws.Exception.TypeOf<InvalidCastException>());
                }
                Assert.That(await conn.ExecuteScalarAsync("SELECT 1"), Is.EqualTo(1));
            }
        }

        [Test, Description("Makes sure that when ConvertInfinityDateTime is true, infinity values are properly converted")]
        public async Task DateConvertInfinity()
        {
            using (var conn = new EDBConnection(ConnectionString + ";ConvertInfinityDateTime=true"))
            {
                conn.Open();

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn)) {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.Date, DateTime.MaxValue);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.Date, DateTime.MinValue);
                    using (var reader = await cmd.ExecuteReaderAsync()) {
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
        public async Task Time()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expected = new TimeSpan(0, 10, 45, 34, 500);

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn))
                {
                    cmd.Parameters.Add(new EDBParameter("p1", EDBDbType.Time) {Value = expected});
                    cmd.Parameters.Add(new EDBParameter("p2", DbType.Time) {Value = expected});
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(TimeSpan)));
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
        [MonoIgnore]
        public async Task TimeTz()
        {
            using (var conn = await OpenConnectionAsync())
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
                    cmd.Parameters.AddWithValue("p1", EDBDbType.TimeTz, dto);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.TimeTz, dtUtc);
                    cmd.Parameters.AddWithValue("p3", EDBDbType.TimeTz, dtLocal);
                    cmd.Parameters.AddWithValue("p4", EDBDbType.TimeTz, dtUnspecified);
                    cmd.Parameters.AddWithValue("p5", EDBDbType.TimeTz, ts);
                    Assert.That(cmd.Parameters.All(p => p.DbType == DbType.Object));

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTimeOffset)));

                            Assert.That(reader.GetFieldValue<DateTimeOffset>(i), Is.EqualTo(new DateTimeOffset(1, 1, 2, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset)));
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTimeOffset)));
                            Assert.That(reader.GetFieldValue<DateTime>(i).Kind, Is.EqualTo(DateTimeKind.Local));
                            Assert.That(reader.GetFieldValue<DateTime>(i), Is.EqualTo(reader.GetFieldValue<DateTimeOffset>(i).LocalDateTime));
                            Assert.That(reader.GetFieldValue<TimeSpan>(i), Is.EqualTo(reader.GetFieldValue<DateTimeOffset>(i).LocalDateTime.TimeOfDay));
                        }
                    }
                }
            }
        }

        [Test]
        public async Task TimeWithTimeZoneBeforeUtcZero()
        {
            using (var conn = await OpenConnectionAsync())
            using (var cmd = new EDBCommand("SELECT TIME WITH TIME ZONE '01:00:00+02'", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                reader.Read();
                Assert.That(reader.GetFieldValue<DateTimeOffset>(0), Is.EqualTo(new DateTimeOffset(1, 1, 2, 1, 0, 0, new TimeSpan(0, 2, 0, 0))));
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
        public async Task Timestamp(DateTime dateTime)
        {
            using (var conn = await OpenConnectionAsync())
            {
                var EDBDateTime = new EDBDateTime(dateTime.Ticks);

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4, @p5, @p6", conn))
                {
                    var p1 = new EDBParameter("p1", EDBDbType.Timestamp);
                    var p2 = new EDBParameter("p2", DbType.DateTime);
                    var p3 = new EDBParameter("p3", DbType.DateTime2);
                    var p4 = new EDBParameter { ParameterName = "p4", Value = EDBDateTime };
                    var p5 = new EDBParameter { ParameterName = "p5", Value = dateTime };
                    var p6 = new EDBParameter<DateTime> { ParameterName = "p6", TypedValue = dateTime };
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
                    p1.Value = p2.Value = p3.Value = EDBDateTime;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            // Regular type (DateTime)
                            Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTime)));
                            Assert.That(reader.GetDateTime(i), Is.EqualTo(dateTime));
                            Assert.That(reader.GetDateTime(i).Kind, Is.EqualTo(DateTimeKind.Unspecified));
                            Assert.That(reader.GetFieldValue<DateTime>(i), Is.EqualTo(dateTime));
                            Assert.That(reader[i], Is.EqualTo(dateTime));
                            Assert.That(reader.GetValue(i), Is.EqualTo(dateTime));

                            // Provider-specific type (EDBTimeStamp)
                            Assert.That(reader.GetTimeStamp(i), Is.EqualTo(EDBDateTime));
                            Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof(EDBDateTime)));
                            Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(EDBDateTime));
                            Assert.That(reader.GetFieldValue<EDBDateTime>(i), Is.EqualTo(EDBDateTime));

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
        public async Task TimeStampSpecial(EDBDateTime value)
        {
            using (var conn = await OpenConnectionAsync())
            using (var cmd = new EDBCommand("SELECT @p", conn)) {
                cmd.Parameters.Add(new EDBParameter { ParameterName = "p", Value = value });
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    reader.Read();
                    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(value));
                    Assert.That(() => reader.GetDateTime(0), Throws.Exception.TypeOf<InvalidCastException>());
                }
                Assert.That(await conn.ExecuteScalarAsync("SELECT 1"), Is.EqualTo(1));
            }
        }

        [Test, Description("Makes sure that when ConvertInfinityDateTime is true, infinity values are properly converted")]
        public async Task TimeStampConvertInfinity()
        {
            using (var conn = new EDBConnection(ConnectionString + ";ConvertInfinityDateTime=true"))
            {
                conn.Open();

                using (var cmd = new EDBCommand("SELECT @p1, @p2", conn))
                {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.Timestamp, DateTime.MaxValue);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.Timestamp, DateTime.MinValue);
                    using (var reader = await cmd.ExecuteReaderAsync())
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
        public async Task TimestampTz()
        {
            using (var conn = await OpenConnectionAsync())
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

                //var dateTimeOffset = new DateTimeOffset(dateTimeLocal, dateTimeLocal - dateTimeUtc);
                var dateTimeOffset = new DateTimeOffset(dateTimeLocal);

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4, @p5, @p6, @p7", conn))
                {
                    cmd.Parameters.AddWithValue("p1", EDBDbType.TimestampTz, dateTimeUtc);
                    cmd.Parameters.AddWithValue("p2", EDBDbType.TimestampTz, dateTimeLocal);
                    cmd.Parameters.AddWithValue("p3", EDBDbType.TimestampTz, dateTimeUnspecified);
                    cmd.Parameters.AddWithValue("p4", EDBDbType.TimestampTz, nDateTimeUtc);
                    cmd.Parameters.AddWithValue("p5", EDBDbType.TimestampTz, nDateTimeLocal);
                    cmd.Parameters.AddWithValue("p6", EDBDbType.TimestampTz, nDateTimeUnspecified);
                    cmd.Parameters.AddWithValue("p7", dateTimeOffset);
                    Assert.That(cmd.Parameters["p7"].EDBDbType, Is.EqualTo(EDBDbType.TimestampTz));

                    using (var reader = await cmd.ExecuteReaderAsync())
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
                            Assert.That(reader.GetFieldValue<DateTimeOffset>(i), Is.EqualTo(dateTimeOffset));
                            var x = reader.GetFieldValue<DateTimeOffset>(i);
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
        public async Task Interval()
        {
            using (var conn = await OpenConnectionAsync())
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

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        reader.Read();

                        // Regular type (TimeSpan)
                        Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(TimeSpan)));
                        Assert.That(reader.GetTimeSpan(0), Is.EqualTo(expectedTimeSpan));
                        Assert.That(reader.GetFieldValue<TimeSpan>(0), Is.EqualTo(expectedTimeSpan));
                        Assert.That(reader[0], Is.EqualTo(expectedTimeSpan));
                        Assert.That(reader.GetValue(0), Is.EqualTo(expectedTimeSpan));

                        // Provider-specific type (EDBInterval)
                        Assert.That(reader.GetInterval(0), Is.EqualTo(expectedEDBInterval));
                        Assert.That(reader.GetProviderSpecificFieldType(0), Is.EqualTo(typeof(EDBTimeSpan)));
                        Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(expectedEDBInterval));
                        Assert.That(reader.GetFieldValue<EDBTimeSpan>(0), Is.EqualTo(expectedEDBInterval));
                    }
                }
            }
        }

        #endregion

        public DateTimeTests(MultiplexingMode multiplexingMode) : base(multiplexingMode) {}
    }
}
