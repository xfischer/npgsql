using System;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension method for setting up EDB services in an <see cref="IServiceCollection" />.
/// </summary>
public static class NpgsqlServiceCollectionExtensions
{
    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB connection string.</param>
    /// <param name="dataSourceBuilderAction">
    /// An action to configure the <see cref="EDBDataSourceBuilder" /> for further customizations of the <see cref="EDBDataSource" />.
    /// </param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
        => AddEDBDataSourceCore(serviceCollection, connectionString, dataSourceBuilderAction, connectionLifetime, dataSourceLifetime);

    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB connection string.</param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
        => AddEDBDataSourceCore(
            serviceCollection, connectionString, dataSourceBuilderAction: null, connectionLifetime, dataSourceLifetime);

    /// <summary>
    /// Registers an <see cref="EDBMultiHostDataSource" /> and an <see cref="EDBConnection" /> in the
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB connection string.</param>
    /// <param name="dataSourceBuilderAction">
    /// An action to configure the <see cref="EDBDataSourceBuilder" /> for further customizations of the <see cref="EDBDataSource" />.
    /// </param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
        => AddEDBMultiHostDataSourceCore(
            serviceCollection, connectionString, dataSourceBuilderAction, connectionLifetime, dataSourceLifetime);

    /// <summary>
    /// Registers an <see cref="EDBMultiHostDataSource" /> and an <see cref="EDBConnection" /> in the
    /// <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB connection string.</param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
        => AddEDBMultiHostDataSourceCore(
            serviceCollection, connectionString, dataSourceBuilderAction: null, connectionLifetime, dataSourceLifetime);

    static IServiceCollection AddEDBDataSourceCore(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder>? dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBDataSource),
                sp =>
                {
                    var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(dataSourceBuilder);
                    return dataSourceBuilder.Build();
                },
                dataSourceLifetime));

        AddCommonServices(serviceCollection, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    static IServiceCollection AddEDBMultiHostDataSourceCore(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder>? dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBMultiHostDataSource),
                sp =>
                {
                    var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(dataSourceBuilder);
                    return dataSourceBuilder.BuildMultiHost();
                },
                dataSourceLifetime));

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBDataSource),
                sp => sp.GetRequiredService<EDBMultiHostDataSource>(),
                dataSourceLifetime));

        AddCommonServices(serviceCollection, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    static void AddCommonServices(
        IServiceCollection serviceCollection,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBConnection),
                sp => sp.GetRequiredService<EDBDataSource>().CreateConnection(),
                connectionLifetime));

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(DbDataSource),
                sp => sp.GetRequiredService<EDBDataSource>(),
                dataSourceLifetime));

        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(DbConnection),
                sp => sp.GetRequiredService<EDBConnection>(),
                connectionLifetime));
    }
}
