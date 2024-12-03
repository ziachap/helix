using Helix.Genetics.Reproduction;
using System.Reflection.Emit;

namespace Helix.Genetics
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

        public List<HiddenNeuronDescriptor> HiddenNeurons { get; set; } = new List<HiddenNeuronDescriptor>();

        public List<OutputNeuronDescriptor> OutputNeurons { get; set; } = new List<OutputNeuronDescriptor>();

        public List<Connection> Connections { get; set; } = new List<Connection>();

        public Fitness Fitness { get; set; } = new Fitness();
    }

    public class Fitness
    {
        public Fitness()
        {
            Auxillary = new Dictionary<string, double>();
        }

        public double PrimaryFitness { get; set; }

        public Dictionary<string, double> Auxillary { get; set; }
    }

    public class Connection : ITopological, ICloneable<Connection>, IEquatable<Connection>
    {
        public Connection(
            int id, int sourceId, int destinationId, double weight, ConnectionIntegrator integrator)
        {
            Id = id;
            SourceId = sourceId;
            DestinationId = destinationId;
            Weight = weight;
            Integrator = integrator;
        }

        public int Id { get; set; }

        public int SourceId { get; set; }

        public int DestinationId { get; set; }

        public double Weight { get; set; }

        /// <summary>
        /// Describes how this connection is applied to its destination.
        /// </summary>
        public ConnectionIntegrator Integrator { get; set; }

        public Connection Clone() => new Connection(Id, SourceId, DestinationId, Weight, Integrator);

        public override string ToString() => $"{SourceId} => {DestinationId}";

        public bool Equals(Connection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SourceId == other.SourceId && DestinationId == other.DestinationId && Integrator == other.Integrator;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Connection)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceId, DestinationId, (int)Integrator);
        }
    }

    public class InputDescriptor : ICloneable<InputDescriptor>
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public InputDescriptor Clone() => new InputDescriptor()
        {
            Id = Id, Label = Label
        };

        public override string ToString() => $"{Id} | {Label}";
    }
    
    public abstract class NeuronDescriptor : ITopological
    {
        public int Id { get; set; }

        public ActivationFunction ActivationFunction { get; set; }

        public AggregationFunction AggregationFunction { get; set; }
    }

    public class HiddenNeuronDescriptor : NeuronDescriptor, ICloneable<HiddenNeuronDescriptor>
    {
        public HiddenNeuronDescriptor Clone()
        {
            return new HiddenNeuronDescriptor()
            {
                Id = Id,
                AggregationFunction = AggregationFunction,
                ActivationFunction = ActivationFunction
            };
        }

        public override string ToString() => $"{Id} | {AggregationFunction} {ActivationFunction}";
    }

    public class OutputNeuronDescriptor : NeuronDescriptor, ICloneable<OutputNeuronDescriptor>
    {
        public OutputNeuronDescriptor Clone()
        {
            return new OutputNeuronDescriptor()
            {
                Id = Id,
                AggregationFunction = AggregationFunction,
                ActivationFunction = ActivationFunction,
                Label = Label
            };
        }

        public string Label { get; set; }

        public override string ToString() => $"{Id} | {Label} | {AggregationFunction} {ActivationFunction}";
    }

    /// <summary>
    /// Describes how a connection is applied to its destination.
    /// </summary>
    public enum ConnectionIntegrator
    {
        /// <summary>
        /// Apply the connection the destination's aggregate function
        /// </summary>
        Aggregate,
        /// <summary>
        /// Multiply the connection to the result of the destination's aggregate function
        /// </summary>
        Modulate
    }

    public enum AggregationFunction
    {
        Sum,
        Average,
        Min,
        Max
    }

    public enum ActivationFunction
    {
        ReLU,
        LogisticApproximantSteep,
        Sine,
    }
}
