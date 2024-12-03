using AutoMapper;
using MathNet.Numerics.Statistics;
using NeatTrader.TickDataProcessor.Data;

namespace NeatTrader.Simulator.Data
{
    public static class NoiseAdder
    {
        private static readonly Random Random = new Random();

        private static readonly Mapper TickSliceMapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<TickSlice, TickSlice>()));

        public static TickSlice[] AddNoise(this ReadOnlySpan<TickSlice> series, double multiplier = 1)
        {
            var src = series.ToArray();
            if (src.Length == 0) return Array.Empty<TickSlice>();

            var mids = src.Select(x => (x.B1 + x.A1) / 2).ToArray();
            var returns = mids.CalculateReturns().ToArray();
            var std = returns.StandardDeviation();

            var results = new TickSlice[src.Length];
            
            results[0] = TickSliceMapper.Map<TickSlice>(src[0]);

            // TODO: This is all wrong. You should base everything off the returns of the mid point
            // TODO: and do a randomized reconstruction of the mid point.
            // TODO: Use the book price offsets from the src data to build the price levels.

            double noise = 0;
            double mid = mids[0];
            for (var i = 1; i < returns.Length; i++)
            {
                var currentReturn = returns[i];

                if (currentReturn != 0)
                {
                    mid += currentReturn.AddNoise(std, multiplier);
                }

                var s = src[i];
                //var noisyReturn = currentReturn == 0 ? 0 : currentReturn.AddNoise(std, multiplier);
                //if (currentReturn != 0) lastNoisyReturn = noisyReturn;

                // Using src mid as the root
                var srcMid = mids[i];

                var b1Offset = s.B1 - srcMid;
                var b2Offset = s.B2 - srcMid;
                var b3Offset = s.B3 - srcMid;
                var b4Offset = s.B4 - srcMid;
                var b5Offset = s.B5 - srcMid;

                var a1Offset = s.A1 - srcMid;
                var a2Offset = s.A2 - srcMid;
                var a3Offset = s.A3 - srcMid;
                var a4Offset = s.A4 - srcMid;
                var a5Offset = s.A5 - srcMid;
                
                var newSlice = TickSliceMapper.Map<TickSlice>(s);

                newSlice.B1 = RoundToTicks(mid + b1Offset);
                newSlice.B2 = RoundToTicks(mid + b2Offset).FitBelow(newSlice.B1);
                newSlice.B3 = RoundToTicks(mid + b3Offset).FitBelow(newSlice.B2);
                newSlice.B4 = RoundToTicks(mid + b4Offset).FitBelow(newSlice.B3);
                newSlice.B5 = RoundToTicks(mid + b5Offset).FitBelow(newSlice.B4);

                newSlice.A1 = RoundToTicks(mid + a1Offset).FitAbove(newSlice.B1);
                newSlice.A2 = RoundToTicks(mid + a2Offset).FitAbove(newSlice.A1);
                newSlice.A3 = RoundToTicks(mid + a3Offset).FitAbove(newSlice.A2);
                newSlice.A4 = RoundToTicks(mid + a4Offset).FitAbove(newSlice.A3);
                newSlice.A5 = RoundToTicks(mid + a5Offset).FitAbove(newSlice.A4);
                
                results[i] = newSlice;
            }

            return results;
        }

        private static double FitBelow(this double price, double tickAbove)
        {
            return Math.Min(price, tickAbove - TraderSimulator.TickSize);
        }

        private static double FitAbove(this double price, double tickAbove)
        {
            return Math.Max(price, tickAbove + TraderSimulator.TickSize);
        }

        private static double RoundToTicks(this double price)
        {
            var p = Math.Round(price / TraderSimulator.TickSize, MidpointRounding.AwayFromZero) * TraderSimulator.TickSize;
            return Math.Round(p, 5);
        }

        public static IEnumerable<double> CalculateReturns(this IEnumerable<double> series)
        {
            double? prev = null;

            foreach (var curr in series)
            {
                if (!prev.HasValue)
                {
                    prev = curr;
                    yield return 0d;
                }
                else
                {
                    var r = curr - prev.Value;
                    prev = curr;
                    yield return r;
                }
            }
        }

        public static IEnumerable<double> AddNoise(this IEnumerable<double> series, double multiplier = 1)
        {
            var stdDev = series.StandardDeviation();

            var noisySeries = new List<double>();
            foreach (var value in series)
            {
                var noise = CreateNoise(stdDev, multiplier);
                noisySeries.Add(value + noise);
            }

            return noisySeries;
        }

        public static double AddNoise(this double value, double stdDev, double multiplier = 1)
        {
            return value + CreateNoise(stdDev, multiplier);
        }

        public static double CreateNoise(double stdDev, double multiplier = 1)
        {
            return GenerateGaussianNoise(mean: 0, stdDev) * multiplier;
        }

        private static double GenerateGaussianNoise(double mean, double stdDev)
        {
            var u1 = 1.0 - Random.NextDouble(); //uniform(0,1] random doubles
            var u2 = 1.0 - Random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }
    }
}
