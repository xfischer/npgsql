using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.Postgres;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient
{
    internal class EDBAQResolverFactory : PgTypeInfoResolverFactory
    {
        public override IPgTypeInfoResolver CreateArrayResolver() => null;
        public override IPgTypeInfoResolver CreateResolver() => new Resolver();

        private class Resolver : IPgTypeInfoResolver
        {
            TypeInfoMappingCollection? _mappings;
            protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

            public PgTypeInfo? GetTypeInfo(System.Type type, DataTypeName? dataTypeName, PgSerializerOptions options)
            {
                PgTypeInfo? typeInfo = Mappings.Find(type, dataTypeName, options);
                if (typeInfo is not null)
                {
                    return typeInfo;
                }
                return null;
            }

            static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
            {
                mappings.AddType<string>("dbms_aq.message_properties_t",
                    (options, mapping, _) => mapping.CreateInfo(options, new EDBAQMessagePropertiesStringConverter(options), preferredFormat: DataFormat.Text, supportsWriting: true),
                    MatchRequirement.DataTypeName);

                return mappings;
            }
        }
    }
    
    internal class EDBAQMessagePropertiesStringConverter : PgStreamingConverter<string>
    {
        private PgSerializerOptions options;

        public EDBAQMessagePropertiesStringConverter(PgSerializerOptions options) => this.options = options;

        // msg size    + (oid size    + param length) * numbers of param
        // int               int             int             int
        // sizeof(int) + sizeof(int) * 2 * _memberHandlers.Length
        public override Size GetSize(SizeContext context, [DisallowNull] string value, ref object writeState)
            => options.TextEncoding.GetByteCount(value);

        public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
        {
            bufferRequirements = BufferRequirements.None;
            return format is DataFormat.Text;
        }
        public override string Read(PgReader reader)
        {
            return reader.GetTextReader(options.TextEncoding).ReadToEnd();
        }

        public async override ValueTask<string> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        {
#if NETFRAMEWORK || NETSTANDARD || NET6_0
            return reader.GetTextReader(options.TextEncoding).ReadToEnd();
#else
            return await reader.GetTextReader(options.TextEncoding).ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#endif
        }
        public override void Write(PgWriter writer, [DisallowNull] string value) => Write(async: false, writer, value).GetAwaiter().GetResult();

        public override ValueTask WriteAsync(PgWriter writer, [DisallowNull] string value, CancellationToken cancellationToken = default) => Write(async: true, writer, value, cancellationToken);

        async ValueTask Write(bool async, PgWriter writer, string value, CancellationToken cancellationToken = default)
        {
            int size = options.TextEncoding.GetByteCount(value);

            if (writer.ShouldFlush(sizeof(int) + size))
                await writer.Flush(async, cancellationToken).ConfigureAwait(false);

            if (async)
                await writer.WriteCharsAsync(value.AsMemory(), options.TextEncoding, cancellationToken).ConfigureAwait(false);
            else
                writer.WriteChars(value.AsSpan(), options.TextEncoding);

        }
    }

}
