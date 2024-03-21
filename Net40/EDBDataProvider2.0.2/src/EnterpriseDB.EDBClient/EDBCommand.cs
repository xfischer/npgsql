// created on 21/5/2002 at 20:03

// EnterpriseDB.EDBClient.EDBCommand.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using EDBTypes;

#if WITHDESIGN

#endif

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
#if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(EDBCommand)), ToolboxItem(true)]
#endif

    public sealed partial class EDBCommand : DbCommand, ICloneable
    {
        private enum PrepareStatus
        {
            NotPrepared,
            NeedsPrepare,
            Prepared
        }

        // Logging related values
        private static readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);

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

        private PrepareStatus prepared = PrepareStatus.NotPrepared;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private byte[] preparedCommandText = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private EDBBind bind = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private EDBExecute execute = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private EDBExecuteOut executeOut = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private EDBRowDescription currentRowDescription = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        private Int64 lastInsertedOID = 0;

        // locals about function support so we don`t need to check it everytime a function is called.
        private Boolean functionChecksDone = false;
        private Boolean functionNeedsColumnListDefinition = false; // Functions don't return record by default.

        private Boolean commandTimeoutSet = false;

        private UpdateRowSource updateRowSource = UpdateRowSource.Both;

        private bool disposed;
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
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see> class.
        /// </summary>
        public EDBCommand()
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            : this(String.Empty, null, null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see> class with the text of the query.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        public EDBCommand(String cmdText)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            : this(cmdText, null, null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see> class with the text of the query and a <see cref="EnterpriseDB.EDBClient.EDBConnection">EDBConnection</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="EnterpriseDB.EDBClient.EDBConnection">EDBConnection</see> that represents the connection to a PostgreSQL server.</param>
        public EDBCommand(String cmdText, EDBConnection connection)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            : this(cmdText, connection, null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see> class with the text of the query, a <see cref="EnterpriseDB.EDBClient.EDBConnection">EDBConnection</see>, and the <see cref="EnterpriseDB.EDBClient.EDBTransaction">EDBTransaction</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="EnterpriseDB.EDBClient.EDBConnection">EDBConnection</see> that represents the connection to a PostgreSQL server.</param>
        /// <param name="transaction">The <see cref="EnterpriseDB.EDBClient.EDBTransaction">EDBTransaction</see> in which the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see> executes.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBCommand(String cmdText, EDBConnection connection, EDBTransaction transaction)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            planName = String.Empty;
            commandText = cmdText;
            this.connection = connection;

            if (this.connection != null)
            {
                this.m_Connector = connection.Connector;
                
                if(this.m_Connector != null)
                    this.m_Connector.Mediator.Type = System.Data.CommandType.Text;

                if (m_Connector != null && m_Connector.AlwaysPrepare)
                {
                    CommandTimeout = m_Connector.DefaultCommandTimeout;
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal EDBCommand(String cmdText, EDBConnector connector, int CommandTimeout = 20)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
        /// <see cref="EnterpriseDB.EDBClient.EDBCommand.CommandText">CommandText</see> property is to be interpreted.
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

        /// <summary>
        /// DB connection.
        /// </summary>
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
        /// Gets or sets the <see cref="EnterpriseDB.EDBClient.EDBConnection">EDBConnection</see>
        /// used by this instance of the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see>.
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

                if (connection != null) {
                    connection.StateChange -= OnConnectionStateChange;
                }
                this.connection = value;
                if (connection != null) {
                    connection.StateChange += OnConnectionStateChange;
                }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Transaction = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                if (this.connection != null)
                {
                    m_Connector = this.connection.Connector;
                    prepared = m_Connector != null && m_Connector.AlwaysPrepare ? PrepareStatus.NeedsPrepare : PrepareStatus.NotPrepared;
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

        void OnConnectionStateChange(object sender, StateChangeEventArgs stateChangeEventArgs)
        {
            switch (stateChangeEventArgs.CurrentState)
            {
                case ConnectionState.Broken:
                case ConnectionState.Closed:
                    prepared = PrepareStatus.NotPrepared;
                    break;
                case ConnectionState.Open:
                    switch (stateChangeEventArgs.OriginalState)
                    {
                        case ConnectionState.Closed:
                        case ConnectionState.Broken:
                            prepared = m_Connector != null && m_Connector.AlwaysPrepare ? PrepareStatus.NeedsPrepare : PrepareStatus.NotPrepared;
                            break;
                    }
                    break;
                case ConnectionState.Connecting:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal Type[] ExpectedTypes { get; set; }

        /// <summary>
        /// DB parameter collection.
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        /// <summary>
        /// Gets the <see cref="EnterpriseDB.EDBClient.EDBParameterCollection">EDBParameterCollection</see>.
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

        /// <summary>
        /// DB transaction.
        /// </summary>
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
        /// Gets or sets the <see cref="EnterpriseDB.EDBClient.EDBTransaction">EDBTransaction</see>
        /// within which the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see> executes.
        /// </summary>
        /// <value>The <see cref="EnterpriseDB.EDBClient.EDBTransaction">EDBTransaction</see>.
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    this.transaction = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
#pragma warning disable CS8603 // Possible null reference return.
                return this.transaction;
#pragma warning restore CS8603 // Possible null reference return.
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
        /// Returns whether this query will execute as a prepared (compiled) query.
        /// </summary>
        public bool IsPrepared
        {
            get
            {
                switch (prepared)
                {
                    case PrepareStatus.NotPrepared:
                        return false;
                    case PrepareStatus.NeedsPrepare:
                    case PrepareStatus.Prepared:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
        /// <returns>A new EDBCommand object.</returns>
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
        /// Creates a new instance of a <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see> object.
        /// </summary>
        /// <returns>A <see cref="EnterpriseDB.EDBClient.EDBParameter">EDBParameter</see> object.</returns>
        public new EDBParameter CreateParameter()
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateParameter");

            return new EDBParameter();
        }

        /// <summary>
        /// Releases the resources used by the <see cref="EnterpriseDB.EDBClient.EDBCommand">EDBCommand</see>.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Note: we only actually perform cleanup here if called from Dispose() (disposing=true), and not
                // if called from a finalizer (disposing=false). This is because we cannot perform any SQL
                // operations from the finalizer (connection may be in use by someone else).
                // We can implement a queue-based solution that will perform cleanup during the next possible
                // window, but this isn't trivial (should not occur in transactions because of possible exceptions,
                // etc.).
                if (prepared == PrepareStatus.Prepared)
                    ExecuteBlind(m_Connector, "DEALLOCATE " + planName);
            }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Transaction = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Connection = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            disposed = true;
            base.Dispose(disposing);
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

        /// <summary>
        /// Design time visible.
        /// </summary>
        public override bool DesignTimeVisible
        {
            get { return designTimeVisible; }
            set { designTimeVisible = value; }
        }
    }
}
