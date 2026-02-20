namespace VectraRuntime.Executor.Native;

public sealed class NativeDispatch
{
    private readonly TextReader  _in;
    private readonly TextWriter  _out;

    public NativeDispatch(TextReader? input = null, TextWriter? output = null)
    {
        _in  = input  ?? Console.In;
        _out = output ?? Console.Out;
    }

    public StackValue Invoke(NativeFunction function, StackValue[] args) => function switch
    {
        NativeFunction.Print     => Print(args),
        NativeFunction.PrintLine => PrintLine(args),
        NativeFunction.Read      => Read(),
        NativeFunction.ReadLine  => ReadLine(),
        NativeFunction.ReadInt   => ReadInt(),
        _                        => throw new InvalidOperationException($"Unknown native function: {function}")
    };

    private StackValue Print(StackValue[] args)
    {
        _out.Write(args[0].ToString());
        return StackValue.Null;
    }

    private StackValue PrintLine(StackValue[] args)
    {
        _out.WriteLine(args[0].ToString());
        return StackValue.Null;
    }

    private StackValue Read()      => StackValue.FromString(_in.Read().ToString());
    private StackValue ReadLine()  => StackValue.FromString(_in.ReadLine() ?? string.Empty);
    private StackValue ReadInt()   => StackValue.FromNumber(int.Parse(_in.ReadLine() ?? "0"));
}