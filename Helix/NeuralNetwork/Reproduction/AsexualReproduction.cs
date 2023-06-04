using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helix.NeuralNetwork.Reproduction
{
    internal class AsexualReproduction
    {
        private readonly IMutationStrategy[] _strategies = {
            new AddConnectionStrategy(),
            new MutateWeightsStrategy(),
        };

        private readonly Random _random = new Random();

        public Genome CreateChild(Genome genome)
        {
            var strategy = _strategies[0];

            var newGenome = strategy.Mutate(genome);

            return newGenome;
        }
    }
}
