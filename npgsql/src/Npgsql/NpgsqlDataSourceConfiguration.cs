using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal;

namespace EnterpriseDB.EDBClient;

sealed record EDBDataSourceConfiguration(string? Name,
    EDBLoggingConfiguration LoggingConfiguration,
	EDBTracingOptions TracingOptions,
    EDBTypeLoadingOptions TypeLoading,
    TransportSecurityHandler TransportSecurityHandler,
    IntegratedSecurityHandler IntegratedSecurityHandler,
#if NET8_0_OR_GREATER // EnterpriseDB 
    Action<SslClientAuthenticationOptions>? SslClientAuthenticationOptionsCallback,
#else
    RemoteCertificateValidationCallback? UserCertificateValidationCallback,
    Action<X509CertificateCollection>? ClientCertificatesCallback,
#endif
    Func<EDBConnectionStringBuilder, string>? PasswordProvider,
    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? PasswordProviderAsync,
    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? PeriodicPasswordProvider,
    TimeSpan PeriodicPasswordSuccessRefreshInterval,
    TimeSpan PeriodicPasswordFailureRefreshInterval,
    PgTypeInfoResolverChain ResolverChain,
    IEnumerable<DbTypeResolverFactory> DbTypeResolverFactories,
    IEDBNameTranslator DefaultNameTranslator,
    Action<EDBConnection>? ConnectionInitializer,
    Func<EDBConnection, Task>? ConnectionInitializerAsync
#if NET8_0_OR_GREATER
    ,Action<NegotiateAuthenticationClientOptions>? NegotiateOptionsCallback
#endif
    );
