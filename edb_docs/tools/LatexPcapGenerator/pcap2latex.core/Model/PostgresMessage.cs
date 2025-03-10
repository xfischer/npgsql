namespace pcap2latex;

public record struct PostgresMessage(char Code, string Name, bool IsFrontEnd);
