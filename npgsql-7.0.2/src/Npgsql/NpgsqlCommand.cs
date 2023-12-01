using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient.Internal;
using System.Text.RegularExpressions;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Represents a SQL statement or function (stored procedure) to execute
/// against a PostgreSQL database. This class cannot be inherited.
/// </summary>
// ReSharper disable once RedundantNameQualifier
[System.ComponentModel.DesignerCategory("")]
public class EDBCommand : DbCommand, ICloneable, IComponent
#if NETFRAMEWORK || NETSTANDARD2_0 // EnterpriseDB
    , IAsyncDisposable
#endif
{
    #region Fields

    EDBTransaction? _transaction;

    readonly EDBConnector? _connector;

    /// <summary>
    /// If this command is (explicitly) prepared, references the connector on which the preparation happened.
    /// Used to detect when the connector was changed (i.e. connection open/close), meaning that the command
    /// is no longer prepared.
    /// </summary>
    EDBConnector? _connectorPreparedOn;

    string _commandText;
    CommandBehavior _behavior;
    int? _timeout;
    readonly EDBParameterCollection _parameters;

    /// <summary>
    /// Whether this <see cref="EDBCommand" /> is wrapped by an <see cref="EDBBatch" />.
    /// </summary>
    internal bool IsWrappedByBatch { get; }

    internal List<EDBBatchCommand> InternalBatchCommands { get; }

    Activity? CurrentActivity;

    /// <summary>
    /// Returns details about each statement that this command has executed.
    /// Is only populated when an Execute* method is called.
    /// </summary>
    [Obsolete("Use the new DbBatch API")]
    public IReadOnlyList<EDBBatchCommand> Statements => InternalBatchCommands.AsReadOnly();

    UpdateRowSource _updateRowSource = UpdateRowSource.Both;

    bool IsExplicitlyPrepared => _connectorPreparedOn != null;

    /// <summary>
    /// Whether this command is cached by <see cref="EDBConnection" /> and returned by <see cref="EDBConnection.CreateCommand" />.
    /// </summary>
    internal bool IsCached { get; set; }

#if DEBUG
    internal static bool EnableSqlRewriting;
    internal static bool EnableStoredProcedureCompatMode;
#else
    internal static readonly bool EnableSqlRewriting;
    internal static readonly bool EnableStoredProcedureCompatMode;
#endif

    internal bool EnableErrorBarriers { get; set; }

    static readonly List<EDBParameter> EmptyParameters = new();

    static readonly SingleThreadSynchronizationContext SingleThreadSynchronizationContext = new("EDBRemainingAsyncSendWorker");

    #endregion Fields

    #region Constants

    internal const int DefaultTimeout = 30;

    #endregion

    #region Constructors

    static EDBCommand()
    {
        EnableSqlRewriting = !AppContext.TryGetSwitch("EnterpriseDB.EDBClient.EnableSqlRewriting", out var enabled) || enabled;
        EnableStoredProcedureCompatMode = AppContext.TryGetSwitch("EnterpriseDB.EDBClient.EnableStoredProcedureCompatMode", out enabled) && enabled;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBCommand"/> class.
    /// </summary>
    public EDBCommand() : this(null, null, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBCommand"/> class with the text of the query.
    /// </summary>
    /// <param name="cmdText">The text of the query.</param>
    // ReSharper disable once IntroduceOptionalParameters.Global
    public EDBCommand(string? cmdText) : this(cmdText, null, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBCommand"/> class with the text of the query and a
    /// <see cref="EDBConnection"/>.
    /// </summary>
    /// <param name="cmdText">The text of the query.</param>
    /// <param name="connection">A <see cref="EDBConnection"/> that represents the connection to a PostgreSQL server.</param>
    // ReSharper disable once IntroduceOptionalParameters.Global
    public EDBCommand(string? cmdText, EDBConnection? connection)
    {
        GC.SuppressFinalize(this);
        InternalBatchCommands = new List<EDBBatchCommand>(1);
        _parameters = new EDBParameterCollection();
        _commandText = cmdText ?? string.Empty;
        InternalConnection = connection;
        CommandType = CommandType.Text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EDBCommand"/> class with the text of the query, a
    /// <see cref="EDBConnection"/>, and the <see cref="EDBTransaction"/>.
    /// </summary>
    /// <param name="cmdText">The text of the query.</param>
    /// <param name="connection">A <see cref="EDBConnection"/> that represents the connection to a PostgreSQL server.</param>
    /// <param name="transaction">The <see cref="EDBTransaction"/> in which the <see cref="EDBCommand"/> executes.</param>
    public EDBCommand(string? cmdText, EDBConnection? connection, EDBTransaction? transaction)
        : this(cmdText, connection)
        => Transaction = transaction;

    /// <summary>
    /// Used when this <see cref="EDBCommand"/> instance is wrapped inside an <see cref="EDBBatch"/>.
    /// </summary>
    internal EDBCommand(int batchCommandCapacity, EDBConnection? connection = null)
    {
        GC.SuppressFinalize(this);
        InternalBatchCommands = new List<EDBBatchCommand>(batchCommandCapacity);
        InternalConnection = connection;
        CommandType = CommandType.Text;
        IsWrappedByBatch = true;

        // These can/should never be used in this mode
        _commandText = null!;
        _parameters = null!;
    }

    internal EDBCommand(string? cmdText, EDBConnector connector) : this(cmdText)
        => _connector = connector;

    /// <summary>
    /// Used when this <see cref="EDBCommand"/> instance is wrapped inside an <see cref="EDBBatch"/>.
    /// </summary>
    internal EDBCommand(EDBConnector connector, int batchCommandCapacity)
        : this(batchCommandCapacity)
        => _connector = connector;

    internal static EDBCommand CreateCachedCommand(EDBConnection connection)
        => new(null, connection) { IsCached = true };

    #endregion Constructors

    #region Public properties

    /// <summary>
    /// Gets or sets the SQL statement or function (stored procedure) to execute at the data source.
    /// </summary>
    /// <value>The SQL statement or function (stored procedure) to execute. The default is an empty string.</value>
    [AllowNull, DefaultValue("")]
    [Category("Data")]
    public override string CommandText
    {
        get => _commandText;
        set
        {
            Debug.Assert(!IsWrappedByBatch);

            _commandText = State == CommandState.Idle
                ? value ?? string.Empty
                : throw new InvalidOperationException("An open data reader exists for this command.");

            ResetPreparation();
            // TODO: Technically should do this also if the parameter list (or type) changes
        }
    }

    /// <summary>
    /// Gets or sets the wait time (in seconds) before terminating the attempt  to execute a command and generating an error.
    /// </summary>
    /// <value>The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</value>
    [DefaultValue(DefaultTimeout)]
    public override int CommandTimeout
    {
        get => _timeout ?? (InternalConnection?.CommandTimeout ?? DefaultTimeout);
        set
        {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), value, "CommandTimeout can't be less than zero.");
            }

            _timeout = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating how the <see cref="EDBCommand.CommandText"/> property is to be interpreted.
    /// </summary>
    /// <value>
    /// One of the <see cref="System.Data.CommandType"/> values. The default is <see cref="System.Data.CommandType.Text"/>.
    /// </value>
    [DefaultValue(CommandType.Text)]
    [Category("Data")]
    public override CommandType CommandType { get; set; }

    internal EDBConnection? InternalConnection { get; private set; }

    /// <summary>
    /// DB connection.
    /// </summary>
    protected override DbConnection? DbConnection
    {
        get => InternalConnection;
        set
        {
            if (InternalConnection == value)
                return;

            InternalConnection = State == CommandState.Idle
                ? (EDBConnection?)value
                : throw new InvalidOperationException("An open data reader exists for this command.");

            Transaction = null;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="EDBConnection"/> used by this instance of the <see cref="EDBCommand"/>.
    /// </summary>
    /// <value>The connection to a data source. The default value is <see langword="null"/>.</value>
    [DefaultValue(null)]
    [Category("Behavior")]
    public new EDBConnection? Connection
    {
        get => (EDBConnection?)DbConnection;
        set => DbConnection = value;
    }

    /// <summary>
    /// Design time visible.
    /// </summary>
    public override bool DesignTimeVisible { get; set; }

    /// <summary>
    /// Gets or sets how command results are applied to the DataRow when used by the
    /// DbDataAdapter.Update(DataSet) method.
    /// </summary>
    /// <value>One of the <see cref="System.Data.UpdateRowSource"/> values.</value>
    [Category("Behavior"), DefaultValue(UpdateRowSource.Both)]
    public override UpdateRowSource UpdatedRowSource
    {
        get => _updateRowSource;
        set
        {
            switch (value)
            {
            // validate value (required based on base type contract)
            case UpdateRowSource.None:
            case UpdateRowSource.OutputParameters:
            case UpdateRowSource.FirstReturnedRecord:
            case UpdateRowSource.Both:
                _updateRowSource = value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Returns whether this query will execute as a prepared (compiled) query.
    /// </summary>
    public bool IsPrepared =>
        _connectorPreparedOn == (InternalConnection?.Connector ?? _connector) &&
        InternalBatchCommands.Any() && InternalBatchCommands.All(s => s.PreparedStatement?.IsPrepared == true);

    #endregion Public properties

    #region Known/unknown Result Types Management

    /// <summary>
    /// Marks all of the query's result columns as either known or unknown.
    /// Unknown result columns are requested from PostgreSQL in text format, and EDB makes no
    /// attempt to parse them. They will be accessible as strings only.
    /// </summary>
    public bool AllResultTypesAreUnknown
    {
        get => _allResultTypesAreUnknown;
        set
        {
            // TODO: Check that this isn't modified after calling prepare
            _unknownResultTypeList = null;
            _allResultTypesAreUnknown = value;
        }
    }

    bool _allResultTypesAreUnknown;

    /// <summary>
    /// Marks the query's result columns as known or unknown, on a column-by-column basis.
    /// Unknown result columns are requested from PostgreSQL in text format, and EDB makes no
    /// attempt to parse them. They will be accessible as strings only.
    /// </summary>
    /// <remarks>
    /// If the query includes several queries (e.g. SELECT 1; SELECT 2), this will only apply to the first
    /// one. The rest of the queries will be fetched and parsed as usual.
    ///
    /// The array size must correspond exactly to the number of result columns the query returns, or an
    /// error will be raised.
    /// </remarks>
    public bool[]? UnknownResultTypeList
    {
        get => _unknownResultTypeList;
        set
        {
            // TODO: Check that this isn't modified after calling prepare
            _allResultTypesAreUnknown = false;
            _unknownResultTypeList = value;
        }
    }

    bool[]? _unknownResultTypeList;

    #endregion

    #region Result Types Management

    /// <summary>
    /// Marks result types to be used when using GetValue on a data reader, on a column-by-column basis.
    /// Used for Entity Framework 5-6 compability.
    /// Only primitive numerical types and DateTimeOffset are supported.
    /// Set the whole array or just a value to null to use default type.
    /// </summary>
    internal Type[]? ObjectResultTypes { get; set; }

    #endregion

    #region State management

    volatile int _state;

    /// <summary>
    /// The current state of the command
    /// </summary>
    internal CommandState State
    {
        get => (CommandState)_state;
        set
        {
            var newState = (int)value;
            if (newState == _state)
                return;
            _state = newState;
        }
    }

    internal void ResetPreparation() => _connectorPreparedOn = null;

    #endregion State management

    #region Parameters

    /// <summary>
    /// Creates a new instance of an <see cref="System.Data.Common.DbParameter"/> object.
    /// </summary>
    /// <returns>A <see cref="System.Data.Common.DbParameter"/> object.</returns>
    protected override DbParameter CreateDbParameter() => CreateParameter();

    /// <summary>
    /// Creates a new instance of a <see cref="EDBParameter"/> object.
    /// </summary>
    /// <returns>An <see cref="EDBParameter"/> object.</returns>
    public new EDBParameter CreateParameter() => new();

    /// <summary>
    /// DB parameter collection.
    /// </summary>
    protected override DbParameterCollection DbParameterCollection => Parameters;

    /// <summary>
    /// Gets the <see cref="EDBParameterCollection"/>.
    /// </summary>
    /// <value>The parameters of the SQL statement or function (stored procedure). The default is an empty collection.</value>
    public new EDBParameterCollection Parameters => _parameters;

    #endregion

    #region DeriveParameters

    const string DeriveParametersForFunctionQuery = @"
SELECT
CASE
	WHEN pg_proc.proargnames IS NULL THEN array_cat(array_fill(''::name,ARRAY[pg_proc.pronargs]),array_agg(pg_attribute.attname ORDER BY pg_attribute.attnum))
	ELSE pg_proc.proargnames
END AS proargnames,
pg_proc.proargtypes,
CASE
	WHEN pg_proc.proallargtypes IS NULL AND (array_agg(pg_attribute.atttypid))[1] IS NOT NULL THEN array_cat(string_to_array(pg_proc.proargtypes::text,' ')::oid[],array_agg(pg_attribute.atttypid ORDER BY pg_attribute.attnum))
	ELSE pg_proc.proallargtypes
END AS proallargtypes,
CASE
	WHEN pg_proc.proargmodes IS NULL AND (array_agg(pg_attribute.atttypid))[1] IS NOT NULL THEN array_cat(array_fill('i'::""char"",ARRAY[pg_proc.pronargs]),array_fill('o'::""char"",ARRAY[array_length(array_agg(pg_attribute.atttypid), 1)]))
    ELSE pg_proc.proargmodes
END AS proargmodes
FROM pg_proc
LEFT JOIN pg_type ON pg_proc.prorettype = pg_type.oid
LEFT JOIN pg_attribute ON pg_type.typrelid = pg_attribute.attrelid AND pg_attribute.attnum >= 1 AND NOT pg_attribute.attisdropped
WHERE pg_proc.oid = :proname::regproc
GROUP BY pg_proc.proargnames, pg_proc.proargtypes, pg_proc.proallargtypes, pg_proc.proargmodes, pg_proc.pronargs;
";

    internal void DeriveParameters()
    {
        var conn = CheckAndGetConnection();
        Debug.Assert(conn is not null);

        if (string.IsNullOrEmpty(CommandText))
            throw new InvalidOperationException("CommandText property has not been initialized");

        using var _ = conn.StartTemporaryBindingScope(out var connector);

        if (InternalBatchCommands.Any(s => s.PreparedStatement?.IsExplicit == true))
            throw new EDBException("Deriving parameters isn't supported for commands that are already prepared.");

        // Here we unprepare statements that possibly are auto-prepared
        Unprepare();

        Parameters.Clear();

        switch (CommandType)
        {
        case CommandType.Text:
            DeriveParametersForQuery(connector);
            break;
        case CommandType.StoredProcedure:
            DeriveParametersForFunction();
            break;
        default:
            throw new NotSupportedException("Cannot derive parameters for CommandType " + CommandType);
        }
    }

    void DeriveParametersForFunction()
    {
        using var c = new EDBCommand(DeriveParametersForFunctionQuery, InternalConnection);
        c.Parameters.Add(new EDBParameter("proname", EDBDbType.Text));
        c.Parameters[0].Value = CommandText;

        string[]? names = null;
        uint[]? types = null;
        char[]? modes = null;
        bool? hasParams = false; //EnterpriseDB Team
        string? paramNames = null; //EnterpriseDB Team

        using (var rdr = c.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult))
        {
            if (rdr.Read())
            {
                if (!rdr.IsDBNull(0))
                    names = rdr.GetFieldValue<string[]>(0);
                if (!rdr.IsDBNull(2))
                    types = rdr.GetFieldValue<uint[]>(2);
                if (!rdr.IsDBNull(3))
                    modes = rdr.GetFieldValue<char[]>(3);
                if (types == null)
                {
                    if (rdr.IsDBNull(1) || rdr.GetFieldValue<uint[]>(1).Length == 0)
                        return;  // Parameter-less function
                    types = rdr.GetFieldValue<uint[]>(1);
                }
            }
            else
                throw new InvalidOperationException($"{CommandText} does not exist in pg_proc");
        }

        var typeMapper = c.InternalConnection!.Connector!.TypeMapper;

        for (var i = 0; i < types.Length; i++)
        {
            var param = new EDBParameter();
            hasParams = true; //EnterpriseDB Team

            var (npgsqlDbType, postgresType) = typeMapper.GetTypeInfoByOid(types[i]);

            param.DataTypeName = postgresType.DisplayName;
            param.PostgresType = postgresType;
            if (npgsqlDbType.HasValue)
                param.EDBDbType = npgsqlDbType.Value;

            if (names != null && i < names.Length)
                param.ParameterName = names[i];
            else
                param.ParameterName = "parameter" + (i + 1);

            if (modes == null) // All params are IN, or server < 8.1.0 (and only IN is supported)
                param.Direction = ParameterDirection.Input;
            else
            {
                param.Direction = modes[i] switch
                {
                    'i' => ParameterDirection.Input,
                    'o' => ParameterDirection.Output,
                    't' => ParameterDirection.Output,
                    'b' => ParameterDirection.InputOutput,
                    'v' => throw new NotSupportedException("Cannot derive function parameter of type VARIADIC"),
                    _ => throw new ArgumentOutOfRangeException("Unknown code in proargmodes while deriving: " + modes[i])
                };
            }

            paramNames = paramNames + ":" + param.ParameterName + ", "; //EnterpriseDB Team
            Parameters.Add(param);
        }

        if (hasParams.HasValue && CommandType == CommandType.StoredProcedure && paramNames is not null) //EnterpriseDB Team
        {
            if (paramNames.Trim().EndsWith(",", StringComparison.OrdinalIgnoreCase))
            {
                paramNames = paramNames.Substring(0, paramNames.LastIndexOf(",", StringComparison.OrdinalIgnoreCase));
            }
            CommandText = CommandText + "(" + paramNames + ")";
        }

    }

    void DeriveParametersForQuery(EDBConnector connector)
    {
        using (connector.StartUserAction())
        {
            LogMessages.DerivingParameters(connector.CommandLogger, CommandText, connector.Id);

            if (IsWrappedByBatch)
                foreach (var batchCommand in InternalBatchCommands)
                    connector.SqlQueryParser.ParseRawQuery(batchCommand, connector.UseConformingStrings, deriveParameters: true);
            else
                connector.SqlQueryParser.ParseRawQuery(this, connector.UseConformingStrings, deriveParameters: true);

            var sendTask = SendDeriveParameters(connector, false);
            if (sendTask.IsFaulted)
                sendTask.GetAwaiter().GetResult();

            try
            {
                foreach (var batchCommand in InternalBatchCommands)
                {
                    Expect<ParseCompleteMessage>(
                        connector.ReadMessage(async: false).GetAwaiter().GetResult(), connector);
                    var paramTypeOIDs = Expect<ParameterDescriptionMessage>(
                        connector.ReadMessage(async: false).GetAwaiter().GetResult(), connector).TypeOIDs;

                    if (batchCommand.PositionalParameters.Count != paramTypeOIDs.Count)
                    {
                        connector.SkipUntil(BackendMessageCode.ReadyForQuery);
                        Parameters.Clear();
                        throw new EDBException("There was a mismatch in the number of derived parameters between the EDB SQL parser and the PostgreSQL parser. Please report this as bug to the EDB developers (https://github.com/npgsql/npgsql/issues).");
                    }

                    for (var i = 0; i < paramTypeOIDs.Count; i++)
                    {
                        try
                        {
                            var param = batchCommand.PositionalParameters[i];
                            var paramOid = paramTypeOIDs[i];

                            var (npgsqlDbType, postgresType) = connector.TypeMapper.GetTypeInfoByOid(paramOid);

                            if (param.EDBDbType != EDBDbType.Unknown && param.EDBDbType != npgsqlDbType)
                                throw new EDBException("The backend parser inferred different types for parameters with the same name. Please try explicit casting within your SQL statement or batch or use different placeholder names.");

                            param.DataTypeName = postgresType.DisplayName;
                            param.PostgresType = postgresType;
                            if (npgsqlDbType.HasValue)
                                param.EDBDbType = npgsqlDbType.Value;
                        }
                        catch
                        {
                            connector.SkipUntil(BackendMessageCode.ReadyForQuery);
                            Parameters.Clear();
                            throw;
                        }
                    }

                    var msg = connector.ReadMessage(async: false).GetAwaiter().GetResult();
                    switch (msg.Code)
                    {
                    case BackendMessageCode.RowDescription:
                    case BackendMessageCode.NoData:
                        break;
                    default:
                        throw connector.UnexpectedMessageReceived(msg.Code);
                    }
                }

                Expect<ReadyForQueryMessage>(connector.ReadMessage(async: false).GetAwaiter().GetResult(), connector);
            }
            finally
            {
                try
                {
                    // Make sure sendTask is complete so we don't race against asynchronous flush
                    sendTask.GetAwaiter().GetResult();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    #endregion

    #region Prepare

    /// <summary>
    /// Creates a server-side prepared statement on the PostgreSQL server.
    /// This will make repeated future executions of this command much faster.
    /// </summary>
    public override void Prepare() => Prepare(false).GetAwaiter().GetResult();

    /// <summary>
    /// Creates a server-side prepared statement on the PostgreSQL server.
    /// This will make repeated future executions of this command much faster.
    /// </summary>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
#if NETSTANDARD2_0 || NETFRAMEWORK //EnterpriseDB Team (NETFRAMEWORK)
    public virtual Task PrepareAsync(CancellationToken cancellationToken = default)
#else
    public override Task PrepareAsync(CancellationToken cancellationToken = default)
#endif
    {
        using (NoSynchronizationContextScope.Enter())
            return Prepare(true, cancellationToken);
    }

    Task Prepare(bool async, CancellationToken cancellationToken = default)
    {
        var connection = CheckAndGetConnection();
        Debug.Assert(connection is not null);
        if (connection.Settings.Multiplexing)
            throw new NotSupportedException("Explicit preparation not supported with multiplexing");
        var connector = connection.Connector!;
        var logger = connector.CommandLogger;

        var needToPrepare = false;

        if (IsWrappedByBatch)
        {
            foreach (var batchCommand in InternalBatchCommands)
            {
                batchCommand.Parameters.ProcessParameters(connector.TypeMapper, validateValues: false, CommandType);
                ProcessRawQuery(connector.SqlQueryParser, connector.UseConformingStrings, batchCommand, connector.DataSource.DatabaseInfo.SupportsRedwoodDialect); //EnterpriseDB Team (additional param)

                needToPrepare = batchCommand.ExplicitPrepare(connector) || needToPrepare;
            }

            if (logger.IsEnabled(LogLevel.Debug) && needToPrepare)
                LogMessages.PreparingCommandExplicitly(logger, string.Join("; ", InternalBatchCommands.Select(c => c.CommandText)), connector.Id);
        }
        else
        {
            Parameters.ProcessParameters(connector.TypeMapper, validateValues: false, CommandType);
            ProcessRawQuery(connector.SqlQueryParser, connector.UseConformingStrings, batchCommand: null, connector.DataSource.DatabaseInfo.SupportsRedwoodDialect); //EnterpriseDB Team (additional param)

            foreach (var batchCommand in InternalBatchCommands)
                needToPrepare = batchCommand.ExplicitPrepare(connector) || needToPrepare;

            if (logger.IsEnabled(LogLevel.Debug) && needToPrepare)
                LogMessages.PreparingCommandExplicitly(logger, CommandText, connector.Id);
        }

        _connectorPreparedOn = connector;

        // It's possible the command was already prepared, or that persistent prepared statements were found for
        // all statements. Nothing to do here, move along.
        return needToPrepare
            ? PrepareLong(this, async, connector, cancellationToken)
            : Task.CompletedTask;

        static async Task PrepareLong(EDBCommand command, bool async, EDBConnector connector, CancellationToken cancellationToken)
        {
            try
            {
                using (connector.StartUserAction(cancellationToken))
                {
                    var sendTask = command.SendPrepare(connector, async, CancellationToken.None);
                    if (sendTask.IsFaulted)
                        sendTask.GetAwaiter().GetResult();

                    try
                    {
                        // Loop over statements, skipping those that are already prepared (because they were persisted)
                        var isFirst = true;
                        foreach (var batchCommand in command.InternalBatchCommands)
                        {
                            if (!batchCommand.IsPreparing)
                                continue;

                            var pStatement = batchCommand.PreparedStatement!;

                            if (pStatement.StatementBeingReplaced != null)
                            {
                                Expect<CloseCompletedMessage>(await connector.ReadMessage(async), connector);
                                pStatement.StatementBeingReplaced.CompleteUnprepare();
                                pStatement.StatementBeingReplaced = null;
                            }

                            Expect<ParseCompleteMessage>(await connector.ReadMessage(async), connector);
                            Expect<ParameterDescriptionMessage>(await connector.ReadMessage(async), connector);
                            var msg = await connector.ReadMessage(async);
                            switch (msg.Code)
                            {
                            case BackendMessageCode.RowDescription:
                                // Clone the RowDescription for use with the prepared statement (the one we have is reused
                                // by the connection)
                                var description = ((RowDescriptionMessage)msg).Clone();
                                command.FixupRowDescription(description, isFirst);
                                batchCommand.Description = description;
                                break;
                            case BackendMessageCode.NoData:
                                batchCommand.Description = null;
                                break;
                            default:
                                throw connector.UnexpectedMessageReceived(msg.Code);
                            }

                            pStatement.State = PreparedState.Prepared;
                            connector.PreparedStatementManager.NumPrepared++;
                            batchCommand.IsPreparing = false;
                            isFirst = false;
                        }

                        Expect<ReadyForQueryMessage>(await connector.ReadMessage(async), connector);
                    }
                    finally
                    {
                        try
                        {
                            // Make sure sendTask is complete so we don't race against asynchronous flush
                            if (async)
                                await sendTask;
                            else
                                sendTask.GetAwaiter().GetResult();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                LogMessages.CommandPreparedExplicitly(connector.CommandLogger, connector.Id);
            }
            catch
            {
                // The statements weren't prepared successfully, update the bookkeeping for them
                foreach (var batchCommand in command.InternalBatchCommands)
                {
                    if (batchCommand.IsPreparing)
                    {
                        batchCommand.IsPreparing = false;
                        batchCommand.PreparedStatement!.AbortPrepare();
                    }
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Unprepares a command, closing server-side statements associated with it.
    /// Note that this only affects commands explicitly prepared with <see cref="Prepare()"/>, not
    /// automatically prepared statements.
    /// </summary>
    public void Unprepare()
        => Unprepare(false).GetAwaiter().GetResult();

    /// <summary>
    /// Unprepares a command, closing server-side statements associated with it.
    /// Note that this only affects commands explicitly prepared with <see cref="Prepare()"/>, not
    /// automatically prepared statements.
    /// </summary>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    public Task UnprepareAsync(CancellationToken cancellationToken = default)
    {
        using (NoSynchronizationContextScope.Enter())
            return Unprepare(true, cancellationToken);
    }

    async Task Unprepare(bool async, CancellationToken cancellationToken = default)
    {
        var connection = CheckAndGetConnection();
        Debug.Assert(connection is not null);
        if (connection.Settings.Multiplexing)
            throw new NotSupportedException("Explicit preparation not supported with multiplexing");
        if (InternalBatchCommands.All(s => !s.IsPrepared))
            return;

        var connector = connection.Connector!;

        LogMessages.UnpreparingCommand(connector.CommandLogger, connector.Id);

        using (connector.StartUserAction(cancellationToken))
        {
            // Just wait for SendClose to complete since each statement takes no more than 20 bytes
            await SendClose(connector, async, cancellationToken);

            foreach (var batchCommand in InternalBatchCommands)
            {
                if (batchCommand.PreparedStatement?.State == PreparedState.BeingUnprepared)
                {
                    Expect<CloseCompletedMessage>(await connector.ReadMessage(async), connector);

                    var pStatement = batchCommand.PreparedStatement;
                    pStatement.CompleteUnprepare();

                    if (!pStatement.IsExplicit)
                        connector.PreparedStatementManager.AutoPrepared[pStatement.AutoPreparedSlotIndex] = null;

                    batchCommand.PreparedStatement = null;
                }
            }

            Expect<ReadyForQueryMessage>(await connector.ReadMessage(async), connector);
        }
    }

    /* EnterpriseDB Team */
#pragma warning disable IDE0049 // Simplify Names
    private static String ReplaceParameterValue(String result, String parameterName, String paramVal)
#pragma warning restore IDE0049 // Simplify Names
#nullable enable
    {
        var quote_pattern = @"['][^']*[']";
        var parameterMarker = string.Empty;
        // search parameter marker since it is not part of the name
        var pattern = "[- |\n\r\t,)(;=+/<>][:|@]" + parameterMarker + parameterName + "([- |\n\r\t,)(;=+/<>]|$)";
        int start, end;
        var withoutquote = result;
        var found = false;
        // First of all
        // Suppress quoted string from query (because we ave to ignore them)
        MatchCollection results = Regex.Matches(result, quote_pattern);
        //TODO:ZK
        //  foreach (Match match in results)
        //foreach (object o in results)
        // {

        //     Match match = (Match)o;

        //     start = match!.Index;
        //     end = match!.Index + match!.Length;
        //     var spaces = new string(' ', match!.Length - 2);
        //     withoutquote = withoutquote.Substring(0, start + 1) + spaces + withoutquote.Substring(end - 1);
        // }
        do
        {
            // Now we look for the searched parameters on the "withoutquote" string
            results = Regex.Matches(withoutquote, pattern);
            if (results.Count == 0)
            {
                // If no parameter is found, go out!
                break;
            }
            // We take the first parameter found
            found = true;
            Match match = results[0];
            start = match.Index;
            if ((match.Length - parameterName.Length) == 3)
            {
                // If the found string is not the end of the string
                end = match.Index + match.Length - 1;
            }
            else
            {
                // If the found string is the end of the string
                end = match.Index + match.Length;
            }
            result = result.Substring(0, start + 1) + paramVal + result.Substring(end);
            withoutquote = withoutquote.Substring(0, start + 1) + paramVal + withoutquote.Substring(end);
        }
        while (true);
        if (!found)
        {
            // throw new IndexOutOfRangeException(String.Format(resman.GetString("Exception_ParamNotInQuery"), parameterName));
        }
        return result;
    }

    #endregion Prepare

    #region Query analysis

    // EnterpriseDB : redwoodDialect param added for multiplexing scenarii where parser is null and databaseInfo is not available
    internal void ProcessRawQuery(SqlQueryParser? parser, bool standardConformingStrings, EDBBatchCommand? batchCommand, bool redwoodDialect) // EnterpriseDB (additional param)
    {
        var (commandText, commandType, parameters) = batchCommand is null
            ? (CommandText, CommandType, Parameters)
            : (batchCommand.CommandText, batchCommand.CommandType, batchCommand.Parameters);

        if (string.IsNullOrEmpty(commandText))
            throw new InvalidOperationException("CommandText property has not been initialized");

        switch (commandType)
        {
        case CommandType.Text:
            switch (parameters.PlaceholderType)
            {
            case PlaceholderType.Positional:
                // In positional parameter mode, we don't need to parse/rewrite the CommandText or reorder the parameters - just use
                // them as is. If the SQL contains a semicolon (legacy batching) when positional parameters are in use, we just send
                // that and PostgreSQL will error (this behavior is by-design - use the new batching API).
                if (batchCommand is null)
                {
                    batchCommand = TruncateStatementsToOne();
                    batchCommand.FinalCommandText = CommandText;
                    batchCommand.PositionalParameters = Parameters.InternalList;
                }
                else
                {
                    batchCommand.FinalCommandText = batchCommand.CommandText;
                    batchCommand.PositionalParameters = batchCommand.Parameters.InternalList;
                }

                ValidateParameterCount(batchCommand);
                break;

            case PlaceholderType.NoParameters:
                // Unless the EnableSqlRewriting AppContext switch is explicitly disabled, queries with no parameters are parsed just
                // like queries with named parameters, since they may contain a semicolon (legacy batching).
                if (EnableSqlRewriting)
                    goto case PlaceholderType.Named;
                goto case PlaceholderType.Positional;

            case PlaceholderType.Named:
                if (!EnableSqlRewriting)
                    throw new NotSupportedException($"Named parameters are not supported when EnterpriseDB.EDBClient.{nameof(EnableSqlRewriting)} is disabled");

                // The parser is cached on EDBConnector - unless we're in multiplexing mode.
                parser ??= new SqlQueryParser(redwoodDialect);  // EnterpriseDB (additional param)

                if (batchCommand is null)
                {
                    parser.ParseRawQuery(this, standardConformingStrings);
                    if (InternalBatchCommands.Count > 1 && _parameters.HasOutputParameters)
                        throw new NotSupportedException("Commands with multiple queries cannot have out parameters");
                    for (var i = 0; i < InternalBatchCommands.Count; i++)
                        ValidateParameterCount(InternalBatchCommands[i]);
                }
                else
                {
                    parser.ParseRawQuery(batchCommand, standardConformingStrings);
                    if (batchCommand.Parameters.HasOutputParameters)
                        throw new NotSupportedException("Batches cannot cannot have out parameters");
                    ValidateParameterCount(batchCommand);
                }

                break;

            case PlaceholderType.Mixed:
                throw new NotSupportedException("Mixing named and positional parameters isn't supported");

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(PlaceholderType), $"Unknown {nameof(PlaceholderType)} value: {Parameters.PlaceholderType}");
            }

            break;

        case CommandType.TableDirect:
            batchCommand ??= TruncateStatementsToOne();
            batchCommand.FinalCommandText = "SELECT * FROM " + CommandText;
            break;

        case CommandType.StoredProcedure:
			//EnterpriseDB Team (different implementation)
            var inputList = parameters.Where(p => p.IsInputDirection).ToList();
            var numInput = _parameters.Count(p => p.IsInputDirection);//EnterpriseDB Team
            var sb = new StringBuilder();
            string parameterName;
            var parseCommand = CommandText;
            if (_parameters.Count > 0 && !parseCommand.Trim().Contains("(") && !parseCommand.Trim().EndsWith(")", StringComparison.OrdinalIgnoreCase))
            {
                parseCommand += "(";
                for (var i = 0; i < _parameters.Count; i++)
                {
                    parseCommand += ":" + _parameters[i].ParameterName + ", ";
                }
                parseCommand = parseCommand.Substring(0, parseCommand.LastIndexOf(",", StringComparison.OrdinalIgnoreCase));
                parseCommand += ")";
            }
            for (var i = 0; i < _parameters.Count; i++)
            {
                parameterName = _parameters[i].ParameterName;
                parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i + 1));
            }
            if (inputList.Count > 0)
            {
                if (!parseCommand.Trim().EndsWith(")", StringComparison.OrdinalIgnoreCase))
                {
                    parseCommand += "(";
                }
            }
            else
            {
                if (!parseCommand.Trim().EndsWith(")", StringComparison.OrdinalIgnoreCase))
                    parseCommand += "( )";
            }

            parseCommand = "CALL " + parseCommand; // This syntax i s only available in 7.3+ as well SupportsPrepare.
            sb.Append(parseCommand);

            batchCommand ??= TruncateStatementsToOne();
            batchCommand.FinalCommandText = sb.ToString();
            batchCommand.PositionalParameters.AddRange(inputList);
            break;

        default:
            throw new InvalidOperationException($"Internal EDB bug: unexpected value {CommandType} of enum {nameof(CommandType)}. Please file a bug.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ValidateParameterCount(EDBBatchCommand batchCommand)
        {
            if (batchCommand.PositionalParameters.Count > ushort.MaxValue)
                throw new EDBException($"A statement cannot have more than {ushort.MaxValue} parameters");
        }
    }

    #endregion

    #region Message Creation / Population

    void BeginSend(EDBConnector connector)
        => connector.WriteBuffer.Timeout = TimeSpan.FromSeconds(CommandTimeout);

    internal Task Write(EDBConnector connector, bool async, bool flush, CancellationToken cancellationToken = default)
    {
        return (_behavior & CommandBehavior.SchemaOnly) == 0
            ? WriteExecute(connector, async, flush, cancellationToken)
            : WriteExecuteSchemaOnly(connector, async, flush, cancellationToken);

        async Task WriteExecute(EDBConnector connector, bool async, bool flush, CancellationToken cancellationToken)
        {
            EDBBatchCommand? batchCommand = null;

            for (var i = 0; i < InternalBatchCommands.Count; i++)
            {
                // The following is only for deadlock avoidance when doing sync I/O (so never in multiplexing)
                ForceAsyncIfNecessary(ref async, i);

                batchCommand = InternalBatchCommands[i];
                var pStatement = batchCommand.PreparedStatement;

                Debug.Assert(batchCommand.FinalCommandText is not null);

                if (pStatement == null || batchCommand.IsPreparing)
                {
                    // The statement should either execute unprepared, or is being auto-prepared.
                    // Send Parse, Bind, Describe

                    // We may have a prepared statement that replaces an existing statement - close the latter first.
                    if (pStatement?.StatementBeingReplaced != null)
                        await connector.WriteClose(StatementOrPortal.Statement, pStatement.StatementBeingReplaced.Name!, async, cancellationToken);

                    await connector.WriteParse(batchCommand.FinalCommandText, batchCommand.StatementName, batchCommand.PositionalParameters, async, cancellationToken);

					//EnterpriseDB Team 
                    if (CommandType == CommandType.StoredProcedure)
                    {
                        await connector.WriteBindOut(
                           batchCommand.PositionalParameters, _parameters, string.Empty, batchCommand.StatementName, AllResultTypesAreUnknown,
                           i == 0 ? UnknownResultTypeList : null,
                           async);

                    }
                    else
                    {
                        await connector.WriteBind(
                        batchCommand.PositionalParameters, string.Empty, batchCommand.StatementName, AllResultTypesAreUnknown,
                        i == 0 ? UnknownResultTypeList : null,
                        async, cancellationToken);
                    }

                    await connector.WriteDescribe(StatementOrPortal.Portal, string.Empty, async, cancellationToken);
                }
                else
                {
                    // The statement is already prepared, only a Bind is needed
                    if (CommandType == CommandType.StoredProcedure)  //EnterpriseDB Team 
                    { 
                        await connector.WriteBindOut(
                           batchCommand.PositionalParameters, _parameters, string.Empty, batchCommand.StatementName, AllResultTypesAreUnknown,
                           i == 0 ? UnknownResultTypeList : null,
                           async);

                    }
                    else
                    {
                        // The statement is already prepared, only a Bind is needed
                        await connector.WriteBind(
                        batchCommand.PositionalParameters, string.Empty, batchCommand.StatementName, AllResultTypesAreUnknown,
                        i == 0 ? UnknownResultTypeList : null,
                        async, cancellationToken);
                    }
                }

				//EnterpriseDB Team 
                if (CommandType == CommandType.StoredProcedure) 
                {
                    await connector.WriteDescribe(StatementOrPortal.Portal, string.Empty, async, cancellationToken);
                    await connector.WriteDescribeOut(StatementOrPortal.Portal, string.Empty, async);

                }

                await connector.WriteExecute(0, async, cancellationToken);

				//EnterpriseDB Team
                if (CommandType == CommandType.StoredProcedure)
                {
                    await connector.WriteExecuteOut(0, async);
                }
                if (batchCommand.AppendErrorBarrier ?? EnableErrorBarriers)
                    await connector.WriteSync(async, cancellationToken);

                if (pStatement != null)
                    connector.PreparedStatementManager.SetLastUsed(pStatement, DateTime.UtcNow); //EnterpriseDB Team
            }

            if (batchCommand is null || !(batchCommand.AppendErrorBarrier ?? EnableErrorBarriers))
            {
                await connector.WriteSync(async, cancellationToken);
            }

            if (flush)
                await connector.Flush(async, cancellationToken);
        }

        async Task WriteExecuteSchemaOnly(EDBConnector connector, bool async, bool flush, CancellationToken cancellationToken)
        {
            var wroteSomething = false;
            for (var i = 0; i < InternalBatchCommands.Count; i++)
            {
                ForceAsyncIfNecessary(ref async, i);

                var batchCommand = InternalBatchCommands[i];

                if (batchCommand.PreparedStatement?.State == PreparedState.Prepared)
                    continue;   // Prepared, we already have the RowDescription

                await connector.WriteParse(batchCommand.FinalCommandText!, batchCommand.StatementName, batchCommand.PositionalParameters, async, cancellationToken);
                await connector.WriteDescribe(StatementOrPortal.Statement, batchCommand.StatementName, async, cancellationToken);
                wroteSomething = true;
            }

            if (wroteSomething)
            {
                await connector.WriteSync(async, cancellationToken);
                if (flush)
                    await connector.Flush(async, cancellationToken);
            }
        }
    }

    async Task SendDeriveParameters(EDBConnector connector, bool async, CancellationToken cancellationToken = default)
    {
        BeginSend(connector);

        for (var i = 0; i < InternalBatchCommands.Count; i++)
        {
            ForceAsyncIfNecessary(ref async, i);

            var batchCommand = InternalBatchCommands[i];

            await connector.WriteParse(batchCommand.FinalCommandText!, string.Empty, EmptyParameters, async, cancellationToken);
            await connector.WriteDescribe(StatementOrPortal.Statement, string.Empty, async, cancellationToken);
        }

        await connector.WriteSync(async, cancellationToken);
        await connector.Flush(async, cancellationToken);
    }

    async Task SendPrepare(EDBConnector connector, bool async, CancellationToken cancellationToken = default)
    {
        BeginSend(connector);

        for (var i = 0; i < InternalBatchCommands.Count; i++)
        {
            ForceAsyncIfNecessary(ref async, i);

            var batchCommand = InternalBatchCommands[i];
            var pStatement = batchCommand.PreparedStatement;

            // A statement may be already prepared, already in preparation (i.e. same statement twice
            // in the same command), or we can't prepare (overloaded SQL)
            if (!batchCommand.IsPreparing)
                continue;

            // We may have a prepared statement that replaces an existing statement - close the latter first.
            var statementToClose = pStatement!.StatementBeingReplaced;
            if (statementToClose != null)
                await connector.WriteClose(StatementOrPortal.Statement, statementToClose.Name!, async, cancellationToken);
            if (CommandType == CommandType.StoredProcedure) //EnterpriseDB Team
            {
                await connector.WriteParseOut(batchCommand.FinalCommandText!, pStatement.Name!, _parameters, async, connector.TypeMapper);
            }
            else
            {
                await connector.WriteParse(batchCommand.FinalCommandText!, pStatement.Name!, batchCommand.PositionalParameters, async, cancellationToken);
            }
            await connector.WriteDescribe(StatementOrPortal.Statement, pStatement.Name!, async, cancellationToken);
        }

        await connector.WriteSync(async, cancellationToken);
        await connector.Flush(async, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ForceAsyncIfNecessary(ref bool async, int numberOfStatementInBatch)
    {
        if (!async && numberOfStatementInBatch > 0)
        {
            // We're synchronously sending the non-first statement in a batch - switch to async writing.
            // See long comment in Execute() above.

            // TODO: we can simply do all batch writing asynchronously, instead of starting with the 2nd statement.
            // For now, writing the first statement synchronously gives us a better chance of handle and bubbling up errors correctly
            // (see sendTask.IsFaulted in Execute()). Once #1323 is done, that shouldn't be needed any more and entire batches should
            // be written asynchronously.
            async = true;
            SynchronizationContext.SetSynchronizationContext(SingleThreadSynchronizationContext);
        }
    }

    async Task SendClose(EDBConnector connector, bool async, CancellationToken cancellationToken = default)
    {
        BeginSend(connector);

        foreach (var batchCommand in InternalBatchCommands.Where(s => s.IsPrepared))
        {
            // No need to force async here since each statement takes no more than 20 bytes

            await connector.WriteClose(StatementOrPortal.Statement, batchCommand.StatementName, async, cancellationToken);
            batchCommand.PreparedStatement!.State = PreparedState.BeingUnprepared;
        }

        await connector.WriteSync(async, cancellationToken);
        await connector.Flush(async, cancellationToken);
    }

    #endregion

    #region Execute Non Query

    /// <summary>
    /// Executes a SQL statement against the connection and returns the number of rows affected.
    /// </summary>
    /// <returns>The number of rows affected if known; -1 otherwise.</returns>
    public override int ExecuteNonQuery() => ExecuteNonQuery(false, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronous version of <see cref="ExecuteNonQuery()"/>
    /// </summary>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        using (NoSynchronizationContextScope.Enter())
            return ExecuteNonQuery(true, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<int> ExecuteNonQuery(bool async, CancellationToken cancellationToken)
    {
        if (CommandType == CommandType.StoredProcedure) //EnterpriseDB Team
        {
            Connection!.Connector!._isScaler = true; //ZKK
        }
        var reader = await ExecuteReader(CommandBehavior.Default, async, cancellationToken);
        try
        {
            if (CommandType != CommandType.StoredProcedure) //EnterpriseDB Team
                while (async ? await reader.NextResultAsync(cancellationToken) : reader.NextResult()) ;

            return reader.RecordsAffected;
        }
        finally
        {
            if (async)
                await reader.DisposeAsync();
            else
                reader.Dispose();
        }
    }

    #endregion Execute Non Query

    #region Execute Scalar

    /// <summary>
    /// Executes the query, and returns the first column of the first row
    /// in the result set returned by the query. Extra columns or rows are ignored.
    /// </summary>
    /// <returns>The first column of the first row in the result set,
    /// or a null reference if the result set is empty.</returns>
    public override object? ExecuteScalar() => ExecuteScalar(false, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronous version of <see cref="ExecuteScalar()"/>
    /// </summary>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task representing the asynchronous operation, with the first column of the
    /// first row in the result set, or a null reference if the result set is empty.</returns>
    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        using (NoSynchronizationContextScope.Enter())
            return ExecuteScalar(true, cancellationToken).AsTask();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async ValueTask<object?> ExecuteScalar(bool async, CancellationToken cancellationToken)
    {
        var behavior = CommandBehavior.SingleRow;
        if (IsWrappedByBatch || !Parameters.HasOutputParameters)
            behavior |= CommandBehavior.SequentialAccess;

        var reader = await ExecuteReader(behavior, async, cancellationToken);
        try
        {
            var read = async ? await reader.ReadAsync(cancellationToken) : reader.Read();
            var value = read && reader.FieldCount != 0 ? reader.GetValue(0) : null;
            // We read the whole result set to trigger any errors
            while (async ? await reader.NextResultAsync(cancellationToken) : reader.NextResult()) ;
            return value;
        }
        finally
        {
            if (async)
                await reader.DisposeAsync();
            else
                reader.Dispose();
        }
    }

    #endregion Execute Scalar

    #region Execute Reader

    /// <summary>
    /// Executes the command text against the connection.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        => ExecuteReader(behavior);

    /// <summary>
    /// Executes the command text against the connection.
    /// </summary>
    /// <param name="behavior">An instance of <see cref="CommandBehavior"/>.</param>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        => await ExecuteReaderAsync(behavior, cancellationToken);

    /// <summary>
    /// Executes the <see cref="CommandText"/> against the <see cref="Connection"/>
    /// and returns a <see cref="EDBDataReader"/>.
    /// </summary>
    /// <param name="behavior">One of the enumeration values that specifies the command behavior.</param>
    /// <returns>A task representing the operation.</returns>
    public new EDBDataReader ExecuteReader(CommandBehavior behavior = CommandBehavior.Default)
        => ExecuteReader(behavior, async: false, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// An asynchronous version of <see cref="ExecuteReader(CommandBehavior)"/>, which executes
    /// the <see cref="CommandText"/> against the <see cref="Connection"/>
    /// and returns a <see cref="EDBDataReader"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public new Task<EDBDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
        => ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);

    /// <summary>
    /// An asynchronous version of <see cref="ExecuteReader(CommandBehavior)"/>,
    /// which executes the <see cref="CommandText"/> against the <see cref="Connection"/>
    /// and returns a <see cref="EDBDataReader"/>.
    /// </summary>
    /// <param name="behavior">One of the enumeration values that specifies the command behavior.</param>
    /// <param name="cancellationToken">
    /// An optional token to cancel the asynchronous operation. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public new Task<EDBDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken = default)
    {
        using (NoSynchronizationContextScope.Enter())
            return ExecuteReader(behavior, async: true, cancellationToken).AsTask();
    }

    // TODO: Maybe pool these?
    internal ManualResetValueTaskSource<EDBConnector> ExecutionCompletion { get; }
        = new();

    internal virtual async ValueTask<EDBDataReader> ExecuteReader(CommandBehavior behavior, bool async, CancellationToken cancellationToken)
    {
        var conn = CheckAndGetConnection();
        _behavior = behavior;

        EDBConnector? connector;
        if (_connector is not null)
        {
            Debug.Assert(conn is null);
            if (behavior.HasFlag(CommandBehavior.CloseConnection))
                throw new ArgumentException($"{nameof(CommandBehavior.CloseConnection)} is not supported with {nameof(EDBConnector)}", nameof(behavior));
            connector = _connector;
        }
        else
        {
            Debug.Assert(conn is not null);

            conn.TryGetBoundConnector(out connector);
        }

        try
        {
            if (connector is not null)
            {
                var dataSource = connector.DataSource;
                var logger = connector.CommandLogger;

                cancellationToken.ThrowIfCancellationRequested();
                // We cannot pass a token here, as we'll cancel a non-send query
                // Also, we don't pass the cancellation token to StartUserAction, since that would make it scope to the entire action (command execution)
                // whereas it should only be scoped to the Execute method.
                connector.StartUserAction(ConnectorState.Executing, this, CancellationToken.None);

                Task? sendTask;

                var validateParameterValues = !behavior.HasFlag(CommandBehavior.SchemaOnly);

                try
                {
                    switch (IsExplicitlyPrepared)
                    {
                    case true:
                        Debug.Assert(_connectorPreparedOn != null);
                        if (IsWrappedByBatch)
                        {
                            foreach (var batchCommand in InternalBatchCommands)
                            {
                                if (batchCommand.ConnectorPreparedOn != connector)
                                {
                                    foreach (var s in InternalBatchCommands)
                                        s.ResetPreparation();
                                    ResetPreparation();
                                    goto case false;
                                }
                                batchCommand.Parameters.ProcessParameters(dataSource.TypeMapper, validateParameterValues, CommandType);
                            }
                        }
                        else
                        {
                            if (_connectorPreparedOn != connector)
                            {
                                // The command was prepared, but since then the connector has changed. Detach all prepared statements.
                                foreach (var s in InternalBatchCommands)
                                    s.PreparedStatement = null;
                                ResetPreparation();
                                goto case false;
                            }
                            Parameters.ProcessParameters(dataSource.TypeMapper, validateParameterValues, CommandType);
                        }

                        EDBEventSource.Log.CommandStartPrepared();
                        break;

                    case false:
                        var numPrepared = 0;

                        if (IsWrappedByBatch)
                        {
                            for (var i = 0; i < InternalBatchCommands.Count; i++)
                            {
                                var batchCommand = InternalBatchCommands[i];

                                batchCommand.Parameters.ProcessParameters(dataSource.TypeMapper, validateParameterValues, CommandType);
                                ProcessRawQuery(connector.SqlQueryParser, connector.UseConformingStrings, batchCommand, dataSource.DatabaseInfo.SupportsRedwoodDialect);   // EnterpriseDB (additional param)

                                if (connector.Settings.MaxAutoPrepare > 0 && batchCommand.TryAutoPrepare(connector))
                                {
                                    batchCommand.ConnectorPreparedOn = connector;
                                    numPrepared++;
                                }
                            }
                        }
                        else
                        {
                            Parameters.ProcessParameters(dataSource.TypeMapper, validateParameterValues, CommandType);
                            ProcessRawQuery(connector.SqlQueryParser, connector.UseConformingStrings, batchCommand: null, dataSource.DatabaseInfo.SupportsRedwoodDialect);  // EnterpriseDB (additional param)

                            if (connector.Settings.MaxAutoPrepare > 0)
                                for (var i = 0; i < InternalBatchCommands.Count; i++)
                                    if (InternalBatchCommands[i].TryAutoPrepare(connector))
                                        numPrepared++;
                        }

                        if (numPrepared > 0)
                        {
                            _connectorPreparedOn = connector;
                            if (numPrepared == InternalBatchCommands.Count)
                                EDBEventSource.Log.CommandStartPrepared();
                        }

                        break;
                    }

                    State = CommandState.InProgress;

                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        connector.QueryLogStopWatch.Restart();

                        if (logger.IsEnabled(LogLevel.Debug))
                            LogExecutingCompleted(connector, executing: true);
                    }

                    EDBEventSource.Log.CommandStart(CommandText);
                    TraceCommandStart(connector);

                    // If a cancellation is in progress, wait for it to "complete" before proceeding (#615)
                    connector.ResetCancellation();

                    // We do not wait for the entire send to complete before proceeding to reading -
                    // the sending continues in parallel with the user's reading. Waiting for the
                    // entire send to complete would trigger a deadlock for multi-statement commands,
                    // where PostgreSQL sends large results for the first statement, while we're sending large
                    // parameter data for the second. See #641.
                    // Instead, all sends for non-first statements are performed asynchronously (even if the user requested sync),
                    // in a special synchronization context to prevents a dependency on the thread pool (which would also trigger
                    // deadlocks).
                    BeginSend(connector);
                    sendTask = Write(connector, async, flush: true, CancellationToken.None);

                    // The following is a hack. It raises an exception if one was thrown in the first phases
                    // of the send (i.e. in parts of the send that executed synchronously). Exceptions may
                    // still happen later and aren't properly handled. See #1323.
                    if (sendTask.IsFaulted)
                        sendTask.GetAwaiter().GetResult();
                }
                catch
                {
                    connector.EndUserAction();
                    throw;
                }

                // TODO: DRY the following with multiplexing, but be careful with the cancellation registration...
                var reader = connector.DataReader;
                reader.Init(this, behavior, InternalBatchCommands, sendTask);
                connector.CurrentReader = reader;
                if (async)
                    await reader.NextResultAsync(cancellationToken);
                else
                    reader.NextResult();

                TraceReceivedFirstResponse();

                return reader;
            }
            else
            {
                Debug.Assert(conn is not null);

                Debug.Assert(conn.Settings.Multiplexing);
                // The connection isn't bound to a connector - it's multiplexing time.
                var dataSource = (MultiplexingDataSource)conn.EDBDataSource;

                // EnterpriseDB Team
                var isRedwood = dataSource.DatabaseInfo.SupportsRedwoodDialect;

                if (!async)
                {
                    // The waiting on the ExecutionCompletion ManualResetValueTaskSource is necessarily
                    // asynchronous, so allowing sync would mean sync-over-async.
                    throw new NotSupportedException(
                        "Synchronous command execution is not supported when multiplexing is on");
                }

                if (IsWrappedByBatch)
                {
                    foreach (var batchCommand in InternalBatchCommands)
                    {
                        batchCommand.Parameters.ProcessParameters(dataSource.TypeMapper, validateValues: true, CommandType);
                        ProcessRawQuery(null, standardConformingStrings: true, batchCommand, isRedwood);  // EnterpriseDB (additional param)
                    }
                }
                else
                {
                    Parameters.ProcessParameters(dataSource.TypeMapper, validateValues: true, CommandType);
                    ProcessRawQuery(null, standardConformingStrings: true, batchCommand: null, isRedwood);  // EnterpriseDB (additional param)
                }

                State = CommandState.InProgress;

                // TODO: Experiment: do we want to wait on *writing* here, or on *reading*?
                // Previous behavior was to wait on reading, which throw the exception from ExecuteReader (and not from
                // the first read). But waiting on writing would allow us to do sync writing and async reading.
                ExecutionCompletion.Reset();
                await dataSource.MultiplexCommandWriter.WriteAsync(this, cancellationToken);
                connector = await new ValueTask<EDBConnector>(ExecutionCompletion, ExecutionCompletion.Version);
                // TODO: Overload of StartBindingScope?
                conn.Connector = connector;
                connector.Connection = conn;
                conn.ConnectorBindingScope = ConnectorBindingScope.Reader;

                var reader = connector.DataReader;
                reader.Init(this, behavior, InternalBatchCommands);
                connector.CurrentReader = reader;
                await reader.NextResultAsync(cancellationToken);

                return reader;
            }
        }
        catch (Exception e)
        {
            var reader = connector?.CurrentReader;
            if (e is not EDBOperationInProgressException && reader is not null)
                await reader.Cleanup(async);

            TraceSetException(e);

            State = CommandState.Idle;

            // Reader disposal contains logic for closing the connection if CommandBehavior.CloseConnection is
            // specified. However, close here as well in case of an error before the reader was even instantiated
            // (e.g. write I/O error)
            if ((behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
            {
                Debug.Assert(_connector is null && conn is not null);
                conn.Close();
            }
            throw;
        }
    }

    #endregion

    #region Transactions

    /// <summary>
    /// DB transaction.
    /// </summary>
    protected override DbTransaction? DbTransaction
    {
        get => _transaction;
        set => _transaction = (EDBTransaction?)value;
    }

    /// <summary>
    /// This property is ignored by EnterpriseDB.EDBClient. PostgreSQL only supports a single transaction at a given time on
    /// a given connection, and all commands implicitly run inside the current transaction started via
    /// <see cref="EDBConnection.BeginTransaction()"/>
    /// </summary>
    public new EDBTransaction? Transaction
    {
        get => (EDBTransaction?)DbTransaction;
        set => DbTransaction = value;
    }

    #endregion Transactions

    #region Cancel

    /// <summary>
    /// Attempts to cancel the execution of an <see cref="EDBCommand" />.
    /// </summary>
    /// <remarks>As per the specs, no exception will be thrown by this method in case of failure.</remarks>
    public override void Cancel()
    {
        if (State != CommandState.InProgress)
            return;

        var connector = Connection?.Connector ?? _connector;
        if (connector is null)
            return;

        connector.PerformUserCancellation();
    }

    #endregion Cancel

    #region Dispose

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _transaction = null;

        State = CommandState.Disposed;

        if (IsCached && InternalConnection is not null && InternalConnection.CachedCommand is null)
        {
            // TODO: Optimize EDBParameterCollection to recycle EDBParameter instances as well
            // TODO: Statements isn't cleared/recycled, leaving this for now, since it'll be replaced by the new batching API

            _commandText = string.Empty;
            CommandType = CommandType.Text;
            _parameters.Clear();
            InternalConnection.CachedCommand = this;
            return;
        }

        IsCached = false;
    }

    #endregion

    #region Tracing

    #endregion Tracing

    internal void TraceCommandStart(EDBConnector connector)
    {
        Debug.Assert(CurrentActivity is null);
        if (EDBActivitySource.IsEnabled)
            CurrentActivity = EDBActivitySource.CommandStart(connector, CommandText, CommandType);
    }

    internal void TraceReceivedFirstResponse()
    {
        if (CurrentActivity is not null)
        {
            EDBActivitySource.ReceivedFirstResponse(CurrentActivity);
        }
    }

    internal void TraceCommandStop()
    {
        if (CurrentActivity is not null)
        {
            EDBActivitySource.CommandStop(CurrentActivity);
            CurrentActivity = null;
        }
    }

    internal void TraceSetException(Exception e)
    {
        if (CurrentActivity is not null)
        {
            EDBActivitySource.SetException(CurrentActivity, e);
            CurrentActivity = null;
        }
    }

    #region Misc

    EDBBatchCommand TruncateStatementsToOne()
    {
        switch (InternalBatchCommands.Count)
        {
        case 0:
            var statement = new EDBBatchCommand();
            InternalBatchCommands.Add(statement);
            return statement;

        case 1:
            statement = InternalBatchCommands[0];
            statement.Reset();
            return statement;

        default:
            statement = InternalBatchCommands[0];
            statement.Reset();
            InternalBatchCommands.Clear();
            InternalBatchCommands.Add(statement);
            return statement;
        }
    }

    /// <summary>
    /// Fixes up the text/binary flag on result columns.
    /// Since Prepare() describes a statement rather than a portal, the resulting RowDescription
    /// will have text format on all result columns. Fix that up.
    /// </summary>
    /// <remarks>
    /// Note that UnknownResultTypeList only applies to the first query, while AllResultTypesAreUnknown applies
    /// to all of them.
    /// </remarks>
    internal void FixupRowDescription(RowDescriptionMessage rowDescription, bool isFirst)
    {
        for (var i = 0; i < rowDescription.Count; i++)
        {
            var field = rowDescription[i];
            field.FormatCode = (UnknownResultTypeList == null || !isFirst ? AllResultTypesAreUnknown : UnknownResultTypeList[i])
                ? FormatCode.Text
                : FormatCode.Binary;
            field.ResolveHandler();
        }
    }

    internal void LogExecutingCompleted(EDBConnector connector, bool executing)
    {
        var logParameters = connector.LoggingConfiguration.IsParameterLoggingEnabled || connector.Settings.LogParameters;
        var logger = connector.LoggingConfiguration.CommandLogger;

        if (InternalBatchCommands.Count == 1)
        {
            var singleCommand = InternalBatchCommands[0];

            if (logParameters && singleCommand.PositionalParameters.Count > 0)
            {
                if (executing)
                {
                    LogMessages.ExecutingCommandWithParameters(
                        logger,
                        singleCommand.FinalCommandText!,
                        singleCommand.PositionalParameters.Select(p => p.Value == DBNull.Value ? "NULL" : p.Value!).ToArray(),
                        connector.Id);
                }
                else
                {
                    LogMessages.CommandExecutionCompletedWithParameters(
                        logger,
                        singleCommand.FinalCommandText!,
                        singleCommand.PositionalParameters.Select(p => p.Value == DBNull.Value ? "NULL" : p.Value!).ToArray(),
                        connector.QueryLogStopWatch.ElapsedMilliseconds,
                        connector.Id);
                }
            }
            else
            {
                if (executing)
                    LogMessages.ExecutingCommand(logger, singleCommand.FinalCommandText!, connector.Id);
                else
                    LogMessages.CommandExecutionCompleted(logger, singleCommand.FinalCommandText!, connector.QueryLogStopWatch.ElapsedMilliseconds, connector.Id);
            }
        }
        else
        {
            if (logParameters)
            {
                var commands = InternalBatchCommands
                    .Select(c => (
                        c.CommandText,
                        Parameters: (object[]?)c.PositionalParameters.Select(p => p.Value == DBNull.Value ? "NULL" : p.Value).ToArray()!)
                    ).ToArray();

                if (executing)
                    LogMessages.ExecutingBatchWithParameters(logger, commands, connector.Id);
                else
                    LogMessages.BatchExecutionCompletedWithParameters(logger, commands, connector.QueryLogStopWatch.ElapsedMilliseconds, connector.Id);
            }
            else
            {
                var commands = InternalBatchCommands.Select(c => c.CommandText).ToArray().ToArray();

                if (executing)
                    LogMessages.ExecutingBatch(logger, commands, connector.Id);
                else
                    LogMessages.BatchExecutionCompleted(logger, commands, connector.QueryLogStopWatch.ElapsedMilliseconds, connector.Id);
            }
        }
    }

    /// <summary>
    /// Create a new command based on this one.
    /// </summary>
    /// <returns>A new EDBCommand object.</returns>
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Create a new command based on this one.
    /// </summary>
    /// <returns>A new EDBCommand object.</returns>
    public virtual EDBCommand Clone()
    {
        var clone = new EDBCommand(CommandText, InternalConnection, Transaction)
        {
            CommandTimeout = CommandTimeout,
            CommandType = CommandType,
            DesignTimeVisible = DesignTimeVisible,
            _allResultTypesAreUnknown = _allResultTypesAreUnknown,
            _unknownResultTypeList = _unknownResultTypeList,
            ObjectResultTypes = ObjectResultTypes
        };
        _parameters.CloneTo(clone._parameters);
        return clone;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    EDBConnection? CheckAndGetConnection()
    {
        if (State == CommandState.Disposed)
            throw new ObjectDisposedException(GetType().FullName);
        if (InternalConnection == null)
        {
            if (_connector is null)
                throw new InvalidOperationException("Connection property has not been initialized.");
            return null;
        }
        switch (InternalConnection.FullState)
        {
        case ConnectionState.Open:
        case ConnectionState.Connecting:
        case ConnectionState.Open | ConnectionState.Executing:
        case ConnectionState.Open | ConnectionState.Fetching:
            return InternalConnection;
        default:
            throw new InvalidOperationException("Connection is not open");
        }
    }

#if NETFRAMEWORK || NETSTANDARD2_0 // EnterpriseDB
    // EnterpriseDB Team
    /// <summary>
    /// Async wrapper for .Net Framework IAsyncDisposable() support
    /// </summary>
    /// <returns></returns>
    public ValueTask DisposeAsync()
    {
        //await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);

        return default(ValueTask);
    }
#endif

    /// <summary>
    /// This event is unsupported by EnterpriseDB.EDBClient. Use <see cref="System.Data.Common.DbConnection.StateChange"/> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler? Disposed
    {
        add => throw new NotSupportedException("The Disposed event isn't supported by EnterpriseDB.EDBClient. Use DbConnection.StateChange instead.");
        remove => throw new NotSupportedException("The Disposed event isn't supported by EnterpriseDB.EDBClient. Use DbConnection.StateChange instead.");
    }

    event EventHandler? IComponent.Disposed
    {
        add => Disposed += value;
        remove => Disposed -= value;
    }

    #endregion
}

enum CommandState
{
    Idle,
    InProgress,
    Disposed
}
