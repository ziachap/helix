using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace NeatTrader.Simulator
{
    public static class EquityCurveAnalyzer
    {
        public static double CalculateEquityCurveStats(List<double> equityCurve)
        {
            int n = equityCurve.Count;
            double[] xData = new double[n];
            double[] yData = new double[n];

            if (n == 0) return 0;

            for (int i = 0; i < n; i++)
            {
                xData[i] = i;
                yData[i] = equityCurve[i];
            }

            double slope = (n * xData.Zip(yData, (x, y) => x * y).Sum() - xData.Sum() * yData.Sum())
                            / (n * xData.Zip(xData, (x1, x2) => x1 * x2).Sum() - xData.Sum() * xData.Sum());

            double[] residuals = new double[n];
            double sumOfAbsDeviations = 0;
            double sumOfDeviationsBelow = 0;
            List<double> drawdownLengths = new List<double>();
            double numCrosses = 0;

            double peak = double.MinValue;

            for (int i = 0; i < n; i++)
            {
                double fittedValue = slope * xData[i];
                residuals[i] = yData[i] - fittedValue;

                double absDeviation = Math.Abs(residuals[i]);
                sumOfAbsDeviations += absDeviation;

                if (residuals[i] < 0)
                {
                    sumOfDeviationsBelow += residuals[i];
                }

                if (yData[i] > peak)
                {
                    peak = yData[i];
                }
                else
                {
                    double drawdown = peak - yData[i];
                    if (drawdown > 0)
                    {
                        drawdownLengths.Add(drawdown);
                    }
                    peak = yData[i];
                }

                if (i > 0 && Math.Sign(residuals[i - 1]) != Math.Sign(residuals[i]))
                {
                    numCrosses++;
                }
            }

            var seOfSlope = StandardErrorOfSlope(xData, yData, slope);
            var avgAbsDeviation = sumOfAbsDeviations / n;
            var avgDeviationBelow = sumOfDeviationsBelow / n;
            var avgDrawdownLength = drawdownLengths.Average();
            var crossesDividedByLength = numCrosses / n;

            var result = (crossesDividedByLength * avgDeviationBelow) / (10_000 * avgDrawdownLength * seOfSlope);

            return result;
        }

        private static double StandardErrorOfSlope(double[] xData, double[] yData, double slope)
        {
            double ssr = yData.Zip(xData, (y, x) => Math.Pow(y - slope * x, 2)).Sum();
            double stdErr = Math.Sqrt(ssr / (xData.Length - 2)) / Math.Sqrt(xData.Zip(xData, (x1, x2) => Math.Pow(x1 - x2, 2)).Sum() / (xData.Length * (xData.Length - 1)));
            return stdErr;
        }
    }
}
