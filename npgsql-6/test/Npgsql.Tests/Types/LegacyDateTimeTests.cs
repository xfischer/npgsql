using System;
using System.Data;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using NUnit.Framework;
using static EnterpriseDB.EDBClient.Util.Statics;

#pragma warning disable 618 // EDBDateTime is obsolete, remove in 7.0

namespace EnterpriseDB.EDBClient.Tests.Types
{
    // Since this test suite manipulates TimeZone, it is incompatible with multiplexing
    [NonParallelizable]
    public class LegacyDateTimeTests : TestBase
    {
        static readonly TestCaseData[] TimestampValues =
        {
            new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "12-APR-98 13:26:38")
                .SetName("TimestampPre2000"),
            new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), "27-JAN-15 08:45:12.345")
                .SetName("TimestampPost2000"),
            new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Utc), "25-JUL-13 00:00:00")
                .SetName("TimestampDateOnly"),
        };

        //[Test, TestCaseSource(nameof(TimestampValues))]
        //public async Task Timestamp_read(DateTime dateTime, string s)
        //{
        //    await using var conn = await OpenConnectionAsync();
        //    await using var cmd = new EDBCommand($"SELECT '{s}'::timestamp without time zone", conn);
        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    await reader.ReadAsync();

        //    Assert.That(reader.GetDataTypeName(0), Is.EqualTo("timestamp without time zone"));
        //    Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(DateTime)));

        //    Assert.That(reader[0], Is.EqualTo(dateTime));
        //    Assert.That(reader.GetDateTime(0), Is.EqualTo(dateTime));
        //    Assert.That(reader.GetDateTime(0).Kind, Is.EqualTo(DateTimeKind.Unspecified));
        //    Assert.That(reader.GetFieldValue<DateTime>(0), Is.EqualTo(dateTime));

        //    // Provider-specific type (EDBTimeStamp)
        //    var npgsqlDateTime = new EDBDateTime(dateTime.Ticks);
        //    Assert.That(reader.GetProviderSpecificFieldType(0), Is.EqualTo(typeof(EDBDateTime)));
        //    Assert.That(reader.GetTimeStamp(0), Is.EqualTo(npgsqlDateTime));
        //    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(npgsqlDateTime));
        //    Assert.That(reader.GetFieldValue<EDBDateTime>(0), Is.EqualTo(npgsqlDateTime));

        //    // DateTimeOffset
        //    Assert.That(() => reader.GetFieldValue<DateTimeOffset>(0), Throws.Exception.TypeOf<InvalidCastException>());
        //}

        //[Test, TestCaseSource(nameof(TimestampValues))]
        //public async Task Timestamp_write_values(DateTime dateTime, string expected)
        //{
        //    Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Utc));

        //    await using var conn = await OpenConnectionAsync();
        //    await using var cmd = new EDBCommand("SELECT $1::text", conn)
        //    {
        //        Parameters =
        //        {
        //            new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), EDBDbType = EDBDbType.Timestamp }
        //        }
        //    };

        //    Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(expected));
        //}

        static Func<EDBParameter>[] TimestampParameters
        {
            get
            {
                var dateTime = new DateTime(1998, 4, 12, 13, 26, 38);

                return new Func<EDBParameter>[]
                {
                    () => new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified) },
                    () => new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Local) },
                    () => new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) },
                    () => new() { Value = dateTime, EDBDbType = EDBDbType.Timestamp },
                    () => new() { Value = dateTime, DbType = DbType.DateTime },
                    () => new() { Value = dateTime, DbType = DbType.DateTime2 },
                    () => new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Unspecified) },
                    () => new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Local) },
                    () => new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Utc) },
                };
            }
        }

        [Test, TestCaseSource(nameof(TimestampParameters))]
        public async Task Timestamp_resolution(Func<EDBParameter> parameterFunc)
        {
            var parameter = parameterFunc();
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
            Assert.That(reader[1], Is.EqualTo("12-APR-98 13:26:38"));
        }

        static EDBParameter[] TimestampInvalidParameters
            => new EDBParameter[]
            {
                new() { Value = new DateTimeOffset(), EDBDbType = EDBDbType.Timestamp }
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

        //[Test, TestCaseSource(nameof(TimestampValues))]
        //public async Task Timestamptz_read(DateTime dateTime, string s)
        //{
        //    Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Utc));

        //    await using var conn = await OpenConnectionAsync();
        //    await using var cmd = new EDBCommand($"SELECT '{s}+00'::timestamp with time zone", conn);
        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    await reader.ReadAsync();

        //    Assert.That(reader.GetDataTypeName(0), Is.EqualTo("timestamp with time zone"));
        //    Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(DateTime)));

        //    Assert.That(reader[0], Is.EqualTo(dateTime.ToLocalTime()));
        //    Assert.That(reader.GetDateTime(0), Is.EqualTo(dateTime.ToLocalTime()));
        //    Assert.That(reader.GetFieldValue<DateTime>(0), Is.EqualTo(dateTime.ToLocalTime()));
        //    Assert.That(reader.GetDateTime(0).Kind, Is.EqualTo(DateTimeKind.Local));

        //    // DateTimeOffset
        //    Assert.That(reader.GetFieldValue<DateTimeOffset>(0), Is.EqualTo(new DateTimeOffset(dateTime.ToLocalTime())));

        //    // Provider-specific type (EDBTimeStamp)
        //    var npgsqlDateTime = new EDBDateTime(dateTime.Ticks);
        //    Assert.That(reader.GetProviderSpecificFieldType(0), Is.EqualTo(typeof(EDBDateTime)));
        //    Assert.That(reader.GetTimeStamp(0), Is.EqualTo(npgsqlDateTime.ToLocalTime()));
        //    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(npgsqlDateTime.ToLocalTime()));
        //    Assert.That(reader.GetFieldValue<EDBDateTime>(0), Is.EqualTo(npgsqlDateTime.ToLocalTime()));
        //    Assert.That(reader.GetTimeStamp(0).Kind, Is.EqualTo(DateTimeKind.Local));
        //}

        static readonly TestCaseData[] TimestampTzValues =
        {
            new TestCaseData(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), "12-APR-98 15:26:38 +02:00")
                .SetName("TimestampPre2000"),
            new TestCaseData(new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), "27-JAN-15 09:45:12.345 +01:00")
                .SetName("TimestampPost2000"),
            new TestCaseData(new DateTime(2013, 7, 25, 0, 0, 0, DateTimeKind.Utc), "25-JUL-13 02:00:00 +02:00")
                .SetName("TimestampDateOnly"),
        };

        //[Test, TestCaseSource(nameof(TimestampTzValues))]
        //public async Task Timestamptz_write_values(DateTime dateTime, string expected)
        //{
        //    Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Utc));

        //    await using var conn = await OpenConnectionAsync();
        //    await using var cmd = new EDBCommand("SELECT $1::text", conn)
        //    {
        //        Parameters = { new() { Value = dateTime, EDBDbType = EDBDbType.TimestampTz } }
        //    };

        //    Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(expected));
        //}

        static EDBParameter[] TimestamptzParameters
        {
            get
            {
                var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);

                return new EDBParameter[]
                {
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = dateTime.ToLocalTime(), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Unspecified), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Utc).ToLocalTime(), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = new EDBDateTime(dateTime.Ticks, DateTimeKind.Utc), EDBDbType = EDBDbType.TimestampTz },
                    new() { Value = new DateTimeOffset(dateTime.ToLocalTime()) }
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

            Assert.That(parameter.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz));
            Assert.That(parameter.DbType, Is.EqualTo(DbType.DateTimeOffset));

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.That(reader[0], Is.EqualTo("timestamp with time zone"));
            Assert.That(reader[1], Is.EqualTo("12-APR-98 15:26:38 +02:00"));
        }

        protected override async ValueTask<EDBConnection> OpenConnectionAsync(string? connectionString = null)
        {
            var conn = await base.OpenConnectionAsync(connectionString);
            await conn.ExecuteNonQueryAsync("SET TimeZone='Europe/Berlin'");
            return conn;
        }

        protected override EDBConnection OpenConnection(string? connectionString = null)
            => throw new NotSupportedException();

        [OneTimeSetUp]
        public void Setup()
        {
#if DEBUG
            LegacyTimestampBehavior = true;
            BuiltInTypeHandlerResolver.ResetMappings();
#else
            Assert.Ignore(
                "Legacy DateTime tests rely on the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch and can only be run in DEBUG builds");
#endif
        }

        [OneTimeTearDown]
        public void Teardown()
        {
#if DEBUG
            LegacyTimestampBehavior = false;
            BuiltInTypeHandlerResolver.ResetMappings();
#endif
        }
    }
}
