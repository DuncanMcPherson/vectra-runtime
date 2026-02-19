namespace VectraRuntime.Loader.Models;

public sealed class VbcModule
{
    public byte VersionMajor { get; }
    public byte VersionMinor { get; }
    public IReadOnlyList<string> Imports { get; }
    public IReadOnlyList<ConstantEntry> Constants { get; }
    public IReadOnlyList<TypeDefinition> Types { get; }
    public IReadOnlyList<MethodBody> MethodBodies { get; }

    public VbcModule(
        byte versionMajor,
        byte versionMinor,
        IReadOnlyList<string> imports,
        IReadOnlyList<ConstantEntry> constants,
        IReadOnlyList<TypeDefinition> types,
        IReadOnlyList<MethodBody> methodBodies)
    {
        VersionMajor = versionMajor;
        VersionMinor = versionMinor;
        Imports = imports;
        Constants = constants;
        Types = types;
        MethodBodies = methodBodies;
    }
}