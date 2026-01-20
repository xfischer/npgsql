using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using System;

namespace EnterpriseDB.EDBClient;

internal class EDBAQConverterFactory() : PgTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver? CreateArrayResolver() => new EDBAQTypeInfoArrayResolver();
    public override IPgTypeInfoResolver CreateResolver() => new EDBAQTypeInfoResolver();

    private class EDBAQTypeInfoResolver() : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddType<EDBAQMessageProperties>("dbms_aq.message_properties_t",
                (options, mapping, _) => mapping.CreateInfo(options, new EDBAQMessagePropertiesTextConverter(options.TextEncoding, isArray: false)),
                isDefault: true);
            mappings.AddType<EDBAQMessageProperties>("message_properties_t",
                (options, mapping, _) => mapping.CreateInfo(options, new EDBAQMessagePropertiesTextConverter(options.TextEncoding, isArray: false)),
                isDefault: true);

            return mappings;
        }
    }

    // See NetTopology for implementation of array resolver
    //
    //private class EDBAQTypeInfoArrayResolver() : EDBAQTypeInfoResolver(), IPgTypeInfoResolver
    //{
    //    TypeInfoMappingCollection? _mappings;
    //    protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

    //    public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
    //        => Mappings.Find(type, dataTypeName, options);

    //    static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
    //    {
    //        mappings.AddArrayType<EDBAQMessageProperties>("dbms_aq.message_properties_t");
    //        mappings.AddArrayType<EDBAQMessageProperties>("message_properties_t");

    //        return mappings;
    //    }
    //}

    private class EDBAQTypeInfoArrayResolver() : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddType<EDBAQMessageProperties>("dbms_aq.message_properties_t",
                (options, mapping, _) => mapping.CreateInfo(options, new EDBAQMessagePropertiesTextConverter(options.TextEncoding, isArray: true)),
                isDefault: true);
            mappings.AddType<EDBAQMessageProperties>("message_properties_t",
                (options, mapping, _) => mapping.CreateInfo(options, new EDBAQMessagePropertiesTextConverter(options.TextEncoding, isArray: true)),
                isDefault: true);
            mappings.AddArrayType<EDBAQMessageProperties>("dbms_aq.message_properties_t");
            mappings.AddArrayType<EDBAQMessageProperties>("message_properties_t");

            return mappings;
        }
    }    

    sealed class EDBAQMessagePropertiesTextConverter(System.Text.Encoding encoding, bool isArray) : StringBasedTextConverter<EDBAQMessageProperties>(encoding)
    {
        public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
        {
            bufferRequirements = BufferRequirements.None;
            return isArray ? true : format is DataFormat.Text;
        }
        protected override EDBAQMessageProperties ConvertFrom(string value)
        {
            if (isArray)
                throw new NotSupportedException("EnterpriseDB: Array conversion is not supported for EDBAQMessageProperties.");

            EDBAQMessageProperties result = new();
            return result.ToObjectParam(value);
        }
        protected override ReadOnlyMemory<char> ConvertTo(EDBAQMessageProperties value)
        {
            if (isArray)
                throw new NotSupportedException("EnterpriseDB: Array conversion is not supported for EDBAQMessageProperties.");

            return value.ToTextParam().AsMemory();
        }
    }
}


