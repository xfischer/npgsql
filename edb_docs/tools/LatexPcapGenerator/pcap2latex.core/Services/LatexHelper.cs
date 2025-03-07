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
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("#", "\\#")
            .Replace("$", "\\$")
            .Replace("%", "\\%")
            .Replace("&", "\\&")
            .Replace("_", "\\_");

    }

    public static string TrimUnescape(string? str, int maxLength)
    {
        if (str == null)
            return string.Empty;

        if (str.Length < maxLength)
            return Unescape(str);
        return Unescape(str[..maxLength]) + "$\\cdots$";
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

    public static string GetProtoDirectionText(bool? isFrontEnd)
    {
        if (isFrontEnd == null)
            return "Unkown";

        return isFrontEnd.Value ? "\\underline{FrontEnd}$\\longrightarrow$BackEnd" : "FrontEnd$\\longleftarrow$\\underline{BackEnd}";
    }


    [Conditional("DEBUG")]
    public static void AppendLineIfDebug(this StringBuilder builder, string content)
    {
        builder.AppendLine(content);
    }

}
