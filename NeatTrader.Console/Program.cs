using System;
using System.Windows;
using Helix;
using Helix.Forms;
using Helix.Genetics;
using Helix.Genetics.Reproduction;
using Helix.Genetics.Reproduction.Asexual;
using Helix.Genetics.Reproduction.Sexual;
using NeatTrader.Simulator;
using Application = System.Windows.Forms.Application;
using FontStyle = System.Drawing.FontStyle;

namespace NeatTrader.Console
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Console.WriteLine(@"========================================");
            System.Console.WriteLine(@"\\\\\\\\\\  HELIX NeatTrader  //////////");
            System.Console.WriteLine(@"========================================");
            
            var asexual = new AsexualReproduction();
            var reproduction = new SexualReproduction();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var g1Form = new GenomeForm()
            {
                Height = 400,
                Width = 600,
            };
            Task.Run(() => Application.Run(g1Form));
            /*
            var g2Form = new GenomeForm()
            {
                Height = 400,
                Width = 600,
            };
            Task.Run(() => Application.Run(g2Form));

            var childForm = new GenomeForm()
            {
                Height = 400,
                Width = 600,
            };
            Task.Run(() => Application.Run(childForm));

            while (true)
            {
                var g1 = GenomeFactory.Create(6, 2);
                for (var i = 0; i < 150; i++) g1 = asexual.CreateChild(g1);
                System.Console.WriteLine("========== g1 ========== ");
                PrintGenome(g1);
                g1Form.Genome = g1;
                
                var g2 = GenomeFactory.Create(6, 2);
                for (var i = 0; i < 150; i++) g2 = asexual.CreateChild(g2);
                System.Console.WriteLine("========== g2 ========== ");
                PrintGenome(g2);
                g2Form.Genome = g2;

                var child = reproduction.CreateChild(g1, g2);
                System.Console.WriteLine("========== child ========== ");
                PrintGenome(child);
                childForm.Genome = child;

                System.Console.WriteLine("press key");
                System.Console.ReadKey();

                Innovation.ClearCache();
            }
            */

            var runner = new EvolutionRunner(phenome =>
            {
                var simulator = new TraderSimulator()
                {
                    HistoryCount = 1_000_000
                };

                return simulator.RunTrial(phenome, "TSLA", false, true);
            });

            runner.OnBestGenome += g => g1Form.Genome = g;
            runner.OnGenerationComplete += () =>
            {
                System.Console.WriteLine($"Generation: {runner.Metrics.Generation}");
                System.Console.WriteLine($"Best Fitness: {runner.Metrics.BestFitness}");
                System.Console.WriteLine($"Genomes: {runner.Metrics.GenomesEvaluated}");
                
            };

            runner.Run();

            System.Console.ReadKey();
        }

        private static void PrintGenome(Genome g)
        {
            foreach (var node in g.HiddenNeurons.OrderBy(x => x.ToString()))
            {
                System.Console.WriteLine(node);
            }
            foreach (var conn in g.Connections.OrderBy(x => x.ToString()))
            {
                System.Console.WriteLine(conn);
            }
        }

        private static Genome MakeGenome() => new()
            {
                Inputs = new List<InputDescriptor>()
                {
                    new InputDescriptor() { Id = 1, Label = "POS" },
                    new InputDescriptor() { Id = 2, Label = "TIME" },
                    new InputDescriptor() { Id = 3, Label = "CONST" },
                    new InputDescriptor() { Id = 8, Label = "RSI_1"  },
                    new InputDescriptor() { Id = 9, Label = "RSI_2" },
                    new InputDescriptor() { Id = 10, Label = "VOL_200"  },
                },
                HiddenNeurons = new List<HiddenNeuronDescriptor>()
                {
                    new HiddenNeuronDescriptor()
                    {
                        Id = 7,
                        AggregationFunction = AggregationFunction.Sum,
                        ActivationFunction = ActivationFunction.ReLU,
                    },
                    new HiddenNeuronDescriptor()
                    {
                        Id = 11,
                        AggregationFunction = AggregationFunction.Sum,
                        ActivationFunction = ActivationFunction.LogisticApproximantSteep,
                    }
                },
                OutputNeurons = new List<OutputNeuronDescriptor>()
                {
                    new OutputNeuronDescriptor()
                    {
                        Id = 5,
                        AggregationFunction = AggregationFunction.Average,
                        ActivationFunction = ActivationFunction.Sine,
                        Label = "SKEW"
                    },
                    new OutputNeuronDescriptor()
                    {
                        Id = 6,
                        AggregationFunction = AggregationFunction.Max,
                        ActivationFunction = ActivationFunction.ReLU,
                        Label = "SPREAD"
                    }
                },
                Connections = new List<Connection>()
                {
                    new (0, 1, 7, 0.8, ConnectionIntegrator.Aggregate),
                    new (1, 2, 7, 3, ConnectionIntegrator.Modulate),
                    new (2, 7, 5, 8.2, ConnectionIntegrator.Aggregate),
                    new (3, 7, 6, -0.5, ConnectionIntegrator.Aggregate),
                    new (4, 6, 7, 0.02, ConnectionIntegrator.Aggregate),
                    new (5, 5, 5, 0.08, ConnectionIntegrator.Aggregate),
                    new (6, 5, 7, -0.08, ConnectionIntegrator.Aggregate),
                    new (7, 7, 11, -0.08, ConnectionIntegrator.Modulate),
                    new (8, 10, 6, 1, ConnectionIntegrator.Aggregate),
                }
            };


        private static Genome MakeGenome2() => new()
        {
            Inputs = new List<InputDescriptor>()
                {
                    new InputDescriptor() { Id = 1, Label = "POS" },
                    new InputDescriptor() { Id = 2, Label = "TIME" },
                    new InputDescriptor() { Id = 3, Label = "CONST" },
                    new InputDescriptor() { Id = 8, Label = "RSI_1"  },
                    new InputDescriptor() { Id = 9, Label = "RSI_2" },
                    new InputDescriptor() { Id = 10, Label = "VOL_200"  },
                },
            HiddenNeurons = new List<HiddenNeuronDescriptor>()
                {
                    new HiddenNeuronDescriptor()
                    {
                        Id = 7, 
                        AggregationFunction = AggregationFunction.Sum,
                        ActivationFunction = ActivationFunction.LogisticApproximantSteep,
                    },
                },
            OutputNeurons = new List<OutputNeuronDescriptor>()
                {
                    new OutputNeuronDescriptor()
                    {
                        Id = 5,
                        AggregationFunction = AggregationFunction.Average,
                        ActivationFunction = ActivationFunction.ReLU,
                        Label = "SKEW"
                    },
                    new OutputNeuronDescriptor()
                    {
                        Id = 6,
                        AggregationFunction = AggregationFunction.Max,
                        ActivationFunction = ActivationFunction.ReLU,
                        Label = "SPREAD"
                    }
                },
            Connections = new List<Connection>()
                {
                    new (0, 8, 7, 0.8, ConnectionIntegrator.Aggregate),
                    new (1, 9, 6, -3, ConnectionIntegrator.Aggregate),
                }
        };
    }
}