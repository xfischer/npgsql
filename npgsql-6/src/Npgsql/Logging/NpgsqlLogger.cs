using System;

#pragma warning disable 1591

namespace EnterpriseDB.EDBClient.Logging
{
    /// <summary>
    /// A generic interface for logging.
    /// </summary>
    public abstract class EDBLogger
    {
        public abstract bool IsEnabled(EDBLogLevel level);
        public abstract void Log(EDBLogLevel level, int connectorId, string msg, Exception? exception = null);

        internal void Trace(string msg, int connectionId = 0) => Log(EDBLogLevel.Trace, connectionId, msg);
        internal void Debug(string msg, int connectionId = 0) => Log(EDBLogLevel.Debug, connectionId, msg);
        internal void Info(string msg, int connectionId = 0) => Log(EDBLogLevel.Info, connectionId, msg);
        internal void Warn(string msg, int connectionId = 0) => Log(EDBLogLevel.Warn, connectionId, msg);
        internal void Error(string msg, int connectionId = 0) => Log(EDBLogLevel.Error, connectionId, msg);
        internal void Fatal(string msg, int connectionId = 0) => Log(EDBLogLevel.Fatal, connectionId, msg);

        internal void Trace(string msg, Exception ex, int connectionId = 0) => Log(EDBLogLevel.Trace, connectionId, msg, ex);
        internal void Debug(string msg, Exception ex, int connectionId = 0) => Log(EDBLogLevel.Debug, connectionId, msg, ex);
        internal void Info(string msg, Exception ex, int connectionId = 0) => Log(EDBLogLevel.Info, connectionId, msg, ex);
        internal void Warn(string msg, Exception ex, int connectionId = 0) => Log(EDBLogLevel.Warn, connectionId, msg, ex);
        internal void Error(string msg, Exception ex, int connectionId = 0) => Log(EDBLogLevel.Error, connectionId, msg, ex);
        internal void Fatal(string msg, Exception ex, int connectionId = 0) => Log(EDBLogLevel.Fatal, connectionId, msg, ex);
    }
}
