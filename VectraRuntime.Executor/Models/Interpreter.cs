using VectraRuntime.Executor.Native;
using VectraRuntime.Executor.Opcodes;
using VectraRuntime.Loader.Models;

namespace VectraRuntime.Executor.Models;

public sealed class Interpreter
{
    private readonly VbcModule _module;
    private readonly NativeDispatch _native;
    
    private readonly Stack<(CallFrame Frame, ushort HandlerIP)> _attemptStack = new();

    public Interpreter(VbcModule module, NativeDispatch? native = null)
    {
        _module = module;
        _native = native ?? new();
    }

    public void Run()
    {
        var main = FindMain();
        var result = Execute(main, []);
        if (result is ExecuteResult.Unwinding u)
            throw new InvalidOperationException($"Unhandled abort: {u.AbortedValue}");
    }

    private MethodBody FindMain()
    {
        var mainEntry = _module.Constants.FirstOrDefault(c =>
                            c.Kind == ConstantKind.Method && c.Name.EndsWith("::Main()")) ??
                        throw new InvalidOperationException("No Program.Main found in module");
        return _module.MethodBodies.FirstOrDefault(b => b.CallablePoolIndex == mainEntry.Index)
               ?? throw new InvalidOperationException("No method found in module");
    }

    private ExecuteResult Execute(MethodBody body, StackValue[] args)
    {
        var frame = new CallFrame(body.LocalSlotCount, body.Bytecode.ToArray());
        for (var i = 0; i < args.Length && i < frame.Locals.Length; i++)
        {
            frame.Locals[i] = args[i];
        }

        return ExecuteFrame(frame);
    }

