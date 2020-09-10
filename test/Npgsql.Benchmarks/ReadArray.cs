using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace EnterpriseDB.EDBClient.Benchmarks
{
    [Config(typeof(ReadArrayConfig))]
    public class ReadArray
    {
        EDBConnection _conn = default!;
        EDBCommand _cmd = default!;
        EDBDataReader _reader = default!;

        [GlobalSetup(Target = nameof(ReadIntArray) + "," + nameof(ReadListOfInt))]
        public void GlobalSetupForInt()
            => GlobalSetupImpl(42);

        [GlobalSetup(Target = nameof(ReadStringArray) + "," + nameof(ReadListOfString))]
        public void GlobalSetupForString()
            => GlobalSetupImpl("The Answer to the Ultimate Question of Life, The Universe, and Everything.");

        [GlobalSetup(Target = nameof(ReadIPAddressArray)
                              + "," + nameof(ReadEDBInetArray)
                              + "," + nameof(ReadListOfIPAddress)
                              + "," + nameof(ReadListOfEDBInet))]
        public void GlobalSetupForInet()
            => GlobalSetupImpl(IPAddress.Loopback);

        void GlobalSetupImpl<T>(T initializationValue)
        {
            _conn = BenchmarkEnvironment.OpenConnection();
            _cmd = new EDBCommand("SELECT @p1;", _conn);
            _cmd.Parameters.AddWithValue("@p1", Enumerable.Repeat(initializationValue, NumArrayElements).ToArray());
            _reader = _cmd.ExecuteReader();
            _reader.Read();
        }

        [Params(0, 10, 1000, 100000)]
        public int NumArrayElements { get; set; }

        [GlobalCleanup]
        public void Cleanup()
        {
            _reader.Dispose();
            _cmd.Dispose();
            _conn.Dispose();
        }

        [Benchmark]
        public void ReadIntArray()
            => ReadArrayImpl<int>();

        [Benchmark]
        public void ReadStringArray()
            => ReadArrayImpl<string>();

        [Benchmark]
        // ReSharper disable once InconsistentNaming
        public void ReadIPAddressArray()
            => ReadArrayImpl<IPAddress>();

        [Benchmark]
        public void ReadEDBInetArray() // PSV for IPAddress
            => ReadArrayImpl<ValueTuple<IPAddress, int>>();

        [Benchmark]
        public void ReadListOfInt()
            => ReadListImpl<int>();

        [Benchmark]
        public void ReadListOfString()
            => ReadListImpl<string>();

        [Benchmark]
        // ReSharper disable once InconsistentNaming
        public void ReadListOfIPAddress()
            => ReadListImpl<IPAddress>();

        [Benchmark]
        public void ReadListOfEDBInet() // PSV for IPAddress
            => ReadListImpl<ValueTuple<IPAddress, int>>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReadArrayImpl<T>()
            => _reader.GetFieldValue<T[]>(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReadListImpl<T>()
            => _reader.GetFieldValue<List<T>>(0);

        class ReadArrayConfig : ManualConfig
        {
            public ReadArrayConfig() => Add(StatisticColumn.OperationsPerSecond);
        }
    }
}
