using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace EnterpriseDB.EDBClient;

sealed class EDBEventSource : EventSource
{
    public static readonly EDBEventSource Log = new();
    // A static to keep the CWT values from making themselves uncollectable if they would have a reference through the
    // EDBEventSource instance to the CWT table, which they would if this was an instance field.
    static readonly EDBEventSourceDataSources DataSourceEvents = new(Log);

    const string EventSourceName = "edb-dotnet";

    internal const int CommandStartId = 3;
    internal const int CommandStopId = 4;

#if !(NETSTANDARD2_0 || NETFRAMEWORK) // EnterpriseDB (NETFRAMEWORK)
    IncrementingPollingCounter? _bytesWrittenPerSecondCounter;
    IncrementingPollingCounter? _bytesReadPerSecondCounter;

    IncrementingPollingCounter? _commandsPerSecondCounter;
    PollingCounter? _totalCommandsCounter;
    PollingCounter? _failedCommandsCounter;
    PollingCounter? _currentCommandsCounter;
    PollingCounter? _preparedCommandsRatioCounter;

    PollingCounter? _poolsCounter;

    PollingCounter? _multiplexingAverageCommandsPerBatchCounter;
    PollingCounter? _multiplexingAverageWriteTimePerBatchCounter;
#endif

    long _bytesWritten;
    long _bytesRead;

    long _totalCommands;
    long _totalPreparedCommands;
    long _currentCommands;
    long _failedCommands;

    long _multiplexingBatchesSent;
    long _multiplexingCommandsSent;
    long _multiplexingTicksWritten;

    internal EDBEventSource() : base(EventSourceName) {}

    // NOTE
    // - The 'Start' and 'Stop' suffixes on the following event names have special meaning in EventSource. They
    //   enable creating 'activities'.
    //   For more information, take a look at the following blog post:
    //   https://blogs.msdn.microsoft.com/vancem/2015/09/14/exploring-eventsource-activity-correlation-and-causation-features/
    // - A stop event's event id must be next one after its start event.

    internal void BytesWritten(long bytesWritten)
    {
        if (IsEnabled())
            Interlocked.Add(ref _bytesWritten, bytesWritten);
    }

    internal void BytesRead(long bytesRead)
    {
        if (IsEnabled())
            Interlocked.Add(ref _bytesRead, bytesRead);
    }

    public void CommandStart(string sql)
    {
        if (IsEnabled())
        {
            Interlocked.Increment(ref _totalCommands);
            Interlocked.Increment(ref _currentCommands);
        }
        EDBSqlEventSource.Log.CommandStart(sql);
    }

    public void CommandStop()
    {
        if (IsEnabled())
            Interlocked.Decrement(ref _currentCommands);
        EDBSqlEventSource.Log.CommandStop();
    }

    internal void CommandStartPrepared()
    {
        if (IsEnabled())
            Interlocked.Increment(ref _totalPreparedCommands);
    }

    internal void CommandFailed()
    {
        if (IsEnabled())
            Interlocked.Increment(ref _failedCommands);
    }

    internal bool TryTrackDataSource(string name, EDBDataSource dataSource, [NotNullWhen(true)]out IDisposable? untrack)
        => DataSourceEvents.TryTrack(name, dataSource, out untrack);

    internal void MultiplexingBatchSent(int numCommands, long elapsedTicks)
    {
        // TODO: CAS loop instead of 3 separate interlocked operations?
        if (IsEnabled())
        {
            Interlocked.Increment(ref _multiplexingBatchesSent);
            Interlocked.Add(ref _multiplexingCommandsSent, numCommands);
            Interlocked.Add(ref _multiplexingTicksWritten, elapsedTicks);
        }
    }

    double GetDataSourceCount() => DataSourceEvents.GetDataSourceCount();

    double GetMultiplexingAverageCommandsPerBatch()
    {
        var batchesSent = Interlocked.Read(ref _multiplexingBatchesSent);
        if (batchesSent == 0)
            return -1;

        var commandsSent = (double)Interlocked.Read(ref _multiplexingCommandsSent);
        return commandsSent / batchesSent;
    }

    double GetMultiplexingAverageWriteTimePerBatch()
    {
        var batchesSent = Interlocked.Read(ref _multiplexingBatchesSent);
        if (batchesSent == 0)
            return -1;

        var ticksWritten = (double)Interlocked.Read(ref _multiplexingTicksWritten);
        return ticksWritten / batchesSent / 1000;
    }

