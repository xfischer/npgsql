using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace pcap2latex;

public sealed class PcapBinaryReader(BinaryReader reader, Encoding encoding) : IDisposable
{
    public uint ReadUInt32() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(reader.ReadUInt32()) : reader.ReadUInt32();
    public int ReadInt32() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(reader.ReadInt32()) : reader.ReadInt32();
    public ushort ReadUInt16() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(reader.ReadUInt16()) : reader.ReadUInt16();
    public short ReadInt16() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(reader.ReadInt16()) : reader.ReadInt16();

    public string ReadNullTerminatedString(int maxLength)
    {
        var array = ArrayPool<byte>.Shared.Rent(maxLength);
        try
        {
            int index = 0;
            byte currentByte = 0;
            while ((currentByte = reader.ReadByte()) != 0)
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

    public void ThrowIfEndOfStream(int len)
    {
        if (reader.BaseStream.Position + len > reader.BaseStream.Length)
            throw new EndOfStreamException($"Not sufficient data to read {len} bytes.");
    }

    public bool HasSufficientData(int len) => reader.BaseStream.Position + len <= reader.BaseStream.Length + 1;

    public byte[] ReadBytes(int count) => reader.ReadBytes(count);

    public char ReadChar() => reader.ReadChar();

    public byte ReadByte() => reader.ReadByte();

    public long Seek(long offset, SeekOrigin origin) => reader.BaseStream.Seek(offset, origin);

    public void Dispose()
    {
        ((IDisposable)reader).Dispose();
    }
}