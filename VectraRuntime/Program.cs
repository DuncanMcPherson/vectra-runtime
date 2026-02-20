using System.Globalization;
using VectraRuntime.Executor.Models;
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
        var executor = new Interpreter(module);
        executor.Run();
        return 0;
    }
}