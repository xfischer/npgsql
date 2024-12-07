using System.Diagnostics;
using System.Text;

namespace pcap2latex;

public static class LatexHelper
{
    public static string Unescape(string str)
    {
        return str
            .Replace("\\", "\\textbackslash ")
            .ReplaceLineEndings(" ")
            .Replace("#", "\\#")
            .Replace("$", "\\$")
            .Replace("%", "\\%")
            .Replace("&", "\\&")
            .Replace("_", "\\_");

    }

    public static string TrimUnescape(string str, int maxLength)
    {
        if (str.Length < maxLength)
            return Unescape(str);
        return Unescape(str.Substring(0, maxLength)) + "$\\cdots$";
    }

    public static T SafeGet<T>(Serialization.Proto proto, int index, Func<Serialization.Field, T> getter)
    {
        if (proto == null || proto.Fields == null || proto.Fields.Count < index || getter == null)
            throw new ArgumentException($"Cannot get proto value {index}");

        return getter(proto.Fields[index]);
    }

    public static string ToFormatString(short format) =>
        format switch
        {
            0 => "Text",
            1 => "Binary",
            _ => "Unknown"
        };
   
    public static string ParamDirection(short value) =>
        value switch
        {
            1 => "IN",
            2 => "OUT",
            3 => "INOUT",
            _ => "??"
        };


    [Conditional("DEBUG")]
    public static void AppendDebugLine(this StringBuilder builder, string content)
    {
        builder.AppendLine(content);
    }

}
