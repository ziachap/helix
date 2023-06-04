namespace Helix.NeuralNetwork.ActivationFunctions;

/// <summary>
/// The Sin function.
/// </summary>
public sealed class Sin : IActivationFunction
{
    public void Fn(ref double x)
    {
        x = Math.Sin(x);
    }
    
    public void Fn(ref double x, ref double y)
    {
        y = Math.Sin(x);
    }
}
