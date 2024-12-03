using System;
using System.Security.Cryptography.X509Certificates;
using NeatTrader.Shared;
using NeatTrader.TickDataProcessor.Data;

namespace NeatTrader.Simulator;

public class SimulatedMarket
{
    public const int Depth = 5;
    public const double ContractSize = 1;

    private readonly Random _random = new Random();

    public event Action<int, double>? OnBuy;
    public event Action<int, double>? OnSell;

    private int Idx { get; set; }
    private DateTime Time { get; set; }
    private Queue<DateTime> RequestTimes { get; set; }
    public BookSide Bids { get; }
    public BookSide Asks { get; }
    public List<Trade> Trades { get; }
    public List<IOrder> Orders { get; }
    public NetPosition Position { get; }

    public double Balance { get; private set; }

    public double Equity()
    {
        var b1 = Bids.Orders[0];
        var a1 = Asks.Orders[0];

        //var imbalance = b1.Quantity / (b1.Quantity + a1.Quantity);
        //var weightedMid = imbalance * a1.Price + (1 - imbalance) * b1.Price;

        var weightedMid = (b1.Price + a1.Price) / 2;
        
        var diff = weightedMid - Position.Price;
        var profit = Position.Quantity * diff * SimulatedMarket.ContractSize;
        var equity = Balance + profit;

        return equity;
    }

    public SimulatedMarket()
    {
        Balance = 100000;
        Trades = new List<Trade>();
        Orders = new List<IOrder>();
        Bids = new BookSide(BookSideType.Bid)
        {
            OnOrderActivated = order => Orders.Add(order)
        };
        Asks = new BookSide(BookSideType.Ask)
        {
            OnOrderActivated = order => Orders.Add(order)
        };
        Position = new NetPosition();
        LimitOrder.ResetOrderId();
        PeggedOrder.ResetOrderId();
        RequestTimes = new Queue<DateTime>();
    }

    public void Clear()
    {
        Balance = 100000;
        Idx = 0;
        Trades.Clear();
        Orders.Clear();
        Bids.Clear();
        Asks.Clear();
        Position.Price = 0;
        Position.Quantity = 0;
        LimitOrder.ResetOrderId();
        PeggedOrder.ResetOrderId();
        OnBuy = null;
        OnSell = null;
        RequestTimes.Clear();
    }
    
    public void SetOrderBook(TickSlice slice, int idx)
    {
        Bids.Orders[0].Price = slice.B1;
        Bids.Orders[0].Quantity = slice.B1V;
        Bids.Orders[1].Price = slice.B2;
        Bids.Orders[1].Quantity = slice.B2V;
        Bids.Orders[2].Price = slice.B3;
        Bids.Orders[2].Quantity = slice.B3V;
        Bids.Orders[3].Price = slice.B4;
        Bids.Orders[3].Quantity = slice.B4V;
        Bids.Orders[4].Price = slice.B5;
        Bids.Orders[4].Quantity = slice.B5V;
        
        Asks.Orders[0].Price = slice.A1;
        Asks.Orders[0].Quantity = slice.A1V;
        Asks.Orders[1].Price = slice.A2;
        Asks.Orders[1].Quantity = slice.A2V;
        Asks.Orders[2].Price = slice.A3;
        Asks.Orders[2].Quantity = slice.A3V;
        Asks.Orders[3].Price = slice.A4;
        Asks.Orders[3].Quantity = slice.A4V;
        Asks.Orders[4].Price = slice.A5;
        Asks.Orders[4].Quantity = slice.A5V;

        Idx = idx;
        Time = slice.TimestampRaw.UnixTimeStampToDateTime();
        while (RequestTimes.TryPeek(out var rt) && Time > rt.AddSeconds(1))
        {
            RequestTimes.Dequeue();
        }
    }

    public void ActivateOrders()
    {
        Bids.ActivatePendingOrder();
        Asks.ActivatePendingOrder();
    }

