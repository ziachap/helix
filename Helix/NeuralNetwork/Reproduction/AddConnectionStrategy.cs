using Helix.Utilities;

namespace Helix.NeuralNetwork.Reproduction;

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
        var ids = genome.AllNodeIds();

        const int attempts = 10;
        for (var i = 0; i < attempts; i++)
        {
            if (TryCreateConnection(genome.Connections, ids, out var connection))
            {
                genome.Connections.Add(connection);
                return;
            }
        }

        // TODO: If random search fails, follow up with a more thorough search.
    }

    private bool TryCreateConnection(List<Connection> connections, int[] ids, out Connection? connection)
    {
        var src = ids[_random.Next(0, ids.Length)];
        var dst = ids[_random.Next(0, ids.Length)];

        if (connections.Any(x => x.SourceIdx == src && x.DestinationIdx == dst))
        {
            connection = null;
            return false;
        }
        
        // TODO: There should maybe be a small chance that this is a multiplicative signal.
        // To be even more thorough, ensure that there are existing additive connections on the dst node first.
        var weight = _gaussian.Sample();
        connection = new Connection(src, dst, weight, SignalIntegrator.Additive);
        return true;
    }
}