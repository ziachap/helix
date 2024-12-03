using NeatTrader.Simulator.Data;
using NeatTrader.TickDataProcessor.Data;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using NeatTrader.Shared;
using NetTrader.Indicator;
using System.Runtime.ConstrainedExecution;
using Helix;
using Helix.Genetics;
using ActivationFunction = NeatTrader.Shared.ActivationFunction;
using ScottPlot.Drawing.Colormaps;

namespace NeatTrader.Simulator
{
    public class TraderSimulator
    {
        public const double Quantity = 1;
        public const double MaxQuantity = 50;
        public const double TickSize = 0.01;
        public const double ContractSize = 1;
        public const int StartingBalance = 10_000;

        public event Action<Snapshot> OnTickComplete;
        public event Action<double> OnBuy;
        public event Action<double> OnSell;

        public int HistoryCount { get; set; } = int.MaxValue;

        // TODO: Forgot what this was even for
        public bool RunNoModel { get; set; } = false; 

        private readonly Random _random = new Random(DateTime.Now.Second);
        private readonly TimeSpan _sod = new TimeSpan(14, 30, 00);
        private readonly TimeSpan _eod = new TimeSpan(20, 59, 50);

        public Snapshot[]? _snapshots = null;
        public SimulationMetrics _metrics = null;

        private double? _myBid;
        private double? _myAsk;
        private DateTime _nextOrderTime;
        private NetPosition _position;
        private TickSlice[]? _jitterArr;
        
