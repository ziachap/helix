namespace Helix.Genetics;

/// <summary>
/// Defines a modifiable part of a neural network topology.
/// </summary>
public interface ITopological
{
    int Id { get; }
}

public interface ICloneable<out T>
{
    T Clone();
}