using System;
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

    [Test]
    public async Task Date_as_DateTime()
    {
        var con = await OpenConnectionAsync();
        TestUtil.EnsureNotEPASRedwood(con);

        await AssertType(new DateTime(2020, 10, 1), "2020-10-01", "date", EDBDbType.Date, DbType.Date, isDefaultForWriting: false);
    }

    [Test]
    public Task Date_as_DateTime_with_date_and_time_before_2000()
        => AssertTypeWrite(new DateTime(1980, 10, 1, 11, 0, 0), "1980-10-01", "date", EDBDbType.Date, DbType.Date, isDefault: false);

    // Internal PostgreSQL representation (days since 2020-01-01), for out-of-range values.
    [Test, EDBExplicit("Works in community")]
    public Task Date_as_int()
        => AssertType(7579, "2020-10-01", "date", EDBDbType.Date, DbType.Date, isDefault: false);

    [Test]
    public Task Daterange_as_EDBRange_of_DateTime()
        => AssertType(
            new EDBRange<DateTime>(new(2002, 3, 4), true, new(2002, 3, 6), false),
            "[2002-03-04,2002-03-06)",
            "daterange",
            EDBDbType.DateRange,
            isDefaultForWriting: false);

    [Test]
    public async Task Datemultirange_as_array_of_EDBRange_of_DateTime()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO");

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertType(
            new[]
            {
                new EDBRange<DateTime>(new(2002, 3, 4), true, new(2002, 3, 6), false),
                new EDBRange<DateTime>(new(2002, 3, 8), true, new(2002, 3, 11), false)
            },
            "{[2002-03-04,2002-03-06),[2002-03-08,2002-03-11)}",
            "datemultirange",
            EDBDbType.DateMultirange,
            isDefaultForWriting: false);
    }

#if NET6_0_OR_GREATER
    [Test, Ignore("")]
    public Task Date_as_DateOnly()
        => AssertType(new DateOnly(2020, 10, 1), "2020-10-01", "date", EDBDbType.Date, DbType.Date, isDefaultForReading: false);

    [Test]
    public Task Daterange_as_EDBRange_of_DateOnly()
        => AssertType(
            new EDBRange<DateOnly>(new(2002, 3, 4), true, new(2002, 3, 6), false),
            "[2002-03-04,2002-03-06)",
            "daterange",
            EDBDbType.DateRange,
            isDefaultForReading: false);

    [Test]
    public async Task Datemultirange_as_array_of_EDBRange_of_DateOnly()
    {
        await using var conn = await OpenConnectionAsync();
                await conn.ExecuteNonQueryAsync("SET datestyle TO ISO");

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertType(
            new[]
            {
                new EDBRange<DateOnly>(new(2002, 3, 4), true, new(2002, 3, 6), false),
                new EDBRange<DateOnly>(new(2002, 3, 8), true, new(2002, 3, 11), false)
            },
            "{[2002-03-04,2002-03-06),[2002-03-08,2002-03-11)}",
            "datemultirange",
            EDBDbType.DateMultirange,
            isDefaultForReading: false);
    }
#endif

    #endregion

    #region Time

    [Test]
    public Task Time_as_TimeSpan()
        => AssertType(
            new TimeSpan(0, 10, 45, 34, 500),
            "10:45:34.5",
            "time without time zone",
            EDBDbType.Time,
            DbType.Time,
            isDefaultForWriting: false);

#if NET6_0_OR_GREATER
    [Test]
    public Task Time_as_TimeOnly()
        => AssertType(
            new TimeOnly(10, 45, 34, 500),
            "10:45:34.5",
            "time without time zone",
            EDBDbType.Time,
            DbType.Time,
            isDefaultForReading: false);
