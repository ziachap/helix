namespace Helix.NeuralNetwork.Reproduction;

internal interface IMutationStrategy
{
    Genome Mutate(Genome genome);
}

internal class MutateWeightsStrategy : IMutationStrategy
{
    public Genome Mutate(Genome genome)
    {
        var newGenome = genome.Clone();

        return newGenome;
    }
}

internal class AddConnectionStrategy : IMutationStrategy
{
    public Genome Mutate(Genome genome)
    {
        var newGenome = genome.Clone();

        var newConnection = new Connection(1, 4, 1, SignalIntegrator.Additive);

        newGenome.Connections.Add(newConnection);

        return newGenome;
    }
}
