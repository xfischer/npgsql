using System.Diagnostics;

namespace pcap2latex
{
    public class BackendKeyDataMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        public int ProcessId { get; private set; }
        public uint SecretKey { get; private set; }

        internal static BackendKeyDataMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            Debug.Assert(len == 12);
            var message = new BackendKeyDataMessage(messageCode, len);
            message.ProcessId = reader.ReadInt32();
            message.SecretKey = reader.ReadUInt32();

            return message;
        }

        internal static BackendKeyDataMessage Read(char code, Serialization.Proto proto)
        {
            var len = Convert.ToInt16(proto.Fields[1].Value, 16);
            var message = new BackendKeyDataMessage(code, len);
            message.ProcessId = LatexHelper.SafeGet(proto, 3, f => int.Parse(f.Show));
            message.SecretKey = LatexHelper.SafeGet(proto, 4, f => uint.Parse(f.Show));
            return message;
        }
    }
}