using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helix.Genetics;

namespace Helix.Speciation
{
    public static class GenomeDistanceMeasure
    {
        public static double Distance(Genome g1, Genome g2)
        {
            // This does both excess and disjoint
            var g1ConnExcess = g1.Connections.Except(g2.Connections).ToList();
            var g2ConnExcess = g1.Connections.Except(g1.Connections).ToList();
            var excess = g1ConnExcess.Count + g2ConnExcess.Count;

            var matchingConnIds = g1.Connections.Intersect(g2.Connections);
            var g1Matching = g1.Connections.Where(x => matchingConnIds.Contains(x)).ToList();
            var g2Matching = g2.Connections.Where(x => matchingConnIds.Contains(x)).ToList();

            var weightDifferences = 0d;
            for (var i = 0; i < g1Matching.Count; i++)
            {
                var g1M = g1Matching[i];
                var g2M = g2Matching[i];

                weightDifferences += Math.Abs(g1M.Weight - g2M.Weight);
            }

            return excess + weightDifferences;
        }
    }
}
