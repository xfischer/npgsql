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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
#if !DNXCORE50
using System.Transactions;
#endif
using  EnterpriseDB.EDBClient.Logging;
using IsolationLevel = System.Data.IsolationLevel;

namespace  EnterpriseDB.EDBClient
{
    /// <summary>
    /// This class represents a connection to a PostgreSQL server.
    /// </summary>
#if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(EDBConnection))]
#endif
#if DNXCORE50
    public sealed class EDBConnection : DbConnection
#else
    [System.ComponentModel.DesignerCategory("")]
    public sealed class EDBConnection : DbConnection
#endif
    {
        #region Fields

        // Set this when disposed is called.
        bool _disposed;

        // Used when we closed the connector due to an error, but are pretending it's open.
        bool _fakingOpen;
        // Used when the connection is closed but an TransactionScope is still active
        // (the actual close is postponed until the scope ends)
        bool _postponingClose;
        bool _postponingDispose;

        /// <summary>
        /// The parsed connection string set by the user
        /// </summary>
        internal EDBConnectionStringBuilder Settings { get; private set; }

        /// <summary>
        /// The actual string provided by the user for the connection string
        /// </summary>
        string _connectionString;

        /// <summary>
        /// The connector object connected to the backend.
        /// </summary>
        internal EDBConnector Connector { get; set; }

        /// <summary>
        /// A counter that gets incremented every time the connection is (re-)opened.
        /// This allows us to identify an "instance" of connection, which is useful since
        /// some resources are released when a connection is closed (e.g. prepared statements).
        /// </summary>
        internal int OpenCounter { get; private set; }

        internal bool WasBroken { get; set; }

#if !DNXCORE50
        EDBPromotableSinglePhaseNotification Promotable
        {
            get { return _promotable ?? (_promotable = new EDBPromotableSinglePhaseNotification(this)); }
        }
        EDBPromotableSinglePhaseNotification _promotable;
#endif

        /// <summary>
        /// The default TCP/IP port for PostgreSQL.
        /// </summary>
        public const int DefaultPort = 5432;

        /// <summary>
        /// Maximum value for connection timeout.
        /// </summary>
        internal const int TimeoutLimit = 1024;

        static readonly ConcurrentDictionary<string, EDBConnectionStringBuilder> BuilderCache = new ConcurrentDictionary<string, EDBConnectionStringBuilder>();

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Constructors / Init / Open

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="EDBConnection">EDBConnection</see> class.
        /// </summary>
        public EDBConnection() : this(String.Empty) {}

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="EDBConnection">EDBConnection</see> class
        /// and sets the <see cref="EDBConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        /// <param name="builder">The connection used to open the PostgreSQL database.</param>
        public EDBConnection(EDBConnectionStringBuilder builder) : this(builder.ConnectionString) { }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="EDBConnection">EDBConnection</see> class
        /// and sets the <see cref="EDBConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        /// <param name="connectionString">The connection used to open the PostgreSQL database.</param>
        public EDBConnection(string connectionString)
        {
            ConnectionString = connectionString;
            Init();
        }

        void Init()
        {
            NoticeDelegate = OnNotice;
            NotificationDelegate = OnNotification;

#if !DNXCORE50
            // Fix authentication problems. See https://bugzilla.novell.com/show_bug.cgi?id=MONO77559 and
            // http://pgfoundry.org/forum/message.php?msg_id=1002377 for more info.
            RSACryptoServiceProvider.UseMachineKeyStore = true;

            _promotable = new EDBPromotableSinglePhaseNotification(this);
#endif
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the
        /// <see cref="EDBConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        public override void Open()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new ArgumentException("Host can't be null");
            if (string.IsNullOrWhiteSpace(UserName) && !IntegratedSecurity)
                throw new ArgumentException("Either Username must be specified or IntegratedSecurity must be on");
            if (Settings.Password == null && !IntegratedSecurity)
                throw new ArgumentException("Either password must be specified or IntegratedSecurity must be on");
            if (ContinuousProcessing && UseSslStream)
                throw new ArgumentException("ContinuousProcessing can't be turned on with UseSslStream");
            Contract.EndContractBlock();

            // If we're postponing a close (see doc on this variable), the connection is already
            // open and can be silently reused
            if (_postponingClose)
                return;

            CheckConnectionClosed();

            Log.Debug("Opening connnection");

            WasBroken = false;

            try
            {
                // Get a Connector, either from the pool or creating one ourselves.
                if (Pooling)
                {
                    Connector = EDBConnectorPool.ConnectorPoolMgr.RequestConnector(this);
                }
                else
                {
                    Connector = new EDBConnector(this) {
                        ProvideClientCertificatesCallback = ProvideClientCertificatesCallback,
                        UserCertificateValidationCallback = UserCertificateValidationCallback
                    };

                    Connector.Open();
                }

                Connector.Notice += NoticeDelegate;
                Connector.Notification += NotificationDelegate;

#if !DNXCORE50
                if (Enlist)
                {
                    Promotable.Enlist(Transaction.Current);
                }
#endif
            }
            catch
            {
                Connector = null;
                throw;
            }
            OpenCounter++;
            OnStateChange(new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
        }

        #endregion Open / Init

        #region Connection string management

        /// <summary>
        /// Gets or sets the string used to connect to a PostgreSQL database. See the manual for details.
        /// </summary>
        /// <value>The connection string that includes the server name,
        /// the database name, and other parameters needed to establish
        /// the initial connection. The default value is an empty string.
        /// </value>
#if WITHDESIGN
        [RefreshProperties(RefreshProperties.All), DefaultValue(""), RecommendedAsConfigurable(true)]
        [EDBSysDescription("Description_ConnectionString", typeof(EDBConnection)), Category("Data")]
        [Editor(typeof(ConnectionStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (value == null) {
                    value = string.Empty;
                }
                EDBConnectionStringBuilder settings;
                if (!BuilderCache.TryGetValue(value, out settings)) {
                    BuilderCache[value] = settings = new EDBConnectionStringBuilder(value);
                }
                Settings = settings;
                // Note that settings.ConnectionString is canonical and may therefore be different from
                // the provided value
                _connectionString = settings.ConnectionString;
            }
        }

        #endregion Connection string management

        #region Configuration settings

        /// <summary>
        /// Backend server host name.
        /// </summary>
#if !DNXCORE50
        [Browsable(true)]
#endif
        public string Host { get { return Settings.Host; } }

        /// <summary>
        /// Backend server port.
        /// </summary>
#if !DNXCORE50
        [Browsable(true)]
#endif
        public int Port { get { return Settings.Port; } }

        /// <summary>
        /// If true, the connection will attempt to use SslStream instead of an internal TlsClientStream.
        /// </summary>
        public bool UseSslStream { get { return Settings.UseSslStream; } }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection
        /// before terminating the attempt and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</value>

#if WITHDESIGN
        [EDBSysDescription("Description_ConnectionTimeout", typeof(EDBConnection))]
#endif

        public override int ConnectionTimeout { get { return Settings.Timeout; } }

        /// <summary>
        /// Gets the time to wait while trying to execute a command
        /// before terminating the attempt and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for a command to complete. The default value is 20 seconds.</value>
        public int CommandTimeout { get { return Settings.CommandTimeout; } }

        /// <summary>
        /// Gets the time to wait before closing unused connections in the pool if the count
        /// of all connections exeeds MinPoolSize.
        /// </summary>
        /// <remarks>
        /// If connection pool contains unused connections for ConnectionLifeTime seconds,
        /// the half of them will be closed. If there will be unused connections in a second
        /// later then again the half of them will be closed and so on.
        /// This strategy provide smooth change of connection count in the pool.
        /// </remarks>
        /// <value>The time (in seconds) to wait. The default value is 15 seconds.</value>
        public int ConnectionLifeTime { get { return Settings.ConnectionLifeTime; } }

        ///<summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <value>The name of the current database or the name of the database to be
        /// used after a connection is opened. The default value is the empty string.</value>
#if WITHDESIGN
        [EDBSysDescription("Description_Database", typeof(EDBConnection))]
#endif

        public override string Database { get { return Settings.Database; } }

        /// <summary>
        /// Gets the database server name.
        /// </summary>
        public override string DataSource { get { return Settings.Host; } }

        /// <summary>
        /// Gets flag indicating if we are using Synchronous notification or not.
        /// The default value is false.
        /// </summary>
        public bool ContinuousProcessing { get { return Settings.ContinuousProcessing; } }

        /// <summary>
        /// Whether to use Windows integrated security to log in.
        /// </summary>
        public bool IntegratedSecurity { get { return Settings.IntegratedSecurity; } }

        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get { return Settings.Username; } }

        /// <summary>
        /// Determine if connection pooling will be used for this connection.
        /// </summary>
        internal bool Pooling { get { return Settings.Pooling; } }

        internal int MinPoolSize { get { return Settings.MinPoolSize; } }
        internal int MaxPoolSize { get { return Settings.MaxPoolSize; } }
        internal int Timeout { get { return Settings.Timeout; } }
        internal bool Enlist { get { return Settings.Enlist; } }
        internal int BufferSize { get { return Settings.BufferSize; } }
        public string EntityTemplateDatabase { get { return Settings.EntityTemplateDatabase; } }
        public string EntityAdminDatabase { get { return Settings.EntityAdminDatabase; } }

        #endregion Configuration settings

        #region State management

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>A bitwise combination of the <see cref="System.Data.ConnectionState">ConnectionState</see> values. The default is <b>Closed</b>.</value>
#if !DNXCORE50
        [Browsable(false)]
#endif
        public ConnectionState FullState
        {
            get
            {
                if (Connector == null || _disposed)
                {
                    return WasBroken ? ConnectionState.Broken : ConnectionState.Closed;
                }

                switch (Connector.State)
                {
                    case ConnectorState.Closed:
                        return ConnectionState.Closed;
                    case ConnectorState.Connecting:
                        return ConnectionState.Connecting;
                    case ConnectorState.Ready:
                        return ConnectionState.Open;
                    case ConnectorState.Executing:
                        return ConnectionState.Open | ConnectionState.Executing;
                    case ConnectorState.Fetching:
                        return ConnectionState.Open | ConnectionState.Fetching;
                    case ConnectorState.Broken:
                        return ConnectionState.Broken;
                    case ConnectorState.Copy:
                        return ConnectionState.Open | ConnectionState.Fetching;
                    default:
                        throw PGUtil.ThrowIfReached("Unknown connector state: " + Connector.State);
                }
            }
        }

        /// <summary>
        /// Gets whether the current state of the connection is Open or Closed
        /// </summary>
        /// <value>ConnectionState.Open, ConnectionState.Closed or ConnectionState.Connecting</value>
#if !DNXCORE50
        [Browsable(false)]
#endif
        public override ConnectionState State
        {
            get
            {
                var s = FullState;
                if ((s & ConnectionState.Open) != 0)
                    return ConnectionState.Open;
                if ((s & ConnectionState.Connecting) != 0)
                    return ConnectionState.Connecting;
                return ConnectionState.Closed;
            }
        }

        #endregion State management

        #region Commands

        /// <summary>
        /// Creates and returns a <see cref="System.Data.Common.DbCommand">DbCommand</see>
        /// object associated with the <see cref="System.Data.Common.DbConnection">IDbConnection</see>.
        /// </summary>
        /// <returns>A <see cref="System.Data.Common.DbCommand">DbCommand</see> object.</returns>
        protected override DbCommand CreateDbCommand()
        {
            return CreateCommand();
        }

        /// <summary>
        /// Creates and returns a <see cref="EDBCommand">EDBCommand</see>
        /// object associated with the <see cref="EDBConnection">EDBConnection</see>.
        /// </summary>
        /// <returns>A <see cref="EDBCommand">EDBCommand</see> object.</returns>
        public new EDBCommand CreateCommand()
        {
            CheckNotDisposed();
            return new EDBCommand("", this);
        }

        #endregion Commands

        #region Transactions

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">The <see cref="System.Data.IsolationLevel">isolation level</see> under which the transaction should run.</param>
        /// <returns>An <see cref="System.Data.Common.DbTransaction">DbTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently the IsolationLevel ReadCommitted and Serializable are supported by the PostgreSQL backend.
        /// There's no support for nested transactions.
        /// </remarks>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>A <see cref="EDBTransaction">EDBTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently there's no support for nested transactions.
        /// </remarks>
        public new EDBTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="level">The <see cref="System.Data.IsolationLevel">isolation level</see> under which the transaction should run.</param>
        /// <returns>A <see cref="EDBTransaction">EDBTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently the IsolationLevel ReadCommitted and Serializable are supported by the PostgreSQL backend.
        /// There's no support for nested transactions.
        /// </remarks>
        public new EDBTransaction BeginTransaction(IsolationLevel level)
        {
            if (level == IsolationLevel.Chaos)
                throw new NotSupportedException("Unsupported IsolationLevel: " + level);
            Contract.EndContractBlock();

            // Note that beginning a transaction doesn't actually send anything to the backend
            // (only prepends), so strictly speaking we don't have to start a user action.
            // However, we do this for consistency as if we did (for the checks and exceptions)
            using (Connector.StartUserAction())
            {
                if (Connector.InTransaction)
                {
                    throw new NotSupportedException("Nested/Concurrent transactions aren't supported.");
                }

                Log.Debug("Beginning transaction with isolation level " + level, Connector.Id);

                return new EDBTransaction(this, level);
            }
        }

        /// <summary>
        /// When a connection is closed within an enclosing TransactionScope and the transaction
        /// hasn't been promoted, we defer the actual closing until the scope ends.
        /// </summary>
        internal void PromotableLocalTransactionEnded()
        {
            if (_postponingDispose)
                Dispose(true);
            else if (_postponingClose)
                ReallyClose();
        }

#if !DNXCORE50
        /// <summary>
        /// Enlist transation.
        /// </summary>
        /// <param name="transaction"></param>
        public override void EnlistTransaction(Transaction transaction)
        {
            Promotable.Enlist(transaction);
        }
#endif

        #endregion

        #region Close

        internal void EmergencyClose()
        {
            _fakingOpen = true;
        }

        /// <summary>
        /// Releases the connection to the database.  If the connection is pooled, it will be
        /// made available for re-use.  If it is non-pooled, the actual connection will be shutdown.
        /// </summary>
        public override void Close()
        {
            if (Connector == null)
                return;

            Log.Debug("Closing connection", Connector.Id);

#if !DNXCORE50
            if (_promotable != null && _promotable.InLocalTransaction)
            {
                _postponingClose = true;
                return;
            }
#endif

            ReallyClose();
        }

        internal void ReallyClose()
        {
            Log.Trace("Really closing connection", Connector.Id);
            _postponingClose = false;

#if !DNXCORE50
            // clear the way for another promotable transaction
            _promotable = null;
#endif

            Connector.Notification -= NotificationDelegate;
            Connector.Notice -= NoticeDelegate;

            CloseOngoingOperations();

            if (Pooling)
            {
                EDBConnectorPool.ConnectorPoolMgr.ReleaseConnector(this, Connector);
            }
            else
            {
                Connector.Close();

                Connector.ProvideClientCertificatesCallback = null;
                Connector.UserCertificateValidationCallback = null;
            }

            Connector = null;

            OnStateChange(new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));
        }

        /// <summary>
        /// Closes ongoing operations, i.e. an open reader exists or a COPY operation still in progress, as
        /// part of a connection close.
        /// Does nothing if the thread has been aborted - the connector will be closed immediately.
        /// </summary>
        void CloseOngoingOperations()
        {
            if ((Thread.CurrentThread.ThreadState & (ThreadState.Aborted | ThreadState.AbortRequested)) != 0) {
                return;
            }

            if (Connector.CurrentReader != null)
            {
                Connector.CurrentReader.Close();
            }
            else if (Connector.State == ConnectorState.Copy)
            {
           //ZK     Contract.Assert(Connector.CurrentCopyOperation != null);

                // Note: we only want to cancel import operations, since in these cases cancel is safe.
                // Export cancellations go through the PostgreSQL "asynchronous" cancel mechanism and are
                // therefore vulnerable to the race condition in #615.
                if (Connector.CurrentCopyOperation is EDBBinaryImporter ||
                    Connector.CurrentCopyOperation is EDBCopyTextWriter ||
                    (Connector.CurrentCopyOperation is EDBRawCopyStream && ((EDBRawCopyStream)Connector.CurrentCopyOperation).CanWrite))
                {
                    try
                    {
                        Connector.CurrentCopyOperation.Cancel();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Error while cancelling COPY on connector close", e);
                    }
                }

                try
                {
                    Connector.CurrentCopyOperation.Dispose();
                }
                catch (Exception e)
                {
                    Log.Warn("Error while disposing cancelled COPY on connector close", e);
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the
        /// <see cref="EDBConnection">EDBConnection</see>.
        /// </summary>
        /// <param name="disposing"><b>true</b> when called from Dispose();
        /// <b>false</b> when being called from the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _postponingDispose = false;
            if (disposing)
            {
                Close();
                if (_postponingClose)
                {
                    _postponingDispose = true;
                    return;
                }
            }

            base.Dispose(disposing);
            _disposed = true;
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Occurs on NoticeResponses from the PostgreSQL backend.
        /// </summary>
        public event NoticeEventHandler Notice;
        internal NoticeEventHandler NoticeDelegate;

        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        public event NotificationEventHandler Notification;
        internal NotificationEventHandler NotificationDelegate;

        //
        // Internal methods and properties
        //
        internal void OnNotice(object o, EDBNoticeEventArgs e)
        {
            if (Notice != null)
            {
                Notice(this, e);
            }
        }

        internal void OnNotification(object o, EDBNotificationEventArgs e)
        {
            if (Notification != null)
            {
                Notification(this, e);
            }
        }

        #endregion Notifications

        #region SSL

        /// <summary>
        /// Returns whether SSL is being used for the connection.
        /// </summary>
        internal bool IsSecure
        {
            get
            {
                CheckConnectionOpen();
                return Connector.IsSecure;
            }
        }

        /// <summary>
        /// Selects the local Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <remarks>
        /// See <see href="https://msdn.microsoft.com/en-us/library/system.net.security.localcertificateselectioncallback(v=vs.110).aspx"/>
        /// </remarks>
        public ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; set; }

        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// Ignored if <see cref="EDBConnectionStringBuilder.TrustServerCertificate"/> is set.
        /// </summary>
        /// <remarks>
        /// See <see href="https://msdn.microsoft.com/en-us/library/system.net.security.remotecertificatevalidationcallback(v=vs.110).aspx"/>
        /// </remarks>
        public RemoteCertificateValidationCallback UserCertificateValidationCallback { get; set; }

        #endregion SSL

        #region Backend version and capabilities

        /// <summary>
        /// Version of the PostgreSQL backend.
        /// This can only be called when there is an active connection.
        /// </summary>
#if !DNXCORE50
        [Browsable(false)]
#endif
        public Version PostgreSqlVersion
        {
            get
            {
                CheckConnectionOpen();
                return Connector.ServerVersion;
            }
        }

        /// <summary>
        /// PostgreSQL server version.
        /// </summary>
        public override string ServerVersion
        {
            get { return PostgreSqlVersion.ToString(); }
        }

        internal bool IsRedshift
        {
            get
            {
                CheckConnectionOpen();
                return Connector.IsRedshift;
            }
        }


        /// <summary>
        /// Process id of backend server.
        /// This can only be called when there is an active connection.
        /// </summary>
#if !DNXCORE50
        [Browsable(false)]
#endif
        // ReSharper disable once InconsistentNaming
        public int ProcessID
        {
            get
            {
                CheckConnectionOpen();
                return Connector.BackendProcessId;
            }
        }

        /// <summary>
        /// Report whether the backend is expecting standard conformant strings.
        /// In version 8.1, Postgres began reporting this value (false), but did not actually support standard conformant strings.
        /// In version 8.2, Postgres began supporting standard conformant strings, but defaulted this flag to false.
        /// As of version 9.1, this flag defaults to true.
        /// </summary>
#if !DNXCORE50
        [Browsable(false)]
#endif
        public bool UseConformantStrings
        {
            get
            {
                CheckConnectionOpen();
                return Connector.UseConformantStrings;
            }
        }

        /// <summary>
        /// Report whether the backend understands the string literal E prefix (>= 8.1).
        /// </summary>
#if !DNXCORE50
        [Browsable(false)]
#endif
            // ReSharper disable once InconsistentNaming
        public bool Supports_E_StringPrefix
        {
            get
            {
                CheckConnectionOpen();
                return Connector.SupportsEStringPrefix;
            }
        }

        /// <summary>
        /// Report whether the backend understands the hex byte format (>= 9.0).
        /// </summary>
#if !DNXCORE50
        [Browsable(false)]
#endif
        public bool SupportsHexByteFormat
        {
            get
            {
                CheckConnectionOpen();
                return Connector.SupportsHexByteFormat;
            }
        }

        #endregion Backend version and capabilities

        #region Copy

        /// <summary>
        /// Begins a binary COPY FROM STDIN operation, a high-performance data import mechanism to a PostgreSQL table.
        /// </summary>
        /// <param name="copyFromCommand">A COPY FROM STDIN SQL command</param>
        /// <returns>A <see cref="EDBBinaryImporter"/> which can be used to write rows and columns</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        public EDBBinaryImporter BeginBinaryImport(string copyFromCommand)
        {
            if (copyFromCommand == null)
                throw new ArgumentNullException("copyFromCommand");
            if (!copyFromCommand.TrimStart().ToUpper().StartsWith("COPY"))
                throw new ArgumentException("Must contain a COPY FROM STDIN command!", "copyFromCommand");
            Contract.EndContractBlock();

            CheckConnectionOpen();
            Connector.StartUserAction(ConnectorState.Copy);
            try
            {
                var importer = new EDBBinaryImporter(Connector, copyFromCommand);
                Connector.CurrentCopyOperation = importer;
                return importer;
            }
            catch
            {
                if (Connector != null) {  // Connector may have been broken
                    Connector.EndUserAction();
                }
                throw;
            }
        }

        /// <summary>
        /// Begins a binary COPY TO STDIN operation, a high-performance data export mechanism from a PostgreSQL table.
        /// </summary>
        /// <param name="copyToCommand">A COPY TO STDIN SQL command</param>
        /// <returns>A <see cref="EDBBinaryExporter"/> which can be used to read rows and columns</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        public EDBBinaryExporter BeginBinaryExport(string copyToCommand)
        {
            if (copyToCommand == null)
                throw new ArgumentNullException("copyToCommand");
            if (!copyToCommand.TrimStart().ToUpper().StartsWith("COPY"))
                throw new ArgumentException("Must contain a COPY TO STDIN command!", "copyToCommand");
            Contract.EndContractBlock();

            CheckConnectionOpen();
            Connector.StartUserAction(ConnectorState.Copy);
            try
            {
                var exporter = new EDBBinaryExporter(Connector, copyToCommand);
                Connector.CurrentCopyOperation = exporter;
                return exporter;

            }
            catch
            {
                if (Connector != null) {  // Connector may have been broken
                    Connector.EndUserAction();
                }
                throw;
            }
        }

        /// <summary>
        /// Begins a textual COPY FROM STDIN operation, a data import mechanism to a PostgreSQL table.
        /// It is the user's responsibility to send the textual input according to the format specified
        /// in <paramref name="copyFromCommand"/>.
        /// </summary>
        /// <param name="copyFromCommand">A COPY FROM STDIN SQL command</param>
        /// <returns>
        /// A TextWriter that can be used to send textual data.</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        public TextWriter BeginTextImport(string copyFromCommand)
        {
            if (copyFromCommand == null)
                throw new ArgumentNullException("copyFromCommand");
            if (!copyFromCommand.TrimStart().ToUpper().StartsWith("COPY"))
                throw new ArgumentException("Must contain a COPY IN command!", "copyFromCommand");
            Contract.EndContractBlock();

            CheckConnectionOpen();
            Connector.StartUserAction(ConnectorState.Copy);
            var writer = new EDBCopyTextWriter(new EDBRawCopyStream(Connector, copyFromCommand));
            Connector.CurrentCopyOperation = writer;
            return writer;
        }

        /// <summary>
        /// Begins a textual COPY FROM STDIN operation, a data import mechanism to a PostgreSQL table.
        /// It is the user's responsibility to parse the textual input according to the format specified
        /// in <paramref name="copyToCommand"/>.
        /// </summary>
        /// <param name="copyToCommand">A COPY TO STDIN SQL command</param>
        /// <returns>
        /// A TextReader that can be used to read textual data.</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        public TextReader BeginTextExport(string copyToCommand)
        {
            if (copyToCommand == null)
                throw new ArgumentNullException("copyToCommand");
            if (!copyToCommand.TrimStart().ToUpper().StartsWith("COPY"))
                throw new ArgumentException("Must contain a COPY OUT command!", "copyToCommand");
            Contract.EndContractBlock();

            CheckConnectionOpen();
            Connector.StartUserAction(ConnectorState.Copy);
            var reader = new EDBCopyTextReader(new EDBRawCopyStream(Connector, copyToCommand));
            Connector.CurrentCopyOperation = reader;
            return reader;
        }

        /// <summary>
        /// Begins a raw binary COPY operation (TO or FROM), a high-performance data export/import mechanism to a PostgreSQL table.
        /// Note that unlike the other COPY API methods, <see cref="BeginRawBinaryCopy"/> doesn't implement any encoding/decoding
        /// and is unsuitable for structured import/export operation. It is useful mainly for exporting a table as an opaque
        /// blob, for the purpose of importing it back later.
        /// </summary>
        /// <param name="copyCommand">A COPY FROM STDIN or COPY TO STDIN SQL command</param>
        /// <returns>A <see cref="EDBRawCopyStream"/> that can be used to read or write raw binary data.</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        public EDBRawCopyStream BeginRawBinaryCopy(string copyCommand)
        {
            if (copyCommand == null)
                throw new ArgumentNullException("copyCommand");
            if (!copyCommand.TrimStart().ToUpper().StartsWith("COPY"))
                throw new ArgumentException("Must contain a COPY IN command!", "copyCommand");
            Contract.EndContractBlock();

            CheckConnectionOpen();
            Connector.StartUserAction(ConnectorState.Copy);
            try
            {
                var stream = new EDBRawCopyStream(Connector, copyCommand);
                if (!stream.IsBinary)
                {
                    // TODO: Stop the COPY operation gracefully, no breaking
                    Connector.Break();
                    throw new ArgumentException("copyToCommand triggered a text transfer, only binary is allowed", "copyCommand");
                }
                Connector.CurrentCopyOperation = stream;
                return stream;
            }
            catch
            {
                if (Connector != null) {  // Connector may have been broken
                    Connector.EndUserAction();
                }
                throw;
            }
        }

        #endregion

        #region Enum registration

        /// <summary>
        /// Registers an enum type for use with this connection.
        ///
        /// Enum labels are mapped by string. The .NET enum labels must correspond exactly to the PostgreSQL labels;
        /// if another label is used in the database, this can be specified for each label with a EnumLabelAttribute.
        /// If there is a discrepancy between the .NET and database labels while an enum is read or written,
        /// an exception will be raised.
        ///
        /// Can only be invoked on an open connection; if the connection is closed the registration is lost.
        /// </summary>
        /// <remarks>
        /// To avoid registering the type for each connection, use the <see cref="RegisterEnumGlobally{T}"/> method.
        /// </remarks>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding enum type in the database.
        /// If null, the .NET type's name in lowercase will be used
        /// </param>
        /// <typeparam name="TEnum">The .NET enum type to be registered</typeparam>
        public void RegisterEnum<TEnum>(string pgName = null) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
                throw new ArgumentException("An enum type must be provided");
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", "pgName");
            if (State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open and idle to perform registration");
            Contract.EndContractBlock();

            Connector.TypeHandlerRegistry.RegisterEnumType<TEnum>(pgName ?? typeof(TEnum).Name.ToLower());
        }

        /// <summary>
        /// Registers an enum type for use with all connections created from now on. Existing connections aren't affected.
        ///
        /// Enum labels are mapped by string. The .NET enum labels must correspond exactly to the PostgreSQL labels;
        /// if another label is used in the database, this can be specified for each label with a EnumLabelAttribute.
        /// If there is a discrepancy between the .NET and database labels while an enum is read or written,
        /// an exception will be raised.
        /// </summary>
        /// <remarks>
        /// To register the type for a specific connection, use the <see cref="RegisterEnum{T}"/> method.
        /// </remarks>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding enum type in the database.
        /// If null, the .NET type's name in lowercase will be used
        /// </param>
        /// <typeparam name="TEnum">The .NET enum type to be associated</typeparam>
        public static void RegisterEnumGlobally<TEnum>(string pgName = null) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
                throw new ArgumentException("An enum type must be provided");
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", "pgName");
            Contract.EndContractBlock();

            TypeHandlerRegistry.RegisterEnumTypeGlobally<TEnum>(pgName ?? typeof(TEnum).Name.ToLower());
        }

        #endregion

        #region State checks

        void CheckConnectionOpen()
        {
            if (_disposed) {
                throw new ObjectDisposedException(typeof(EDBConnection).Name);
            }

            if (_fakingOpen)
            {
                if (Connector != null)
                {
                    try
                    {
                        Close();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                Open();
                _fakingOpen = false;
            }

            if (_postponingClose || Connector == null)
            {
                throw new InvalidOperationException("Connection is not open");
            }
        }

        void CheckConnectionClosed()
        {
            if (_disposed) {
                throw new ObjectDisposedException(typeof(EDBConnection).Name);
            }

            if (Connector != null) {
                throw new InvalidOperationException("Connection already open");
            }
        }

        void CheckNotDisposed()
        {
            if (_disposed) {
                throw new ObjectDisposedException(typeof(EDBConnection).Name);
            }
        }

        internal void CheckReady()
        {
            if (_disposed) {
                throw new ObjectDisposedException(typeof(EDBConnection).Name);
            }

            if (Connector == null) {
                throw new InvalidOperationException("Connection is not open");
            }
        }

        #endregion State checks

        #region Schema operations
#if !DNXCORE50
        /// <summary>
        /// Returns the supported collections
        /// </summary>
        public override DataTable GetSchema()
        {
            return EDBSchema.GetMetaDataCollections();
        }

        /// <summary>
        /// Returns the schema collection specified by the collection name.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>The collection specified.</returns>
        public override DataTable GetSchema(string collectionName)
        {
            return GetSchema(collectionName, null);
        }

        /// <summary>
        /// Returns the schema collection specified by the collection name filtered by the restrictions.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="restrictions">
        /// The restriction values to filter the results.  A description of the restrictions is contained
        /// in the Restrictions collection.
        /// </param>
        /// <returns>The collection specified.</returns>
        public override DataTable GetSchema(string collectionName, string[] restrictions)
        {
            switch (collectionName)
            {
                case "MetaDataCollections":
                    return EDBSchema.GetMetaDataCollections();
                case "Restrictions":
                    return EDBSchema.GetRestrictions();
                case "DataSourceInformation":
                    return EDBSchema.GetDataSourceInformation();
                case "DataTypes":
                    throw new NotSupportedException();
                case "ReservedWords":
                    return EDBSchema.GetReservedWords();
                    // custom collections for  EnterpriseDB.EDBClient
                case "Databases":
                    return EDBSchema.GetDatabases(this, restrictions);
                case "Schemata":
                    return EDBSchema.GetSchemata(this, restrictions);
                case "Tables":
                    return EDBSchema.GetTables(this, restrictions);
                case "Columns":
                    return EDBSchema.GetColumns(this, restrictions);
                case "Views":
                    return EDBSchema.GetViews(this, restrictions);
                case "Users":
                    return EDBSchema.GetUsers(this, restrictions);
                case "Indexes":
                    return EDBSchema.GetIndexes(this, restrictions);
                case "IndexColumns":
                    return EDBSchema.GetIndexColumns(this, restrictions);
                case "Constraints":
                case "PrimaryKey":
                case "UniqueKeys":
                case "ForeignKeys":
                    return EDBSchema.GetConstraints(this, restrictions, collectionName);
                case "ConstraintColumns":
                    return EDBSchema.GetConstraintColumns(this, restrictions);
                default:
                    throw new ArgumentOutOfRangeException("collectionName", collectionName, "Invalid collection name");
            }
        }

#endif
        #endregion Schema operations

        #region Misc

        /// <summary>
        /// This method changes the current database by disconnecting from the actual
        /// database and connecting to the specified.
        /// </summary>
        /// <param name="dbName">The name of the database to use in place of the current database.</param>
        public override void ChangeDatabase(String dbName)
        {
            if (dbName == null)
                throw new ArgumentNullException("dbName");
            if (string.IsNullOrEmpty(dbName))
                throw new ArgumentOutOfRangeException("dbName", dbName, String.Format("Invalid database name: {0}", dbName));
            Contract.EndContractBlock();

            CheckNotDisposed();
            Log.Debug("Changing database to " + dbName, Connector.Id);

            Close();

            // Mutating the current `settings` object would invalidate the cached instance, so work on a copy instead.
            Settings = Settings.Clone();
            Settings.Database = dbName;
            _connectionString = Settings.ConnectionString;

            Open();
        }

#if !DNXCORE50
        /// <summary>
        /// DB provider factory.
        /// </summary>
        protected override DbProviderFactory DbProviderFactory
        {
            get { return EDBFactory.Instance; }
        }
#endif

        /// <summary>
        /// Clear connection pool.
        /// </summary>
        public static void ClearPool(EDBConnection connection)
        {
            EDBConnectorPool.ConnectorPoolMgr.ClearPool(connection);
        }

        /// <summary>
        /// Clear all connection pools.
        /// </summary>
        public static void ClearAllPools()
        {
            EDBConnectorPool.ConnectorPoolMgr.ClearAllPools();
        }

        /// <summary>
        /// Flushes the type cache for this connection's connection string and reloads the
        /// types for this connection only.
        /// </summary>
        internal void ReloadTypes()
        {
            TypeHandlerRegistry.ClearBackendTypeCache(ConnectionString);
            TypeHandlerRegistry.Setup(Connector);
        }

        #endregion Misc
    }

    #region Delegates

    /// <summary>
    /// Represents the method that handles the <see cref="EDBConnection.Notification">Notice</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDBNoticeEventArgs">EDBNoticeEventArgs</see> that contains the event data.</param>
    public delegate void NoticeEventHandler(Object sender, EDBNoticeEventArgs e);

    /// <summary>
    /// Represents the method that handles the <see cref="EDBConnection.Notification">Notification</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="EDBNotificationEventArgs">EDBNotificationEventArgs</see> that contains the event data.</param>
    public delegate void NotificationEventHandler(Object sender, EDBNotificationEventArgs e);

    /// <summary>
    /// Represents the method that allows the application to provide a certificate collection to be used for SSL client authentication
    /// </summary>
    /// <param name="certificates">A <see cref="System.Security.Cryptography.X509Certificates.X509CertificateCollection">X509CertificateCollection</see> to be filled with one or more client certificates.</param>
    public delegate void ProvideClientCertificatesCallback(X509CertificateCollection certificates);

    #endregion
}
