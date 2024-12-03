using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helix.Genetics;
using Helix.Genetics.Reproduction.Asexual;
using Helix.Genetics.Reproduction.Sexual;
using Helix.Speciation;

namespace Helix
{
    public class EvolutionRunner
    {
        private readonly Func<IBlackBox, Fitness> _evaluator;

        public event Action<Genome>? OnBestGenome;
        public event Action OnGenerationComplete;

        public EvolutionMetrics Metrics { get; }
        public Genome BestGenome { get; set; }
        public MultiEmitterMapElitesGPT[] MapElites { get; set; }
        public GeneticDistanceSpeciation Species { get; set; }
        public bool Running { get; private set; }

        public EvolutionRunner(Func<IBlackBox, Fitness> evaluator)
        {
            _evaluator = evaluator;
            Metrics = new EvolutionMetrics();
            
            MapElites = new MultiEmitterMapElitesGPT[]
            {
                new MultiEmitterMapElitesGPT(12,
                    "profit", x => x.Fitness.Auxillary["profit"],
                    "maxDrawdown", x => x.Fitness.Auxillary["maxDrawdown"],
                    GenomeFactory.Create(8, 2)
                ),
                new MultiEmitterMapElitesGPT(12,
                    "tradeCountClosed", x => x.Fitness.Auxillary["tradeCountClosed"],
                    "excursionRatio", x => x.Fitness.Auxillary["excursionRatio"],
                    GenomeFactory.Create(8, 2)
                ),
            };

            Species = new GeneticDistanceSpeciation(GenomeDistanceMeasure.Distance, 5);
        }

