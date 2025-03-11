using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace pcap2latex;



public sealed class PcapToLatexOptions
{    
    /// <summary>
    /// Delegate called to provide additionnal template for a given <see cref="PostgresMessageBase"/>.
    /// Implementers should return an <see cref="ITextTransformer"/> instance or <c>null</c> when message should be transformed using default transformer
    /// </summary>
    /// <remarks>Any template returned by this function will take precedence over the default template.</remarks>
    public Func<PostgresMessageBase, ITextTransformer?>? CustomTemplateProvider;

    public Func<string?, GenerationState, ITextTransformer?>? CustomHeaderProvider;
}
