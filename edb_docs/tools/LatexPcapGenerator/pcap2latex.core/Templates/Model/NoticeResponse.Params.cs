namespace pcap2latex;

public partial class NoticeResponse(NoticeResponseMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public List<(char FieldType, string Message)> Messages { get; } = message.Fields;
}
