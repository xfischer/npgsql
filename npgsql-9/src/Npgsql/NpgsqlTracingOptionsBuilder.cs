using System;
using System.Diagnostics;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// A builder to configure EDB .NET Connector's support for OpenTelemetry tracing.
/// </summary>
public sealed class EDBTracingOptionsBuilder
{
    Func<EDBCommand, bool>? _commandFilter;
    Func<EDBBatch, bool>? _batchFilter;
    Action<Activity, EDBCommand>? _commandEnrichmentCallback;
    Action<Activity, EDBBatch>? _batchEnrichmentCallback;
    Func<EDBCommand, string?>? _commandSpanNameProvider;
    Func<EDBBatch, string?>? _batchSpanNameProvider;
    bool _enableFirstResponseEvent = true;

    internal EDBTracingOptionsBuilder()
    {
    }

    /// <summary>
    /// Configures a filter function that determines whether to emit tracing information for an <see cref="EDBCommand"/>.
    /// By default, tracing information is emitted for all commands.
    /// </summary>
    public EDBTracingOptionsBuilder ConfigureCommandFilter(Func<EDBCommand, bool>? commandFilter)
    {
        _commandFilter = commandFilter;
        return this;
    }

    /// <summary>
    /// Configures a filter function that determines whether to emit tracing information for an <see cref="EDBBatch"/>.
    /// By default, tracing information is emitted for all batches.
    /// </summary>
    public EDBTracingOptionsBuilder ConfigureBatchFilter(Func<EDBBatch, bool>? batchFilter)
    {
        _batchFilter = batchFilter;
        return this;
    }

    /// <summary>
    /// Configures a callback that can enrich the <see cref="Activity"/> emitted for the given <see cref="EDBCommand"/>.
    /// </summary>
    public EDBTracingOptionsBuilder ConfigureCommandEnrichmentCallback(Action<Activity, EDBCommand>? commandEnrichmentCallback)
    {
        _commandEnrichmentCallback = commandEnrichmentCallback;
        return this;
    }

    /// <summary>
    /// Configures a callback that can enrich the <see cref="Activity"/> emitted for the given <see cref="EDBBatch"/>.
    /// </summary>
    public EDBTracingOptionsBuilder ConfigureBatchEnrichmentCallback(Action<Activity, EDBBatch>? batchEnrichmentCallback)
    {
        _batchEnrichmentCallback = batchEnrichmentCallback;
        return this;
    }

    /// <summary>
    /// Configures a callback that provides the tracing span's name for an <see cref="EDBCommand"/>. If <c>null</c>, the default standard
    /// span name is used, which is the database name.
    /// </summary>
    public EDBTracingOptionsBuilder ConfigureCommandSpanNameProvider(Func<EDBCommand, string?>? commandSpanNameProvider)
    {
        _commandSpanNameProvider = commandSpanNameProvider;
        return this;
    }

    /// <summary>
    /// Configures a callback that provides the tracing span's name for an <see cref="EDBBatch"/>. If <c>null</c>, the default standard
    /// span name is used, which is the database name.
    /// </summary>
    public EDBTracingOptionsBuilder ConfigureBatchSpanNameProvider(Func<EDBBatch, string?>? batchSpanNameProvider)
    {
        _batchSpanNameProvider = batchSpanNameProvider;
        return this;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to enable the "time-to-first-read" event.
    /// Default is true to preserve existing behavior.
    /// </summary>
    public EDBTracingOptionsBuilder EnableFirstResponseEvent(bool enable = true)
    {
        _enableFirstResponseEvent = enable;
        return this;
    }

    internal EDBTracingOptions Build() => new()
    {
        CommandFilter = _commandFilter,
        BatchFilter = _batchFilter,
        CommandEnrichmentCallback = _commandEnrichmentCallback,
        BatchEnrichmentCallback = _batchEnrichmentCallback,
        CommandSpanNameProvider = _commandSpanNameProvider,
        BatchSpanNameProvider = _batchSpanNameProvider,
        EnableFirstResponseEvent = _enableFirstResponseEvent
    };
}

sealed class EDBTracingOptions
{
    internal Func<EDBCommand, bool>? CommandFilter { get; init; }
    internal Func<EDBBatch, bool>? BatchFilter { get; init; }
    internal Action<Activity, EDBCommand>? CommandEnrichmentCallback { get; init; }
    internal Action<Activity, EDBBatch>? BatchEnrichmentCallback { get; init; }
    internal Func<EDBCommand, string?>? CommandSpanNameProvider { get; init; }
    internal Func<EDBBatch, string?>? BatchSpanNameProvider { get; init; }
    internal bool EnableFirstResponseEvent { get; init; }
}
