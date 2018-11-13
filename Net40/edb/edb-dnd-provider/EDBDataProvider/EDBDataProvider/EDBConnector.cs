//	Copyright (C) 2002 The EDB Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//	Connector.cs
// ------------------------------------------------------------------
//	Project
//		EDB
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - created
//		          - 06/??/2004 - Glen Parker<glenebob@nwlink.com> rewritten

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Data;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Net.Sockets;

using Mono.Security.Protocol.Tls;

using EDBTypes;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// !!! Helper class, for compilation only.
    /// Connector implements the logic for the Connection Objects to
    /// access the physical connection to the database, and isolate
    /// the application developer from connection pooling internals.
    /// </summary>
    internal class EDBConnector
    {
        // Immutable.
        internal EDBConnectionString                ConnectionString;

        /// <summary>
        /// Occurs on NoticeResponses from the PostgreSQL backend.
        /// </summary>
        internal event NoticeEventHandler			         Notice;

        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        internal event NotificationEventHandler        Notification;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateSelectionCallback delegate.
        /// </summary>
        internal event CertificateSelectionCallback    CertificateSelectionCallback;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateValidationCallback delegate.
        /// </summary>
        internal event CertificateValidationCallback   CertificateValidationCallback;

        /// <summary>
        /// Mono.Security.Protocol.Tls.PrivateKeySelectionCallback delegate.
        /// </summary>
        internal event PrivateKeySelectionCallback     PrivateKeySelectionCallback;

        private ConnectionState                  _connection_state;

        // The physical network connection to the backend.
        private Stream                           _stream;

        private Socket                           _socket;
         
        // Mediator which will hold data generated from backend.
        private EDBMediator                   _mediator;

        private ProtocolVersion                  _backendProtocolVersion;
        private ServerVersion                    _serverVersion;

        // Values for possible CancelRequest messages.
        private EDBBackEndKeyData             _backend_keydata;

        // Flag for transaction status.
        //        private Boolean                         _inTransaction = false;
        private EDBTransaction                _transaction = null;

        private Boolean                          _supportsPrepare = false;

		private Boolean							_supportCallable = false ;  //EDB team 

        private EDBBackendTypeMapping         _oidToNameMapping = null;

        private Encoding                         _encoding;

        private Boolean                          _isInitialized;

        private Boolean                          _pooled;
        private Boolean                          _shared;

        private EDBState                      _state;


        private Int32                            _planIndex;
        private Int32                            _portalIndex;

        private const String                     _planNamePrefix = "EDBPlan";
        private const String                     _portalNamePrefix = "EDBPortal";
             
        private Thread                           _notificationThread;
        
        // The AutoResetEvent to synchronize processing threads.
        internal AutoResetEvent                   _notificationAutoResetEvent;
        
        // Counter of notification thread start/stop requests in order to 
        internal Int16                            _notificationThreadStopCount;



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Shared">Controls whether the connector can be shared.</param>
        public EDBConnector(EDBConnectionString ConnectionString, bool Pooled, bool Shared)
        {
            this.ConnectionString = ConnectionString;
            _connection_state = ConnectionState.Closed;
            _pooled = Pooled;
            _shared = Shared;
            _isInitialized = false;
            _state = EDBClosedState.Instance;
            _mediator = new EDBMediator();
            _oidToNameMapping = new EDBBackendTypeMapping();
            _planIndex = 0;
            _portalIndex = 0;
            _notificationThreadStopCount = 1;
            _notificationAutoResetEvent = new AutoResetEvent(true);

        }


        internal String Host
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.Host);
            }
        }

        internal Int32 Port
        {
            get
            {
                return ConnectionString.ToInt32(ConnectionStringKeys.Port, ConnectionStringDefaults.Port);
            }
        }

        internal String Database
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.Database, UserName);
            }
        }

        internal String UserName
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.UserName);
            }
        }

        internal String Password
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.Password);
            }
        }

        internal Boolean SSL
        {
            get
            {
                return ConnectionString.ToBool(ConnectionStringKeys.SSL);
            }
        }
		

        internal SslMode SslMode
        {
            get
            {
                return ConnectionString.ToSslMode(ConnectionStringKeys.SslMode);
            }
        }

        internal Int32 ConnectionTimeout
        {
            get
            {
                return ConnectionString.ToInt32(ConnectionStringKeys.Timeout, ConnectionStringDefaults.Timeout);
            }
        }

        internal Int32 CommandTimeout
        {
            get
            {
                return ConnectionString.ToInt32(ConnectionStringKeys.CommandTimeout, ConnectionStringDefaults.CommandTimeout);
            }
        }
        

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        internal ConnectionState State {
            get
            {
			
                return _connection_state;
            }
        }


        // State
        internal void Query (EDBCommand queryCommand)
        {
            CurrentState.Query(this, queryCommand );
        }

        internal void Authenticate (string password)
        {
            CurrentState.Authenticate(this, password );
        }

        internal void Parse (EDBParse parse ,EDBCommand cmd)
        {
			
            CurrentState.Parse(this, parse ,cmd,this.SupportsCallable);  //Change EDB Team 28Th OCT
			
        }
		

        internal void Flush ()
        {
            CurrentState.Flush(this);
        }

        internal void Sync ()
        {
            CurrentState.Sync(this);
        }

        internal void Bind (EDBBind bind)
        {
            CurrentState.Bind(this, bind);
        }

        internal void Describe (EDBDescribe describe)
        {
            CurrentState.Describe(this, describe);
        }

        internal void Execute (EDBExecute execute)
        {
            CurrentState.Execute(this, execute);
        }



        /// <summary>
        /// This method checks if the connector is still ok.
        /// We try to send a simple query text, select 1 as ConnectionTest;
        /// </summary>
	
        internal Boolean IsValid()
        {
            try
            {
                // Here we use a fake EDBCommand, just to send the test query string.
                Query(new EDBCommand("select 1 as ConnectionTest"));
                Mediator.ResetResponses();
                Mediator.ResetExpectations();
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// This method is responsible for releasing all resources associated with this Connector.
        /// </summary>

        internal void ReleaseResources()
        {
            ReleasePlansPortals();
            ReleaseRegisteredListen();


        }

        internal void ReleaseRegisteredListen()
        {
            Query(new EDBCommand("unlisten *", this));

        }

        /// <summary>
        /// This method is responsible to release all portals used by this Connector.
        /// </summary>
        internal void ReleasePlansPortals()
        {
            Int32 i = 0;

            if (_planIndex > 0)
            {
                for(i = 1; i <= _planIndex ; i++)
                    Query(new EDBCommand(String.Format("deallocate \"{0}\";", _planNamePrefix + i)));
            }

            _portalIndex = 0;
            _planIndex = 0;


        }



        /// <summary>
        /// Check for mediator errors (sent by backend) and throw the appropriate
        /// exception if errors found.  This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckErrors()
        {
            if (_mediator.Errors.Count > 0)
            {
                throw new EDBException(_mediator.Errors);
            }
        }

        /// <summary>
        /// Check for notices and fire the appropiate events.
        /// This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckNotices()
        {
            if (Notice != null)
            {
                foreach (EDBError E in _mediator.Notices)
                {
                    Notice(this, new EDBNoticeEventArgs(E));
                }
            }
        }

        /// <summary>
        /// Check for notifications and fire the appropiate events.
        /// This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckNotifications()
        {
            if (Notification != null)
            {
                foreach (EDBNotificationEventArgs E in _mediator.Notifications)
                {
                    Notification(this, E);
                }
            }
        }

        /// <summary>
        /// Check for errors AND notifications in one call.
        /// </summary>
        internal void CheckErrorsAndNotifications()
        {
            CheckNotices();
            CheckNotifications();
            CheckErrors();
        }

        /// <summary>
        /// Default SSL CertificateSelectionCallback implementation.
        /// </summary>
        internal X509Certificate DefaultCertificateSelectionCallback(
            X509CertificateCollection      clientCertificates,
            X509Certificate                serverCertificate,
            string                         targetHost,
            X509CertificateCollection      serverRequestedCertificates)
        {
            if (CertificateSelectionCallback != null)
            {
                return CertificateSelectionCallback(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Default SSL CertificateValidationCallback implementation.
        /// </summary>
        internal bool DefaultCertificateValidationCallback(
            X509Certificate       certificate,
            int[]                 certificateErrors)
        {
            if (CertificateValidationCallback != null)
            {
                return CertificateValidationCallback(certificate, certificateErrors);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Default SSL PrivateKeySelectionCallback implementation.
        /// </summary>
        internal AsymmetricAlgorithm DefaultPrivateKeySelectionCallback(
            X509Certificate                certificate,
            string                         targetHost)
        {
            if (PrivateKeySelectionCallback != null)
            {
                return PrivateKeySelectionCallback(certificate, targetHost);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Version of backend server this connector is connected to.
        /// </summary>
        internal ServerVersion ServerVersion
        {
            get
            {
                return _serverVersion;
            }
            set
            {
                _serverVersion = value;
            }
        }

        internal Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        /// <summary>
        /// Backend protocol version in use by this connector.
        /// </summary>
        internal ProtocolVersion BackendProtocolVersion
        {
            get
            {
                return _backendProtocolVersion;
            }
            set
            {
                _backendProtocolVersion = value;
            }
        }

        /// <summary>
        /// The physical connection stream to the backend.
        /// </summary>
        internal Stream Stream {
            get
            {
                return _stream;
            }
            set
            {
                _stream = value;
            }
        }

        /// <summary>
        /// The physical connection socket to the backend.
        /// </summary>

        internal Socket Socket {
            get
            {
                return _socket;
            }
            set
            {
                _socket = value;
            }
        }

        /// <summary>
        /// Reports if this connector is fully connected.
        /// </summary>
        internal Boolean IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            set
            {
                _isInitialized = value;
            }
        }

        internal EDBState CurrentState {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }


        internal bool Pooled
        {
            get
            {
                return _pooled;
            }
        }

        internal bool Shared
        {
            get
            {
                return _shared;
            }
        }

        internal EDBBackEndKeyData BackEndKeyData {
            get
            {
                return _backend_keydata;
            }
        }

        internal EDBBackendTypeMapping OidToNameMapping {
            get
            {
                return _oidToNameMapping;
            }
        }

        /// <summary>
        /// The connection mediator.
        /// </summary>
        internal EDBMediator	Mediator {
            get
            {
                return _mediator;
            }
        }

        /// <summary>
        /// Report if the connection is in a transaction.
        /// </summary>
        internal EDBTransaction Transaction {
            get
            {
                return _transaction;
            }
            set
            {
                _transaction = value;
            }
        }

        /// <summary>
        /// Report whether the current connection can support prepare functionality.
        /// </summary>
        internal Boolean SupportsPrepare {
            get
            {
                return _supportsPrepare;
            }
            set
            {
                _supportsPrepare = value;
            }
        }
		
        
		
		
		/// <summary>
        /// This method is required to set all the version dependent features flags.
        /// SupportsPrepare means the server can use prepared query plans (7.3+)
        /// </summary>
        // FIXME - should be private
        internal void ProcessServerVersion ()
        {
            this._supportsPrepare = (ServerVersion >= new ServerVersion(7,3,0));
			this._supportCallable = (ServerVersion >= new ServerVersion(8,0,3));  //EDB Team Zahid K: 28th OCT 2005

        }

        /// <value>Counts the numbers of Connections that share
        /// this Connector. Used in Release() to decide wether this
        /// connector is to be moved to the PooledConnectors list.</value>
        // internal int mShareCount;

        /// <summary>
        /// Opens the physical connection to the server.
        /// </summary>
        /// <remarks>Usually called by the RequestConnector
        /// Method of the connection pool manager.</remarks>
        internal void Open()
        {
            ProtocolVersion      PV;

            // If Connection.ConnectionString specifies a protocol version, we will
            // not try to fall back to version 2 on failure.
            if (ConnectionString.Contains(ConnectionStringKeys.Protocol))
            {
                PV = ConnectionString.ToProtocolVersion(ConnectionStringKeys.Protocol);
            }
            else
            {
                PV = ProtocolVersion.Unknown;
            }

            _backendProtocolVersion = (PV == ProtocolVersion.Unknown) ? ProtocolVersion.Version3 : PV;

            // Reset state to initialize new connector in pool.
            Encoding = Encoding.Default;
            CurrentState = EDBClosedState.Instance;

            // Get a raw connection, possibly SSL...
            CurrentState.Open(this);
            // Establish protocol communication and handle authentication...
            CurrentState.Startup(this);

            // Check for protocol not supported.  If we have been told what protocol to use,
            // we will not try this step.
            if (_mediator.Errors.Count > 0 && PV == ProtocolVersion.Unknown)
            {
                // If we attempted protocol version 3, it may be possible to drop back to version 2.
                if (BackendProtocolVersion == ProtocolVersion.Version3)
                {
                    EDBError       Error0 = (EDBError)_mediator.Errors[0];

                    // If EDBError.ReadFromStream_Ver_3() encounters a version 2 error,
                    // it will set its own protocol version to version 2.  That way, we can tell
                    // easily if the error was a FATAL: protocol error.
                    if (Error0.BackendProtocolVersion == ProtocolVersion.Version2)
                    {
                        // Try using the 2.0 protocol.
                        _mediator.ResetResponses();
                        BackendProtocolVersion = ProtocolVersion.Version2;
                        CurrentState = EDBClosedState.Instance;

                        // Get a raw connection, possibly SSL...
                        CurrentState.Open(this);
                        // Establish protocol communication and handle authentication...
                        CurrentState.Startup(this);
                    }
                }
            }

            // Check for errors and do the Right Thing.
            // FIXME - CheckErrors needs to be moved to Connector
            CheckErrors();

            _backend_keydata = _mediator.BackendKeyData;

            // Change the state of connection to open and ready.
            _connection_state = ConnectionState.Open;
            CurrentState = EDBReadyState.Instance;

            String       ServerVersionString = String.Empty;

            // First try to determine backend server version using the newest method.
            if (((EDBParameterStatus)_mediator.Parameters["__npgsql_server_version"]) != null)
                ServerVersionString = ((EDBParameterStatus)_mediator.Parameters["__npgsql_server_version"]).ParameterValue;


            // Fall back to the old way, SELECT VERSION().
            // This should not happen for protocol version 3+.
            if (ServerVersionString.Length == 0)
            {
                EDBCommand command = new EDBCommand("select version();set DATESTYLE TO ISO;", this);
                ServerVersionString = PGUtil.ExtractServerVersion( (String)command.ExecuteScalar() );
            }

            // Cook version string so we can use it for enabling/disabling things based on
            // backend version.
            ServerVersion = PGUtil.ParseServerVersion(ServerVersionString);

            // Adjust client encoding.

            //EDBCommand commandEncoding1 = new EDBCommand("show client_encoding", _connector);
            //String clientEncoding1 = (String)commandEncoding1.ExecuteScalar();

            if (ConnectionString.ToString(ConnectionStringKeys.Encoding, ConnectionStringDefaults.Encoding).ToUpper() == "UNICODE")
            {
                Encoding = Encoding.UTF8;
                EDBCommand commandEncoding = new EDBCommand("SET CLIENT_ENCODING TO UNICODE", this);
                commandEncoding.ExecuteNonQuery();
            }

            // Make a shallow copy of the type mapping that the connector will own.
            // It is possible that the connector may add types to its private
            // mapping that will not be valid to another connector, even
            // if connected to the same backend version.
            _oidToNameMapping = EDBTypesHelper.CreateAndLoadInitialTypesMapping(this).Clone();

            ProcessServerVersion();

            // The connector is now fully initialized. Beyond this point, it is
            // safe to release it back to the pool rather than closing it.
            IsInitialized = true;
        }


        /// <summary>
        /// Closes the physical connection to the server.
        /// </summary>
        internal void Close()
        {
            try
            {
                this.CurrentState.Close(this);
            }
            catch {}
        }

        internal void CancelRequest()
        {
            
            EDBConnector CancelConnector = new EDBConnector(ConnectionString, false, false);
            
            CancelConnector._backend_keydata = BackEndKeyData;
            
            
            // Get a raw connection, possibly SSL...
            CancelConnector.CurrentState.Open(CancelConnector);
            
            // Cancel current request.
            CancelConnector.CurrentState.CancelRequest(CancelConnector);
            
            
        }


    ///<summary>
    /// Returns next portal index.
    ///</summary>
    internal String NextPortalName()
        {
            return _portalNamePrefix + System.Threading.Interlocked.Increment(ref _portalIndex);
        }


        ///<summary>
        /// Returns next plan index.
        ///</summary>
        internal String NextPlanName()
        {

            return _planNamePrefix + System.Threading.Interlocked.Increment(ref _planIndex);
        }

		
		/// <summary>
		/// EDB Team ::Zahid K Dated: 28th OCT 05 for callable statement
		/// Report whether the current connection can support Callable statement functionality.
		/// </summary>
		internal Boolean SupportsCallable 
		{
			get
			{
				return _supportCallable;
			}
			set
			{
				_supportCallable = value;
			}
		}

       
        internal void RemoveNotificationThread()
        {
            // Wait notification thread finish its work.
            _notificationAutoResetEvent.WaitOne();
            
            // Kill notification thread.
            _notificationThread.Abort();
            _notificationThread = null;
            
            // Special case in order to not get problems with thread synchronization.
            // It will be turned to 0 when synch thread is created.
            _notificationThreadStopCount = 1;  
            
        }
        
        internal void AddNotificationThread()
        {
            
            _notificationThreadStopCount = 0;
            _notificationAutoResetEvent.Set();
            
            EDBContextHolder contextHolder = new EDBContextHolder(this, CurrentState);
            
            _notificationThread = new Thread(new ThreadStart(contextHolder.ProcessServerMessages));
            
            _notificationThread.Start();
            
            
            
        }
        
        internal void StopNotificationThread()
        {
            
            _notificationThreadStopCount++;
            
            if (_notificationThreadStopCount == 1) // If this call was the first to increment.
            {
                
                _notificationAutoResetEvent.WaitOne();
                
            }
        }
        
        internal void ResumeNotificationThread()
        {
            _notificationThreadStopCount--;
            if (_notificationThreadStopCount == 0)
            {
                // Release the synchronization handle.
                
                _notificationAutoResetEvent.Set();
            }
            
        }
      internal Boolean IsNotificationThreadRunning
        {
            get
            {
                return _notificationThreadStopCount <= 0;
                
            }
        }
        
		 internal class EDBContextHolder
        {
        
            private EDBConnector connector;
            private EDBState     state;
        
            internal EDBContextHolder(EDBConnector connector, EDBState state)
            {
                this.connector = connector;
                this.state = state;
            
            }
        
            internal void ProcessServerMessages()
            {
                
                while(true)
                {
                    this.connector._notificationAutoResetEvent.WaitOne();
                    
                    if (this.connector.Socket.Poll(1000, SelectMode.SelectRead))
                    {
                        // reset any responses just before getting new ones
                        this.connector.Mediator.ResetResponses();
                        this.state.ProcessBackendResponses(this.connector);
                        this.connector.CheckErrorsAndNotifications();
                    }
               
                    this.connector._notificationAutoResetEvent.Set();
                }
    
                
                
            }
        
        }

		

    }
}
