using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static EnterpriseDB.EDBClient.Tests.TestUtil;

#pragma warning disable 618 // EDBDateTime, EDBDate and EDBTimespan are obsolete, remove in 7.0

namespace EnterpriseDB.EDBClient.Tests.Types
{
    // Since this test suite manipulates TimeZone, it is incompatible with multiplexing
    public class DateTimeTests : TestBase
    {
        #region Date

        [Test]
        public async Task Date()
        {
            using var conn = await OpenConnectionAsync();
            var dateTime = new DateTime(2002, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified);
            var npgsqlDate = new EDBDate(dateTime);

            using var cmd = new EDBCommand("SELECT @p1, @p2", conn);
            var p1 = new EDBParameter("p1", EDBDbType.Date) {Value = npgsqlDate};
            var p2 = new EDBParameter {ParameterName = "p2", Value = npgsqlDate};
            Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Date));
            Assert.That(p2.DbType, Is.EqualTo(DbType.Date));
            cmd.Parameters.Add(p1);
            cmd.Parameters.Add(p2);
            using var reader = await cmd.ExecuteReaderAsync();
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
                Assert.That(reader.GetDate(i), Is.EqualTo(npgsqlDate));
                Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof(EDBDate)));
                Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(npgsqlDate));
                Assert.That(reader.GetFieldValue<EDBDate>(i), Is.EqualTo(npgsqlDate));

                // Internal PostgreSQL representation, for out-of-range values.
                Assert.That(() => reader.GetInt32(0), Throws.Nothing);
            }
        }

#if NET6_0_OR_GREATER
        [Test]
        public async Task Date_DateOnly()
        {
            using var conn = await OpenConnectionAsync();
            var dateOnly = new DateOnly(2002, 3, 4);
            var dateTime = dateOnly.ToDateTime(default);

            using var cmd = new EDBCommand("SELECT @p1", conn);
            var p1 = new EDBParameter { ParameterName = "p1", Value = dateOnly };
            Assert.That(p1.EDBDbType, Is.EqualTo(EDBDbType.Date));
            Assert.That(p1.DbType, Is.EqualTo(DbType.Date));
            cmd.Parameters.Add(p1);

            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read();

            Assert.That(reader.GetFieldValue<DateOnly>(0), Is.EqualTo(dateOnly));
            Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(DateTime)));
            Assert.That(reader.GetDateTime(0), Is.EqualTo(dateTime));
            Assert.That(reader[0], Is.EqualTo(dateTime));
            Assert.That(reader.GetValue(0), Is.EqualTo(dateTime));
        }

        [Test]
        public async Task Date_DateOnly_range()
        {
            using var conn = await OpenConnectionAsync();
            var range = new EDBRange<DateOnly>(new(2002, 3, 4), true, new(2002, 3, 6), false);

            using var cmd = new EDBCommand("SELECT @p1", conn);
            var p1 = new EDBParameter { ParameterName = "p1", Value = range };
            Assert.That(p1.EDBDbType, Is.EqualTo(EDBDbType.DateRange));
            Assert.That(p1.DbType, Is.EqualTo(DbType.Object));
            cmd.Parameters.Add(p1);

            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read();

            Assert.That(reader.GetFieldValue<EDBRange<DateOnly>>(0), Is.EqualTo(range));
        }
