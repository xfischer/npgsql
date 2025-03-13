
namespace pcap2latex;

public interface IPcapService
{
    IEnumerable<PostgresPacket> ConvertPcap(string pcapFile, ushort pgsqlPortNumber = 5432);
}