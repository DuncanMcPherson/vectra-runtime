namespace VectraRuntime.Executor.Models;

public sealed class VectraArray
{
    public ushort ElementTypeIndex { get; }
    private readonly StackValue[] _elements;

    public VectraArray(ushort elementTypeIndex, int size)
    {
        ElementTypeIndex = elementTypeIndex;
        _elements = new StackValue[size];
        Array.Fill(_elements, StackValue.Null);
    }

    public StackValue Get(int index)
    {
        if (index < 0 || index >= _elements.Length)
            throw new IndexOutOfRangeException($"Array index {index} is out of range.");
        return _elements[index];
    }

    public void Set(int index, StackValue value)
    {
        if (index < 0 || index >= _elements.Length)
            throw new IndexOutOfRangeException($"Array index {index} is out of range.");
        _elements[index] = value;
    }
}