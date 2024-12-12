using System.Collections.ObjectModel;

namespace pcap2latex;

public class PcapReadState
{
    public ushort Port { get; internal set; }
    public byte[]? PreviousBufferLeftover { get; internal set; } = null;
    public RowDescriptionMessage? LastRowDescription { get; internal set; }
    public OutDescriptionMessage? LastOutDescription { get; internal set; }


    private readonly Dictionary<int, bool> _sslRequestedFromClient = new();
    public bool SSLRequestedFromClient(int port)
    {
        if (_sslRequestedFromClient.TryGetValue(port, out var ssl))
            return ssl;
        return false;
    }
    private readonly Dictionary<int, AuthenticationGenericMessage?> _lastAuthPacket = new();
    public AuthenticationGenericMessage? PopLastAuthPacket(int port)
    {
        if (_lastAuthPacket.TryGetValue(port, out var message))
        {
            _lastAuthPacket.Remove(port);
            return message;
        }
        return null;
    }
    public void SetLastAuthPacket(int port, AuthenticationGenericMessage? message)
    {
        _lastAuthPacket[port] = message;
    }
    public void SetSSLRequestedFromClient(int port, bool value)
    {
        _sslRequestedFromClient[port] = value;
    }

}
