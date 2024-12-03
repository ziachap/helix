using Helix.Genetics;

namespace Helix.Speciation
{
    public class MapElitesGPT
    {
        private static int _idCounter;
        private static int NewId => Interlocked.Increment(ref _idCounter);

        private double[] _minBehavior;
        private double[] _maxBehavior;
        private Genome[,] _elitesMatrix;
        private Func<Genome, double?> _xSelector;
        private Func<Genome, double?> _ySelector;
        
        public int GridSize { get; }
        public string XDescription { get; }
        public string YDescription { get; }
        public List<Genome> Species { get; }

        public MapElitesGPT(int gridSize,
            string xDescription,
            Func<Genome, double?> xSelector,
            string yDescription,
            Func<Genome, double?> ySelector)
        {
            GridSize = gridSize;
            _xSelector = xSelector;
            _ySelector = ySelector;
            XDescription = xDescription;
            YDescription = yDescription;

            _minBehavior = new double[2] { double.MaxValue, double.MaxValue };
            _maxBehavior = new double[2] { double.MinValue, double.MinValue };

            _elitesMatrix = new Genome[gridSize, gridSize];
            Species = new List<Genome>(_elitesMatrix.Length);
        }

        public void IntegratePopulation(IList<Genome> population)
        {
            var fitPopulation = population.Where(x => x.Fitness.PrimaryFitness > 0).ToList();

            UpdateBehaviorBounds(fitPopulation);

            foreach (var genome in fitPopulation)
            {
                var xValue = _xSelector(genome);
                var yValue = _ySelector(genome);

                if (!xValue.HasValue || !yValue.HasValue) continue;

                int xIndex = GetXIndex(xValue.Value);
                int yIndex = GetYIndex(yValue.Value);
                UpdateCell(genome, xIndex, yIndex);
            }
            UpdateSpecies();
        }

        private void UpdateSpecies()
        {
            var existing = new List<Genome>();
            foreach (var genome in _elitesMatrix)
            {
                if (genome != null)
                {
                    existing.Add(genome);
                }
            }

            var speciesGenomes = Species.ToList();
            var genomesToAdd = existing.Where(x => speciesGenomes.All(y => y.Id != x.Id)).ToList();
            var genomesToRemove = speciesGenomes.Where(x => existing.All(y => y.Id != x.Id)).ToList();

            Species.AddRange(genomesToAdd);
            Species.RemoveAll(genomesToRemove.Contains);
        }

        private void UpdateBehaviorBounds(IList<Genome> population)
        {
            foreach (var genome in population)
            {
                var xValue = _xSelector(genome);
                var yValue = _ySelector(genome);

                if (!xValue.HasValue || !yValue.HasValue) continue;

                _minBehavior[0] = Math.Min(_minBehavior[0], xValue.Value);
                _maxBehavior[0] = Math.Max(_maxBehavior[0], xValue.Value);

                _minBehavior[1] = Math.Min(_minBehavior[1], yValue.Value);
                _maxBehavior[1] = Math.Max(_maxBehavior[1], yValue.Value);
            }
        }

        private int GetXIndex(double behavior)
        {
            if (behavior == _minBehavior[0]) return 0;
            if (behavior == _maxBehavior[0]) return GridSize - 1;

            int x = (int)Math.Floor((behavior - _minBehavior[0]) / (_maxBehavior[0] - _minBehavior[0]) * (GridSize - 1));
            return x;
        }

        private int GetYIndex(double behavior)
        {
            if (behavior == _minBehavior[1]) return 0;
            if (behavior == _maxBehavior[1]) return GridSize - 1;

            int y = (int)Math.Floor((behavior - _minBehavior[1]) / (_maxBehavior[1] - _minBehavior[1]) * (GridSize - 1));

            return y;
        }

        protected virtual void UpdateCell(Genome genome, int x, int y)
        {
            if (_elitesMatrix[x, y] == null || genome.Fitness.PrimaryFitness > _elitesMatrix[x, y].Fitness.PrimaryFitness)
            {
                _elitesMatrix[x, y] = genome;
            }
        }

        public Genome[,] GetElitesMatrix()
        {
            return _elitesMatrix;
        }
    }
}
