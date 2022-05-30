using System;

namespace EnterpriseDB.EDBClient.Logging
{
    class NoOpLoggingProvider : IEDBLoggingProvider
    {
        public EDBLogger CreateLogger(string name) => NoOpLogger.Instance;
    }

    class NoOpLogger : EDBLogger
    {
        internal static NoOpLogger Instance = new();

        NoOpLogger() {}
        public override bool IsEnabled(EDBLogLevel level) => false;
        public override void Log(EDBLogLevel level, int connectorId, string msg, Exception? exception = null)
        {
        }
    }
}
