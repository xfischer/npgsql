using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient.Tests;
using EnterpriseDB.EDBClient.Tests.Support;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.DependencyInjection.Tests;

[TestFixture(DataSourceMode.Standard)]
[TestFixture(DataSourceMode.Slim)]
public class DependencyInjectionTests(DataSourceMode mode)
{
    [Test]
    public async Task EDBDataSource_is_registered_properly([Values] bool async)
    {
        var serviceCollection = new ServiceCollection();
        RegisterDataSource(serviceCollection, TestUtil.ConnectionString);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        var dataSource = serviceProvider.GetRequiredService<EDBDataSource>();

        await using var connection = async
            ? await dataSource.OpenConnectionAsync()
            : dataSource.OpenConnection();
    }

    [Test]
    public async Task EDBMultiHostDataSource_is_registered_properly([Values] bool async)
    {
        var serviceCollection = new ServiceCollection();
        RegisterMultiHostDataSource(serviceCollection, TestUtil.ConnectionString);

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        var multiHostDataSource = serviceProvider.GetRequiredService<EDBMultiHostDataSource>();
        var dataSource = serviceProvider.GetRequiredService<EDBDataSource>();

        Assert.That(dataSource, Is.SameAs(multiHostDataSource));

        await using var connection = async
            ? await dataSource.OpenConnectionAsync()
            : dataSource.OpenConnection();
    }

    [Test]
    public async Task EDBDataSource_with_service_key_is_registered_properly([Values] bool async)
    {
        const string serviceKey = "key";
        var serviceCollection = new ServiceCollection();
        RegisterDataSource(serviceCollection, TestUtil.ConnectionString, serviceKey);
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        var dataSource = serviceProvider.GetRequiredKeyedService<EDBDataSource>(serviceKey);
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<EDBDataSource>());
        await using var connection = async
            ? await dataSource.OpenConnectionAsync()
            : dataSource.OpenConnection();
    }
    [Test]
    public async Task EDBMultiHostDataSource_with_service_key_is_registered_properly([Values] bool async)
    {
        const string serviceKey = "key";
        var serviceCollection = new ServiceCollection();
        RegisterMultiHostDataSource(serviceCollection, TestUtil.ConnectionString, serviceKey);
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        var multiHostDataSource = serviceProvider.GetRequiredKeyedService<EDBMultiHostDataSource>(serviceKey);
        var dataSource = serviceProvider.GetRequiredKeyedService<EDBDataSource>(serviceKey);
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<EDBMultiHostDataSource>());
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<EDBDataSource>());
        Assert.That(dataSource, Is.SameAs(multiHostDataSource));
        await using var connection = async
            ? await dataSource.OpenConnectionAsync()
            : dataSource.OpenConnection();
    }
    [Test]
    public void EDBDataSource_is_registered_as_singleton_by_default()
    {
        var serviceCollection = new ServiceCollection();
        RegisterDataSource(serviceCollection, TestUtil.ConnectionString);

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        var scopeServiceProvider1 = scope1.ServiceProvider;
        var scopeServiceProvider2 = scope2.ServiceProvider;

        var dataSource1 = scopeServiceProvider1.GetRequiredService<EDBDataSource>();
        var dataSource2 = scopeServiceProvider2.GetRequiredService<EDBDataSource>();

        Assert.That(dataSource2, Is.SameAs(dataSource1));
    }

    [Test]
    public async Task EDBConnection_is_registered_properly([Values] bool async)
    {
        var serviceCollection = new ServiceCollection();
        RegisterDataSource(serviceCollection, TestUtil.ConnectionString);

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var scopedServiceProvider = scope.ServiceProvider;

        var connection = scopedServiceProvider.GetRequiredService<EDBConnection>();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));

        if (async)
            await connection.OpenAsync();
        else
            connection.Open();
    }

    [Test]
    public void EDBConnection_is_registered_as_transient_by_default()
    {
        var serviceCollection = new ServiceCollection();
        RegisterDataSource(serviceCollection, "Host=localhost;Username=test;Password=test");

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope1 = serviceProvider.CreateScope();
        var scopedServiceProvider1 = scope1.ServiceProvider;

        var connection1 = scopedServiceProvider1.GetRequiredService<EDBConnection>();
        var connection2 = scopedServiceProvider1.GetRequiredService<EDBConnection>();

        Assert.That(connection2, Is.Not.SameAs(connection1));

        using var scope2 = serviceProvider.CreateScope();
        var scopedServiceProvider2 = scope2.ServiceProvider;

        var connection3 = scopedServiceProvider2.GetRequiredService<EDBConnection>();
        Assert.That(connection3, Is.Not.SameAs(connection1));
    }

    [Test]
    public async Task LoggerFactory_is_picked_up_from_ServiceCollection()
    {
        var listLoggerProvider = new ListLoggerProvider();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(b => b.AddProvider(listLoggerProvider));
        RegisterDataSource(serviceCollection, TestUtil.ConnectionString);
        await using var serviceProvider = serviceCollection.BuildServiceProvider();

        var dataSource = serviceProvider.GetRequiredService<EDBDataSource>();
        await using var command = dataSource.CreateCommand("SELECT 1");

        using (listLoggerProvider.Record())
            _ = command.ExecuteNonQuery();

        Assert.That(listLoggerProvider.Log.Any(l => l.Id == EDBEventId.CommandExecutionCompleted));
    }

    IServiceCollection RegisterDataSource(ServiceCollection serviceCollection, string connectionString, object? serviceKey = null)
        => mode switch
        {
            DataSourceMode.Standard => serviceCollection.AddEDBDataSource(connectionString, serviceKey: serviceKey),
            DataSourceMode.Slim => serviceCollection.AddEDBSlimDataSource(connectionString, serviceKey: serviceKey),
            _ => throw new NotSupportedException($"Mode {mode} not supported")
        };

    IServiceCollection RegisterMultiHostDataSource(ServiceCollection serviceCollection, string connectionString, object? serviceKey = null)
        => mode switch
        {
            DataSourceMode.Standard => serviceCollection.AddMultiHostEDBDataSource(connectionString, serviceKey: serviceKey),
            DataSourceMode.Slim => serviceCollection.AddMultiHostEDBSlimDataSource(connectionString, serviceKey: serviceKey),
            _ => throw new NotSupportedException($"Mode {mode} not supported")
        };
}

public enum DataSourceMode
{
    Standard,
    Slim
}
