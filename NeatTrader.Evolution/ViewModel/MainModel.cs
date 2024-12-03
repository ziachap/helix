using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Helix;
using Helix.Genetics;
using Helix.Speciation;
using NeatTrader.Evolution.Annotations;
using NeatTrader.Shared;
using NeatTrader.Simulator;
using NetTrader.Indicator;

namespace NeatTrader.Evolution.ViewModel;

public class MainModel : BaseViewModel, IDisposable
{
    public event Action<Genome>? OnBestGenome;
    public event Action<ChartingContext>? OnTrialComplete;
    
    public MainModel()
    {
        Metrics = new ObservableCollection<KeyValuePair>();

        StartCommand = new DelegateCommand(_ =>
        {

            if (EvolutionAlgorithmRunner == null)
            {
                EvolutionAlgorithmRunner = new EvolutionRunner(phenome =>
                {
                    var simulator = new TraderSimulator()
                    {
                        HistoryCount = 2_200_000
                    };

                    return simulator.RunTrial(phenome, "TSLA", false, true);
                });

                EvolutionAlgorithmRunner.OnGenerationComplete += () =>
                {
                    try
                    {
                        //State = EvolutionAlgorithmRunner.RunState switch
                        //{
                        //    RunState.Ready => EvolutionState.Idle,
                        //    RunState.Running => EvolutionState.Running,
                        //    RunState.Paused => EvolutionState.Paused,
                        //    RunState.Terminated => EvolutionState.Idle,
                        //};
                        //
                        //var ea = EvolutionAlgorithm;
                        //var pop = ea.Population;

                        Generation = EvolutionAlgorithmRunner.Metrics.Generation;
                        //TotalEvaluationCount = ea.Stats.TotalEvaluationCount;
                        //EvaluationsPerSec = ea.Stats.EvaluationsPerSec;
                        BestFitness = EvolutionAlgorithmRunner.Metrics.BestFitness;

                        // Trial and plot the best genome's performance
                        var bestGenome = EvolutionAlgorithmRunner.BestGenome;

                        // Include best genome from the MAP elites archives
                        //var mapElites = ea._speciationStrategy as MapElitesSpeciationStrategyGPT;

                        /*
                         // This is to search the deep grid for best solution
                        if (mapElites != null)
                        {
                            foreach (var elitesArchive in mapElites.Elites)
                            {
                                var matrix = elitesArchive.Matrix;
                                var size = elitesArchive.XSize;
                                var ySize = elitesArchive.YSize;

                                for (int x = 0; x < xSize; x++)
                                {
                                    for (int y = 0; y < ySize; y++)
                                    {
                                        var bin = matrix[y, x];
                                        var bestGenomes = bin.BestGenomes;

                                        foreach (var genome in bestGenomes)
                                        {
                                            if (genome.FitnessInfo.PrimaryFitness > bestGenome.FitnessInfo.PrimaryFitness)
                                            {
                                                bestGenome = genome;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        */

                        var phenome = GenomeDecoder.Decode(bestGenome);

                        OnBestGenome?.Invoke(bestGenome);
                        
                        var simulator = new TraderSimulator
                        {
                            //HistoryCount = (int)(TraderEvaluator.HistoryCount * 20),
                            HistoryCount = 4500_000
                        };
                        simulator.RunTrial(phenome, "TSLA");
                        var snapshots = simulator._snapshots;
                        
                        var metrics = new List<KeyValuePair>
                        {
                            //new KeyValuePair("Mode", Enum.GetName(ea.ComplexityRegulationMode)),
                            new KeyValuePair("Generation", Generation.ToString()),
                            new KeyValuePair("TotalEvaluationCount", TotalEvaluationCount.ToString()),
                            new KeyValuePair("EPS", EvaluationsPerSec.ToString("0.00")),
                            new KeyValuePair("BestFitness", BestFitness.ToString()),
                            //new KeyValuePair("BestComplexity", pop.NeatPopulationStats.BestComplexity.ToString("0.00")),
                            //new KeyValuePair("MeanComplexity", pop.NeatPopulationStats.MeanComplexity.ToString("0.00")),
                            new KeyValuePair("Profit", simulator._metrics.Profit.ToString("0.00")),
                            new KeyValuePair("MaxDrawdown", simulator._metrics.MaxDrawdown.ToString("0.00")),
                        };
                        
                        //metrics.Add(new KeyValuePair("Population", pop.GenomeList.Count.ToString()));
                        //if (mapElites != null)
                        //{
                        //    metrics.AddRange(mapElites.Elites.SelectMany((x, i) =>
                        //    {
                        //        var speciesCount = x.Species.GenomeList.Count;
                        //        return new[]
                        //        {
                        //            new KeyValuePair($"Species {i + 1}", speciesCount.ToString()),
                        //            new KeyValuePair($"Emitters {i + 1}", x.Emitters.Count.ToString())
                        //        };
                        //    }));
                        //}

                        metrics.AddRange(EvolutionAlgorithmRunner.Species.SpeciesList.SelectMany((x, i) =>
                        {
                            var speciesCount = x.Population.GenomeList.Count;
                            return new[]
                            {
                                new KeyValuePair($"Species {i + 1}", speciesCount.ToString()),
                            };
                        }));

                        OnTrialComplete?.Invoke(new ChartingContext
                        {
                            Snapshots = snapshots,
                            Species = EvolutionAlgorithmRunner.Species.SpeciesList.ToArray(),
                            //Genomes = pop.GenomeList,
                            MapElites = EvolutionAlgorithmRunner.MapElites.ToArray(),
                            Metrics = metrics,
                            NeatPopulationStats = EvolutionAlgorithmRunner.Metrics,
                            //SpeciationStrategy = mapElites
                        });

                        SimulationMetrics = simulator._metrics;
                        
                        UI.WaitForUpdate();
                    }
                    catch (Exception ex)
                    {

                    }
                };
            }

            EvolutionAlgorithmRunner.Run();
        });

        StopCommand = new DelegateCommand(_ =>
        {
            if (EvolutionAlgorithmRunner == null) return;
            EvolutionAlgorithmRunner.TogglePause();
        });

        LoadBestGenomeCommand = new DelegateCommand(_ =>
        {
            /*
            string filepath = SelectFileToOpen("Load seed genome", "net", "(*.net)|*.net");
            if (string.IsNullOrEmpty(filepath))
                return;

            INeatExperiment<double> neatExperiment = GetNeatExperiment();
            MetaNeatGenome<double> metaNeatGenome = NeatUtils.CreateMetaNeatGenome(neatExperiment);

            try
            {
                // Load the seed genome.
                NeatGenome<double> seedGenome = NeatGenomeLoader.Load(filepath, metaNeatGenome, 0);

                // Create an instance of the default connection weight mutation scheme.
                var weightMutationScheme = WeightMutationSchemeFactory.CreateDefaultScheme(
                    neatExperiment.ConnectionWeightScale);

                var neatPop = NeatPopulationFactory<double>.CreatePopulation(
                    metaNeatGenome,
                    neatExperiment.PopulationSize,
                    seedGenome,
                    neatExperiment.ReproductionAsexualSettings,
                    weightMutationScheme);
                
            }
            catch (Exception ex)
            {
                //__log.ErrorFormat("Error loading genome. Error message [{0}]", ex.Message);
            }

            string SelectFileToOpen(string dialogTitle, string fileExtension, string filter)
            {
                OpenFileDialog oDialog = new()
                {
                    AddExtension = true,
                    DefaultExt = fileExtension,
                    Filter = filter,
                    Title = dialogTitle,
                    RestoreDirectory = true
                };

                // Show dialog and block until user selects a file.
                if (oDialog.ShowDialog() == DialogResult.OK)
                    return oDialog.FileName;

                // No selection.
                return null;
            }
            */
        }); 

        SaveBestGenomeCommand = new DelegateCommand(_ =>
        {
            /*
            if (EvolutionAlgorithmRunner?.RunState != RunState.Paused) return;

            var population = EvolutionAlgorithm.Population;
            var bestGenome = population.BestGenome;

            // Ask the user to select a file path and name to save to.
            var filepath = FileHelper.SelectFileToSave("Save best genome", "net", "(*.net)|*.net");
            if (string.IsNullOrEmpty(filepath)) return;

            // Save the genome.
            try
            {
                NeatGenomeSaver.Save(bestGenome, filepath);
            }
            catch (Exception ex)
            {
                //__log.ErrorFormat("Error saving genome; [{0}]", ex.Message);
            }
            */
        });
    }

