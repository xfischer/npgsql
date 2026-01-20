using System;
using System.Net;
using System.Net.NetworkInformation;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.ResolverFactories;

sealed class NetworkTypeInfoResolverFactory : PgTypeInfoResolverFactory
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
            // macaddr
            mappings.AddType<PhysicalAddress>(DataTypeNames.MacAddr,
                static (options, mapping, _) => mapping.CreateInfo(options, new MacaddrConverter(macaddr8: false)), isDefault: true);
            mappings.AddType<PhysicalAddress>(DataTypeNames.MacAddr8,
                static (options, mapping, _) => mapping.CreateInfo(options, new MacaddrConverter(macaddr8: true)),
                mapping => mapping with { MatchRequirement = MatchRequirement.DataTypeName });

            // inet
            // There are certain IPAddress values like Loopback or Any that return a *private* derived type (see https://github.com/dotnet/runtime/issues/27870).
            mappings.AddType<IPAddress>(DataTypeNames.Inet,
                static (options, mapping, _) => new PgTypeInfo(options, new IPAddressConverter(), new DataTypeName(mapping.DataTypeName),
                    unboxedType: mapping.Type != typeof(IPAddress) ? mapping.Type : null),
                mapping => mapping with
                {
                    MatchRequirement = MatchRequirement.Single,
                    TypeMatchPredicate = type => type is null || typeof(IPAddress).IsAssignableFrom(type)
                });
            mappings.AddStructType<EDBInet>(DataTypeNames.Inet,
                static (options, mapping, _) => mapping.CreateInfo(options, new EDBInetConverter()));

#if NET8_0_OR_GREATER
            mappings.AddStructType<IPNetwork>(DataTypeNames.Cidr,
                static (options, mapping, _) => mapping.CreateInfo(options, new IPNetworkConverter()), isDefault: true);

#pragma warning disable CS0618 // EDBCidr is obsolete
            mappings.AddStructType<EDBCidr>(DataTypeNames.Cidr,
                static (options, mapping, _) => mapping.CreateInfo(options, new EDBCidrConverter()));
#pragma warning restore CS0618
#else
#pragma warning disable CS0618 // EDBCidr is obsolete
            mappings.AddStructType<EDBCidr>(DataTypeNames.Cidr,
                static (options, mapping, _) => mapping.CreateInfo(options, new EDBCidrConverter()), isDefault: true);
#pragma warning restore CS0618	
#endif




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
            // macaddr
            mappings.AddArrayType<PhysicalAddress>(DataTypeNames.MacAddr);
            mappings.AddArrayType<PhysicalAddress>(DataTypeNames.MacAddr8);

            // inet
            mappings.AddArrayType<IPAddress>(DataTypeNames.Inet);
            mappings.AddStructArrayType<EDBInet>(DataTypeNames.Inet);

            // cidr
#if NET8_0_OR_GREATER
            mappings.AddStructArrayType<IPNetwork>(DataTypeNames.Cidr);
#endif
            mappings.AddStructArrayType<EDBCidr>(DataTypeNames.Cidr);


            return mappings;
        }
    }
}
