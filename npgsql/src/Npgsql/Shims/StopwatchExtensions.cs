using System;
using System.Diagnostics;

namespace System.Diagnostics;

#if !NET8_0_OR_GREATER
internal static class StopwatchExtensions
{

    internal static TimeSpan GetElapsedTime(long startingTimestamp)
    => new TimeSpan(Stopwatch.GetTimestamp() - startingTimestamp);

}
#endif
