#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The  EnterpriseDB.EDBClient Development Team
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
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
#if NET45 || NET451
using System.DirectoryServices;
using System.Security.Principal;
#endif

namespace  EnterpriseDB.EDBClient
{
    /// <summary>
    /// Provides a simple way to create and manage the contents of connection strings used by
    /// the <see cref="EDBConnection"/> class.
    /// </summary>
    public sealed class EDBConnectionStringBuilder : DbConnectionStringBuilder
    {
        #region Fields

        /// <summary>
        /// Makes all valid keywords for a property to that property (e.g. User Name -> Username, UserId -> Username...)
        /// </summary>
        static readonly Dictionary<string, PropertyInfo> PropertiesByKeyword;

        /// <summary>
        /// Maps CLR property names (e.g. BufferSize) to their canonical keyword name, which is the
        /// property's [DisplayName] (e.g. Buffer Size)
        /// </summary>
        static readonly Dictionary<string, string> PropertyNameToCanonicalKeyword;

        /// <summary>
        /// Maps each property to its [DefaultValue]
        /// </summary>
        static readonly Dictionary<PropertyInfo, object> PropertyDefaults;

        static readonly string[] Empty = new string[0];

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EDBConnectionStringBuilder class.
        /// </summary>
        public EDBConnectionStringBuilder() { Init(); }

#if NET45 || NET451
        /// <summary>
        /// Initializes a new instance of the EDBConnectionStringBuilder class, optionally using ODBC rules for quoting values.
        /// </summary>
        /// <param name="useOdbcRules">true to use {} to delimit fields; false to use quotation marks.</param>
        public EDBConnectionStringBuilder(bool useOdbcRules) : base(useOdbcRules) { Init(); }
#endif

        /// <summary>
        /// Initializes a new instance of the EDBConnectionStringBuilder class and sets its <see cref="DbConnectionStringBuilder.ConnectionString"/>.
        /// </summary>
        public EDBConnectionStringBuilder(string connectionString)
        {
            Init();
            ConnectionString = connectionString;
        }

        void Init()
        {
            // Set the strongly-typed properties to their default values
            foreach (var kv in PropertyDefaults)
                kv.Key.SetValue(this, kv.Value);
            // Setting the strongly-typed properties here also set the string-based properties in the base class.
            // Clear them (default settings = empty connection string)
            base.Clear();
        }

        #endregion

        #region Static initialization

        static EDBConnectionStringBuilder()
        {
            var properties = typeof(EDBConnectionStringBuilder)
                .GetProperties()
                .Where(p => p.GetCustomAttribute<EDBConnectionStringPropertyAttribute>() != null)
                .ToArray();

            Contract.Assume(properties.All(p => p.CanRead && p.CanWrite));
            Contract.Assume(properties.All(p => p.GetCustomAttribute<DisplayNameAttribute>() != null));

            PropertiesByKeyword = (
                from p in properties
                let displayName = p.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToUpperInvariant()
                let propertyName = p.Name.ToUpperInvariant()
                from k in new[] { displayName }
                  .Concat(propertyName != displayName ? new[] { propertyName } : Empty )
                  .Concat(p.GetCustomAttribute<EDBConnectionStringPropertyAttribute>().Aliases
                    .Select(a => a.ToUpperInvariant())
                  )
                  .Select(k => new { Property = p, Keyword = k })
                select k
            ).ToDictionary(t => t.Keyword, t => t.Property);

            PropertyNameToCanonicalKeyword = properties.ToDictionary(
                p => p.Name,
                p => p.GetCustomAttribute<DisplayNameAttribute>().DisplayName
            );

            PropertyDefaults = properties
                .Where(p => p.GetCustomAttribute<ObsoleteAttribute>() == null)
                .ToDictionary(
                p => p,
                p => p.GetCustomAttribute<DefaultValueAttribute>() != null
                    ? p.GetCustomAttribute<DefaultValueAttribute>().Value
                    : (p.PropertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(p.PropertyType) : null)
            );
        }

