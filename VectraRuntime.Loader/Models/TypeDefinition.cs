namespace VectraRuntime.Loader.Models;

public sealed class TypeDefinition
{
    public ushort PoolIndex { get; }
    public IReadOnlyList<MethodDefinition> Methods { get; }

    public TypeDefinition(ushort poolIndex, IReadOnlyList<MethodDefinition> methods)
    {
        PoolIndex = poolIndex;
        Methods = methods;
    }
}