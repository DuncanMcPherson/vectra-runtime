namespace VectraRuntime.Loader.Models;

public sealed class ConstantEntry
{
    public ushort Index { get; }
    public ConstantKind Kind { get; }
    public string Name { get; }
    public double? NumericValue { get; }
    
    public ConstantEntry(ushort index, ConstantKind kind, string name, double? numericValue = null)
    {
        Index = index;
        Kind = kind;
        Name = name;
        NumericValue = numericValue;
    }
}