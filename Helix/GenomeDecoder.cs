using Helix.NeuralNetwork;
using Helix.NeuralNetwork.ActivationFunctions;

namespace Helix;

public static class GenomeDecoder
{
    public static AcyclicNeuralNetwork Decode(Genome genome)
    {
        var inputCount = genome.Inputs.Count;
        var outputCount = genome.OuputNeurons.Count;
        var totalCount = genome.Inputs.Count + genome.HiddenNeurons.Count + genome.OuputNeurons.Count;

        // Activation functions
        var actFns = new IActivationFunction?[totalCount];
        for (var i = 0; i < genome.Inputs.Count; i++)
        {
            actFns[i] = null;
        }
        for (var i = 0; i < genome.HiddenNeurons.Count; i++)
        {
            // TODO: Activation function factory
            var actFn = genome.HiddenNeurons[i].ActivationFunction switch
            {
                _ => new ReLU()
            };

            actFns[genome.Inputs.Count + i] = actFn;
        }
        for (var i = 0; i < genome.OuputNeurons.Count; i++)
        {
            // TODO: Activation function factory
            var actFn = genome.OuputNeurons[i].ActivationFunction switch
            {
                _ => new ReLU()
            };

            actFns[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = actFn;
        }

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
        foreach (var output in genome.OuputNeurons)
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
            var srcNodes = genome.Connections.Where(x => x.EndIdx == id);

            var srcIds = srcNodes.Select(x => idToIdx[x.SourceIdx]).ToArray();
            var srcWeights = srcNodes.Select(x => x.Weight).ToArray();
            var srcIntegrators = srcNodes.Select(x => x.SignalIntegrator).ToArray();

            srcMap[genome.Inputs.Count + i] = srcIds;
            weightMap[genome.Inputs.Count + i] = srcWeights;
            integratorMap[genome.Inputs.Count + i] = srcIntegrators;
        }
        for (var i = 0; i < genome.OuputNeurons.Count; i++)
        {
            var id = genome.OuputNeurons[i].Id;
            var srcNodes = genome.Connections.Where(x => x.EndIdx == id);

            var srcIds = srcNodes.Select(x => idToIdx[x.SourceIdx]).ToArray();
            var srcWeights = srcNodes.Select(x => x.Weight).ToArray();
            var srcIntegrators = srcNodes.Select(x => x.SignalIntegrator).ToArray();

            srcMap[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = srcIds;
            weightMap[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = srcWeights;
            integratorMap[genome.Inputs.Count + genome.HiddenNeurons.Count + i] = srcIntegrators;
        }

        return new AcyclicNeuralNetwork(
            inputCount, outputCount, totalCount, actFns, srcMap, weightMap, integratorMap);
    }
}