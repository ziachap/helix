using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Helix.Genetics;
using Helix.NeuralNetwork.ActivationFunctions;

namespace Helix.NeuralNetwork
{
    public class CyclicNeuralNetwork : IBlackBox
    {
        private readonly int _inputCount;
        private readonly int _outputCount;
        private readonly int _totalCount;

        // All these arrays should be the same length

        internal readonly Memory<double> _preActivation;
        private readonly Memory<double> _postActivation;
        private readonly Memory<AggregationFunction?> _aggrFns;
        private readonly Memory<IActivationFunction?> _actFns;
        private readonly Memory<int[]> _srcMap;
        private readonly Memory<double[]> _weightMap;
        private readonly Memory<ConnectionIntegrator[]> _integrators;
        private readonly Memory<bool> _cyclic;

        // For all nodes...
        // Use a working value array to store each node's current output value.

        // For neuronal nodes...
        // Given a hidden/output node layout such as [I, I, H, H, H, O, O]

        // Use an signal integration fn array [null, null, Sum, Sum, Mult, Mult, Sum]
        // Use an activation fn array [null, null, ReLU, ReLU, Sin, Exp, Logistic]

        // A reverse idx connection map [[], [], [0, 1], [3, 5, 6], etc...]
        // To instantly get the indexes of the source nodes to retrieve source node's output

        // A reverse weight connection map [[], [], [2, 1.2], [-0.1, 3.1, 3.1], etc...]
        // To instantly get the connection weights corresponding to the above idx map above.

        public CyclicNeuralNetwork(
            int inputCount, 
            int outputCount,
            int totalCount,
            Memory<AggregationFunction?> aggrFns,
            Memory<IActivationFunction?> actFns,
            Memory<int[]> srcMap, 
            Memory<double[]> weightMap,
            Memory<ConnectionIntegrator[]> integrators,
            Memory<bool> cyclic
            )
        {
            _inputCount = inputCount;
            _outputCount = outputCount;
            _totalCount = totalCount;

            Debug.Assert(integrators.Length == totalCount);
            Debug.Assert(actFns.Length == totalCount);
            Debug.Assert(srcMap.Length == totalCount);
            Debug.Assert(weightMap.Length == totalCount);

            _preActivation = new double[totalCount];
            _postActivation = new double[totalCount];
            _aggrFns = aggrFns;
            _actFns = actFns;
            _srcMap = srcMap;
            _weightMap = weightMap;
            _integrators = integrators;
            _cyclic = cyclic;
        }

        public Span<double> Inputs => _preActivation.Slice(0, _inputCount).Span;
        public Span<double> Outputs => _preActivation.Slice(_totalCount - _outputCount, _outputCount).Span;

        public void Activate()
        {
            Span<double> preActivation = _preActivation.Span;
            Span<double> postActivation = _postActivation.Span;
            Span<AggregationFunction?> aggrFns = _aggrFns.Span;
            Span<IActivationFunction?> actFns = _actFns.Span;
            Span<bool> cyclic = _cyclic.Span;

            // TODO: This should ideally be a 1D contiguous array, by storing start/end idx for each node
            Span<int[]> srcMap = _srcMap.Span;
            Span<double[]> weightMap = _weightMap.Span;
            Span<ConnectionIntegrator[]> integrators = _integrators.Span;
            
            ref double preActivationRef = ref MemoryMarshal.GetReference(preActivation);
            ref double postActivationRef = ref MemoryMarshal.GetReference(postActivation);
            
            const int passes = 2; // TODO: consider making this configurable
            for (var cycle = 0; cycle < passes; cycle++)
            {
                // Cycle through hidden and output nodes
                for (var i = _inputCount; i < _totalCount; i++)
                {
                    ref double post = ref Unsafe.Add(ref postActivationRef, i);
                    ref double pre = ref Unsafe.Add(ref preActivationRef, i);

                    post = 0d;

                    // Apply signals from source nodes
                    var srcIdxs = srcMap[i];
                    var weights = weightMap[i];
                    var aggrFn = aggrFns[i];
                    var actFn = actFns[i];
                    var connIntegrators = integrators[i];
                    var isCyclic = cyclic[i];

                    // TODO: Aggregation function should be stored as an implementation inside the network
                    // Like how activation fns are stored

                    // Apply aggregate connections
                    if (aggrFn == AggregationFunction.Sum)
                    {
                        for (var j = 0; j < srcIdxs.Length; j++)
                        {
                            if (connIntegrators[j] != ConnectionIntegrator.Aggregate) continue;
                            post = Math.FusedMultiplyAdd(Unsafe.Add(ref preActivationRef, srcIdxs[j]), weights[j], post);
                        }
                    }
                    else if (aggrFn == AggregationFunction.Average)
                    {
                        for (var j = 0; j < srcIdxs.Length; j++)
                        {
                            if (connIntegrators[j] != ConnectionIntegrator.Aggregate) continue;
                            post = Math.FusedMultiplyAdd(Unsafe.Add(ref preActivationRef, srcIdxs[j]), weights[j], post);
                        }
                        post /= srcIdxs.Length;
                    }
                    else if (aggrFn == AggregationFunction.Max && srcIdxs.Length > 0)
                    {
                        post = Unsafe.Add(ref preActivationRef, srcIdxs[0]) * weights[0];

                        for (var j = 1; j < srcIdxs.Length; j++)
                        {
                            if (connIntegrators[j] != ConnectionIntegrator.Aggregate) continue;
                            post = Math.Max(Unsafe.Add(ref preActivationRef, srcIdxs[j]) * weights[j], post);
                        }
                    }
                    else if (aggrFn == AggregationFunction.Max && srcIdxs.Length > 0)
                    {
                        post = Unsafe.Add(ref preActivationRef, srcIdxs[0]) * weights[0];

                        for (var j = 1; j < srcIdxs.Length; j++)
                        {
                            if (connIntegrators[j] != ConnectionIntegrator.Aggregate) continue;
                            post = Math.Max(Unsafe.Add(ref preActivationRef, srcIdxs[j]) * weights[j], post);
                        }
                    }

                    // Apply modulation connections (multiplicative)
                    for (var j = 0; j < srcIdxs.Length; j++)
                    {
                        if (connIntegrators[j] != ConnectionIntegrator.Modulate) continue;
                        post *= Unsafe.Add(ref preActivationRef, srcIdxs[j]) * weights[j];
                    }

                    // Apply activation function
                    actFn.Fn(ref post);
                    
                    //if (!isCyclic) pre = post;

                    pre = post;
                }

                // Copy post-activation to pre-activation
                for (var i = _inputCount; i < _totalCount; i++)
                {
                    ref double pre = ref Unsafe.Add(ref preActivationRef, i);
                    ref double post = ref Unsafe.Add(ref postActivationRef, i);
                    pre = post;
                    post = 0;
                }
            }
        }

        public void Reset()
        {
            _preActivation.Span.Fill(0);
            _postActivation.Span.Fill(0);
        }
    }
}
