namespace NeatTrader.Simulator;

public class NetPosition
{
    public double Price;
    public double Quantity;

    public override string ToString() => $"{Price} | {Quantity}";
    
    /// <summary>
    /// Consolidates the trade into the net position.
    /// </summary>
    /// <param name="trade"></param>
    /// <returns>Profit generated from the trade</returns>
    public double ConsolidateTrade(Trade trade)
    {
        if (Quantity == 0)
        {
            Price = trade.Price;
            Quantity = trade.Side == Side.Buy ? trade.Quantity : -trade.Quantity;
            return 0;
        }

        var netPrice = Price;
        var netQuantity = Quantity;
        var netQuantityAbs = Math.Abs(Quantity);

        var totalQuantity = netQuantityAbs + trade.Quantity;

        switch (trade.Side)
        {
            case Side.Buy when Quantity > 0:
                var avgPrice1 = ((Price * Quantity) + (trade.Price * trade.Quantity))
                                / totalQuantity;
                Price = avgPrice1;
                Quantity = totalQuantity;
                break;
            case Side.Sell when Quantity < 0:
                var avgPrice2 = ((Price * netQuantityAbs) + (trade.Price * trade.Quantity))
                                / totalQuantity;
                Price = avgPrice2;
                Quantity = -totalQuantity;
                break;
            case Side.Buy when Quantity < 0:
                var difference1 = netPrice - trade.Price;
                var profit1 = difference1 * Math.Min(trade.Quantity, netQuantityAbs);

                // Full close
                if (trade.Quantity == netQuantityAbs)
                {
                    Price = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (trade.Quantity <= netQuantityAbs)
                {
                    Price = netPrice;
                    Quantity = netQuantity + trade.Quantity;
                }
                // Full close and flip to long
                else
                {
                    Price = trade.Price;
                    Quantity += trade.Quantity;
                }
                
                return profit1;
            case Side.Sell when Quantity > 0:
                var difference2 = trade.Price - netPrice;
                var profit2 = difference2 * Math.Min(trade.Quantity, netQuantityAbs);

                // Full close
                if (trade.Quantity == netQuantityAbs)
                {
                    Price = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (trade.Quantity < netQuantityAbs)
                {
                    Price = netPrice;
                    Quantity = netQuantity - trade.Quantity;
                }
                // Full close and flip to short
                else
                {
                    Price = trade.Price;
                    Quantity -= trade.Quantity;
                }
                
                return profit2;
        }

        return 0;
    }

    /// <summary>
    /// Consolidates the trade into the net position.
    /// </summary>
    /// <returns>Profit generated from the trade</returns>
    public double ConsolidateTrade(Side side, double quantity, double price)
    {
        if (Quantity == 0)
        {
            Price = price;
            Quantity = side == Side.Buy ? quantity : -quantity;
            return 0;
        }

        var netPrice = Price;
        var netQuantity = Quantity;
        var netQuantityAbs = Math.Abs(Quantity);

        var totalQuantity = netQuantityAbs + quantity;

        switch (side)
        {
            case Side.Buy when Quantity > 0:
                var avgPrice1 = ((Price * Quantity) + (price * quantity))
                                / totalQuantity;
                Price = avgPrice1;
                Quantity = totalQuantity;
                break;
            case Side.Sell when Quantity < 0:
                var avgPrice2 = ((Price * netQuantityAbs) + (price * quantity))
                                / totalQuantity;
                Price = avgPrice2;
                Quantity = -totalQuantity;
                break;
            case Side.Buy when Quantity < 0:
                var difference1 = netPrice - price;
                var profit1 = difference1 * Math.Min(quantity, netQuantityAbs);

                // Full close
                if (quantity == netQuantityAbs)
                {
                    Price = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (quantity <= netQuantityAbs)
                {
                    Price = netPrice;
                    Quantity = netQuantity + quantity;
                }
                // Full close and flip to long
                else
                {
                    Price = price;
                    Quantity += quantity;
                }

                return profit1;
            case Side.Sell when Quantity > 0:
                var difference2 = price - netPrice;
                var profit2 = difference2 * Math.Min(quantity, netQuantityAbs);

                // Full close
                if (quantity == netQuantityAbs)
                {
                    Price = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (quantity < netQuantityAbs)
                {
                    Price = netPrice;
                    Quantity = netQuantity - quantity;
                }
                // Full close and flip to short
                else
                {
                    Price = price;
                    Quantity -= quantity;
                }

                return profit2;
        }

        return 0;
    }
}