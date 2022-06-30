using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using NetTopologySuite.Geometries;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Scaffolding.Internal;

public class NpgsqlNetTopologySuiteCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
{
    private static readonly MethodInfo _useNetTopologySuiteMethodInfo
        = typeof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions.UseNetTopologySuite),
            typeof(NpgsqlDbContextOptionsBuilder),
            typeof(CoordinateSequenceFactory),
            typeof(PrecisionModel),
            typeof(Ordinates),
            typeof(bool));

    public override MethodCallCodeFragment GenerateProviderOptions()
        => new(_useNetTopologySuiteMethodInfo);
}