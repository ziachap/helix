namespace Helix;

public interface IBlackBox
{
    public Span<double> Inputs { get; }
    public Span<double> Outputs { get; }
    void Activate();
}