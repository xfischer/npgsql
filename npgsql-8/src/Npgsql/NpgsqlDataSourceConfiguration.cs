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
    TransportSecurityHandler TransportSecurityHandler,
    IntegratedSecurityHandler userCertificateValidationCallback,
    RemoteCertificateValidationCallback? UserCertificateValidationCallback,
    Action<X509CertificateCollection>? ClientCertificatesCallback,
    Func<EDBConnectionStringBuilder, string>? PasswordProvider,
    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? PasswordProviderAsync,
    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? PeriodicPasswordProvider,
    TimeSpan PeriodicPasswordSuccessRefreshInterval,
    TimeSpan PeriodicPasswordFailureRefreshInterval,
    IEnumerable<IPgTypeInfoResolver> ResolverChain,
    List<HackyEnumTypeMapping> HackyEnumMappings,
    IEDBNameTranslator DefaultNameTranslator,
    Action<EDBConnection>? ConnectionInitializer,
    Func<EDBConnection, Task>? ConnectionInitializerAsync);
