namespace Helix.Genetics;

public static class GenomeExtensions
{
    public static int[] AllNodeIds(this Genome genome)
    {
        var inputIds = genome.Inputs.Select(x => x.Id);
        var hiddenIds = genome.HiddenNeurons.Select(x => ((NeuronDescriptor)x).Id);
        var outputIds = genome.OutputNeurons.Select(x => x.Id);
        return inputIds.Concat(hiddenIds).Concat(outputIds).ToArray();
    }

    public static int[] NeuronalNodeIds(this Genome genome)
    {
        var hiddenIds = genome.HiddenNeurons.Select(x => ((NeuronDescriptor)x).Id);
        var outputIds = genome.OutputNeurons.Select(x => x.Id);
        return hiddenIds.Concat(outputIds).ToArray();
    }

    public static int TotalNodeCount(this Genome genome)
    {
        var inputs = genome.Inputs.Count;
        var hidden = genome.HiddenNeurons.Count;
        var outputs = genome.OutputNeurons.Count;
        return inputs + hidden + outputs;
    }
}