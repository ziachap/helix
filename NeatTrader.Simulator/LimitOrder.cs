using System;

namespace NeatTrader.Simulator;

public interface IOrder
{
    int Idx { get; set; }
    double Quantity { get; }
    Side Side { get; }
}

public class LimitOrder : IOrder
{
    private static int _orderId;
    public static int ResetOrderId() => Interlocked.Exchange(ref _orderId, 0);
    public static int OrderId() => Interlocked.Increment(ref _orderId);

    public LimitOrder(int id, double price, double quantity, Side side,
        double? stopPrice = null, Action? onFill = null)
    {
        Id = id;
        Price = price;
        Quantity = quantity;
        Side = side;
        Idx = 0;
        StopPrice = stopPrice ?? (side == Side.Buy ? 0 : double.MaxValue);
        OnFill = onFill;
    }

    public int Id { get; }
    public double Price { get; }
    public double StopPrice { get; }
    public double Quantity { get; }
    public Side Side { get; }
    public int Idx { get; set; }
    public Action? OnFill { get; }

    public override string ToString() => $"{Price} | {Quantity} | {Enum.GetName(typeof(Side), Side)}";
}

public class PeggedOrder : IOrder
{
    private static int _orderId;
    public static int ResetOrderId() => Interlocked.Exchange(ref _orderId, 0);
    public static int OrderId() => Interlocked.Increment(ref _orderId);

    public PeggedOrder(int id, double offset, double quantity, Side side)
    {
        Id = id;
        Offset = offset;
        Quantity = quantity;
        Side = side;
        Idx = 0;
    }

    public int Id { get; }
    public double Offset { get; }
    public double Quantity { get; }
    public Side Side { get; }
    public int Idx { get; set; }
    
    public override string ToString() => $"{Offset} | {Quantity} | {Enum.GetName(typeof(Side), Side)}";
}

public static class OrderExtensions
{
    //public static double GetPrice(this PeggedOrder order, double bid, double ask, double tickSize)
    //{
    //    var currentPrice = order.Side == Side.Buy ? ask - order.Offset : bid + order.Offset;
    //
    //    currentPrice = Math.Round(currentPrice / tickSize, MidpointRounding.AwayFromZero) * tickSize;
    //
    //    return currentPrice;
    //}

    public static double GetPrice(this IOrder order, double bid, double ask, double tickSize)
    {
        if (order is PeggedOrder pegOrder)
        {
            var currentPrice = pegOrder.Side == Side.Buy ? ask - pegOrder.Offset : bid + pegOrder.Offset;

            currentPrice = Math.Round(currentPrice / tickSize, MidpointRounding.AwayFromZero) * tickSize;

            return currentPrice;
        }
        else if (order is LimitOrder limitOrder)
        {
            return limitOrder.Price;
        }

        return 0;
    }
}