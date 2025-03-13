
namespace pcap2latex;

public interface IPcapToLatexService
{
    GenerationState PcapToLaTeX(IEnumerable<PostgresPacket> pgSqlPackets, string latexOutputFile, bool standalone = true);
    GenerationState PcapToLaTeX_MultipleFiles(IEnumerable<PostgresPacket> pgSqlPackets, string latexOutputDirectory);
}