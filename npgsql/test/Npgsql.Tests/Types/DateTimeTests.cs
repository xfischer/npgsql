using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using EDBTypes;
using EnterpriseDB.EDBClient.Tests.Support;
using NUnit.Framework;
using static EnterpriseDB.EDBClient.Tests.TestUtil;

namespace EnterpriseDB.EDBClient.Tests.Types;

// Since this test suite manipulates TimeZone, it is incompatible with multiplexing
public class DateTimeTests : TestBase
{
    #region Date

#if NET8_0_OR_GREATER
    [Test, Ignore("Redwood dates are time stamp, fix test")]
    public Task Date_as_DateOnly()
        => AssertType(new DateOnly(2020, 10, 1), "2020-10-01", "date", EDBDbType.Date, DbType.Date);
#endif

    [Test]
    public async Task Date_as_DateTime()
    {
        var con = await OpenConnectionAsync(); // EnterpriseDB
        TestUtil.EnsureNotEPASRedwood(con);

        await AssertType(new DateTime(2020, 10, 1), "2020-10-01", "date", EDBDbType.Date, DbType.Date, isDefault: false);
    }

    [Test]
    public Task Date_as_DateTime_with_date_and_time_before_2000()
        => AssertTypeWrite(new DateTime(1980, 10, 1, 11, 0, 0), "1980-10-01", "date", EDBDbType.Date, DbType.Date, isDefault: false);

    // Internal PostgreSQL representation (days since 2020-01-01), for out-of-range values.
    [Test, Ignore("Redwood dates are time stamp, fix test")]
    public Task Date_as_int()
        => AssertType(7579, "2020-10-01", "date", EDBDbType.Date, DbType.Date, isDefault: false);


#if NET8_0_OR_GREATER
    [Test]
    public async Task Daterange_as_EDBRange_of_DateOnly()
    {
        // EnterpriseDB : disable UnmappedTypes or test purposes
        await using var dataSource = base.CreateDataSource(b => b.DisableLegacyDateAndTime());
        await using var conn = await dataSource.OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB

        await AssertType(
            dataSource,
            new EDBRange<DateOnly>(new(2002, 3, 4), true, new(2002, 3, 6), false),
            "[2002-03-04,2002-03-06)",
            "daterange",
            EDBDbType.DateRange,
            skipArrayCheck: true); // EDBRange<T>[] is mapped to multirange by default, not array; test separately
    }
			
	[Test]
    public Task Daterange_array_as_EDBRange_of_DateOnly_array()
        => AssertType(
            new[]
            {
                new EDBRange<DateOnly>(new(2002, 3, 4), true, new(2002, 3, 6), false),
                new EDBRange<DateOnly>(new(2002, 3, 8), true, new(2002, 3, 9), false)
            },
            """{"[2002-03-04,2002-03-06)","[2002-03-08,2002-03-09)"}""",
            "daterange[]",
            EDBDbType.DateRange | EDBDbType.Array,
            isDefaultForWriting: false);
#endif

    [Test]
    public Task Daterange_as_EDBRange_of_DateTime()
        => AssertType(
            new EDBRange<DateTime>(new(2002, 3, 4), true, new(2002, 3, 6), false),
            "[2002-03-04,2002-03-06)",
            "daterange",
            EDBDbType.DateRange,
            isDefault: false);

#if NET8_0_OR_GREATER
[Test]
    public async Task Datemultirange_as_array_of_EDBRange_of_DateOnly()
    {
        // EnterpriseDB : disable UnmappedTypes or test purposes
        await using var dataSource = base.CreateDataSource(b => b.DisableUnmappedTypes().DisableRecordsAsTuples().DisableDynamicJson());
        await using var conn = await dataSource.OpenConnectionAsync();
		await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertType(
            dataSource, // EnterpriseDB force datasource without opt-ins
            new[]
            {
                new EDBRange<DateOnly>(new(2002, 3, 4), true, new(2002, 3, 6), false),
                new EDBRange<DateOnly>(new(2002, 3, 8), true, new(2002, 3, 11), false)
            },
            "{[04-MAR-02,06-MAR-02),[08-MAR-02,11-MAR-02)}", // EnterpriseDB redwood dates
            "datemultirange",
            EDBDbType.DateMultirange);
    }
#endif

