using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
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
        private readonly Memory<IActivationFunction?> _actFns;
        private readonly Memory<int[]> _srcMap;
        private readonly Memory<double[]> _weightMap;
        private readonly Memory<SignalIntegrator[]> _integrators;

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
            Memory<IActivationFunction?> actFns,
            Memory<int[]> srcMap, 
            Memory<double[]> weightMap,
            Memory<SignalIntegrator[]> integrators)
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
            _actFns = actFns;
            _srcMap = srcMap;
            _weightMap = weightMap;
            _integrators = integrators;
        }

        public Span<double> Inputs => _preActivation.Slice(0, _inputCount).Span;
        public Span<double> Outputs => _preActivation.Slice(_totalCount - _outputCount, _outputCount).Span;

        public void Activate()
        {
            Span<double> preActivation = _preActivation.Span;
            Span<double> postActivation = _postActivation.Span;
            Span<IActivationFunction?> actFns = _actFns.Span;

            // TODO: This should ideally be a 1D contiguous array, by storing start/end idx for each node
            Span<int[]> srcMap = _srcMap.Span;
            Span<double[]> weightMap = _weightMap.Span;
            Span<SignalIntegrator[]> integrators = _integrators.Span;
            
            ref double preActivationRef = ref MemoryMarshal.GetReference(preActivation);
            ref double postActivationRef = ref MemoryMarshal.GetReference(postActivation);

            // Cycle through hidden and output nodes
            for (var i = Inputs.Length; i < _postActivation.Length; i++)
            {
                ref double post = ref Unsafe.Add(ref postActivationRef, i);

                post = 0d;
                
                // Apply signals from source nodes
                var srcIdxs = srcMap[i];
                var weights = weightMap[i];
                var actFn = actFns[i];
                var connIntegrators = integrators[i];
                
                // Summation connections
                for (var j = 0; j < srcIdxs.Length; j++)
                {
                    if (connIntegrators[j] != SignalIntegrator.Additive) continue;
                    post = Math.FusedMultiplyAdd(Unsafe.Add(ref preActivationRef, srcIdxs[j]), weights[j], post);
                }

                // Multiplication connections
                for (var j = 0; j < srcIdxs.Length; j++)
                {
                    if (connIntegrators[j] != SignalIntegrator.Multiplicative) continue;
                    post *= Unsafe.Add(ref preActivationRef, srcIdxs[j]) * weights[j];
                }

                // Apply activation function
                actFn.Fn(ref post);
            }

            // Copy post-activation to pre-activation
            for (var i = Inputs.Length; i < _postActivation.Length; i++)
            {
                ref double pre = ref Unsafe.Add(ref preActivationRef, i);
                ref double post = ref Unsafe.Add(ref postActivationRef, i);
                pre = post;
                post = 0;
            }
        }

        public void Reset()
        {
            _preActivation.Span.Fill(0);
            _postActivation.Span.Fill(0);
        }
    }
}