        #endregion

        #region Non-static property handling

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="keyword">The key of the item to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public override object this[string keyword]
        {
            get
            {
                object value;
                if (!TryGetValue(keyword, out value)) {
                    throw new ArgumentException("Keyword not supported: " + keyword, nameof(keyword));
                }
                return value;
            }
            set
            {
                if (value == null) {
                    Remove(keyword);
                    return;
                }

                var p = GetProperty(keyword);
                try {
                    object convertedValue;
                    if (p.PropertyType.GetTypeInfo().IsEnum && value is string) {
                        convertedValue = Enum.Parse(p.PropertyType, (string)value);
                    } else {
                        convertedValue = Convert.ChangeType(value, p.PropertyType);
                    }
                    p.SetValue(this, convertedValue);
                } catch (Exception e) {
                    throw new ArgumentException("Couldn't set " + keyword, keyword, e);
                }
            }
        }

        /// <summary>
        /// Removes the entry with the specified key from the DbConnectionStringBuilder instance.
        /// </summary>
        /// <param name="keyword">The key of the key/value pair to be removed from the connection string in this DbConnectionStringBuilder.</param>
        /// <returns><b>true</b> if the key existed within the connection string and was removed; <b>false</b> if the key did not exist.</returns>
        public override bool Remove(string keyword)
        {
            var p = GetProperty(keyword);
            string cannonicalName = PropertyNameToCanonicalKeyword[p.Name];
            var removed = base.ContainsKey(cannonicalName);
            // Note that string property setters call SetValue, which itself calls base.Remove().
            p.SetValue(this, PropertyDefaults[p]);
            base.Remove(cannonicalName);
            return removed;
        }

        /// <summary>
        /// Clears the contents of the <see cref="EDBConnectionStringBuilder"/> instance.
        /// </summary>
        public override void Clear()
        {
            Contract.Assert(Keys != null);
            foreach (var k in Keys.Cast<string>().ToArray()) {
                Remove(k);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="EDBConnectionStringBuilder"/> contains a specific key.
        /// </summary>
        /// <param name="keyword">The key to locate in the <see cref="EDBConnectionStringBuilder"/>.</param>
        /// <returns><b>true</b> if the <see cref="EDBConnectionStringBuilder"/> contains an entry with the specified key; otherwise <b>false</b>.</returns>
        public override bool ContainsKey(string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException(nameof(keyword));
            Contract.EndContractBlock();

            return PropertiesByKeyword.ContainsKey(keyword.ToUpperInvariant());
        }

        PropertyInfo GetProperty(string keyword)
        {
            PropertyInfo p;
            if (!PropertiesByKeyword.TryGetValue(keyword.ToUpperInvariant(), out p)) {
                throw new ArgumentException("Keyword not supported: " + keyword, nameof(keyword));
            }
            return p;
        }

        /// <summary>
        /// Retrieves a value corresponding to the supplied key from this <see cref="EDBConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="keyword">The key of the item to retrieve.</param>
        /// <param name="value">The value corresponding to the key.</param>
        /// <returns><b>true</b> if keyword was found within the connection string, <b>false</b> otherwise.</returns>
        public override bool TryGetValue(string keyword, [CanBeNull] out object value)
        {
            if (keyword == null)
                throw new ArgumentNullException(nameof(keyword));
            Contract.EndContractBlock();

            PropertyInfo p;
            if (!PropertiesByKeyword.TryGetValue(keyword.ToUpperInvariant(), out p))
            {
                value = null;
                return false;
            }

            value = GetProperty(keyword).GetValue(this) ?? "";
            return true;

        }

        void SetValue(string propertyName, [CanBeNull] object value)
        {
            var canonicalKeyword = PropertyNameToCanonicalKeyword[propertyName];
            if (value == null) {
                base.Remove(canonicalKeyword);
            } else {
                base[canonicalKeyword] = value;
            }

            lock (_connectionStringWithoutPasswordLocker)
            {
                _connectionStringWithoutPassword = null;
            }
        }

        #endregion

        #region Properties - Connection

        /// <summary>
        /// The hostname or IP address of the PostgreSQL server to connect to.
        /// </summary>
        [Category("Connection")]
        [Description("The hostname or IP address of the PostgreSQL server to connect to.")]
        [DisplayName("Host")]
        [EDBConnectionStringProperty("Server")]
        [CanBeNull]
        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                SetValue(nameof(Host), value);
            }
        }
        string _host;

