using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcap2latex.Templates;

public partial class Unknown(UnknownMessage message) : ITextTransformer
{
    public char Code { get; } = message.Code;
    public int Length { get; } = message.Length;
}
