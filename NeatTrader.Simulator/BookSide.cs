namespace NeatTrader.Simulator;

public class BookSide
{
    public Action<IOrder>? OnOrderActivated;

    public BookSide(BookSideType bookType)
    {
        BookType = bookType;
        Orders = new BookEntry[SimulatedMarket.Depth];
        PendingOrders = new List<PendingOrder>();
    }

    public void Clear()
    {
        Orders[0] = new BookEntry(0, 0);
        Orders[1] = new BookEntry(0, 0);
        Orders[2] = new BookEntry(0, 0);
        Orders[3] = new BookEntry(0, 0);
        Orders[4] = new BookEntry(0, 0);
        MyOrder = null;
        PendingOrders.Clear();
    }
    
    public BookSideType BookType { get; }
    public BookEntry[] Orders { get; }
    
    public IOrder? MyOrder { get; private set; }
    public IOrder? PendingOrder { get; private set; }
    public List<PendingOrder> PendingOrders { get; }

    private readonly Random _random = new Random();
    private const int MinTickCountdown = 1;
    private const int MaxTickCountdown = 1;
    
    public void QueueOrder(IOrder order)
    {
        PendingOrder = order;
        return;

        var pendingOrder = new PendingOrder(order,
            o => MyOrder = o, _random.Next(MinTickCountdown, MaxTickCountdown));
        PendingOrders.Add(pendingOrder);
    }

    public void RemoveOrder()
    {
        MyOrder = null;
        PendingOrder = null;
    }

    public void ActivatePendingOrder()
    {
        if (PendingOrder != null)
        {
            MyOrder = PendingOrder;
            PendingOrder = null;
        }
        return;
        var toRemove = new List<PendingOrder>();

        foreach (var pendingOrder in PendingOrders)
        {
            if (pendingOrder.PendingTickCountdown <= 0)
            {
                pendingOrder.OrderActivate?.Invoke(pendingOrder.Order);
                toRemove.Add(pendingOrder);
                OnOrderActivated?.Invoke(MyOrder);
            }
            else
            {
                pendingOrder.PendingTickCountdown--;
            }
        }
        
        foreach (var pendingLimitOrder in toRemove)
        {
            PendingOrders.Remove(pendingLimitOrder);
        }
    }

    /// <summary>
    /// Attempts to match market order to the order book and returns true if our order was filled.
    /// </summary>
    public bool TryMatchMarketOrder(Side side, double quantity, out Trade myTrade)
    {
        myTrade = default;

        if (BookType == BookSideType.Bid && side == Side.Buy)
            throw new Exception("Buy order cannot be matched with bid book.");
        if (BookType == BookSideType.Ask && side == Side.Sell)
            throw new Exception("Sell order cannot be matched with ask book.");

        if (MyOrder == null) return false;
        var myLimit = (LimitOrder)MyOrder;

        // Match order
        var filled = 0d;
        // Step through corresponding side of the book
        foreach (var bookEntry in Orders)
        {
            var entryQuantity = bookEntry.Quantity;
            var entryPrice = bookEntry.Price;
            var leftToFill = quantity - filled;

            // If I have the better price, fill my order immediately
            if (side == Side.Buy)
            {
                if (myLimit.Price < bookEntry.Price)
                {
                    myTrade = MakeTradeFromMyOrder();
                    return true;
                }
            }
            else
            {
                if (myLimit.Price > bookEntry.Price)
                {
                    myTrade = MakeTradeFromMyOrder();
                    return true;
                }
            }

            // Filled rest of order with existing orders
            if (leftToFill <= entryQuantity) return false;

            // Fill partially with existing orders
            filled += entryQuantity;

            // Evaluate my order at the back of the queue
            if (Math.Abs(myLimit.Price - entryPrice) < 0.000001)
            {
                myTrade = MakeTradeFromMyOrder();
                return true;
            }

            if (filled >= quantity) return false;
        }

        return false;

        Trade MakeTradeFromMyOrder()
        {
            return new Trade(myLimit.Price, myLimit.Quantity, myLimit.Side);
        }
    }
}

public enum BookSideType
{
    Bid, Ask
}

public class PendingOrder
{
    public PendingOrder(IOrder order, Action<IOrder> orderActivate, int pendingTickCountdown)
    {
        Order = order;
        OrderActivate = orderActivate;
        PendingTickCountdown = pendingTickCountdown;
    }

    public IOrder Order { get; }
    public Action<IOrder> OrderActivate { get; }
    public int PendingTickCountdown { get; set; }
}