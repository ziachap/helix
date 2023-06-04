﻿namespace Helix.Utilities;

public class GaussianDistribution
{
    private readonly Random _random;

    public GaussianDistribution()
    {
        _random = new Random();
    }

    /// <summary>
    /// Returns a random number sampled from the gaussian distribution.
    /// </summary>
    /// <returns></returns>
    public double Sample(double mean = 0, double stdDev = 1)
    {
        double u1 = 1.0 - _random.NextDouble();
        double u2 = 1.0 - _random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                               Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}