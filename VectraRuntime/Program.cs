using System.Globalization;
using VectraRuntime.Loader.Models;

namespace VectraRuntime;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("No file specified");
            return 1;
        }

        var path = args[0];
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"File not found: {path}");
            return 1;
        }
        
        using var stream = File.OpenRead(path);
        var module = new VbcLoader().Load(stream);

        Console.WriteLine($"Version: {module.VersionMajor}.{module.VersionMinor}");

        Console.WriteLine($"\nImports ({module.Imports.Count}):");
        foreach (var import in module.Imports)
            Console.WriteLine($"  - {import}");

        Console.WriteLine($"\nConstants ({module.Constants.Count}):");
        foreach (var c in module.Constants)
        {
            var value = c.NumericValue.HasValue ? c.NumericValue.Value.ToString(CultureInfo.InvariantCulture) : c.Name;
            Console.WriteLine($"  [{c.Index}] {c.Kind,-12} {value}");
        }

        Console.WriteLine($"\nTypes ({module.Types.Count}):");
        foreach (var t in module.Types)
        {
            Console.WriteLine($"  PoolIndex={t.PoolIndex} Methods={t.Methods.Count}");
            foreach (var m in t.Methods)
                Console.WriteLine($"    Method PoolIndex={m.PoolIndex} Params={m.ParameterCount}");
        }

        Console.WriteLine($"\nMethod Bodies ({module.MethodBodies.Count}):");
        foreach (var b in module.MethodBodies)
            Console.WriteLine($"  CallablePoolIndex={b.CallablePoolIndex} Locals={b.LocalSlotCount} BytecodeLength={b.Bytecode.Count}");

        return 0;
    }
}