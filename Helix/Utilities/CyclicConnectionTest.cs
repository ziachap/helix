using System;
using Helix.Genetics;

namespace Helix.Utilities
{
    internal static class CyclicConnectionTest
    {
        public static IEnumerable<int> CyclicNodeIds(this List<Connection> connections)
        {
            return CyclicNodeIds(connections, new List<int>()).Distinct();
        }
        
        private static IEnumerable<int> CyclicNodeIds(List<Connection> connections, List<int> visited)
        {
            var cyclicIds = new List<int>();

            foreach (var connection in connections.ToList())
            {
                visited.Add(connection.SourceId);

                if (visited.Contains(connection.DestinationId))
                {
                    var idx = visited.IndexOf(connection.DestinationId);
                    return visited.GetRange(idx, visited.Count - idx).ToList();
                }

                var destinationConnections = connections
                    .Where(x => x.SourceId == connection.DestinationId)
                    .ToList();

                cyclicIds.AddRange(CyclicNodeIds(destinationConnections, visited));
            }

            return cyclicIds;
        }
    }
}
