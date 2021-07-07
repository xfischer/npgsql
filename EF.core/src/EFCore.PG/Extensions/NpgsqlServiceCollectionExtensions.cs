using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Migrations;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Migrations.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Update.Internal;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Utilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to configure Entity Framework Core for Npgsql.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class NpgsqlServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Npgsql database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkNpgsql([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder =
                new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                    .TryAdd<LoggingDefinitions, NpgsqlLoggingDefinitions>()
                    .TryAdd<IDatabaseProvider, DatabaseProvider<NpgsqlOptionsExtension>>()
                    .TryAdd<IValueGeneratorCache>(p => p.GetService<INpgsqlValueGeneratorCache>())
                    .TryAdd<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                    .TryAdd<ISqlGenerationHelper, NpgsqlSqlGenerationHelper>()
                    .TryAdd<IRelationalAnnotationProvider, NpgsqlAnnotationProvider>()
                    .TryAdd<IModelValidator, NpgsqlModelValidator>()
                    .TryAdd<IProviderConventionSetBuilder, NpgsqlConventionSetBuilder>()
                    .TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>()
                    .TryAdd<IUpdateSqlGenerator, NpgsqlUpdateSqlGenerator>()
                    .TryAdd<IModificationCommandBatchFactory, NpgsqlModificationCommandBatchFactory>()
                    .TryAdd<IValueGeneratorSelector, NpgsqlValueGeneratorSelector>()
                    .TryAdd<IRelationalConnection>(p => p.GetService<INpgsqlRelationalConnection>())
                    .TryAdd<IMigrationsSqlGenerator, NpgsqlMigrationsSqlGenerator>()
                    .TryAdd<IRelationalDatabaseCreator, NpgsqlDatabaseCreator>()
                    .TryAdd<IHistoryRepository, NpgsqlHistoryRepository>()
                    .TryAdd<ICompiledQueryCacheKeyGenerator, NpgsqlCompiledQueryCacheKeyGenerator>()
                    .TryAdd<IExecutionStrategyFactory, NpgsqlExecutionStrategyFactory>()
                    .TryAdd<IMethodCallTranslatorProvider, NpgsqlMethodCallTranslatorProvider>()
                    .TryAdd<IMemberTranslatorProvider, NpgsqlMemberTranslatorProvider>()
                    .TryAdd<IEvaluatableExpressionFilter, NpgsqlEvaluatableExpressionFilter>()
                    .TryAdd<IQuerySqlGeneratorFactory, NpgsqlQuerySqlGeneratorFactory>()
                    .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, NpgsqlSqlTranslatingExpressionVisitorFactory>()
                    .TryAdd<IRelationalParameterBasedSqlProcessorFactory, NpgsqlParameterBasedSqlProcessorFactory>()
                    .TryAdd<ISqlExpressionFactory, NpgsqlSqlExpressionFactory>()
                    .TryAdd<ISingletonOptions, INpgsqlOptions>(p => p.GetService<INpgsqlOptions>())
                    .TryAdd<IValueConverterSelector, NpgsqlValueConverterSelector>()
                    .TryAdd<IQueryCompilationContextFactory, NpgsqlQueryCompilationContextFactory>()
                    .TryAddProviderSpecificServices(
                        b => b
                             .TryAddSingleton<INpgsqlValueGeneratorCache, NpgsqlValueGeneratorCache>()
                             .TryAddSingleton<INpgsqlOptions, NpgsqlOptions>()
                             .TryAddScoped<INpgsqlSequenceValueGeneratorFactory, NpgsqlSequenceValueGeneratorFactory>()
                             .TryAddScoped<INpgsqlRelationalConnection, NpgsqlRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
