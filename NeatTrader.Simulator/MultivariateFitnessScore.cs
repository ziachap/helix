namespace NeatTrader.Simulator
{
    internal class MultivariateFitnessScore
    {
        private readonly List<FitnessMetric> _metrics = new List<FitnessMetric>();

        public void AddMetric(double value, double min, double max, double weight, GoalFunction goalFunction)
        {
            if (weight <= 0) throw new ArgumentException("Weight must be more than zero");
            _metrics.Add(new FitnessMetric(value, min, max, weight, goalFunction));
        }

        public void AddMetric(double value, double weight, GoalFunction goalFunction)
        {
            if (weight <= 0) throw new ArgumentException("Weight must be more than zero");
            _metrics.Add(new FitnessMetric(Math.Abs(value), weight, goalFunction));
        }

        public double FitnessScoreNormalized()
        {
            if (_metrics.Count == 0) return 0;

            var totalWeight = _metrics.Sum(x => x.Weight);

            const double outMin = -5;
            const double outMax = 5;

            var t1 = _metrics
                .Select(x =>
                {
                    var s = ((x.Value - x.Min) / (x.Max - x.Min)) * (outMax - outMin) + outMin;
                    var sig = Sigmoid(s);
                    return Math.Pow(sig, x.GoalFunction == GoalFunction.Maximize ? x.Weight : -x.Weight);
                })
                .Aggregate(1d, (curr, next) => curr * next);

            var score = Math.Pow(t1, 1 / totalWeight);

            return score;
        }

        public double FitnessScore()
        {
            if (_metrics.Count == 0) return 0;

            var totalWeight = _metrics.Sum(x => x.Weight);

            var t1 = _metrics
                .Select(x => Math.Pow(x.Value, x.GoalFunction == GoalFunction.Maximize ? x.Weight : -x.Weight))
                .Aggregate(1d, (curr, next) => curr * next);

            var score = Math.Pow(t1, 1 / totalWeight);

            return score;
        }

        private static double Sigmoid(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }
    }

    internal class FitnessMetric
    {
        public FitnessMetric(double value, double min, double max, double weight, GoalFunction goalFunction)
        {
            Value = value;
            Min = min;
            Max = max;
            Weight = weight;
            GoalFunction = goalFunction;
        }

        public FitnessMetric(double value, double weight, GoalFunction goalFunction)
        {
            Value = value;
            Weight = weight;
            GoalFunction = goalFunction;
        }

        public double Value { get; }
        public double Min { get; }
        public double Max { get; } = 1;
        public double Weight { get; }
        public GoalFunction GoalFunction { get; }
    }

    internal enum GoalFunction
    {
        /// <summary> Prefer values further away from zero. </summary>
        Maximize,
        /// <summary> Prefer values closer to zero. </summary>
        Minimize
    }
}
