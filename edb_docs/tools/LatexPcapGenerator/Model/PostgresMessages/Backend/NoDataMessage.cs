namespace pcap2latex
{
    internal class NoDataMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        internal static NoDataMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            var packet = new NoDataMessage(messageCode, len);

            return packet;
        }

        internal static NoDataMessage Read(char messageCode, Serialization.Proto proto)
        {
            var len = Convert.ToInt16(proto.Fields[1].Value, 16);
            var packet = new NoDataMessage(messageCode, len);

            return packet;
        }
    }
}