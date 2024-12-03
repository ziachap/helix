namespace Helix.Genetics.Reproduction.Asexual.Mutations;

internal interface IMutationStrategy
{
    void Mutate(Genome genome);
}

internal class DeleteConnectionStrategy : IMutationStrategy
{
    public void Mutate(Genome genome)
    {
    }
}