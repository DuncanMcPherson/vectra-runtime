namespace VectraRuntime.Executor;

public sealed class VectraObject
{
    public ushort TypePoolIndex { get; }
    private readonly Dictionary<ushort, StackValue> _fields = [];

    public VectraObject(ushort typePoolIndex)
    {
        TypePoolIndex = typePoolIndex;
    }

    public StackValue GetField(ushort memberPoolIndex)
        => _fields.TryGetValue(memberPoolIndex, out var value) ? value : StackValue.Null;

    public void SetField(ushort memberPoolIndex, StackValue value)
        => _fields[memberPoolIndex] = value;
}