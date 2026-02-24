namespace VectraRuntime.Executor.Opcodes;

public static class OpcodeOperands
{
    private static readonly Dictionary<Opcode, int> _counts = new()
    {
        { Opcode.NOP,          0 },
        { Opcode.POP,          0 },
        { Opcode.DUP,          0 },

        { Opcode.LOAD_LOCAL,   1 },
        { Opcode.STORE_LOCAL,  1 },

        { Opcode.LOAD_CONST,   1 },
        { Opcode.LOAD_NULL,    0 },
        { Opcode.LOAD_TRUE,    0 },
        { Opcode.LOAD_FALSE,   0 },

        { Opcode.NEW_OBJ,      1 },
        { Opcode.LOAD_MEMBER,  1 },
        { Opcode.STORE_MEMBER, 1 },

        { Opcode.CALL,         2 },
        { Opcode.CALL_CTOR,    2 },
        { Opcode.RET,          0 },
        { Opcode.CALL_NATIVE,  2 },

        { Opcode.ADD, 0 },
        { Opcode.SUB, 0 },
        { Opcode.MUL, 0 },
        { Opcode.DIV, 0 },
        { Opcode.MOD, 0 },
        { Opcode.NEG, 0 },
        { Opcode.NOT, 0 },

        { Opcode.CEQ, 0 },
        { Opcode.CNE, 0 },
        { Opcode.CLT, 0 },
        { Opcode.CLE, 0 },
        { Opcode.CGT, 0 },
        { Opcode.CGE, 0 },

        { Opcode.JMP,       1 },
        { Opcode.JMP_TRUE,  1 },
        { Opcode.JMP_FALSE, 1 },
        { Opcode.ABORT,      0 },
        { Opcode.ENTER_ATTEMPT, 1 },
        { Opcode.LEAVE_ATTEMPT, 0 },
        { Opcode.ENTER_DEBRIEF, 0 },
        { Opcode.LEAVE_DEBRIEF, 0 },
    };

    public static int For(Opcode opcode)
        => _counts.TryGetValue(opcode, out var count)
            ? count
            : throw new InvalidOperationException($"Unknown opcode: 0x{(byte)opcode:X2}");
}