using System;
using OpenTelemetry.Metrics;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient;

/// <summary>
/// Extension method for setting up Npgsql OpenTelemetry metrics.
/// </summary>
public static class MeterProviderBuilderExtensions
{
    /// <summary>
    /// Subscribes to the Npgsql metrics reporter to enable OpenTelemetry metrics.
    /// </summary>
    public static MeterProviderBuilder AddEDBInstrumentation(
        this MeterProviderBuilder builder,
        Action<EDBMetricsOptions>? options = null)
        => builder.AddMeter("EDB");
}
