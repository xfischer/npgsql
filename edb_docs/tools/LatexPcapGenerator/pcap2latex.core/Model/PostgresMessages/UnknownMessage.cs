using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcap2latex;
public class UnknownMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public required byte[] Data { get; set; }

    internal static UnknownMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var data = reader.ReadBytes(len - 4); // read until end
        var message = new UnknownMessage(pgMessage, len) { Data = data };

        return message;
    }
}