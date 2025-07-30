using System;
using System.ComponentModel;
using EnterpriseDB.EDBClient;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class EDBServiceCollectionExtensions
{
    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddEDBDataSourceCore(
            serviceCollection, serviceKey: null, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
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
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddEDBDataSourceCore(serviceCollection, serviceKey: null, connectionString,
            static (_, builder, state) => ((Action<EDBDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddEDBSlimDataSourceCore(
            serviceCollection, serviceKey: null, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

    /// <summary>
    /// Registers an <see cref="EDBDataSource" /> and an <see cref="EDBConnection" /> in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
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
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBSlimDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddEDBSlimDataSourceCore(serviceCollection, serviceKey: null, connectionString,
            static (_, builder, state) => ((Action<EDBSlimDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

    /// <summary>
    /// Registers an <see cref="EDBMultiHostDataSource" /> and an <see cref="EDBConnection" /> in the
    /// <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddMultiHostEDBDataSourceCore(
            serviceCollection, serviceKey: null, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

    /// <summary>
    /// Registers an <see cref="EDBMultiHostDataSource" /> and an <see cref="EDBConnection" /> in the
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
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
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddMultiHostEDBDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddMultiHostEDBDataSourceCore(
            serviceCollection, serviceKey: null, connectionString,
            static (_, builder, state) => ((Action<EDBDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);

    /// <summary>
    /// Registers an <see cref="EDBMultiHostDataSource" /> and an <see cref="EDBConnection" /> in the
    /// <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
    /// <param name="connectionLifetime">
    /// The lifetime with which to register the <see cref="EDBConnection" /> in the container.
    /// Defaults to <see cref="ServiceLifetime.Transient" />.
    /// </param>
    /// <param name="dataSourceLifetime">
    /// The lifetime with which to register the <see cref="EDBDataSource" /> service in the container.
    /// Defaults to <see cref="ServiceLifetime.Singleton" />.
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddMultiHostEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddMultiHostEDBSlimDataSourceCore(
            serviceCollection, serviceKey: null, connectionString, dataSourceBuilderAction: null,
            connectionLifetime, dataSourceLifetime, state: null);

    /// <summary>
    /// Registers an <see cref="EDBMultiHostDataSource" /> and an <see cref="EDBConnection" /> in the
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">An EDB .NET connection string.</param>
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
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Defined for binary compatibility with 7.0")]
    public static IServiceCollection AddMultiHostEDBSlimDataSource(
        this IServiceCollection serviceCollection,
        string connectionString,
        Action<EDBSlimDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime,
        ServiceLifetime dataSourceLifetime)
        => AddMultiHostEDBSlimDataSourceCore(
            serviceCollection, serviceKey: null, connectionString,
            static (_, builder, state) => ((Action<EDBSlimDataSourceBuilder>)state!)(builder),
            connectionLifetime, dataSourceLifetime, state: dataSourceBuilderAction);
}
