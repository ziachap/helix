using Helix.Genetics.Reproduction;

namespace Helix.Genetics
{
    public static class GenomeFactory
    {
        private static uint _genomeId = 0;
        public static uint NextGenomeId() => Interlocked.Increment(ref _genomeId);

        // TODO
        private static readonly Random Random = new Random();

        public static Genome Create(int inputs, int outputs)
        {
            var nodeId = 0;
            int NextNodeId() => Interlocked.Increment(ref nodeId);

            var inputNodes = new List<InputDescriptor>();
            for (var i = 0; i < inputs; i++)
            {
                inputNodes.Add(new InputDescriptor { Id = NextNodeId() });
            }

            //inputNodes[0].Label = "POS";
            //inputNodes[1].Label = "TIME";
            //inputNodes[2].Label = "1";
            //inputNodes[3].Label = "OPEN_PROFIT";
            //inputNodes[4].Label = "RSI_1";
            //inputNodes[5].Label = "RSI_2";

            var outputNodes = new List<OutputNeuronDescriptor>();
            for (var i = 0; i < outputs; i++)
            {
                outputNodes.Add(new OutputNeuronDescriptor()
                {
                    Id = NextNodeId(),
                    ActivationFunction = ActivationFunction.LogisticApproximantSteep,
                    AggregationFunction = AggregationFunction.Sum
                });
            }

            //outputNodes[0].Label = "SPREAD";
            //outputNodes[1].Label = "SKEW";

            Innovation.BoostNodeInnovationId(outputNodes.Last().Id);

            var inputRandom = inputNodes[Random.Next(0, inputs)];
            var outputRandom = outputNodes[Random.Next(0, outputs)];
            var connId = Innovation.ConnectionInnovationId(inputRandom.Id, outputRandom.Id);

            // TODO: Choose random connections according to a distribution
            var connections = new List<Connection>
            {
                new Connection(connId, inputRandom.Id, outputRandom.Id, 0.8, ConnectionIntegrator.Aggregate),
            };

            var genome = new Genome
            {
                Id = NextGenomeId(),
                MetaParameters = new Dictionary<string, double>(),
                Inputs = inputNodes,
                HiddenNeurons = new List<HiddenNeuronDescriptor>(),
                OutputNeurons = outputNodes,
                Connections = connections
            };

            if (genome.InvalidInnovations())
            {

            }

            return genome;
        }

        public static Genome CreateFrom(Genome genome)
        {
            // TODO: Would be nice if there was a more automatic way of deep cloning everything
            // This function could be easily forgotten about.

            var inputNodes = genome.Inputs.Select(x => x.Clone()).ToList();
            var hiddenNodes = genome.HiddenNeurons.Select(x => x.Clone()).ToList();
            var outputNodes = genome.OutputNeurons.Select(x => x.Clone()).ToList();
            
            var connections = genome.Connections
                .Select(x => x.Clone())
                .ToList();

            return new Genome
            {
                Id = NextGenomeId(),
                MetaParameters = genome.MetaParameters.ToDictionary(x => x.Key, x => x.Value),
                Inputs = inputNodes,
                HiddenNeurons = hiddenNodes,
                OutputNeurons = outputNodes,
                Connections = connections
            };
        }
    }
}
