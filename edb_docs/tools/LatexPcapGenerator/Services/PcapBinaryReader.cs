using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace pcap2latex;

internal class PcapBinaryReader : BinaryReader
{
    private readonly Encoding encoding;

    public PcapBinaryReader(Stream input, Encoding encoding) : base(input)
    {
        this.encoding = encoding;
    }

    public override uint ReadUInt32() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(base.ReadUInt32()) : base.ReadUInt32();
    public override int ReadInt32() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(base.ReadInt32()) : base.ReadInt32();
    public override ushort ReadUInt16() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(base.ReadUInt16()) : base.ReadUInt16();
    public override short ReadInt16() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(base.ReadInt16()) : base.ReadInt16();

    internal string ReadNullTerminatedString(int maxLength)
    {
        var array = ArrayPool<byte>.Shared.Rent(maxLength);
        try
        {
            int index = 0;
            byte currentByte = 0;
            while ((currentByte = base.ReadByte()) != 0)
            {
                array[index++] = currentByte;
                if (currentByte == 0)
                    return string.Empty;

            }

            if (currentByte == 0)
            {
                return encoding.GetString(array, 0, index);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
        return string.Empty;
    }

    internal void ThrowIfEndOfStream(int len)
    {
        if (base.BaseStream.Position + len > base.BaseStream.Length)
            throw new EndOfStreamException($"Not sufficient data to read {len} bytes.");
    }

    internal bool HasSufficientData(int len) => base.BaseStream.Position + len <= base.BaseStream.Length+1;
}