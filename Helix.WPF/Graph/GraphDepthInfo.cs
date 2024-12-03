﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using System.Diagnostics;

namespace SharpNeat.Graphs.Acyclic;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

/// <summary>
/// Conveys summary information from a network depth analysis.
/// </summary>
public sealed class GraphDepthInfo : IEquatable<GraphDepthInfo>
{
    /// <summary>
    /// The total depth of the graph.
    /// This is the highest value within _nodeDepths, + 1 (because the first layer is layer 0).
    /// </summary>
    public int GraphDepth { get; }
    
    /// <summary>
    /// An array containing the depth of each node in the digraph.
    /// </summary>
    public int[] NodeDepthArr { get; }

    /// <summary>
    /// Construct with the provided info.
    /// </summary>
    /// <param name="graphDepth">The total depth of the graph.</param>
    /// <param name="nodeDepthArr">An array containing the depth of each node in the digraph.</param>
    public GraphDepthInfo(int graphDepth, int[] nodeDepthArr)
    {
        Debug.Assert(graphDepth >= 0);
        Debug.Assert(nodeDepthArr is not null);

        GraphDepth = graphDepth;
        NodeDepthArr = nodeDepthArr;
    }

    #region IEquatable

    /// <summary>
    /// Determines whether the specified <see cref="GraphDepthInfo" /> is equal to the current <see cref="GraphDepthInfo" />.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>true if the objects are equal; otherwise false.</returns>
    public bool Equals(GraphDepthInfo? other)
    {
        return other is not null
            && GraphDepth == other.GraphDepth
            && NodeDepthArr.SequenceEqual(other.NodeDepthArr);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as GraphDepthInfo);
    }

    #endregion
}
