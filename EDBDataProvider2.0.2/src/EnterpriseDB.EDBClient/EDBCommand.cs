// created on 21/5/2002 at 20:03

// Npgsql.NpgsqlCommand.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using EDBTypes;
using System.Collections.Generic;

#if WITHDESIGN

#endif

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
#if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(NpgsqlCommand)), ToolboxItem(true)]
#endif

    public sealed partial class EDBCommand : DbCommand, ICloneable
    {
        private enum PrepareStatus
        {
            NotPrepared,
            NeedsPrepare,
            V2Prepared,
            V3Prepared
        }

        // Logging related values
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Regex parameterReplace = new Regex(@"([:@][\w\.]*)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex POSTGRES_TEXT_ARRAY = new Regex(@"^array\[+'", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private EDBConnection connection;
        private EDBConnector m_Connector; //renamed to account for hiding it in a local function
        //if all locals were named with this prefix, it would solve LOTS of issues.
        private EDBTransaction transaction;
        private String commandText;
        private Int32 timeout;
        private CommandType commandType;
        private readonly EDBParameterCollection parameters = new EDBParameterCollection();
        private String planName;
        private Boolean designTimeVisible;

        private Boolean functionNeedsColumnListDefinition = false; // Functions don't return record by default.


		private PrepareStatus prepared = PrepareStatus.NotPrepared;
        private EDBParse parse;
        private EDBBind bind;
        private EDBExecute execute = null;
        private EDBRowDescription currentRowDescription = null;

        private Int64 lastInsertedOID = 0;


        
        // locals about function support so we don`t need to check it everytime a function is called.
        
        private Boolean functionChecksDone = false;
        
        private Boolean addProcedureParenthesis = false; // Do not add procedure parenthesis by default.

        private Boolean functionReturnsRecord = false; // Functions don't return record by default.

        private Boolean functionReturnsRefcursor = false; // Functions don't return refcursor by default.
  //TODO ZK Delme      private Boolean functionNeedsColumnListDefinition = false; // Functions don't return record by default.

        private Boolean commandTimeoutSet = false;

        private UpdateRowSource updateRowSource = UpdateRowSource.Both;

        private static readonly Array ParamNameCharTable;

        // Constructors
        static EDBCommand()
        {
            ParamNameCharTable = BuildParameterNameCharacterTable();
        }

        private static Array BuildParameterNameCharacterTable()
        {
            Array paramNameCharTable;

            // Table has lower bound of (int)'.';
            paramNameCharTable = Array.CreateInstance(typeof(byte), new int[] {'z' - '.' + 1}, new int[] {'.'});

            paramNameCharTable.SetValue((byte)'.', (int)'.');

            for (int i = '0' ; i <= '9' ; i++)
            {
                paramNameCharTable.SetValue((byte)i, i);
            }

            for (int i = 'A' ; i <= 'Z' ; i++)
            {
                paramNameCharTable.SetValue((byte)i, i);
            }

            paramNameCharTable.SetValue((byte)'_', (int)'_');

            for (int i = 'a' ; i <= 'z' ; i++)
            {
                paramNameCharTable.SetValue((byte)i, i);
            }

            return paramNameCharTable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class.
        /// </summary>
        public EDBCommand()
            : this(String.Empty, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class with the text of the query.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        public EDBCommand(String cmdText)
            : this(cmdText, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class with the text of the query and a <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> that represents the connection to a PostgreSQL server.</param>
        public EDBCommand(String cmdText, EDBConnection connection)
            : this(cmdText, connection, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class with the text of the query, a <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>, and the <see cref="Npgsql.EDBTransaction">EDBTransaction</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> that represents the connection to a PostgreSQL server.</param>
        /// <param name="transaction">The <see cref="Npgsql.EDBTransaction">EDBTransaction</see> in which the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> executes.</param>
        public EDBCommand(String cmdText, EDBConnection connection, EDBTransaction transaction)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            planName = String.Empty;
            commandText = cmdText;
            this.connection = connection;

            if (this.connection != null)
            {
                this.m_Connector = connection.Connector;

                if (m_Connector != null && m_Connector.AlwaysPrepare)
                {
                    CommandTimeout = m_Connector.CommandTimeout;
                    prepared = PrepareStatus.NeedsPrepare;
                }
            }

            commandType = CommandType.Text;
            this.Transaction = transaction;

            SetCommandTimeout();
        }
              /// <summary>
        /// Used to execute internal commands.
        /// </summary>
        internal EDBCommand(String cmdText, EDBConnector connector, int CommandTimeout = 20)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            planName = String.Empty;
            commandText = cmdText;
            this.m_Connector = connector;
            this.CommandTimeout = CommandTimeout;
            commandType = CommandType.Text;

            // Removed this setting. It was causing too much problem.
            // Do internal commands really need different timeout setting?
            // Internal commands aren't affected by command timeout value provided by user.
            // timeout = 20;
        }
        /// <summary>
        /// Used to execute internal commands.
        /// </summary>
        internal EDBCommand(String cmdText, EDBConnector connector)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);


            planName = String.Empty;
            commandText = cmdText;
            this.m_Connector = connector;
            this.CommandTimeout = CommandTimeout;
            commandType = CommandType.Text;

            // Removed this setting. It was causing too much problem.
            // Do internal commands really need different timeout setting?
            // Internal commands aren't affected by command timeout value provided by user.
            // timeout = 20;
        }

        // Public properties.
        /// <summary>
        /// Gets or sets the SQL statement or function (stored procedure) to execute at the data source.
        /// </summary>
        /// <value>The Transact-SQL statement or stored procedure to execute. The default is an empty string.</value>
        [Category("Data"), DefaultValue("")]
        public override String CommandText
        {
            get { return commandText; }

            set
            {
                // [TODO] Validate commandtext.
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandText", value);
                commandText = value;

                UnPrepare();

                functionChecksDone = false;
            }
        }

//TODO ZK

        private void UnPrepare()
        {
            if (prepared == PrepareStatus.V3Prepared)
            {
                bind = null;
                execute = null;
                currentRowDescription = null;
                prepared = PrepareStatus.NeedsPrepare;
            }
            else if (prepared == PrepareStatus.V2Prepared)
            {
                planName = String.Empty;
                prepared = PrepareStatus.NeedsPrepare;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt
        /// to execute a command and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for the command to execute.
        /// The default is 20 seconds.</value>
        [DefaultValue(20)]
        public override Int32 CommandTimeout
        {
            get { return timeout; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", resman.GetString("Exception_CommandTimeoutLessZero"));
                }

                timeout = value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandTimeout", value);
                commandTimeoutSet = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the
        /// <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> property is to be interpreted.
        /// </summary>
        /// <value>One of the <see cref="System.Data.CommandType">CommandType</see> values. The default is <see cref="System.Data.CommandType">CommandType.Text</see>.</value>
        [Category("Data"), DefaultValue(CommandType.Text)]
        public override CommandType CommandType
        {
            get { return commandType; }

            set
            {
                commandType = value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandType", value);
            }
        }

        protected override DbConnection DbConnection
        {
            get { return Connection; }

            set
            {
                Connection = (EDBConnection)value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "DbConnection", value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>
        /// used by this instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        /// <value>The connection to a data source. The default value is a null reference.</value>
        [Category("Behavior"), DefaultValue(null)]
        public new EDBConnection Connection
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Connection");
                return connection;
            }

            set
            {
                if (this.Connection == value)
                {
                    return;
                }

                //if (this.transaction != null && this.transaction.Connection == null)
                //  this.transaction = null;

                // All this checking needs revising. It should be simpler.
                // This this.Connector != null check was added to remove the nullreferenceexception in case
                // of the previous connection has been closed which makes Connector null and so the last check would fail.
                // See bug 1000581 for more details.
                if (this.transaction != null && this.connection != null && this.Connector != null && this.Connector.Transaction != null)
                {
                    throw new InvalidOperationException(resman.GetString("Exception_SetConnectionInTransaction"));
                }

                this.connection = value;
                Transaction = null;
                if (this.connection != null)
                {
                    m_Connector = this.connection.Connector;

                    if (m_Connector != null && m_Connector.AlwaysPrepare)
                    {
                        prepared = PrepareStatus.NeedsPrepare;
                    }
                }

                SetCommandTimeout();

                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Connection", value);
            }
        }

        internal EDBConnector Connector
        {
            get
            {
                if (this.connection != null)
                {
                    m_Connector = this.connection.Connector;
                }

                return m_Connector;
            }
        }

        internal Type[] ExpectedTypes { get; set; }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        /// <summary>
        /// Gets the <see cref="Npgsql.EDBParameterCollection">EDBParameterCollection</see>.
        /// </summary>
        /// <value>The parameters of the SQL statement or function (stored procedure). The default is an empty collection.</value>
#if WITHDESIGN
        [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
#endif

        public new EDBParameterCollection Parameters
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Parameters");
                return parameters;
            }
        }


        protected override DbTransaction DbTransaction
        {
            get { return Transaction; }
            set
            {
                Transaction = (EDBTransaction)value;
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "IDbCommand.Transaction", value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Npgsql.EDBTransaction">EDBTransaction</see>
        /// within which the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> executes.
        /// </summary>
        /// <value>The <see cref="Npgsql.EDBTransaction">EDBTransaction</see>.
        /// The default value is a null reference.</value>
#if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif

        public new EDBTransaction Transaction
        {
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Transaction");

                if (this.transaction != null && this.transaction.Connection == null)
                {
                    this.transaction = null;
                }
                return this.transaction;
            }

            set
            {
                EDBEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Transaction", value);

                this.transaction = value;
            }
        }

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
            get
            {
                EDBEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "UpdatedRowSource");

                return updateRowSource;
            }
            set 
            {
                switch (value)
                {
                        // validate value (required based on base type contract)
                    case UpdateRowSource.None:
                    case UpdateRowSource.OutputParameters:
                    case UpdateRowSource.FirstReturnedRecord:
                    case UpdateRowSource.Both:
                        updateRowSource = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns oid of inserted row. This is only updated when using executenonQuery and when command inserts just a single row. If table is created without oids, this will always be 0.
        /// </summary>
        public Int64 LastInsertedOID
        {
            get { return lastInsertedOID; }
        }


        /// <summary>
        /// Attempts to cancel the execution of a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        /// <remarks>This Method isn't implemented yet.</remarks>
        public override void Cancel()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Cancel");

            try
            {
                // get copy for thread safety of null test
                EDBConnector connector = Connector;
                if (connector != null)
                {
                    connector.CancelRequest();
                }
            }
            catch (IOException)
            {
                Connection.ClearPool();
            }
            catch (EDBException)
            {
                // Cancel documentation says the Cancel doesn't throw on failure
            }
        }

        /// <summary>
        /// Create a new command based on this one.
        /// </summary>
        /// <returns>A new NpgsqlCommand object.</returns>
        Object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Create a new command based on this one.
        /// </summary>
        /// <returns>A new EDBCommand object.</returns>
        public EDBCommand Clone()
        {
            // TODO: Add consistency checks.

            EDBCommand clone = new EDBCommand(CommandText, Connection, Transaction);
            clone.CommandTimeout = CommandTimeout;
            clone.CommandType = CommandType;
            clone.DesignTimeVisible = DesignTimeVisible;
            if (ExpectedTypes != null)
            {
                clone.ExpectedTypes = (Type[])ExpectedTypes.Clone();
            }
            foreach (EDBParameter parameter in Parameters)
            {
                clone.Parameters.Add(parameter.Clone());
            }
            return clone;
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.Common.DbParameter">DbParameter</see> object.
        /// </summary>
        /// <returns>An <see cref="System.Data.Common.DbParameter">DbParameter</see> object.</returns>
        protected override DbParameter CreateDbParameter()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateDbParameter");

            return CreateParameter();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Npgsql.EDBParameter">EDBParameter</see> object.
        /// </summary>
        /// <returns>A <see cref="Npgsql.EDBParameter">EDBParameter</see> object.</returns>
        public new EDBParameter CreateParameter()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateParameter");

            return new EDBParameter();
        }

        /// <summary>
        /// Slightly optimised version of ExecuteNonQuery() for internal ues in cases where the number
        /// of affected rows is of no interest.
        /// </summary>
      /*  internal void ExecuteBlind()
        {
            GetReader(CommandBehavior.SequentialAccess).Dispose();
        }
        */
        internal void ExecuteBlind()
        {
            EDBQuery query;

            // Bypass cpmmand parsing overhead and send commandText verbatim.
            query = new EDBQuery(m_Connector, commandText);

            // Block the notification thread before writing anything to the wire.
            using (var blocker = m_Connector.BlockNotificationThread())
            {
                // Write the Query message to the wire.
                m_Connector.Query(query);

                // Flush, and wait for and discard all responses.
                m_Connector.ProcessAndDiscardBackendResponses();
            }
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected if known; -1 otherwise.</returns>
        public override Int32 ExecuteNonQuery()
        {
            //We treat this as a simple wrapper for calling ExecuteReader() and then
            //update the records affected count at every call to NextResult();
            
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteNonQuery");
            int? ret = null;
            m_Connector.Mediator.IsReader = false;

            using (EDBDataReader rdr = GetReader(CommandBehavior.SequentialAccess))
            {
                do
                {
                    int thisRecord = rdr.RecordsAffected;
                    if (thisRecord != -1)
                    {
                        ret = (ret ?? 0) + thisRecord;
                    }
                    lastInsertedOID = rdr.LastInsertedOID ?? lastInsertedOID;
                }
                while (rdr.NextResult());
            }
            return ret ?? -1;
        }

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="behavior">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        public new EDBDataReader ExecuteReader()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader");
            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="cb">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        /// <remarks>Currently the CommandBehavior parameter is ignored.</remarks>
        public new EDBDataReader ExecuteReader(CommandBehavior cb)
        {   
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader", cb);
            
            // Close connection if requested even when there is an error.
                if(m_Connector != null)
                    m_Connector.Mediator.IsReader = true;

                    try
                    {
                        if (connection != null)
                        {
                            if (connection.PreloadReader)
                            {
                                //Adjust behaviour so source reader is sequential access - for speed - and doesn't close the connection - or it'll do so at the wrong time.
                                CommandBehavior adjusted = (cb | CommandBehavior.SequentialAccess) & ~CommandBehavior.CloseConnection;
                                return new CachingDataReader(GetReader(adjusted), cb);
                            }
                        }
                    return GetReader(cb);
                    }
                    catch (Exception)
                    {
                        if ((cb & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
            {
                            connection.Close();
            }
                          throw;
                    }
                
        }
        
        //internal ForwardsOnlyDataReader GetReader(CommandBehavior cb)
        //{
        //    try
        //    {

        //        // reset any responses just before getting new ones
        //        Connector.Mediator.ResetResponses();

        //        // Set command timeout.
        //        m_Connector.Mediator.CommandTimeout = CommandTimeout;

        //        /*
        //         * EDB TEAM:
        //         * Paramters are required in EDBState. We need to bind paremter values returned by server.
        //         * We also require command type for some logical decisions in EDBState.
        //         */
        //        if (parameters != null)
        //            m_Connector.Mediator.Parameters = parameters;
        //        m_Connector.Mediator.Type = commandType;

        //        using (m_Connector.BlockNotificationThread())
        //        {
        //            if (parse == null)
        //            {
        //                return new ForwardsOnlyDataReader(m_Connector.QueryEnum(this), cb, this, m_Connector.BlockNotificationThread(), false);
        //            }
        //            //return new ForwardsOnlyDataReader(m_Connector.QueryEnum(this), cb, this, m_Connector.BlockNotificationThread(), false);
        //            else
        //            {
        //                BindParameters();

        //                ForwardsOnlyDataReader forwardsOnlyDataReader = new ForwardsOnlyDataReader(m_Connector.ExecuteEnum(new EDBExecute(bind.PortalName
        //                    , 0)), cb, this,
        //                                               m_Connector.BlockNotificationThread(), true);
        //                /* EDBTeam:
        //                 * check if any of the out parameter is refcursor. If found get cursor data and associate a
        //                 * reader with it.
        //                 * 
        //                 */
        //                foreach (EDBParameter p in m_Connector.Mediator.Parameters)
        //                {

        //                    /*
        //                     * Check if paramter of type recursor
        //                     * Bind a data reader with as paramter value
        //                     * 
        //                     */
        //                   if (p.EDBDbType == EDBDbType.RefCursor)
        //                   {
        //                       /*
        //                        * if refcurosor is null, then value of the paramter will be empty string
        //                        */
        //                       if (p.Value.ToString().Trim().Equals("fetch all in \"\""))
        //                       {
        //                           p.Value = "";
        //                       }
        //                       else
        //                       {
        //                           /*
        //                            * Check if refcursor value is sent as null from server. Else
        //                            * make a query to fetch all of the cursor result.
        //                            */
        //                           if (p.Value != DBNull.Value)
        //                           {
        //                               p.Value = "fetch all in \"" + p.Value.ToString() + "\"";
        //                               EDBCommand command = new EDBCommand(p.Value.ToString(), Connection);
        //                               m_Connector.Mediator.Type = command.CommandType;
        //                               ForwardsOnlyDataReader rd = (ForwardsOnlyDataReader)command.ExecuteReader();
        //                               p.Value = new CachingDataReader(rd, cb);
        //                           }
        //                       }
                               
        //                   }
        //                }
        //                return forwardsOnlyDataReader;
        //            }
        //        }
        //    }
        //    catch (IOException ex)
        //    {
        //        throw ClearPoolAndCreateException(ex);
        //    }
        //}

        internal ForwardsOnlyDataReader GetReader(CommandBehavior cb)
        {
            CheckConnectionState();

            // reset any responses just before getting new ones
            Connector.Mediator.ResetResponses();

            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            if (parameters != null)
                m_Connector.Mediator.Parameters = parameters;

            m_Connector.Mediator.Type = commandType;


            // Block the notification thread before writing anything to the wire.
            using (m_Connector.BlockNotificationThread())
            {
                IEnumerable<IServerResponseObject> responseEnum;
                ForwardsOnlyDataReader reader;

                if (m_Connector.CommandTimeoutSent == -1 || m_Connector.CommandTimeoutSent != this.CommandTimeout)
                {
                    EDBCommand toq = new EDBCommand(string.Format("SET statement_timeout = {0}", this.CommandTimeout * 1000), m_Connector);

                    toq.ExecuteBlind();

                    m_Connector.CommandTimeoutSent = this.CommandTimeout;
                }

                if (prepared == PrepareStatus.NeedsPrepare)
                {
                    PrepareInternal();
                }

                if (prepared == PrepareStatus.NotPrepared || prepared == PrepareStatus.V2Prepared)
                {
                    EDBQuery query;

                    //GetParseCommandText
                    if (commandType == System.Data.CommandType.StoredProcedure)
                    {
                        m_Connector.Mediator.Type = CommandType.Text;
                        query = new EDBQuery(m_Connector, GetParseCommandText());
                        //m_Connector.Mediator.Type = CommandType.Text;

                    }
                    else
                        query = new EDBQuery(m_Connector, GetCommandText());
                       
                    // Write the Query message to the wire.
                    m_Connector.Query(query);

                    // Flush and wait for responses.
                    responseEnum = m_Connector.ProcessBackendResponsesEnum();

                    /*if (commandType == System.Data.CommandType.StoredProcedure)
                    {

                     //   m_Connector.Flush();
                        reader = new ForwardsOnlyDataReader(m_Connector.ExecuteEnum(new EDBExecute(bind.PortalName, 0)), cb, this,
                                                          m_Connector.BlockNotificationThread(), true);
                    }
                    else */
                        // Construct the return reader.
                        reader = new ForwardsOnlyDataReader(
                            responseEnum,
                            cb,
                            this,
                            m_Connector.BlockNotificationThread()
                        );

                    if (
                        commandType == CommandType.StoredProcedure
                        && reader.FieldCount == 1
                        && reader.GetDataTypeName(0) == "refcursor"
                    )
                    {
                        // When a function returns a sole column of refcursor, transparently
                        // FETCH ALL from every such cursor and return those results.
                        StringWriter sw = new StringWriter();
                        string queryText;

                        while (reader.Read())
                        {
                            sw.WriteLine("FETCH ALL FROM \"{0}\";", reader.GetString(0));
                        }

                        reader.Dispose();

                        queryText = sw.ToString();

                        if (queryText == "")
                        {
                            queryText = ";";
                        }

                        // Passthrough the commandtimeout to the inner command, so user can also control its timeout.
                        // TODO: Check if there is a better way to handle that.

                        query = new EDBQuery(m_Connector, queryText);

                        // Write the Query message to the wire.
                        m_Connector.Query(query);

                        // Flush and wait for responses.
                        responseEnum = m_Connector.ProcessBackendResponsesEnum();

                        // Construct the return reader.
                        reader = new ForwardsOnlyDataReader(
                            responseEnum,
                            cb,
                            this,
                            m_Connector.BlockNotificationThread()
                        );
                    }
                }
                else
                {
                    // Update the Bind object with current parameter data as needed.
                    BindParameters();
                    // Write the Bind, and Sync message to the wire.
                    m_Connector.Bind(bind);

                    if (commandType == System.Data.CommandType.StoredProcedure)
                    {

                        m_Connector.Flush();
                        reader = new ForwardsOnlyDataReader(m_Connector.ExecuteEnum(new EDBExecute(bind.PortalName, 0)), cb, this,
                                                          m_Connector.BlockNotificationThread(), true);

                        foreach (EDBParameter p in m_Connector.Mediator.Parameters)
                        {

                            /*
                             * Check if paramter of type recursor
                             * Bind a data reader with as paramter value
                             * 
                             */
                            if (p.EDBDbType == EDBDbType.RefCursor)
                            {
                                /*
                                 * if refcurosor is null, then value of the paramter will be empty string
                                 */
                                if (p.Value.ToString().Trim().Equals("fetch all in \"\""))
                                {
                                    p.Value = "";
                                }
                                else
                                {
                                    /*
                                     * Check if refcursor value is sent as null from server. Else
                                     * make a query to fetch all of the cursor result.
                                     */
                                    if (p.Value != DBNull.Value)
                                    {
                                        p.Value = "fetch all in \"" + p.Value.ToString() + "\"";
                                        EDBCommand command = new EDBCommand(p.Value.ToString(), Connection);
                                        m_Connector.Mediator.Type = command.CommandType;
                                        ForwardsOnlyDataReader rd = (ForwardsOnlyDataReader)command.ExecuteReader();
                                        p.Value = new CachingDataReader(rd, cb);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        m_Connector.Execute(execute);
                        m_Connector.Sync();

                        // Flush and wait for responses.
                        responseEnum = m_Connector.ProcessBackendResponsesEnum();

                        // Construct the return reader, possibly with a saved row description from Prepare().
                        reader = new ForwardsOnlyDataReader(
                            responseEnum,
                            cb,
                            this,
                            m_Connector.BlockNotificationThread(),
                            true,
                            currentRowDescription
                        );
                    
                    
                    }
              }

                return reader;
            }
        }



        private void CheckConnectionState()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CheckConnectionState");

            // Check the connection state.
            if (Connector == null || Connector.State == ConnectionState.Closed)
            {
                throw new InvalidOperationException(resman.GetString("Exception_ConnectionNotOpen"));
            }
            if (Connector.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(
                    "There is already an open DataReader associated with this Command which must be closed first.");
            }
        }
        ///<summary>
        /// This method binds the parameters from parameters collection to the bind
        /// message.
        /// </summary>
        private void BindParameters()
        {
            if (parameters.Count != 0)
            {
                byte[][] parameterValues = bind.ParameterValues;
                Int16[] parameterFormatCodes = bind.ParameterFormatCodes;
                bool bindAll = false;
                bool bound = false;

                if (parameterValues == null || parameterValues.Length != parameters.Count)
                {
                    parameterValues = new byte[parameters.Count][];
                    bindAll = true;
                }

                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    if (! bindAll && parameters[i].Bound)
                    {
                        continue;
                    }

                    parameterValues[i] = parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, true, Connector.NativeToBackendTypeConverterOptions);

                    bound = true;
                    parameters[i].Bound = true;

                    if (parameterValues[i] == null && parameters[i].EDBDbType != EDBDbType.Date && parameters[i].EDBDbType != EDBDbType.RefCursor)
                    {
                        parameterFormatCodes[i]= (Int16)FormatCode.Binary;
                    } else {
                        parameterFormatCodes[i] = parameters[i].TypeInfo.SupportsBinaryBackendData ? (Int16)FormatCode.Binary : (Int16)FormatCode.Text;
                    }
                   
                }

                if (bound)
                {
                    bind.ParameterValues = parameterValues;
                    bind.ParameterFormatCodes = parameterFormatCodes;
                }
                Connector.RequireReadyForQuery = false;

                Connector.Bind(bind);

                Connector.Flush();
               
            }
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row
        /// in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set,
        /// or a null reference if the result set is empty.</returns>
        public override Object ExecuteScalar()
        {
            using (
                EDBDataReader reader =
                    GetReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow))
            {
                return reader.Read() && reader.FieldCount != 0 ? reader.GetValue(0) : null;
            }
        }


        /// <summary>
        /// Creates a prepared version of the command on a PostgreSQL server.
        /// </summary>
        public override void Prepare()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Prepare");

            // Check the connection state.
            CheckConnectionState();

            if (!m_Connector.SupportsPrepare)
            {
                return; // Do nothing.
            }

            UnPrepare();

            // reset any responses just before getting new ones
            Connector.Mediator.ResetResponses();

            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            PrepareInternal();
        }


   
        /// <summary>
        /// Creates a prepared version of the command on a PostgreSQL server.
        /// </summary>
        public void PrepareInternal()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Prepare");

            // Check the connection state.
      
            // reset any responses just before getting new ones
            Connector.Mediator.ResetResponses();

            foreach (EDBParameter edbParameter in this.parameters)
            {
                if (edbParameter.TypeInfo.EDBDbType == EDBDbType.RefCursor)
                {
                    m_Connector.Mediator.hasRefcursorType = true;
                    break;
                }
            }


            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            if (!m_Connector.SupportsPrepare)
            {
                return; // Do nothing.
            }

            if (m_Connector.BackendProtocolVersion == ProtocolVersion.Version2)
            {
                using (EDBCommand command = new EDBCommand(GetPrepareCommandText(), m_Connector))
                {
                    command.ExecuteBlind();
                }
            }
            else
            {
                using (m_Connector.BlockNotificationThread())
                {
                    try
                    {
                        EDBCommand command = new EDBCommand();
                        command.CommandType = commandType;
                        // Use the extended query parsing...
                        planName = m_Connector.NextPlanName();
                        String portalName = "";// m_Connector.NextPortalName();

                        if (command.commandType == CommandType.StoredProcedure)
                        {
                            parse = new EDBParse(planName, GetParseCommandText(), new Int32[] { }, parameters, command);
                            prepared = PrepareStatus.V3Prepared;
                               
                        }
                        else
                            parse = new EDBParse(planName, GetCommandText(true, true), new Int32[] { }, parameters, command);
                        m_Connector.Sync();
                        m_Connector.Parse(parse,command);

                        // We need that because Flush() doesn't cause backend to send
                        // ReadyForQuery on error. Without ReadyForQuery, we don't return 
                        // from query extended processing.

                        // We could have used Connector.Flush() which sends us back a
                        // ReadyForQuery, but on postgresql server below 8.1 there is an error
                        // with extended query processing which hinders us from using it.
                        m_Connector.RequireReadyForQuery = false;
                       // m_Connector.Flush();
                        Connector.Flush();
                        // Description...
                         EDBDescribe describe = new EDBDescribeStatement(planName);
                        m_Connector.Describe(describe);
                        EDBRowDescription returnRowDesc = m_Connector.Sync();
                        
                        Int16[] resultFormatCodes;


                        if (returnRowDesc != null)
                        {
                            resultFormatCodes = new Int16[returnRowDesc.NumFields];

                            for (int i = 0; i < returnRowDesc.NumFields; i++)
                            {
                                EDBRowDescription.FieldData returnRowDescData = returnRowDesc[i];

                                if (returnRowDescData.TypeInfo != null)
                                {
                                    // Binary format?
                                    // PG always defaults to text encoding.  We can fix up the row description
                                    // here based on support for binary encoding.  Once this is done,
                                    // there is no need to request another row description after Bind.
                                    returnRowDescData.FormatCode = returnRowDescData.TypeInfo.SupportsBinaryBackendData ? FormatCode.Binary : FormatCode.Text;
                                    resultFormatCodes[i] = (Int16)returnRowDescData.FormatCode;
                                }
                                else
                                {
                                    // Text format (default).
                                    resultFormatCodes[i] = (Int16)FormatCode.Text;
                                }
                            }
                        }
                        else
                        {
                            resultFormatCodes = new Int16[] { 0 };
                        }
                        currentRowDescription = returnRowDesc;

                        bind = new EDBBind(portalName, planName, new Int16[Parameters.Count], null, resultFormatCodes);
                      //  bind = new EDBBind("", planName, new Int16[Parameters.Count], null, resultFormatCodes);
                        execute = new EDBExecute(portalName, 0);
                       
                    }
                    catch (IOException e)
                    {
                        throw ClearPoolAndCreateException(e);
                    }
                    catch
                    {

                        // As per documentation:

                        // "[...] When an error is detected while processing any extended-query message,

                        // the backend issues ErrorResponse, then reads and discards messages until a

                        // Sync is reached, then issues ReadyForQuery and returns to normal message processing.[...]"

                        // So, send a sync command if we get any problems.



                        m_Connector.Sync();

                        throw;
                    }
                }
            }
        }

        /*
        /// <summary>
        /// Releases the resources used by the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        protected override void Dispose (bool disposing)
        {
            
            if (disposing)
            {
                // Only if explicitly calling Close or dispose we still have access to 
                // managed resources.
                EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose");
                if (connection != null)
                {
                    connection.Dispose();
                }
                base.Dispose(disposing);
                
            }
        }*/

        private void PrepareInternal_pg()
        {
            if (m_Connector.BackendProtocolVersion == ProtocolVersion.Version2)
            {
                planName = Connector.NextPlanName();

                // BackendEncoding.UTF8Encoding.GetString() is temporary.  A new optimization for
                // ExecuteBlind() will negate the need.
                using (EDBCommand command = new EDBCommand(BackendEncoding.UTF8Encoding.GetString(GetCommandText(true, false)), m_Connector))
                {
                    command.ExecuteBlind();
                    prepared = PrepareStatus.V2Prepared;
                }
            }
            else
            {
                // Use the extended query parsing...
                EDBCommand command = new EDBCommand();
                command.CommandType = CommandType.Text;

                planName = m_Connector.NextPlanName();
                String portalName = "";
                EDBParse parse = new EDBParse(planName, GetCommandText(true, true), new Int32[] { },null,null);
                EDBDescribe statementDescribe = new EDBDescribeStatement(planName);
                IEnumerable<IServerResponseObject> responseEnum;
                EDBRowDescription returnRowDesc = null;

                // Write Parse, Describe, and Sync messages to the wire.
                m_Connector.Parse(parse,command);
                m_Connector.Describe(statementDescribe);
                m_Connector.Sync();

                // Flush and wait for response.
                responseEnum = m_Connector.ProcessBackendResponsesEnum();

                // Look for a NpgsqlRowDescription in the responses, discarding everything else.
                foreach (IServerResponseObject response in responseEnum)
                {
                    if (response is EDBRowDescription)
                    {
                        returnRowDesc = (EDBRowDescription)response;
                    }
                    else if (response is IDisposable)
                    {
                        (response as IDisposable).Dispose();
                    }
                }

                Int16[] resultFormatCodes;

                if (returnRowDesc != null)
                {
                    resultFormatCodes = new Int16[returnRowDesc.NumFields];

                    for (int i = 0; i < returnRowDesc.NumFields; i++)
                    {
                        EDBRowDescription.FieldData returnRowDescData = returnRowDesc[i];

                        if (returnRowDescData.TypeInfo != null)
                        {
                            // Binary format?
                            // PG always defaults to text encoding.  We can fix up the row description
                            // here based on support for binary encoding.  Once this is done,
                            // there is no need to request another row description after Bind.
                            returnRowDescData.FormatCode = returnRowDescData.TypeInfo.SupportsBinaryBackendData ? FormatCode.Binary : FormatCode.Text;
                            resultFormatCodes[i] = (Int16)returnRowDescData.FormatCode;
                        }
                        else
                        {
                            // Text format (default).
                            resultFormatCodes[i] = (Int16)FormatCode.Text;
                        }
                    }
                }
                else
                {
                    resultFormatCodes = new Int16[] { 0 };
                }

                // Save the row description for use with all future Executes.
                currentRowDescription = returnRowDesc;

                // The Bind and Execute message objects live through multiple Executes.
                // Only Bind changes at all between Executes, which is done in BindParameters().
                bind = new EDBBind(portalName, planName, new Int16[Parameters.Count], null, resultFormatCodes);
                execute = new EDBExecute(portalName, 0);

                prepared = PrepareStatus.V3Prepared;
            }
        }

        /// <summary>
        /// This method substitutes the <see cref="Npgsql.NpgsqlCommand.Parameters">Parameters</see>, if exist, in the command
        /// to their actual values.
        /// The parameter name format is <b>:ParameterName</b>.
        /// </summary>
        /// <returns>A version of <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> with the <see cref="Npgsql.NpgsqlCommand.Parameters">Parameters</see> inserted.</returns>
        /// <summary>
        /// This method substitutes the <see cref="Npgsql.NpgsqlCommand.Parameters">Parameters</see>, if exist, in the command
        /// to their actual values.
        /// The parameter name format is <b>:ParameterName</b>.
        /// </summary>
        /// <returns>A version of <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> with the <see cref="Npgsql.NpgsqlCommand.Parameters">Parameters</see> inserted.</returns>
        internal byte[] GetCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetCommandText");

            byte[] ret = string.IsNullOrEmpty(planName) ? GetCommandText(false, false) : GetExecuteCommandText();
            // In constructing the command text, we potentially called internal
            // queries.  Reset command timeout and SQL sent.
            m_Connector.Mediator.ResetResponses();
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            return ret;
        }

        private byte[] GetExecuteCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPreparedCommandText");

            MemoryStream result = new MemoryStream();

            result.WriteString("EXECUTE {0}", planName);

            if (parameters.Count != 0)
            {
                result.WriteByte((byte)ASCIIBytes.ParenLeft);

                for (int i = 0; i < Parameters.Count ; i++)
                {
                    var p = Parameters[i];

                    if (i > 0)
                    {
                        result.WriteByte((byte)ASCIIBytes.Comma);
                    }

                    // Add parentheses wrapping parameter value before the type cast to avoid problems with Int16.MinValue, Int32.MinValue and Int64.MinValue
                    // See bug #1010543
                    result.WriteByte((byte)ASCIIBytes.ParenLeft);

                    byte[] serialization;

                    serialization = p.TypeInfo.ConvertToBackend(p.Value, false, Connector.NativeToBackendTypeConverterOptions);

                    result
                        .WriteBytes(serialization)
                        .WriteBytes((byte)ASCIIBytes.ParenRight);

                    if (p.UseCast)
                    {
                        PGUtil.WriteString(result, string.Format("::{0}", p.TypeInfo.CastName));

                        if (p.TypeInfo.UseSize && (p.Size > 0))
                        {
                            result.WriteString("({0})", p.Size);
                        }
                    }
                }

             result.WriteByte((byte)ASCIIBytes.ParenRight);
            }

            return result.ToArray();
        }
        private Boolean CheckFunctionNeedsColumnDefinitionList()
        {
            // If and only if a function returns "record" and has no OUT ("o" in proargmodes), INOUT ("b"), or TABLE
            // ("t") return arguments to characterize the result columns, we must provide a column definition list.
            // See http://pgfoundry.org/forum/forum.php?thread_id=1075&forum_id=519
            // We would use our Output and InputOutput parameters to construct that column definition list.  If we have
            // no such parameters, skip the check: we could only construct "AS ()", which yields a syntax error.

            // Updated after 0.99.3 to support the optional existence of a name qualifying schema and allow for case insensitivity
            // when the schema or procedure name do not contain a quote.
            // The hard-coded schema name 'public' was replaced with code that uses schema as a qualifier, only if it is provided.

            String returnRecordQuery;

            StringBuilder parameterTypes = new StringBuilder("");

            // Process parameters

            Boolean seenDef = false;
            foreach (EDBParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Input) || (p.Direction == ParameterDirection.InputOutput))
                {
                    parameterTypes.Append(Connection.Connector.OidToNameMapping[p.TypeInfo.Name].OID.ToString() + " ");
                }

                if ((p.Direction == ParameterDirection.Output) || (p.Direction == ParameterDirection.InputOutput))
                {
                    seenDef = true;
                }
            }

            if (!seenDef)
            {
                return false;
            }

            // Process schema name.

            String schemaName = String.Empty;
            String procedureName = String.Empty;

            String[] fullName = CommandText.Split('.');

            String predicate = "prorettype = ( select oid from pg_type where typname = 'record' ) "
                + "and proargtypes=:proargtypes and proname=:proname "
                // proargmodes && array['o','b','t']::"char"[] performs just as well, but it requires PostgreSQL 8.2.
                + "and ('o' = any (proargmodes) OR 'b' = any (proargmodes) OR 't' = any (proargmodes)) is not true";
            if (fullName.Length == 2)
            {
                returnRecordQuery =
                "select count(*) > 0 from pg_proc p left join pg_namespace n on p.pronamespace = n.oid where " + predicate + " and n.nspname=:nspname";

                schemaName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
                procedureName = (fullName[1].IndexOf("\"") != -1) ? fullName[1] : fullName[1].ToLower();
            }
            else
            {
                // Instead of defaulting don't use the nspname, as an alternative, query pg_proc and pg_namespace to try and determine the nspname.
                //schemaName = "public"; // This was removed after build 0.99.3 because the assumption that a function is in public is often incorrect.
                returnRecordQuery =
                    "select count(*) > 0 from pg_proc p where " + predicate;

                procedureName = (CommandText.IndexOf("\"") != -1) ? CommandText : CommandText.ToLower();
            }

            bool ret;

            using (EDBCommand c = new EDBCommand(returnRecordQuery, Connection))
            {
                c.Parameters.Add(new EDBParameter("proargtypes", EDBDbType.Oidvector));
                c.Parameters.Add(new EDBParameter("proname", EDBDbType.Name));

                c.Parameters[0].Value = parameterTypes.ToString();
                c.Parameters[1].Value = procedureName;

                if (schemaName != null && schemaName.Length > 0)
                {
                    c.Parameters.Add(new EDBParameter("nspname", EDBDbType.Name));
                    c.Parameters[2].Value = schemaName;
                }

                ret = (Boolean)c.ExecuteScalar();
            }

            // reset any responses just before getting new ones
            m_Connector.Mediator.ResetResponses();

            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            return ret;
        }


        /// <summary>
        /// Process this.commandText, trimming each distinct command and substituting paramater
        /// tokens.
        /// </summary>
        /// <param name="prepare"></param>
        /// <param name="forExtendQuery"></param>
        /// <returns>UTF8 encoded command ready to be sent to the backend.</returns>
        private byte[] GetCommandText(bool prepare, bool forExtendQuery)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetCommandText");

            MemoryStream commandBuilder = new MemoryStream();
            StringChunk[] chunks;

            chunks = GetDistinctTrimmedCommands(commandText);

            if (chunks.Length > 1)
            {
                if (prepare || commandType == CommandType.StoredProcedure)
                {
                    throw new EDBException("Multiple queries not supported for this command type");
                }
            }

            foreach (StringChunk chunk in chunks)
            {
                if (commandBuilder.Length > 0)
                {
                    commandBuilder
                        .WriteBytes((byte)ASCIIBytes.SemiColon)
                        .WriteBytes(ASCIIByteArrays.LineTerminator);
                }

                if (prepare && !forExtendQuery)
                {
                    commandBuilder
                        .WriteString("PREPARE ")
                        .WriteString(planName)
                        .WriteString(" AS ");
                }

                if (commandType == CommandType.StoredProcedure)
                {
                    if (!prepare && !functionChecksDone)
                    {
                        functionNeedsColumnListDefinition = Parameters.Count != 0 && CheckFunctionNeedsColumnDefinitionList();

                        functionChecksDone = true;
                    }

                    commandBuilder.WriteString(
                        Connector.SupportsPrepare
                        ? "CALL " // This syntax is only available in 7.3+ as well SupportsPrepare.
                        : "SELECT " //Only a single result return supported. 7.2 and earlier.
                    );

                    if (commandText[chunk.Begin + chunk.Length - 1] == ')')
                    {
                        AppendCommandReplacingParameterValues(commandBuilder, commandText, chunk.Begin, chunk.Length, prepare, forExtendQuery);
                    }
                    else
                    {
                        commandBuilder
                            .WriteString(commandText.Substring(chunk.Begin, chunk.Length))
                            .WriteBytes((byte)ASCIIBytes.ParenLeft);

                        if (prepare)
                        {
                            AppendParameterPlaceHolders(commandBuilder);
                        }
                        else
                        {
                            AppendParameterValues(commandBuilder);
                        }

                        commandBuilder.WriteBytes((byte)ASCIIBytes.ParenRight);
                    }

                    if (!prepare && functionNeedsColumnListDefinition)
                    {
                        AddFunctionColumnListSupport(commandBuilder);
                    }
                }
                else if (commandType == CommandType.TableDirect)
                {
                    commandBuilder
                        .WriteString("SELECT * FROM ")
                        .WriteString(commandText.Substring(chunk.Begin, chunk.Length));
                }
                else
                {
                    AppendCommandReplacingParameterValues(commandBuilder, commandText, chunk.Begin, chunk.Length, prepare, forExtendQuery);
                }
            }

            return commandBuilder.ToArray();
        }

        private void AddFunctionColumnListSupport(Stream st)
        {
            bool isFirstOutputOrInputOutput = true;

            PGUtil.WriteString(st, " AS (");

            for (int i = 0; i < Parameters.Count; i++)
            {
                var p = Parameters[i];

                switch (p.Direction)
                {
                    case ParameterDirection.Output:
                    case ParameterDirection.InputOutput:
                        if (isFirstOutputOrInputOutput)
                        {
                            isFirstOutputOrInputOutput = false;
                        }
                        else
                        {
                            st.WriteString(", ");
                        }

                        st
                            .WriteString(p.CleanName)
                            .WriteBytes((byte)ASCIIBytes.Space)
                            .WriteString(p.TypeInfo.Name);

                        break;
                }
            }

            st.WriteByte((byte)ASCIIBytes.ParenRight);
        }

        private enum TokenType
        {
            None,
            LineComment,
            BlockComment,
            Quoted,
            LineCommentBegin,
            BlockCommentBegin,
            BlockCommentEnd,
            Param,
            Colon,
            FullTextMatchOp
        }


        /// <summary>
        /// Find the beginning and end of each distinct SQL command and produce
        /// a list of descriptors, one for each command.  Commands described are trimmed of
        /// leading and trailing white space and their terminating semi-colons.
        /// </summary>
        /// <param name="src">Raw command text.</param>
        /// <returns>List of chunk descriptors.</returns>
        private static StringChunk[] GetDistinctTrimmedCommands(string src)
        {
            TokenType currTokenType = TokenType.None;
            bool quoteEscape = false;
            int currCharOfs = -1;
            int currChunkBeg = 0;
            int currChunkRawLen = 0;
            int currChunkTrimLen = 0;
            List<StringChunk> chunks = new List<StringChunk>();

            foreach (char ch in src)
            {
                currCharOfs++;

                // goto label for character re-evaluation:
            ProcessCharacter:

                switch (currTokenType)
                {
                    case TokenType.None:
                        switch (ch)
                        {
                            case '\'':
                                currTokenType = TokenType.Quoted;

                                currChunkRawLen++;
                                currChunkTrimLen = currChunkRawLen;

                                break;

                            case ';':
                                if (currChunkTrimLen > 0)
                                {
                                    chunks.Add(new StringChunk(currChunkBeg, currChunkTrimLen));
                                }

                                currChunkBeg = currCharOfs + 1;
                                currChunkRawLen = 0;
                                currChunkTrimLen = 0;

                                break;

                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                                if (currChunkTrimLen == 0)
                                {
                                    currChunkBeg++;
                                }
                                else
                                {
                                    currChunkRawLen++;
                                }

                                break;

                            case '/':
                                currTokenType = TokenType.BlockCommentBegin;

                                currChunkRawLen++;
                                currChunkTrimLen = currChunkRawLen;

                                break;

                            case '-':
                                currTokenType = TokenType.LineCommentBegin;

                                currChunkRawLen++;
                                currChunkTrimLen = currChunkRawLen;

                                break;

                            default:
                                currChunkRawLen++;
                                currChunkTrimLen = currChunkRawLen;

                                break;

                        }

                        break;

                    case TokenType.LineCommentBegin:
                        if (ch == '-')
                        {
                            currTokenType = TokenType.LineComment;
                        }
                        else
                        {
                            currTokenType = TokenType.None;
                        }

                        currChunkRawLen++;
                        currChunkTrimLen = currChunkRawLen;

                        break;

                    case TokenType.BlockCommentBegin:
                        if (ch == '*')
                        {
                            currTokenType = TokenType.BlockComment;
                        }
                        else
                        {
                            currTokenType = TokenType.None;
                        }

                        currChunkRawLen++;
                        currChunkTrimLen = currChunkRawLen;

                        break;

                    case TokenType.BlockCommentEnd:
                        if (ch == '/')
                        {
                            currTokenType = TokenType.None;
                        }
                        else
                        {
                            currTokenType = TokenType.BlockComment;
                        }

                        currChunkRawLen++;
                        currChunkTrimLen = currChunkRawLen;

                        break;

                    case TokenType.Quoted:
                        switch (ch)
                        {
                            case '\'':
                                if (quoteEscape)
                                {
                                    quoteEscape = false;
                                }
                                else
                                {
                                    quoteEscape = true;
                                }

                                currChunkRawLen++;
                                currChunkTrimLen = currChunkRawLen;

                                break;

                            default:
                                if (quoteEscape)
                                {
                                    quoteEscape = false;
                                    currTokenType = TokenType.None;

                                    // Re-evaluate this character
                                    goto ProcessCharacter;
                                }
                                else
                                {
                                    currChunkRawLen++;
                                    currChunkTrimLen = currChunkRawLen;
                                }

                                break;

                        }

                        break;

                    case TokenType.LineComment:
                        if (ch == '\n')
                        {
                            currTokenType = TokenType.None;
                        }

                        currChunkRawLen++;
                        currChunkTrimLen = currChunkRawLen;

                        break;

                    case TokenType.BlockComment:
                        if (ch == '*')
                        {
                            currTokenType = TokenType.BlockCommentEnd;
                        }

                        currChunkRawLen++;
                        currChunkTrimLen = currChunkRawLen;

                        break;

                }
            }

            if (currChunkTrimLen > 0)
            {
                chunks.Add(new StringChunk(currChunkBeg, currChunkTrimLen));
            }

            return chunks.ToArray();
        }

        /// <summary>
        /// Append a region of a source command text to an output command, performing parameter token
        /// substitutions.
        /// </summary>
        /// <param name="dest">Stream to which to append output.</param>
        /// <param name="src">Command text.</param>
        /// <param name="begin">Starting index within src.</param>
        /// <param name="length">Length of region to be processed.</param>
        /// <param name="prepare"></param>
        /// <param name="forExtendedQuery"></param>
        private void AppendCommandReplacingParameterValues(Stream dest, string src, int begin, int length, bool prepare, bool forExtendedQuery)
        {
            char lastChar = '\0';
            TokenType currTokenType = TokenType.None;
            char paramMarker = '\0';
            int currTokenBeg = begin;
            int currTokenLen = 0;
            Dictionary<EDBParameter, int> paramOrdinalMap = null;
            int end = begin + length;

            if (prepare)
            {
                paramOrdinalMap = new Dictionary<EDBParameter, int>();

                for (int i = 0; i < parameters.Count; i++)
                {
                    paramOrdinalMap[parameters[i]] = i + 1;
                }
            }

            for (int currCharOfs = begin; currCharOfs < end; currCharOfs++)
            {
                char ch = src[currCharOfs];

                // goto label for character re-evaluation:
            ProcessCharacter:

                switch (currTokenType)
                {
                    case TokenType.None:
                        switch (ch)
                        {
                            case '\'':
                                if (currTokenLen > 0)
                                {
                                    dest.WriteString(src.Substring(currTokenBeg, currTokenLen));
                                }

                                currTokenType = TokenType.Quoted;

                                currTokenBeg = currCharOfs;
                                currTokenLen = 1;

                                break;

                            case ':':
                                if (currTokenLen > 0)
                                {
                                    dest.WriteString(src.Substring(currTokenBeg, currTokenLen));
                                }

                                currTokenType = TokenType.Colon;

                                currTokenBeg = currCharOfs;
                                currTokenLen = 1;

                                break;

                            case '<':
                            case '@':
                                if (currTokenLen > 0)
                                {
                                    dest.WriteString(src.Substring(currTokenBeg, currTokenLen));
                                }

                                currTokenType = TokenType.FullTextMatchOp;

                                currTokenBeg = currCharOfs;
                                currTokenLen = 1;

                                break;

                            case '-':
                                if (currTokenLen > 0)
                                {
                                    dest.WriteString(src.Substring(currTokenBeg, currTokenLen));
                                }

                                currTokenType = TokenType.LineCommentBegin;

                                currTokenBeg = currCharOfs;
                                currTokenLen = 1;

                                break;

                            case '/':
                                if (currTokenLen > 0)
                                {
                                    dest.WriteString(src.Substring(currTokenBeg, currTokenLen));
                                }

                                currTokenType = TokenType.BlockCommentBegin;

                                currTokenBeg = currCharOfs;
                                currTokenLen = 1;

                                break;

                            default:
                                currTokenLen++;

                                break;

                        }

                        break;

                    case TokenType.Param:
                        if (IsParamNameChar(ch))
                        {
                            currTokenLen++;
                        }
                        else
                        {
                            string paramName = src.Substring(currTokenBeg, currTokenLen);
                            EDBParameter parameter;
                            bool wroteParam = false;

                            if (parameters.TryGetValue(paramName, out parameter))
                            {
                               if (
                                  (parameter.Direction == ParameterDirection.Input) ||
                                  (parameter.Direction == ParameterDirection.InputOutput) 
                               
                              )
                                {
                                    if (prepare)
                                    {
                                        AppendParameterPlaceHolder(dest, parameter, paramOrdinalMap[parameter]);
                                    }
                                    else
                                    {
                                        AppendParameterValue(dest, parameter);
                                    }
                                }

                                wroteParam = true;
                            }

                            if (!wroteParam)
                            {
                                dest.WriteString("{0}{1}", paramMarker, paramName);
                            }

                            currTokenType = TokenType.None;
                            currTokenBeg = currCharOfs;
                            currTokenLen = 0;

                            // Re-evaluate this character
                            goto ProcessCharacter;
                        }

                        break;

                    case TokenType.Quoted:
                        switch (ch)
                        {
                            case '\'':
                                currTokenLen++;

                                break;

                            default:
                                if (currTokenLen > 1 && lastChar == '\'')
                                {
                                    dest.WriteString(src.Substring(currTokenBeg, currTokenLen));

                                    currTokenType = TokenType.None;
                                    currTokenBeg = currCharOfs;
                                    currTokenLen = 0;

                                    // Re-evaluate this character
                                    goto ProcessCharacter;
                                }
                                else
                                {
                                    currTokenLen++;
                                }

                                break;

                        }

                        break;

                    case TokenType.LineComment:
                        if (ch == '\n')
                        {
                            currTokenType = TokenType.None;
                        }

                        currTokenLen++;

                        break;

                    case TokenType.BlockComment:
                        if (ch == '*')
                        {
                            currTokenType = TokenType.BlockCommentEnd;
                        }

                        currTokenLen++;

                        break;

                    case TokenType.Colon:
                        if (IsParamNameChar(ch))
                        {
                            // Switch to parameter name token, include this character.
                            currTokenType = TokenType.Param;

                            currTokenBeg = currCharOfs;
                            currTokenLen = 1;
                            paramMarker = ':';
                        }
                        else
                        {
                            // Demote to the unknown token type and continue.
                            currTokenType = TokenType.None;
                            currTokenLen++;
                        }

                        break;

                    case TokenType.FullTextMatchOp:
                        if (lastChar == '@' && IsParamNameChar(ch))
                        {
                            // Switch to parameter name token, include this character.
                            currTokenType = TokenType.Param;

                            currTokenBeg = currCharOfs;
                            currTokenLen = 1;
                            paramMarker = '@';
                        }
                        else
                        {
                            // Demote to the unknown token type.
                            currTokenType = TokenType.None;

                            // Re-evaluate this character
                            goto ProcessCharacter;
                        }

                        break;

                    case TokenType.LineCommentBegin:
                        if (ch == '-')
                        {
                            currTokenType = TokenType.LineComment;
                            currTokenLen++;
                        }
                        else
                        {
                            // Demote to the unknown token type.
                            currTokenType = TokenType.None;

                            // Re-evaluate this character
                            goto ProcessCharacter;
                        }

                        break;

                    case TokenType.BlockCommentBegin:
                        if (ch == '*')
                        {
                            currTokenType = TokenType.BlockComment;
                            currTokenLen++;
                        }
                        else
                        {
                            // Demote to the unknown token type.
                            currTokenType = TokenType.None;

                            // Re-evaluate this character
                            goto ProcessCharacter;
                        }

                        break;

                    case TokenType.BlockCommentEnd:
                        if (ch == '/')
                        {
                            currTokenType = TokenType.None;
                            currTokenLen++;
                        }
                        else
                        {
                            currTokenType = TokenType.BlockComment;
                            currTokenLen++;
                        }

                        break;

                }

                lastChar = ch;
            }

            switch (currTokenType)
            {
                case TokenType.Param:
                    string paramName = src.Substring(currTokenBeg, currTokenLen);
                    EDBParameter parameter;
                    bool wroteParam = false;

                    if (parameters.TryGetValue(paramName, out parameter))
                    {
                        if (
                            (parameter.Direction == ParameterDirection.Input) ||
                            (parameter.Direction == ParameterDirection.InputOutput)
                        )
                        {
                            if (prepare)
                            {
                                AppendParameterPlaceHolder(dest, parameter, paramOrdinalMap[parameter]);
                            }
                            else
                            {
                                AppendParameterValue(dest, parameter);
                            }
                        }

                        wroteParam = true;
                    }

                    if (!wroteParam)
                    {
                        dest.WriteString("{0}{1}", paramMarker, paramName);
                    }

                    break;

                default:
                    if (currTokenLen > 0)
                    {
                        dest.WriteString(src.Substring(currTokenBeg, currTokenLen));
                    }

                    break;

            }
        }
        private static bool IsParamNameChar(char ch)
        {
            if (ch < '.' || ch > 'z')
            {
                return false;
            }
            else
            {
                return ((byte)ParamNameCharTable.GetValue(ch) != 0);
            }
        }
        private void AppendParameterValues(Stream dest)
        {
            bool first = true;

            for (int i = 0; i < parameters.Count; i++)
            {
                EDBParameter parameter = parameters[i];

                if (
                    (parameter.Direction == ParameterDirection.Input) ||
                    (parameter.Direction == ParameterDirection.InputOutput)
                )
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        dest.WriteString(", ");
                    }

                    AppendParameterValue(dest, parameter);
                }
            }
        }

        private void AppendParameterValue(Stream dest, EDBParameter parameter)
        {
            byte[] serialised = parameter.TypeInfo.ConvertToBackend(parameter.Value, false, Connector.NativeToBackendTypeConverterOptions);

            // Add parentheses wrapping parameter value before the type cast to avoid problems with Int16.MinValue, Int32.MinValue and Int64.MinValue
            // See bug #1010543
            // Check if this parenthesis can be collapsed with the previous one about the array support. This way, we could use
            // only one pair of parentheses for the two purposes instead of two pairs.
            dest
                .WriteBytes((byte)ASCIIBytes.ParenLeft)
                .WriteBytes((byte)ASCIIBytes.ParenLeft)
                .WriteBytes(serialised)
                .WriteBytes((byte)ASCIIBytes.ParenRight);

            if (parameter.UseCast)
            {
                dest.WriteString("::{0}", parameter.TypeInfo.CastName);

                if (parameter.TypeInfo.UseSize && (parameter.Size > 0))
                {
                    dest.WriteString("({0})", parameter.Size);
                }
            }

            dest.WriteBytes((byte)ASCIIBytes.ParenRight);
        }
        private class StringChunk
        {
            public readonly int Begin;
            public readonly int Length;

            public StringChunk(int begin, int length)
            {
                this.Begin = begin;
                this.Length = length;
            }
        }
        private static void PassEscapedArray(StringBuilder query, string array)
        {
            bool inTextLiteral = false;
            int endAt = array.Length - 1;//leave last char for separate append as we don't have to continually check we're safe to add the next char too.
            for (int i = 0; i != endAt; ++i)
            {
                if (array[i] == '\'')
                {
                    if (!inTextLiteral)
                    {
                        query.Append("E'");
                        inTextLiteral = true;
                    }
                    else if (array[i + 1] == '\'')//SQL-escaped '
                    {
                        query.Append("''");
                        ++i;
                    }
                    else
                    {
                        query.Append('\'');
                        inTextLiteral = false;
                    }
                }
                else
                    query.Append(array[i]);
            }
            query.Append(array[endAt]);
        }


        private void AppendParameterPlaceHolders(Stream dest)
        {
            bool first = true;

            for (int i = 0; i < parameters.Count; i++)
            {
                EDBParameter parameter = parameters[i];

                if (
                    (parameter.Direction == ParameterDirection.Input) ||
                    (parameter.Direction == ParameterDirection.InputOutput)
                )
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        dest.WriteString(", ");
                    }

                    AppendParameterPlaceHolder(dest, parameter, i + 1);
                }
            }
        }
        private void AppendParameterPlaceHolder(Stream dest, EDBParameter parameter, int paramNumber)
        {
            string parameterSize = "";

           dest.WriteBytes((byte)ASCIIBytes.ParenLeft);

            if (parameter.TypeInfo.UseSize && (parameter.Size > 0))
            {
                parameterSize = string.Format("({0})", parameter.Size);
            }

            if (parameter.UseCast)
            {
                dest.WriteString("${0}::{1}{2}", paramNumber, parameter.TypeInfo.CastName, parameterSize);
            }
            else
            {
                dest.WriteString("${0}{1}", paramNumber, parameterSize);
            }

        dest.WriteBytes((byte)ASCIIBytes.ParenRight);
        }

        private StringBuilder GetClearCommandText()
        {
            if (EDBEventLog.Level == LogLevel.Debug)
            {
                EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetClearCommandText");
            }

            StringBuilder result = PGUtil.TrimStringBuilder(new StringBuilder(commandText));

            switch (commandType)
            {
                case CommandType.TableDirect:
                    return result.Insert(0, "select * from "); // There is no parameter support on table direct.
                case CommandType.StoredProcedure:
                    if (!functionChecksDone)
                    {
                        functionReturnsRecord = Parameters.Count != 0 && !CheckFunctionHasOutParameters() && CheckFunctionReturn("record");

                        functionReturnsRefcursor = CheckFunctionReturn("refcursor");

                        // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection. Also check if command text finishes in a ";" which would make EDB incorrectly append a "()" when executing this command text.
                        switch (result[result.Length - 1])
                        {
                            case ')':
                            case ';':
                                addProcedureParenthesis = false;
                                break;
                            default:
                                addProcedureParenthesis = true;
                                break;
                        }

                        functionChecksDone = true;
                    }

                    result.Insert(0,
                        Connector.SupportsPrepare
                        ? "select * from " // This syntax is only available in 7.3+ as well SupportsPrepare.
                        : "select " //Only a single result return supported. 7.2 and earlier.
                       );
                    break;
            }

            if (parameters.Count == 0)
            {
                if (addProcedureParenthesis)
                    result.Append("()");

                // If function returns ref cursor just process refcursor-result function call
                // and return command which will be used to return data from refcursor.

                if (functionReturnsRefcursor)
                    return ProcessRefcursorFunctionReturn(result.ToString());

                if (functionReturnsRecord)
                    AddFunctionReturnsRecordSupport(result);

                return result;
            }

            // Get parameters in query string to translate them to their actual values.

            // This regular expression gets all the parameters in format :param or @param
            // and everythingelse.
            // This is only needed if query string has parameters. Else, just append the
            // parameter values in order they were put in parameter collection.


            // If parenthesis don't need to be added, they were added by user with parameter names. Replace them.
            if (!addProcedureParenthesis)
            {
                Dictionary<string, EDBParameter> parameterIndex = new Dictionary<string, EDBParameter>(parameters.Count);

                foreach (EDBParameter parameter in parameters)
                    parameterIndex[parameter.CleanName] = parameter;


                StringBuilder sb = new StringBuilder();
                foreach (String s in parameterReplace.Split(result.ToString()))
                    if (s.Length != 0)
                    {
                        EDBParameter p = null;
                        string parameterName = s;

                        if ((parameterName[0] == ':') || (parameterName[0] == '@'))
                        {
                            parameterName = parameterName.Remove(0, 1);

                            parameterIndex.TryGetValue(parameterName, out p);

                        }



                        if (p != null)
                        {
                            // It's a parameter. Lets handle it.
                            switch (p.Direction)
                            {
                                case ParameterDirection.Input:
                                case ParameterDirection.InputOutput:
                                    //Start the probably-redundant parenthesis. Queries should operate much as if they were in the a parameter or
                                    //variable in a postgres function. Generally this is the case without the parentheses (hence "probably redundant")
                                    //but there are exceptions to this rule. E.g. consider the postgres function:
                                    //
                                    //CREATE FUNCTION first_param(integer[])RETURNS int AS'select $1[1]'LANGUAGE 'sql' STABLE STRICT;
                                    //
                                    //The equivalent commandtext would be "select :param[1]", but this fails without the parentheses.
                                    sb.Append('(');
                                    //TODO ZK
                                    string serialised= null;// p.TypeInfo.ConvertToBackend(p.Value, false);
                                //    byte[] serialised = p.TypeInfo.ConvertToBackend(p.Value, false, Connector.NativeToBackendTypeConverterOptions);

                                    // Add parentheses wrapping parameter value before the type cast to avoid problems with Int16.MinValue, Int32.MinValue and Int64.MinValue
                                    // See bug #1010543
                                    // Check if this parenthesis can be collapsed with the previous one about the array support. This way, we could use
                                    // only one pair of parentheses for the two purposes instead of two pairs.
                                    sb.Append('(');
                            
                                    if (Connector.UseConformantStrings)
                                        switch (serialised[0])
                                        {
                                            case '\''://type passed as string or string with type.
                                                //We could test to see if \ is used anywhere, but then we could be doing quite an expensive check (if the value is large) for little gain.
                                                sb.Append("E").Append(serialised);
                                                break;
                                            case 'a':
                                                if (POSTGRES_TEXT_ARRAY.IsMatch(serialised))
                                                    PassEscapedArray(sb, serialised);
                                                else
                                                    sb.Append(serialised);
                                                break;
                                            default:
                                                sb.Append(serialised);
                                                break;
                                        }
                                    else
                                        sb.Append(serialised);

                                    sb.Append(')');


                                    if (p.UseCast)
                                    {
                                        sb.Append("::").Append(p.TypeInfo.CastName);
                                        if (p.TypeInfo.UseSize && (p.Size > 0))
                                            sb.Append('(').Append(p.Size).Append(')');
                                    }

                                    sb.Append(')');//Close probably-redundant parenthesis.
                                    break;
                            }
                        }
                        else
                        {
                            sb.Append(s);
                        }
                    }
                result = sb;
            }

            else
            {
                result.Append('(');

                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    EDBParameter Param = parameters[i];


                    switch (Param.Direction)
                    {
                        case ParameterDirection.Input:
                        case ParameterDirection.InputOutput:
                            // Add parentheses wrapping parameter value before the type cast to avoid problems with Int16.MinValue, Int32.MinValue and Int64.MinValue
                            // See bug #1010543
                            result.Append('(');
                            result.Append(Param.TypeInfo.ConvertToBackend(Param.Value, false));
                            result.Append(')');
                            if (Param.UseCast)
                            {
                                result.Append("::").Append(Param.TypeInfo.CastName);
                                if (Param.TypeInfo.UseSize && Param.Size > 0)
                                    result.Append('(').Append(Param.Size).Append(')');
                            }
                            result.Append(',');
                            break;
                    }
                }


                // Remove a trailing comma added from parameter handling above. If any.
                // Maybe there are only output parameters. If so, there will be no comma.
                if (result[result.Length - 1] == ',')
                    result = result.Remove(result.Length - 1, 1);
                result.Append(')');
            }

            if (functionReturnsRecord)
            {
                AddFunctionReturnsRecordSupport(result);
            }

            // If function returns ref cursor just process refcursor-result function call
            // and return command which will be used to return data from refcursor.

            if (functionReturnsRefcursor)
            {
                return ProcessRefcursorFunctionReturn(result.ToString());
            }

            return result;
        }


        private Boolean CheckFunctionHasOutParameters()
        {
            // Check if this function has output parameters.
            // This is used to enable or not the colum definition list 
            // when calling functions which return record.
            // Functions which has out or inout parameters have return record
            // but doesn't allow column definition list.
            // See http://pgfoundry.org/forum/forum.php?thread_id=1075&forum_id=519
            // for discussion about that.


            // inout parameters are only supported from 8.1+ versions.
            if (Connection.PostgreSqlVersion < new Version(8, 1, 0))
            {
                return false;
            }


            //String outParameterExistanceQuery =
            //    "select count(*) > 0 from pg_proc where proname=:proname and ('o' = any (proargmodes) OR 'b' = any(proargmodes))";


            // Updated after 0.99.3 to support the optional existence of a name qualifying schema and allow for case insensitivity
            // when the schema or procedure name do not contain a quote.
            // The hard-coded schema name 'public' was replaced with code that uses schema as a qualifier, only if it is provided.

            String returnRecordQuery;

            StringBuilder parameterTypes = new StringBuilder("");


            // Process parameters

            foreach (EDBParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Input) || (p.Direction == ParameterDirection.InputOutput))
                {
                    parameterTypes.Append(Connection.Connector.OidToNameMapping[p.TypeInfo.Name].OID + " ");
                }
            }


            // Process schema name.

            String schemaName = String.Empty;
            String procedureName = String.Empty;


            String[] fullName = CommandText.Split('.');

            if (fullName.Length == 2)
            {
                returnRecordQuery =
                    "select count(*) > 0 from pg_proc p left join pg_namespace n on p.pronamespace = n.oid where prorettype = ( select oid from pg_type where typname = 'record' ) and proargtypes=:proargtypes and proname=:proname and n.nspname=:nspname and ('o' = any (proargmodes) OR 'b' = any(proargmodes))";

                schemaName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
                procedureName = (fullName[1].IndexOf("\"") != -1) ? fullName[1] : fullName[1].ToLower();
            }
            else
            {
                // Instead of defaulting don't use the nspname, as an alternative, query pg_proc and pg_namespace to try and determine the nspname.
                //schemaName = "public"; // This was removed after build 0.99.3 because the assumption that a function is in public is often incorrect.
                returnRecordQuery =
                    "select count(*) > 0 from pg_proc p where prorettype = ( select oid from pg_type where typname = 'record' ) and proargtypes=:proargtypes and proname=:proname and ('o' = any (proargmodes) OR 'b' = any(proargmodes))";

                procedureName = (CommandText.IndexOf("\"") != -1) ? CommandText : CommandText.ToLower();
            }


            EDBCommand c = new EDBCommand(returnRecordQuery, Connection);

            c.Parameters.Add(new EDBParameter("proargtypes", EDBDbType.Oidvector));
            c.Parameters.Add(new EDBParameter("proname", EDBDbType.Name));

            c.Parameters[0].Value = parameterTypes.ToString();
            c.Parameters[1].Value = procedureName;

            if (schemaName != null && schemaName.Length > 0)
            {
                c.Parameters.Add(new EDBParameter("nspname", EDBDbType.Name));
                c.Parameters[2].Value = schemaName;
            }


            Boolean ret = (Boolean)c.ExecuteScalar();

            // reset any responses just before getting new ones
            m_Connector.Mediator.ResetResponses();

            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            return ret;
        }

        private Boolean CheckFunctionReturn(String ReturnType)
        {
            // Updated after 0.99.3 to support the optional existence of a name qualifying schema and allow for case insensitivity
            // when the schema or procedure name do not contain a quote.
            // The hard-coded schema name 'public' was replaced with code that uses schema as a qualifier, only if it is provided.

            String returnRecordQuery;

            StringBuilder parameterTypes = new StringBuilder("");


            // Process parameters

            foreach (EDBParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Input) || (p.Direction == ParameterDirection.InputOutput))
                {
                    parameterTypes.Append(Connection.Connector.OidToNameMapping[p.TypeInfo.Name].OID + " ");
                }
            }


            // Process schema name.

            String schemaName = String.Empty;
            String procedureName = String.Empty;


            String[] fullName = CommandText.Split('.');

            if (fullName.Length == 2)
            {
                returnRecordQuery =
                    "select count(*) > 0 from pg_proc p left join pg_namespace n on p.pronamespace = n.oid where prorettype = ( select oid from pg_type where typname = :typename ) and proargtypes=:proargtypes and proname=:proname and n.nspname=:nspname";

                schemaName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
                procedureName = (fullName[1].IndexOf("\"") != -1) ? fullName[1] : fullName[1].ToLower();
            }
            else
            {
                // Instead of defaulting don't use the nspname, as an alternative, query pg_proc and pg_namespace to try and determine the nspname.
                //schemaName = "public"; // This was removed after build 0.99.3 because the assumption that a function is in public is often incorrect.
                returnRecordQuery =
                    "select count(*) > 0 from pg_proc p where prorettype = ( select oid from pg_type where typname = :typename ) and proargtypes=:proargtypes and proname=:proname";

                procedureName = (CommandText.IndexOf("\"") != -1) ? CommandText : CommandText.ToLower();
            }


            bool ret;

            using (EDBCommand c = new EDBCommand(returnRecordQuery, Connection))
            {
                c.Parameters.Add(new EDBParameter("typename", EDBDbType.Name));
                c.Parameters.Add(new EDBParameter("proargtypes", EDBDbType.Oidvector));
                c.Parameters.Add(new EDBParameter("proname", EDBDbType.Name));

                c.Parameters[0].Value = ReturnType;
                c.Parameters[1].Value = parameterTypes.ToString();
                c.Parameters[2].Value = procedureName;

                if (schemaName != null && schemaName.Length > 0)
                {
                    c.Parameters.Add(new EDBParameter("nspname", EDBDbType.Name));
                    c.Parameters[3].Value = schemaName;
                }


                ret = (Boolean)c.ExecuteScalar();
            }

            // reset any responses just before getting new ones
            m_Connector.Mediator.ResetResponses();

            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            return ret;
        }


        private void AddFunctionReturnsRecordSupport(StringBuilder sb)
        {
            sb.Append(" as (");
            foreach (EDBParameter p in Parameters)
                switch (p.Direction)
                {
                    case ParameterDirection.Output:
                    case ParameterDirection.InputOutput:
                        sb.Append(p.CleanName).Append(" ").Append(p.TypeInfo.Name).Append(",");
                        break;
                }
            sb[sb.Length - 1] = ')';

        }

        ///<summary>
        /// This methods takes a string with a function call witch returns a refcursor or a set of
        /// refcursor. It will return the names of the open cursors/portals which will hold
        /// results. In turn, it returns the string which is needed to get the data of this cursors
        /// in form of one resultset for each cursor open. This way, clients don't need to do anything
        /// else besides calling function normally to get results in this way.
        ///</summary>
        private StringBuilder ProcessRefcursorFunctionReturn(String FunctionCall)
        {
            StringBuilder sb = new StringBuilder();
            using (EDBCommand c = new EDBCommand(FunctionCall, Connection))
            {
                using (EDBDataReader dr = c.GetReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                {
                    while (dr.Read())
                    {
                        sb.Append("fetch all from \"").Append(dr.GetString(0)).Append("\";");
                    }
                }
            }

            sb.Append(";"); // Just in case there is no response from refcursor function return.

            // reset any responses just before getting new ones
            m_Connector.Mediator.ResetResponses();

            // Set command timeout.
            m_Connector.Mediator.CommandTimeout = CommandTimeout;

            return sb;
        }


        private StringBuilder GetPreparedCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPreparedCommandText");

            StringBuilder result = new StringBuilder("execute ").Append(planName);

            if (parameters.Count != 0)
            {
                result.Append('(');

                foreach (EDBParameter p in parameters)
                {
                    // Add parentheses wrapping parameter value before the type cast to avoid problems with Int16.MinValue, Int32.MinValue and Int64.MinValue
                    // See bug #1010543
                    result.Append('(');
                    result.Append(p.TypeInfo.ConvertToBackend(p.Value, false));
                    result.Append(')');
                    if (p.UseCast)
                    {
                        result.Append("::").Append(p.TypeInfo.CastName);
                        if (p.TypeInfo.UseSize && (p.Size > 0))
                            result.Append('(').Append(p.Size).Append(')');
                    }
                    result.Append(',');
                }

                result[result.Length - 1] = ')';
            }
            return result;
        }


        private String GetParseCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetParseCommandText");

            Boolean addProcedureParenthesis = false; // Do not add procedure parenthesis by default.

            String parseCommand = commandText;

            if (commandType == CommandType.StoredProcedure)
            {
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!parseCommand.Trim().EndsWith(")"))
                {
                    addProcedureParenthesis = true;
                    parseCommand += "(";
                }

                //parseCommand = string.Format("select * from {0}", parseCommand); // This syntax is only available in 7.3+ as well SupportsPrepare.
                /*
                 * EDBTeam
                 */
                parseCommand = "call " + parseCommand; // This syntax i s only available in 7.3+ as well SupportsPrepare.
            }
            else
            {
                if (commandType == CommandType.TableDirect)
                {
                    return string.Format("select * from {0}", parseCommand); // There is no parameter support on TableDirect.
                }
            }
            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    /*if ((parameters[i].Direction == ParameterDirection.Input) ||
                        (parameters[i].Direction == ParameterDirection.InputOutput))
                    {
                        if (!addProcedureParenthesis)
                        {
                            //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                            parameterName = parameters[i].CleanName;
                            //textCommand = textCommand.Replace(':' + parameterName, "$" + (i+1));
                            
                            // Just add typecast if needed.
                            if (parameters[i].UseCast)
                                parseCommand = ReplaceParameterValue(parseCommand, parameterName, string.Format("${0}::{1}", (i + 1), parameters[i].TypeInfo.CastName));
                            else
                                parseCommand = ReplaceParameterValue(parseCommand, parameterName, string.Format("${0}", (i + 1)));
                        }
                        else
                        {
                            if (parameters[i].UseCast)
                                parseCommand += string.Format("${0}::{1}", (i + 1), parameters[i].TypeInfo.CastName);
                            else
                                parseCommand += string.Format("${0}", (i + 1));
                        }
                        
                        if (parameters[i].TypeInfo.UseSize && (parameters[i].Size > 0))
                        {
                            parseCommand += string.Format("({0})", parameters[i].Size);
                        }
                    
                    }*/
                    parameterName = parameters[i].ParameterName;
                    parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i + 1));
                   // prepared = PrepareStatus.V3Prepared;
                }
            }
            return string.Format("{0}{1}", parseCommand, addProcedureParenthesis ? ")" : string.Empty);
        }


        private String GetPrepareCommandText()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPrepareCommandText");

            Boolean addProcedureParenthesis = false; // Do not add procedure parenthesis by default.

            planName = Connector.NextPlanName();

            StringBuilder command = new StringBuilder("prepare " + planName);

            String textCommand = commandText;

            if (commandType == CommandType.StoredProcedure)
            {
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!textCommand.Trim().EndsWith(")"))
                {
                    addProcedureParenthesis = true;
                    textCommand += "(";
                }

                textCommand = "select * from " + textCommand;
            }
            else if (commandType == CommandType.TableDirect)
            {
                return "select * from " + textCommand; // There is no parameter support on TableDirect.
            }


            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    if ((parameters[i].Direction == ParameterDirection.Input) ||
                        (parameters[i].Direction == ParameterDirection.InputOutput))
                    {
                        if (!addProcedureParenthesis)
                        {
                            //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                            parameterName = parameters[i].CleanName;
                            // The space in front of '$' fixes a parsing problem in 7.3 server
                            // which gives errors of operator when finding the caracters '=$' in
                            // prepare text
                            textCommand = ReplaceParameterValue(textCommand, parameterName, " $" + (i + 1));
                        }
                        else
                        {
                            textCommand += " $" + (i + 1);
                        }
                    }
                }

                //[TODO] Check if there are any missing parameters in the query.
                // For while, an error is thrown saying about the ':' char.

                command.Append('(');

                for (i = 0; i < parameters.Count; i++)
                {
                    //                    command.Append(EDBTypesHelper.GetDefaultTypeInfo(parameters[i].DbType));
                    if (parameters[i].UseCast)
                        command.Append(parameters[i].TypeInfo.Name);
                    else
                        command.Append("unknown");

                    command.Append(',');
                }

                command = command.Remove(command.Length - 1, 1);
                command.Append(')');
            }

            if (addProcedureParenthesis)
            {
                textCommand += ")";
            }

            command.Append(" as ");
            command.Append(textCommand);


            return command.ToString();
        }


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
                throw new IndexOutOfRangeException(String.Format(resman.GetString("Exception_ParamNotInQuery"), parameterName));
            }
            return result;
        }

        private void SetCommandTimeout()
        {
            if (commandTimeoutSet)
                return;
            if (Connection != null)
            {
                timeout = Connection.CommandTimeout;
            }
            else
            {
                timeout = (int)EDBConnectionStringBuilder.GetDefaultValue(Keywords.CommandTimeout);
            }
        }

        internal EDBException ClearPoolAndCreateException(Exception e)
        {
            Connection.ClearPool();
            return new EDBException(resman.GetString("Exception_ConnectionBroken"), e);
        }

        public override bool DesignTimeVisible
        {
            get { return designTimeVisible; }
            set { designTimeVisible = value; }
        }
    }
}
