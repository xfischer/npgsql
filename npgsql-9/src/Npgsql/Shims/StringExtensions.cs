using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System;

#if NETFRAMEWORK || NETSTANDARD2_0

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class StringExtensions
{
    public static bool Contains(this string s, string other, StringComparison comparison) => s.IndexOf(other, comparison) >= 0;

    public static string Trim(this string s, char trimChar) => s.Trim(new char[] { trimChar });

}

#endif

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
