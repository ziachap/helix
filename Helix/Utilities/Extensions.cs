using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helix.Genetics;

namespace Helix.Utilities
{
    public static class Extensions
    {
        public static string ToShortString(this AggregationFunction aggrFn) => aggrFn switch
        {
            AggregationFunction.Sum => "SUM",
            AggregationFunction.Average => "AVG",
            AggregationFunction.Min => "MIN",
            AggregationFunction.Max => "MAX",
            _ => new string(aggrFn.ToString().ToUpper().Take(3).ToArray())
        };

        public static string ToShortString(this ActivationFunction actFn) => actFn switch
        {
            ActivationFunction.ReLU => "ReLU",
            ActivationFunction.Sine => "SINE",
            ActivationFunction.LogisticApproximantSteep => "LOGI",
            _ => new string(actFn.ToString().ToUpper().Take(4).ToArray())
        };
    }
}
