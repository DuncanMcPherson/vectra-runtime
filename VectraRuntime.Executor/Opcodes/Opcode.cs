namespace VectraRuntime.Executor.Opcodes;

public enum Opcode : byte
{
    NOP        = 0x00,
    POP        = 0x01,
    DUP        = 0x02,

    LOAD_LOCAL  = 0x10,
    STORE_LOCAL = 0x11,

    LOAD_CONST  = 0x20,
    LOAD_NULL   = 0x21,
    LOAD_TRUE   = 0x22,
    LOAD_FALSE  = 0x23,

    NEW_OBJ     = 0x30,
    LOAD_MEMBER = 0x31,
    STORE_MEMBER= 0x32,

    CALL        = 0x40,
    CALL_CTOR   = 0x41,
    RET         = 0x42,
    CALL_NATIVE = 0x43,

    ADD = 0x50,
    SUB = 0x51,
    MUL = 0x52,
    DIV = 0x53,
    MOD = 0x54,
    NEG = 0x55,
    NOT = 0x56,

    CEQ = 0x60,
    CNE = 0x61,
    CLT = 0x62,
    CLE = 0x63,
    CGT = 0x64,
    CGE = 0x65,

    JMP       = 0x70,
    JMP_TRUE  = 0x71,
    JMP_FALSE = 0x72,
    ABORT     = 0x73,
    ENTER_ATTEMPT = 0x74,
    LEAVE_ATTEMPT = 0x75,
    ENTER_DEBRIEF = 0x76,
    LEAVE_DEBRIEF = 0x77
}