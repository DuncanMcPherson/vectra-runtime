namespace VectraRuntime.Loader.Models;

public sealed class MethodBody
{
    public ushort CallablePoolIndex { get; }
    public ushort LocalSlotCount { get; }
    public IReadOnlyList<byte> Bytecode { get; }

    public MethodBody(ushort callablePoolIndex, ushort localSlotCount, IReadOnlyList<byte> bytecode)
    {
        CallablePoolIndex = callablePoolIndex;
        LocalSlotCount = localSlotCount;
        Bytecode = bytecode;
    }
}