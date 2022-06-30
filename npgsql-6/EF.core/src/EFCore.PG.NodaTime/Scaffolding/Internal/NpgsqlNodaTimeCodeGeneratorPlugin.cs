using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NodaTime.Scaffolding.Internal;

public class NpgsqlNodaTimeCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
{
    private static readonly MethodInfo _useNodaTimeMethodInfo
        = typeof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions.UseNodaTime),
            typeof(NpgsqlDbContextOptionsBuilder));

    public override MethodCallCodeFragment GenerateProviderOptions()
        => new(_useNodaTimeMethodInfo);
}