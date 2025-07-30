using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Configures EDB logging
/// </summary>
public class EDBLoggingConfiguration
{
    internal static readonly EDBLoggingConfiguration NullConfiguration
        = new(NullLoggerFactory.Instance, isParameterLoggingEnabled: false);

    internal static ILoggerFactory GlobalLoggerFactory = NullLoggerFactory.Instance;
    internal static bool GlobalIsParameterLoggingEnabled;

    internal EDBLoggingConfiguration(ILoggerFactory loggerFactory, bool isParameterLoggingEnabled)
    {
        ConnectionLogger = loggerFactory.CreateLogger("EnterpriseDB.EDBClient.Connection");
        CommandLogger = loggerFactory.CreateLogger("EnterpriseDB.EDBClient.Command");
        TransactionLogger = loggerFactory.CreateLogger("EnterpriseDB.EDBClient.Transaction");
        CopyLogger = loggerFactory.CreateLogger("EnterpriseDB.EDBClient.Copy");
        ReplicationLogger = loggerFactory.CreateLogger("EnterpriseDB.EDBClient.Replication");
        ExceptionLogger = loggerFactory.CreateLogger("EnterpriseDB.EDBClient.Exception");

        IsParameterLoggingEnabled = isParameterLoggingEnabled;
    }

    internal ILogger ConnectionLogger { get; }
    internal ILogger CommandLogger { get; }
    internal ILogger TransactionLogger { get; }
    internal ILogger CopyLogger { get; }
    internal ILogger ReplicationLogger { get; }
    internal ILogger ExceptionLogger { get; }

    /// <summary>
    /// Determines whether parameter contents will be logged alongside SQL statements - this may reveal sensitive information.
    /// Defaults to false.
    /// </summary>
    internal bool IsParameterLoggingEnabled { get; }

    /// <summary>
    /// <para>
    /// Globally initializes EDB logging to use the provided <paramref name="loggerFactory" />.
    /// Must be called before any EDB APIs are used.
    /// </para>
    /// <para>
    /// This is a legacy-only, backwards compatibility API. New applications should set the logger factory on
    /// <see cref="EDBDataSourceBuilder" /> and use the resulting <see cref="EDBDataSource "/> instead.
    /// </para>
    /// </summary>
    /// <param name="loggerFactory">The logging factory to use when logging from EnterpriseDB.EDBClient.</param>
    /// <param name="parameterLoggingEnabled">
    /// Determines whether parameter contents will be logged alongside SQL statements - this may reveal sensitive information.
    /// Defaults to <see langword="false" />.
    /// </param>
    public static void InitializeLogging(ILoggerFactory loggerFactory, bool parameterLoggingEnabled = false)
        => (GlobalLoggerFactory, GlobalIsParameterLoggingEnabled) = (loggerFactory, parameterLoggingEnabled);
}