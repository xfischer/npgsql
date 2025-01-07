using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.ResolverFactories;
using EnterpriseDB.EDBClient.NameTranslation;
using EnterpriseDB.EDBClient.Properties;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides a simple API for configuring and creating an <see cref="EDBDataSource" />, from which database connections can be obtained.
/// </summary>
/// <remarks>
/// On this builder, various features are disabled by default; unless you're looking to save on code size (e.g. when publishing with
/// NativeAOT), use <see cref="EDBDataSourceBuilder" /> instead.
/// </remarks>
public sealed class EDBSlimDataSourceBuilder : IEDBTypeMapper
{
    static UnsupportedTypeInfoResolver<EDBSlimDataSourceBuilder> UnsupportedTypeInfoResolver { get; } = new();

    ILoggerFactory? _loggerFactory;
    bool _sensitiveDataLoggingEnabled;
    List<Action<EDBTracingOptionsBuilder>>? _tracingOptionsBuilderCallbacks;
    List<Action<EDBTypeLoadingOptionsBuilder>>? _typeLoadingOptionsBuilderCallbacks;

    TransportSecurityHandler _transportSecurityHandler = new();
    RemoteCertificateValidationCallback? _userCertificateValidationCallback;
    Action<X509CertificateCollection>? _clientCertificatesCallback;
#if NET7_0_OR_GREATER // EnterpriseDB
    Action<SslClientAuthenticationOptions>? _sslClientAuthenticationOptionsCallback;
#endif

#if NET7_0_OR_GREATER
    Action<NegotiateAuthenticationClientOptions>? _negotiateOptionsCallback;
#endif

    IntegratedSecurityHandler _integratedSecurityHandler = new();

    Func<EDBConnectionStringBuilder, string>? _passwordProvider;
    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? _passwordProviderAsync;

    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? _periodicPasswordProvider;
    TimeSpan _periodicPasswordSuccessRefreshInterval, _periodicPasswordFailureRefreshInterval;

    PgTypeInfoResolverChainBuilder _resolverChainBuilder = new(); // mutable struct, don't make readonly.

    readonly UserTypeMapper _userTypeMapper;

    Action<EDBConnection>? _connectionInitializer;
    Func<EDBConnection, Task>? _connectionInitializerAsync;

    internal JsonSerializerOptions? JsonSerializerOptions { get; private set; }

    internal Action<EDBSlimDataSourceBuilder> ConfigureDefaultFactories { get; set; }

    /// <summary>
    /// A connection string builder that can be used to configured the connection string on the builder.
    /// </summary>
    public EDBConnectionStringBuilder ConnectionStringBuilder { get; }

    /// <summary>
    /// Returns the connection string, as currently configured on the builder.
    /// </summary>
    public string ConnectionString => ConnectionStringBuilder.ToString();

    static EDBSlimDataSourceBuilder()
        => GlobalTypeMapper.Instance.AddGlobalTypeMappingResolvers([new AdoTypeInfoResolverFactory()]);

    /// <summary>
    /// A diagnostics name used by EnterpriseDB.EDBClient when generating tracing, logging and metrics.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Constructs a new <see cref="EDBSlimDataSourceBuilder" />, optionally starting out from the given
    /// <paramref name="connectionString"/>.
    /// </summary>
    public EDBSlimDataSourceBuilder(string? connectionString = null)
        : this(new EDBConnectionStringBuilder(connectionString))
    { }

    internal EDBSlimDataSourceBuilder(EDBConnectionStringBuilder connectionStringBuilder)
    {
        ConnectionStringBuilder = connectionStringBuilder;
        _userTypeMapper = new() { DefaultNameTranslator = GlobalTypeMapper.Instance.DefaultNameTranslator };
        ConfigureDefaultFactories = static instance => instance.AppendDefaultFactories();
        ConfigureResolverChain = static chain => chain.Add(UnsupportedTypeInfoResolver);
    }

    /// <summary>
    /// Sets the <see cref="ILoggerFactory" /> that will be used for logging.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UseLoggerFactory(ILoggerFactory? loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    /// <summary>
    /// Enables parameters to be included in logging. This includes potentially sensitive information from data sent to PostgreSQL.
    /// You should only enable this flag in development, or if you have the appropriate security measures in place based on the
    /// sensitivity of this data.
    /// </summary>
    /// <param name="parameterLoggingEnabled">If <see langword="true" />, then sensitive data is logged.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableParameterLogging(bool parameterLoggingEnabled = true)
    {
        _sensitiveDataLoggingEnabled = parameterLoggingEnabled;
        return this;
    }

    /// <summary>
    /// Configure type loading options for the DataSource. Calling this again will replace
    /// the prior action.
    /// </summary>
    public EDBSlimDataSourceBuilder ConfigureTypeLoading(Action<EDBTypeLoadingOptionsBuilder> configureAction)
    {
#if NET6_0_OR_GREATER // EnterpriseDB
        ArgumentNullException.ThrowIfNull(configureAction);
#else
        if (configureAction is null)
            throw new ArgumentNullException(nameof(configureAction));
#endif

        _typeLoadingOptionsBuilderCallbacks ??= new();
        _typeLoadingOptionsBuilderCallbacks.Add(configureAction);
        return this;
    }

