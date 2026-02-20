using System.Globalization;
using System.Text;

namespace VectraRuntime.Loader.Models;

public sealed class VbcLoader
{
    private static readonly byte[] Magic = "VBC"u8.ToArray();
    public VbcModule Load(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadBytes(3);
        if (magic.Length < 3 || !magic.SequenceEqual(Magic))
            throw new InvalidDataException("Invalid VBC Magic number");
        
        var versionMajor = reader.ReadByte();
        var versionMinor = reader.ReadByte();

        var imports = ReadImports(reader);
        var constants = ReadConstants(reader);
        var types = ReadTypes(reader);
        var bodies = ReadBodies(reader);
        
        return new VbcModule(versionMajor, versionMinor, imports, constants, types, bodies);
    }

    private static List<string> ReadImports(BinaryReader reader)
    {
        var count = reader.ReadUInt16();
        var imports = new List<string>(count);
        for (var i = 0; i < count; i++)
            imports.Add(ReadString(reader));
        return imports;
    }

    private static List<ConstantEntry> ReadConstants(BinaryReader reader)
    {
        var count = reader.ReadUInt16();
        var entries = new List<ConstantEntry>(count);
        for (ushort i = 0; i < count; i++)
        {
            var kind = (ConstantKind)reader.ReadByte();
            if (kind == ConstantKind.Number)
            {
                var value = reader.ReadDouble();
                entries.Add(new ConstantEntry(i, kind, value.ToString(CultureInfo.InvariantCulture), value));
            }
            else
            {
                var name = ReadString(reader);
                entries.Add(new ConstantEntry(i, kind, name));
            }
        }
        return entries;
    }

    private static List<TypeDefinition> ReadTypes(BinaryReader reader)
    {
        var count = reader.ReadUInt16();
        var types = new List<TypeDefinition>(count);
        for (var i = 0; i < count; i++)
        {
            var typeIndex = reader.ReadUInt16();
            var methodCount = reader.ReadUInt16();
            var methods = new List<MethodDefinition>(methodCount);
            for (var j = 0; j < methodCount; j++)
            {
                var methodIndex = reader.ReadUInt16();
                var paramCount = reader.ReadUInt16();
                methods.Add(new MethodDefinition(methodIndex, paramCount));
            }
            types.Add(new TypeDefinition(typeIndex, methods));
        }
        return types;
    }

    private static List<MethodBody> ReadBodies(BinaryReader reader)
    {
        var count = reader.ReadUInt16();
        var bodies = new List<MethodBody>(count);
        for (var i = 0; i < count; i++)
        {
            var callableIndex = reader.ReadUInt16();
            var localSlotCount = reader.ReadUInt16();
            var byteCount = reader.ReadUInt16();
            var byteCode = reader.ReadBytes(byteCount);
            bodies.Add(new MethodBody(callableIndex, localSlotCount, byteCode));
        }
        return bodies;
    }

    private static string ReadString(BinaryReader reader)
    {
        var length = reader.ReadUInt16();
        var bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }
}