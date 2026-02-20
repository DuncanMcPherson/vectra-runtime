namespace VectraRuntime.Loader.Models;

public sealed class MethodDefinition
{
    public ushort PoolIndex { get; }
    public ushort ParameterCount { get; }
    
    public MethodDefinition(ushort poolIndex, ushort parameterCount)
    {
        PoolIndex = poolIndex;
        ParameterCount = parameterCount;
    }
}