    /// <summary>
    /// Configures OpenTelemetry tracing options.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder ConfigureTracing(Action<EDBTracingOptionsBuilder> configureAction)
    {
#if NET6_0_OR_GREATER // EnterpriseDB
        ArgumentNullException.ThrowIfNull(configureAction);
#else
        if (configureAction is null)
            throw new ArgumentNullException(nameof(configureAction));
#endif

        _tracingOptionsBuilderCallbacks ??= new();
        _tracingOptionsBuilderCallbacks.Add(configureAction);
        return this;
    }

    /// <summary>
    /// Configures the JSON serializer options used when reading and writing all System.Text.Json data.
    /// </summary>
    /// <param name="serializerOptions">Options to customize JSON serialization and deserialization.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder ConfigureJsonOptions(JsonSerializerOptions serializerOptions)
    {
#if NET6_0_OR_GREATER // EnterpriseDB
        ArgumentNullException.ThrowIfNull(serializerOptions);
#else
        if (serializerOptions is null)
            throw new ArgumentNullException(nameof(serializerOptions));
#endif

        JsonSerializerOptions = serializerOptions;
        return this;
    }

    #region Authentication

    /// <summary>
    /// When using SSL/TLS, this is a callback that allows customizing how the PostgreSQL-provided certificate is verified. This is an
    /// advanced API, consider using <see cref="SslMode.VerifyFull" /> or <see cref="SslMode.VerifyCA" /> instead.
    /// </summary>
    /// <param name="userCertificateValidationCallback">The callback containing custom callback verification logic.</param>
    /// <remarks>
    /// <para>
    /// Cannot be used in conjunction with <see cref="SslMode.Disable" />, <see cref="SslMode.VerifyCA" /> or
    /// <see cref="SslMode.VerifyFull" />.
    /// </para>
    /// <para>
    /// See <see href="https://msdn.microsoft.com/en-us/library/system.net.security.remotecertificatevalidationcallback(v=vs.110).aspx"/>.
    /// </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
#if NET7_0_OR_GREATER // EnterpriseDB
    [Obsolete("Use UseSslClientAuthenticationOptionsCallback")]
#endif
    public EDBSlimDataSourceBuilder UseUserCertificateValidationCallback(
        RemoteCertificateValidationCallback userCertificateValidationCallback)
    {
        _userCertificateValidationCallback = userCertificateValidationCallback;

        return this;
    }

    /// <summary>
    /// Specifies an SSL/TLS certificate which EnterpriseDB.EDBClient will send to PostgreSQL for certificate-based authentication.
    /// </summary>
    /// <param name="clientCertificate">The client certificate to be sent to PostgreSQL when opening a connection.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
#if NET7_0_OR_GREATER // EnterpriseDB
    [Obsolete("Use UseSslClientAuthenticationOptionsCallback")]
#endif
    public EDBSlimDataSourceBuilder UseClientCertificate(X509Certificate? clientCertificate)
    {
        if (clientCertificate is null)
            return UseClientCertificatesCallback(null);

        var clientCertificates = new X509CertificateCollection { clientCertificate };
        return UseClientCertificates(clientCertificates);
    }

    /// <summary>
    /// Specifies a collection of SSL/TLS certificates which EnterpriseDB.EDBClient will send to PostgreSQL for certificate-based authentication.
    /// </summary>
    /// <param name="clientCertificates">The client certificate collection to be sent to PostgreSQL when opening a connection.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
#if NET7_0_OR_GREATER // EnterpriseDB
    [Obsolete("Use UseSslClientAuthenticationOptionsCallback")]
#endif
    public EDBSlimDataSourceBuilder UseClientCertificates(X509CertificateCollection? clientCertificates)
        => UseClientCertificatesCallback(clientCertificates is null ? null : certs => certs.AddRange(clientCertificates));


#if NET7_0_OR_GREATER
    /// <summary>
    /// When using SSL/TLS, this is a callback that allows customizing SslStream's authentication options.
    /// </summary>
    /// <param name="sslClientAuthenticationOptionsCallback">The callback to customize SslStream's authentication options.</param>
    /// <remarks>
    /// <para>
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslclientauthenticationoptions?view=net-8.0"/>.
    /// </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UseSslClientAuthenticationOptionsCallback(Action<SslClientAuthenticationOptions>? sslClientAuthenticationOptionsCallback)
    {
        _sslClientAuthenticationOptionsCallback = sslClientAuthenticationOptionsCallback;

        return this;
    }
#endif

