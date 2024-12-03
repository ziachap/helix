namespace Helix.Genetics.Reproduction.Asexual.Mutations;

internal class MutateActivationFunctionStrategy : IMutationStrategy
{
    private readonly Random _random;

    private readonly ActivationFunction[] _activationFunctions = new ActivationFunction[]
    {
        ActivationFunction.Sine,
        ActivationFunction.LogisticApproximantSteep,
        ActivationFunction.ReLU
    };

    public MutateActivationFunctionStrategy()
    {
        _random = new Random();
    }

    public void Mutate(Genome genome)
    {
        var allNeurons = genome.HiddenNeurons
            .Cast<NeuronDescriptor>()
            .Concat(genome.OutputNeurons)
            .ToArray();

        var idx = _random.Next(0, allNeurons.Length);
        var neuron = allNeurons[idx];

        var actFns = _activationFunctions.Where(x => x != neuron.ActivationFunction).ToArray();
        var actFnIdx = _random.Next(0, actFns.Length);
        var actFn = actFns[actFnIdx];

        neuron.ActivationFunction = actFn;
    }
}