#endif

        #endregion

        #region Time

        [Test]
        public async Task Time()
        {
            using var conn = await OpenConnectionAsync();
            var expected = new TimeSpan(0, 10, 45, 34, 500);

            using var cmd = new EDBCommand("SELECT @p1, @p2", conn);
            cmd.Parameters.Add(new EDBParameter("p1", EDBDbType.Time) {Value = expected});
            cmd.Parameters.Add(new EDBParameter("p2", DbType.Time) {Value = expected});
            using var reader = await cmd.ExecuteReaderAsync();
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

#if NET6_0_OR_GREATER
        [Test]
        public async Task Time_TimeOnly()
        {
            using var conn = await OpenConnectionAsync();
            var timeOnly = new TimeOnly(10, 45, 34, 500);
            var timeSpan = timeOnly.ToTimeSpan();

            using var cmd = new EDBCommand("SELECT @p1", conn);
            cmd.Parameters.Add(new EDBParameter { ParameterName = "p1", Value = timeOnly });

            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read();

            Assert.That(reader.GetFieldValue<TimeOnly>(0), Is.EqualTo(timeOnly));
            Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(TimeSpan)));
            Assert.That(reader.GetTimeSpan(0), Is.EqualTo(timeSpan));
            Assert.That(reader.GetFieldValue<TimeSpan>(0), Is.EqualTo(timeSpan));
            Assert.That(reader[0], Is.EqualTo(timeSpan));
            Assert.That(reader.GetValue(0), Is.EqualTo(timeSpan));
        }
