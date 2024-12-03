using Helix.Genetics.Reproduction.Asexual.Mutations;
using Helix.Utilities;

namespace Helix.Genetics.Reproduction.Asexual
{
    public class AsexualReproduction
    {
        private readonly (double probability, IMutationStrategy strategy)[] _strategies;
        
        public AsexualReproduction()
        {
            _strategies = new (double, IMutationStrategy)[]
            {
                (0.05, new AddNodeStrategy()),
                (0.1, new AddConnectionStrategy()),
                (0.1, new MutateSignalIntegratorStrategy()),
                (0.1, new MutateActivationFunctionStrategy()),
                (0.8, new MutateWeightsStrategy()),
            };
        }

        public Genome CreateChild(Genome genome)
        {
            var rnd = new DiscreteDistribution(_strategies.Select(x => x.probability).ToArray());
            var idx = rnd.Sample();
            var strategy = _strategies[idx].strategy;

            //Console.WriteLine("Mutator: " + strategy.GetType().Name);

            var child = GenomeFactory.CreateFrom(genome);

            strategy.Mutate(child);

            return child;
        }
    }
}
