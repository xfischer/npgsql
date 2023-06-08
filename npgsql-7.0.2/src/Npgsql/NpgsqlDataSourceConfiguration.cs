using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.Internal.TypeMapping;

namespace EnterpriseDB.EDBClient;

sealed record EDBDataSourceConfiguration(
    EDBLoggingConfiguration LoggingConfiguration,
    RemoteCertificateValidationCallback? UserCertificateValidationCallback,
    Action<X509CertificateCollection>? ClientCertificatesCallback,
    Func<EDBConnectionStringBuilder, CancellationToken, ValueTask<string>>? PeriodicPasswordProvider,
    TimeSpan PeriodicPasswordSuccessRefreshInterval,
    TimeSpan PeriodicPasswordFailureRefreshInterval,
    List<TypeHandlerResolverFactory> ResolverFactories,
    Dictionary<string, IUserTypeMapping> UserTypeMappings,
    IEDBNameTranslator DefaultNameTranslator,
    Action<EDBConnection>? ConnectionInitializer,
    Func<EDBConnection, Task>? ConnectionInitializerAsync);
