namespace Helix.NeuralNetwork.Reproduction;

internal interface IMutationStrategy
{
    void Mutate(Genome genome);
}

internal class MutateWeightsStrategy : IMutationStrategy
{
    public void Mutate(Genome genome)
    {
    }
}

internal class DeleteConnectionStrategy : IMutationStrategy
{
    public void Mutate(Genome genome)
    {
    }
}