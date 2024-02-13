using System;
using System.Data;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal.ResolverFactories;
using EDBTypes;
using NUnit.Framework;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient.Tests.Types;

// Since this test suite manipulates TimeZone, it is incompatible with multiplexing
[NonParallelizable]
public class LegacyDateTimeTests : TestBase
{
    [Test]
    public Task Timestamp_with_all_DateTime_kinds([Values] DateTimeKind kind)
        => AssertType(
            new DateTime(1998, 4, 12, 13, 26, 38, 789, kind),
            "1998-04-12 13:26:38.789",
            "timestamp without time zone",
            EDBDbType.Timestamp,
            DbType.DateTime);

    [Test]
    [TestCase(DateTimeKind.Utc, TestName = "Timestamptz_write_utc_DateTime_does_not_convert")]
    [TestCase(DateTimeKind.Unspecified, TestName = "Timestamptz_write_unspecified_DateTime_does_not_convert")]
    public Task Timestamptz_write_utc_DateTime_does_not_convert(DateTimeKind kind)
        => AssertTypeWrite(
            new DateTime(1998, 4, 12, 13, 26, 38, 789, kind),
            "1998-04-12 15:26:38.789+02",
            "timestamp with time zone",
            EDBDbType.TimestampTz,
            DbType.DateTimeOffset,
            isDefault: false);

    [Test]
    public Task Timestamptz_local_DateTime_converts()
    {
        // In legacy mode, we convert local DateTime to UTC when writing, and convert to local when reading,
        // using the machine time zone.
        var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, 789, DateTimeKind.Utc).ToLocalTime();

        return AssertType(
            dateTime,
            "1998-04-12 15:26:38.789+02",
            "timestamp with time zone",
            EDBDbType.TimestampTz,
            DbType.DateTimeOffset,
            isDefaultForWriting: false);
    }

    EDBDataSource _dataSource = null!;
    protected override EDBDataSource DataSource => _dataSource;

    [OneTimeSetUp]
    public void Setup()
    {
#if DEBUG
        LegacyTimestampBehavior = true;
        _dataSource = CreateDataSource(builder =>
        {
            // Can't use the static AdoTypeInfoResolver instance, it already captured the feature flag.
            builder.AddTypeInfoResolverFactory(new AdoTypeInfoResolverFactory());
            builder.ConnectionStringBuilder.Timezone = "Europe/Berlin";
        });
        EDBDataSourceBuilder.ResetGlobalMappings(overwrite: true);
#else
        Assert.Ignore(
            "Legacy DateTime tests rely on the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch and can only be run in DEBUG builds");
#endif
    }

#if DEBUG
    [OneTimeTearDown]
    public void Teardown()
    {
        LegacyTimestampBehavior = false;
        _dataSource.Dispose();
        EDBDataSourceBuilder.ResetGlobalMappings(overwrite: true);
    }
#endif
}
