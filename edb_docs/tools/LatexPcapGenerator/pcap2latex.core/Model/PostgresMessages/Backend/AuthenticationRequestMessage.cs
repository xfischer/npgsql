using System.Diagnostics;

namespace pcap2latex;

public class AuthenticationMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public int AuthenticationType { get; }

    internal static AuthenticationMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();

        if (len == 8)
        {
            var intData = reader.ReadInt32();
            return intData switch
            {
                0 => new AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationOK"),
                2 => new AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationKerberosV5"),
                3 => new AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationClearText"),
                7 => new AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationGSS"),
                9 => new AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationSSPI"),
                _ => throw new InvalidDataException($"Invalid Authentication packet! Got value {intData} as 2nd int32 field (first was {len} length).")
            };
        }
        else if (len == 12)
        {
            return AuthenticationMD5PasswordMessage.Read(messageCode, len, reader);
        }
        else
        {
            var intData = reader.ReadInt32();
            return intData switch
            {
                8 => new AuthenticationGSSContinueMessage(messageCode, len, intData, reader.ReadBytes(len - 4 - 4)), // read bytes left: total - len(4) - intdata(4)
                10 => AuthenticationSASLMessage.Read(messageCode, len, intData, reader),
                11 => new AuthenticationSASLContinueMessage(messageCode, len, intData, reader.ReadBytes(len - 4 - 4)),
                12 => new AuthenticationSASLFinalMessage(messageCode, len, intData, reader.ReadBytes(len - 4 - 4)),
                _ => throw new InvalidDataException($"Invalid Authentication packet! Got value {intData} as 2nd int32 field. (first was {len} length)")
            };
        }

        throw new InvalidDataException("Invalid Authentication packet!");
    }
}

public class AuthenticationGSSContinueMessage(char messageCode, int len, int intData, byte[] bytes) : AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationGSSContinue")
{
    public byte[] AuthData { get; } = bytes;

    internal override IPostgresMessage ReadResponseMessage(char code, PcapBinaryReader reader)
    {
        throw new NotImplementedException();
    }
}

public class AuthenticationSASLContinueMessage(char messageCode, int len, int intData, byte[] bytes) : AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationSASLContinue")
{
    public byte[] SASLData { get; } = bytes;

    internal override IPostgresMessage ReadResponseMessage(char code, PcapBinaryReader reader) => SASLResponseMessage.Read(code, reader);
}

public class AuthenticationSASLFinalMessage(char messageCode, int len, int intData, byte[] bytes) : AuthenticationGenericMessage(messageCode, len, intData, "AuthenticationSASLFinal")
{
    public byte[] SASLOutcome { get; } = bytes;

    internal override IPostgresMessage ReadResponseMessage(char code, PcapBinaryReader reader) => throw new NotImplementedException();

}
public class SASLResponseMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public byte[] AuthData { get; private set; } = [];

    internal static SASLResponseMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new SASLResponseMessage(messageCode, len)
        {
            AuthData = reader.ReadBytes(len - 4)
        };

        return packet;
    }
}

public class AuthenticationGenericMessage(char code, int length, int data, string commonName) : AuthenticationMessage(code, length)
{
    internal virtual IPostgresMessage ReadResponseMessage(char code, PcapBinaryReader reader) { throw new NotImplementedException(); }

    public int Data { get; } = data;
    public string AuthenticationName { get; } = commonName;
}

public class AuthenticationMD5PasswordMessage(char code, int length) : AuthenticationGenericMessage(code, length, 0, "AuthenticationMD5Password")
{
    internal static AuthenticationMD5PasswordMessage Read(char messageCode, int length, PcapBinaryReader reader)
    {
        var packet = new AuthenticationMD5PasswordMessage(messageCode, length)
        {
            MD5Check = reader.ReadInt32()
        };
        Debug.Assert(packet.MD5Check == 5);
        packet.Salt = reader.ReadBytes(4);
        return packet;
    }

    internal override IPostgresMessage ReadResponseMessage(char code, PcapBinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public int MD5Check { get; private set; }
    public byte[] Salt { get; private set; } = [];
}

public class AuthenticationSASLMessage(char code, int length, int intData) : AuthenticationGenericMessage(code, length, intData, "AuthenticationSASL")
{
    public List<string> Mechanisms { get; } = [];

    internal static AuthenticationSASLMessage Read(char messageCode, int length, int intData, PcapBinaryReader reader)
    {
        var packet = new AuthenticationSASLMessage(messageCode, length, intData);

        string mechanism;
        do
        {
            mechanism = reader.ReadNullTerminatedString(length);
            if (!string.IsNullOrEmpty(mechanism))
            {
                packet.Mechanisms.Add(mechanism);
            }
        }
        while (mechanism != "");

        return packet;
    }

    internal override IPostgresMessage ReadResponseMessage(char code, PcapBinaryReader reader) => SASLInitialResponseMessage.Read(code, reader);
}
public class SASLInitialResponseMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string Mechanism { get; private set; } = "";
    public int InitialResponseLength { get; private set; }
    public byte[] InitialResponse { get; private set; } = [];

    internal static SASLInitialResponseMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new SASLInitialResponseMessage(messageCode, len)
        {
            Mechanism = reader.ReadNullTerminatedString(len),
            InitialResponseLength = reader.ReadInt32()
        };
        if (packet.InitialResponseLength > 0)
        {
            packet.InitialResponse = reader.ReadBytes(packet.InitialResponseLength);
        }

        return packet;
    }
}