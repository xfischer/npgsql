using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Properties;
using EnterpriseDB.EDBClient.Util;

namespace EnterpriseDB.EDBClient.Internal;

class TransportSecurityHandler
{
    public virtual bool SupportEncryption => false;

    public virtual Func<X509Certificate2?>? RootCertificateCallback
    {
        get => throw new NotSupportedException(string.Format(EDBStrings.TransportSecurityDisabled, nameof(EDBSlimDataSourceBuilder.EnableTransportSecurity)));
        set => throw new NotSupportedException(string.Format(EDBStrings.TransportSecurityDisabled, nameof(EDBSlimDataSourceBuilder.EnableTransportSecurity)));
    }

    public virtual Task NegotiateEncryption(bool async, EDBConnector connector, SslMode sslMode, EDBTimeout timeout, bool isFirstAttempt)
        => throw new NotSupportedException(string.Format(EDBStrings.TransportSecurityDisabled, nameof(EDBSlimDataSourceBuilder.EnableTransportSecurity)));

    public virtual void AuthenticateSASLSha256Plus(EDBConnector connector, ref string mechanism, ref string cbindFlag, ref string cbind,
        ref bool successfulBind)
        => throw new NotSupportedException(string.Format(EDBStrings.TransportSecurityDisabled, nameof(EDBSlimDataSourceBuilder.EnableTransportSecurity)));
}

sealed class RealTransportSecurityHandler : TransportSecurityHandler
{
    public override bool SupportEncryption => true;

    public override Func<X509Certificate2?>? RootCertificateCallback { get; set; }

    public override Task NegotiateEncryption(bool async, EDBConnector connector, SslMode sslMode, EDBTimeout timeout, bool isFirstAttempt)
        => connector.NegotiateEncryption(sslMode, timeout, async, isFirstAttempt);

    public override void AuthenticateSASLSha256Plus(EDBConnector connector, ref string mechanism, ref string cbindFlag, ref string cbind,
            ref bool successfulBind)
        => connector.AuthenticateSASLSha256Plus(ref mechanism, ref cbindFlag, ref cbind, ref successfulBind);
}
