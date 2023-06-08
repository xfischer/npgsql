using BenchmarkDotNet.Attributes;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Benchmarks;

[MemoryDiagnoser]
public class ResolveHandler
{
    EDBDataSource? _dataSource;
    TypeMapper _typeMapper = null!;

    [Params(0, 1, 2)]
    public int NumPlugins { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var dataSourceBuilder = new EDBDataSourceBuilder();
        if (NumPlugins > 0)
            dataSourceBuilder.UseNodaTime();
        if (NumPlugins > 1)
            dataSourceBuilder.UseNetTopologySuite();
        _dataSource = dataSourceBuilder.Build();
        _typeMapper = _dataSource.TypeMapper;
    }

    [GlobalCleanup]
    public void Cleanup() => _dataSource?.Dispose();

    [Benchmark]
    public EDBTypeHandler ResolveOID()
        => _typeMapper.ResolveByOID(23); // int4

    [Benchmark]
    public EDBTypeHandler ResolveEDBDbType()
        => _typeMapper.ResolveByEDBDbType(EDBDbType.Integer);

    [Benchmark]
    public EDBTypeHandler ResolveDataTypeName()
        => _typeMapper.ResolveByDataTypeName("integer");

    [Benchmark]
    public EDBTypeHandler ResolveClrTypeNonGeneric()
        => _typeMapper.ResolveByValue((object)8);

    [Benchmark]
    public EDBTypeHandler ResolveClrTypeGeneric()
        => _typeMapper.ResolveByValue(8);
}
