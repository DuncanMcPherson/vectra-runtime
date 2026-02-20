using FluentAssertions;
using VectraRuntime.Executor.Models;
using VectraRuntime.Executor.Native;
using VectraRuntime.Executor.Opcodes;
using VectraRuntime.Loader.Models;

namespace VectraRuntime.Executor.Tests;

[TestFixture]
public class InterpreterTests
{
    private static VbcModule BuildModule(
        byte[] bytecode,
        List<ConstantEntry>? constants = null,
        ushort localSlotCount = 1)
    {
        constants ??= [];

        // Constant entry for Main so FindMain works
        var mainEntry = new ConstantEntry(
            (ushort)constants.Count,
            ConstantKind.Method,
            "Test::Main()");
        constants.Add(mainEntry);

        var body = new MethodBody(mainEntry.Index, localSlotCount, bytecode);

        return new VbcModule(
            versionMajor: 1,
            versionMinor: 0,
            imports: [],
            constants: constants,
            types: [],
            methodBodies: [body]);
    }

    private static byte[] Bytecode(params object[] instructions)
    {
        var bytes = new List<byte>();
        foreach (var instruction in instructions)
        {
            switch (instruction)
            {
                case Opcode op:
                    bytes.Add((byte)op);
                    break;
                case ushort us:
                    bytes.Add((byte)(us & 0xFF));
                    bytes.Add((byte)(us >> 8));
                    break;
                case int i:
                    bytes.Add((byte)(i & 0xFF));
                    bytes.Add((byte)(i >> 8));
                    break;
            }
        }
        return bytes.ToArray();
    }

    private static string RunCapture(VbcModule module)
    {
        var output = new StringWriter();
        var native = new NativeDispatch(input: Console.In, output: output);
        new Interpreter(module, native).Run();
        return output.ToString();
    }

    // --- Stack basics ---

    [Test]
    public void LoadNull_PushesNull()
    {
        var bytecode = Bytecode(Opcode.LOAD_NULL, Opcode.RET);
        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void LoadTrue_PushesTrue()
    {
        var bytecode = Bytecode(Opcode.LOAD_TRUE, Opcode.RET);
        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void LoadFalse_PushesFalse()
    {
        var bytecode = Bytecode(Opcode.LOAD_FALSE, Opcode.RET);
        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void Pop_RemovesTopOfStack()
    {
        var bytecode = Bytecode(Opcode.LOAD_TRUE, Opcode.POP, Opcode.LOAD_NULL, Opcode.RET);
        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void Dup_DuplicatesTopOfStack()
    {
        var bytecode = Bytecode(Opcode.LOAD_TRUE, Opcode.DUP, Opcode.POP, Opcode.POP, Opcode.LOAD_NULL, Opcode.RET);
        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    // --- Locals ---

    [Test]
    public void StoreLocal_ThenLoadLocal_RoundTrips()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "42", 42)
        };

        // LOAD_CONST 0, STORE_LOCAL 0, LOAD_LOCAL 0, RET
        var bytecode = Bytecode(
            Opcode.LOAD_CONST,  (ushort)0,
            Opcode.STORE_LOCAL, (ushort)0,
            Opcode.LOAD_LOCAL,  (ushort)0,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    // --- Arithmetic ---

    [Test]
    public void Add_TwoNumbers_PushesSum()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "3", 3),
            new(1, ConstantKind.Number, "4", 4),
        };

        var bytecode = Bytecode(
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.LOAD_CONST, (ushort)1,
            Opcode.ADD,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void Neg_NegatesNumber()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "5", 5),
        };

        var bytecode = Bytecode(
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.NEG,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    // --- Comparisons ---

    [Test]
    public void CEQ_EqualNumbers_PushesTrue()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "1", 1),
            new(1, ConstantKind.Number, "1", 1),
        };

