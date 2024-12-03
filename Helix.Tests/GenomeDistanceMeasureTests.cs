using FluentAssertions;
using Helix.Genetics;
using Helix.Speciation;

namespace Helix.Tests
{
    public class GenomeDistanceMeasureTests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static Genome CreateBlankGenome(int inputs, int outputs)
        {
            var genome = GenomeFactory.Create(inputs, outputs);
            genome.Connections.Clear();
            return genome;
        }

        [Test]
        public void Distance_Is_0_When_Genomes_Are_Same()
        {
            var g1 = CreateBlankGenome(2, 4);
            g1.Connections.Add(new Connection(0, 1, 3, 1, ConnectionIntegrator.Aggregate));
            g1.Connections.Add(new Connection(1, 2, 4, 0.3, ConnectionIntegrator.Aggregate));
            
            var distance = GenomeDistanceMeasure.Distance(g1, g1);

            distance.Should().Be(0);
        }

        [Test]
        public void Distance()
        {
            var g1 = CreateBlankGenome(2, 4);
            g1.Connections.Add(new Connection(0, 1, 3, 1, ConnectionIntegrator.Aggregate));
            g1.Connections.Add(new Connection(1, 2, 4, 0.3, ConnectionIntegrator.Aggregate));

            var g2 = CreateBlankGenome(1, 1);
            g2.Connections.Add(new Connection(2, 1, 3, -0.6, ConnectionIntegrator.Aggregate));
            g1.Connections.Add(new Connection(3, 4, 5, 0.8, ConnectionIntegrator.Aggregate));
            g1.Connections.Add(new Connection(4, 3, 4, 1, ConnectionIntegrator.Aggregate));

            var distance = GenomeDistanceMeasure.Distance(g1, g2);

            distance.Should().Be(4.6);
        }
    }
}