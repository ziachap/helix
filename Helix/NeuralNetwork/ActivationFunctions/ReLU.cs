using System.Runtime.CompilerServices;

namespace Helix.NeuralNetwork.ActivationFunctions;

/// <summary>
/// Rectified linear activation unit (ReLU).
/// </summary>
public class ReLU : IActivationFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fn(ref double x)
    {
        long xlong = Unsafe.As<double, long>(ref x);
        x = BitConverter.Int64BitsToDouble(xlong & ~(xlong >> 63));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fn(ref double x, ref double y)
    {
        long xlong = Unsafe.As<double, long>(ref x);
        y = BitConverter.Int64BitsToDouble(xlong & ~(xlong >> 63));
    }
}