    [Test]
    public async Task Datemultirange_as_array_of_EDBRange_of_DateTime()
    {
        // EnterpriseDB : disable UnmappedTypes or test purposes
        await using var dataSource = base.CreateDataSource(b => b.DisableUnmappedTypes());
        await using var conn = await dataSource.OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertType(
            dataSource, // EnterpriseDB force datasource without opt-ins
            new[]
            {
                new EDBRange<DateTime>(new(2002, 3, 4), true, new(2002, 3, 6), false),
                new EDBRange<DateTime>(new(2002, 3, 8), true, new(2002, 3, 11), false)
            },
            "{[04-MAR-02,06-MAR-02),[08-MAR-02,11-MAR-02)}", // EnterpriseDB redwood dates
            "datemultirange",
            EDBDbType.DateMultirange,
            isDefault: false);
    }

    #endregion

    #region Time

#if NET8_0_OR_GREATER
    [Test]
    public async Task Time_as_TimeOnly()
    {
        // EnterpriseDB : disable UnmappedTypes or test purposes
        await using var dataSource = base.CreateDataSource(b => b.DisableLegacyDateAndTime());
        await using var conn = await dataSource.OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB
        await AssertType(dataSource,
        new TimeOnly(10, 45, 34, 500),
        "10:45:34.5",
        "time without time zone",
        EDBDbType.Time,
        DbType.Time);
    }
#endif

    [Test]
    public Task Time_as_TimeSpan()
        => AssertType(
            new TimeSpan(0, 10, 45, 34, 500),
            "10:45:34.5",
            "time without time zone",
            EDBDbType.Time,
            DbType.Time,
            isDefault: false);

    #endregion

    #region Time with timezone

    static readonly TestCaseData[] TimeTzValues =
    [
        new TestCaseData(new DateTimeOffset(1, 1, 2, 13, 3, 45, 510, TimeSpan.FromHours(2)), "13:03:45.51+02")
            .SetName("Timezone"),
        new TestCaseData(new DateTimeOffset(1, 1, 2, 1, 0, 45, 510, TimeSpan.FromHours(-3)), "01:00:45.51-03")
            .SetName("Negative_timezone"),
        new TestCaseData(new DateTimeOffset(1212720130000, TimeSpan.Zero), "09:41:12.013+00")
            .SetName("Utc"),
        new TestCaseData(new DateTimeOffset(1, 1, 2, 1, 0, 0, new TimeSpan(0, 2, 0, 0)), "01:00:00+02")
            .SetName("Before_utc_zero")
    ];

    [Test, TestCaseSource(nameof(TimeTzValues))]
    public Task TimeTz_as_DateTimeOffset(DateTimeOffset time, string sqlLiteral)
        => AssertType(time, sqlLiteral, "time with time zone", EDBDbType.TimeTz, isDefault: false);

    #endregion

    #region Timestamp

