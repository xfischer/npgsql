using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient.Properties;

namespace EnterpriseDB.EDBClient.Internal;

class IntegratedSecurityHandler
{
    public virtual bool IsSupported => false;

    public virtual ValueTask<string?> GetUsername(bool async, bool includeRealm, ILogger connectionLogger, CancellationToken cancellationToken)
    {
        connectionLogger.LogDebug(string.Format(EDBStrings.IntegratedSecurityDisabled, nameof(EDBSlimDataSourceBuilder.EnableIntegratedSecurity)));
        return new();
    }

    public virtual ValueTask NegotiateAuthentication(bool async, EDBConnector connector)
        => throw new NotSupportedException(string.Format(EDBStrings.IntegratedSecurityDisabled, nameof(EDBSlimDataSourceBuilder.EnableIntegratedSecurity)));
}

sealed class RealIntegratedSecurityHandler : IntegratedSecurityHandler
{
    public override bool IsSupported => true;

    public override ValueTask<string?> GetUsername(bool async, bool includeRealm, ILogger connectionLogger, CancellationToken cancellationToken)
        => KerberosUsernameProvider.GetUsername(async, includeRealm, connectionLogger, cancellationToken);

    public override ValueTask NegotiateAuthentication(bool async, EDBConnector connector)
        => new(connector.AuthenticateGSS(async));
}
