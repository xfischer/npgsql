using System.Diagnostics;
using static pcap2latex.LatexHelper;

namespace pcap2latex.Templates;

public partial class NoticeResponse : ITextTransformer
{
    public int Length { get; }
    public List<(char FieldType, string Message)> Messages { get; }

    public NoticeResponse(NoticeResponseMessage message) {

        Length = message.Length;
        Messages = message.Fields;
    }

    
}
