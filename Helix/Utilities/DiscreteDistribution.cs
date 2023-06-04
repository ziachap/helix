namespace Helix.Utilities;

public class DiscreteDistribution
{
    private readonly double[] _distribution;
    private readonly double _max;
    private readonly Random _random;

    public DiscreteDistribution(double[] probabilities)
    {
        if (probabilities == null) 
            throw new ArgumentNullException(nameof(probabilities));
        if (probabilities.Length == 0) 
            throw new ArgumentException("Probabilities cannot be empty", nameof(probabilities));

        _distribution = new double[probabilities.Length];
        var current = 0d;

        for (int i = 0; i < probabilities.Length; i++)
        {
            current += probabilities[i];
            _distribution[i] = current;
        }

        _max = _distribution[^1];

        _random = new Random();
    }

    /// <summary>
    /// Returns a zero-based index sampled from the distribution.
    /// </summary>
    /// <returns></returns>
    public int Sample()
    {
        var next = _random.NextDouble() * _max;

        for (var i = 0; i < _distribution.Length; i++)
        {
            if (next < _distribution[i]) return i;
        }

        throw new Exception("Sample is out of the distribution range.");
    }
}