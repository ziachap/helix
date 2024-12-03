using Helix.Utilities;

namespace Helix.Genetics.Reproduction.Asexual.Mutations;

internal class MutateWeightsStrategy : IMutationStrategy
{
    private readonly DiscreteDistribution _selectionDistribution;
    private readonly DiscreteDistribution _weightStrategyDistribution;
    private readonly GaussianDistribution _gaussianDistribution;
    private readonly Random _random;

    public MutateWeightsStrategy()
    {
        _selectionDistribution = new DiscreteDistribution(new double[]
        {
            12, 4, 2, 1
        });

        _weightStrategyDistribution = new DiscreteDistribution(new double[]
        {
            8, 2
        });

        _gaussianDistribution = new GaussianDistribution();
        _random = new Random();
    }

    public void Mutate(Genome genome)
    {
        var numberOfConnections = _selectionDistribution.Sample() + 1;

        //Console.WriteLine("  numberOfConnections: " + numberOfConnections);

        var connections = genome.Connections.OrderBy(x => Guid.NewGuid()).Take(numberOfConnections);

        foreach (var connection in connections)
        {
            var strategy = _weightStrategyDistribution.Sample();

            //Console.WriteLine("  strategy: " + (strategy == 0 ? "delta shift" : "randomize"));

            switch (strategy)
            {
                case 0:
                    // Delta shift
                    var delta = _gaussianDistribution.Sample(0, 0.2);
                    connection.Weight += delta;
                    break;
                case 1:
                    // Random value
                    // TODO: Should this be a uniform or gaussian distribution?
                    //var newWeight = (_random.NextDouble() * 2) - 1;
                    var newWeight = _gaussianDistribution.Sample(0, 1);
                    connection.Weight = newWeight;
                    break;
            }
        }
    }
}