// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.

using Helix.Genetics;

namespace Helix.Forms.Graph;

/// <summary>
/// Represents a directed graph, with supplementary data suitable for producing a 2D visual representation of the graph.
/// </summary>
public sealed class DirectedGraphViewModel
{
    /// <summary>
    /// Represents the directed graph topology.
    /// </summary>
    public Genome Genome { get; }

    /// <summary>
    /// Graph connection/vertex weights.
    /// </summary>
    public float[] WeightArr { get; }

    /// <summary>
    /// Provides a ID/label for each node.
    /// </summary>
    public int[] NodeIdByIdx { get; }

    /// <summary>
    /// Provides a 2D position for each node.
    /// </summary>
    public Point[] NodePosByIdx { get; }

    /// <summary>
    /// Provides a mapping from ID to activation function name.
    /// </summary>
    public Dictionary<int, ActivationFunction> NodeActivationFunctions { get; }

    /// <summary>
    /// Mapping from node ID to label.
    /// </summary>
    public Dictionary<int, string> NodeLabels { get; }

    #region Construction

    /// <summary>
    /// Construct with the provided digraph, weights, and node IDs.
    /// The node positions array is allocated, but must be updated with actual positions outside of this constructor.
    /// </summary>
    /// <param name="digraph">Directed graph.</param>
    /// <param name="weightArr">Graph connection/vertex weights.</param>
    /// <param name="nodeIdByIdx">Provides a ID/label for each node.</param>
    public DirectedGraphViewModel(
        Genome genome,
        float[] weightArr,
        Dictionary<int, ActivationFunction> nodeActivationFunctions, 
        Dictionary<int, string> nodeLabels)
    {
        if(weightArr.Length != genome.Connections.Count)
            throw new ArgumentException("Weight and connection ID arrays must have same length.", nameof(weightArr));
        
        Genome = genome;
        WeightArr = weightArr;
        NodeIdByIdx = genome.AllNodeIds();
        NodeActivationFunctions = nodeActivationFunctions;
        NodeLabels = nodeLabels;
        NodePosByIdx = new Point[genome.TotalNodeCount()];
    }

    /// <summary>
    /// Construct with the provided digraph, weights, node IDs, and node positions.
    /// </summary>
    /// <param name="digraph">Directed graph.</param>
    /// <param name="weightArr">Graph connection/vertex weights.</param>
    /// <param name="nodeIdByIdx">Provides a ID/label for each node.</param>
    /// <param name="nodePosByIdx">Provides a 2D position for each node.</param>
    public DirectedGraphViewModel(
        Genome genome,
        float[] weightArr,
        Point[] nodePosByIdx, 
        Dictionary<int, ActivationFunction> nodeActivationFunctions,
        Dictionary<int, string> nodeLabels)
    {
        if(weightArr.Length != genome.Connections.Count)
            throw new ArgumentException("Weight and connection ID arrays must have same length.", nameof(weightArr));

        //if(nodeIdByIdx.Count != digraph.TotalNodeCount)
        //    throw new ArgumentException("Node counts must match.", nameof(nodeIdByIdx));

        if(nodePosByIdx.Length != genome.TotalNodeCount())
            throw new ArgumentException("Node counts must match.", nameof(nodePosByIdx));

        Genome = genome;
        WeightArr = weightArr;
        NodeIdByIdx = genome.AllNodeIds();
        NodePosByIdx = nodePosByIdx;
        NodeActivationFunctions = nodeActivationFunctions;
        NodeLabels = nodeLabels;
    }

    #endregion
}
