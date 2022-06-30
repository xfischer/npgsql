using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class NpgsqlRelationalConnection : RelationalConnection, INpgsqlRelationalConnection
{
    private ProvideClientCertificatesCallback? ProvideClientCertificatesCallback { get; }
    private RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; }
    private ProvidePasswordCallback? ProvidePasswordCallback { get; }

    /// <summary>
    ///     Indicates whether the store connection supports ambient transactions
    /// </summary>
    protected override bool SupportsAmbientTransactions => true;

    public NpgsqlRelationalConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
    {
        var npgsqlOptions =
            dependencies.ContextOptions.Extensions.OfType<NpgsqlOptionsExtension>().FirstOrDefault();

        if (npgsqlOptions is not null)
        {
            ProvideClientCertificatesCallback = npgsqlOptions.ProvideClientCertificatesCallback;
            RemoteCertificateValidationCallback = npgsqlOptions.RemoteCertificateValidationCallback;
            ProvidePasswordCallback = npgsqlOptions.ProvidePasswordCallback;
        }
    }

    protected override DbConnection CreateDbConnection()
    {
        var conn = new EDBConnection(ConnectionString);
        if (ProvideClientCertificatesCallback is not null)
        {
            conn.ProvideClientCertificatesCallback = ProvideClientCertificatesCallback;
        }

        if (RemoteCertificateValidationCallback is not null)
        {
            conn.UserCertificateValidationCallback = RemoteCertificateValidationCallback;
        }

        if (ProvidePasswordCallback is not null)
        {
            conn.ProvidePasswordCallback = ProvidePasswordCallback;
        }

        return conn;
    }

    public virtual INpgsqlRelationalConnection CreateMasterConnection()
    {
        var adminDb = Dependencies.ContextOptions.FindExtension<NpgsqlOptionsExtension>()?.AdminDatabase
            ?? "postgres";
        var csb = new EDBConnectionStringBuilder(ConnectionString) {
            Database = adminDb,
            Pooling = false,
            Multiplexing = false
        };

        var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions);
        var connectionString = csb.ToString();

        relationalOptions = relationalOptions.Connection is not null
            ? relationalOptions.WithConnection(((EDBConnection)DbConnection).CloneWith(connectionString))
            : relationalOptions.WithConnectionString(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(relationalOptions);

        return new NpgsqlRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options });
    }

    [AllowNull]
    public new virtual EDBConnection DbConnection
    {
        get => (EDBConnection)base.DbConnection;
        set => base.DbConnection = value;
    }

    // Accessing Transaction.Current is expensive, so don't do it if Enlist is false in the connection string
    public override Transaction? CurrentAmbientTransaction
        => DbConnection.Settings.Enlist ? Transaction.Current : null;

    public virtual NpgsqlRelationalConnection CloneWith(string connectionString)
    {
        var clonedDbConnection = DbConnection.CloneWith(connectionString);

        var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions)
            .WithConnectionString(null)
            .WithConnection(clonedDbConnection);

        var optionsBuilder = new DbContextOptionsBuilder();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(relationalOptions);

        return new NpgsqlRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options });
    }
}