#endif

        #endregion

        #region Time with timezone

        [Test]
        [MonoIgnore]
        public async Task TimeTz()
        {
            using var conn = await OpenConnectionAsync();
            var tzOffset = TimeZoneInfo.Local.BaseUtcOffset;
            if (tzOffset == TimeSpan.Zero)
                Assert.Ignore("Test cannot run when machine timezone is UTC");

            // Note that the date component of the below is ignored
            var dto = new DateTimeOffset(5, 5, 5, 13, 3, 45, 510, tzOffset);

            using var cmd = new EDBCommand("SELECT @p", conn);
            cmd.Parameters.AddWithValue("p", EDBDbType.TimeTz, dto);
            Assert.That(cmd.Parameters.All(p => p.DbType == DbType.Object));

            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read();

            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTimeOffset)));
                Assert.That(reader.GetFieldValue<DateTimeOffset>(i), Is.EqualTo(new DateTimeOffset(1, 1, 2, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset)));
                Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(DateTimeOffset)));
            }
        }

        [Test]
        public async Task TimeTz_before_utc_zero()
        {
            using var conn = await OpenConnectionAsync();
            using var cmd = new EDBCommand("SELECT TIME WITH TIME ZONE '01:00:00+02'", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read();
            Assert.That(reader.GetFieldValue<DateTimeOffset>(0), Is.EqualTo(new DateTimeOffset(1, 1, 2, 1, 0, 0, new TimeSpan(0, 2, 0, 0))));
        }

        #endregion

        #region Timestamp

        static readonly TestCaseData[] TimestampValues =
        {
            new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified), "1998-04-12 13:26:38")
                .SetName("TimestampPre2000"),
            new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Unspecified), "2015-01-27 08:45:12.345")
                .SetName("TimestampPost2000"),
            new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Unspecified), "2013-07-25 00:00:00")
                .SetName("TimestampDateOnly")
        };

        [Test, TestCaseSource(nameof(TimestampValues))]
        public async Task Timestamp_read(DateTime dateTime, string s)
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand($"SELECT '{s}'::timestamp without time zone", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            Assert.That(reader.GetDataTypeName(0), Is.EqualTo("timestamp without time zone"));
            Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(DateTime)));

            Assert.That(reader[0], Is.EqualTo(dateTime));
            Assert.That(reader.GetDateTime(0), Is.EqualTo(dateTime));
            Assert.That(reader.GetDateTime(0).Kind, Is.EqualTo(DateTimeKind.Unspecified));
            Assert.That(reader.GetFieldValue<DateTime>(0), Is.EqualTo(dateTime));

            // Provider-specific type (EDBTimeStamp)
            var npgsqlDateTime = new EDBDateTime(dateTime);
            Assert.That(reader.GetProviderSpecificFieldType(0), Is.EqualTo(typeof(EDBDateTime)));
            Assert.That(reader.GetTimeStamp(0), Is.EqualTo(npgsqlDateTime));
            Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(npgsqlDateTime));
            Assert.That(reader.GetFieldValue<EDBDateTime>(0), Is.EqualTo(npgsqlDateTime));

            // DateTimeOffset
            Assert.That(() => reader.GetFieldValue<DateTimeOffset>(0), Throws.Exception.TypeOf<InvalidCastException>());

            // Internal PostgreSQL representation, for out-of-range values.
            Assert.That(() => reader.GetInt64(0), Throws.Nothing);
        }

        [Test, TestCaseSource(nameof(TimestampValues))]
        public async Task Timestamp_write_values(DateTime dateTime, string expected)
        {
            Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Unspecified));

            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT $1::text", conn)
            {
                Parameters =
                {
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), EDBDbType = EDBDbType.Timestamp }
                }
            };

            Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(expected));
        }

        static EDBParameter[] TimestampParameters
        {
            get
            {
                var dateTime = new DateTime(1998, 4, 12, 13, 26, 38);

                return new EDBParameter[]
                {
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified) },
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Local) },
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Local), EDBDbType = EDBDbType.Timestamp },
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Local), DbType = DbType.DateTime2 },
                    new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Unspecified) },
                    new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Local) },
                    new() { Value = -54297202000000L, EDBDbType = EDBDbType.Timestamp }
                };
            }
        }

        [Test, TestCaseSource(nameof(TimestampParameters))]
        public async Task Timestamp_resolution(EDBParameter parameter)
        {
            await using var conn = await OpenConnectionAsync();
            conn.TypeMapper.Reset();

            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters = { parameter }
            };

            Assert.That(parameter.EDBDbType, Is.EqualTo(EDBDbType.Timestamp));
            Assert.That(parameter.DbType, Is.EqualTo(DbType.DateTime).Or.EqualTo(DbType.DateTime2));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("timestamp without time zone"));
            Assert.That(reader[1], Is.EqualTo("1998-04-12 13:26:38"));
        }

        static EDBParameter[] TimestampInvalidParameters
            => new EDBParameter[]
            {
                new() { Value = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc), EDBDbType = EDBDbType.Timestamp },
                new() { Value = new EDBDateTime(0, DateTimeKind.Utc), EDBDbType = EDBDbType.Timestamp },
                new() { Value = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), EDBDbType = EDBDbType.Timestamp }
            };

        [Test, TestCaseSource(nameof(TimestampInvalidParameters))]
        public async Task Timestamp_resolution_failure(EDBParameter parameter)
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT $1::text", conn)
            {
                Parameters = { parameter }
            };

            Assert.That(() => cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<InvalidCastException>());
        }

        [Test]
        public async Task Timestamp_array_resolution()
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters = { new() { Value = new[] { new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local) } } }
            };

            Assert.That(cmd.Parameters[0].DataTypeName, Is.EqualTo("timestamp without time zone[]"));
            Assert.That(cmd.Parameters[0].EDBDbType, Is.EqualTo(EDBDbType.Array | EDBDbType.Timestamp));
            Assert.That(cmd.Parameters[0].DbType, Is.EqualTo(DbType.Object));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("timestamp without time zone[]"));
            Assert.That(reader[1], Is.EqualTo(@"{""1998-04-12 13:26:38""}"));
        }

        [Test]
        public async Task Timestamp_range_resolution()
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new EDBRange<DateTime>(
                            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
                            new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local))
                    }
                }
            };

            Assert.That(cmd.Parameters[0].DataTypeName, Is.EqualTo("tsrange"));
            Assert.That(cmd.Parameters[0].EDBDbType, Is.EqualTo(EDBDbType.Range | EDBDbType.Timestamp));
            Assert.That(cmd.Parameters[0].DbType, Is.EqualTo(DbType.Object));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("tsrange"));
            Assert.That(reader[1], Is.EqualTo(@"[""1998-04-12 13:26:38"",""1998-04-12 15:26:38""]"));
        }

        [Test]
        public async Task Timestamp_multirange_resolution()
        {
            await using var conn = await OpenConnectionAsync();
            MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new[]
                        {
                            new EDBRange<DateTime>(
                                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
                                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local)),
                            new EDBRange<DateTime>(
                                new DateTime(1998, 4, 13, 13, 26, 38, DateTimeKind.Local),
                                new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Local)),
                        }
                    }
                }
            };

            Assert.That(cmd.Parameters[0].DataTypeName, Is.EqualTo("tsmultirange"));
            Assert.That(cmd.Parameters[0].EDBDbType, Is.EqualTo(EDBDbType.Multirange | EDBDbType.Timestamp));
            Assert.That(cmd.Parameters[0].DbType, Is.EqualTo(DbType.Object));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("tsmultirange"));
            Assert.That(reader[1], Is.EqualTo(@"{[""1998-04-12 13:26:38"",""1998-04-12 15:26:38""],[""1998-04-13 13:26:38"",""1998-04-13 15:26:38""]}"));
        }

        #endregion

        #region Timestamp with timezone

        static readonly TestCaseData[] TimestampTzReadValues =
        {
            new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "1998-04-12 13:26:38+00")
                .SetName("TimestampPre2000"),
            new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), "2015-01-27 08:45:12.345+00")
                .SetName("TimestampPost2000"),
            new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Utc), "2013-07-25 00:00:00+00")
                .SetName("TimestampDateOnly")
        };

        [Test, TestCaseSource(nameof(TimestampTzReadValues))]
        public async Task Timestamptz_read(DateTime dateTime, string s)
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand($"SELECT '{s}'::timestamp with time zone", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            Assert.That(reader.GetDataTypeName(0), Is.EqualTo("timestamp with time zone"));
            Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(DateTime)));

            Assert.That(reader[0], Is.EqualTo(dateTime));
            Assert.That(reader.GetDateTime(0), Is.EqualTo(dateTime));
            Assert.That(reader.GetFieldValue<DateTime>(0), Is.EqualTo(dateTime));
            Assert.That(reader.GetDateTime(0).Kind, Is.EqualTo(DateTimeKind.Utc));

            // DateTimeOffset
            Assert.That(reader.GetFieldValue<DateTimeOffset>(0), Is.EqualTo(new DateTimeOffset(dateTime, TimeSpan.Zero)));
            Assert.That(reader.GetFieldValue<DateTimeOffset>(0).Offset, Is.EqualTo(TimeSpan.Zero));

            // Provider-specific type (EDBTimeStamp)
            var npgsqlDateTime = new EDBDateTime(dateTime.Ticks, DateTimeKind.Utc);
            Assert.That(reader.GetProviderSpecificFieldType(0), Is.EqualTo(typeof(EDBDateTime)));
            Assert.That(reader.GetTimeStamp(0), Is.EqualTo(npgsqlDateTime));
            Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(npgsqlDateTime));
            Assert.That(reader.GetFieldValue<EDBDateTime>(0), Is.EqualTo(npgsqlDateTime));
            Assert.That(reader.GetTimeStamp(0).Kind, Is.EqualTo(DateTimeKind.Utc));

            // Internal PostgreSQL representation, for out-of-range values.
            Assert.That(() => reader.GetInt64(0), Throws.Nothing);
        }

        static readonly TestCaseData[] TimestampTzWriteValues =
        {
            new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "1998-04-12 13:26:38")
                .SetName("TimestampTzPre2000"),
            new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), "2015-01-27 08:45:12.345")
                .SetName("TimestampTzPost2000"),
            new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Utc), "2013-07-25 00:00:00")
                .SetName("TimestampTzDateOnly"),
            new TestCaseData(EDBDateTime.Infinity, "infinity")
                .SetName("TimestampTzEDBDateTimeInfinity"),
            new TestCaseData(EDBDateTime.NegativeInfinity, "-infinity")
                .SetName("TimestampTzEDBDateTimeNegativeInfinity"),
            new TestCaseData(new EDBDateTime(-5, 3, 3, 1, 0, 0, DateTimeKind.Utc), "0005-03-03 01:00:00 BC")
                .SetName("TimestampTzBC"),
            new TestCaseData(DateTime.MinValue, "-infinity")
                .SetName("TimestampNegativeInfinity"),
            new TestCaseData(DateTime.MaxValue, "infinity")
                .SetName("TimestampInfinity")
        };

        [Test, TestCaseSource(nameof(TimestampTzWriteValues))]
        public async Task Timestamptz_write_values(object dateTime, string expected)
        {
            await using var conn = await OpenConnectionAsync();

            // PG sends local timestamptz *text* representations (according to TimeZone). Convert to a timestamp without time zone at UTC
            // for sensible assertions.
            await using var cmd = new EDBCommand("SELECT ($1 AT TIME ZONE 'UTC')::text", conn)
            {
                Parameters = { new() { Value = dateTime, EDBDbType = EDBDbType.TimestampTz } }
            };

            Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(expected));
        }

        static EDBParameter[] TimestamptzParameters
        {
            get
            {
                var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);

                return new EDBParameter[]
                {
                    new() { Value = dateTime },
                    new() { Value = dateTime, EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Utc), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = new DateTimeOffset(dateTime) },
                    new() { Value = -54297202000000L, EDBDbType = EDBDbType.TimestampTz }
                };
            }
        }

        [Test, TestCaseSource(nameof(TimestamptzParameters))]
        public async Task Timestamptz_resolution(EDBParameter parameter)
        {
            await using var conn = await OpenConnectionAsync();
            conn.TypeMapper.Reset();
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters = { parameter }
            };

            Assert.That(parameter.DataTypeName, Is.EqualTo("timestamp with time zone"));
            Assert.That(parameter.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz));
            Assert.That(parameter.DbType, Is.EqualTo(DbType.DateTime).Or.EqualTo(DbType.DateTimeOffset));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("timestamp with time zone"));
            Assert.That(reader[1], Is.EqualTo("1998-04-12 15:26:38+02"));
        }

        static EDBParameter[] TimestamptzInvalidParameters
            => new EDBParameter[]
            {
                new() { Value = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), EDBDbType = EDBDbType.TimestampTz },
                new() { Value = DateTime.Now, EDBDbType = EDBDbType.TimestampTz },
                new() { Value = new EDBDateTime(0, DateTimeKind.Unspecified), EDBDbType = EDBDbType.TimestampTz },
                new() { Value = new EDBDateTime(0, DateTimeKind.Local), EDBDbType = EDBDbType.TimestampTz },
                new() { Value = new DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromHours(2)) }
            };

        [Test, TestCaseSource(nameof(TimestamptzInvalidParameters))]
        public async Task Timestamptz_resolution_failure(EDBParameter parameter)
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT $1::text", conn)
            {
                Parameters = { parameter }
            };

            Assert.That(() => cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<InvalidCastException>());
        }

        [Test]
        public async Task Timestamptz_array_resolution()
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters = { new() { Value = new[] { new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc) } } }
            };

            Assert.That(cmd.Parameters[0].DataTypeName, Is.EqualTo("timestamp with time zone[]"));
            Assert.That(cmd.Parameters[0].EDBDbType, Is.EqualTo(EDBDbType.Array | EDBDbType.TimestampTz));
            Assert.That(cmd.Parameters[0].DbType, Is.EqualTo(DbType.Object));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("timestamp with time zone[]"));
            Assert.That(reader[1], Is.EqualTo(@"{""1998-04-12 15:26:38+02""}"));
        }

        [Test]
        public async Task Timestamptz_range_resolution()
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new EDBRange<DateTime>(
                            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                            new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc))
                    }
                }
            };

            Assert.That(cmd.Parameters[0].DataTypeName, Is.EqualTo("tstzrange"));
            Assert.That(cmd.Parameters[0].EDBDbType, Is.EqualTo(EDBDbType.Range | EDBDbType.TimestampTz));
            Assert.That(cmd.Parameters[0].DbType, Is.EqualTo(DbType.Object));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("tstzrange"));
            Assert.That(reader[1], Is.EqualTo(@"[""1998-04-12 15:26:38+02"",""1998-04-12 17:26:38+02""]"));
        }

        [Test]
        public async Task Timestamptz_multirange_resolution()
        {
            await using var conn = await OpenConnectionAsync();
            MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");
            await using var cmd = new EDBCommand("SELECT pg_typeof($1)::text, $1::text", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new[]
                        {
                            new EDBRange<DateTime>(
                                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
                            new EDBRange<DateTime>(
                                new DateTime(1998, 4, 13, 13, 26, 38, DateTimeKind.Utc),
                                new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Utc)),
                        }
                    }
                }
            };

            Assert.That(cmd.Parameters[0].DataTypeName, Is.EqualTo("tstzmultirange"));
            Assert.That(cmd.Parameters[0].EDBDbType, Is.EqualTo(EDBDbType.Multirange | EDBDbType.TimestampTz));
            Assert.That(cmd.Parameters[0].DbType, Is.EqualTo(DbType.Object));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("tstzmultirange"));
            Assert.That(reader[1], Is.EqualTo(@"{[""1998-04-12 15:26:38+02"",""1998-04-12 17:26:38+02""],[""1998-04-13 15:26:38+02"",""1998-04-13 17:26:38+02""]}"));
        }

        [Test]
        public async Task Cannot_mix_DateTime_Kinds_in_array()
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT $1", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new[]
                        {
                            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
                        }
                    }
                }
            };

            Assert.That(() => cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<Exception>());
        }

        [Test]
        public async Task Cannot_mix_DateTime_Kinds_in_range()
        {
            await using var conn = await OpenConnectionAsync();
            await using var cmd = new EDBCommand("SELECT $1", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new EDBRange<DateTime>(
                            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local))
                    }
                }
            };

            Assert.That(() => cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<InvalidCastException>());
        }

        [Test]
        public async Task Cannot_mix_DateTime_Kinds_in_multirange()
        {
            await using var conn = await OpenConnectionAsync();
            MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");
            await using var cmd = new EDBCommand("SELECT $1", conn)
            {
                Parameters =
                {
                    new()
                    {
                        Value = new[]
                        {
                            new EDBRange<DateTime>(
                                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
                            new EDBRange<DateTime>(
                                new DateTime(1998, 4, 13, 13, 26, 38, DateTimeKind.Local),
                                new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Local)),
                        }
                    }
                }
            };

            Assert.That(() => cmd.ExecuteReaderAsync(), Throws.Exception.TypeOf<InvalidCastException>());
        }

        [Test]
        public void EDBParameterDbType_is_value_dependent_datetime_or_datetime2()
        {
            var localtimestamp = new EDBParameter { Value = DateTime.Now };
            var unspecifiedtimestamp = new EDBParameter { Value = new DateTime() };
            Assert.AreEqual(DbType.DateTime2, localtimestamp.DbType);
            Assert.AreEqual(DbType.DateTime2, unspecifiedtimestamp.DbType);

            // We don't support any DateTimeOffset other than offset 0 which maps to timestamptz,
            // we might add an exception for offset == DateTimeOffset.Now.Offset (local offset) mapping to timestamp at some point.
            // var dtotimestamp = new EDBParameter { Value = DateTimeOffset.Now };
            // Assert.AreEqual(DbType.DateTime2, dtotimestamp.DbType);

            var timestamptz = new EDBParameter { Value = DateTime.UtcNow };
            var dtotimestamptz = new EDBParameter { Value = DateTimeOffset.UtcNow };
            Assert.AreEqual(DbType.DateTime, timestamptz.DbType);
            Assert.AreEqual(DbType.DateTime, dtotimestamptz.DbType);
        }

        [Test]
        public void EDBParameterEDBDbType_is_value_dependent_timestamp_or_timestamptz()
        {
            var localtimestamp = new EDBParameter { Value = DateTime.Now };
            var unspecifiedtimestamp = new EDBParameter { Value = new DateTime() };
            Assert.AreEqual(EDBDbType.Timestamp, localtimestamp.EDBDbType);
            Assert.AreEqual(EDBDbType.Timestamp, unspecifiedtimestamp.EDBDbType);

            var timestamptz = new EDBParameter { Value = DateTime.UtcNow };
            var dtotimestamptz = new EDBParameter { Value = DateTimeOffset.UtcNow };
            Assert.AreEqual(EDBDbType.TimestampTz, timestamptz.EDBDbType);
            Assert.AreEqual(EDBDbType.TimestampTz, dtotimestamptz.EDBDbType);
        }

        #endregion

        #region Interval

        [Test]
        public async Task Interval()
        {
            using var conn = await OpenConnectionAsync();
            var expectedEDBTimeSpan = new EDBTimeSpan(1, 2, 3, 4, 5);
            var expectedTimeSpan = new TimeSpan(1, 2, 3, 4, 5);
            var expectedEDBInterval = new EDBInterval(0, 1, 7384005000);

            using var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4", conn);
            var p1 = new EDBParameter("p1", EDBDbType.Interval);
            var p2 = new EDBParameter("p2", expectedTimeSpan);
            var p3 = new EDBParameter("p3", expectedEDBTimeSpan);
            var p4 = new EDBParameter("p4", expectedEDBInterval);
            Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Interval));
            Assert.That(p2.DbType, Is.EqualTo(DbType.Object));
            Assert.That(p3.EDBDbType, Is.EqualTo(EDBDbType.Interval));
            Assert.That(p3.DbType, Is.EqualTo(DbType.Object));
            Assert.That(p4.EDBDbType, Is.EqualTo(EDBDbType.Interval));
            Assert.That(p4.DbType, Is.EqualTo(DbType.Object));
            cmd.Parameters.Add(p1);
            cmd.Parameters.Add(p2);
            cmd.Parameters.Add(p3);
            cmd.Parameters.Add(p4);
            p1.Value = expectedEDBTimeSpan;

            using var reader = await cmd.ExecuteReaderAsync();
            reader.Read();

            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                // Regular type (TimeSpan)
                Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(TimeSpan)));
                Assert.That(reader.GetTimeSpan(i), Is.EqualTo(expectedTimeSpan));
                Assert.That(reader.GetFieldValue<TimeSpan>(i), Is.EqualTo(expectedTimeSpan));
                Assert.That(reader[i], Is.EqualTo(expectedTimeSpan));
                Assert.That(reader.GetValue(i), Is.EqualTo(expectedTimeSpan));

                // Provider-specific type (EDBInterval)
                Assert.That(reader.GetInterval(i), Is.EqualTo(expectedEDBTimeSpan));
                Assert.That(reader.GetProviderSpecificFieldType(i), Is.EqualTo(typeof(EDBTimeSpan)));
                Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(expectedEDBTimeSpan));
                Assert.That(reader.GetFieldValue<EDBTimeSpan>(i), Is.EqualTo(expectedEDBTimeSpan));

                // Internal PostgreSQL representation, for out-of-range values.
                Assert.That(() => reader.GetFieldValue<EDBInterval>(i), Is.EqualTo(expectedEDBInterval));
            }
        }

        [Test]
        public async Task Interval_with_months_cannot_read_as_TimeSpan()
        {
            using var conn = await OpenConnectionAsync();
            using var cmd = new EDBCommand("SELECT '1 month 2 days'::interval", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            Assert.That(() => reader.GetTimeSpan(0), Throws.Exception.TypeOf<InvalidCastException>());
        }

        #endregion

        protected override async ValueTask<EDBConnection> OpenConnectionAsync(string? connectionString = null)
        {
            var conn = await base.OpenConnectionAsync(connectionString);
            await conn.ExecuteNonQueryAsync("SET TimeZone='Europe/Berlin'");
            return conn;
        }

        protected override EDBConnection OpenConnection(string? connectionString = null)
            => throw new NotSupportedException();
    }
}
