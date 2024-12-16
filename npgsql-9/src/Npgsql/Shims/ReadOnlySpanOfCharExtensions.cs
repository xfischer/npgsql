using System;
using System.Runtime.CompilerServices;

namespace EnterpriseDB.EDBClient.Netstandard20;

static class ReadOnlySpanOfCharExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseInt(this ReadOnlySpan<char> span)
        => int.Parse(span
#if NETSTANDARD2_0 || NETFRAMEWORK // EnterpriseDB (NETFRAMEWORK)
                    .ToString()
#endif
        );
}