    public DelegateCommand StartCommand { get; }
    public DelegateCommand StopCommand { get; }
    public DelegateCommand LoadBestGenomeCommand { get; }
    public DelegateCommand SaveBestGenomeCommand { get; }
    
    public SimulationMetrics SimulationMetrics { get; set; }
    public ObservableCollection<KeyValuePair> Metrics { get; set; }

    public EvolutionState State { get; set; } = EvolutionState.Idle;

    public int Generation { get; set; }
    public double BestFitness { get; set; }
    public ulong TotalEvaluationCount { get; set; }
    public double EvaluationsPerSec { get; set; }
    
    private EvolutionRunner EvolutionAlgorithmRunner { get; set; }

    public void Dispose()
    {
    }
}

public class KeyValuePair : BaseViewModel
{
    public KeyValuePair()
    {
    }

    public KeyValuePair(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; set; }
    public string Value { get; set; }
}

public enum EvolutionState
{
    Idle = 0,
    Running = 1,
    Paused = 2
}

public class ChartingContext
{
    public Snapshot[] Snapshots { get; set; }
    public Species[] Species { get; set; }
    public MultiEmitterMapElitesGPT[] MapElites { get; set; }
    //public List<NeatGenome<double>> Genomes { get; set; }
    public List<KeyValuePair> Metrics { get; set; }
    public EvolutionMetrics NeatPopulationStats { get; set; }
    //[CanBeNull] public MapElitesSpeciationStrategyGPT SpeciationStrategy { get; set; }
}
