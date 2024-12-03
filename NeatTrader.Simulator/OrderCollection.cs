namespace NeatTrader.Simulator;

public struct Order
{
    internal static int _idCounter = 0;

    public Order(double price, double lots)
    {
        Id = Interlocked.Increment(ref _idCounter);
        Price = price;
        Lots = lots;
        Idx = 0;
    }

    public int Id;
    public double Price;
    public double Lots;
    public int Idx;
}

public unsafe struct OrderCollection
{
    private const int MaxSize = 30000;

    // MAP: Order ID => Index
    internal Dictionary<int, int> _idMap;
    internal Stack<int> _nextInsertIdx;
    internal int _length = 0;

    // Order fields
    internal Order[] _orders;

    public OrderCollection()
    {
        _orders = new Order[MaxSize];
        _idMap = new Dictionary<int, int>();
        _nextInsertIdx = new Stack<int>();
        for (int i = MaxSize - 1; i >= 0; i--)
        {
            _nextInsertIdx.Push(i);
        }
    }

    public void Insert(Order order)
    {
        if (!_nextInsertIdx.TryPop(out var index)) return;
        _orders[index] = order;
        _idMap[order.Id] = index;
    }
    
    public void Remove(int id)
    {
        // Clear the order
        var idx = _idMap[id];
        var order = _orders[idx];
        order.Idx = 0;
        order.Lots = 0;
        order.Id = 0;
        order.Price = 0;

        // Adjust indexes
        _nextInsertIdx.Push(idx);
        _idMap.Remove(id);
    } 
        
    public Order Get(int id)
    {
        var idx = _idMap[id];
        return _orders[idx];
    }

    public void Clear()
    {
        foreach (var (id, idx) in _idMap)
        {
            _nextInsertIdx.Push(idx);
        }
        _idMap.Clear();
    }

    public void Iterate(Action<Order> action)
    {
        foreach (var (id, idx) in _idMap)
        {
            action(_orders[idx]);
        }
    }

    public IEnumerable<Order> All()
    {
        foreach (var (id, idx) in _idMap)
        {
            yield return _orders[idx];
        }
    }
}


public class FastCollection<T> where T : struct
{
    private const int MaxSize = 30000;

    // MAP: ID => Index
    internal Dictionary<int, int> _idMap;
    internal Stack<int> _nextInsertIdx;
    internal int _length = 0;
    
    internal T[] _values;

    public FastCollection()
    {
        _values = new T[MaxSize];
        _idMap = new Dictionary<int, int>();
        _nextInsertIdx = new Stack<int>();
        for (int i = MaxSize - 1; i >= 0; i--)
        {
            _nextInsertIdx.Push(i);
        }
    }

    public void Insert(T order, int id)
    {
        if (!_nextInsertIdx.TryPop(out var index)) return;
        _values[index] = order;
        _idMap[id] = index;
    }

    public void Remove(int id)
    {
        // Clear the order
        var idx = _idMap[id];

        // Adjust indexes
        _nextInsertIdx.Push(idx);
        _idMap.Remove(id);
    }

    public T Get(int id)
    {
        var idx = _idMap[id];
        return _values[idx];
    }

    public void Clear()
    {
        foreach (var (id, idx) in _idMap)
        {
            _nextInsertIdx.Push(idx);
        }
        _idMap.Clear();
    }

    public void Iterate(Action<T> action)
    {
        foreach (var (id, idx) in _idMap)
        {
            action(_values[idx]);
        }
    }

    public IEnumerable<T> All()
    {
        foreach (var (id, idx) in _idMap)
        {
            yield return _values[idx];
        }
    }
}