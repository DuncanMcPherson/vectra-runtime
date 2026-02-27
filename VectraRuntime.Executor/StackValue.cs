using VectraRuntime.Executor.Models;

namespace VectraRuntime.Executor;

public enum StackValueKind
{
    Null,
    Bool,
    Number,
    String,
    Object,
    Array
}

public sealed class StackValue
{
    public static readonly StackValue Null  = new(StackValueKind.Null,   null);
    public static readonly StackValue True  = new(StackValueKind.Bool,   true);
    public static readonly StackValue False = new(StackValueKind.Bool,   false);

    public StackValueKind Kind  { get; }
    public object?        Raw   { get; }

    private StackValue(StackValueKind kind, object? raw)
    {
        Kind = kind;
        Raw  = raw;
    }

    public static StackValue FromNumber(double value) => new(StackValueKind.Number, value);
    public static StackValue FromString(string value) => new(StackValueKind.String, value);
    public static StackValue FromObject(VectraObject value) => new(StackValueKind.Object, value);
    public static StackValue FromArray(VectraArray value) => new(StackValueKind.Array, value);

    public bool         AsBoolean() => Kind == StackValueKind.Bool   && (bool)Raw!;
    public double       AsNumber()  => Kind == StackValueKind.Number ? (double)Raw! : throw new InvalidCastException($"Expected Number, got {Kind}");
    public string       AsString()  => Kind == StackValueKind.String ? (string)Raw! : throw new InvalidCastException($"Expected String, got {Kind}");
    public VectraObject AsObject()  => Kind == StackValueKind.Object ? (VectraObject)Raw! : throw new InvalidCastException($"Expected Object, got {Kind}");
    public VectraArray  AsArray()   => Kind == StackValueKind.Array  ? (VectraArray)Raw! : throw new InvalidCastException($"Expected Array, got {Kind}");

    public override string ToString() => Kind switch
    {
        StackValueKind.Null   => "null",
        StackValueKind.Bool   => AsBoolean().ToString().ToLower(),
        StackValueKind.Number => AsNumber().ToString(System.Globalization.CultureInfo.InvariantCulture),
        StackValueKind.String => AsString(),
        StackValueKind.Object => AsObject().ToString()!,
        StackValueKind.Array  => $"array[{AsArray().ElementTypeIndex}]",
        _                     => throw new InvalidOperationException($"Unknown kind: {Kind}")
    };
}