    static readonly TestCaseData[] TimestampValues =
    [
        new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified), "1998-04-12 13:26:38")
            .SetName("Timestamp_pre2000"),
        new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Unspecified), "2015-01-27 08:45:12.345")
            .SetName("Timestamp_post2000"),
        new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Unspecified), "2013-07-25 00:00:00")
            .SetName("Timestamp_date_only")
    ];

    [Test, TestCaseSource(nameof(TimestampValues))]
    public async Task Timestamp_as_DateTime(DateTime dateTime, string sqlLiteral)
    {
        await AssertType(dateTime, sqlLiteral, "timestamp without time zone", EDBDbType.Timestamp, DbType.DateTime2,
            // Explicitly check kind as well.
            comparer: (actual, expected) => actual.Kind == expected.Kind && actual.Equals(expected));

        await AssertType(
            new List<DateTime> { dateTime, dateTime }, $$"""{"{{sqlLiteral}}","{{sqlLiteral}}"}""", "timestamp without time zone[]", EDBDbType.Timestamp | EDBDbType.Array,
            isDefaultForReading: false);
    }

    [Test]
    public Task Timestamp_cannot_write_utc_DateTime()
        => AssertTypeUnsupportedWrite<DateTime, ArgumentException>(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "timestamp without time zone");

    [Test]
    public Task Timestamp_as_long()
        => AssertType(
            -54297202000000,
            "1998-04-12 13:26:38",
            "timestamp without time zone",
            EDBDbType.Timestamp,
            DbType.DateTime2,
            isDefault: false);

    [Test]
    public Task Timestamp_cannot_use_as_DateTimeOffset()
        => AssertTypeUnsupported(
            new DateTimeOffset(1998, 4, 12, 13, 26, 38, TimeSpan.Zero),
            "1998-04-12 13:26:38",
            "timestamp without time zone");

    [Test]
    public Task Tsrange_as_EDBRange_of_DateTime()
        => AssertType(
            new EDBRange<DateTime>(
                new(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
                new(1998, 4, 12, 15, 26, 38, DateTimeKind.Local)),
            @"[""1998-04-12 13:26:38"",""1998-04-12 15:26:38""]",
            "tsrange",
            EDBDbType.TimestampRange,
            skipArrayCheck: true); // EDBRange<T>[] is mapped to multirange by default, not array; test separately

    [Test]
    public Task Tsrange_array_as_EDBRange_of_DateTime_array()
        => AssertType(
            new[]
            {
                new EDBRange<DateTime>(
                    new(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
                    new(1998, 4, 12, 15, 26, 38, DateTimeKind.Local)),
                new EDBRange<DateTime>(
                    new(1998, 4, 13, 13, 26, 38, DateTimeKind.Local),
                    new(1998, 4, 13, 15, 26, 38, DateTimeKind.Local)),
            },
            """{"[\"1998-04-12 13:26:38\",\"1998-04-12 15:26:38\"]","[\"1998-04-13 13:26:38\",\"1998-04-13 15:26:38\"]"}""",
            "tsrange[]",
            EDBDbType.TimestampRange | EDBDbType.Array,
            isDefault: false);

    [Test]
    public async Task Tsmultirange_as_array_of_EDBRange_of_DateTime()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertType(
            new[]
            {
                new EDBRange<DateTime>(
                    new(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
                    new(1998, 4, 12, 15, 26, 38, DateTimeKind.Local)),
                new EDBRange<DateTime>(
                    new(1998, 4, 13, 13, 26, 38, DateTimeKind.Local),
                    new(1998, 4, 13, 15, 26, 38, DateTimeKind.Local)),
            },
            @"{[""1998-04-12 13:26:38"",""1998-04-12 15:26:38""],[""1998-04-13 13:26:38"",""1998-04-13 15:26:38""]}",
            "tsmultirange",
            EDBDbType.TimestampMultirange);
    }

    #endregion

    #region Timestamp with timezone

    // Note that the below text representations are local (according to TimeZone, which is set to Europe/Berlin in this test class),
    // because that's how PG does timestamptz *text* representation.
    static readonly TestCaseData[] TimestampTzWriteValues =
    [
        new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "1998-04-12 15:26:38+02")
            .SetName("Timestamptz_write_pre2000"),
        new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), "2015-01-27 09:45:12.345+01")
            .SetName("Timestamptz_write_post2000"),
        new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Utc), "2013-07-25 02:00:00+02")
            .SetName("Timestamptz_write_date_only")
    ];

    [Test, TestCaseSource(nameof(TimestampTzWriteValues))]
    public async Task Timestamptz_as_DateTime(DateTime dateTime, string sqlLiteral)
    {
        await AssertType(dateTime, sqlLiteral, "timestamp with time zone", EDBDbType.TimestampTz, DbType.DateTime,
            // Explicitly check kind as well.
            comparer: (actual, expected) => actual.Kind == expected.Kind && actual.Equals(expected));

        await AssertType(
            new List<DateTime> { dateTime, dateTime }, $$"""{"{{sqlLiteral}}","{{sqlLiteral}}"}""", "timestamp with time zone[]", EDBDbType.TimestampTz | EDBDbType.Array,
            isDefaultForReading: false);

    }

    [Test]
    public async Task Timestamptz_infinity_as_DateTime()
    {
        await AssertType(DateTime.MinValue, "-infinity", "timestamp with time zone", EDBDbType.TimestampTz, DbType.DateTime,
            isDefault: false);
        await AssertType(DateTime.MaxValue, "infinity", "timestamp with time zone", EDBDbType.TimestampTz, DbType.DateTime,
            isDefault: false);
    }

    [Test]
    public async Task Timestamptz_cannot_write_non_utc_DateTime()
    {
        await AssertTypeUnsupportedWrite<DateTime, ArgumentException>(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified), "timestamp with time zone");
        await AssertTypeUnsupportedWrite<DateTime, ArgumentException>(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local), "timestamp with time zone");
    }

    [Test]
    public async Task Timestamptz_as_DateTimeOffset_utc()
    {
        var dateTimeOffset = await AssertType(
            new DateTimeOffset(1998, 4, 12, 13, 26, 38, TimeSpan.Zero),
            "1998-04-12 15:26:38+02",
            "timestamp with time zone",
            EDBDbType.TimestampTz,
            DbType.DateTime,
            isDefaultForReading: false);

        Assert.That(dateTimeOffset.Offset, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public Task Timestamptz_as_DateTimeOffset_utc_with_DbType_DateTimeOffset()
        => AssertTypeWrite(
            new DateTimeOffset(1998, 4, 12, 13, 26, 38, TimeSpan.Zero),
            "1998-04-12 15:26:38+02",
            "timestamp with time zone",
            EDBDbType.TimestampTz,
            DbType.DateTimeOffset,
            inferredDbType: DbType.DateTime,
            isDefault: false);

    [Test]
    public Task Timestamptz_cannot_write_non_utc_DateTimeOffset()
        => AssertTypeUnsupportedWrite<DateTimeOffset, ArgumentException>(new DateTimeOffset(1998, 4, 12, 13, 26, 38, TimeSpan.FromHours(2)));

    [Test]
    public Task Timestamptz_as_long()
        => AssertType(
            -54297202000000,
            "1998-04-12 15:26:38+02",
            "timestamp with time zone",
            EDBDbType.TimestampTz,
            DbType.DateTime,
            isDefault: false);

    [Test]
    public async Task Timestamptz_array_as_DateTimeOffset_array()
    {
        var dateTimeOffsets = await AssertType(
            new[]
            {
                new DateTimeOffset(1998, 4, 12, 13, 26, 38, TimeSpan.Zero),
                new DateTimeOffset(1999, 4, 12, 13, 26, 38, TimeSpan.Zero)
            },
            """{"1998-04-12 15:26:38+02","1999-04-12 15:26:38+02"}""",
            "timestamp with time zone[]",
            EDBDbType.TimestampTz | EDBDbType.Array,
            isDefaultForReading: false);

        Assert.That(dateTimeOffsets[0].Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(dateTimeOffsets[1].Offset, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public Task Tstzrange_as_EDBRange_of_DateTime()
        => AssertType(
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            @"[""1998-04-12 15:26:38+02"",""1998-04-12 17:26:38+02""]",
            "tstzrange",
            EDBDbType.TimestampTzRange,
            skipArrayCheck: true); // EDBRange<T>[] is mapped to multirange by default, not array; test separately

    [Test]
    public Task Tstzrange_array_as_EDBRange_of_DateTime_array()
        => AssertType(
            new[]
            {
                new EDBRange<DateTime>(
                    new(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                    new(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
                new EDBRange<DateTime>(
                    new(1998, 4, 13, 13, 26, 38, DateTimeKind.Utc),
                    new(1998, 4, 13, 15, 26, 38, DateTimeKind.Utc)),
            },
            """{"[\"1998-04-12 15:26:38+02\",\"1998-04-12 17:26:38+02\"]","[\"1998-04-13 15:26:38+02\",\"1998-04-13 17:26:38+02\"]"}""",
            "tstzrange[]",
            EDBDbType.TimestampTzRange | EDBDbType.Array,
            isDefault: false);

    [Test]
    public async Task Tstzmultirange_as_array_of_EDBRange_of_DateTime()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertType(
            new[]
            {
                new EDBRange<DateTime>(
                    new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                    new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
                new EDBRange<DateTime>(
                    new DateTime(1998, 4, 13, 13, 26, 38, DateTimeKind.Utc),
                    new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Utc)),
            },
            @"{[""1998-04-12 15:26:38+02"",""1998-04-12 17:26:38+02""],[""1998-04-13 15:26:38+02"",""1998-04-13 17:26:38+02""]}",
            "tstzmultirange",
            EDBDbType.TimestampTzMultirange);
    }

    [Test]
    public Task Cannot_mix_DateTime_Kinds_in_array()
        => AssertTypeUnsupportedWrite<DateTime[], ArgumentException>([
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local)
        ]);


    [Test]
    public Task Cannot_mix_DateTime_Kinds_in_range()
        => AssertTypeUnsupportedWrite<EDBRange<DateTime>, ArgumentException>(new EDBRange<DateTime>(
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local)));

    [Test]
    public async Task Cannot_mix_DateTime_Kinds_in_multirange()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertTypeUnsupportedWrite<EDBRange<DateTime>[], ArgumentException>([
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 13, 13, 26, 38, DateTimeKind.Local),
                new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Local))
        ]);
    }

    [Test]
    public void EDBParameterDbType_is_value_dependent_datetime_or_datetime2()
    {
        var localtimestamp = new EDBParameter { Value = DateTime.Now };
        var unspecifiedtimestamp = new EDBParameter { Value = new DateTime() };
        Assert.That(localtimestamp.DbType, Is.EqualTo(DbType.DateTime2));
        Assert.That(unspecifiedtimestamp.DbType, Is.EqualTo(DbType.DateTime2));

        // We don't support any DateTimeOffset other than offset 0 which maps to timestamptz,
        // we might add an exception for offset == DateTimeOffset.Now.Offset (local offset) mapping to timestamp at some point.
        // var dtotimestamp = new EDBParameter { Value = DateTimeOffset.Now };
        // Assert.That(DbType.DateTime2, dtotimestamp.DbType);

        var timestamptz = new EDBParameter { Value = DateTime.UtcNow };
        var dtotimestamptz = new EDBParameter { Value = DateTimeOffset.UtcNow };
        Assert.That(timestamptz.DbType, Is.EqualTo(DbType.DateTime));
        Assert.That(dtotimestamptz.DbType, Is.EqualTo(DbType.DateTime));
    }

    [Test]
    public void EDBParameterEDBDbType_is_value_dependent_timestamp_or_timestamptz()
    {
        var localtimestamp = new EDBParameter { Value = DateTime.Now };
        var unspecifiedtimestamp = new EDBParameter { Value = new DateTime() };
        Assert.That(localtimestamp.EDBDbType, Is.EqualTo(EDBDbType.Timestamp));
        Assert.That(unspecifiedtimestamp.EDBDbType, Is.EqualTo(EDBDbType.Timestamp));

        var timestamptz = new EDBParameter { Value = DateTime.UtcNow };
        var dtotimestamptz = new EDBParameter { Value = DateTimeOffset.UtcNow };
        Assert.That(timestamptz.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz));
        Assert.That(dtotimestamptz.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz));
    }

    [Test]
    public async Task Array_of_nullable_timestamptz()
        => await AssertType(
            new DateTime?[]
            {
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                null
            },
            @"{""1998-04-12 15:26:38+02"",NULL}",
            "timestamp with time zone[]",
            EDBDbType.TimestampTz | EDBDbType.Array,
            isDefault: false);

    #endregion

    #region Interval

    static readonly TestCaseData[] IntervalValues =
    [
        new TestCaseData(new TimeSpan(0, 2, 3, 4, 5), "02:03:04.005")
            .SetName("Interval_time_only"),
        new TestCaseData(new TimeSpan(1, 2, 3, 4, 5), "1 day 02:03:04.005")
            .SetName("Interval_with_day"),
        new TestCaseData(new TimeSpan(61, 2, 3, 4, 5), "61 days 02:03:04.005")
            .SetName("Interval_with_many_days"),
        new TestCaseData(new TimeSpan(new TimeSpan(2, 3, 4).Ticks + 10), "02:03:04.000001")
            .SetName("Interval_with_microsecond")
    ];

    [Test, TestCaseSource(nameof(IntervalValues))]
    public Task Interval_as_TimeSpan(TimeSpan timeSpan, string sqlLiteral)
        => AssertType(timeSpan, sqlLiteral, "interval", EDBDbType.Interval);

    [Test]
    public Task Interval_write_as_TimeSpan_truncates_ticks()
        => AssertTypeWrite(
            new TimeSpan(new TimeSpan(2, 3, 4).Ticks + 1),
            "02:03:04",
            "interval",
            EDBDbType.Interval);

    [Test]
    public Task Interval_as_EDBInterval()
        => AssertType(
            new EDBInterval(2, 15, 7384005000),
            "2 mons 15 days 02:03:04.005", "interval",
            EDBDbType.Interval,
            isDefaultForReading: false);

    [Test]
    public Task Interval_with_months_cannot_read_as_TimeSpan()
        => AssertTypeUnsupportedRead<TimeSpan, InvalidCastException>("1 month 2 days", "interval");

    #endregion

    protected override async ValueTask<EDBConnection> OpenConnectionAsync(string? newConnString = null)
    {
        var conn = await base.OpenConnectionAsync(newConnString);
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO"); // EnterpriseDB
        await conn.ExecuteNonQueryAsync("SET TimeZone='Europe/Berlin'");
        return conn;
    }

    protected override EDBConnection OpenConnection()
        => throw new NotSupportedException();
}
