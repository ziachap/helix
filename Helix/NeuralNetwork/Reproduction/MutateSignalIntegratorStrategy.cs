using Helix.Utilities;
using System;
using System.Runtime.CompilerServices;

namespace Helix.NeuralNetwork.Reproduction;

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
        // Find out which connections are eligible to have their signal integrator changed
        var canBeMultiplicative = GetConnectionsEligibleForMultiplicativeIntegrator(genome.Connections);
        var canBeAdditive = GetConnectionsEligibleForAdditiveIntegrator(genome.Connections);
        var eligibleConnections = canBeMultiplicative.Concat(canBeAdditive).ToArray();

        if (eligibleConnections.Length == 0) return;

        // Select a random connection
        var idx = _random.Next(0, eligibleConnections.Length);
        var connection = eligibleConnections[idx];

        // Change the signal integrator
        connection.SignalIntegrator = connection.SignalIntegrator switch
        {
            SignalIntegrator.Additive => SignalIntegrator.Multiplicative,
            SignalIntegrator.Multiplicative => SignalIntegrator.Additive,
            _ => connection.SignalIntegrator
        };

        // Randomize the weight
        connection.Weight = _gaussian.Sample();
    }

    private IEnumerable<Connection> GetConnectionsEligibleForMultiplicativeIntegrator(List<Connection> connections)
    {
        return connections
            .Where(x => x.SignalIntegrator == SignalIntegrator.Additive && HasAdditiveSiblings(x));

        bool HasAdditiveSiblings(Connection c) 
            => Siblings(c).Any(x => x.SignalIntegrator == SignalIntegrator.Additive);

        IEnumerable<Connection> Siblings(Connection c) 
            => connections.Where(x => x.DestinationIdx == c.DestinationIdx);
    }

    private IEnumerable<Connection> GetConnectionsEligibleForAdditiveIntegrator(List<Connection> connections)
    {
        return connections
            .Where(x => x.SignalIntegrator == SignalIntegrator.Multiplicative);
    }
}