    /// <summary>
    /// Buy at market price
    /// </summary>
    public void ExecuteMarketBid(double quantity)
    {
        if (RequestTimes.Count >= 2) return;

        var price = Asks.Orders[0].Price;
        var id = LimitOrder.OrderId();

        var order = new LimitOrder(id, price, quantity, Side.Buy)
        {
            Idx = Idx
        };

        Orders.Add(order);

        var trade = new Trade(price, order.Quantity, order.Side)
        {
            Idx = Idx
        };
        
        trade.Profit = Position.ConsolidateTrade(trade);
        Trades.Add(trade);

        Balance += trade.Profit * ContractSize;

        RequestTimes.Enqueue(Time);

        OnBuy?.Invoke(Idx, price);
    }

    /// <summary>
    /// Sell at market price
    /// </summary>
    public void ExecuteMarketAsk(double quantity)
    {
        if (RequestTimes.Count >= 2) return;

        var price = Bids.Orders[0].Price;
        var id = LimitOrder.OrderId();

        var order = new LimitOrder(id, price, quantity, Side.Sell)
        {
            Idx = Idx
        };

        Orders.Add(order);

        var trade = new Trade(price, order.Quantity, order.Side)
        {
            Idx = Idx
        };

        trade.Profit = Position.ConsolidateTrade(trade);
        Trades.Add(trade);

        Balance += trade.Profit * ContractSize;

        RequestTimes.Enqueue(Time);

        OnBuy?.Invoke(Idx, price);
    }

    /// <summary>
    /// Set the level you want to buy at.
    /// </summary>
    public void SetBidOrder(double price, double quantity, double? stopPrice = null, Action? onFill = null)
    {
        if (Bids.MyOrder is LimitOrder currentOrder && currentOrder.Price == price) return;

        if (RequestTimes.Count >= 2) return;

        var id = LimitOrder.OrderId();
        var order = new LimitOrder(id, price, quantity, Side.Buy, stopPrice, onFill)
        {
            Idx = Idx
        };

        Bids.QueueOrder(order);

        RequestTimes.Enqueue(Time);
    }

    /// <summary>
    /// Set the level you want to sell at.
    /// </summary>
    public void SetAskOrder(double price, double quantity, double? stopPrice = null, Action? onFill = null)
    {
        if (Asks.MyOrder is LimitOrder currentOrder && currentOrder.Price == price) return;

        if (RequestTimes.Count >= 2) return;

        var id = LimitOrder.OrderId();
        var order = new LimitOrder(id, price, quantity, Side.Sell, stopPrice, onFill)
        {
            Idx = Idx
        };

        Asks.QueueOrder(order);

        RequestTimes.Enqueue(Time);
    }

    /// <summary>
    /// Set the level you want to peg a buy at.
    /// </summary>
    public void SetPeggedBidOrder(double offset, double quantity)
    {
        if (Bids.MyOrder is PeggedOrder currentOrder && currentOrder.Offset == offset) return;

        var id = LimitOrder.OrderId();
        var order = new PeggedOrder(id, offset, quantity, Side.Buy)
        {
            Idx = Idx
        };

        Bids.QueueOrder(order);
    }

    /// <summary>
    /// Set the level you want to peg a sell at.
    /// </summary>
    public void SetPeggedAskOrder(double offset, double quantity)
    {
        if (Asks.MyOrder is PeggedOrder currentOrder && currentOrder.Offset == offset) return;

        var id = LimitOrder.OrderId();
        var order = new PeggedOrder(id, offset, quantity, Side.Sell)
        {
            Idx = Idx
        };

        Asks.QueueOrder(order);
    }

    public void CancelBidOrder()
    {
        Bids.RemoveOrder();
    }

    public void CancelAskOrder()
    {
        Asks.RemoveOrder();
    }
    
