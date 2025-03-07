namespace pcap2latex;

public interface ITextTransformer
{
    string TransformText();

    float EstimateBytefieldRowCount() => 1f;
}
