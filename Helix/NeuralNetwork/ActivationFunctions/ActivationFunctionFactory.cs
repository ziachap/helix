namespace Helix.NeuralNetwork.ActivationFunctions;

public static class ActivationFunctionFactory
{
    public static IActivationFunction Create(ActivationFunction actFn) => actFn switch
    {
        ActivationFunction.ReLU => new ReLU(),
        ActivationFunction.LogisticApproximantSteep => new LogisticApproximantSteep(),
        ActivationFunction.Sine => new Sin(),
        _ => throw new NotImplementedException(),
    };
}