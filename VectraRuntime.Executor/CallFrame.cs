using VectraRuntime.Executor.Opcodes;

namespace VectraRuntime.Executor;

public sealed class CallFrame
{
    public StackValue[] Locals { get; }
    public byte[] Bytecode { get; }
    public int IP { get; set; }
    public Stack<StackValue> Stack { get; } = new();
    public StackValue? PendingReturn { get; set; }
    public int? DebriefIP { get; set; }

    public CallFrame(int paramCount, byte[] bytecode)
    {
        Bytecode = bytecode;
        IP = 0;

        var maxSlot = ScanMaxLocalIndex(bytecode);
        var size = Math.Max(paramCount, maxSlot + 1);
        
        Locals = new StackValue[size];
        Array.Fill(Locals, StackValue.Null);
    }
    
    
    
    public void Push(StackValue value) => Stack.Push(value);
    public StackValue Pop() => Stack.Pop();

    public StackValue TryPop()
    {
        return Stack.Count == 0 ? StackValue.Null : Stack.Pop();
    }
    public StackValue Peek() => Stack.Peek();

    public bool EndOfCode => IP >= Bytecode.Length;
    
    public byte ReadByte() => Bytecode[IP++];

    public ushort ReadUShort()
    {
        var lo = Bytecode[IP++];
        var hi = Bytecode[IP++];
        return (ushort)(lo | (hi << 8));
    }

    private static int ScanMaxLocalIndex(byte[] bytecode)
    {
        var max = 0;
        var i = 0;
        while (i < bytecode.Length)
        {
            var op = (Opcode)bytecode[i++];
            var operandCount = OpcodeOperands.For(op);

            if (op is Opcode.LOAD_LOCAL or Opcode.STORE_LOCAL && i + 1 < bytecode.Length)
            {
                var index = bytecode[i] | (bytecode[i + 1] << 8);
                if (index > max) max = index;
            }

            i += operandCount * 2;
        }
        return max;
    }
}