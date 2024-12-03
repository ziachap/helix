using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helix.Genetics;
using Helix.Genetics.Reproduction.Asexual;

namespace Helix.Speciation
{
    public class Population
    {
        private readonly int _size;

        public List<Genome> GenomeList { get; set; }

        public Population(int size)
        {
            _size = size;
            GenomeList = new List<Genome>();
        }

        public void Initialize()
        {
            GenomeList = Enumerable.Range(1, _size).Select(x =>
            {
                return GenomeFactory.Create(8, 2);
            }).ToList();
        }

        public void AddGenome(Genome genome)
        {
            GenomeList.Add(genome);
            TrimGenomes();
        }

        public void AddGenomes(IEnumerable<Genome> genomes)
        {
            GenomeList.AddRange(genomes);
            TrimGenomes();
        }

        public IEnumerable<Genome> ProduceOffspring()
        {
            var reproduction = new AsexualReproduction();

            var genomes = GenomeList
                .Take(_size / 2)
                .Select(reproduction.CreateChild)
                .ToList();

            return genomes;
        }

        private void TrimGenomes()
        {
            GenomeList = GenomeList
                .OrderByDescending(x => x.Fitness.PrimaryFitness)
                .Take(_size)
                .ToList();
        }
    }
}
