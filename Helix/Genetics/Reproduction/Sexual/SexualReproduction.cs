namespace Helix.Genetics.Reproduction.Sexual
{
    public class SexualReproduction
    {
        private readonly Random _random = new Random();

        public Genome CreateChild(Genome parent1, Genome parent2)
        {
            // TODO: Don't do what it says below, if they the same then include all disjoint/excess genes
            // If both parents have the same/similar fitness, randomly select one

            if (Math.Abs(parent1.Fitness.PrimaryFitness - parent2.Fitness.PrimaryFitness) < 0.00000001)
            {
                return CreateEqualChild(parent1, parent2);
            }

            var moreFitParent = parent1.Fitness.PrimaryFitness > parent2.Fitness.PrimaryFitness ? parent1 : parent2;
            var lessFitParent = parent1.Id == moreFitParent.Id ? parent2 : parent1;
            
            var child = GenomeFactory.CreateFrom(moreFitParent);
            child.Connections.Clear();

            // Crossover hidden nodes
            child.HiddenNeurons = CrossoverHiddenNeurons(moreFitParent, parent1, parent2).ToList();

            var connectionsParent1 = parent1.Connections.ToDictionary(x => x.Id);
            var connectionsParent2 = parent2.Connections.ToDictionary(x => x.Id);

            foreach (var gene1 in connectionsParent1)
            {
                if (connectionsParent2.TryGetValue(gene1.Key, out Connection gene2))
                {
                    // Matching genes - inherit from either parent randomly
                    Connection geneToInherit = (_random.NextDouble() < 0.5) ? gene1.Value : gene2;
                    child.Connections.Add(new Connection(
                        geneToInherit.Id,
                        geneToInherit.SourceId,
                        geneToInherit.DestinationId,
                        geneToInherit.Weight,
                        geneToInherit.Integrator)
                    {
                    });
                }
                else
                {
                    // Disjoint or excess gene from parent 1
                    if (moreFitParent == parent1)
                    {
                        child.Connections.Add(new Connection(
                            gene1.Value.Id,
                            gene1.Value.SourceId,
                            gene1.Value.DestinationId,
                            gene1.Value.Weight,
                            gene1.Value.Integrator));
                    }
                }
            }

            // Disjoint or excess genes from parent 2
            foreach (var gene2 in connectionsParent2)
            {
                if (!connectionsParent1.ContainsKey(gene2.Key) && moreFitParent == parent2)
                {
                    child.Connections.Add(new Connection(
                        gene2.Value.Id,
                        gene2.Value.SourceId,
                        gene2.Value.DestinationId,
                        gene2.Value.Weight,
                        gene2.Value.Integrator));
                }
            }

            // TODO: Crossover meta parameters? average them?

            if (child.InvalidInnovations())
            {

            }

            return child;
        }

        private Genome CreateEqualChild(Genome parent1, Genome parent2)
        {
            var child = GenomeFactory.CreateFrom(parent1);
            child.Connections.Clear();

            // Crossover hidden nodes
            child.HiddenNeurons = CrossoverHiddenNeurons(null, parent1, parent2).ToList();

            var connectionsParent1 = parent1.Connections.ToDictionary(x => x.Id);
            var connectionsParent2 = parent2.Connections.ToDictionary(x => x.Id);

            foreach (var gene1 in connectionsParent1)
            {
                if (connectionsParent2.TryGetValue(gene1.Key, out Connection gene2))
                {
                    // Matching genes - inherit from either parent randomly
                    Connection geneToInherit = (_random.NextDouble() < 0.5) ? gene1.Value : gene2;
                    child.Connections.Add(new Connection(
                        geneToInherit.Id,
                        geneToInherit.SourceId,
                        geneToInherit.DestinationId,
                        geneToInherit.Weight,
                        geneToInherit.Integrator));
                }
                else
                {
                    child.Connections.Add(new Connection(
                        gene1.Value.Id,
                        gene1.Value.SourceId,
                        gene1.Value.DestinationId,
                        gene1.Value.Weight,
                        gene1.Value.Integrator));
                }
            }

            // Disjoint or excess genes from parent 2
            foreach (var gene2 in connectionsParent2)
            {
                if (!connectionsParent1.ContainsKey(gene2.Key))
                {
                    child.Connections.Add(new Connection(
                        gene2.Value.Id,
                        gene2.Value.SourceId,
                        gene2.Value.DestinationId,
                        gene2.Value.Weight,
                        gene2.Value.Integrator));
                }
            }

            // TODO: Crossover meta parameters? average them?
            
            return child;
        }

        private IEnumerable<HiddenNeuronDescriptor> CrossoverHiddenNeurons(Genome moreFitParent, Genome p1, Genome p2)
        {
            Dictionary<int, HiddenNeuronDescriptor> hiddenNodesParent1;
            Dictionary<int, HiddenNeuronDescriptor> hiddenNodesParent2;
            try
            {
                hiddenNodesParent1 = p1.HiddenNeurons.ToDictionary(x => x.Id);
                hiddenNodesParent2 = p2.HiddenNeurons.ToDictionary(x => x.Id);
            }
            catch (Exception ex)
            {
                throw;
            }

            foreach (var gene1 in hiddenNodesParent1)
            {
                if (hiddenNodesParent2.TryGetValue(gene1.Key, out HiddenNeuronDescriptor gene2))
                {
                    // Matching genes - inherit from either parent randomly
                    var geneToInherit = (_random.NextDouble() < 0.5) ? gene1.Value : gene2;
                    yield return new HiddenNeuronDescriptor()
                    {
                        Id = geneToInherit.Id,
                        AggregationFunction = geneToInherit.AggregationFunction,
                        ActivationFunction = geneToInherit.ActivationFunction,
                    };
                }
                else
                {
                    // Disjoint or excess gene from parent 1
                    if (moreFitParent == p1 || moreFitParent == null)
                    {
                        yield return new HiddenNeuronDescriptor()
                        {
                            Id = gene1.Value.Id,
                            AggregationFunction = gene1.Value.AggregationFunction,
                            ActivationFunction = gene1.Value.ActivationFunction,
                        };
                    }
                }
            }

            // Disjoint or excess genes from parent 2
            foreach (var gene2 in hiddenNodesParent2)
            {
                if (!hiddenNodesParent1.ContainsKey(gene2.Key))
                {
                    yield return new HiddenNeuronDescriptor()
                    {
                        Id = gene2.Value.Id,
                        AggregationFunction = gene2.Value.AggregationFunction,
                        ActivationFunction = gene2.Value.ActivationFunction,
                    };
                }
            }
        }
    }
}
