using System.Collections.Concurrent;
using Helix.Genetics;

namespace Helix.Speciation;

public class MultiEmitterMapElitesGPT : MapElitesGPT
{
    public ConcurrentBag<Emitter> Emitters { get; }

    public MultiEmitterMapElitesGPT(int gridSize,
        string xDescription,
        Func<Genome, double?> xSelector,
        string yDescription,
        Func<Genome, double?> ySelector,
        Genome seedGenome)
        : base(gridSize, xDescription, xSelector, yDescription, ySelector)
    {
        Emitters = new ConcurrentBag<Emitter>();
        AddEmitter(0, 0, seedGenome);
    }

    public void AddEmitter(int x, int y, Genome elite)
    {
        var emitter = new Emitter(x, y, elite);
        Emitters.Add(emitter);
    }

    public void EmitAndUpdate(Func<Genome, Genome> createGenomeFunc)
    {
        var newGenomes = Emitters
            .Select(emitter => emitter.GenerateNewGenome(createGenomeFunc))
            .Where(genome => genome != null)
            .ToList();

        IntegratePopulation(newGenomes);
    }

    protected override void UpdateCell(Genome genome, int x, int y)
    {
        base.UpdateCell(genome, x, y);

        var elite = GetElitesMatrix()[x, y];
        var existingEmitter = Emitters.FirstOrDefault(emitter => emitter.X == x && emitter.Y == y);

        if (existingEmitter != null)
        {
            existingEmitter.UpdateElite(elite);
        }
        else
        {
            AddEmitter(x, y, elite);
        }
    }
}

public class Emitter
{
    public int X { get; }
    public int Y { get; }
    public Genome Elite { get; private set; }

    public Emitter(int x, int y, Genome elite)
    {
        X = x;
        Y = y;
        Elite = elite;
    }

    public Genome GenerateNewGenome(Func<Genome, Genome> createGenomeFunc)
    {
        if (Elite == null) return null;
        return createGenomeFunc(Elite);
    }

    public void UpdateElite(Genome newGenome)
    {
        Elite = newGenome;
    }
}