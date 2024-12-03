using Helix.Utilities;

namespace Helix.Genetics.Reproduction.Asexual.Mutations;

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
        var existingConnection = genome.Connections[idx];
        
        // Create the new hidden node
        var id = Innovation.HiddenNodeInnovationId(
            existingConnection.SourceId, 
            existingConnection.DestinationId);

        // This covers a particular edge case, for example:
        // 1) Node is added in place of connection 1 => 2
        // 2) Node is given innovation id
        // 3) New connection is added via mutation 1 => 2
        // 4) The new connection is replaced with new node
        // The new node innovation id would conflict with previous node, therefore give it a new id.
        if (genome.HiddenNeurons.Any(x => x.Id == id)) id = Innovation.NextInnovationId();
        
        var node = new HiddenNeuronDescriptor()
        {
            Id = id,
            ActivationFunction = ActivationFunction.ReLU, // TODO: Randomize activation fn?
            AggregationFunction = AggregationFunction.Sum // TODO: Randomize aggregation fn?
        };

        // Create new connection based on existing connection to the new hidden node
        var preId = Innovation.ConnectionInnovationId(existingConnection.SourceId, node.Id);
        var preConnection = new Connection(
            preId, 
            existingConnection.SourceId,
            node.Id,
            existingConnection.Weight, 
            ConnectionIntegrator.Aggregate);
        
        // Create a new connection from hidden node to destination
        var weight = _gaussian.Sample();
        var postId = Innovation.ConnectionInnovationId(node.Id, existingConnection.DestinationId);
        var postConnection = new Connection(
            postId, 
            node.Id, 
            existingConnection.DestinationId,
            weight, 
            existingConnection.Integrator);

        // Remove the connection
        genome.Connections.RemoveAt(idx);

        // Add new genes
        genome.HiddenNeurons.Add(node);
        genome.Connections.Add(preConnection);
        genome.Connections.Add(postConnection);

        if (genome.InvalidInnovations())
        {

        }
    }
}