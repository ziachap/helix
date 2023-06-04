namespace Helix.NeuralNetwork.ActivationFunctions
{
    public interface IActivationFunction
    {
        void Fn(ref double x);

        void Fn(ref double x, ref double y);
    }
}
