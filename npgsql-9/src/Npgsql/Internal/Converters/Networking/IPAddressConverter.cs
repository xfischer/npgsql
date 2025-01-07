using System.Net;
using System.Net.Sockets;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class IPAddressConverter : PgBufferedConverter<IPAddress>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
        => EDBInetConverter.CanConvertImpl(format, out bufferRequirements);

    public override Size GetSize(SizeContext context, IPAddress value, ref object? writeState)
        => EDBInetConverter.GetSizeImpl(context, value, ref writeState);

    protected override IPAddress ReadCore(PgReader reader)
        => EDBInetConverter.ReadImpl(reader, shouldBeCidr: false).Address;

    protected override void WriteCore(PgWriter writer, IPAddress value)
        => EDBInetConverter.WriteImpl(
            writer,
            (value, (byte)(value.AddressFamily == AddressFamily.InterNetwork ? 32 : 128)),
            isCidr: false);
}