    /// <summary>
    /// Specifies a callback to modify the collection of SSL/TLS client certificates which EDB .NET Connector will send to PostgreSQL for
    /// certificate-based authentication. This is an advanced API, consider using <see cref="UseClientCertificate" /> or
    /// <see cref="UseClientCertificates" /> instead.
    /// </summary>
    /// <param name="clientCertificatesCallback">The callback to modify the client certificate collection.</param>
    /// <remarks>
    /// <para>
    /// The callback is invoked every time a physical connection is opened, and is therefore suitable for rotating short-lived client
    /// certificates. Simply make sure the certificate collection argument has the up-to-date certificate(s).
    /// </para>
    /// <para>
    /// The callback's collection argument already includes any client certificates specified via the connection string or environment
    /// variables.
    /// </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
#if NET7_0_OR_GREATER // EnterpriseDB
    [Obsolete("Use UseSslClientAuthenticationOptionsCallback")]
#endif
    public EDBSlimDataSourceBuilder UseClientCertificatesCallback(Action<X509CertificateCollection>? clientCertificatesCallback)
    {
        _clientCertificatesCallback = clientCertificatesCallback;

        return this;
    }

    /// <summary>
    /// Sets the <see cref="X509Certificate2" /> that will be used validate SSL certificate, received from the server.
    /// </summary>
    /// <param name="rootCertificate">The CA certificate.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UseRootCertificate(X509Certificate2? rootCertificate)
        => rootCertificate is null
            ? UseRootCertificateCallback(null)
            : UseRootCertificateCallback(() => rootCertificate);

    /// <summary>
    /// Specifies a callback that will be used to validate SSL certificate, received from the server.
    /// </summary>
    /// <param name="rootCertificateCallback">The callback to get CA certificate.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    /// <remarks>
    /// This overload, which accepts a callback, is suitable for scenarios where the certificate rotates
    /// and might change during the lifetime of the application.
    /// When that's not the case, use the overload which directly accepts the certificate.
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UseRootCertificateCallback(Func<X509Certificate2>? rootCertificateCallback)
    {
        _transportSecurityHandler.RootCertificateCallback = rootCertificateCallback;

        return this;
    }

