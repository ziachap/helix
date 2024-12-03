using Helix.Genetics;

namespace Helix.Speciation;

public class Species
{
    public Genome Representative { get; set; }
    public Population Population { get; set; }
}

public class GeneticDistanceSpeciation
{
    public delegate double DistanceDelegate(Genome genome1, Genome genome2);
    private DistanceDelegate DistanceFunction;
    private double DistanceThreshold;

    public List<Species> SpeciesList { get; private set; }
    
    public GeneticDistanceSpeciation(DistanceDelegate distanceFunction, double distanceThreshold)
    {
        DistanceFunction = distanceFunction;
        DistanceThreshold = distanceThreshold;
        SpeciesList = new List<Species>();
    }

    public void Speciate(List<Genome> population)
    {
        SpeciesList.Clear();

        foreach (var genome in population)
        {
            bool foundSpecies = false;

            foreach (var species in SpeciesList)
            {
                double distance = DistanceFunction(species.Representative, genome);

                if (distance < DistanceThreshold)
                {
                    species.Population.AddGenome(genome);
                    foundSpecies = true;
                    break;
                }
            }

            if (!foundSpecies)
            {
                var newSpecies = new Species()
                {
                    Representative = genome,
                    Population = new Population(100)
                };
                newSpecies.Population.AddGenome(genome);
                SpeciesList.Add(newSpecies);
            }
        }
    }
}