using NeatTrader.Files;
using NeatTrader.TickDataProcessor.Data;
using System.Runtime.InteropServices;
using NeatTrader.Shared;

namespace NeatTrader.Simulator.Data
{
    public static class TrainingDataLoader
    {
        private static readonly Dictionary<string, ReadOnlyMemory<TickSlice>> _cache = new();

        private static readonly object Lock = new object();

        public static ReadOnlyMemory<TickSlice> LoadSymbol(string symbol)
        {
            lock (Lock)
            {
                if (_cache.ContainsKey(symbol)) return _cache[symbol];

                var outputDirectory = DataConfig.Global.OutputDataPath;
                var outputFilepath = Path.Combine(outputDirectory, symbol, $"{symbol}_OUT.parquet");

                if (!File.Exists(outputFilepath))
                    throw new FileNotFoundException("Tick output file does not exist for symbol: " + symbol);

                var data = ParquetFile.ReadAsync<TickSlice>(outputFilepath).Result.ToArray();
                
                return _cache[symbol] = data;
            }
        }
    }

    /// <summary>
    /// Provides historical tick slices in one contiguous array
    /// </summary>
    public static class TrainingDataLoaderHPC
    {
        //public const int SliceSize = 30;


        private static readonly Dictionary<string, NoisyTrainingDataHPC> NoisyCache = new();

        private static readonly Dictionary<string, TrainingDataHPC> Cache = new();

        private static readonly object Lock = new object();
        private static readonly object NoisyLock = new object();

        public static void PrepareSymbolWithRandomNoise(string symbol, int numberOfNoisySets, bool force = false)
        {
            lock (NoisyLock)
            {
                if (NoisyCache.ContainsKey(symbol) && !force) return;

                var noisySets = Enumerable.Range(1, numberOfNoisySets)
                    .Select(_ => LoadSymbolNoCache(symbol, true))
                    .ToArray();

                NoisyCache[symbol] = new NoisyTrainingDataHPC(noisySets);
            }
        }

        public static TrainingDataHPC LoadSymbolWithRandomNoise(string symbol)
        {
            lock (NoisyLock)
            {
                if (!NoisyCache.ContainsKey(symbol))
                    throw new Exception($"Must call {nameof(PrepareSymbolWithRandomNoise)}() before loading noisy data.");
                return NoisyCache[symbol].GetRandomData();
            }
        }

        public static TrainingDataHPC LoadSymbol(string symbol, bool noisy = false)
        {
            lock (Lock)
            {
                if (Cache.ContainsKey(symbol)) return Cache[symbol];

                return Cache[symbol] = LoadSymbolNoCache(symbol, noisy);
            }
        }

        public static TrainingDataHPC LoadSymbolNoCache(string symbol, bool noisy = false)
        {
            lock (Lock)
            {
                var span = TrainingDataLoader.LoadSymbol(symbol).Span;

                if (noisy) span = span.AddNoise(0.04).AsSpan();

                var frameSize = 0;
                var result = new List<double>(span.Length * 30); // estimation

                for (int i = 0; i < span.Length; i++)
                {
                    var slice = span[i];
                    result.Add(slice.TimestampRaw);
                    result.Add(slice.B1);
                    result.Add(slice.B1V);
                    result.Add(slice.A1);
                    result.Add(slice.A1V);
                    result.Add(slice.B2);
                    result.Add(slice.B2V);
                    result.Add(slice.A2);
                    result.Add(slice.A2V);
                    result.Add(slice.B3);
                    result.Add(slice.B3V);
                    result.Add(slice.A3);
                    result.Add(slice.A3V);
                    result.Add(slice.B4);
                    result.Add(slice.B4V);
                    result.Add(slice.A4);
                    result.Add(slice.A4V);
                    result.Add(slice.B5);
                    result.Add(slice.B5V);
                    result.Add(slice.A5);
                    result.Add(slice.A5V);
                    result.Add(slice.BidTradesExecutedVolume);
                    result.Add(slice.AskTradesExecutedVolume);

                    result.Add(slice.WEIGHTED_MID);
                    result.Add(slice.VOLATILITY);
                    result.Add(slice.TRADES_SYMMETRIC_LIQUIDITY);
                    result.Add(slice.TRADE_IMBALANCE_EMA);
                    result.Add(slice.TRADE_IMBALANCE_EMA_Z);
                    result.Add(slice.SLOPE_RATIO_EMA);
                    result.Add(slice.SLOPE_RATIO_EMA_Z);
                    result.Add(slice.OFI_EMA);
                    result.Add(slice.OFI_EMA_Z);
                    result.Add(slice.AVG_TRADE_QTY_IMBALANCE);
                    result.Add(slice.SUPER);
                    result.Add(slice.SUPER_EMA);

                    if (i == 0)
                    {
                        frameSize = result.Count;
                    }

                    //if (i == 0 && result.Count != SliceSize)
                    //    throw new Exception("Slice size mismatch, expected " + result.Count);
                }

                var arr = result.ToArray();

                return new TrainingDataHPC(frameSize, arr);
            }
        }
    }

    public class TrainingDataHPC
    {
        public TrainingDataHPC(int frameSize, ReadOnlyMemory<double> data)
        {
            FrameSize = frameSize;
            Data = data;
        }

        public int FrameSize { get; }
        public ReadOnlyMemory<double> Data { get; }
    }

    public class NoisyTrainingDataHPC
    {
        public NoisyTrainingDataHPC(TrainingDataHPC[] dataSets)
        {
            DataSets = dataSets;
            Random = new Random();
        }

        private TrainingDataHPC[] DataSets { get; }
        private Random Random { get; }

        public TrainingDataHPC GetRandomData()
        {
            var next = Random.Next(0, DataSets.Length - 1);
            return DataSets[next];
        }
    }
}
