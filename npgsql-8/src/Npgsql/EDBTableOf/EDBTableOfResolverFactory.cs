using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal;

internal class EDBTableOfResolverFactory : PgTypeInfoResolverFactory
{
    private readonly string[] _knownTableOfTypes;

    public EDBTableOfResolverFactory(string[] knownTableOfTypes) => _knownTableOfTypes = knownTableOfTypes;

    public override IPgTypeInfoResolver? CreateArrayResolver() => null;
    public override IPgTypeInfoResolver CreateResolver() => new Resolver(_knownTableOfTypes);
}

internal class Resolver : IPgTypeInfoResolver
{
    private readonly string[] _knownTableOfTypes;

    TypeInfoMappingCollection? _mappings;
    protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(), _knownTableOfTypes);

    public Resolver(string[] knownTableOfTypes) => _knownTableOfTypes = knownTableOfTypes;

    public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
        => Mappings.Find(type, dataTypeName, options);

    static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings, string[] knownTableOfTypes)
    {
        // EnterpriseDB : parameter is seen as INOUT even if it's OUT (EPAS behaviour).
        // Setting supportsWriting to false will raise an exception as parameter Bind is called.
        foreach (var dataTypeName in knownTableOfTypes)
        {
            var matchRequirement = MatchRequirement.DataTypeName;

            mappings.AddType<ArrayList>(dataTypeName,
                (options, mapping, _) => mapping.CreateInfo(options, new BackendTextToArrayConverter(options, dataTypeName), preferredFormat: DataFormat.Text, supportsWriting: true),
                matchRequirement);
        }

        return mappings;
    }
}

internal class BackendTextToArrayConverter : PgConverter<ArrayList>
{
    private readonly PgSerializerOptions _options;
    private readonly string _dataTypeName;
    private PostgresArrayType? _pgType;

    public BackendTextToArrayConverter(PgSerializerOptions options, string dataTypeName)
        : base(customDbNullPredicate: true)
    {
        _options = options;
        _dataTypeName = dataTypeName;
    }

    protected override bool IsDbNullValue(ArrayList? value, ref object? writeState) => value == null;

    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.None;
        if (format != DataFormat.Text)
            return false;

        // Nested table of type should be seen as a pgArrayOfCompositeType : an array with elements matching TABLE OF signature
        if (_options.DatabaseInfo.TryGetPostgresTypeByName(_dataTypeName, out var pgType)
             && pgType is PostgresArrayType pgArrayOfCompositeType)
        {
            _pgType = pgArrayOfCompositeType;
            return true;
        }        

        return false;
    }

    public override Size GetSize(SizeContext context, [DisallowNull] ArrayList value, ref object? writeState) => Size.Unknown;

    internal async override ValueTask<object> ReadAsObject(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        if (_pgType is null)
            throw new InvalidOperationException("Not type found for reading the current value");

        string value;

        if (async)
        {
            var textReader = await reader.GetTextReaderAsync(_options.TextEncoding, cancellationToken).ConfigureAwait(false);
#if !NET7_0_OR_GREATER
            value = await textReader.ReadToEndAsync().ConfigureAwait(false);
#else
            value = await textReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#endif
        }
        else
        {
            value = reader.GetTextReader(_options.TextEncoding).ReadToEnd();
        }

        var arrayList = ArrayBackendToNativeTypeConverter.ToArrayList(value, _options, _pgType);


        return arrayList;
    }

    public override ArrayList Read(PgReader reader) => throw new NotImplementedException();

    public override ValueTask<ArrayList> ReadAsync(PgReader reader, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    internal override ValueTask WriteAsObject(bool async, PgWriter writer, object value, CancellationToken cancellationToken) => throw new NotImplementedException();

    public override void Write(PgWriter writer, [DisallowNull] ArrayList value) => throw new NotImplementedException();

    public override ValueTask WriteAsync(PgWriter writer, [DisallowNull] ArrayList value, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

