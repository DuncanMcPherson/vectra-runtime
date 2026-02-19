using FluentAssertions;
using VectraRuntime.Loader.Models;

namespace VectraRuntime.Loader.Tests;

[TestFixture]
public class VbcLoaderTests
{
    private static byte[] BuildMinimalVbc(
        byte[]? magic = null,
        byte[]? version = null,
        byte[]? imports = null,
        byte[]? constants = null,
        byte[]? types = null,
        byte[]? bodies = null)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        
        w.Write(magic   ?? "VBC"u8.ToArray());
        w.Write(version ?? [0x01, 0x00]);
        w.Write(imports  ?? "\0\0"u8.ToArray()); // 0 imports
        w.Write(constants ?? "\0\0"u8.ToArray()); // 0 constants
        w.Write(types   ?? "\0\0"u8.ToArray()); // 0 types
        w.Write(bodies  ?? "\0\0"u8.ToArray()); // 0 bodies
        w.Flush();
        
        return ms.ToArray();
    }

    private static VbcModule LoadBytes(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return new VbcLoader().Load(ms);
    }

[Test]
    public void Load_ValidHeader_ReturnsModule()
    {
        var module = LoadBytes(BuildMinimalVbc());
        module.Should().NotBeNull();
    }

    [Test]
    public void Load_ValidHeader_ParsesVersion()
    {
        var module = LoadBytes(BuildMinimalVbc());
        module.VersionMajor.Should().Be(1);
        module.VersionMinor.Should().Be(0);
    }

    [Test]
    public void Load_InvalidMagic_Throws()
    {
        var bytes = BuildMinimalVbc(magic: "XYZ"u8.ToArray());
        var act = () => LoadBytes(bytes);
        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Load_EmptyStream_Throws()
    {
        var act = () => LoadBytes([]);
        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Load_NoImports_ReturnsEmptyImports()
    {
        var module = LoadBytes(BuildMinimalVbc());
        module.Imports.Should().BeEmpty();
    }

    [Test]
    public void Load_WithImports_ParsesImportNames()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write("VBC"u8);
        w.Write([0x01, 0x00]);

        // 2 imports
        w.Write((ushort)2);
        WriteString(w, "Core.Math");
        WriteString(w, "Core.IO");

        w.Write((ushort)0); // constants
        w.Write((ushort)0); // types
        w.Write((ushort)0); // bodies

        w.Flush();
        var module = LoadBytes(ms.ToArray());

        module.Imports.Should().HaveCount(2);
        module.Imports[0].Should().Be("Core.Math");
        module.Imports[1].Should().Be("Core.IO");
    }

    [Test]
    public void Load_WithStringConstant_ParsesCorrectly()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write("VBC"u8);
        w.Write([0x01, 0x00]);
        w.Write((ushort)0); // imports

        // 1 string constant
        w.Write((ushort)1);
        w.Write((byte)ConstantKind.String);
        WriteString(w, "hello");

        w.Write((ushort)0); // types
        w.Write((ushort)0); // bodies

        w.Flush();
        var module = LoadBytes(ms.ToArray());

        module.Constants.Should().HaveCount(1);
        module.Constants[0].Kind.Should().Be(ConstantKind.String);
        module.Constants[0].Name.Should().Be("hello");
    }

    [Test]
    public void Load_WithNumberConstant_ParsesCorrectly()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write("VBC"u8);
        w.Write([0x01, 0x00]);
        w.Write((ushort)0); // imports

        // 1 number constant
        w.Write((ushort)1);
        w.Write((byte)ConstantKind.Number);
        w.Write(3.14);

        w.Write((ushort)0); // types
        w.Write((ushort)0); // bodies

        w.Flush();
        var module = LoadBytes(ms.ToArray());

        module.Constants[0].Kind.Should().Be(ConstantKind.Number);
        module.Constants[0].NumericValue.Should().Be(3.14);
    }

    private static void WriteString(BinaryWriter w, string s)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(s);
        w.Write((ushort)bytes.Length);
        w.Write(bytes);
    }}