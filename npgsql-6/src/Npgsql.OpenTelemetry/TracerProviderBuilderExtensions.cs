using System;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Extension method for setting up EDB OpenTelemetry tracing.
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// Subscribes to the EDB activity source to enable OpenTelemetry tracing.
        /// </summary>
        public static TracerProviderBuilder AddEDB(
            this TracerProviderBuilder builder,
            Action<EDBTracingOptions>? options = null)
            => builder.AddSource("EDB");
    }
}
