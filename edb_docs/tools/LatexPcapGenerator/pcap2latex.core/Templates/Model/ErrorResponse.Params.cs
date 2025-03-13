namespace pcap2latex.Templates;

public partial class ErrorResponse(ErrorResponseMessage message) : ITextTransformer
{
    public int Length { get; } = message.Length;
    public List<(char FieldType, string Message)> Messages { get; } = message.Fields;
}
