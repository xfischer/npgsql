using BenchmarkDotNet.Attributes;

namespace EDBBenchmark;

[Config(typeof(EDBManualConfig))]
public class GetFieldValue
{
    readonly NpgsqlConnection _conn;
    readonly NpgsqlCommand _cmd;
    readonly NpgsqlDataReader _reader;

    public GetFieldValue()
    {
        _conn = BenchmarkEnvironment.OpenConnection();
        _cmd = new NpgsqlCommand("SELECT 0, 'str'", _conn);
        _reader = _cmd.ExecuteReader();
        _reader.Read();
    }

    [Benchmark]
    public void NullableField() => _reader.GetFieldValue<int?>(0);

    [Benchmark]
    public void ValueTypeField() => _reader.GetFieldValue<int>(0);

    [Benchmark]
    public void ReferenceTypeField() => _reader.GetFieldValue<string>(1);

    [Benchmark]
    public void ObjectField() => _reader.GetFieldValue<object>(1);

}