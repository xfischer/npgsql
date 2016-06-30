#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Sockets;
using AsyncRewriter;
using  EnterpriseDB.EDBClient.BackendMessages;
using  EnterpriseDB.EDBClient.FrontendMessages;
using  EnterpriseDB.EDBClient.Logging;
using System.Text.RegularExpressions;


namespace  EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
#if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(EDBCommand)), ToolboxItem(true)]
#endif
#if DNXCORE50
    public sealed partial class EDBCommand : DbCommand
#else
    [System.ComponentModel.DesignerCategory("")]
    public sealed partial class EDBCommand : DbCommand, ICloneable
#endif
    {
        #region Fields

        EDBConnection _connection;
        /// <summary>
        /// Cached version of _connection.Connector, for performance
        /// </summary>
        EDBConnector _connector;
        EDBTransaction _transaction;
        String _commandText;
        int? _timeout;
        readonly EDBParameterCollection _parameters = new EDBParameterCollection();

       public  List<EDBStatement> _queries;

        int _queryIndex;

        UpdateRowSource _updateRowSource = UpdateRowSource.Both;

        /// <summary>
        /// Indicates whether this command has been prepared.
        /// Never access this field directly, use <see cref="IsPrepared"/> instead.
        /// </summary>
        bool _isPrepared;

        /// <summary>
        /// For prepared commands, captures the connection's <see cref="EDBConnection.OpenCounter"/>
        /// at the time the command was prepared. This allows us to know whether the connection was
        /// closed since the command was prepared.
        /// </summary>
        int _prepareConnectionOpenId;

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constants

        internal const int DefaultTimeout = 30;

        /// <summary>
        /// Specifies the maximum number of queries we allow in a multiquery, separated by semicolons.
        /// We limit this because of deadlocks: as we send Parse and Bind messages to the backend, the backend
        /// replies with ParseComplete and BindComplete messages which we do not read until we finished sending
        /// all messages. Once our buffer gets full the backend will get stuck writing, and then so will we.
        /// </summary>
        internal const int MaxQueriesInMultiquery = 5000;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBCommand">EDBCommand</see> class.
        /// </summary>
        public EDBCommand() : this(String.Empty, null, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBCommand">EDBCommand</see> class with the text of the query.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        public EDBCommand(String cmdText) : this(cmdText, null, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBCommand">EDBCommand</see> class with the text of the query and a <see cref="EDBConnection">EDBConnection</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="EDBConnection">EDBConnection</see> that represents the connection to a PostgreSQL server.</param>
        public EDBCommand(String cmdText, EDBConnection connection) : this(cmdText, connection, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EDBCommand">EDBCommand</see> class with the text of the query, a <see cref="EDBConnection">EDBConnection</see>, and the <see cref="EDBTransaction">EDBTransaction</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="EDBConnection">EDBConnection</see> that represents the connection to a PostgreSQL server.</param>
        /// <param name="transaction">The <see cref="EDBTransaction">EDBTransaction</see> in which the <see cref="EDBCommand">EDBCommand</see> executes.</param>
        public EDBCommand(string cmdText, EDBConnection connection, EDBTransaction transaction)
        {
            Init(cmdText);
            Connection = connection;
            Transaction = transaction;
        }

        void Init(string cmdText)
        {
            _commandText = cmdText;
            CommandType = CommandType.Text;
            _queries = new List<EDBStatement>();
        }

        #endregion Constructors

        #region Public properties

        /// <summary>
        /// Gets or sets the SQL statement or function (stored procedure) to execute at the data source.
        /// </summary>
        /// <value>The Transact-SQL statement or stored procedure to execute. The default is an empty string.</value>
        [DefaultValue("")]
#if !DNXCORE50
        [Category("Data")]
#endif
        public override String CommandText
        {
            get { return _commandText; }
            set
            {
                // [TODO] Validate commandtext.
                _commandText = value;
                DeallocatePrepared();
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt  to execute a command and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</value>
        [DefaultValue(DefaultTimeout)]
        public override int CommandTimeout
        {
            get
            {
                return _timeout ?? (
                    _connection != null
                      ? _connection.CommandTimeout
                      : DefaultTimeout
                );
            }
            set
            {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value", value, "CommandTimeout can't be less than zero.");
                }

                _timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the
        /// <see cref="EDBCommand.CommandText">CommandText</see> property is to be interpreted.
        /// </summary>
        /// <value>One of the <see cref="System.Data.CommandType">CommandType</see> values. The default is <see cref="System.Data.CommandType">CommandType.Text</see>.</value>
        [DefaultValue(CommandType.Text)]
#if !DNXCORE50
        [Category("Data")]
#endif
        public override CommandType CommandType { get; set; }

        /// <summary>
        /// DB connection.
        /// </summary>
        protected override DbConnection DbConnection
        {
            get { return Connection; }
            set { Connection = (EDBConnection)value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="EDBConnection">EDBConnection</see>
        /// used by this instance of the <see cref="EDBCommand">EDBCommand</see>.
        /// </summary>
        /// <value>The connection to a data source. The default value is a null reference.</value>
        [DefaultValue(null)]
#if !DNXCORE50
        [Category("Behavior")]
#endif
        public new EDBConnection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection == value)
                {
                    return;
                }

                //if (this._transaction != null && this._transaction.Connection == null)
                //  this._transaction = null;

                // All this checking needs revising. It should be simpler.
                // This this.Connector != null check was added to remove the nullreferenceexception in case
                // of the previous connection has been closed which makes Connector null and so the last check would fail.
                // See bug 1000581 for more details.
                if (_transaction != null && _connection != null && _connection.Connector != null && _connection.Connector.InTransaction)
                {
                    throw new InvalidOperationException("The Connection property can't be changed with an uncommited transaction.");
                }

                IsPrepared = false;
                _connection = value;
                Transaction = null;
            }
        }

        /// <summary>
        /// Design time visible.
        /// </summary>
        public override bool DesignTimeVisible { get; set; }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="System.Data.DataRow">DataRow</see>
        /// when used by the <see cref="System.Data.Common.DbDataAdapter.Update(DataSet)">Update</see>
        /// method of the <see cref="System.Data.Common.DbDataAdapter">DbDataAdapter</see>.
        /// </summary>
        /// <value>One of the <see cref="System.Data.UpdateRowSource">UpdateRowSource</see> values.</value>
#if WITHDESIGN
        [Category("Behavior"), DefaultValue(UpdateRowSource.Both)]
#endif
        public override UpdateRowSource UpdatedRowSource
        {
            get { return _updateRowSource; }
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
        public bool IsPrepared
        {
            get
            {
                if (_isPrepared)
                {
                // ZK  Contract.Assert(Connection != null);
                    if (Connection.State != ConnectionState.Open || _prepareConnectionOpenId != Connection.OpenCounter) {
                        _isPrepared = false;
                    }
                }
                return _isPrepared;
            }

            private set
            {
                Contract.Requires(!value || Connection != null);
                _isPrepared = value;
                if (value) {
                    _prepareConnectionOpenId = Connection.OpenCounter;
                }
            }
        }

        #endregion Public properties

        #region Known/unknown Result Types Management

        /// <summary>
        /// Marks all of the query's result columns as either known or unknown.
        /// Unknown results column are requested them from PostgreSQL in text format, and  EnterpriseDB.EDBClient makes no
        /// attempt to parse them. They will be accessible as strings only.
        /// </summary>
        public bool AllResultTypesAreUnknown
        {
            get { return _allResultTypesAreUnknown; }
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
        /// Unknown results column are requested them from PostgreSQL in text format, and  EnterpriseDB.EDBClient makes no
        /// attempt to parse them. They will be accessible as strings only.
        /// </summary>
        /// <remarks>
        /// If the query includes several queries (e.g. SELECT 1; SELECT 2), this will only apply to the first
        /// one. The rest of the queries will be fetched and parsed as usual.
        ///
        /// The array size must correspond exactly to the number of result columns the query returns, or an
        /// error will be raised.
        /// </remarks>
        public bool[] UnknownResultTypeList
        {
            get { return _unknownResultTypeList; }
            set
            {
                // TODO: Check that this isn't modified after calling prepare
                _allResultTypesAreUnknown = false;
                _unknownResultTypeList = value;
            }
        }

        bool[] _unknownResultTypeList;

        #endregion

        #region Result Types Management

        /// <summary>
        /// Marks result types to be used when using GetValue on a data reader, on a column-by-column basis.
        /// Used for Entity Framework 5-6 compability.
        /// Only primitive numerical types and DateTimeOffset are supported.
        /// Set the whole array or just a value to null to use default type.
        /// </summary>
        public Type[] ObjectResultTypes { get; set; }

        #endregion

        #region State management

        int _state;

        /// <summary>
        /// Gets the current state of the connector
        /// </summary>
        internal CommandState State
        {
            get { return (CommandState)_state; }
            set
            {
                var newState = (int)value;
                if (newState == _state)
                    return;
                Interlocked.Exchange(ref _state, newState);
            }
        }

        #endregion State management

        #region Parameters

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.Common.DbParameter">DbParameter</see> object.
        /// </summary>
        /// <returns>An <see cref="System.Data.Common.DbParameter">DbParameter</see> object.</returns>
        protected override DbParameter CreateDbParameter()
        {
            return CreateParameter();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="EDBParameter">EDBParameter</see> object.
        /// </summary>
        /// <returns>A <see cref="EDBParameter">EDBParameter</see> object.</returns>
        public new EDBParameter CreateParameter()
        {
            return new EDBParameter();
        }

        /// <summary>
        /// DB parameter collection.
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        /// <summary>
        /// Gets the <see cref="EDBParameterCollection">EDBParameterCollection</see>.
        /// </summary>
        /// <value>The parameters of the SQL statement or function (stored procedure). The default is an empty collection.</value>
#if WITHDESIGN
        [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
#endif

        public new EDBParameterCollection Parameters { get { return _parameters; } }

        #endregion

        #region Prepare

        /// <summary>
        /// Creates a prepared version of the command on a PostgreSQL server.
        /// </summary>
        public override void Prepare()
        {
            Prechecks();
            if (Parameters.Any(p => !p.IsTypeExplicitlySet)) {
                throw new InvalidOperationException("EDBCommand.Prepare method requires all parameters to have an explicitly set type.");
            }

            _connector = Connection.Connector;
            Log.Debug("Prepare command", _connector.Id);

            using (_connector.StartUserAction())
            {
                DeallocatePrepared();
                ProcessRawQuery();
                for (var j = 0; j < _queries.Count; j++)
                {
                    switch (CommandType)
                    {


                        case CommandType.StoredProcedure:
                            int i = 0;
                            var query = _queries[i];
                            ParseOutMessage parseOutMessage;
                            DescribeMessage describeMessage;

                            describeMessage = _connector.DescribeMessage;
                            parseOutMessage = _connector.ParseOutMessage;
                            query.PreparedStatementName = _connector.NextPreparedStatementName();
                            _connector.AddMessage(parseOutMessage.Populate(query, _parameters, _connector.TypeHandlerRegistry));
                            _connector.AddMessage(describeMessage.Populate(StatementOrPortal.Statement,
                               query.PreparedStatementName));
                            break;
                    }
                }
                if (CommandType != System.Data.CommandType.StoredProcedure)
                {
                    for (var i = 0; i < _queries.Count; i++)
                    {
                        var query = _queries[i];
                        ParseMessage parseMessage;
                        DescribeMessage describeMessage;
                        if (i == 0)
                        {
                            parseMessage = _connector.ParseMessage;
                            describeMessage = _connector.DescribeMessage;
                        }
                        else
                        {
                            parseMessage = new ParseMessage();
                            describeMessage = new DescribeMessage();
                        }

                        query.PreparedStatementName = _connector.NextPreparedStatementName();
                        _connector.AddMessage(parseMessage.Populate(query, _connector.TypeHandlerRegistry));
                        _connector.AddMessage(describeMessage.Populate(StatementOrPortal.Statement,
                            query.PreparedStatementName));
                    }
                }
                _connector.AddMessage(SyncMessage.Instance);
                _connector.SendAllMessages();

                _queryIndex = 0;

                while (true)
                {
                    var msg = _connector.ReadSingleMessage();
                    switch (msg.Code)
                    {
                    case BackendMessageCode.CompletedResponse: // prepended messages, e.g. begin transaction
                    case BackendMessageCode.ParseComplete:
                    case BackendMessageCode.ParameterDescription:
                        continue;
                    case BackendMessageCode.RowDescription:
                        var description = (RowDescriptionMessage) msg;
                        FixupRowDescription(description, _queryIndex == 0);
                        _queries[_queryIndex++].Description = description;
                        continue;
                    case BackendMessageCode.NoData:
                        _queries[_queryIndex++].Description = null;
                        continue;
                    case BackendMessageCode.ReadyForQuery:
                       // Contract.Assume(_queryIndex == _queries.Count);
                        IsPrepared = true;
                        return;
                    default:
                        throw _connector.UnexpectedMessageReceived(msg.Code);
                    }
                }
            }
        }

        void DeallocatePrepared()
        {
            if (!IsPrepared) { return; }

            foreach (var query in _queries) {
                _connector.PrependInternalMessage(new CloseMessage(StatementOrPortal.Statement, query.PreparedStatementName));
            }
            _connector.PrependInternalMessage(SyncMessage.Instance);
            IsPrepared = false;
        }

        #endregion Prepare

        #region Query analysis

        void ProcessRawQuery()
        {
            _queries.Clear();
            switch (CommandType) {
            case CommandType.Text:
                
               


                SqlQueryParser.ParseRawQuery(CommandText, _connection == null || _connection.UseConformantStrings, _parameters, _queries);
                if (_queries.Count > 1 && _parameters.Any(p => p.IsOutputDirection)) {
                    throw new NotSupportedException("Commands with multiple queries cannot have out parameters");
                }
                break;
            case CommandType.TableDirect:
                _queries.Add(new EDBStatement("SELECT * FROM " + CommandText, new List<EDBParameter>()));
                break;
            case CommandType.StoredProcedure:
                var numInput = _parameters.Count(p => p.IsInputDirection);
                var sb = new StringBuilder();
                string parameterName;
                string parseCommand = CommandText ;
                   // parseCommand.Substring()


                //EDB
           
                for(var i = 0 ; i < _parameters.Count; i++){
                parameterName = _parameters[i].ParameterName;
                parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i + 1));
                }
                if (_parameters.Count > 0)
                {
                    if (!parseCommand.Trim().EndsWith(")"))
                    {
                        // addProcedureParenthesis = true;
                        parseCommand += "(";
                    }
                }else
                    parseCommand += "( )";
                //parseCommand = string.Format("select * from {0}", parseCommand); // This syntax is only available in 7.3+ as well SupportsPrepare.
                /*
                 * EDBTeam
                 */
                parseCommand = "CALL " + parseCommand; // This syntax i s only available in 7.3+ as well SupportsPrepare.
                

              //  sb.Append("SELECT * FROM "); ZK EDB CheckMe
              
                sb.Append(parseCommand);
                
                //sb.Append('(');

                //sb.Append("CALL emp_query3( ");
                //for (var i = 1; i <= _parameters.Count; i++)
                //{
                //    if (_parameters[i-1].Direction == ParameterDirection.ReturnValue)
                //        continue;
                //    sb.Append('$');

                //    sb.Append(i);
                //    if (i <= _parameters.Count -1)
                //    {
                //        if (_parameters[i].Direction == ParameterDirection.ReturnValue)
                //            continue;
                //        sb.Append(',');
                //    }
                //}
                //sb.Append(')');
                _queries.Add(new EDBStatement(sb.ToString(), _parameters.Where(p => p.IsInputDirection).ToList()));
                break;
            default:
                throw PGUtil.ThrowIfReached();
            }
        }

        #endregion

        #region Frontend message creation


        /* EnterpriseDB Team */
        private static String ReplaceParameterValue(String result, String parameterName, String paramVal)
        {
            String quote_pattern = @"['][^']*[']";
            string parameterMarker = string.Empty;
            // search parameter marker since it is not part of the name
            String pattern = "[- |\n\r\t,)(;=+/<>][:|@]" + parameterMarker + parameterName + "([- |\n\r\t,)(;=+/<>]|$)";
            Int32 start, end;
            String withoutquote = result;
            Boolean found = false;
            // First of all
            // Suppress quoted string from query (because we ave to ignore them)
            MatchCollection results = Regex.Matches(result, quote_pattern);
            foreach (Match match in results)
            {
                start = match.Index;
                end = match.Index + match.Length;
                String spaces = new String(' ', match.Length - 2);
                withoutquote = withoutquote.Substring(0, start + 1) + spaces + withoutquote.Substring(end - 1);
            }
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
        internal void ValidateAndCreateMessages(CommandBehavior behavior = CommandBehavior.Default)
        {
            _connector = Connection.Connector;
          foreach (EDBParameter p in Parameters) { //
              if (p.EDBDbType == EDBTypes.EDBDbType.Varchar && p.Direction == ParameterDirection.Output)
                  continue;
                p.Bind(_connector.TypeHandlerRegistry);
                if (p.LengthCache != null) {
                    p.LengthCache.Clear();
                }
                p.ValidateAndGetLength();
            }

            // For prepared SchemaOnly queries, we already have the RowDescriptions from the Prepare phase.
            // No need to send anything
            if (IsPrepared && (behavior & CommandBehavior.SchemaOnly) != 0) {
                return;
            }

            // Set the frontend timeout
            _connector.UserCommandFrontendTimeout = CommandTimeout;
            // If needed, prepend a "SET statement_timeout" message to set the backend timeout
            _connector.PrependBackendTimeoutMessage(CommandTimeout);

            // Create actual messages depending on scenario
            if (IsPrepared) {
                CreateMessagesPrepared(behavior);
            } else {
                if ((behavior & CommandBehavior.SchemaOnly) == 0) {
                    CreateMessagesNonPrepared(behavior);
                } else {
                    CreateMessagesSchemaOnly(behavior);
                }
            }
        }

        void CreateMessagesNonPrepared(CommandBehavior behavior)
        {
            Contract.Requires((behavior & CommandBehavior.SchemaOnly) == 0);

            ProcessRawQuery();

            var portalNames = _queries.Count > 1
                ? Enumerable.Range(0, _queries.Count).Select(i => "MQ" + i).ToArray()
                : null;

            for (var i = 0; i < _queries.Count; i++)
            {
                var query = _queries[i];

                ParseMessage parseMessage;
                DescribeMessage describeMessage;
                BindMessage bindMessage;
                if (i == 0)
                {
                    parseMessage = _connector.ParseMessage;
                    describeMessage = _connector.DescribeMessage;
                    bindMessage = _connector.BindMessage;
                }
                else
                {
                    parseMessage = new ParseMessage();
                    describeMessage = new DescribeMessage();
                    bindMessage = new BindMessage();
                }

                _connector.AddMessage(parseMessage.Populate(query, _connector.TypeHandlerRegistry));
                _connector.AddMessage(describeMessage.Populate(StatementOrPortal.Statement));

                bindMessage.Populate(
                    _connector.TypeHandlerRegistry,
                    query.InputParameters,
                    _queries.Count == 1 ? "" : portalNames[i]
                );
                if (AllResultTypesAreUnknown) {
                    bindMessage.AllResultTypesAreUnknown = AllResultTypesAreUnknown;
                } else if (i == 0 && UnknownResultTypeList != null) {
                    bindMessage.UnknownResultTypeList = UnknownResultTypeList;
                }
                _connector.AddMessage(bindMessage);
            }

            if (_queries.Count == 1) {
                _connector.AddMessage(_connector.ExecuteMessage.Populate("", (behavior & CommandBehavior.SingleRow) != 0 ? 1 : 0));
            } else
                for (var i = 0; i < _queries.Count; i++) {
                    // TODO: Verify SingleRow behavior for multiqueries
                    _connector.AddMessage(new ExecuteMessage(portalNames[i], (behavior & CommandBehavior.SingleRow) != 0 ? 1 : 0));
                    _connector.AddMessage(new CloseMessage(StatementOrPortal.Portal, portalNames[i]));
                }
            _connector.AddMessage(SyncMessage.Instance);
        }

        void CreateMessagesPrepared(CommandBehavior behavior)
        {
            for (var i = 0; i < _queries.Count; i++)
            {
                BindMessage bindMessage;
                BindOutMessage bindOutMessage;
                DescribeMessage describeMessage;
                DescribeOutMessage describeOutMessage;
                ExecuteMessage executeMessage;
                ExecuteOutMessage executeOutMessage;
                   //m_Connector.DescribeOut(statementDescribeOut); //ZK
                   //     m_Connector.Execute(execute);
                   //     m_Connector.ExecuteOut(executeOut)
                if (i == 0)
                {

                    bindOutMessage = _connector.BindOutMessage;
                    bindMessage = _connector.BindMessage;
                    describeMessage = _connector.DescribeMessage;
                    describeOutMessage = _connector.DescribeOutMessage;
                    executeMessage = _connector.ExecuteMessage;
                   executeOutMessage = _connector.ExecuteOutMessage;
//                    DescribeMessage 


                }
                else
                {
                    bindOutMessage = new BindOutMessage();
                    bindMessage = new BindMessage();
                    describeMessage = new DescribeMessage();
                    describeOutMessage = new DescribeOutMessage();
                    executeMessage = new ExecuteMessage();
                    executeOutMessage = new ExecuteOutMessage();
                }

                var query = _queries[i];

                //TODO ZK Checkme, Inputparam are being passed only, should pass all count .e.g. 6

                if (CommandType == System.Data.CommandType.StoredProcedure)
                {

                    bindOutMessage.Populate(_connector.TypeHandlerRegistry, query.InputParameters, _parameters, "", query.PreparedStatementName);
                    describeMessage.Populate(StatementOrPortal.Portal);
                    describeOutMessage.Populate(StatementOrPortal.Portal);
                    executeMessage.Populate("", (behavior & CommandBehavior.SingleRow) != 0 ? 1 : 0);
                    executeOutMessage.Populate("", (behavior & CommandBehavior.SingleRow) != 0 ? 1 : 0);
          
                }
                else
                    bindMessage.Populate(_connector.TypeHandlerRegistry, query.InputParameters, "", query.PreparedStatementName);
                if (AllResultTypesAreUnknown)   
                {
                    bindMessage.AllResultTypesAreUnknown = AllResultTypesAreUnknown;
                }
                else if (i == 0 && UnknownResultTypeList != null)
                {
                    bindMessage.UnknownResultTypeList = UnknownResultTypeList;
                }
                if (CommandType == System.Data.CommandType.StoredProcedure)
                { //ZK
                    _connector.AddMessage(bindOutMessage);
                    _connector.AddMessage(describeMessage);
                    _connector.AddMessage(describeOutMessage);
                    _connector.AddMessage(executeMessage);
                    _connector.AddMessage(executeOutMessage);
                    _connector.AddMessage(SyncMessage.Instance);
                    

                }
                else
                {
                    _connector.AddMessage(bindMessage);
                    _connector.AddMessage(executeMessage.Populate("", (behavior & CommandBehavior.SingleRow) != 0 ? 1 : 0));
                }
            }
            _connector.AddMessage(SyncMessage.Instance);
        }

        void CreateMessagesSchemaOnly(CommandBehavior behavior)
        {
            Contract.Requires((behavior & CommandBehavior.SchemaOnly) != 0);

            ProcessRawQuery();

            for (var i = 0; i < _queries.Count; i++)
            {
                ParseMessage parseMessage;
                DescribeMessage describeMessage;
                if (i == 0) {
                    parseMessage = _connector.ParseMessage;
                    describeMessage = _connector.DescribeMessage;
                } else {
                    parseMessage = new ParseMessage();
                    describeMessage = new DescribeMessage();
                }

                _connector.AddMessage(parseMessage.Populate(_queries[i], _connector.TypeHandlerRegistry));
                _connector.AddMessage(describeMessage.Populate(StatementOrPortal.Statement));
            }

            _connector.AddMessage(SyncMessage.Instance);
        }

        #endregion

        #region Execute

        [RewriteAsync]
        internal EDBDataReader Execute(CommandBehavior behavior = CommandBehavior.Default)
        {
            State = CommandState.InProgress;
            try
            {
                _queryIndex = 0;
                _connector.SendAllMessages();

                if (!IsPrepared)
                {
                    IBackendMessage msg;
                    do
                    {
                        msg = _connector.ReadSingleMessage();
                    } while (!ProcessMessageForUnprepared(msg, behavior));
                }  

                var reader = new EDBDataReader(this, behavior, _queries);
                reader.Init();
            /*    
                if (_parameters != null)
                    if (_parameters.Count  != 0)
                    {
                        while (reader.Read()) ;
                        reader.Read();
                        reader.Close();

                        for (int i = 0; i < _parameters.Count; i++)
                        {


                            Console.WriteLine(_parameters[0].Value.ToString());

                          

                            string p = "fetch all in \"" + _parameters[i].Value + "\"";
                            EDBCommand command1 = new EDBCommand(p.ToString(), _connector.Connection);
                            EDBDataReader rd2 = command1.ExecuteReader(CommandBehavior.SingleResult);
                            int fieldcont = rd2.FieldCount;
                            Console.WriteLine(rd2.IsCaching.ToString());
                            
                            _parameters[i].Value = (EDBDataReader)rd2;
                          //  while (rd2.Read()) ;
                            //rd2.Close();
                            //EDBCommand cmd = new EDBCommand("select 1",_connector.Connection);
                            //EDBDataReader reader11 = cmd.ExecuteReader(CommandBehavior.SingleResult);
                            //reader11.Init();
                        }
                   
                }*/
                //   EDBDataReader = reader = new EDBDataReader("select 1 from dual", CommandBehavior.SingleResult, _queries[1]);
                _connector.CurrentReader = reader;
                return reader;


            }
            catch
            {
                State = CommandState.Idle;
                throw;
            }
        }

        bool ProcessMessageForUnprepared(IBackendMessage msg, CommandBehavior behavior)
        {
            Contract.Requires(!IsPrepared);

            switch (msg.Code) {
            case BackendMessageCode.CompletedResponse:  // e.g. begin transaction
            case BackendMessageCode.ParseComplete:
            case BackendMessageCode.ParameterDescription:
            case BackendMessageCode.CloseComplete: //ZK TODO CHeck me
                return false;
            case BackendMessageCode.RowDescription:
          // ZK     Contract.Assert(_queryIndex < _queries.Count);
                var description = (RowDescriptionMessage)msg;
                FixupRowDescription(description, _queryIndex == 0);
                _queries[_queryIndex].Description = description;
                if ((behavior & CommandBehavior.SchemaOnly) != 0) {
                    _queryIndex++;
                }
                return false;
            case BackendMessageCode.NoData:
        //ZK        Contract.Assert(_queryIndex < _queries.Count);
                _queries[_queryIndex].Description = null;
                return false;
            case BackendMessageCode.BindComplete:
                Contract.Assume((behavior & CommandBehavior.SchemaOnly) == 0);
                return ++_queryIndex == _queries.Count;
            case BackendMessageCode.ReadyForQuery:
           //ZK TODO CHECKME     Contract.Assume((behavior & CommandBehavior.SchemaOnly) != 0);
                return true;  // End of a SchemaOnly command
            default:
                throw _connector.UnexpectedMessageReceived(msg.Code);
            }
        }

        #endregion

        #region Execute Non Query

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected if known; -1 otherwise.</returns>
        public override int ExecuteNonQuery()
        {
            return ExecuteNonQueryInternal();
        }

        /// <summary>
        /// Asynchronous version of <see cref="ExecuteNonQuery"/>
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.Register(Cancel);
            try
            {
                return await ExecuteNonQueryInternalAsync().ConfigureAwait(false);
            }
            catch (EDBException e)
            {
                if (e.Code == "57014")
                    throw new TaskCanceledException(e.Message);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [RewriteAsync]
        int ExecuteNonQueryInternal()
        {
            Prechecks();
            Log.Debug("ExecuteNonQuery", Connection.Connector.Id);
            using (Connection.Connector.StartUserAction())
            {
                ValidateAndCreateMessages();
                EDBDataReader reader;
                using (reader = Execute())
                {
                    if(!_isPrepared)
                    while (reader.NextResult()) ;
                }
                return reader.RecordsAffected;
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
        public override object ExecuteScalar()
        {
            return ExecuteScalarInternal();
        }

        /// <summary>
        /// Asynchronous version of <see cref="ExecuteScalar"/>
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the first column of the
        /// first row in the result set, or a null reference if the result set is empty.</returns>
        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.Register(Cancel);
            try
            {
                return await ExecuteScalarInternalAsync().ConfigureAwait(false);
            }
            catch (EDBException e)
            {
                if (e.Code == "57014")
                    throw new TaskCanceledException(e.Message);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [RewriteAsync]
        object ExecuteScalarInternal()
        {
            Prechecks();
            Log.Debug("ExecuteNonScalar", Connection.Connector.Id);
            using (Connection.Connector.StartUserAction())
            {
                var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleRow;
                ValidateAndCreateMessages(behavior);
                using (var reader = Execute(behavior))
                {
                    return reader.Read() && reader.FieldCount != 0 ? reader.GetValue(0) : null;
                }
            }
        }

        #endregion Execute Scalar

        #region Execute Reader

        /// <summary>
        /// Executes the CommandText against the Connection, and returns an DbDataReader.
        /// </summary>
        /// <remarks>
        /// Unlike the ADO.NET method which it replaces, this method returns a  EnterpriseDB.EDBClient-specific
        /// DataReader.
        /// </remarks>
        /// <returns>A DbDataReader object.</returns>
        public new EDBDataReader ExecuteReader()
        {
            return (EDBDataReader)base.ExecuteReader();
        }

        /// <summary>
        /// Executes the CommandText against the Connection, and returns an DbDataReader using one
        /// of the CommandBehavior values.
        /// </summary>
        /// <remarks>
        /// Unlike the ADO.NET method which it replaces, this method returns a  EnterpriseDB.EDBClient-specific
        /// DataReader.
        /// </remarks>
        /// <returns>A DbDataReader object.</returns>
        public new EDBDataReader ExecuteReader(CommandBehavior behavior)
        {
            return (EDBDataReader)base.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <param name="behavior">An instance of <see cref="CommandBehavior"/>.</param>
        /// <param name="cancellationToken">A task representing the operation.</param>
        /// <returns></returns>
        protected async override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.Register(Cancel);
            try
            {
                return await ExecuteDbDataReaderInternalAsync(behavior).ConfigureAwait(false);
            }
            catch (EDBException e)
            {
                if (e.Code == "57014")
                    throw new TaskCanceledException(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            Log.Debug("ExecuteReader with CommandBehavior=" + behavior);
            return ExecuteDbDataReaderInternal(behavior);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [RewriteAsync]
        EDBDataReader ExecuteDbDataReaderInternal(CommandBehavior behavior)
        {
            Prechecks();

            Log.Debug("ExecuteReader", Connection.Connector.Id);

            Connection.Connector.StartUserAction();
            try
            {
                 ValidateAndCreateMessages(behavior);
                return Execute(behavior);
            }
            catch
            {
                if (Connection.Connector != null) {
                    Connection.Connector.EndUserAction();
                }

                // Close connection if requested even when there is an error.
                if ((behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)   
                {
                    _connection.Close();
                }

                throw;
            }
        }

        #endregion

        #region Transactions

        /// <summary>
        /// DB transaction.
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get { return Transaction; }
            set { Transaction = (EDBTransaction)value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="EDBTransaction">EDBTransaction</see>
        /// within which the <see cref="EDBCommand">EDBCommand</see> executes.
        /// </summary>
        /// <value>The <see cref="EDBTransaction">EDBTransaction</see>.
        /// The default value is a null reference.</value>
#if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif

        public new EDBTransaction Transaction
        {
            get
            {
                if (_transaction != null && _transaction.Connection == null)
                {
                    _transaction = null;
                }
                return _transaction;
            }
            set
            {
                _transaction = value;
            }
        }

        #endregion Transactions

        #region Cancel

        /// <summary>
        /// Attempts to cancel the execution of a <see cref="EDBCommand">EDBCommand</see>.
        /// </summary>
        /// <remarks>As per the specs, no exception will be thrown by this method in case of failure</remarks>
        public override void Cancel()
        {
            if (State == CommandState.Disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (Connection == null)
                throw new InvalidOperationException("Connection property has not been initialized.");

            var connector = Connection.Connector;
            if (State != CommandState.InProgress) {
                Log.Debug(String.Format("Skipping cancel because command is in state {0}", State), connector.Id);
                return;
            }

            Log.Debug("Cancelling command", connector.Id);
            try
            {
                connector.CancelRequest();
            }
            catch (Exception e)
            {
                var socketException = e.InnerException as SocketException;
                if (socketException == null || socketException.SocketErrorCode != SocketError.ConnectionReset)
                {
                    Log.Warn("Exception caught while attempting to cancel command", e, connector.Id);
                }
            }
        }

        #endregion Cancel

        #region Dispose

        /// <summary>
        /// Releases the resources used by the <see cref="EDBCommand">EDBCommand</see>.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (State == CommandState.Disposed)
                return;

            if (disposing)
            {
                // Note: we only actually perform cleanup here if called from Dispose() (disposing=true), and not
                // if called from a finalizer (disposing=false). This is because we cannot perform any SQL
                // operations from the finalizer (connection may be in use by someone else).
                // We can implement a queue-based solution that will perform cleanup during the next possible
                // window, but this isn't trivial (should not occur in transactions because of possible exceptions,
                // etc.).

                if (IsPrepared)
                {
                    DeallocatePrepared();
                }
            }
            Transaction = null;
            Connection = null;
            State = CommandState.Disposed;
            base.Dispose(disposing);
        }

        #endregion

        #region Misc

        /// <summary>
        /// Fixes up the text/binary flag on result columns.
        /// Since we send the Describe command right after the Parse and before the Bind, the resulting RowDescription
        /// will have text format on all result columns. Fix that up.
        /// </summary>
        /// <remarks>
        /// Note that UnknownResultTypeList only applies to the first query, while AllResultTypesAreUnknown applies
        /// to all of them.
        /// </remarks>
        void FixupRowDescription(RowDescriptionMessage rowDescription, bool isFirst)
        {
            for (var i = 0; i < rowDescription.NumFields; i++)
            {
                var field = rowDescription[i];
                field.FormatCode =
                    (UnknownResultTypeList == null || !isFirst ? AllResultTypesAreUnknown : UnknownResultTypeList[i])
                    ? FormatCode.Text
                    : FormatCode.Binary;
                if (field.FormatCode == FormatCode.Text)
                {
                    field.Handler = Connection.Connector.TypeHandlerRegistry.UnrecognizedTypeHandler;
                }
            }
        }

        void LogCommand()
        {
            if (!Log.IsEnabled(EDBLogLevel.Debug))
            {
                return;
            }

            var sb = new StringBuilder();
            sb.Append("Executing statement(s):");
            foreach (var s in _queries)
            {
                sb
                    .AppendLine()
                    .Append("\t")
                    .Append(s.SQL);
            }

            if (EDBLogManager.IsParameterLoggingEnabled && Parameters.Any())
            {
                sb
                    .AppendLine()
                    .AppendLine("Parameters:");
                for (var i = 0; i < Parameters.Count; i++)
                {
                    sb
                        .Append("\t$")
                        .Append(i + 1)
                        .Append(": ")
                        .Append(Convert.ToString(Parameters[i].Value, CultureInfo.InvariantCulture));
                }
            }

            Log.Debug(sb.ToString(), Connection.Connector.Id);
        }

#if !DNXCORE50
        /// <summary>
        /// Create a new command based on this one.
        /// </summary>
        /// <returns>A new EDBCommand object.</returns>
        Object ICloneable.Clone()
        {
            return Clone();
        }
#endif

        /// <summary>
        /// Create a new command based on this one.
        /// </summary>
        /// <returns>A new EDBCommand object.</returns>
        public EDBCommand Clone()
        {
            // TODO: Add consistency checks.

            var clone = new EDBCommand(CommandText, Connection, Transaction)
            {
                CommandTimeout = CommandTimeout,
                CommandType = CommandType,
                DesignTimeVisible = DesignTimeVisible,
                _allResultTypesAreUnknown = _allResultTypesAreUnknown,
                _unknownResultTypeList = _unknownResultTypeList,
                ObjectResultTypes = ObjectResultTypes
            };
            foreach (EDBParameter parameter in Parameters)
            {
                clone.Parameters.Add(parameter.Clone());
            }
            return clone;
        }

        void Prechecks()
        {
            if (State == CommandState.Disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (Connection == null)
                throw new InvalidOperationException("Connection property has not been initialized.");
            Connection.CheckReady();
        }

        #endregion

        #region Invariants

        [ContractInvariantMethod]
        void ObjectInvariants()
        {
            Contract.Invariant(!(AllResultTypesAreUnknown && UnknownResultTypeList != null));
            Contract.Invariant(Connection != null || !IsPrepared);
        }

        #endregion
    }

    enum CommandState
    {
        Idle,
        InProgress,
        Disposed
    }
}
