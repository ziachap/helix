using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Helix
{
    internal static class GenomeFactory
    {
        private static uint _genomeId = 0;
        private static uint NextGenomeId() => Interlocked.Increment(ref _genomeId);
        
        public static Genome Create(int inputs, int outputs)
        {
            var nodeId = 0;

            int NextNodeId() => Interlocked.Increment(ref nodeId);

            var inputNodes = new List<InputDescriptor>();
            for (var i = 0; i < inputs; i++)
            {
                inputNodes.Add(new InputDescriptor { Id = NextNodeId() });
            }

            var outputNodes = new List<NeuronDescriptor>();
            for (var i = 0; i < outputs; i++)
            {
                outputNodes.Add(new NeuronDescriptor()
                {
                    Id = NextNodeId(),
                    ActivationFunction = ActivationFunction.ReLU,
                });
            }

            // TODO: Choose random connections according to a distribution
            var connections = new List<Connection>
            {
                new Connection(1, 3, 0.8, SignalIntegrator.Additive), 
            };

            var genome = new Genome
            {
                Id = NextGenomeId(),
                MetaParameters = new Dictionary<string, double>(),
                Inputs = inputNodes,
                HiddenNeurons = new List<NeuronDescriptor>(),
                OuputNeurons = outputNodes,
                Connections = connections
            };

            return genome;
        }

        public static Genome CreateFrom(Genome genome)
        {
            // TODO: Would be nice if there was a more automatic way of deep cloning everything
            // This function could be easily forgotten about.

            var inputNodes = genome.Inputs.Select(x => new InputDescriptor
            {
                Id = x.Id
            }).ToList();
            
            var hiddenNodes = genome.HiddenNeurons.Select(x => new NeuronDescriptor()
            {
                Id = x.Id,
                ActivationFunction = x.ActivationFunction
            }).ToList();

            var outputNodes = genome.OuputNeurons.Select(x => new NeuronDescriptor()
            {
                Id = x.Id,
                ActivationFunction = x.ActivationFunction
            }).ToList();
            
            var connections = genome.Connections
                .Select(x => new Connection(x.SourceIdx, x.DestinationIdx, x.Weight, x.SignalIntegrator))
                .ToList();

            return new Genome
            {
                Id = NextGenomeId(),
                MetaParameters = genome.MetaParameters.ToDictionary(x => x.Key, x => x.Value),
                Inputs = inputNodes,
                HiddenNeurons = hiddenNodes,
                OuputNeurons = outputNodes,
                Connections = connections
            };
        }
    }
}