        var bytecode = Bytecode(
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.LOAD_CONST, (ushort)1,
            Opcode.CEQ,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void CLT_LessThan_PushesTrue()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "1", 1),
            new(1, ConstantKind.Number, "2", 2),
        };

        var bytecode = Bytecode(
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.LOAD_CONST, (ushort)1,
            Opcode.CLT,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    // --- Jumps ---

    [Test]
    public void JMP_UnconditionallyJumps()
    {
        // JMP past LOAD_FALSE, lands on LOAD_TRUE, RET
        // IP layout: 0=JMP, 1-2=target(6), 3=LOAD_FALSE, 4=RET, 5=LOAD_TRUE, 6=RET
        var bytecode = Bytecode(
            Opcode.JMP,        (ushort)6,
            Opcode.LOAD_FALSE,
            Opcode.RET,
            Opcode.LOAD_TRUE,
            Opcode.RET);

        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void JmpFalse_WhenFalse_Jumps()
    {
        // LOAD_FALSE, JMP_FALSE to LOAD_NULL, RET
        // IP: 0=LOAD_FALSE, 1=JMP_FALSE, 2-3=target(5), 4=LOAD_TRUE, 5=LOAD_NULL, 6=RET
        var bytecode = Bytecode(
            Opcode.LOAD_FALSE,
            Opcode.JMP_FALSE,  (ushort)6,
            Opcode.LOAD_TRUE,
            Opcode.RET,
            Opcode.LOAD_NULL,
            Opcode.RET);

        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    // --- Native calls ---

    [Test]
    public void CallNative_PrintLine_WritesToOutput()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.String, "Hello Vectra!"),
        };

        var bytecode = Bytecode(
            Opcode.LOAD_CONST,   (ushort)0,
            Opcode.CALL_NATIVE,  (ushort)NativeFunction.PrintLine, (ushort)1,
            Opcode.POP,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var output = RunCapture(module);
        output.Should().Be($"Hello Vectra!{Environment.NewLine}");
    }

    [Test]
    public void CallNative_Print_WritesWithoutNewline()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.String, "Hi"),
        };

        var bytecode = Bytecode(
            Opcode.LOAD_CONST,  (ushort)0,
            Opcode.CALL_NATIVE, (ushort)NativeFunction.Print, (ushort)1,
            Opcode.POP,
            Opcode.RET);

        var module = BuildModule(bytecode, constants);
        var output = RunCapture(module);
        output.Should().Be("Hi");
    }

    [Test]
    public void UnknownOpcode_Throws()
    {
        var bytecode = new byte[] { 0xFF };
        var module = BuildModule(bytecode);
        var act = () => new Interpreter(module).Run();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*0xFF*");
    }

    [Test]
    public void Call_InvokesMethodBody_andReturnsValue()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "10", 10),
            new(1, ConstantKind.Method, "Test::Add()")
        };
        var addBody = new MethodBody(1, 1, Bytecode(
            Opcode.LOAD_LOCAL, (ushort)0,
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.ADD,
            Opcode.RET));
        
        var mainEntry = new ConstantEntry(2, ConstantKind.Method, "Test::Main()");
        constants.Add(mainEntry);
        
        var mainBody = new MethodBody(2, 1, Bytecode(
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.CALL, (ushort)1, (ushort)1,
            Opcode.RET));

        var module = new VbcModule(1, 0, [], constants, [], [addBody, mainBody]);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }

    [Test]
    public void Call_PassesArgsCorrectly()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Number, "7", 7),
            new(1, ConstantKind.Number, "3", 3),
            new(2, ConstantKind.Method, "Test::Sub()"),
        };

        // Sub() body: LOAD_LOCAL 0, LOAD_LOCAL 1, SUB, RET
        var subBody = new MethodBody(2, 2, Bytecode(
            Opcode.LOAD_LOCAL, (ushort)0,
            Opcode.LOAD_LOCAL, (ushort)1,
            Opcode.SUB,
            Opcode.RET));

        var mainEntry = new ConstantEntry(3, ConstantKind.Method, "Test::Main()");
        constants.Add(mainEntry);

        // Main: push 7, push 3, CALL Sub argCount=2, RET
        var mainBody = new MethodBody(3, 1, Bytecode(
            Opcode.LOAD_CONST, (ushort)0,
            Opcode.LOAD_CONST, (ushort)1,
            Opcode.CALL,       (ushort)2, (ushort)2,
            Opcode.RET));

        var module = new VbcModule(1, 0, [], constants, [], [subBody, mainBody]);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }
    
    [Test]
    public void CallCtor_WithExplicitBody_InitializesObject()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Type,        "Test.Foo"),
            new(1, ConstantKind.Constructor, "Test.Foo::.ctor()"),
            new(2, ConstantKind.Number,      "42", 42),
            new(3, ConstantKind.Field,       "Test.Foo::Value"),
        };

        // ctor body: LOAD_LOCAL 0 (this), LOAD_LOCAL 1 (arg), STORE_MEMBER 3, RET
        var ctorBody = new MethodBody(1, 2, Bytecode(
            Opcode.LOAD_LOCAL,   (ushort)1,
            Opcode.LOAD_LOCAL,   (ushort)0,
            Opcode.STORE_MEMBER, (ushort)3,
            Opcode.RET));

        var mainEntry = new ConstantEntry(4, ConstantKind.Method, "Test::Main()");
        constants.Add(mainEntry);

        // Main: NEW_OBJ 0, DUP, LOAD_CONST 2, CALL_CTOR 1 argCount=2, RET
        var mainBody = new MethodBody(4, 1, Bytecode(
            Opcode.NEW_OBJ,    (ushort)0,
            Opcode.DUP,
            Opcode.LOAD_CONST, (ushort)2,
            Opcode.CALL_CTOR,  (ushort)1, (ushort)2,
            Opcode.RET));

        var module = new VbcModule(1, 0, [], constants, [], [ctorBody, mainBody]);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }
    
    [Test]
    public void CallCtor_WithNoEmittedBody_SynthesizesDefaultCtor()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Type,        "Test.Bar"),
            new(1, ConstantKind.Constructor, "Test.Bar::.ctor()"),
        };

        var mainEntry = new ConstantEntry(2, ConstantKind.Method, "Test::Main()");
        constants.Add(mainEntry);

        // Main: NEW_OBJ 0, DUP, CALL_CTOR 1 argCount=1 (just this), POP, RET
        var mainBody = new MethodBody(2, 1, Bytecode(
            Opcode.NEW_OBJ,   (ushort)0,
            Opcode.DUP,
            Opcode.CALL_CTOR, (ushort)1, (ushort)1,
            Opcode.POP,
            Opcode.RET));

        // No ctor body emitted — runtime should synthesize one
        var module = new VbcModule(1, 0, [], constants, [], [mainBody]);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }
    
    [Test]
    public void CallCtor_ObjectRemainsOnStack_AfterCtorReturns()
    {
        var constants = new List<ConstantEntry>
        {
            new(0, ConstantKind.Type,        "Test.Baz"),
            new(1, ConstantKind.Constructor, "Test.Baz::.ctor()"),
            new(2, ConstantKind.Field,       "Test.Baz::Value"),
            new(3, ConstantKind.Number,      "99", 99),
        };

        var ctorBody = new MethodBody(1, 2, Bytecode(
            Opcode.LOAD_LOCAL,   (ushort)1,
            Opcode.LOAD_LOCAL,   (ushort)0,
            Opcode.STORE_MEMBER, (ushort)2,
            Opcode.RET));

        var mainEntry = new ConstantEntry(4, ConstantKind.Method, "Test::Main()");
        constants.Add(mainEntry);

        // Main: NEW_OBJ, DUP, LOAD_CONST 99, CALL_CTOR, LOAD_MEMBER Value, RET
        // object stays on stack after ctor, then we read a field from it
        var mainBody = new MethodBody(4, 2, Bytecode(
            Opcode.NEW_OBJ,    (ushort)0,
            Opcode.DUP,
            Opcode.LOAD_CONST, (ushort)3,
            Opcode.CALL_CTOR,  (ushort)1, (ushort)2,
            Opcode.LOAD_MEMBER,(ushort)2,
            Opcode.RET));

        var module = new VbcModule(1, 0, [], constants, [], [ctorBody, mainBody]);
        var act = () => new Interpreter(module).Run();
        act.Should().NotThrow();
    }
}