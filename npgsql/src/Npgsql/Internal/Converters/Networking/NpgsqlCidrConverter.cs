using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

#pragma warning disable CS0618 // EDBCidr is obsolete
sealed class EDBCidrConverter : PgBufferedConverter<EDBCidr>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
        => EDBInetConverter.CanConvertImpl(format, out bufferRequirements);

    public override Size GetSize(SizeContext context, EDBCidr value, ref object? writeState)
        => EDBInetConverter.GetSizeImpl(context, value.Address, ref writeState);

    protected override EDBCidr ReadCore(PgReader reader)
    {
        var (ip, netmask) = EDBInetConverter.ReadImpl(reader, shouldBeCidr: true);
        return new(ip, netmask);
    }

    protected override void WriteCore(PgWriter writer, EDBCidr value)
        => EDBInetConverter.WriteImpl(writer, (value.Address, value.Netmask), isCidr: true);
}
#pragma warning restore CS0618
