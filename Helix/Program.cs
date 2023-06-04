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
                    Console.WriteLine($"- Hidden: {c.Id} | w: {c.ActivationFunction}");
                }
                foreach (var c in genome.OuputNeurons)
                {
                    Console.WriteLine($"- Output: {c.Id} | w: {c.ActivationFunction}");
                }
                foreach (var c in genome.Connections)
                {
                    Console.WriteLine($"- Connection: {c.SourceIdx} => {c.DestinationIdx} | w: {c.Weight:0.000} | {c.SignalIntegrator}");
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
                    new (1, 3, 0.8, SignalIntegrator.Additive),
                    new (2, 3, 3, SignalIntegrator.Multiplicative),
                    new (3, 4, 1.2, SignalIntegrator.Additive),
                    new (2, 5, -0.5, SignalIntegrator.Additive),
                    new (4, 4, 0.08, SignalIntegrator.Additive),
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
