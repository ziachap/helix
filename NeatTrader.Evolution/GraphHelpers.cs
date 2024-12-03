using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using ScottPlot.Drawing;
using System.Drawing;
using System.Windows;
using Helix.Genetics;
using Helix.Speciation;
using ScottPlot.Drawing.Colormaps;
using Style = ScottPlot.Style;

namespace NeatTrader.Evolution
{
    public static class GraphHelpers
    {
        public static void InitializeElites(this WpfPlot plot)
        {
            plot.Plot.Margins(0, 0);
            plot.Plot.Frameless();
            plot.Configuration.Pan = false;
            plot.Configuration.Zoom = false;
            plot.Plot.Style(Style.Blue2);
            plot.Background = System.Windows.Media.Brushes.DarkGray;
        }

        public static void PlotElites(this WpfPlot plot, MapElitesGPT elites)
        {
            var size = elites.GridSize;

            var matrix = new double?[size, size];
            //var centroid = new double?[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var genome = elites.GetElitesMatrix()[y, x];
                    if (genome != null)
                    {
                        //matrix[y, x] = Math.Sqrt(bin.BestFitness);
                        matrix[y, x] = genome.Fitness.PrimaryFitness;
                        //if (bin.Centroid) centroid[y, x] = 1;
                    }
                }
            }

            //for (int i = 0; i < Math.Max(ySize * ySize, xSize * xSize); i++)
            //{
            //    var bin = elites.Matrix[i % ySize, i / xSize];
            //    if (bin.BestGenome != null)
            //    {
            //        //matrix[i % size, i / size] = Math.Sqrt(bin.BestFitness);
            //        matrix[i % ySize, i / xSize] = bin.BestFitness;
            //        if (bin.Centroid) centroid[i % ySize, i / xSize] = 1;
            //    }
            //}

            plot.Plot.Clear();
            var heatmap = plot.Plot.AddHeatmap(matrix, Colormap.Thermal);
            //var heatmapCentroid = plot.Plot.AddHeatmap(centroid, Colormap.Grayscale);

            var bg = Color.Black;
            var xLabel = plot.Plot.AddAnnotation(elites.XDescription, 0, -40);
            xLabel.BackgroundColor = Color.FromArgb(110, bg.R, bg.G, bg.B);
            xLabel.Font.Color = Color.White;
            xLabel.Font.Size = 14;
            xLabel.Font.Bold = true;
            xLabel.Shadow = false;

            var yLabel = plot.Plot.AddAnnotation(elites.YDescription, 0, -18);
            yLabel.BackgroundColor = Color.FromArgb(110, bg.R, bg.G, bg.B);
            yLabel.Font.Color = Color.White;
            yLabel.Font.Size = 14;
            yLabel.Font.Bold = true;
            yLabel.Shadow = false;

            plot.Refresh();
        }
        /*
        public static void PlotElites(this WpfPlot plot, List<Genome> genomes, int xAuxIdx, int yAuxIdx)
        {
            var matrix = CreateElitesMatrix(genomes, 16, xAuxIdx, yAuxIdx);

            plot.Plot.Clear();
            var heatmap = plot.Plot.AddHeatmap(matrix, Colormap.Turbo);
            plot.Refresh();
        }

        public static double[,] CreateElitesMatrix(List<Genome> genomes, int size, int xAuxIdx, int yAuxIdx)
        {
            const double stdMult = 1;

            double[,] matrix = new double[size, size];

            if (genomes.Any(x => x.Fitness.AuxFitnessScores == null)) return matrix;

            // Create X bins
            var meanX = genomes.Average(x => x.Fitness.Auxillary[xAuxIdx]);
            var stdX = genomes.Select(x => x.Fitness.Auxillary[xAuxIdx]).StandardDeviation();
            var minXActual = genomes.Min(x => x.Fitness.Auxillary[xAuxIdx]);
            var maxXActual = genomes.Max(x => x.Fitness.Auxillary[xAuxIdx]);
            var minX = Math.Max(minXActual, meanX - (stdX * stdMult));
            var maxX = Math.Min(maxXActual, meanX + (stdX * stdMult));
            var incX = (maxX - minX) / size;
            if (incX == 0) return matrix;

            var binsX = new double[size];
            var currentX = minX;
            for (int i = 0; i < size; i++)
            {
                binsX[i] = currentX;
                currentX += incX;
            }

            // Create Y bins
            var meanY = genomes.Average(x => x.FitnessInfo.AuxFitnessScores[yAuxIdx]);
            var stdY = genomes.Select(x => x.FitnessInfo.AuxFitnessScores[yAuxIdx]).StandardDeviation();
            var minYActual = genomes.Min(x => x.FitnessInfo.AuxFitnessScores[yAuxIdx]);
            var maxYActual = genomes.Max(x => x.FitnessInfo.AuxFitnessScores[yAuxIdx]) * 1.0000001;
            var minY = Math.Max(minYActual, meanY - (stdY * stdMult));
            var maxY = Math.Min(maxYActual, meanY + (stdY * stdMult));
            var incY = (maxY - minY) / size;
            if (incY == 0) return matrix;

            var binsY = new double[size];
            var currentY = minY;
            for (int i = 0; i < size; i++)
            {
                binsY[i] = currentY;
                currentY += incY;
            }
            
            foreach (var genome in genomes)
            {
                var fitness = genome.FitnessInfo.PrimaryFitness;
                var aux = genome.FitnessInfo.AuxFitnessScores;

                if (aux == null) continue;

                var xVal = aux[xAuxIdx];
                var yVal = aux[yAuxIdx];

                // Find which X bin this genome fits in
                int? binIdX = null;
                for (int i = 0; i < size - 1; i++)
                {
                    if (xVal >= binsX[i] && xVal < binsX[i + 1])
                    {
                        binIdX = i;
                        break;
                    }
                }

                // Find which Y bin this genome fits in
                int? binIdY = null;
                for (int i = 0; i < size - 1; i++)
                {
                    if (yVal >= binsY[i] && yVal < binsY[i + 1])
                    {
                        binIdY = i;
                        break;
                    }
                }

                if (binIdY.HasValue && binIdX.HasValue)
                {
                    matrix[binIdY.Value, binIdX.Value]++;
                }

                //if (fitness > matrix[binIdY, binIdX]) matrix[binIdY, binIdX] = fitness;
            }

            return matrix;
        }
        */
    }
}
