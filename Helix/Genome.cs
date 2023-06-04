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
        /// Mutable parameters that are not reflected in the neural network but are applied to the task directly.
        /// </summary>
        public Dictionary<string, double> MetaParameters { get; set; } = new Dictionary<string, double>();

        public List<InputDescriptor> Inputs { get; set; } = new List<InputDescriptor>();

        public List<NeuronDescriptor> HiddenNeurons { get; set; } = new List<NeuronDescriptor>();

        public List<NeuronDescriptor> OuputNeurons { get; set; } = new List<NeuronDescriptor>();

        public List<Connection> Connections { get; set; } = new List<Connection>();

        public Genome Clone()
        {
            // TODO: Deep clone
            return new Genome()
            {
                MetaParameters = MetaParameters.ToDictionary(x => x.Key, x => x.Value),
                Inputs = Inputs.Select(x => x).ToList(),
                HiddenNeurons = HiddenNeurons.Select(x => x).ToList(),
                OuputNeurons = OuputNeurons.Select(x => x).ToList(),
                Connections = Connections.Select(x => x).ToList(),
            };
        }
    }

    public class Connection
    {
        public Connection()
        {
        }

        public Connection(int sourceIdx, int endIdx, double weight, SignalIntegrator integrator)
        {
            SourceIdx = sourceIdx;
            EndIdx = endIdx;
            Weight = weight;
            SignalIntegrator = integrator;
        }

        public int SourceIdx { get; set; }
        public int EndIdx { get; set; }
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
        LogisticSteep,
        Sine,
    }
}