        /// <summary>
        /// Run a back-test using the provided black box and return fitness metric.
        /// </summary>
        /// <returns></returns>
        public unsafe Fitness RunTrial(IBlackBox agent, string symbol,
            bool randomChunks = false,
            bool highPerformance = false)
        {
            // Reset variables
            _myBid = null;
            _myAsk = null;
            _nextOrderTime = DateTime.MinValue;
            _position = new NetPosition();
            _metrics = new SimulationMetrics();
            double _balance = StartingBalance;
            var drawdownTime = 0;
            var maxEquity = 0d;
            var maxDrawdown = 0d;
            var inventoryMaxAbs = 0d;
            var inventorySum = 0d;
            var maxDrawdownTime = 0;
            var tradeCountClosed = 0;
            var tradeCountAll = 0;
            var maxAdverseExcursion = 0d;
            var favourableExcursionSum = 0d;
            var adverseExcursionSum = 0d;
            var favourableExcursionCount = 0;
            var adverseExcursionCount = 0;

            // Prepare historical data

            //TrainingDataLoaderHPC.PrepareSymbolWithRandomNoise(symbol, 8);
            //var trainingData = TrainingDataLoaderHPC.LoadSymbolWithRandomNoise(symbol);
            var trainingData = TrainingDataLoaderHPC.LoadSymbol(symbol);
            var fastSpanM = trainingData.Data;
            var historyLengthTotal = fastSpanM.Length / trainingData.FrameSize;
            
            var startIdx = randomChunks
                ? _random.Next(0, Math.Max(1, historyLengthTotal - HistoryCount - 1))
                : 0;
            
            var endIdx = randomChunks
                ? Math.Min(startIdx + HistoryCount, historyLengthTotal)
                : Math.Min(HistoryCount, historyLengthTotal);
            
            var fastSpan = fastSpanM
                .Slice(startIdx * trainingData.FrameSize, endIdx * trainingData.FrameSize)
                .Span;

            var inputs = agent.Inputs;
            var outputs = agent.Outputs;

            var historyLength = fastSpan.Length / trainingData.FrameSize;

            _snapshots ??= new Snapshot[historyLength];
            
            double lastBid = 0d;
            double lastAsk = 0d;

            double prevBidVolume = 0d;
            double prevAskVolume = 0d;
            double prevBidVolumeReq = 0d;
            double prevAskVolumeReq = 0d;

            // Step through history
            fixed (double* ptr = fastSpan)
            {
                double* endPtr = ptr + fastSpan.Length; // pointer to the end of the array

                var idx = 0;

                for (double* ctr = ptr; ctr < endPtr; ctr += trainingData.FrameSize)
                {
                    var snapshotIdx = idx;

                    var offset = 0;

                    double timestampD = *(ctr + offset); offset++; ;
                    DateTime timestamp = timestampD.UnixTimeStampToDateTimeUTC();
                    double b0 = *(ctr + offset); offset++;
                    double b0V = *(ctr + offset); offset++;
                    double a0 = *(ctr + offset); offset++;
                    double a0V = *(ctr + offset); offset++;
                    double b1 = *(ctr + offset); offset++;
                    double b1V = *(ctr + offset); offset++;
                    double a1 = *(ctr + offset); offset++;
                    double a1V = *(ctr + offset); offset++;
                    double b2 = *(ctr + offset); offset++;
                    double b2V = *(ctr + offset); offset++;
                    double a2 = *(ctr + offset); offset++;
                    double a2V = *(ctr + offset); offset++;
                    double b3 = *(ctr + offset); offset++;
                    double b3V = *(ctr + offset); offset++;
                    double a3 = *(ctr + offset); offset++;
                    double a3V = *(ctr + offset); offset++;
                    double b4 = *(ctr + offset); offset++;
                    double b4V = *(ctr + offset); offset++;
                    double a4 = *(ctr + offset); offset++;
                    double a4V = *(ctr + offset); offset++;
                    double bidVolumeTraded = *(ctr + offset); offset++;
                    double askVolumeTraded = *(ctr + offset); offset++;

                    double WEIGHTED_MID = *(ctr + offset); offset++;
                    double VOLATILITY = *(ctr + offset); offset++;
                    double TRADES_SYMMETRIC_LIQUIDITY = *(ctr + offset); offset++;
                    double TRADE_IMBALANCE_EMA = *(ctr + offset); offset++;
                    double TRADE_IMBALANCE_EMA_Z = *(ctr + offset); offset++;
                    double SLOPE_RATIO_EMA = *(ctr + offset); offset++;
                    double SLOPE_RATIO_EMA_Z = *(ctr + offset); offset++; 
                    double OFI_EMA = *(ctr + offset); offset++;
                    double OFI_EMA_Z = *(ctr + offset); offset++;    
                    double AVG_TRADE_QTY_IMBALANCE = *(ctr + offset); offset++;
                    double SUPER = *(ctr + offset); offset++;
                    double SUPER_EMA = *(ctr + offset); offset++;

                    
                    lastBid = b0;
                    lastAsk = a0;

                    // Skip when outside market hours
                    var tod = timestamp.TimeOfDay;
                    var inTradingHours = tod >= _sod && tod < _eod;
                    var timeLeft = Math.Max(0, (_eod.TotalSeconds - tod.TotalSeconds) / (_eod.TotalSeconds - _sod.TotalSeconds));

                    var mid = (b0 + a0) / 2;

                    var openProfit = 0d;
                    if (_position.Quantity != 0)
                    {
                        var diff = mid - _position.Price;
                        openProfit = _position.Quantity * diff * ContractSize;
                    }
                    var equity = _balance + openProfit;

                    // =========== Exchange actions =========== 

                    if (a0 <= _myBid)
                    {
                        var tradeProfit = _position.ConsolidateTrade(Side.Buy, Quantity, _myBid.Value);
                        _balance += tradeProfit * ContractSize;
                        if (tradeProfit != 0) tradeCountClosed++;
                        tradeCountAll++;

                        _myBid = null;
                    }
                    else if (b0 < _myBid && bidVolumeTraded > prevBidVolumeReq) // simulated market orders
                    {
                        var tradeProfit = _position.ConsolidateTrade(Side.Buy, Quantity, _myBid.Value);
                        _balance += tradeProfit * ContractSize;
                        if (tradeProfit != 0) tradeCountClosed++;
                        tradeCountAll++;
                        _myBid = null;
                    }

                    if (b0 >= _myAsk)
                    {
                        var tradeProfit = _position.ConsolidateTrade(Side.Sell, Quantity, _myAsk.Value);
                        _balance += tradeProfit * ContractSize;
                        if (tradeProfit != 0) tradeCountClosed++;
                        tradeCountAll++;

                        _myAsk = null;
                    }
                    else if (a0 > _myAsk && askVolumeTraded > prevAskVolumeReq) // simulated market orders
                    {
                        var tradeProfit = _position.ConsolidateTrade(Side.Sell, Quantity, _myAsk.Value);
                        _balance += tradeProfit * ContractSize;
                        if (tradeProfit != 0) tradeCountClosed++;
                        tradeCountAll++;
                        _myAsk = null;
                    }

                    // Set prev variables
                    prevBidVolume = b0V;
                    prevAskVolume = a0V;

                    prevBidVolumeReq = 0d;
                    if (_myBid <= b0) prevBidVolumeReq += b0V;
                    if (_myBid <= b1) prevBidVolumeReq += b1V;
                    if (_myBid <= b2) prevBidVolumeReq += b2V;
                    if (_myBid <= b3) prevBidVolumeReq += b3V;
                    if (_myBid <= b4) prevBidVolumeReq += b4V;

                    prevAskVolumeReq = 0d;
                    if (_myAsk >= a0) prevAskVolumeReq += a0V;
                    if (_myAsk >= a1) prevAskVolumeReq += a1V;
                    if (_myAsk >= a2) prevAskVolumeReq += a2V;
                    if (_myAsk >= a3) prevAskVolumeReq += a3V;
                    if (_myAsk >= a4) prevAskVolumeReq += a4V;


                    // =========== Agent actions =========== 
                    double spreadSignal = 0d;
                    double skewSignal = 0d;

                    var qUnits = _position.Quantity / Quantity;
                    inputs[0] = qUnits;
                    inputs[1] = timeLeft;
                    inputs[2] = 1;
                    inputs[3] = Math.Sign(openProfit) * Math.Log10(1 + Math.Abs(openProfit));

                    inputs[4] = VOLATILITY;
                    inputs[5] = TRADES_SYMMETRIC_LIQUIDITY;
                    inputs[6] = TRADE_IMBALANCE_EMA;
                    inputs[7] = TRADE_IMBALANCE_EMA_Z;
                    inputs[8] = OFI_EMA;
                    inputs[9] = OFI_EMA_Z;
                    inputs[10] = SLOPE_RATIO_EMA_Z;
                    inputs[11] = AVG_TRADE_QTY_IMBALANCE;
                    inputs[12] = SUPER;
                    inputs[13] = SUPER_EMA;

                    agent.Activate();

                    //var bidWideSignal = outputs[0];
                    //var askWideSignal = outputs[1];

                    // Market orders

                    //var mktBuy = outputs[2] >= 1;
                    //var mktSell = outputs[3] >= 1;
                    //
                    //if (mktBuy)
                    //{
                    //    var tradeProfit = _position.ConsolidateTrade(Side.Buy, Quantity, a0);
                    //    _balance += tradeProfit * ContractSize;
                    //    if (tradeProfit != 0) tradeCountClosed++;
                    //    tradeCountAll++;
                    //}
                    //if (mktSell)
                    //{
                    //    var tradeProfit = _position.ConsolidateTrade(Side.Sell, Quantity, b0);
                    //    _balance += tradeProfit * ContractSize;
                    //    if (tradeProfit != 0) tradeCountClosed++;
                    //    tradeCountAll++;
                    //}

                    //var minSpread = 1;
                    //var maxSpread = 20;
                    //var diffSpread = maxSpread - minSpread;
                    //var maxSkew = maxSpread;
                    //
                    //if (TRADES_SYMMETRIC_LIQUIDITY == 0)
                    //{
                    //    spreadSignal = maxSpread;
                    //}
                    //else
                    //{
                    //    spreadSignal = Math.Log(1 + Math.Min(maxSpread, (VOLATILITY / TRADES_SYMMETRIC_LIQUIDITY) * maxSpread * 2));
                    //}
                    //
                    //skewSignal = (outputs[1] * maxSkew) - qUnits * 10 * timeLeft;

                    /*********** Avellaneda-Stoikov ************/
                    var alpha = -SUPER_EMA * 100; // alpha skew signal
                    var sigma = 2d; // volatility
                    //var sigma = 1 + VOLATILITY * 2; // volatility
                    var k = 1.5d; // order book liquidity
                    //var k = Math.Max(0.1, TRADES_SYMMETRIC_LIQUIDITY); // order book liquidity
                    var gamma = 0.2d; // inventory risk aversion

                    var r = WEIGHTED_MID - qUnits * gamma * Math.Pow(sigma, 2) * timeLeft;// + (alpha * TickSize);
                    var delta = gamma * Math.Pow(sigma, 2) * timeLeft + (2 / gamma) * Math.Log(1 + gamma / k);

                    var bid = r - delta / 2;
                    var ask = r + delta / 2;

                    spreadSignal = alpha;
                    skewSignal = r - WEIGHTED_MID;

                    /******************************************/

                    // var dumpInventory = outputs[2];

                    //var bidSignal = spreadSignal + minSpread;
                    //var askSignal = spreadSignal + minSpread;

                    if (!double.IsFinite(spreadSignal)) return new Fitness();
                    if (!double.IsFinite(skewSignal)) return new Fitness();

                    if (inTradingHours)
                    {
                        bool repriceBid = false;
                        bool repriceAsk = false;

                        //if (dumpInventory > 1 && _position.Quantity != 0)
                        //{
                        //    var side = _position.Quantity < 0 ? Side.Buy : Side.Sell;
                        //    var price = _position.Quantity < 0 ? a0 : b0;
                        //    var tradeProfit = _position.ConsolidateTrade(side, Math.Abs(_position.Quantity), price);
                        //    _balance += tradeProfit * ContractSize;
                        //    if (tradeProfit != 0) tradeCountClosed++;
                        //    tradeCountAll++;
                        //}

                        // cancel when price moves too far from orders
                        //if (_myBid != null)
                        //{
                        //    if (b0 - _myBid > maxSpread * TickSize * 2)
                        //    {
                        //        repriceBid = true;
                        //    }
                        //}
                        //if (_myAsk != null)
                        //{
                        //    if (_myAsk - a0 > maxSpread * TickSize * 2)
                        //    {
                        //        repriceAsk = true;
                        //    }
                        //}

                        // Allow a reprice if past next order time
                        if (timestamp >= _nextOrderTime)
                        {
                            repriceBid = true;
                            repriceAsk = true;

                            //if (_myBid == null)
                            //{
                            //    repriceBid = true;
                            //}
                            //
                            //if (_myAsk == null)
                            //{
                            //    repriceAsk = true;
                            //}
                        }
                        
                        if (repriceBid)
                        {
                            //var bidTicks = bidSignal * TickSize;
                            //var myBid = RoundPrice(WEIGHTED_MID - bidTicks + (skewSignal * TickSize));
                            //myBid = Math.Min(myBid, b0);
                            //_myBid = myBid;

                            var myBid = RoundPrice(bid);
                            _myBid = myBid;
                        }
                        else
                        {

                        }

                        if (repriceAsk)
                        {
                            //var askTicks = askSignal * TickSize;
                            //var myAsk = RoundPrice(WEIGHTED_MID + askTicks + (skewSignal * TickSize));
                            //myAsk = Math.Max(myAsk, a0);
                            //_myAsk = myAsk;

                            var myAsk = RoundPrice(ask);
                            _myAsk = myAsk;
                        }
                        else
                        {

                        }

                        if (repriceBid || repriceAsk)
                        {
                            _nextOrderTime = timestamp.AddSeconds(20);
                        }
                    }
                    // Close all position outside hours
                    else if (_position.Quantity != 0)
                    {
                        _myBid = null;
                        _myAsk = null;
                    
                        if (_position.Quantity > 0)
                        {
                            var tradeProfit = _position.ConsolidateTrade(Side.Sell, Quantity, b0);
                            _balance += tradeProfit * ContractSize;
                            if (tradeProfit != 0) tradeCountClosed++;
                            tradeCountAll++;
                        }
                        else if (_position.Quantity < 0)
                        {
                            var tradeProfit = _position.ConsolidateTrade(Side.Buy, Quantity, a0);
                            _balance += tradeProfit * ContractSize;
                            if (tradeProfit != 0) tradeCountClosed++;
                            tradeCountAll++;
                        }
                    }

                    if (_position.Quantity > MaxQuantity) _myBid = null;
                    if (_position.Quantity < -MaxQuantity) _myAsk = null;

                    maxEquity = equity > maxEquity ? equity : maxEquity;

                    var drawdown = maxEquity - equity;
                    if (drawdown > 0)
                        drawdownTime++;
                    else
                        drawdownTime = 0;

                    if (drawdown > maxDrawdown) maxDrawdown = drawdown;
                    if (drawdownTime > maxDrawdownTime) maxDrawdownTime = drawdownTime;
                    var positionAbs = Math.Abs(_position.Quantity);
                    if (positionAbs > inventoryMaxAbs) inventoryMaxAbs = positionAbs;
                    inventorySum += _position.Quantity;
                    if (-openProfit > maxAdverseExcursion) maxAdverseExcursion = -openProfit;

                    if (openProfit > 0)
                    {
                        favourableExcursionSum += openProfit;
                        favourableExcursionCount++;
                    }
                    else if (openProfit < 0)
                    {
                        adverseExcursionSum += -openProfit;
                        adverseExcursionCount++;
                    }

                    if (!highPerformance)
                    {
                        ref Snapshot snapshot = ref _snapshots[snapshotIdx];
                        snapshot.Idx = idx;
                        snapshot.BestBid = b0;
                        snapshot.BestAsk = a0;
                        snapshot.Balance = _balance;
                        snapshot.Equity = equity;
                        snapshot.Position = _position.Quantity;
                        snapshot.Drawdown = -drawdown;
                        snapshot.DrawdownTime = drawdownTime;
                        snapshot.Excursion = openProfit;
                        snapshot.Spread = spreadSignal;
                        snapshot.Skew = skewSignal;
                        snapshot.MyBid = _myBid;
                        snapshot.MyAsk = _myAsk;

                        OnTickComplete?.Invoke(snapshot);
                    }

                    idx++;
                }
            }
            
            var mid_ = (lastBid + lastBid) / 2;

            var openProfit_ = 0d;
            if (_position.Quantity != 0)
            {
                var diff = mid_ - _position.Price;
                openProfit_ = _position.Quantity * diff * ContractSize;
            }
            var equity_ = _balance + openProfit_;

            var actualProfit = Math.Min(equity_, _balance) - StartingBalance;
            var profit = actualProfit < 0 ? Math.Pow(1.1, actualProfit) : actualProfit + 1.1;
            //var profit = Math.Max(0, actualProfit);

            if (tradeCountClosed < 100) return new Fitness() { PrimaryFitness = 0 };

            var romad = profit / maxDrawdown;

            if (double.IsNaN(inventorySum)) inventorySum = double.MaxValue;
            var inventoryAvg = inventorySum / historyLength;
            var inventoryStd = 3 * Quantity;
            var inventoryScore = Math.Abs(ActivationFunction.Gaussian(inventoryAvg, 1, 0, inventoryStd));
            
            var favourableExcursionAvg = favourableExcursionSum / favourableExcursionCount;
            var adverseExcursionAvg = adverseExcursionSum / adverseExcursionCount;
            var excursionRatio = favourableExcursionAvg / adverseExcursionAvg;
            if (!double.IsFinite(excursionRatio)) excursionRatio = 1;

            var maximise = Math.Pow(romad, 2)
                           * Math.Sqrt(tradeCountClosed)
                           //* tradeCountClosed
                           * inventoryScore
                           * Math.Sqrt(excursionRatio)
                           * 1000;
            var minimise = Math.Pow(inventoryMaxAbs / Quantity, 2)
                           * Math.Sqrt(maxDrawdownTime);
                           //* maxAdverseExcursion;

            var fitness = Math.Max(0, maximise / minimise);

            if (fitness < 0 || !double.IsFinite(fitness))
            {
                fitness = 0;
            }

            //var ecm = EquityCurveAnalyzer.CalculateEquityCurveStats(_snapshots.Select(x => x.Equity).ToList());
            var ecm = 1;

            _metrics.Profit = actualProfit;
            _metrics.AverageInventorySize = inventoryAvg;
            _metrics.AverageInventoryScore = inventoryScore;
            _metrics.MaxInventory = inventoryMaxAbs;
            _metrics.MaxDrawdown = maxDrawdown;
            _metrics.MaxDrawdownTime = maxDrawdownTime;
            _metrics.ClosedTradeCount = tradeCountClosed;
            _metrics.MaximumAdverseExcursion = maxAdverseExcursion;

            var aux = new Dictionary<string, double>
            {
                { "profit", actualProfit },
                { "maxDrawdown", maxDrawdown },
                { "maxDrawdownTime", maxDrawdownTime },
                { "tradeCountAll", tradeCountAll },
                { "tradeCountClosed", tradeCountClosed },
                { "maxAdverseExcursion", maxAdverseExcursion },
                { "inventoryMaxAbs", inventoryMaxAbs },
                { "inventoryAvg", inventoryAvg },
                { "inventoryScore", inventoryScore },
                { "excursionRatio", excursionRatio },
                { "ecm", ecm},
            };
            
            return new Fitness()
            {
                PrimaryFitness = fitness,
                Auxillary = aux
            };
        }

