using Helix.NeuralNetwork.Reproduction;

namespace Helix
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"=========================================");
            Console.WriteLine(@"\\\\\\\\\\\\\\\\  HELIX  ////////////////");
            Console.WriteLine(@"=========================================");
            
            var genome = new Genome()
            {
                Inputs = new List<InputDescriptor>()
                {
                    new InputDescriptor() { Id = 1 },
                    new InputDescriptor() { Id = 2 },
                },
                HiddenNeurons = new List<NeuronDescriptor>()
                {
                    new NeuronDescriptor()
                    {
                        Id = 3,
                        ActivationFunction = ActivationFunction.ReLU,
                    }
                },
                OuputNeurons = new List<NeuronDescriptor>()
                {
                    new NeuronDescriptor()
                    {
                        Id = 4,
                        ActivationFunction = ActivationFunction.ReLU,
                    },
                    new NeuronDescriptor()
                    {
                        Id = 5,
                        ActivationFunction = ActivationFunction.ReLU,
                    }
                },
                Connections = new List<Connection>()
                {
                    new Connection(1, 3, 0.8, SignalIntegrator.Additive),
                    new Connection(2, 3, 3, SignalIntegrator.Multiplicative),
                    new Connection(3, 4, 1.2, SignalIntegrator.Additive),
                    new Connection(2, 5, -0.5, SignalIntegrator.Additive),
                    new Connection(1, 5, 0.8, SignalIntegrator.Additive),
                }
            };

            var reproduction = new AsexualReproduction();

            var newGenome = reproduction.CreateChild(genome);

            var net = GenomeDecoder.Decode(newGenome);

            net.Inputs[0] = 1;
            net.Inputs[1] = 1;

            for (var i = 0; i < 10_000; i++)
            {
                net.Activate();
            }

            var output = net.Outputs;
            var working = net._nodes.Span;

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
