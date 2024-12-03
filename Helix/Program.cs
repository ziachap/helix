using System.Runtime.CompilerServices;
using Helix.Genetics;
using Helix.Genetics.Reproduction.Asexual;

[assembly: InternalsVisibleTo("Helix.Tests")]

namespace Helix
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"=========================================");
            Console.WriteLine(@"\\\\\\\\\\\\\\\\  HELIX  ////////////////");
            Console.WriteLine(@"=========================================");
            
            //RunNeuralNetTest();
            RunMutationTest();

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
        
        private static void RunMutationTest()
        {
            var genome = GenomeFactory.Create(2, 2);
            
            var reproduction = new AsexualReproduction();

            while (true)
            {
                genome = reproduction.CreateChild(genome);

                Console.WriteLine("GENOME " + genome.Id);
                foreach (var c in genome.HiddenNeurons)
                {
                    Console.WriteLine($"- Hidden: {((NeuronDescriptor)c).Id} | w: {c.ActivationFunction}");
                }
                foreach (var c in genome.OutputNeurons)
                {
                    Console.WriteLine($"- Output: {c.Id} | w: {c.ActivationFunction}");
                }
                foreach (var c in genome.Connections)
                {
                    Console.WriteLine($"- Connection: {c.SourceId} => {c.DestinationId} | w: {c.Weight:0.000} | {c.Integrator}");
                }

                var net = GenomeDecoder.Decode(genome);

                net.Inputs[0] = 1;
                net.Inputs[1] = 1;

                for (var i = 0; i < 5; i++)
                {
                    net.Activate();
                    
                    var working = net._preActivation.ToArray();
                    var outputs = net.Outputs.ToArray();

                    Console.WriteLine("OUTPUT: " 
                                      + string.Join(", ", working.Select(x => x.ToString("0.####"))) 
                                      + " => "
                                      + string.Join(", ", outputs.Select(x => x.ToString("0.####"))));

                }

                Console.ReadKey();
                Console.WriteLine();
            }
        }

        private static void RunNeuralNetTest()
        {
            var genome = new Genome()
            {
                Inputs = new List<InputDescriptor>()
                {
                    new InputDescriptor() { Id = 1 },
                    new InputDescriptor() { Id = 2 },
                },
                HiddenNeurons = new List<HiddenNeuronDescriptor>()
                {
                    new HiddenNeuronDescriptor()
                    {
                        Id = 3,
                        ActivationFunction = ActivationFunction.ReLU,
                    }
                },
                OutputNeurons = new List<OutputNeuronDescriptor>()
                {
                    new OutputNeuronDescriptor()
                    {
                        Id = 4,
                        ActivationFunction = ActivationFunction.ReLU,
                    },
                    new OutputNeuronDescriptor()
                    {
                        Id = 5,
                        ActivationFunction = ActivationFunction.ReLU,
                    }
                },
                Connections = new List<Connection>()
                {
                    new (0, 1, 3, 0.8, ConnectionIntegrator.Aggregate),
                    new (0, 2, 3, 3, ConnectionIntegrator.Modulate),
                    new (0, 3, 4, 1.2, ConnectionIntegrator.Aggregate),
                    new (0, 2, 5, -0.5, ConnectionIntegrator.Aggregate),
                    new (0, 4, 4, 0.08, ConnectionIntegrator.Aggregate),
                }
            };

            var net = GenomeDecoder.Decode(genome);

            net.Inputs[0] = 1;
            net.Inputs[1] = 1;

            for (var i = 0; i < 10_000; i++)
            {
                net.Activate();

                var output = net.Outputs;
                var working = net._preActivation.Span;

                foreach (var w in working)
                {
                    Console.WriteLine(w.ToString("0.####"));
                }

                Console.ReadKey();
            }
        }
    }
}
