namespace VectraRuntime.Loader.Models;

public enum ConstantKind : byte
{
    Type        = 0x01,
    Constructor = 0x02,
    Method      = 0x03,
    Field       = 0x04,
    Property    = 0x05,
    String      = 0x06,
    Number      = 0x07
}