using pcap2latex.Model;
using pcap2latex.Serialization;
using pcap2latex.Templates;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace pcap2latex;

public static class PdmlService
{
    public static IEnumerable<PostgresPacket> ConvertPdmlToPcap(string pdmlInputFile)
    {
        using var xmlReader = XmlReader.Create(pdmlInputFile);
        var pdmlFile = new XmlSerializer(typeof(Pdml)).Deserialize(xmlReader) as Pdml; // TODO process with XmlReader for better memory consumption
        if (pdmlFile == null)
            throw new InvalidDataException("Invalid pdml file!");

        var pgSqlPackets = pdmlFile.Packet.Where(p => p.Proto.Exists(proto => PdmlHelper.IsPgsqlProto(proto))).ToList();
        if (pgSqlPackets.Count == 0)
            yield break;

        int packetIndex = 1;
        foreach (var p in pgSqlPackets)
        {
            var protos = p.Proto.Where(p => PdmlHelper.IsPgsqlProto(p)).ToList();
            PostgresPacket packet = new PostgresPacket()
            {
                IsFrontEnd = PdmlHelper.IsFrontEnd(protos) ?? false,
                PacketIndex = packetIndex
            };

            foreach (var proto in protos)
            {
                var messageCode = ParseMessageCode(proto);
                var message = PostgresMessages.GetMessage(messageCode, packet.IsFrontEnd);
                if (message == null)
                {
                    Console.WriteLine($"No message definition found for code '{messageCode}'");
                    continue;
                }

                var pgMessage = ProcessPostGresMessage(message.Value, proto);
                if (pgMessage != null)
                    packet.Messages.Add(pgMessage);
            }

            yield return packet;

            packetIndex++;
        }
    }

    private static char ParseMessageCode(Proto proto)
    {
        var typeFieldValue = proto.Fields.First(f => f.Name == $"{proto.Name}.type").Value;
        if (typeFieldValue == null)
            return '?';
        else
            return (char)short.Parse(typeFieldValue, NumberStyles.AllowHexSpecifier);
    }

    private static PostgresMessageBase ProcessPostGresMessage(PostgresMessage message, Proto proto)
    {
        var protoSize = int.Parse(proto.Size);

        try
        {
            PostgresMessageBase? pgMessage = message.Name switch
            {
                nameof(Parse) => ParseMessage.Read(message.code, proto),
                nameof(Bind) => BindMessage.Read(message.code, proto),
                nameof(Describe) => DescribeMessage.Read(message.code, proto),
                nameof(Execute) => ExecuteMessage.Read(message.code, proto),
                nameof(ExecuteOut) => ExecuteOutMessage.Read(message.code, proto),
                nameof(Sync) => SyncMessage.Read(message.code, proto),
                nameof(Query) => QueryMessage.Read(message.code, proto),
                nameof(ParseOut) => ParseOutMessage.Read(message.code, proto),
                nameof(NoData) => NoDataMessage.Read(message.code, proto),
                nameof(BindComplete) => BindCompleteMessage.Read(message.code, proto),
                nameof(ParseComplete) => ParseCompleteMessage.Read(message.code, proto),
                nameof(ParameterDescription) => ParameterDescriptionMessage.Read(message.code, proto),
                nameof(RowDescription) => RowDescriptionMessage.Read(message.code, proto),
                nameof(ReadyForQuery) => ReadyForQueryMessage.Read(message.code, proto),
                nameof(DescribeOut) => DescribeOutMessage.Read(message.code, proto),
                nameof(OutDescription) => OutDescriptionMessage.Read(message.code, proto),
                nameof(DataRow) => DataRowMessage.Read(message.code, proto),
                nameof(CommandComplete) => CommandCompleteMessage.Read(message.code, proto),
                nameof(NoticeResponse) => NoticeResponseMessage.Read(message.code, proto),
                "ParamData" => SendOutTupleMessage.Read(message.code, proto),
                nameof(Terminate) => TerminateMessage.Read(message.code, proto),
                "StartupMessage" when protoSize == 8 => SSLRequestMessage.Read(message.code, proto),
                "StartupMessage" when protoSize == 1 => SSLResponseMessage.Read(message.code),
                "StartupMessage" when protoSize > 8 => StartupMessageMessage.Read(message.code, proto),
                "AuthenticationRequest" => AuthenticationMessage.Read(message.code, proto),
                //nameof(Password) => LastAuthPacket!.ReadResponseMessage(messageCode, reader),
                nameof(ParameterStatus) => ParameterStatusMessage.Read(message.code, proto),
                nameof(BackendKeyData) => BackendKeyDataMessage.Read(message.code, proto),
                _ => null,
            };

            return pgMessage;
        }
        catch (Exception)
        {
            Console.WriteLine($"Message not supported for code: '{message.Name}'");
            return null;
        }
    }
}
