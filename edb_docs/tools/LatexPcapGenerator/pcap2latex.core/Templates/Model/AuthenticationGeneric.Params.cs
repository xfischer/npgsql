using static pcap2latex.LatexHelper;

namespace pcap2latex;

public partial class AuthenticationGeneric : ITextTransformer
{
    public int Length { get; }
    public string AuthName { get; }
    public int AuthTypeCode { get; }
    public string? Data { get; } = string.Empty;

    public AuthenticationGeneric(AuthenticationGenericMessage message)
    {
        Length = message.Length;
        AuthName = message.AuthenticationName;
        AuthTypeCode = message.Data;

        Data = message switch
        {
            AuthenticationSASLMessage m => TrimUnescape(string.Join(',', m.Mechanisms), 50),
            AuthenticationSASLContinueMessage m => TrimUnescape("SASLData: " + Convert.ToHexStringLower(m.SASLData), 50),
            AuthenticationSASLFinalMessage m => TrimUnescape("Outcome: " + Convert.ToHexStringLower(m.SASLOutcome), 50),
            { AuthenticationName: "AuthenticationOK"} => null,
            _ => null
        };
    }
}
