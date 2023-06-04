using System.Runtime.CompilerServices;

namespace Helix.NeuralNetwork.ActivationFunctions;

/// <summary>
/// The logistic function with a steepened slope, and implemented using a fast to compute approximation of exp().
/// </summary>
public class LogisticApproximantSteep : IActivationFunction
{
    public void Fn(ref double x)
    {
        x = 1.0 / (1.0 + ExpApprox(-4.9 * x));
    }
    
    public void Fn(ref double x, ref double y)
    {
        y = 1.0 / (1.0 + ExpApprox(-4.9 * x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ExpApprox(double val)
    {
        long tmp = (long)((1512775 * val) + (1072693248 - 60801));
        return BitConverter.Int64BitsToDouble(tmp << 32);
    }
}