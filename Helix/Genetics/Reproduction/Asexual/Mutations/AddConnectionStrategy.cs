using Helix.Utilities;

namespace Helix.Genetics.Reproduction.Asexual.Mutations;

internal class AddConnectionStrategy : IMutationStrategy
{
    private readonly GaussianDistribution _gaussian;
    private readonly Random _random;

    public AddConnectionStrategy()
    {
        _gaussian = new GaussianDistribution();
        _random = new Random();
    }

    public void Mutate(Genome genome)
    {
        var allIds = genome.AllNodeIds();
        var neuronIds = genome.NeuronalNodeIds();

        const int attempts = 10;
        for (var i = 0; i < attempts; i++)
        {
            if (TryCreateConnection(out var connection))
            {
                genome.Connections.Add(connection);
                return;
            }
        }

        // TODO: If random search fails, follow up with a more thorough search.

        if (genome.InvalidInnovations())
        {

        }

        bool TryCreateConnection(out Connection? connection)
        {
            var src = allIds[_random.Next(0, allIds.Length)];
            var dst = neuronIds[_random.Next(0, neuronIds.Length)];

            if (genome.Connections.Any(x => x.SourceId == src && x.DestinationId == dst))
            {
                connection = null;
                return false;
            }

            // TODO: There should maybe be a small chance that this is a multiplicative signal.
            // To be even more thorough, ensure that there are existing additive connections on the dst node first.
            var weight = _gaussian.Sample();
            var id = Innovation.ConnectionInnovationId(src, dst);
            connection = new Connection(id, src, dst, weight, ConnectionIntegrator.Aggregate);
            return true;
        }
    }
}