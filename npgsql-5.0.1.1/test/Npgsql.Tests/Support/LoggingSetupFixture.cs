using System;
using NUnit.Framework;
using NLog.Config;
using NLog.Targets;
using NLog;
using EnterpriseDB.EDBClient.Logging;
using EnterpriseDB.EDBClient.Tests;
using EnterpriseDB.EDBClient.Tests.Support;

// ReSharper disable once CheckNamespace

[SetUpFixture]
public class LoggingSetupFixture
{
    [OneTimeSetUp]
    public void Setup()
    {
        var logLevelText = Environment.GetEnvironmentVariable("NPGSQL_TEST_LOGGING");
        if (logLevelText == null)
            return;
        if (!Enum.TryParse(logLevelText, true, out EDBLogLevel logLevel))
            throw new ArgumentOutOfRangeException($"Invalid loglevel in NPGSQL_TEST_LOGGING: {logLevelText}");

        var config = new LoggingConfiguration();
        var consoleTarget = new ColoredConsoleTarget
        {
            Layout = @"${message} ${exception:format=tostring}"
        };
        config.AddTarget("console", consoleTarget);
        var rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
        config.LoggingRules.Add(rule);
        LogManager.Configuration = config;

        EDBLogManager.Provider = new NLogLoggingProvider();
        EDBLogManager.IsParameterLoggingEnabled = true;
    }
}