    /// <summary>
    /// Executes most probable market order and fills it according to the best prices in the current order book.
    /// </summary>
    public void ExecuteProbabilisticMarketOrders(TickSlice previousSlice, TickSlice slice)
    {
        const double tradeFee = 0.35;

        var bidQuantityMoved = FlowEstimator.EstimateBidQuantityHit(previousSlice, slice);
        var askQuantityMoved = FlowEstimator.EstimateAskQuantityHit(previousSlice, slice);

        // Randomize size
        const double scale = 0.99;

        if (askQuantityMoved > 0)
        {
            //var requiredSize = Math.Abs(_random.NextGaussian(askQuantityMoved, 5)) * scale;
            var requiredSize = askQuantityMoved * scale;
            if (Asks.TryMatchMarketOrder(Side.Buy, requiredSize, out var trade))
            {
                trade.Idx = Idx;
                trade.Profit = Position.ConsolidateTrade(trade);
                Trades.Add(trade);

                //Balance -= tradeFee;
                Balance += trade.Profit * ContractSize;

                Asks.RemoveOrder();

                OnSell?.Invoke(Idx, trade.Price);
            }
        }
        else if (bidQuantityMoved > 0)
        {
            //var requiredSize = Math.Abs(_random.NextGaussian(bidQuantityMoved, 5)) * scale;
            var requiredSize = bidQuantityMoved * scale;
            if (Bids.TryMatchMarketOrder(Side.Sell, requiredSize, out var trade))
            {
                trade.Idx = Idx;
                trade.Profit = Position.ConsolidateTrade(trade);
                Trades.Add(trade);

                //Balance -= tradeFee;
                Balance += trade.Profit * ContractSize;

                Bids.RemoveOrder();

                OnBuy?.Invoke(Idx, trade.Price);
            }
        }
    }

    /// <summary>
    /// Attempts to fill my orders if the spread has crossed the orders
    /// </summary>
    public void FillOrdersCrossingSpread()
    {
        const double tradeFee = 0.35;

        var bestAsk = Asks.Orders[0];
        var bestBid = Bids.Orders[0];

        // Limit orders
        if (Asks.MyOrder is not null && Asks.MyOrder is LimitOrder myAsk && bestBid.Price >= myAsk.Price)
        {
            var trade = new Trade(myAsk.Price, myAsk.Quantity, myAsk.Side)
            {
                Idx = Idx
            };

            trade.Profit = Position.ConsolidateTrade(trade);
            Trades.Add(trade);

            //Balance -= tradeFee;
            Balance += trade.Profit * ContractSize;

            Asks.RemoveOrder();

            OnSell?.Invoke(Idx, trade.Price);

            myAsk.OnFill?.Invoke();
        }

        if (Bids.MyOrder is not null && Bids.MyOrder is LimitOrder myBid && bestAsk.Price <= myBid.Price)
        {
            var trade = new Trade(myBid.Price, myBid.Quantity, myBid.Side)
            {
                Idx = Idx
            };

            trade.Profit = Position.ConsolidateTrade(trade);
            Trades.Add(trade);

            //Balance -= tradeFee;
            Balance += trade.Profit * ContractSize;

            Bids.RemoveOrder();

            OnBuy?.Invoke(Idx, trade.Price);

            myBid.OnFill?.Invoke();
        }
    }
}

/// <summary>
/// Some extension methods for <see cref="Random"/> for creating a few more kinds of random stuff.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    ///   Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
    /// </summary>
    /// <param name="r"></param>
    /// <param name = "mu">Mean of the distribution</param>
    /// <param name = "sigma">Standard deviation</param>
    /// <returns></returns>
    public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
    {
        var u1 = r.NextDouble();
        var u2 = r.NextDouble();

        var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                              Math.Sin(2.0 * Math.PI * u2);

        var rand_normal = mu + sigma * rand_std_normal;

        return rand_normal;
    }
}


public enum Side
{
    Buy, Sell
}

public class BookEntry
{
    public BookEntry(double price, double quantity)
    {
        Price = price;
        Quantity = quantity;
    }

    public double Price;
    public double Quantity;

    public override string ToString() => $"{Price} | {Quantity}";
}

public class Trade
{
    public Trade(double price, double quantity, Side side)
    {
        Price = price;
        Quantity = quantity;
        Side = side;
        Idx = 0;
        Profit = 0;
    }

    public double Price { get; }
    public double Quantity { get; }
    public Side Side { get; }
    public int Idx { get; set; }
    public double Profit { get; set; }

    public override string ToString() => $"{Price} | {Quantity} | {Enum.GetName(typeof(Side), Side)}";
}