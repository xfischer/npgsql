using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace epascap2latex.Infrastructure;

public class SpectreConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, SpectreConsoleLogger> _loggers = new ();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new SpectreConsoleLogger(name, LogLevel.Trace));
    }

    public void Dispose() => _loggers.Clear();
}
