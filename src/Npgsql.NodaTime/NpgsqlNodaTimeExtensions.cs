using System;
using System.Data;
using NodaTime;
using EnterpriseDB.EDBClient.NodaTime;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Extension adding the NodaTime plugin to an EnterpriseDB.EDBClient type mapper.
    /// </summary>
    public static class EDBNodaTimeExtensions
    {
        /// <summary>
        /// Sets up NodaTime mappings for the PostgreSQL date/time types.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
        public static IEDBTypeMapper UseNodaTime(this IEDBTypeMapper mapper)
            => mapper
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "timestamp",
                    EDBDbType = EDBDbType.Timestamp,
                    DbTypes = new[] { DbType.DateTime, DbType.DateTime2 },
                    ClrTypes = new[] { typeof(Instant), typeof(LocalDateTime), typeof(DateTime) },
                    InferredDbType = DbType.DateTime,
                    TypeHandlerFactory = new TimestampHandlerFactory()
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "timestamp with time zone",
                    EDBDbType = EDBDbType.TimestampTz,
                    ClrTypes = new[] { typeof(ZonedDateTime), typeof(OffsetDateTime), typeof(DateTimeOffset) },
                    TypeHandlerFactory = new TimestampTzHandlerFactory()
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "date",
                    EDBDbType = EDBDbType.Date,
                    DbTypes = new[] { DbType.Date },
                    ClrTypes = new[] { typeof(LocalDate),  typeof(EDBDate) },
                    TypeHandlerFactory = new DateHandlerFactory()
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "time",
                    EDBDbType = EDBDbType.Time,
                    DbTypes = new[] { DbType.Time },
                    ClrTypes = new[] { typeof(LocalTime) },
                    TypeHandlerFactory = new TimeHandlerFactory()
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "time with time zone",
                    EDBDbType = EDBDbType.TimeTz,
                    ClrTypes = new[] { typeof(OffsetTime) },
                    TypeHandlerFactory = new TimeTzHandlerFactory()
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "interval",
                    EDBDbType = EDBDbType.Interval,
                    ClrTypes = new[] { typeof(Period), typeof(TimeSpan), typeof(EDBTimeSpan) },
                    TypeHandlerFactory = new IntervalHandlerFactory()
                }.Build());
    }
}