    /// <summary>
    /// Configures a periodic password provider, which is automatically called by the data source at some regular interval. This is the
    /// recommended way to fetch a rotating access token.
    /// </summary>
    /// <param name="passwordProvider">A callback which returns the password to be sent to PostgreSQL.</param>
    /// <param name="successRefreshInterval">How long to cache the password before re-invoking the callback.</param>
    /// <param name="failureRefreshInterval">
    /// If a password refresh attempt fails, it will be re-attempted with this interval.
    /// This should typically be much lower than <paramref name="successRefreshInterval" />.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// The provided callback is invoked in a timer, and not when opening connections. It therefore doesn't affect opening time.
    /// </para>
    /// <para>
    /// The provided cancellation token is only triggered when the entire data source is disposed. If you'd like to apply a timeout to the
    /// token fetching, do so within the provided callback.
    /// </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UsePeriodicPasswordProvider(
        Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? passwordProvider,
        TimeSpan successRefreshInterval,
        TimeSpan failureRefreshInterval)
    {
        if (successRefreshInterval < TimeSpan.Zero)
            throw new ArgumentException(
                string.Format(EDBStrings.ArgumentMustBePositive, nameof(successRefreshInterval)), nameof(successRefreshInterval));
        if (failureRefreshInterval < TimeSpan.Zero)
            throw new ArgumentException(
                string.Format(EDBStrings.ArgumentMustBePositive, nameof(failureRefreshInterval)), nameof(failureRefreshInterval));

        _periodicPasswordProvider = passwordProvider;
        _periodicPasswordSuccessRefreshInterval = successRefreshInterval;
        _periodicPasswordFailureRefreshInterval = failureRefreshInterval;

        return this;
    }

    /// <summary>
    /// Configures a password provider, which is called by the data source when opening connections.
    /// </summary>
    /// <param name="passwordProvider">
    /// A callback that may be invoked during <see cref="EDBConnection.Open()" /> which returns the password to be sent to PostgreSQL.
    /// </param>
    /// <param name="passwordProviderAsync">
    /// A callback that may be invoked during <see cref="EDBConnection.OpenAsync(CancellationToken)" /> which returns the password to be sent to PostgreSQL.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// The provided callback is invoked when opening connections. Therefore its important the callback internally depends on cached
    /// data or returns quickly otherwise. Any unnecessary delay will affect connection opening time.
    /// </para>
    /// </remarks>
    public EDBSlimDataSourceBuilder UsePasswordProvider(
        Func<EDBConnectionStringBuilder, string>? passwordProvider,
        Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? passwordProviderAsync)
    {
        if (passwordProvider is null != passwordProviderAsync is null)
            throw new ArgumentException(EDBStrings.SyncAndAsyncPasswordProvidersRequired);

        _passwordProvider = passwordProvider;
        _passwordProviderAsync = passwordProviderAsync;
        return this;
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// When using Kerberos, this is a callback that allows customizing default settings for Kerberos authentication.
    /// </summary>
    /// <param name="negotiateOptionsCallback">The callback containing logic to customize Kerberos authentication settings.</param>
    /// <remarks>
    /// <para>
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/api/system.net.security.negotiateauthenticationclientoptions?view=net-7.0"/>.
    /// </para>
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UseNegotiateOptionsCallback(Action<NegotiateAuthenticationClientOptions>? negotiateOptionsCallback)
    {
        _negotiateOptionsCallback = negotiateOptionsCallback;

        return this;
    }
#endif

    #endregion Authentication

    #region Type mapping

    /// <inheritdoc />
    public IEDBNameTranslator DefaultNameTranslator
    {
        get => _userTypeMapper.DefaultNameTranslator;
        set => _userTypeMapper.DefaultNameTranslator = value;
    }

    /// <summary>
    /// Maps a CLR enum to a PostgreSQL enum type.
    /// </summary>
    /// <remarks>
    /// CLR enum labels are mapped by name to PostgreSQL enum labels.
    /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
    /// which defaults to <see cref="EDBSnakeCaseNameTranslator"/>.
    /// You can also use the <see cref="PgNameAttribute"/> on your enum fields to manually specify a PostgreSQL enum label.
    /// If there is a discrepancy between the .NET and database labels while an enum is read or written,
    /// an exception will be raised.
    /// </remarks>
    /// <param name="pgName">
    /// A PostgreSQL type name for the corresponding enum type in the database.
    /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
    /// </param>
    /// <param name="nameTranslator">
    /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
    /// Defaults to <see cref="DefaultNameTranslator" />.
    /// </param>
    /// <typeparam name="TEnum">The .NET enum type to be mapped</typeparam>
    public EDBSlimDataSourceBuilder MapEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum
    {
        _userTypeMapper.MapEnum<TEnum>(pgName, nameTranslator);
        return this;
    }

    /// <inheritdoc />
    public bool UnmapEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum
        => _userTypeMapper.UnmapEnum<TEnum>(pgName, nameTranslator);

    /// <summary>
    /// Maps a CLR enum to a PostgreSQL enum type.
    /// </summary>
    /// <remarks>
    /// CLR enum labels are mapped by name to PostgreSQL enum labels.
    /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
    /// which defaults to <see cref="EDBSnakeCaseNameTranslator"/>.
    /// You can also use the <see cref="PgNameAttribute"/> on your enum fields to manually specify a PostgreSQL enum label.
    /// If there is a discrepancy between the .NET and database labels while an enum is read or written,
    /// an exception will be raised.
    /// </remarks>
    /// <param name="clrType">The .NET enum type to be mapped</param>
    /// <param name="pgName">
    /// A PostgreSQL type name for the corresponding enum type in the database.
    /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
    /// </param>
    /// <param name="nameTranslator">
    /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
    /// Defaults to <see cref="DefaultNameTranslator" />.
    /// </param>
    [RequiresDynamicCode("Calling MapEnum with a Type can require creating new generic types or methods. This may not work when AOT compiling.")]
    public EDBSlimDataSourceBuilder MapEnum([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
    {
        _userTypeMapper.MapEnum(clrType, pgName, nameTranslator);
        return this;
    }

    /// <inheritdoc />
    public bool UnmapEnum([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        => _userTypeMapper.UnmapEnum(clrType, pgName, nameTranslator);

    /// <summary>
    /// Maps a CLR type to a PostgreSQL composite type.
    /// </summary>
    /// <remarks>
    /// CLR fields and properties by string to PostgreSQL names.
    /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
    /// which defaults to <see cref="EDBSnakeCaseNameTranslator"/>.
    /// You can also use the <see cref="PgNameAttribute"/> on your members to manually specify a PostgreSQL name.
    /// If there is a discrepancy between the .NET type and database type while a composite is read or written,
    /// an exception will be raised.
    /// </remarks>
    /// <param name="pgName">
    /// A PostgreSQL type name for the corresponding composite type in the database.
    /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
    /// </param>
    /// <param name="nameTranslator">
    /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
    /// Defaults to <see cref="DefaultNameTranslator" />.
    /// </param>
    /// <typeparam name="T">The .NET type to be mapped</typeparam>
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public EDBSlimDataSourceBuilder MapComposite<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string? pgName = null, IEDBNameTranslator? nameTranslator = null)
    {
        _userTypeMapper.MapComposite(typeof(T), pgName, nameTranslator);
        return this;
    }

    /// <inheritdoc />
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public bool UnmapComposite<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        => _userTypeMapper.UnmapComposite(typeof(T), pgName, nameTranslator);

    /// <summary>
    /// Maps a CLR type to a composite type.
    /// </summary>
    /// <remarks>
    /// Maps CLR fields and properties by string to PostgreSQL names.
    /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
    /// which defaults to <see cref="DefaultNameTranslator" />.
    /// If there is a discrepancy between the .NET type and database type while a composite is read or written,
    /// an exception will be raised.
    /// </remarks>
    /// <param name="clrType">The .NET type to be mapped.</param>
    /// <param name="pgName">
    /// A PostgreSQL type name for the corresponding composite type in the database.
    /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
    /// </param>
    /// <param name="nameTranslator">
    /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
    /// Defaults to <see cref="DefaultNameTranslator" />.
    /// </param>
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public EDBSlimDataSourceBuilder MapComposite([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)]
        Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
    {
        _userTypeMapper.MapComposite(clrType, pgName, nameTranslator);
        return this;
    }

    /// <inheritdoc />
    [RequiresDynamicCode("Mapping composite types involves serializing arbitrary types which can require creating new generic types or methods. This is currently unsupported with NativeAOT, vote on issue #5303 if this is important to you.")]
    public bool UnmapComposite([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)]
        Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        => _userTypeMapper.UnmapComposite(clrType, pgName, nameTranslator);


    /// <inheritdoc />
    public void AddTypeInfoResolverFactory(PgTypeInfoResolverFactory factory) => _resolverChainBuilder.PrependResolverFactory(factory);

    /// <inheritdoc />
    void IEDBTypeMapper.Reset() => _resolverChainBuilder.Clear();

    internal Action<List<IPgTypeInfoResolver>> ConfigureResolverChain { get; set; }
    internal void AppendResolverFactory(PgTypeInfoResolverFactory factory)
        => _resolverChainBuilder.AppendResolverFactory(factory);
    internal void AppendResolverFactory<T>(Func<T> factory) where T : PgTypeInfoResolverFactory
        => _resolverChainBuilder.AppendResolverFactory(factory);

    internal void AppendDefaultFactories()
    {
        // When used publicly we start off with our slim defaults.
        _resolverChainBuilder.AppendResolverFactory(_userTypeMapper);
        if (GlobalTypeMapper.Instance.GetUserMappingsResolverFactory() is { } userMappingsResolverFactory)
            _resolverChainBuilder.AppendResolverFactory(userMappingsResolverFactory);
        foreach (var factory in GlobalTypeMapper.Instance.GetPluginResolverFactories())
            _resolverChainBuilder.AppendResolverFactory(factory);
        _resolverChainBuilder.AppendResolverFactory(new AdoTypeInfoResolverFactory());
    }

    #endregion Type mapping

    #region Optional opt-ins

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>array</c> types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableArrays()
    {
        _resolverChainBuilder.EnableArrays();
        return this;
    }

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>range</c> types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableRanges()
    {
        _resolverChainBuilder.EnableRanges();
        return this;
    }

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>multirange</c> types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableMultiranges()
    {
        _resolverChainBuilder.EnableMultiranges();
        return this;
    }

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>record</c> type as a .NET <c>object[]</c>.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableRecords()
    {
        AddTypeInfoResolverFactory(new RecordTypeInfoResolverFactory());
        return this;
    }

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>tsquery</c> and <c>tsvector</c> types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableFullTextSearch()
    {
        AddTypeInfoResolverFactory(new FullTextSearchTypeInfoResolverFactory());
        return this;
    }

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>ltree</c> extension types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableLTree()
    {
        AddTypeInfoResolverFactory(new LTreeTypeInfoResolverFactory());
        return this;
    }

    /// <summary>
    /// Sets up mappings for extra conversions from PostgreSQL to .NET types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableExtraConversions()
    {
        AddTypeInfoResolverFactory(new ExtraConversionResolverFactory());
        return this;
    }

    /// <summary>
    /// Enables the possibility to use TLS/SSl encryption for connections to PostgreSQL. This does not guarantee that encryption will
    /// actually be used; see <see href="https://www.npgsql.org/doc/security.html"/> for more details.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableTransportSecurity()
    {
        _transportSecurityHandler = new RealTransportSecurityHandler();
        return this;
    }

    /// <summary>
    /// Enables the possibility to use GSS/SSPI authentication for connections to PostgreSQL. This does not guarantee that it will
    /// actually be used; see <see href="https://www.npgsql.org/doc/security.html"/> for more details.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableIntegratedSecurity()
    {
        _integratedSecurityHandler = new RealIntegratedSecurityHandler();
        return this;
    }

    /// <summary>
    /// Sets up network mappings. This allows mapping PhysicalAddress, IPAddress, EDBInet and EDBCidr types
    /// to PostgreSQL <c>macaddr</c>, <c>macaddr8</c>, <c>inet</c> and <c>cidr</c> types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableNetworkTypes()
    {
        _resolverChainBuilder.AppendResolverFactory(new NetworkTypeInfoResolverFactory());
        return this;
    }

    /// <summary>
    /// Sets up network mappings. This allows mapping types like EDBPoint and EDBPath
    /// to PostgreSQL <c>point</c>, <c>path</c> and so on types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableGeometricTypes()
    {
        _resolverChainBuilder.AppendResolverFactory(new GeometricTypeInfoResolverFactory());
        return this;
    }

    /// <summary>
    /// Sets up System.Text.Json mappings. This allows mapping JsonDocument and JsonElement types to PostgreSQL <c>json</c> and <c>jsonb</c>
    /// types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder EnableJsonTypes()
    {
        _resolverChainBuilder.AppendResolverFactory(() => new JsonTypeInfoResolverFactory(JsonSerializerOptions));
        return this;
    }

    /// <summary>
    /// Sets up dynamic System.Text.Json mappings. This allows mapping arbitrary .NET types to PostgreSQL <c>json</c> and <c>jsonb</c>
    /// types, as well as <see cref="JsonNode" /> and its derived types.
    /// </summary>
    /// <param name="jsonbClrTypes">
    /// A list of CLR types to map to PostgreSQL <c>jsonb</c> (no need to specify <see cref="EDBDbType.Jsonb" />).
    /// </param>
    /// <param name="jsonClrTypes">
    /// A list of CLR types to map to PostgreSQL <c>json</c> (no need to specify <see cref="EDBDbType.Json" />).
    /// </param>
    /// <remarks>
    /// Due to the dynamic nature of these mappings, they are not compatible with NativeAOT or trimming.
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [RequiresUnreferencedCode("Json serializer may perform reflection on trimmed types.")]
    [RequiresDynamicCode("Serializing arbitrary types to json can require creating new generic types or methods, which requires creating code at runtime. This may not work when AOT compiling.")]
    public EDBSlimDataSourceBuilder EnableDynamicJson(
        Type[]? jsonbClrTypes = null,
        Type[]? jsonClrTypes = null)
    {
        _resolverChainBuilder.AppendResolverFactory(() => new JsonDynamicTypeInfoResolverFactory(jsonbClrTypes, jsonClrTypes, JsonSerializerOptions));
        return this;
    }

    // EnterpriseDB : remove optins (see EC-3060)
    internal EDBSlimDataSourceBuilder DisableDynamicJson()
    {
        _resolverChainBuilder.RemoveResolverFactory(typeof(JsonDynamicTypeInfoResolverFactory));
        return this;
    }

    // EnterpriseDB : remove optins (see EC-3060)
    internal EDBSlimDataSourceBuilder DisableRecordsAsTuples()
    {
        _resolverChainBuilder.RemoveResolverFactory(typeof(TupledRecordTypeInfoResolverFactory));
        return this;
    }


    // EnterpriseDB : remove optins (see EC-3060)
    internal EDBSlimDataSourceBuilder DisableUnmappedTypes()
    {
        _resolverChainBuilder.RemoveResolverFactory(typeof(UnmappedTypeInfoResolverFactory));
        return this;
    }

    /// <summary>
    /// Sets up mappings for the PostgreSQL <c>record</c> type as a .NET <see cref="ValueTuple" /> or <see cref="Tuple" />.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [RequiresUnreferencedCode("The mapping of PostgreSQL records as .NET tuples requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode("The mapping of PostgreSQL records as .NET tuples requires dynamic code usage which is incompatible with NativeAOT.")]
    public EDBSlimDataSourceBuilder EnableRecordsAsTuples()
    {
        AddTypeInfoResolverFactory(new TupledRecordTypeInfoResolverFactory());
        return this;
    }

    /// <summary>
    /// Sets up mappings allowing the use of unmapped enum, range and multirange types.
    /// </summary>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [RequiresUnreferencedCode("The use of unmapped enums, ranges or multiranges requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode("The use of unmapped enums, ranges or multiranges requires dynamic code usage which is incompatible with NativeAOT.")]
    public EDBSlimDataSourceBuilder EnableUnmappedTypes()
    {
        AddTypeInfoResolverFactory(new UnmappedTypeInfoResolverFactory());
        return this;
    }

    #endregion Optional opt-ins

    /// <summary>
    /// Register a connection initializer, which allows executing arbitrary commands when a physical database connection is first opened.
    /// </summary>
    /// <param name="connectionInitializer">
    /// A synchronous connection initialization lambda, which will be called from <see cref="EDBConnection.Open()" /> when a new physical
    /// connection is opened.
    /// </param>
    /// <param name="connectionInitializerAsync">
    /// An asynchronous connection initialization lambda, which will be called from
    /// <see cref="EDBConnection.OpenAsync(CancellationToken)" /> when a new physical connection is opened.
    /// </param>
    /// <remarks>
    /// If an initializer is registered, both sync and async versions must be provided. If you do not use sync APIs in your code, simply
    /// throw <see cref="NotSupportedException" />, which would also catch accidental cases of sync opening.
    /// </remarks>
    /// <remarks>
    /// Take care that the setting you apply in the initializer does not get reverted when the connection is returned to the pool, since
    /// EnterpriseDB.EDBClient sends <c>DISCARD ALL</c> by default. The <see cref="EDBConnectionStringBuilder.NoResetOnClose" /> option can be used to
    /// turn this off.
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public EDBSlimDataSourceBuilder UsePhysicalConnectionInitializer(
        Action<EDBConnection>? connectionInitializer,
        Func<EDBConnection, Task>? connectionInitializerAsync)
    {
        if (connectionInitializer is null != connectionInitializerAsync is null)
            throw new ArgumentException(EDBStrings.SyncAndAsyncConnectionInitializersRequired);

        _connectionInitializer = connectionInitializer;
        _connectionInitializerAsync = connectionInitializerAsync;

        return this;
    }

    /// <summary>
    /// Builds and returns an <see cref="EDBDataSource" /> which is ready for use.
    /// </summary>
    public EDBDataSource Build()
    {
        var (connectionStringBuilder, config) = PrepareConfiguration();

        if (ConnectionStringBuilder.Host!.Contains(','))
        {
            ValidateMultiHost();

            return new EDBMultiHostDataSource(connectionStringBuilder, config);
        }

        return ConnectionStringBuilder.Multiplexing
            ? new MultiplexingDataSource(connectionStringBuilder, config)
            : ConnectionStringBuilder.Pooling
                ? new PoolingDataSource(connectionStringBuilder, config)
                : new UnpooledDataSource(connectionStringBuilder, config);
    }

    /// <summary>
    /// Builds and returns a <see cref="EDBMultiHostDataSource" /> which is ready for use for load-balancing and failover scenarios.
    /// </summary>
    public EDBMultiHostDataSource BuildMultiHost()
    {
        var (connectionStringBuilder, config) = PrepareConfiguration();

        ValidateMultiHost();

        return new(connectionStringBuilder, config);
    }

    (EDBConnectionStringBuilder, EDBDataSourceConfiguration) PrepareConfiguration()
    {
        ConnectionStringBuilder.PostProcessAndValidate();
        var connectionStringBuilder = ConnectionStringBuilder.Clone();

#if NET7_0_OR_GREATER // EnterpriseDB
        var sslClientAuthenticationOptionsCallback = _sslClientAuthenticationOptionsCallback;
        var hasCertificateCallbacks = _userCertificateValidationCallback is not null || _clientCertificatesCallback is not null;
        if (sslClientAuthenticationOptionsCallback is not null && hasCertificateCallbacks)
        {
            throw new NotSupportedException(EDBStrings.SslClientAuthenticationOptionsCallbackWithOtherCallbacksNotSupported);
        }

        if (sslClientAuthenticationOptionsCallback is null && hasCertificateCallbacks)
        {
            sslClientAuthenticationOptionsCallback = options =>
            {
                if (_clientCertificatesCallback is not null)
                {
                    options.ClientCertificates ??= new X509Certificate2Collection();
                    _clientCertificatesCallback.Invoke(options.ClientCertificates);
                }

                if (_userCertificateValidationCallback is not null)
                {
                    options.RemoteCertificateValidationCallback = _userCertificateValidationCallback;
                }
            };
        }

        if (!_transportSecurityHandler.SupportEncryption && sslClientAuthenticationOptionsCallback is not null)
        {
            throw new InvalidOperationException(EDBStrings.TransportSecurityDisabled);
        }
#else // EnterpriseDB
        if (!_transportSecurityHandler.SupportEncryption && (_userCertificateValidationCallback is not null || _clientCertificatesCallback is not null))
        {
            throw new InvalidOperationException(EDBStrings.TransportSecurityDisabled);
        }
#endif

        if (_passwordProvider is not null && _periodicPasswordProvider is not null)
        {
            throw new NotSupportedException(EDBStrings.CannotSetMultiplePasswordProviderKinds);
        }

        if ((_passwordProvider is not null || _periodicPasswordProvider is not null) &&
            (ConnectionStringBuilder.Password is not null || ConnectionStringBuilder.Passfile is not null))
        {
            throw new NotSupportedException(EDBStrings.CannotSetBothPasswordProviderAndPassword);
        }

        ConfigureDefaultFactories(this);

        var typeLoadingOptionsBuilder = new EDBTypeLoadingOptionsBuilder();
#pragma warning disable CS0618 // Type or member is obsolete
        typeLoadingOptionsBuilder.EnableTableCompositesLoading(connectionStringBuilder.LoadTableComposites);
        typeLoadingOptionsBuilder.EnableTypeLoading(connectionStringBuilder.ServerCompatibilityMode is not ServerCompatibilityMode.NoTypeLoading);
#pragma warning restore CS0618 // Type or member is obsolete
        foreach (var callback in _typeLoadingOptionsBuilderCallbacks ?? (IEnumerable<Action<EDBTypeLoadingOptionsBuilder>>)[])
            callback.Invoke(typeLoadingOptionsBuilder);
        var typeLoadingOptions = typeLoadingOptionsBuilder.Build();

        var tracingOptionsBuilder = new EDBTracingOptionsBuilder();
        foreach (var callback in _tracingOptionsBuilderCallbacks ?? (IEnumerable<Action<EDBTracingOptionsBuilder>>)[])
            callback.Invoke(tracingOptionsBuilder);
        var tracingOptions = tracingOptionsBuilder.Build();

        return (connectionStringBuilder, new(
            Name,
            _loggerFactory is null
                ? EDBLoggingConfiguration.NullConfiguration
                : new EDBLoggingConfiguration(_loggerFactory, _sensitiveDataLoggingEnabled),
            tracingOptions,
            typeLoadingOptions,
            _transportSecurityHandler,
            _integratedSecurityHandler,
#if NET7_0_OR_GREATER // EnterpriseDB
            sslClientAuthenticationOptionsCallback,
#else
            _userCertificateValidationCallback,
            _clientCertificatesCallback,
#endif
            _passwordProvider,
            _passwordProviderAsync,
            _periodicPasswordProvider,
            _periodicPasswordSuccessRefreshInterval,
            _periodicPasswordFailureRefreshInterval,
            _resolverChainBuilder.Build(ConfigureResolverChain),
            HackyEnumMappings(),
            DefaultNameTranslator,
            _connectionInitializer,
            _connectionInitializerAsync
#if NET7_0_OR_GREATER
            ,_negotiateOptionsCallback
#endif
            ));

        List<HackyEnumTypeMapping> HackyEnumMappings()
        {
            var mappings = new List<HackyEnumTypeMapping>();

            if (_userTypeMapper.Items.Count > 0)
                foreach (var userTypeMapping in _userTypeMapper.Items)
                    if (userTypeMapping is UserTypeMapper.EnumMapping enumMapping)
                        mappings.Add(new(enumMapping.ClrType, enumMapping.PgTypeName, enumMapping.NameTranslator));

            if (GlobalTypeMapper.Instance.HackyEnumTypeMappings.Count > 0)
                mappings.AddRange(GlobalTypeMapper.Instance.HackyEnumTypeMappings);

            return mappings;
        }
    }

    void ValidateMultiHost()
    {
        if (ConnectionStringBuilder.TargetSessionAttributes is not null)
            throw new InvalidOperationException(EDBStrings.CannotSpecifyTargetSessionAttributes);
        if (ConnectionStringBuilder.Multiplexing)
            throw new NotSupportedException("Multiplexing is not supported with multiple hosts");
        if (ConnectionStringBuilder.ReplicationMode != ReplicationMode.Off)
            throw new NotSupportedException("Replication is not supported with multiple hosts");
    }

    IEDBTypeMapper IEDBTypeMapper.ConfigureJsonOptions(JsonSerializerOptions serializerOptions)
        => ConfigureJsonOptions(serializerOptions);

    [RequiresUnreferencedCode("Json serializer may perform reflection on trimmed types.")]
    [RequiresDynamicCode(
        "Serializing arbitrary types to json can require creating new generic types or methods, which requires creating code at runtime. This may not work when AOT compiling.")]
    IEDBTypeMapper IEDBTypeMapper.EnableDynamicJson(Type[]? jsonbClrTypes, Type[]? jsonClrTypes)
        => EnableDynamicJson(jsonbClrTypes, jsonClrTypes);

    [RequiresUnreferencedCode(
        "The mapping of PostgreSQL records as .NET tuples requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode(
        "The mapping of PostgreSQL records as .NET tuples requires dynamic code usage which is incompatible with NativeAOT.")]
    IEDBTypeMapper IEDBTypeMapper.EnableRecordsAsTuples()
        => EnableRecordsAsTuples();

    [RequiresUnreferencedCode(
        "The use of unmapped enums, ranges or multiranges requires reflection usage which is incompatible with trimming.")]
    [RequiresDynamicCode(
        "The use of unmapped enums, ranges or multiranges requires dynamic code usage which is incompatible with NativeAOT.")]
    IEDBTypeMapper IEDBTypeMapper.EnableUnmappedTypes()
        => EnableUnmappedTypes();

    /// <inheritdoc />
    IEDBTypeMapper IEDBTypeMapper.MapEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string? pgName, IEDBNameTranslator? nameTranslator)
    {
        _userTypeMapper.MapEnum<TEnum>(pgName, nameTranslator);
        return this;
    }
    IEDBTypeMapper IEDBTypeMapper.MapEnum(Type clrType, string pgName, IEDBNameTranslator nameTranslator)
    {
        _userTypeMapper.MapEnum(clrType, pgName, nameTranslator);
        return this;
    }

    // EnterpriseDB : remove optins (see EC-3060)
    IEDBTypeMapper IEDBTypeMapper.DisableDynamicJson() => DisableDynamicJson();
    IEDBTypeMapper IEDBTypeMapper.DisableUnmappedTypes() => DisableUnmappedTypes();
    IEDBTypeMapper IEDBTypeMapper.DisableRecordsAsTuples() => DisableRecordsAsTuples();
    IEDBTypeMapper IEDBTypeMapper.MapComposite<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(string? pgName, IEDBNameTranslator? nameTranslator)
    {
        _userTypeMapper.MapComposite(typeof(T), pgName, nameTranslator);
        return this;
    }

    IEDBTypeMapper IEDBTypeMapper.MapComposite(Type clrType, string pgName, IEDBNameTranslator nameTranslator)
    {
        _userTypeMapper.MapComposite(clrType, pgName, nameTranslator);
        return this;
    }
}
