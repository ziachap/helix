using Helix.NeuralNetwork;
using Helix.NeuralNetwork.ActivationFunctions;

namespace Helix;

public static class GenomeDecoder
{
    public static AcyclicNeuralNetwork DecodeAcyclic(Genome genome)
    {
        var inputCount = genome.Inputs.Count;
        var outputCount = genome.OutputNeurons.Count;
        var totalCount = genome.Inputs.Count + genome.HiddenNeurons.Count + genome.OutputNeurons.Count;

        // Activation functions
        var actFns = new IActivationFunction?[totalCount];
        for (var i = 0; i < genome.Inputs.Count; i++)
        {
            actFns[i] = null;
        }
        for (var i = 0; i < genome.HiddenNeurons.Count; i++)
        {
            var actFn = genome.HiddenNeurons[i].ActivationFunction;
            actFns[genome.Inputs.Count + i] = ActivationFunctionFactory.Create(actFn);
        }
        for (var i = 0; i < genome.OutputNeurons.Count; i++)
        {
            var actFn = genome.OutputNeurons[i].ActivationFunction;
            actFns[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = ActivationFunctionFactory.Create(actFn);
        }
        
        var (srcMap, weightMap, integratorMap) = CreateMaps(genome);

        return new AcyclicNeuralNetwork(
            inputCount, outputCount, totalCount, actFns, srcMap, weightMap, integratorMap);
    }

    public static CyclicNeuralNetwork Decode(Genome genome)
    {
        var inputCount = genome.Inputs.Count;
        var outputCount = genome.OutputNeurons.Count;
        var totalCount = genome.Inputs.Count + genome.HiddenNeurons.Count + genome.OutputNeurons.Count;

        // Activation functions
        var actFns = new IActivationFunction?[totalCount];
        for (var i = 0; i < genome.Inputs.Count; i++)
        {
            actFns[i] = null;
        }

        for (var i = 0; i < genome.HiddenNeurons.Count; i++)
        {
            var actFn = genome.HiddenNeurons[i].ActivationFunction;
            actFns[genome.Inputs.Count + i] = ActivationFunctionFactory.Create(actFn);
        }

        for (var i = 0; i < genome.OutputNeurons.Count; i++)
        {
            var actFn = genome.OutputNeurons[i].ActivationFunction;
            actFns[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = ActivationFunctionFactory.Create(actFn);
        }

        var (srcMap, weightMap, integratorMap) = CreateMaps(genome);
        
        return new CyclicNeuralNetwork(
            inputCount, outputCount, totalCount, actFns, srcMap, weightMap, integratorMap);
    }

    private static (int[][] srcMap, double[][] weightMap, SignalIntegrator[][] integratorMap) CreateMaps(Genome genome)
    {
        var totalCount = genome.Inputs.Count + genome.HiddenNeurons.Count + genome.OutputNeurons.Count;

        // Map from Id to idx
        Dictionary<int, int> idToIdx = new Dictionary<int, int>();
        var idx = 0;
        foreach (var input in genome.Inputs)
        {
            idToIdx[input.Id] = idx;
            idx++;
        }

        foreach (var hidden in genome.HiddenNeurons)
        {
            idToIdx[hidden.Id] = idx;
            idx++;
        }

        foreach (var output in genome.OutputNeurons)
        {
            idToIdx[output.Id] = idx;
            idx++;
        }

        // Src idx, weights and integrators map

        var srcMap = new int[totalCount][];
        var weightMap = new double[totalCount][];
        var integratorMap = new SignalIntegrator[totalCount][];
        for (var i = 0; i < genome.Inputs.Count; i++)
        {
            srcMap[i] = Array.Empty<int>();
            weightMap[i] = Array.Empty<double>();
            integratorMap[i] = Array.Empty<SignalIntegrator>();
        }

        for (var i = 0; i < genome.HiddenNeurons.Count; i++)
        {
            var id = genome.HiddenNeurons[i].Id;
            var srcNodes = genome.Connections.Where(x => x.DestinationIdx == id);

            var srcIds = srcNodes.Select(x => idToIdx[x.SourceIdx]).ToArray();
            var srcWeights = srcNodes.Select(x => x.Weight).ToArray();
            var srcIntegrators = srcNodes.Select(x => x.SignalIntegrator).ToArray();

            srcMap[genome.Inputs.Count + i] = srcIds;
            weightMap[genome.Inputs.Count + i] = srcWeights;
            integratorMap[genome.Inputs.Count + i] = srcIntegrators;
        }

        for (var i = 0; i < genome.OutputNeurons.Count; i++)
        {
            var id = genome.OutputNeurons[i].Id;
            var srcNodes = genome.Connections.Where(x => x.DestinationIdx == id);

            var srcIds = srcNodes.Select(x => idToIdx[x.SourceIdx]).ToArray();
            var srcWeights = srcNodes.Select(x => x.Weight).ToArray();
            var srcIntegrators = srcNodes.Select(x => x.SignalIntegrator).ToArray();

            srcMap[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = srcIds;
            weightMap[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = srcWeights;
            integratorMap[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = srcIntegrators;
        }

        return (srcMap, weightMap, integratorMap);
    }

}