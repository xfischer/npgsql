using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Not part of the public API
public static class StringExtensions
{
    public static readonly char[] BlankChars = [' ', '\r', '\n', '\f', '\t'];

    /// <summary>
    /// Checks if the string contains a word followed by a blank character.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="word"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ContainsWord(this string query, string word, StringComparison comparison)
    {
        if (query.Length <= word.Length)
            return false;

        var wordStart = query.IndexOf(word, comparison);
        if (wordStart < 0)
            return false;

        if (BlankChars.Contains(query[wordStart + word.Length]))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if the string starts with a word, and is followed by a blank character.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="word"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool StartsWithWord(this string query, string word, StringComparison comparison)
    {
        if (query.Length <= word.Length)
            return false;

        var wordStart = query.IndexOf(word, comparison);
        if (wordStart != 0)
            return false;

        if (BlankChars.Contains(query[wordStart + word.Length]))
            return true;

        return true;
    }

#if NETFRAMEWORK || NETSTANDARD2_0

    public static bool Contains(this string s, string other, StringComparison comparison) => s.IndexOf(other, comparison) >= 0;

    public static bool Contains(this string? s, char c) => s is null ? false : s!.Contains(c.ToString());

#endif
}
#pragma warning restore CS1591 
#pragma warning disable RS0016 // Not part of the public API
