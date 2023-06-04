using Helix.Utilities;

namespace Helix.NeuralNetwork.Reproduction;

internal class AddNodeStrategy : IMutationStrategy
{
    private readonly GaussianDistribution _gaussian;
    private readonly Random _random;

    public AddNodeStrategy()
    {
        _gaussian = new GaussianDistribution();
        _random = new Random();
    }

    public void Mutate(Genome genome)
    {
        // Select a random connection
        var idx = _random.Next(0, genome.Connections.Count);
        var preConnection = genome.Connections[idx];
        var preConnectionDestination = preConnection.DestinationIdx;

        // Remove the connection
        genome.Connections.RemoveAt(idx);

        // Create the new hidden node
        var maxId = genome.AllNodeIds().Max();
        var node = new NeuronDescriptor()
        {
            Id = maxId + 1,
            ActivationFunction = ActivationFunction.ReLU // TODO: Randomize activation fn
        };

        // Reattach existing connection to the new hidden node
        preConnection.DestinationIdx = node.Id;

        // Create a new connection from hidden node to destination
        var weight = _gaussian.Sample();
        var postConnection = new Connection(node.Id, preConnectionDestination, weight, SignalIntegrator.Additive);

        genome.HiddenNeurons.Add(node);
        genome.Connections.Add(preConnection);
        genome.Connections.Add(postConnection);
    }
}