        public void Run()
        {
            Task.Run(() =>
            {
                int populationSize = 100;

                var population = new List<Genome>();

                population = Enumerable.Range(1, populationSize)
                    .Select(x => GenomeFactory.Create(14, 2))
                    .ToList();

                var reproduction = new AsexualReproduction();
                var reproductionSexual = new SexualReproduction();

                //foreach (var multiEmitterMapElitesGpt in MapElites)
                //{
                //    for (var i = 0; i < 5; i++)
                //    {
                //        multiEmitterMapElitesGpt.AddEmitter(i + 1, i + 1, GenomeFactory.Create(14, 2));
                //
                //    }
                //}
                Running = true;

                while (true)
                {
                    while (!Running) Thread.Sleep(1);

                    //genomes = genomes.Concat(newGenomes).ToList();

                    Console.WriteLine("Speciating... ");
                    Species.Speciate(population);

                    Console.WriteLine("Evaluating... ");

                    //Parallel.ForEach(genomes, 
                    //    new ParallelOptions()
                    //    {
                    //        MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * 0.6)
                    //    }, 
                    //    genome =>
                    //{
                    //    // Evaluate genome and set fitness
                    //    var phenome = GenomeDecoder.Decode(genome);
                    //    genome.Fitness = _evaluator.Invoke(phenome);
                    //});

                    var offspring = Species.SpeciesList
                        .SelectMany(s => s.Population.ProduceOffspring())
                        .ToList();

                    Parallel.ForEach(offspring,
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * 0.8),
                            //MaxDegreeOfParallelism = 1,
                        },
                        genome =>
                        {
                            // Evaluate genome and set fitness
                            var phenome = GenomeDecoder.Decode(genome);
                            genome.Fitness = _evaluator.Invoke(phenome);
                        });

                    //Console.WriteLine("Crossover... ");
                    //// Crossover
                    //Parallel.ForEach(GetSpeciesPairs(),
                    //    new ParallelOptions()
                    //    {
                    //        MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * 0.6)
                    //    },
                    //    (species) =>
                    //    {
                    //        var g1 = species.s1.Species.OrderByDescending(x => x.Fitness.PrimaryFitness).First();
                    //        var g2 = species.s2.Species.OrderByDescending(x => x.Fitness.PrimaryFitness).First();
                    //
                    //        var newGenome = reproductionSexual.CreateChild(g1, g2);
                    //
                    //        // Evaluate genome and set fitness
                    //        var phenome = GenomeDecoder.Decode(newGenome);
                    //        newGenome.Fitness = _evaluator.Invoke(phenome);
                    //        if (newGenome.Fitness.PrimaryFitness == 0) return;
                    //
                    //        species.s1.IntegratePopulation(new List<Genome>() {newGenome});
                    //        species.s2.IntegratePopulation(new List<Genome>() {newGenome});
                    //    });

                    // TOPSIS
                    /*
                    var matrix = genomes.Select(x =>
                    {
                        var aux = x.FitnessInfo.AuxFitnessScores;

                        return new double[]
                        {
                            (double)aux.Get("profit"),
                            (double)aux.Get("inventoryScore"),
                            (double)aux.Get("excursionRatio"),
                            (double)aux.Get("ecm"),

                            (double)aux.Get("maxDrawdown"),
                            (double)aux.Get("maxDrawdownTime"),
                            (double)aux.Get("maxAdverseExcursion"),
                            //(double)Math.Pow(aux.Get("inventoryMaxAbs"), 2),
                            (double)aux.Get("inventoryMaxAbs"),
                        };
                    }).ToArray();

                    var weights = new double[]
                    {
                        3,
                        1,
                        1,
                        2,

                        2,
                        1,
                        1,
                        1
                    };

                    // Index of columns where criterion is maximized
                    var benefitCriteria = new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                    };

                    var scores = new Topsis(matrix, weights, benefitCriteria).Calculate();

                    var idx = 0;
                    foreach (var genome in genomes)
                    {
                        var score = scores[idx];
                        var fitness = genome.FitnessInfo.PrimaryFitness;
                        var aux = genome.FitnessInfo.AuxFitnessScores;
                        var newFitness = Math.Pow(fitness, 1d / 4);
                        genome.Fitness = new FitnessInfo(Math.Pow(score, 2) * newFitness, genome.FitnessInfo.AuxFitnessScores);
                        idx++;
                    }
                    */

                    //genomes = genomes
                    //    .OrderByDescending(x => x.Fitness)
                    //    .Take((int)(populationSize * 0.5))
                    //    .ToList();
                    //

                    population = population.Concat(offspring)
                        .OrderByDescending(x => x.Fitness.PrimaryFitness)
                        .ToList();

                    foreach (var mapElite in MapElites)
                    {
                        mapElite.IntegratePopulation(population);
                    }

                    var best = population.First();
                    var meanFitness = population.Average(x => x.Fitness.PrimaryFitness);

                    BestGenome = best;
                    OnBestGenome?.Invoke(best);

                    Metrics.BestFitness = best.Fitness.PrimaryFitness;
                    Metrics.MeanFitness = meanFitness;
                    Metrics.GenomesEvaluated += populationSize;
                    Metrics.Generation += 1;

                    OnGenerationComplete?.Invoke();

                    Console.WriteLine("GENOME " + best.Id);
                    foreach (var c in best.HiddenNeurons)
                    {
                        Console.WriteLine($"- Hidden: {c.Id} | w: {c.ActivationFunction}");
                    }
                    foreach (var c in best.OutputNeurons)
                    {
                        Console.WriteLine($"- Output: {c.Id} | w: {c.ActivationFunction}");
                    }
                    foreach (var c in best.Connections)
                    {
                        Console.WriteLine($"- Connection: {c.SourceId} => {c.DestinationId} | w: {c.Weight:0.000} | {c.Integrator}");
                    }
                    foreach (var s in Species.SpeciesList)
                    {
                        Console.WriteLine($"Species: " + s.Population.GenomeList.Count);
                        Console.WriteLine($"best: " + s.Representative.Fitness.PrimaryFitness);
                    }

                    Console.WriteLine();
                    //Console.ReadKey();
                }
            });
        }

        public void TogglePause() => Running = !Running;


        public List<(MultiEmitterMapElitesGPT s1, MultiEmitterMapElitesGPT s2)> GetSpeciesPairs()
        {
            List<(MultiEmitterMapElitesGPT s1, MultiEmitterMapElitesGPT s2)> pairs =
                new List<(MultiEmitterMapElitesGPT s1, MultiEmitterMapElitesGPT s2)>();

            for (int i = 0; i < MapElites.Length; i++)
            {
                for (int j = i + 1; j < MapElites.Length; j++)
                {
                    pairs.Add(new (MapElites[i], MapElites[j]));
                }
            }

            return pairs;
        }
    }

    public class EvolutionMetrics
    {
        public double BestFitness { get; set; }
        public double MeanFitness { get; set; }
        public int GenomesEvaluated { get; set; }
        public int Generation { get; set; }
    }
}
