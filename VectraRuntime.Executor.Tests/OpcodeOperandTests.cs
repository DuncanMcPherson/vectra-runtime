using FluentAssertions;
using VectraRuntime.Executor.Opcodes;

namespace VectraRuntime.Executor.Tests;

public class OpcodeOperandTests
{
    [TestCase(Opcode.NOP,          0)]
    [TestCase(Opcode.POP,          0)]
    [TestCase(Opcode.DUP,          0)]
    [TestCase(Opcode.LOAD_LOCAL,   1)]
    [TestCase(Opcode.STORE_LOCAL,  1)]
    [TestCase(Opcode.LOAD_CONST,   1)]
    [TestCase(Opcode.LOAD_NULL,    0)]
    [TestCase(Opcode.LOAD_TRUE,    0)]
    [TestCase(Opcode.LOAD_FALSE,   0)]
    [TestCase(Opcode.NEW_OBJ,      1)]
    [TestCase(Opcode.LOAD_MEMBER,  1)]
    [TestCase(Opcode.STORE_MEMBER, 1)]
    [TestCase(Opcode.CALL,         2)]
    [TestCase(Opcode.CALL_CTOR,    2)]
    [TestCase(Opcode.RET,          0)]
    [TestCase(Opcode.CALL_NATIVE,  2)]
    [TestCase(Opcode.ADD,          0)]
    [TestCase(Opcode.SUB,          0)]
    [TestCase(Opcode.MUL,          0)]
    [TestCase(Opcode.DIV,          0)]
    [TestCase(Opcode.MOD,          0)]
    [TestCase(Opcode.NEG,          0)]
    [TestCase(Opcode.CEQ,          0)]
    [TestCase(Opcode.CNE,          0)]
    [TestCase(Opcode.CLT,          0)]
    [TestCase(Opcode.CLE,          0)]
    [TestCase(Opcode.CGT,          0)]
    [TestCase(Opcode.CGE,          0)]
    [TestCase(Opcode.JMP,          1)]
    [TestCase(Opcode.JMP_TRUE,     1)]
    [TestCase(Opcode.JMP_FALSE,    1)]
    public void OperandCount_IsCorrect(Opcode opcode, int expected)
    {
        OpcodeOperands.For(opcode).Should().Be(expected);
    }

    [Test]
    public void OperandCount_UnknownOpcode_Throws()
    {
        var act = () => OpcodeOperands.For((Opcode)0xFF);
        act.Should().Throw<InvalidOperationException>();
    }
}