        private double RoundPrice(double price)
        {
            var p = Math.Round(price / TickSize, MidpointRounding.AwayFromZero) * TickSize;
            return Math.Round(p, 5);
        }
    }

    public struct Snapshot
    {
        public int Idx { get; set; }
        public double BestBid { get; set; }
        public double BestAsk { get; set; }
        public double Balance { get; set; }
        public double Equity { get; set; }
        public double Position { get; set; }
        public double Drawdown { get; set; }
        public double Excursion { get; set; }
        public double? Spread { get; set; }
        public double? Skew { get; set; }
        public double? MyBid { get; set; }
        public double? MyAsk { get; set; }
        //public double BidQ { get; set; }
        //public double AskQ { get; set; }
        public double Output1 { get; set; }
        public double Output2 { get; set; }
        public double Output3 { get; set; }
        public int DrawdownTime { get; set; }
    }

    public class SimulationMetrics
    {
        public double Profit { get; set; }
        public double MaxDrawdown { get; set; }
        public int MaxDrawdownTime { get; set; }
        public int ClosedTradeCount { get; set; }
        public int OrderCount { get; set; }
        public double AverageInventorySize { get; set; }
        public double MaxInventory { get; set; }
        public double AverageInventoryScore { get; set; }
        public double SharpeRatio { get; set; }
        public double SortinoRatio { get; set; }
        public double MaximumAdverseExcursion { get; set; }
    }
}
