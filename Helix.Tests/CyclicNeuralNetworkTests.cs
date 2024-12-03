using FluentAssertions;
using Helix.Genetics;

namespace Helix.Tests
{
    public class CyclicNeuralNetworkTests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static Genome CreateBlankGenome(int inputs, int outputs)
        {
            var genome = GenomeFactory.Create(inputs, outputs);
            genome.Connections.Clear();
            return genome;
        }

        [Test]
        public void Input_To_Output()
        {
            var genome = CreateBlankGenome(1, 1);
            genome.Connections.Add(new Connection(0, 1, 2, 1, ConnectionIntegrator.Aggregate));

            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = 1;
            net.Activate();
            net.Outputs[0].Should().Be(1);
        }

        [Test]
        public void Cyclic_Connection()
        {
            var genome = CreateBlankGenome(1, 1);
            genome.Connections.Add(new Connection(0, 1, 2, 1, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 2, 2, 0.2, ConnectionIntegrator.Aggregate));

            var net = GenomeDecoder.Decode(genome);
            
            net.Inputs[0] = 1;
            net.Activate();
            net.Outputs[0].Should().Be(1.2);

            net.Inputs[0] = 0.5;
            net.Activate();
            net.Outputs[0].Should().Be(0.648);
        }

        [Test]
        public void Cyclic_ConnectionWith_Hidden_Node()
        {
            var genome = CreateBlankGenome(1, 1);

            genome.HiddenNeurons.Add(new HiddenNeuronDescriptor
            {
                Id = 3,
                ActivationFunction = ActivationFunction.ReLU,
                AggregationFunction = AggregationFunction.Sum
            });

            genome.Connections.Add(new Connection(0, 1, 3, 1, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 3, 2, 1, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 2, 3, 0.2, ConnectionIntegrator.Aggregate));
            
            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = 1;
            net.Activate();
            net.Outputs[0].Should().Be(1.2);

            net.Inputs[0] = 0.5;
            net.Activate();
            net.Outputs[0].Should().Be(0.648);
        }

        [Test]
        public void Hidden_Node()
        {
            var genome = CreateBlankGenome(1, 1);

            genome.HiddenNeurons.Add(new HiddenNeuronDescriptor
            {
                Id = 3,
                ActivationFunction = ActivationFunction.ReLU,
                AggregationFunction = AggregationFunction.Sum
            });

            genome.Connections.Add(new Connection(0, 1, 3, 0.5, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 3, 2, 0.75, ConnectionIntegrator.Aggregate));

            genome.OutputNeurons[0].ActivationFunction = ActivationFunction.ReLU;

            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = 4;
            net.Activate();
            net.Outputs[0].Should().Be(1.5);
        }

        [Test]
        public void Single_Activation_Function()
        {
            var genome = CreateBlankGenome(1, 1);
            genome.Connections.Add(new Connection(0, 1, 2, 1, ConnectionIntegrator.Aggregate));

            genome.OutputNeurons[0].ActivationFunction = ActivationFunction.ReLU;

            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = -1;
            net.Activate();
            net.Outputs[0].Should().Be(0);
        }
        
        [Test]
        public void Multiple_Activation_Functions()
        {
            var genome = CreateBlankGenome(1, 2);
            genome.Connections.Add(new Connection(0, 1, 2, 1, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 1, 3, 1, ConnectionIntegrator.Aggregate));

            genome.OutputNeurons[0].ActivationFunction = ActivationFunction.ReLU;
            genome.OutputNeurons[1].ActivationFunction = ActivationFunction.Sine;

            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = -1;
            net.Activate();
            net.Outputs[0].Should().Be(0);
            net.Outputs[1].Should().BeApproximately(-0.8414709, 0.00001);
        }

        [Test]
        public void Modulating_Multiplicative_Connection()
        {
            var genome = CreateBlankGenome(3, 1);
            genome.Connections.Add(new Connection(0, 1, 4, 1, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 2, 4, 1, ConnectionIntegrator.Aggregate));
            genome.Connections.Add(new Connection(0, 3, 4, 1, ConnectionIntegrator.Modulate));

            genome.OutputNeurons[0].ActivationFunction = ActivationFunction.ReLU;

            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = -1;
            net.Inputs[1] = -2;
            net.Inputs[2] = -0.5;
            net.Activate();
            net.Outputs[0].Should().Be(1.5);
        }

        public class AggregateFunctions
        {
            [Test]
            public void Sum()
            {
                var genome = CreateBlankGenome(2, 1);
                genome.Connections.Add(new Connection(0, 1, 3, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 2, 3, 1, ConnectionIntegrator.Aggregate));

                genome.OutputNeurons[0].AggregationFunction = AggregationFunction.Sum;

                var net = GenomeDecoder.Decode(genome);

                net.Inputs[0] = 3.2;
                net.Inputs[1] = -0.7;
                net.Activate();
                net.Outputs[0].Should().Be(2.5);
            }

            [Test]
            public void Average()
            {
                var genome = CreateBlankGenome(3, 1);
                genome.Connections.Add(new Connection(0, 1, 4, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 2, 4, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 3, 4, 1, ConnectionIntegrator.Aggregate));

                genome.OutputNeurons[0].AggregationFunction = AggregationFunction.Average;

                var net = GenomeDecoder.Decode(genome);

                net.Inputs[0] = 3.2;
                net.Inputs[1] = -0.7;
                net.Inputs[2] = 2;
                net.Activate();
                net.Outputs[0].Should().Be(1.5);
            }

            [Test]
            public void Max()
            {
                var genome = CreateBlankGenome(3, 1);
                genome.Connections.Add(new Connection(0, 1, 4, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 2, 4, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 3, 4, 1, ConnectionIntegrator.Aggregate));

                genome.OutputNeurons[0].AggregationFunction = AggregationFunction.Max;

                var net = GenomeDecoder.Decode(genome);

                net.Inputs[0] = 3.2;
                net.Inputs[1] = -0.7;
                net.Inputs[2] = 2;
                net.Activate();
                net.Outputs[0].Should().Be(3.2);
            }

            [Test]
            public void Min()
            {
                var genome = CreateBlankGenome(3, 1);
                genome.Connections.Add(new Connection(0, 1, 4, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 2, 4, 1, ConnectionIntegrator.Aggregate));
                genome.Connections.Add(new Connection(0, 3, 4, 1, ConnectionIntegrator.Aggregate));

                genome.OutputNeurons[0].AggregationFunction = AggregationFunction.Max;

                var net = GenomeDecoder.Decode(genome);

                net.Inputs[0] = 3.2;
                net.Inputs[1] = -0.7;
                net.Inputs[2] = 2;
                net.Activate();
                net.Outputs[0].Should().Be(3.2);
            }
        }
    }
}