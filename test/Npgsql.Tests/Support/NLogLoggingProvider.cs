using System;
using NLog;
using EnterpriseDB.EDBClient.Logging;

namespace EnterpriseDB.EDBClient.Tests.Support
{
    class NLogLoggingProvider : IEDBLoggingProvider
    {
        public EDBLogger CreateLogger(string name)
        {
            return new NLogLogger(name);
        }
    }

    class NLogLogger : EDBLogger
    {
        readonly Logger _log;

        internal NLogLogger(string name)
        {
            _log = LogManager.GetLogger(name);
        }

        public override bool IsEnabled(EDBLogLevel level)
        {
            return _log.IsEnabled(ToNLogLogLevel(level));
        }

        public override void Log(EDBLogLevel level, int connectorId, string msg, Exception? exception = null)
        {
            var ev = new LogEventInfo(ToNLogLogLevel(level), "", msg);
            if (exception != null)
                ev.Exception = exception;
            if (connectorId != 0)
                ev.Properties["ConnectorId"] = connectorId;
            _log.Log(ev);
        }

        static LogLevel ToNLogLogLevel(EDBLogLevel level)
            => level switch
            {
                EDBLogLevel.Trace => LogLevel.Trace,
                EDBLogLevel.Debug => LogLevel.Debug,
                EDBLogLevel.Info  => LogLevel.Info,
                EDBLogLevel.Warn  => LogLevel.Warn,
                EDBLogLevel.Error => LogLevel.Error,
                EDBLogLevel.Fatal => LogLevel.Fatal,
                _                    => throw new ArgumentOutOfRangeException(nameof(level))
            };
    }
}