#endif

    #endregion

    #region Time with timezone

    [Test]
    public async Task TimeTz_as_DateTimeOffset()
    {
        await AssertTypeRead("13:03:45.51+02",
            "time with time zone", new DateTimeOffset(1, 1, 2, 13, 3, 45, 510, TimeSpan.FromHours(2)));

        await AssertTypeWrite(
            new DateTimeOffset(1, 1, 1, 13, 3, 45, 510, TimeSpan.FromHours(2)),
            "13:03:45.51+02",
            "time with time zone",
            EDBDbType.TimeTz,
            isDefault: false);
    }

    [Test]
    public Task TimeTz_before_utc_zero()
        => AssertTypeRead("01:00:00+02",
            "time with time zone", new DateTimeOffset(1, 1, 2, 1, 0, 0, new TimeSpan(0, 2, 0, 0)));

    #endregion

    #region Timestamp

    static readonly TestCaseData[] TimestampValues =
    {
        new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified), "1998-04-12 13:26:38")
            .SetName("Timestamp_pre2000"),
        new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Unspecified), "2015-01-27 08:45:12.345")
            .SetName("Timestamp_post2000"),
        new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Unspecified), "2013-07-25 00:00:00")
            .SetName("Timestamp_date_only")
    };

    [Test, TestCaseSource(nameof(TimestampValues))]
    public Task Timestamp_as_DateTime(DateTime dateTime, string sqlLiteral)
        => AssertType(dateTime, sqlLiteral, "timestamp without time zone", EDBDbType.Timestamp, DbType.DateTime2);

    [Test]
    public Task Timestamp_cannot_write_utc_DateTime()
        => AssertTypeUnsupportedWrite(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "timestamp without time zone");

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
            EDBDbType.TimestampRange);

    [Test]
    public async Task Tsmultirange_as_array_of_EDBRange_of_DateTime()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO");

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
    {
        new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "1998-04-12 15:26:38+02")
            .SetName("Timestamptz_write_pre2000"),
        new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), "2015-01-27 09:45:12.345+01")
            .SetName("Timestamptz_write_post2000"),
        new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Utc), "2013-07-25 02:00:00+02")
            .SetName("Timestamptz_write_date_only")
    };

    [Test, TestCaseSource(nameof(TimestampTzWriteValues))]
    public Task Timestamptz_as_DateTime(DateTime dateTime, string sqlLiteral)
        => AssertType(dateTime, sqlLiteral, "timestamp with time zone", EDBDbType.TimestampTz, DbType.DateTime);

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
        await AssertTypeUnsupportedWrite(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified), "timestamp with time zone");
        await AssertTypeUnsupportedWrite(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local), "timestamp with time zone");
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
        => AssertTypeUnsupportedWrite(new DateTimeOffset(1998, 4, 12, 13, 26, 38, TimeSpan.FromHours(2)));

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
    public Task Tstzrange_as_EDBRange_of_DateTime()
        => AssertType(
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            @"[""1998-04-12 15:26:38+02"",""1998-04-12 17:26:38+02""]",
            "tstzrange",
            EDBDbType.TimestampTzRange);

    [Test]
    public async Task Tstzmultirange_as_array_of_EDBRange_of_DateTime()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO");

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
        => AssertTypeUnsupportedWrite<DateTime[], Exception>(new[]
        {
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local),
        });


    [Test]
    public Task Cannot_mix_DateTime_Kinds_in_range()
        => AssertTypeUnsupportedWrite(new EDBRange<DateTime>(
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
            new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local)));

    [Test]
    public async Task Cannot_mix_DateTime_Kinds_in_multirange()
    {
        await using var conn = await OpenConnectionAsync();
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO");

        MinimumPgVersion(conn, "14.0", "Multirange types were introduced in PostgreSQL 14");

        await AssertTypeUnsupportedWrite(new[]
        {
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)),
            new EDBRange<DateTime>(
                new DateTime(1998, 4, 13, 13, 26, 38, DateTimeKind.Local),
                new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Local)),
        });
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

    static readonly TestCaseData[] IntervalValues =
    {
        new TestCaseData(new TimeSpan(0, 2, 3, 4, 5), "02:03:04.005")
            .SetName("Interval_time_only"),
        new TestCaseData(new TimeSpan(1, 2, 3, 4, 5), "1 day 02:03:04.005")
            .SetName("Interval_with_day"),
        new TestCaseData(new TimeSpan(61, 2, 3, 4, 5), "61 days 02:03:04.005")
            .SetName("Interval_with_many_days"),
        new TestCaseData(new TimeSpan(new TimeSpan(2, 3, 4).Ticks + 10), "02:03:04.000001")
            .SetName("Interval_with_microsecond")
    };

    [Test, TestCaseSource(nameof(IntervalValues))]
    public Task Interval_as_TimeSpan(TimeSpan timeSpan, string sqlLiteral)
        => AssertType(timeSpan, sqlLiteral, "interval", EDBDbType.Interval, isDefaultForWriting: false);

    [Test]
    public Task Interval_write_as_TimeSpan_truncates_ticks()
        => AssertTypeWrite(
            new TimeSpan(new TimeSpan(2, 3, 4).Ticks + 1),
            "02:03:04",
            "interval",
            EDBDbType.Interval,
            isDefault: false);

    [Test]
    public Task Interval_as_EDBInterval()
        => AssertType(
            new EDBInterval(2, 15, 7384005000),
            "2 mons 15 days 02:03:04.005", "interval",
            EDBDbType.Interval,
            isDefaultForReading: false,
            isDefaultForWriting: false);

    [Test]
    public Task Interval_with_months_cannot_read_as_TimeSpan()
        => AssertTypeUnsupportedRead("1 month 2 days", "interval");

    #endregion

    protected override async ValueTask<EDBConnection> OpenConnectionAsync(string? connectionString = null)
    {
        var conn = await base.OpenConnectionAsync(connectionString);
        await conn.ExecuteNonQueryAsync("SET datestyle TO ISO");
        await conn.ExecuteNonQueryAsync("SET TimeZone='Europe/Berlin'");
        return conn;
    }

    protected override EDBConnection OpenConnection(string? connectionString = null)
        => throw new NotSupportedException();
}
