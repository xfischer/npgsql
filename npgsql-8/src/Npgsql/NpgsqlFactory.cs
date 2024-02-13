using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// A factory to create instances of various EDB objects.
/// </summary>
[Serializable]
public sealed class EDBFactory : DbProviderFactory, IServiceProvider
{
    /// <summary>
    /// Gets an instance of the <see cref="EDBFactory"/>.
    /// This can be used to retrieve strongly typed data objects.
    /// </summary>
    public static readonly EDBFactory Instance = new();

    EDBFactory() {}

    /// <summary>
    /// Returns a strongly typed <see cref="DbCommand"/> instance.
    /// </summary>
    public override DbCommand CreateCommand() => new EDBCommand();

    /// <summary>
    /// Returns a strongly typed <see cref="DbConnection"/> instance.
    /// </summary>
    public override DbConnection CreateConnection() => new EDBConnection();

    /// <summary>
    /// Returns a strongly typed <see cref="DbParameter"/> instance.
    /// </summary>
    public override DbParameter CreateParameter() => new EDBParameter();

    /// <summary>
    /// Returns a strongly typed <see cref="DbConnectionStringBuilder"/> instance.
    /// </summary>
    public override DbConnectionStringBuilder CreateConnectionStringBuilder() => new EDBConnectionStringBuilder();

    /// <summary>
    /// Returns a strongly typed <see cref="DbCommandBuilder"/> instance.
    /// </summary>
    public override DbCommandBuilder CreateCommandBuilder() => new EDBCommandBuilder();

    /// <summary>
    /// Returns a strongly typed <see cref="DbDataAdapter"/> instance.
    /// </summary>
    public override DbDataAdapter CreateDataAdapter() => new EDBDataAdapter();

#if !(NETSTANDARD2_0 || NETFRAMEWORK) // EnterpriseDB (NETFRAMEWORK)
    /// <summary>
    /// Specifies whether the specific <see cref="DbProviderFactory"/> supports the <see cref="DbDataAdapter"/> class.
    /// </summary>
    public override bool CanCreateDataAdapter => true;

    /// <summary>
    /// Specifies whether the specific <see cref="DbProviderFactory"/> supports the <see cref="DbCommandBuilder"/> class.
    /// </summary>
    public override bool CanCreateCommandBuilder => true;
#endif

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public override bool CanCreateBatch => true;

    /// <inheritdoc/>
    public override DbBatch CreateBatch() => new EDBBatch();

    /// <inheritdoc/>
    public override DbBatchCommand CreateBatchCommand() => new EDBBatchCommand();
#endif

#if NET7_0_OR_GREATER
    /// <inheritdoc/>
    public override DbDataSource CreateDataSource(string connectionString)
        => EDBDataSource.Create(connectionString);
#endif

    #region IServiceProvider Members

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type serviceType, or null if there is no service object of type serviceType.</returns>
    public object? GetService(Type serviceType) => null;

    #endregion
}