        /// <summary>
        /// The TCP/IP port of the PostgreSQL server.
        /// </summary>
        [Category("Connection")]
        [Description("The TCP port of the PostgreSQL server.")]
        [DisplayName("Port")]
        [EDBConnectionStringProperty]
        [DefaultValue(EDBConnection.DefaultPort)]
        public int Port
        {
            get { return _port; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid port: " + value);
                Contract.EndContractBlock();

                _port = value;
                SetValue(nameof(Port), value);
            }
        }
        int _port;

        ///<summary>
        /// The PostgreSQL database to connect to.
        /// </summary>
        [Category("Connection")]
        [Description("The PostgreSQL database to connect to.")]
        [DisplayName("Database")]
        [EDBConnectionStringProperty("DB")]
        public string Database
        {
            get { return _database; }
            set
            {
                _database = value;
                SetValue(nameof(Database), value);
            }
        }
        string _database;

        /// <summary>
        /// The username to connect with. Not required if using IntegratedSecurity.
        /// </summary>
        [Category("Connection")]
        [Description("The username to connect with. Not required if using IntegratedSecurity.")]
        [DisplayName("Username")]
        [EDBConnectionStringProperty("User Name", "UserId", "User Id", "UID")]
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                SetValue(nameof(Username), value);
            }
        }
        string _username;

        /// <summary>
        /// The password to connect with. Not required if using IntegratedSecurity.
        /// </summary>
        [Category("Connection")]
        [Description("The password to connect with. Not required if using IntegratedSecurity.")]
        [PasswordPropertyText(true)]
        [DisplayName("Password")]
        [EDBConnectionStringProperty("PSW", "PWD")]
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                SetValue(nameof(Password), value);
            }
        }
        string _password;

        /// <summary>
        /// The optional application name parameter to be sent to the backend during connection initiation.
        /// </summary>
        [Category("Connection")]
        [Description("The optional application name parameter to be sent to the backend during connection initiation")]
        [DisplayName("Application Name")]
        [EDBConnectionStringProperty]
        public string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                _applicationName = value;
                SetValue(nameof(ApplicationName), value);
            }
        }
        string _applicationName;

        /// <summary>
        /// Whether to enlist in an ambient TransactionScope.
        /// </summary>
        [Category("Connection")]
        [Description("Whether to enlist in an ambient TransactionScope.")]
        [DisplayName("Enlist")]
        [EDBConnectionStringProperty]
        public bool Enlist
        {
            get { return _enlist; }
            set
            {
                _enlist = value;
                SetValue(nameof(Enlist), value);
            }
        }
        bool _enlist;

        /// <summary>
        /// Gets or sets the schema search path.
        /// </summary>
        [Category("Connection")]
        [Description("Gets or sets the schema search path.")]
        [DisplayName("Search Path")]
        [EDBConnectionStringProperty]
        public string SearchPath
        {
            get { return _searchpath; }
            set
            {
                _searchpath = value;
                SetValue(nameof(SearchPath), value);
            }
        }
        string _searchpath;

        /// <summary>
        /// Gets or sets the schema search path.
        /// </summary>
        [Category("Connection")]
        [Description("Gets or sets the client_encoding parameter.")]
        [DisplayName("Client Encoding")]
        [EDBConnectionStringProperty]
        public string ClientEncoding
        {
            get { return _clientEncoding; }
            set
            {
                _clientEncoding = value;
                SetValue(nameof(ClientEncoding), value);
            }
        }
        string _clientEncoding;

        /// <summary>
        /// Gets or sets the client_encoding parameter.
        /// </summary>
        [Category("Connection")]
        [Description("Gets or sets the .NET encoding that will be used to encode/decode PostgreSQL string data.")]
        [DisplayName("Encoding")]
        [DefaultValue("UTF8")]
        [EDBConnectionStringProperty]
        public string Encoding
        {
            get { return _encoding; }
            set
            {
                _encoding = value;
                SetValue(nameof(Encoding), value);
            }
        }
        string _encoding;

        #endregion

        #region Properties - Security

        /// <summary>
        /// Controls whether SSL is required, disabled or preferred, depending on server support.
        /// </summary>
        [Category("Security")]
        [Description("Controls whether SSL is required, disabled or preferred, depending on server support.")]
        [DisplayName("SSL Mode")]
        [EDBConnectionStringProperty]
        public SslMode SslMode
        {
            get { return _sslmode; }
            set
            {
                _sslmode = value;
                SetValue(nameof(SslMode), value);
            }
        }
        SslMode _sslmode;

        /// <summary>
        /// Whether to trust the server certificate without validating it.
        /// </summary>
        [Category("Security")]
        [Description("Whether to trust the server certificate without validating it.")]
        [DisplayName("Trust Server Certificate")]
        [EDBConnectionStringProperty]
        public bool TrustServerCertificate
        {
            get { return _trustServerCertificate; }
            set
            {
                _trustServerCertificate = value;
                SetValue(nameof(TrustServerCertificate), value);
            }
        }
        bool _trustServerCertificate;

        /// <summary>
        ///  EnterpriseDB.EDBClient uses its own internal implementation of TLS/SSL. Turn this on to use .NET SslStream instead.
        /// </summary>
        [Category("Security")]
        [Description(" EnterpriseDB.EDBClient uses its own internal implementation of TLS/SSL. Turn this on to use .NET SslStream instead.")]
        [DisplayName("Use SSL Stream")]
        [EDBConnectionStringProperty]
        public bool UseSslStream
        {
            get { return _useSslStream; }
            set
            {
                _useSslStream = value;
                SetValue(nameof(UseSslStream), value);
            }
        }
        bool _useSslStream;

        /// <summary>
        /// Whether to use Windows integrated security to log in.
        /// </summary>
        [Category("Security")]
        [Description("Whether to use Windows integrated security to log in.")]
        [DisplayName("Integrated Security")]
        [EDBConnectionStringProperty]
        public bool IntegratedSecurity
        {
            get { return _integratedSecurity; }
            set
            {
                // No integrated security if we're on mono and .NET 4.5 because of ClaimsIdentity,
                // see https://github.com/ EnterpriseDB.EDBClient/ EnterpriseDB.EDBClient/issues/133
                if (value && Type.GetType("Mono.Runtime") != null)
                    throw new NotSupportedException("IntegratedSecurity is currently unsupported on mono and .NET 4.5 (see https://github.com/ EnterpriseDB.EDBClient/ EnterpriseDB.EDBClient/issues/133)");
                _integratedSecurity = value;
                SetValue(nameof(IntegratedSecurity), value);
            }
        }
        bool _integratedSecurity;

        /// <summary>
        /// The Kerberos service name to be used for authentication.
        /// </summary>
        [Category("Security")]
        [Description("The Kerberos service name to be used for authentication.")]
        [DisplayName("Kerberos Service Name")]
        [EDBConnectionStringProperty("Krbsrvname")]
        [DefaultValue("postgres")]
        public string KerberosServiceName
        {
            get { return _kerberosServiceName; }
            set
            {
                _kerberosServiceName = value;
                SetValue(nameof(KerberosServiceName), value);
            }
        }
        string _kerberosServiceName;

        /// <summary>
        /// The Kerberos realm to be used for authentication.
        /// </summary>
        [Category("Security")]
        [Description("The Kerberos realm to be used for authentication.")]
        [DisplayName("Include Realm")]
        [EDBConnectionStringProperty]
        public bool IncludeRealm
        {
            get { return _includeRealm; }
            set
            {
                _includeRealm = value;
                SetValue(nameof(IncludeRealm), value);
            }
        }
        bool _includeRealm;

        /// <summary>
        /// Gets or sets a Boolean value that indicates if security-sensitive information, such as the password, is not returned as part of the connection if the connection is open or has ever been in an open state.
        /// </summary>
        [Category("Security")]
        [Description("Gets or sets a Boolean value that indicates if security-sensitive information, such as the password, is not returned as part of the connection if the connection is open or has ever been in an open state.")]
        [DisplayName("Persist Security Info")]
        [EDBConnectionStringProperty]
        public bool PersistSecurityInfo
        {
            get { return _persistSecurityInfo; }
            set
            {
                _persistSecurityInfo = value;
                SetValue(nameof(PersistSecurityInfo), value);
            }
        }
        bool _persistSecurityInfo;

        #endregion

        #region Properties - Pooling

        /// <summary>
        /// Whether connection pooling should be used.
        /// </summary>
        [Category("Pooling")]
        [Description("Whether connection pooling should be used.")]
        [DisplayName("Pooling")]
        [EDBConnectionStringProperty]
        [DefaultValue(true)]
        public bool Pooling
        {
            get { return _pooling; }
            set
            {
                _pooling = value;
                SetValue(nameof(Pooling), value);
            }
        }
        bool _pooling;

        /// <summary>
        /// The minimum connection pool size.
        /// </summary>
        [Category("Pooling")]
        [Description("The minimum connection pool size.")]
        [DisplayName("Minimum Pool Size")]
        [EDBConnectionStringProperty]
        [DefaultValue(1)]
        public int MinPoolSize
        {
            get { return _minPoolSize; }
            set
            {
                if (value < 0 || value > PoolManager.PoolSizeLimit)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "MinPoolSize must be between 0 and " + PoolManager.PoolSizeLimit);
                Contract.EndContractBlock();

                _minPoolSize = value;
                SetValue(nameof(MinPoolSize), value);
            }
        }
        int _minPoolSize;

        /// <summary>
        /// The maximum connection pool size.
        /// </summary>
        [Category("Pooling")]
        [Description("The maximum connection pool size.")]
        [DisplayName("Maximum Pool Size")]
        [EDBConnectionStringProperty]
        [DefaultValue(100)]
        public int MaxPoolSize
        {
            get { return _maxPoolSize; }
            set
            {
                if (value < 0 || value > PoolManager.PoolSizeLimit)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "MaxPoolSize must be between 0 and " + PoolManager.PoolSizeLimit);
                Contract.EndContractBlock();

                _maxPoolSize = value;
                SetValue(nameof(MaxPoolSize), value);
            }
        }
        int _maxPoolSize;

        /// <summary>
        /// The time to wait before closing idle connections in the pool if the count
        /// of all connections exceeds MinPoolSize.
        /// </summary>
        /// <value>The time (in seconds) to wait. The default value is 300.</value>
        [Category("Pooling")]
        [Description("The time to wait before closing unused connections in the pool if the count of all connections exeeds MinPoolSize.")]
        [DisplayName("Connection Idle Lifetime")]
        [EDBConnectionStringProperty]
        [DefaultValue(300)]
        public int ConnectionIdleLifetime
        {
            get { return _connectionIdleLifetime; }
            set
            {
                _connectionIdleLifetime = value;
                SetValue(nameof(ConnectionIdleLifetime), value);
            }
        }
        int _connectionIdleLifetime;

        /// <summary>
        /// How many seconds the pool waits before attempting to prune idle connections that are beyond
        /// idle lifetime (<see cref="ConnectionIdleLifetime"/>.
        /// </summary>
        /// <value>The interval (in seconds). The default value is 10.</value>
        [Category("Pooling")]
        [Description("How many seconds the pool waits before attempting to prune idle connections that are beyond idle lifetime.")]
        [DisplayName("Connection Pruning Interval")]
        [EDBConnectionStringProperty]
        [DefaultValue(10)]
        public int ConnectionPruningInterval
        {
            get { return _connectionPruningInterval; }
            set
            {
                _connectionPruningInterval = value;
                SetValue(nameof(ConnectionPruningInterval), value);
            }
        }
        int _connectionPruningInterval;

        #endregion

        #region Properties - Timeouts

        /// <summary>
        /// The time to wait (in seconds) while trying to establish a connection before terminating the attempt and generating an error.
        /// Defaults to 15 seconds.
        /// </summary>
        [Category("Timeouts")]
        [Description("The time to wait (in seconds) while trying to establish a connection before terminating the attempt and generating an error.")]
        [DisplayName("Timeout")]
        [EDBConnectionStringProperty]
        [DefaultValue(15)]
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value < 0 || value > EDBConnection.TimeoutLimit)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Timeout must be between 0 and " + EDBConnection.TimeoutLimit);
                Contract.EndContractBlock();

                _timeout = value;
                SetValue(nameof(Timeout), value);
            }
        }
        int _timeout;

        /// <summary>
        /// The time to wait (in seconds) while trying to execute a command before terminating the attempt and generating an error.
        /// Defaults to 30 seconds.
        /// </summary>
        [Category("Timeouts")]
        [Description("The time to wait (in seconds) while trying to execute a command before terminating the attempt and generating an error. Set to zero for infinity.")]
        [DisplayName("Command Timeout")]
        [EDBConnectionStringProperty]
        [DefaultValue(EDBCommand.DefaultTimeout)]
        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "CommandTimeout can't be negative");
                Contract.EndContractBlock();

                _commandTimeout = value;
                SetValue(nameof(CommandTimeout), value);
            }
        }
        int _commandTimeout;

        /// <summary>
        /// The time to wait (in seconds) while trying to execute a an internal command before terminating the attempt and generating an error.
        /// </summary>
        [Category("Timeouts")]
        [Description("The time to wait (in seconds) while trying to execute a an internal command before terminating the attempt and generating an error. -1 uses CommandTimeout, 0 means no timeout.")]
        [DisplayName("Internal Command Timeout")]
        [EDBConnectionStringProperty]
        [DefaultValue(-1)]
        public int InternalCommandTimeout
        {
            get { return _internalCommandTimeout; }
            set
            {
                if (value != 0 && value != -1 && value < EDBConnector.MinimumInternalCommandTimeout)
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"InternalCommandTimeout must be >= {EDBConnector.MinimumInternalCommandTimeout}, 0 (infinite) or -1 (use CommandTimeout)");
                Contract.EndContractBlock();

                _internalCommandTimeout = value;
                SetValue(nameof(InternalCommandTimeout), value);
            }
        }
        int _internalCommandTimeout;

        #endregion

        #region Properties - Entity Framework

        /// <summary>
        /// The database template to specify when creating a database in Entity Framework. If not specified,
        /// PostgreSQL defaults to "template1".
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/manage-ag-templatedbs.html
        /// </remarks>
        [Category("Entity Framework")]
        [Description("The database template to specify when creating a database in Entity Framework. If not specified, PostgreSQL defaults to \"template1\".")]
        [DisplayName("EF Template Database")]
        [EDBConnectionStringProperty]
        public string EntityTemplateDatabase
        {
            get { return _entityTemplateDatabase; }
            set
            {
                _entityTemplateDatabase = value;
                SetValue(nameof(EntityTemplateDatabase), value);
            }
        }
        string _entityTemplateDatabase;

        /// <summary>
        /// The database admin to specify when creating and dropping a database in Entity Framework. This is needed because
        ///  EnterpriseDB.EDBClient needs to connect to a database in order to send the create/drop database command.
        /// If not specified, defaults to "template1". Check EDBServices.UsingPostgresDBConnection for more information.
        /// </summary>
        [Category("Entity Framework")]
        [Description("The database admin to specify when creating and dropping a database in Entity Framework. If not specified, defaults to \"template1\".")]
        [DisplayName("EF Admin Database")]
        [EDBConnectionStringProperty]
        public string EntityAdminDatabase
        {
            get { return _entityAdminDatabase; }
            set
            {
                _entityAdminDatabase = value;
                SetValue(nameof(EntityAdminDatabase), value);
            }
        }
        string _entityAdminDatabase;

        #endregion

        #region Properties - Advanced

        /// <summary>
        /// The number of seconds of connection inactivity before  EnterpriseDB.EDBClient sends a keepalive query.
        /// Set to 0 (the default) to disable.
        /// </summary>
        [Category("Advanced")]
        [Description("The number of seconds of connection inactivity before  EnterpriseDB.EDBClient sends a keepalive query.")]
        [DisplayName("Keepalive")]
        [EDBConnectionStringProperty]
        public int KeepAlive
        {
            get { return _keepAlive; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "KeepAlive can't be negative");
                Contract.EndContractBlock();

                _keepAlive = value;
                SetValue(nameof(KeepAlive), value);
            }
        }
        int _keepAlive;

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        [Category("Advanced")]
        [Description("Determines the size of the internal buffer  EnterpriseDB.EDBClient uses when reading or writing. Increasing may improve performance if transferring large values from the database.")]
        [DisplayName("Buffer Size")]
        [EDBConnectionStringProperty]
        [DefaultValue(ReadBuffer.DefaultBufferSize)]
        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                _bufferSize = value;
                SetValue(nameof(BufferSize), value);
            }
        }
        int _bufferSize;

        #endregion

        #region Properties - Compatibility

        /// <summary>
        /// A compatibility mode for special PostgreSQL server types.
        /// </summary>
        [Category("Compatibility")]
        [Description("A compatibility mode for special PostgreSQL server types.")]
        [DisplayName("Server Compatibility Mode")]
        [EDBConnectionStringProperty]
        public ServerCompatibilityMode ServerCompatibilityMode
        {
            get { return _serverCompatibilityMode; }
            set
            {
                _serverCompatibilityMode = value;
                SetValue(nameof(ServerCompatibilityMode), value);
            }
        }
        ServerCompatibilityMode _serverCompatibilityMode;

        /// <summary>
        /// Makes MaxValue and MinValue timestamps and dates readable as infinity and negative infinity.
        /// </summary>
        [Category("Compatibility")]
        [Description("Makes MaxValue and MinValue timestamps and dates readable as infinity and negative infinity.")]
        [DisplayName("Convert Infinity DateTime")]
        [EDBConnectionStringProperty]
        public bool ConvertInfinityDateTime
        {
            get { return _convertInfinityDateTime; }
            set
            {
                _convertInfinityDateTime = value;
                SetValue(nameof(ConvertInfinityDateTime), value);
            }
        }
        bool _convertInfinityDateTime;

        #endregion

        #region Properties - Obsolete

        /// <summary>
        /// Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html
        /// </summary>
        [Category("Obsolete")]
        [Description("Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html")]
        [DisplayName("Connection Lifetime")]
        [EDBConnectionStringProperty]
        [Obsolete]
        public int ConnectionLifeTime
        {
            get { return 0; }
            set { throw new NotSupportedException("The ConnectionLifeTime parameter is no longer supported. Please see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html"); }
        }

        /// <summary>
        /// Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html
        /// </summary>
        [Category("Obsolete")]
        [Description("Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html")]
        [DisplayName("Continuous Processing")]
        [EDBConnectionStringProperty]
        [Obsolete]
        public bool ContinuousProcessing
        {
            get { return false; }
            set { throw new NotSupportedException("The ContinuousProcessing parameter is no longer supported. Please see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html"); }
        }

        /// <summary>
        /// Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html
        /// </summary>
        [Category("Obsolete")]
        [Description("Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html")]
        [DisplayName("Backend Timeouts")]
        [EDBConnectionStringProperty]
        [Obsolete]
        public bool BackendTimeouts
        {
            get { return false; }
            set { throw new NotSupportedException("The BackendTimeouts parameter is no longer supported. Please see http://www. EnterpriseDB.EDBClient.org/doc/3.1/migration.html"); }
        }

        /// <summary>
        /// Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.0/migration.html
        /// </summary>
        [Category("Obsolete")]
        [Description("Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.0/migration.html")]
        [DisplayName("Preload Reader")]
        [EDBConnectionStringProperty]
        [Obsolete]
        public bool PreloadReader
        {
            get { return false; }
            set { throw new NotSupportedException("The PreloadReader parameter is no longer supported. Please see http://www. EnterpriseDB.EDBClient.org/doc/3.0/migration.html"); }
        }

        /// <summary>
        /// Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.0/migration.html
        /// </summary>
        [Category("Obsolete")]
        [Description("Obsolete, see http://www. EnterpriseDB.EDBClient.org/doc/3.0/migration.html")]
        [DisplayName("Use Extended Types")]
        [EDBConnectionStringProperty]
        [Obsolete]
        public bool UseExtendedTypes
        {
            get { return false; }
            set { throw new NotSupportedException("The UseExtendedTypes parameter is no longer supported. Please see http://www. EnterpriseDB.EDBClient.org/doc/3.0/migration.html"); }
        }

        #endregion

        #region Misc

        internal string ToStringWithoutPassword()
        {
            lock (_connectionStringWithoutPasswordLocker)
            {
                if (_connectionStringWithoutPassword != null)
                {
                    return _connectionStringWithoutPassword;
                }

                var clone = Clone();
                clone.Password = null;
                _connectionStringWithoutPassword = clone.ToString();
                return _connectionStringWithoutPassword;
            }
        }

        readonly object _connectionStringWithoutPasswordLocker = new object();
        [CanBeNull]
        string _connectionStringWithoutPassword;

        internal EDBConnectionStringBuilder Clone() => new EDBConnectionStringBuilder(ConnectionString);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            var o = obj as EDBConnectionStringBuilder;
            return o != null && EquivalentTo(o);
        }

        /// <summary>
        /// Hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Host?.GetHashCode() ?? 0;

        #endregion

        #region Attributes

        [AttributeUsage(AttributeTargets.Property)]
        [MeansImplicitUse]
        class EDBConnectionStringPropertyAttribute : Attribute
        {
            internal string[] Aliases { get; }

            internal EDBConnectionStringPropertyAttribute()
            {
                Aliases = Empty;
            }

            internal EDBConnectionStringPropertyAttribute(params string[] aliases)
            {
                Aliases = aliases;
            }
        }

#if NETSTANDARD1_3
        [AttributeUsage(AttributeTargets.Property)]
        class DescriptionAttribute : Attribute
        {
            internal DescriptionAttribute(string description) { }
        }
#endif

        #endregion
    }

    #region Enums

    /// <summary>
    /// An option specified in the connection string that activates special compatibility features.
    /// </summary>
    [PublicAPI]
    public enum ServerCompatibilityMode
    {
        /// <summary>
        /// No special server compatibility mode is active
        /// </summary>
        None,
        /// <summary>
        /// The server is an Amazon Redshift instance.
        /// </summary>
        Redshift,
    }

    /// <summary>
    /// Specifies how to manage SSL.
    /// </summary>
    [PublicAPI]
    public enum SslMode
    {
        /// <summary>
        /// SSL is disabled. If the server requires SSL, the connection will fail.
        /// </summary>
        Disable,
        /// <summary>
        /// Prefer SSL connections if the server allows them, but allow connections without SSL.
        /// </summary>
        Prefer,
        /// <summary>
        /// Fail the connection if the server doesn't suppotr SSL.
        /// </summary>
        Require,
    }

    #endregion
}
