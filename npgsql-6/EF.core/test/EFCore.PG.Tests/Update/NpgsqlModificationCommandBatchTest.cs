using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Xunit;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Tests.Update;

public class NpgsqlModificationCommandBatchTest
{
    [Fact]
    public void AddCommand_returns_false_when_max_batch_size_is_reached()
    {
        var typeMapper = new NpgsqlTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
            new NpgsqlOptions());

        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var batch = new NpgsqlModificationCommandBatch(
            new ModificationCommandBatchFactoryDependencies(
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        typeMapper)),
                new NpgsqlSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new NpgsqlSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()),
                        typeMapper)),
                new TypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies(
                        typeMapper, new CoreSingletonOptions())),
                new CurrentDbContext(new FakeDbContext()),
                logger),
            1);

        Assert.True(
            batch.AddCommand(
                CreateModificationCommand("T1", null, false)));
        Assert.False(
            batch.AddCommand(
                CreateModificationCommand("T1", null, false)));
    }

    private class FakeDbContext : DbContext
    {
    }

    private static IModificationCommand CreateModificationCommand(
        string name,
        string schema,
        bool sensitiveLoggingEnabled)
    {
        var modificationCommandParameters = new ModificationCommandParameters(
            name, schema, sensitiveLoggingEnabled);

        var modificationCommand = new ModificationCommandFactory().CreateModificationCommand(
            modificationCommandParameters);

        return modificationCommand;
    }
}