    protected override void OnEventCommand(EventCommandEventArgs command)
    {
        if (command.Command is EventCommand.Enable)
        {
            #if !(NETSTANDARD2_0 || NETFRAMEWORK) // EnterpriseDB (NETFRAMEWORK)
            // Comment taken from RuntimeEventSource in CoreCLR
            // NOTE: These counters will NOT be disposed on disable command because we may be introducing
            // a race condition by doing that. We still want to create these lazily so that we aren't adding
            // overhead by at all times even when counters aren't enabled.
            // On disable, PollingCounters will stop polling for values so it should be fine to leave them around.

            _bytesWrittenPerSecondCounter = new IncrementingPollingCounter("bytes-written-per-second", this, () => Interlocked.Read(ref _bytesWritten))
            {
                DisplayName = "Bytes Written",
                DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _bytesReadPerSecondCounter = new IncrementingPollingCounter("bytes-read-per-second", this, () => Interlocked.Read(ref _bytesRead))
            {
                DisplayName = "Bytes Read",
                DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _commandsPerSecondCounter = new IncrementingPollingCounter("commands-per-second", this, () => Interlocked.Read(ref _totalCommands))
            {
                DisplayName = "Command Rate",
                DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _totalCommandsCounter = new PollingCounter("total-commands", this, () => Interlocked.Read(ref _totalCommands))
            {
                DisplayName = "Total Commands",
            };

            _currentCommandsCounter = new PollingCounter("current-commands", this, () => Interlocked.Read(ref _currentCommands))
            {
                DisplayName = "Current Commands"
            };

            _failedCommandsCounter = new PollingCounter("failed-commands", this, () => Interlocked.Read(ref _failedCommands))
            {
                DisplayName = "Failed Commands"
            };

            _preparedCommandsRatioCounter = new PollingCounter(
                "prepared-commands-ratio",
                this,
                () => (double)Interlocked.Read(ref _totalPreparedCommands) / Interlocked.Read(ref _totalCommands) * 100)
            {
                DisplayName = "Prepared Commands Ratio",
                DisplayUnits = "%"
            };

            _poolsCounter = new PollingCounter("connection-pools", this, GetDataSourceCount)
            {
                DisplayName = "Connection Pools"
            };

            _multiplexingAverageCommandsPerBatchCounter = new PollingCounter("multiplexing-average-commands-per-batch", this, GetMultiplexingAverageCommandsPerBatch)
            {
                DisplayName = "Average commands per multiplexing batch"
            };

            _multiplexingAverageWriteTimePerBatchCounter = new PollingCounter("multiplexing-average-write-time-per-batch", this, GetMultiplexingAverageWriteTimePerBatch)
            {
                DisplayName = "Average write time per multiplexing batch",
                DisplayUnits = "us"
            };
            #endif

            DataSourceEvents.EnableAll();
        }
    }
}

// This is a separate class to avoid accidentally making the CWT instance reachable through the value.
// The EventSource is stored in the counters, part of the value, so the EventSource *must not* reference this instance on an instance field.
// This goes for any state captured by the value, which is why the other state has its own object for the value to reference.
// See https://github.com/dotnet/runtime/issues/12255.
sealed class EDBEventSourceDataSources(EventSource eventSource)
{
#if NETFRAMEWORK || NETSTANDARD
    readonly EnumerableWeakTable<EDBDataSource, Lazy<DataSourceEvents>> _dataSources = new();
#else
    readonly ConditionalWeakTable<EDBDataSource, Lazy<DataSourceEvents>> _dataSources = new();
#endif
    readonly StrongBox<(int DataSourceCount, ConcurrentDictionary<string, bool> DataSourceNames)> _nonCwtState = new((0, new()));

    internal double GetDataSourceCount() => _nonCwtState.Value.DataSourceCount;

    internal bool TryTrack(string name, EDBDataSource dataSource, [NotNullWhen(true)]out IDisposable? untrack)
    {
        untrack = null;
        if (!_nonCwtState.Value.DataSourceNames.TryAdd(name, default))
            return false;

        var lazy = new Lazy<DataSourceEvents>(
            () => new DataSourceEvents(name: name, dataSource, eventSource, _nonCwtState),
            LazyThreadSafetyMode.ExecutionAndPublication);
#if NETFRAMEWORK || NETSTANDARD
        var tracked = true; _dataSources.Add(dataSource, lazy);
#else
        var tracked = _dataSources.TryAdd(dataSource, lazy);
#endif

        if (tracked)
        {
            Interlocked.Increment(ref _nonCwtState.Value.DataSourceCount);
            // We must initialize directly when the event source is already enabled.
            if (eventSource.IsEnabled())
                untrack = lazy.Value;
            else
                untrack = new DataSourceEventsDisposable(lazy);
        }

        return tracked;
    }

    internal void EnableAll()
    {
#if NETFRAMEWORK || NETSTANDARD
        foreach (var dataSourceKv in _dataSources.ToEnumerable())
#else
        foreach (var dataSourceKv in _dataSources)
#endif
        {
            _ = dataSourceKv.Value.Value;
        }
    }

    sealed class DataSourceEventsDisposable(Lazy<DataSourceEvents> events) : IDisposable
    {
        public void Dispose() => events.Value.Dispose();
    }

    sealed class DataSourceEvents : IDisposable
    {
        readonly string _name;
        readonly StrongBox<(int Count, ConcurrentDictionary<string, bool> Names)> _state;
#if !(NETFRAMEWORK || NETSTANDARD2_0)
        readonly PollingCounter _idleConnections;
        readonly PollingCounter _busyConnections;
#endif
        int _disposed;

        public DataSourceEvents(string name, EDBDataSource dataSource, EventSource eventSource, StrongBox<(int, ConcurrentDictionary<string, bool>)> state)
        {
            _name = name;
            _state = state;
#if !(NETFRAMEWORK || NETSTANDARD2_0)
            _idleConnections = new($"idle-connections-{name}", eventSource, () => dataSource.Statistics.Idle)
            {
                DisplayName = $"Idle Connections [{name}]"
            };
            _busyConnections = new($"busy-connections-{name}", eventSource, () => dataSource.Statistics.Busy)
            {
                DisplayName = $"Busy Connections [{name}]"
            };
#endif
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) is 1)
                return;
#if !(NETFRAMEWORK || NETSTANDARD2_0)
            _idleConnections.Dispose();
            _busyConnections.Dispose();
#endif

            Interlocked.Decrement(ref _state.Value.Count);
            var success = _state.Value.Names.TryRemove(_name, out _);
            Debug.Assert(success);
        }
    }
//#endif
}
