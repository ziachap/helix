using Helix.Utilities;

namespace Helix.Genetics.Reproduction.Asexual.Mutations;

internal class MutateSignalIntegratorStrategy : IMutationStrategy
{
    private readonly GaussianDistribution _gaussian;
    private readonly Random _random;

    public MutateSignalIntegratorStrategy()
    {
        _gaussian = new GaussianDistribution();
        _random = new Random();
    }

    public void Mutate(Genome genome)
    {
        // Find out which connections are eligible to have their integrator changed
        var canBeMultiplicative = GetConnectionsEligibleForMultiplicativeIntegrator(genome.Connections);
        var canBeAdditive = GetConnectionsEligibleForAdditiveIntegrator(genome.Connections);
        var eligibleConnections = canBeMultiplicative.Concat(canBeAdditive).ToArray();

        if (eligibleConnections.Length == 0) return;

        // Select a random connection
        var idx = _random.Next(0, eligibleConnections.Length);
        var connection = eligibleConnections[idx];

        // Change the integrator
        connection.Integrator = connection.Integrator switch
        {
            ConnectionIntegrator.Aggregate => ConnectionIntegrator.Modulate,
            ConnectionIntegrator.Modulate => ConnectionIntegrator.Aggregate,
            _ => connection.Integrator
        };

        // Randomize the weight
        connection.Weight = _gaussian.Sample();
    }

    private IEnumerable<Connection> GetConnectionsEligibleForMultiplicativeIntegrator(List<Connection> connections)
    {
        return connections
            .Where(x => x.Integrator == ConnectionIntegrator.Aggregate && HasAdditiveSiblings(x));

        bool HasAdditiveSiblings(Connection c) 
            => Siblings(c).Any(x => x.Integrator == ConnectionIntegrator.Aggregate);

        IEnumerable<Connection> Siblings(Connection c) 
            => connections.Where(x => x.SourceId != c.SourceId && x.DestinationId == c.DestinationId);
    }

    private IEnumerable<Connection> GetConnectionsEligibleForAdditiveIntegrator(List<Connection> connections)
    {
        return connections
            .Where(x => x.Integrator == ConnectionIntegrator.Modulate);
    }
}