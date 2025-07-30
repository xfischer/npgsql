using System;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.ResolverFactories;

sealed class GeometricTypeInfoResolverFactory : PgTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver CreateResolver() => new Resolver();
    public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver();

    class Resolver : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddStructType<EDBPoint>(DataTypeNames.Point,
                static (options, mapping, _) => mapping.CreateInfo(options, new PointConverter()), isDefault: true);
            mappings.AddStructType<EDBBox>(DataTypeNames.Box,
                static (options, mapping, _) => mapping.CreateInfo(options, new BoxConverter()), isDefault: true);
            mappings.AddStructType<EDBPolygon>(DataTypeNames.Polygon,
                static (options, mapping, _) => mapping.CreateInfo(options, new PolygonConverter()), isDefault: true);
            mappings.AddStructType<EDBLine>(DataTypeNames.Line,
                static (options, mapping, _) => mapping.CreateInfo(options, new LineConverter()), isDefault: true);
            mappings.AddStructType<EDBLSeg>(DataTypeNames.LSeg,
                static (options, mapping, _) => mapping.CreateInfo(options, new LineSegmentConverter()), isDefault: true);
            mappings.AddStructType<EDBPath>(DataTypeNames.Path,
                static (options, mapping, _) => mapping.CreateInfo(options, new PathConverter()), isDefault: true);
            mappings.AddStructType<EDBCircle>(DataTypeNames.Circle,
                static (options, mapping, _) => mapping.CreateInfo(options, new CircleConverter()), isDefault: true);

            return mappings;
        }
    }

    sealed class ArrayResolver : Resolver, IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        new TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddStructArrayType<EDBPoint>(DataTypeNames.Point);
            mappings.AddStructArrayType<EDBBox>(DataTypeNames.Box);
            mappings.AddStructArrayType<EDBPolygon>(DataTypeNames.Polygon);
            mappings.AddStructArrayType<EDBLine>(DataTypeNames.Line);
            mappings.AddStructArrayType<EDBLSeg>(DataTypeNames.LSeg);
            mappings.AddStructArrayType<EDBPath>(DataTypeNames.Path);
            mappings.AddStructArrayType<EDBCircle>(DataTypeNames.Circle);

            return mappings;
        }
    }
}
