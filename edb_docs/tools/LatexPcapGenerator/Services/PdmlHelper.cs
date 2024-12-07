using pcap2latex.Serialization;

namespace pcap2latex;

internal static class PdmlHelper
{
    public static string GetProtoDirectionText(bool? isFrontEnd)
    {
        if (isFrontEnd == null)
            return "Unkown";
        
        return isFrontEnd.Value ? "\\underline{FrontEnd}$\\longrightarrow$BackEnd" : "FrontEnd$\\longleftarrow$\\underline{BackEnd}";
    }

    public static bool? IsFrontEnd(List<Proto>? protos)
    {
        if (protos == null)
        {
            return null;
        }

        foreach (var proto in protos)
        {
            foreach (var f in proto.Fields)
            {
                if (f.Name == $"{proto.Name}.frontend")
                {
                    return f.Show == "True";
                }
            }
        }

        // check is ssl response
        foreach (var proto in protos)
        {
            foreach (var f in proto.Fields)
            {
                if (f.Name == $"{proto.Name}.ssl_response")
                    return false;
            }
        }

        return null;
    }

    public static bool IsPgsqlProto(Proto proto) => proto.Name.StartsWith("pgsql");


}
