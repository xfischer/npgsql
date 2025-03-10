using System.Runtime.CompilerServices;

namespace pcap2latex;


public abstract class PostgresMessageBase(PostgresMessage pgMessage, int length)
{
    public char Code => pgMessage.Code;

    public string Name => pgMessage.Name;

    public int Length => length;

    public bool FrontEnd => pgMessage.IsFrontEnd;

    public virtual string GetStringRepresentation() => this.GetType().Name;
}