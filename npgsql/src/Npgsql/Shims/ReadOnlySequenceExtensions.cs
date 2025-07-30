namespace System.Buffers;

static class ReadOnlySequenceExtensions
{
    public static ReadOnlySpan<T> GetFirstSpan<T>(this ReadOnlySequence<T> sequence)
    {
#if NETSTANDARD || NETFRAMEWORK // EnterpriseDB (NETFRAMEWORK)
        return sequence.First.Span;
# else
        return sequence.FirstSpan;
#endif
    }
}
