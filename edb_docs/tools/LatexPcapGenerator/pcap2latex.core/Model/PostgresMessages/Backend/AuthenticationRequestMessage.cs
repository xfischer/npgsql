using System.Diagnostics;

namespace pcap2latex;

public class AuthenticationMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public int AuthenticationType { get; }

    internal static AuthenticationMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();

        if (len == 8)
        {
            var intData = reader.ReadInt32();
            return intData switch
            {
                0 => new AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationOK"),
                2 => new AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationKerberosV5"),
                3 => new AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationClearText"),
                7 => new AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationGSS"),
                9 => new AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationSSPI"),
                _ => throw new InvalidDataException($"Invalid Authentication packet! Got value {intData} as 2nd int32 field (first was {len} length).")
            };
        }
        else if (len == 12)
        {
            return AuthenticationMD5PasswordMessage.Read(pgMessage, len, reader);
        }
        else
        {
            var intData = reader.ReadInt32();
            return intData switch
            {
                8 => new AuthenticationGSSContinueMessage(pgMessage, len, intData, reader.ReadBytes(len - 4 - 4)), // read bytes left: total - len(4) - intdata(4)
                10 => AuthenticationSASLMessage.Read(pgMessage, len, intData, reader),
                11 => new AuthenticationSASLContinueMessage(pgMessage, len, intData, reader.ReadBytes(len - 4 - 4)),
                12 => new AuthenticationSASLFinalMessage(pgMessage, len, intData, reader.ReadBytes(len - 4 - 4)),
                _ => throw new InvalidDataException($"Invalid Authentication packet! Got value {intData} as 2nd int32 field. (first was {len} length)")
            };
        }

        throw new InvalidDataException("Invalid Authentication packet!");
    }
}

public class AuthenticationGSSContinueMessage(PostgresMessage pgMessage, int len, int intData, byte[] bytes) : AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationGSSContinue")
{
    public byte[] AuthData { get; } = bytes;

    internal override PostgresMessageBase ReadResponseMessage(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        throw new NotImplementedException();
    }
}

public class AuthenticationSASLContinueMessage(PostgresMessage pgMessage, int len, int intData, byte[] bytes) : AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationSASLContinue")
{
    public byte[] SASLData { get; } = bytes;

    internal override PostgresMessageBase ReadResponseMessage(PostgresMessage pgMessage, PcapBinaryReader reader) => SASLResponseMessage.Read(pgMessage, reader);
}

public class AuthenticationSASLFinalMessage(PostgresMessage pgMessage, int len, int intData, byte[] bytes) : AuthenticationGenericMessage(pgMessage, len, intData, "AuthenticationSASLFinal")
{
    public byte[] SASLOutcome { get; } = bytes;

    internal override PostgresMessageBase ReadResponseMessage(PostgresMessage pgMessage, PcapBinaryReader reader) => throw new NotImplementedException();

}
public class SASLResponseMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public byte[] AuthData { get; private set; } = [];

    internal static SASLResponseMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new SASLResponseMessage(pgMessage, len)
        {
            AuthData = reader.ReadBytes(len - 4)
        };

        return packet;
    }
}

public class AuthenticationGenericMessage(PostgresMessage pgMessage, int length, int data, string commonName) : AuthenticationMessage(pgMessage, length)
{
    internal virtual PostgresMessageBase ReadResponseMessage(PostgresMessage pgMessage, PcapBinaryReader reader) { throw new NotImplementedException(); }

    public int Data { get; } = data;
    public string AuthenticationName { get; } = commonName;
}

public class AuthenticationMD5PasswordMessage(PostgresMessage pgMessage, int length) : AuthenticationGenericMessage(pgMessage, length, 0, "AuthenticationMD5Password")
{
    internal static AuthenticationMD5PasswordMessage Read(PostgresMessage pgMessage, int length, PcapBinaryReader reader)
    {
        var packet = new AuthenticationMD5PasswordMessage(pgMessage, length)
        {
            MD5Check = reader.ReadInt32()
        };
        Debug.Assert(packet.MD5Check == 5);
        packet.Salt = reader.ReadBytes(4);
        return packet;
    }

    internal override PostgresMessageBase ReadResponseMessage(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public int MD5Check { get; private set; }
    public byte[] Salt { get; private set; } = [];
}

public class AuthenticationSASLMessage(PostgresMessage pgMessage, int length, int intData) : AuthenticationGenericMessage(pgMessage, length, intData, "AuthenticationSASL")
{
    public List<string> Mechanisms { get; } = [];

    internal static AuthenticationSASLMessage Read(PostgresMessage pgMessage, int length, int intData, PcapBinaryReader reader)
    {
        var packet = new AuthenticationSASLMessage(pgMessage, length, intData);

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

    internal override PostgresMessageBase ReadResponseMessage(PostgresMessage pgMessage, PcapBinaryReader reader) => SASLInitialResponseMessage.Read(pgMessage, reader);
}
public class SASLInitialResponseMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public string Mechanism { get; private set; } = "";
    public int InitialResponseLength { get; private set; }
    public byte[] InitialResponse { get; private set; } = [];

    internal static SASLInitialResponseMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new SASLInitialResponseMessage(pgMessage, len)
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