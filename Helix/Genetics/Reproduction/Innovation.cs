using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helix.Genetics.Reproduction
{
    public static class Innovation
    {
        private static readonly Dictionary<string, int> InnovationIdCache = new();

        private static int _innovationId = 0;
        public static int NextInnovationId() => Interlocked.Increment(ref _innovationId);

        private static int _innovationConnectionId = 0;
        public static int NextInnovationConnectionId() => Interlocked.Increment(ref _innovationConnectionId);

        public static int ConnectionInnovationId(int srcId, int dstId)
        {
            var key = $"CONNECTION_{srcId}_{dstId}";

            return InnovationIdCache.ContainsKey(key)
                ? InnovationIdCache[key]
                : InnovationIdCache[key] = NextInnovationConnectionId();
        }

        public static int HiddenNodeInnovationId(int srcId, int dstId)
        {
            var key = $"HIDDEN_{srcId}_{dstId}";

            return InnovationIdCache.ContainsKey(key)
                ? InnovationIdCache[key]
                : InnovationIdCache[key] = NextInnovationId();
        }
        
        public static void BoostNodeInnovationId(int lastId)
        {
            if (_innovationId <= lastId) _innovationId = lastId + 1;
        }

        public static void ClearCache()
        {
            //InnovationIdCache.Clear();
        }

        public static bool InvalidInnovations(this Genome g)
        {
            if (g.HiddenNeurons.GroupBy(x => x.Id).Any(x => x.Count() > 1)) return true;
            if (g.Connections.GroupBy(x => x.Id).Any(x => x.Count() > 1)) return true;
            return false;
        }
    }
}
