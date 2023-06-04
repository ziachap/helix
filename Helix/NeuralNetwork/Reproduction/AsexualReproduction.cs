using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Helix.Utilities;

namespace Helix.NeuralNetwork.Reproduction
{
    internal class AsexualReproduction
    {
        private readonly IMutationStrategy[] _strategies = {
            new AddNodeStrategy(),
            new AddConnectionStrategy(),
            new MutateSignalIntegratorStrategy(),
            new MutateActivationFunctionStrategy(),
            new MutateWeightsStrategy(),
        };

        private readonly double[] _probabilities = {
            0.1,
            0.5,
            0.2,
            0.2,
            0.02,
        };

        private readonly DiscreteDistribution _rnd;

        public AsexualReproduction()
        {
            _rnd = new DiscreteDistribution(_probabilities);
        }

        public Genome CreateChild(Genome genome)
        {
            var idx = _rnd.Sample();
            var strategy = _strategies[idx];

            Console.WriteLine("Mutator: " + strategy.GetType().Name);

            var child = GenomeFactory.CreateFrom(genome);

            strategy.Mutate(child);

            return child;
        }
    }
}
