
namespace pcap2latex
{
    public class CommandCompleteMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        public string Message { get; private set; } = "";

        internal static CommandCompleteMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            var message = new CommandCompleteMessage(messageCode, len);
            message.Message = reader.ReadNullTerminatedString(len);

            return message;
        }
    }
}