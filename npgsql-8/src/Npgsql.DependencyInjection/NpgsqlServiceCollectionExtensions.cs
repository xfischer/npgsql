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
public static partial class EDBServiceCollectionExtensions
{
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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddEDBDataSourceCore(
            serviceCollection, serviceKey, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddEDBDataSourceCore(serviceCollection, serviceKey, connectionString,
            static (_, builder, state) => ((Action<EDBDataSourceBuilder>)state!)(builder)
            , connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<IServiceProvider, EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddEDBDataSourceCore(serviceCollection, serviceKey, connectionString,
            static (sp, builder, state) => ((Action<IServiceProvider, EDBDataSourceBuilder>)state!)(sp, builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddEDBSlimDataSourceCore(
            serviceCollection, serviceKey, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB connection string.</param>
    /// <param name="dataSourceBuilderAction">
    /// An action to configure the <see cref="EDBSlimDataSourceBuilder" /> for further customizations of the <see cref="EDBDataSource" />.
    /// </param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBSlimDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddEDBSlimDataSourceCore(serviceCollection, serviceKey, connectionString,
            static (_, builder, state) => ((Action<EDBSlimDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB connection string.</param>
    /// <param name="dataSourceBuilderAction">
    /// An action to configure the <see cref="EDBSlimDataSourceBuilder" /> for further customizations of the <see cref="EDBDataSource" />.
    /// </param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<IServiceProvider, EDBSlimDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddEDBSlimDataSourceCore(serviceCollection, serviceKey, connectionString,
            static (sp, builder, state) => ((Action<IServiceProvider, EDBSlimDataSourceBuilder>)state!)(sp, builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddMultiHostEDBDataSourceCore(
            serviceCollection, serviceKey, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddMultiHostEDBDataSourceCore(
            serviceCollection, serviceKey, connectionString,
            static (_, builder, state) => ((Action<EDBDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<IServiceProvider, EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddMultiHostEDBDataSourceCore(
            serviceCollection, serviceKey, connectionString,
            static (sp, builder, state) => ((Action<IServiceProvider, EDBDataSourceBuilder>)state!)(sp, builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddMultiHostEDBSlimDataSourceCore(
            serviceCollection, serviceKey, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBSlimDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddMultiHostEDBSlimDataSourceCore(
            serviceCollection, serviceKey, connectionString,
            static (_, builder, state) => ((Action<EDBSlimDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

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
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the data source.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMultiHostEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<IServiceProvider, EDBSlimDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Transient,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton,
        object? serviceKey = null)
        => AddMultiHostEDBSlimDataSourceCore(
            serviceCollection, serviceKey, connectionString,
            static (sp, builder, state) => ((Action<IServiceProvider, EDBSlimDataSourceBuilder>)state!)(sp, builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

    static IServiceCollection AddEDBDataSourceCore(
        this IServiceCollection serviceCollection,
        object? serviceKey,
        string connectionString,
        Action<IServiceProvider, EDBDataSourceBuilder, object?>? dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime,
        object? state)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBDataSource),
                serviceKey,
                (sp, key) =>
                {
                    var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(sp, dataSourceBuilder, state);
                    return dataSourceBuilder.Build();
                },
                dataSourceLifetime));

        AddCommonServices(serviceCollection, serviceKey, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    static IServiceCollection AddEDBSlimDataSourceCore(
        this IServiceCollection serviceCollection,
        object? serviceKey,
        string connectionString,
        Action<IServiceProvider, EDBSlimDataSourceBuilder, object?>? dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime,
        object? state)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBDataSource),
                serviceKey,
                (sp, key) =>
                {
                    var dataSourceBuilder = new EDBSlimDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(sp, dataSourceBuilder, state);
                    return dataSourceBuilder.Build();
                },
                dataSourceLifetime));

        AddCommonServices(serviceCollection, serviceKey, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    static IServiceCollection AddMultiHostEDBDataSourceCore(
        this IServiceCollection serviceCollection,
        object? serviceKey,
        string connectionString,
        Action<IServiceProvider, EDBDataSourceBuilder, object?>? dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime,
        object? state)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBMultiHostDataSource),
                serviceKey,
                (sp, key) =>
                {
                    var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(sp, dataSourceBuilder, state);
                    return dataSourceBuilder.BuildMultiHost();
                },
                dataSourceLifetime));

        if (serviceKey is not null)
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(EDBDataSource),
                    serviceKey,
                    (sp, key) => sp.GetRequiredKeyedService<EDBMultiHostDataSource>(key),
                    dataSourceLifetime));
        }
        else
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(EDBDataSource),
                    sp => sp.GetRequiredService<EDBMultiHostDataSource>(),
                    dataSourceLifetime));

        }

        AddCommonServices(serviceCollection, serviceKey, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    static IServiceCollection AddMultiHostEDBSlimDataSourceCore(
        this IServiceCollection serviceCollection,
        object? serviceKey,
        string connectionString,
        Action<IServiceProvider, EDBSlimDataSourceBuilder, object?>? dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime,
        object? state)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(EDBMultiHostDataSource),
                serviceKey,
                (sp, _) =>
                {
                    var dataSourceBuilder = new EDBSlimDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                    dataSourceBuilderAction?.Invoke(sp, dataSourceBuilder, state);
                    return dataSourceBuilder.BuildMultiHost();
                },
                dataSourceLifetime));

        if (serviceKey is not null)
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(EDBDataSource),
                    serviceKey,
                    (sp, key) => sp.GetRequiredKeyedService<EDBMultiHostDataSource>(key),
                    dataSourceLifetime));
        }
        else
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(EDBDataSource),
                    sp => sp.GetRequiredService<EDBMultiHostDataSource>(),
                    dataSourceLifetime));

        }

        AddCommonServices(serviceCollection, serviceKey, connectionLifetime, dataSourceLifetime);

        return serviceCollection;
    }

    static void AddCommonServices(
        IServiceCollection serviceCollection,
        object? serviceKey,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
    {
        // We don't try to invoke KeyedService methods if there is no service key.
        // This allows user code that use non-standard containers without support for IKeyedServiceProvider to keep on working.
        if (serviceKey is not null)
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(EDBConnection),
                    serviceKey,
                    (sp, key) => sp.GetRequiredKeyedService<EDBDataSource>(key).CreateConnection(),
                    connectionLifetime));

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(DbDataSource),
                    serviceKey,
                    (sp, key) => sp.GetRequiredKeyedService<EDBDataSource>(key),
                    dataSourceLifetime));

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(DbConnection),
                    serviceKey,
                    (sp, key) => sp.GetRequiredKeyedService<EDBConnection>(key),
                    connectionLifetime));
        }
        else
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
}