    private ExecuteResult ExecuteFrame(CallFrame frame)
    {
        while (!frame.EndOfCode)
        {
            var opcode = (Opcode)frame.ReadByte();
            switch (opcode)
            {
                case Opcode.NOP: break;

                case Opcode.POP: frame.TryPop(); break;

                case Opcode.DUP: frame.Push(frame.Peek()); break;

                case Opcode.LOAD_NULL: frame.Push(StackValue.Null); break;
                case Opcode.LOAD_TRUE: frame.Push(StackValue.True); break;
                case Opcode.LOAD_FALSE: frame.Push(StackValue.False); break;

                case Opcode.LOAD_CONST:
                {
                    var index = frame.ReadUShort();
                    var entry = _module.Constants[index];
                    frame.Push(entry.Kind == ConstantKind.Number
                        ? StackValue.FromNumber(entry.NumericValue!.Value)
                        : StackValue.FromString(entry.Name));
                    break;
                }

                case Opcode.LOAD_LOCAL:
                {
                    var index = frame.ReadUShort();
                    frame.Push(frame.Locals[index]);
                    break;
                }

                case Opcode.STORE_LOCAL:
                {
                    var index = frame.ReadUShort();
                    frame.Locals[index] = frame.Pop();
                    break;
                }

                case Opcode.ADD:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    if (a.Kind == StackValueKind.String || b.Kind == StackValueKind.String)
                    {
                        frame.Push(StackValue.FromString(a.ToString() + b));
                    }
                    else
                    {
                        frame.Push(StackValue.FromNumber(a.AsNumber() + b.AsNumber()));
                    }
                    break;
                }
                case Opcode.SUB:
                {
                    var b = frame.Pop().AsNumber();
                    var a = frame.Pop().AsNumber();
                    frame.Push(StackValue.FromNumber(a - b));
                    break;
                }
                case Opcode.MUL:
                {
                    var b = frame.Pop().AsNumber();
                    var a = frame.Pop().AsNumber();
                    frame.Push(StackValue.FromNumber(a * b));
                    break;
                }
                case Opcode.DIV:
                {
                    var b = frame.Pop().AsNumber();
                    var a = frame.Pop().AsNumber();
                    frame.Push(StackValue.FromNumber(a / b));
                    break;
                }
                case Opcode.MOD:
                {
                    var b = frame.Pop().AsNumber();
                    var a = frame.Pop().AsNumber();
                    frame.Push(StackValue.FromNumber(a % b));
                    break;
                }
                case Opcode.NEG:
                {
                    var a = frame.Pop().AsNumber();
                    frame.Push(StackValue.FromNumber(-a));
                    break;
                }
                case Opcode.NOT:
                {
                    var value = frame.Pop();
                    frame.Push(value.AsBoolean() ? StackValue.False : StackValue.True);
                    break;
                }

                case Opcode.CEQ:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    frame.Push(AreEqual(a, b) ? StackValue.True : StackValue.False);
                    break;
                }
                case Opcode.CNE:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    frame.Push(AreEqual(a, b) ? StackValue.False : StackValue.True);
                    break;
                }
                case Opcode.CLT:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    var result = a.Kind == StackValueKind.String || b.Kind == StackValueKind.String
                        ? string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal) < 0
                        : a.AsNumber() < b.AsNumber();
                    frame.Push(result ? StackValue.True : StackValue.False);
                    break;
                }
                case Opcode.CLE:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    var result = a.Kind == StackValueKind.String || b.Kind == StackValueKind.String
                        ? string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal) <= 0
                        : a.AsNumber() <= b.AsNumber();
                    frame.Push(result ? StackValue.True : StackValue.False);
                    break;
                }
                case Opcode.CGT:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    var result = a.Kind == StackValueKind.String || b.Kind == StackValueKind.String
                        ? string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal) > 0
                        : a.AsNumber() > b.AsNumber();
                    frame.Push(result ? StackValue.True : StackValue.False);
                    break;
                }
                case Opcode.CGE:
                {
                    var b = frame.Pop();
                    var a = frame.Pop();
                    var result = a.Kind == StackValueKind.String || b.Kind == StackValueKind.String
                        ? string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal) >= 0
                        : a.AsNumber() >= b.AsNumber();
                    frame.Push(result ? StackValue.True : StackValue.False);
                    break;
                }

                case Opcode.JMP:
                {
                    var target = frame.ReadUShort();
                    frame.IP = target;
                    break;
                }
                case Opcode.JMP_TRUE:
                {
                    var target = frame.ReadUShort();
                    if (frame.Pop().AsBoolean()) frame.IP = target;
                    break;
                }
                case Opcode.JMP_FALSE:
                {
                    var target = frame.ReadUShort();
                    if (!frame.Pop().AsBoolean()) frame.IP = target;
                    break;
                }

                case Opcode.CALL_NATIVE:
                {
                    var funcIndex = frame.ReadUShort();
                    var argCount = frame.ReadUShort();
                    var args = PopArgs(frame, argCount);
                    var result = _native.Invoke((NativeFunction)funcIndex, args);
                    frame.Push(result);
                    break;
                }

                case Opcode.CALL:
                {
                    var methodIndex = frame.ReadUShort();
                    var argCount = frame.ReadUShort();
                    var args = PopArgs(frame, argCount);
                    var methodBody = FindMethodBody(methodIndex);
                    var result = Execute(methodBody, args);
                    if (result is ExecuteResult.Unwinding u)
                        return HandleUnwind(frame, u.AbortedValue);
                    frame.Push(((ExecuteResult.Normal)result).Value);
                    break;
                }

                case Opcode.NEW_OBJ:
                {
                    var typeIndex = frame.ReadUShort();
                    var obj = new VectraObject(typeIndex);
                    frame.Push(StackValue.FromObject(obj));
                    break;
                }

                case Opcode.CALL_CTOR:
                {
                    var ctorIndex = frame.ReadUShort();
                    var argCount = frame.ReadUShort();
                    var args = PopArgs(frame, argCount);
                    var ctorBody = FindMethodBody(ctorIndex);
                    var res = Execute(ctorBody, args);
                    if (res is ExecuteResult.Unwinding u)
                        return HandleUnwind(frame, u.AbortedValue);
                    // object remains on stack from NEW_OBJ
                    break;
                }

                case Opcode.LOAD_MEMBER:
                {
                    var memberIndex = frame.ReadUShort();
                    var obj = frame.Pop().AsObject();
                    frame.Push(obj.GetField(memberIndex));
                    break;
                }

                case Opcode.STORE_MEMBER:
                {
                    var memberIndex = frame.ReadUShort();
                    var obj = frame.Pop().AsObject();
                    var value = frame.Pop();
                    obj.SetField(memberIndex, value);
                    frame.Push(value);
                    break;
                }

                case Opcode.RET:
                    return new ExecuteResult.Normal(frame.Stack.Count > 0 ? frame.Pop() : StackValue.Null);

                case Opcode.ABORT:
                {
                    var value = frame.Pop();
                    return HandleUnwind(frame, value);
                }
                case Opcode.ENTER_ATTEMPT:
                {
                    var handlerIp = frame.ReadUShort();
                    _attemptStack.Push((frame, handlerIp));
                    break;
                }
                case Opcode.LEAVE_ATTEMPT:
                    _attemptStack.Pop();
                    break;
                case Opcode.ENTER_DEBRIEF:
                case Opcode.LEAVE_DEBRIEF:
                    break;

                default:
                    throw new InvalidOperationException($"Unknown opcode: 0x{(byte)opcode:X2}");
            }
        }

        return new ExecuteResult.Normal(StackValue.Null);
    }

    private ExecuteResult HandleUnwind(CallFrame frame, StackValue abortedValue)
    {
        if (_attemptStack.TryPeek(out var attempt) && attempt.Frame == frame)
        {
            _attemptStack.Pop();
            frame.Push(abortedValue);
            frame.IP = attempt.HandlerIP;
            return ExecuteFrame(frame);
        }
        return new ExecuteResult.Unwinding(abortedValue);
    }

    private MethodBody FindMethodBody(ushort poolIndex)
    {
        var body = _module.MethodBodies.FirstOrDefault(b => b.CallablePoolIndex == poolIndex);
        if (body is not null) return body;

        var entry = _module.Constants[poolIndex];
        if (entry.Kind == ConstantKind.Constructor)
            return new MethodBody(poolIndex, 1, [(byte)Opcode.RET]);
        throw new InvalidOperationException($"No method body for pool index {poolIndex}.");
    }

    private static bool AreEqual(StackValue a, StackValue b)
    {
        if (a.Kind != b.Kind) return false;
        return a.Kind switch
        {
            StackValueKind.Null => true,
            StackValueKind.Bool => a.AsBoolean() == b.AsBoolean(),
            StackValueKind.Number => a.AsNumber() == b.AsNumber(),
            StackValueKind.String => a.AsString() == b.AsString(),
            StackValueKind.Object => ReferenceEquals(a.Raw, b.Raw),
            _ => false
        };
    }

    private static StackValue[] PopArgs(CallFrame frame, ushort count)
    {
        var args = new StackValue[count];
        for (var i = count - 1; i >= 0; i--)
            args[i] = frame.Pop();
        return args;
    }
}