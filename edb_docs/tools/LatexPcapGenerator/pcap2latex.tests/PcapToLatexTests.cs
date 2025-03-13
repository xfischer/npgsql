using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace pcap2latex.tests;

public class PcapToLatexTests
{

    private IEnumerable<PostgresPacket> GetTestPackets()
    {
        var pcapOptions = new PcapPostgresOptions();
        pcapOptions.AddDefaultPostgresMessages();
        PcapService pcapService = new PcapService(NullLogger<PcapService>.Instance, Options.Create(pcapOptions));
        return pcapService.ConvertPcap("TestData/extendedQuery.pcapng", pgsqlPortNumber: 5432);
    }

    [Fact]
    public void Convert_Standalone()
    {
        // arrange
        var packets = GetTestPackets();
        var options = new PcapToLatexOptions();
        PcapToLatexService service = new PcapToLatexService(NullLogger<PcapToLatexService>.Instance, Options.Create(options));


        // act
        using var memStream = new MemoryStream();
        var state = service.PcapToLaTeX(packets, memStream, standalone: true);
        memStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(memStream);
        var content = streamReader.ReadToEnd();

        // assert        
        Assert.Equal(3, state.StatsPacketsProcessed);
        Assert.Equal(21, state.StatsMesssagesProcessed);

        Assert.Contains("documentclass[margin=8mm]{standalone}", content);
    }

    [Fact]
    public void Convert_Article()
    {
        // arrange
        var packets = GetTestPackets();
        var options = new PcapToLatexOptions();
        PcapToLatexService service = new PcapToLatexService(NullLogger<PcapToLatexService>.Instance, Options.Create(options));


        // act
        using var memStream = new MemoryStream();
        var state = service.PcapToLaTeX(packets, memStream, standalone: false);
        memStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(memStream);
        var content = streamReader.ReadToEnd();

        // assert        
        Assert.Equal(3, state.StatsPacketsProcessed);
        Assert.Equal(21, state.StatsMesssagesProcessed);

        Assert.Contains("documentclass{article}", content);
    }
}