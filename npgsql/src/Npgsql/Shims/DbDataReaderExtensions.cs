#if NETSTANDARD2_0 || NETFRAMEWORK // EnterpriseDB (NETFRAMEWORK)

#pragma warning disable 1591

using System.Data.Common;

// ReSharper disable once CheckNamespace
namespace System.Data
{
    static class DataReaderExtensions
    {
        public static bool IsDBNull(this DbDataReader reader, string name)
            => reader.IsDBNull(reader.GetOrdinal(name));
    }
}
#endif
