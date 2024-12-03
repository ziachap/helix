using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms.Integration;
using Helix.Forms;
using NeatTrader.Evolution.ViewModel;
using NeatTrader.Shared;
using NeatTrader.Simulator;
using ScottPlot;
using ScottPlot.Drawing;
using ScottPlot.Renderable;

namespace NeatTrader.Evolution
{
    public partial class MainWindow : Window
    {
        public MainModel Model => (MainModel)DataContext;
        
        public MainWindow()
        {
            DataContext = new MainModel();
            InitializeComponent();

            var axis1 = HistoryPlot.Plot.AddAxis(Edge.Right, 2);
            HistoryPlot.Plot.SetAxisLimits(yMin: TraderSimulator.StartingBalance - 1000, yAxisIndex: 1);
            axis1.IsVisible = false;
            HistoryPlot.Plot.Benchmark(enable: true);
            HistoryPlot.Plot.SetAxisLimits(null, null, 15200, 17300, 0, 0);
            HistoryPlot.Refresh();
            HistoryPlot.AxesChanged += (sender, args) =>
            {
                PositionPlot.Plot.MatchAxis(HistoryPlot.Plot, horizontal: true, vertical: false);
                PositionPlot.Plot.MatchLayout(HistoryPlot.Plot, horizontal: true, vertical: false);
                PositionPlot.Refresh();
                SignalsPlot.Plot.MatchAxis(HistoryPlot.Plot, horizontal: true, vertical: false);
                SignalsPlot.Plot.MatchLayout(HistoryPlot.Plot, horizontal: true, vertical: false);
                SignalsPlot.Refresh();
            };

            PositionPlot.Plot.SetAxisLimits(null, null, -30, 30, 0, 1);
            var axis2 = PositionPlot.Plot.AddAxis(Edge.Right, 2);
            axis2.IsVisible = false;
            var axis3 = PositionPlot.Plot.AddAxis(Edge.Right, 3);
            axis3.IsVisible = false;

            MapElites1Plot.InitializeElites();
            MapElites2Plot.InitializeElites();
            MapElites3Plot.InitializeElites();
            MapElites4Plot.InitializeElites();

            FitnessPlot.Plot.Margins(0, 0);
            var bestFitness = FitnessPlot.Plot.AddScatterList(Color.Red, 2, markerShape: MarkerShape.none);
            var meanFitness = FitnessPlot.Plot.AddScatterList(Color.LightCoral, 1, markerShape: MarkerShape.none);
            var bestComplexity = FitnessPlot.Plot.AddScatterList(Color.Green, 2, markerShape: MarkerShape.none);
            var meanComplexity = FitnessPlot.Plot.AddScatterList(Color.LightGreen, 1, markerShape: MarkerShape.none);
            bestComplexity.YAxisIndex = 1;
            meanComplexity.YAxisIndex = 1;
            FitnessPlot.Refresh();

            // Species genomes
            var species1Control = new GenomeControl();
            Species1.Children.Add(new WindowsFormsHost()
            {
                Child = species1Control
            });
            var species2Control = new GenomeControl();
            Species2.Children.Add(new WindowsFormsHost()
            {
                Child = species2Control
            });
            var species3Control = new GenomeControl();
            Species3.Children.Add(new WindowsFormsHost()
            {
                Child = species3Control
            });
            var species4Control = new GenomeControl();
            Species4.Children.Add(new WindowsFormsHost()
            {
                Child = species4Control
            });

            Model.OnTrialComplete += context =>
            {
                Dispatcher.Invoke(() =>
                {
	                var snapshots = context.Snapshots;
	                var species = context.Species;
	                //var genomes = context.Genomes;
	                var stats = context.NeatPopulationStats;
	                var mapElites = context.MapElites;

                    try
                    {
                        Model.Metrics.Clear();
                        foreach (var metric in context.Metrics)
                        {
                            Model.Metrics.Add(metric);
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (mapElites != null)
                        {
                            if (mapElites.ElementAtOrDefault(0) != null)
                                MapElites1Plot.PlotElites(mapElites[0]);
                            if (mapElites.ElementAtOrDefault(1) != null)
                                MapElites2Plot.PlotElites(mapElites[1]);
                            //if (mapElites.Elites.ElementAtOrDefault(2) != null)
                            //    Species3Plot.PlotElites(mapElites.Elites[2]);
                            //if (mapElites.Elites.ElementAtOrDefault(3) != null)
                            //    Species4Plot.PlotElites(mapElites.Elites[3]);
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (species.Length >= 1) species1Control.Genome = species[0].Representative;
                        if (species.Length >= 2) species2Control.Genome = species[1].Representative;
                        if (species.Length >= 3) species3Control.Genome = species[2].Representative;
                        if (species.Length >= 4) species4Control.Genome = species[3].Representative;
                    }
                    catch
                    {
                    }

                    //PCAPlot.Plot.Clear();
                    //var pca = speciation.Elites[0].PCA;
                    //var xs = pca.Select(x => x[0]).ToArray();
                    //var ys = pca.Select(x => x[1]).ToArray();
                    //PCAPlot.Plot.AddScatterPoints(xs, ys);
                    //PCAPlot.Refresh();

                    try
                    {
                        HistoryPlot.Plot.Clear();

                        var drawdown = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.Drawdown).ToArray());
                        drawdown.Color = Color.Cyan;
                        drawdown.YAxisIndex = 2;

                        var myBid = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.MyBid ?? x.BestBid).ToArray());
                        myBid.Color = Color.LawnGreen;
                        myBid.StepDisplay = true;
                        myBid.YAxisIndex = 0;
                        var myAsk = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.MyAsk ?? x.BestAsk).ToArray());
                        myAsk.Color = Color.DeepPink;
                        myAsk.StepDisplay = true;
                        myAsk.YAxisIndex = 0;

                        var bestBid = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.BestBid).ToArray());
                        bestBid.Color = Color.RoyalBlue;
                        bestBid.StepDisplay = true;
                        bestBid.YAxisIndex = 0;
                        var bestAsk = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.BestAsk).ToArray());
                        bestAsk.Color = Color.Red;
                        bestAsk.StepDisplay = true;
                        bestAsk.YAxisIndex = 0;

                        var equity = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.Equity).ToArray());
                        equity.Color = Color.Orange;
                        equity.YAxisIndex = 1;
                        var balance = HistoryPlot.Plot.AddSignal(snapshots.Select(x => x.Balance).ToArray());
                        balance.Color = Color.Black;
                        balance.StepDisplay = true;
                        balance.YAxisIndex = 1;
                        //var vline = HistoryPlot.Plot.AddVerticalLine(TraderEvaluator.HistoryCount);
                        //vline.Color = Color.Black;
                        //vline.LineWidth = 1;

                        HistoryPlot.Refresh();
                    }
                    catch (Exception ex)
                    {
                    }


                    PositionPlot.Plot.Clear();

                    var hline2 = PositionPlot.Plot.AddHorizontalLine(0);
                    hline2.Color = Color.Gray;
                    hline2.LineWidth = 1;
                    hline2.LineStyle = LineStyle.Dash;
                    var askSignal = PositionPlot.Plot.AddSignal(snapshots.Select(x => x.Skew.ReplaceNonFiniteOrNull(0d)).ToArray());
                    askSignal.Color = Color.Red;
                    askSignal.StepDisplay = true;
                    askSignal.YAxisIndex = 1;
                    var bidSignal = PositionPlot.Plot.AddSignal(snapshots.Select(x => x.Spread.ReplaceNonFiniteOrNull(0d)).ToArray());
                    bidSignal.Color = Color.Green;
                    bidSignal.StepDisplay = true;
                    bidSignal.YAxisIndex = 2;

                    //var askSignalQ = PositionPlot.Plot.AddSignal(snapshots.Select(x => x.AskQ).ToArray());
                    //askSignalQ.Color = Color.HotPink;
                    //askSignalQ.StepDisplay = true;
                    //askSignalQ.YAxisIndex = 2;
                    //var bidSignalQ = PositionPlot.Plot.AddSignal(snapshots.Select(x => x.BidQ).ToArray());
                    //bidSignalQ.Color = Color.LawnGreen;
                    //bidSignalQ.StepDisplay = true;
                    //bidSignalQ.YAxisIndex = 2;

                    var position = PositionPlot.Plot.AddSignal(snapshots.Select(x => x.Position).ToArray());
                    position.Color = Color.Blue;
                    position.StepDisplay = true;
                    position.YAxisIndex = 0;
                    var hline3 = PositionPlot.Plot.AddHorizontalLine(0);
                    hline3.Color = Color.Orange;
                    hline3.LineWidth = 1;
                    hline3.LineStyle = LineStyle.Dash;
                    hline3.YAxisIndex = 1;
                    var hline4 = PositionPlot.Plot.AddHorizontalLine(0);
                    hline3.Color = Color.GreenYellow;
                    hline3.LineWidth = 1;
                    hline3.LineStyle = LineStyle.Dash;
                    hline3.YAxisIndex = 2;
                    //var vline2 = PositionPlot.Plot.AddVerticalLine(TraderEvaluator.HistoryCount);
                    //vline2.Color = Color.Black;
                    //vline2.LineWidth = 1;

                    PositionPlot.Refresh();

                    SignalsPlot.Plot.Clear();

                    var excursion = SignalsPlot.Plot.AddSignal(snapshots.Select(x => x.Excursion).ToArray());
                    excursion.FillAboveAndBelow(Color.Green, Color.Red);
                    excursion.Color = Color.Black;
                    excursion.BaselineY = 0;

                    //var vline3 = SignalsPlot.Plot.AddVerticalLine(TraderEvaluator.HistoryCount);
                    //vline3.Color = Color.Black;
                    //vline3.LineWidth = 1;

                    SignalsPlot.Refresh();
                    
					// Fitness plot
                    try
                    {
                        bestFitness.Add(bestFitness.Count, stats.BestFitness);
                        meanFitness.Add(meanFitness.Count, stats.MeanFitness);
                        //bestComplexity.Add(bestComplexity.Count, stats.BestComplexity);
                        //meanComplexity.Add(meanComplexity.Count, stats.MeanComplexity);
                        FitnessPlot.Plot.AxisAuto();
                        FitnessPlot.Refresh();
                    }
                    catch
                    {
                        throw;
                    }
                });
            };

            // Best genome control
            var host = new WindowsFormsHost();
            var control = new GenomeControl();
            host.Child = control;
            GenomeView.Children.Add(host);
            Model.OnBestGenome += genome => Dispatcher.Invoke(() => control.Genome = genome);
        }
    }
}
