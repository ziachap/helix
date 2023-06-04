using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helix
{
    public class Genome
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Mutatable parameters that are not reflected in the neural network but are applied to the task directly.
        /// </summary>
        public Dictionary<string, double> MetaParameters { get; set; } = new Dictionary<string, double>();

        public List<InputDescriptor> Inputs { get; set; } = new List<InputDescriptor>();

        public List<NeuronDescriptor> HiddenNeurons { get; set; } = new List<NeuronDescriptor>();

        public List<NeuronDescriptor> OuputNeurons { get; set; } = new List<NeuronDescriptor>();

        public List<Connection> Connections { get; set; } = new List<Connection>();
    }

    public static class GenomeExtensions
    {
        public static int[] AllNodeIds(this Genome genome)
        {
            var inputIds = genome.Inputs.Select(x => x.Id);
            var hiddenIds = genome.HiddenNeurons.Select(x => x.Id);
            var outputIds = genome.OuputNeurons.Select(x => x.Id);
            return inputIds.Concat(hiddenIds).Concat(outputIds).ToArray();
        }
    }

    public class Connection
    {
        public Connection()
        {
        }

        public Connection(int sourceIdx, int destinationIdx, double weight, SignalIntegrator integrator)
        {
            SourceIdx = sourceIdx;
            DestinationIdx = destinationIdx;
            Weight = weight;
            SignalIntegrator = integrator;
        }

        public int SourceIdx { get; set; }
        public int DestinationIdx { get; set; }
        public double Weight { get; set; }
        public SignalIntegrator SignalIntegrator { get; set; }
    }

    public class InputDescriptor
    {
        public int Id { get; set; }
    }

    public class NeuronDescriptor
    {
        public int Id { get; set; }

        public ActivationFunction ActivationFunction { get; set; }
    }

    public enum SignalIntegrator
    {
        Additive,
        Multiplicative
    }

    public enum ActivationFunction
    {
        ReLU,
        LogisticApproximantSteep,
        Sine,
    }
}
