using Sprig.Codegen;
using Sprig.Codegen.Syntax;
using Sprig.IO;

internal class Program {
    private static void Main(string[] args) {
        if (args.Length == 0) {
            Console.Error.WriteLine("usage: sprig <source-path>");
            return;
        }

        var paths = GetFilePaths(args);
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;

        foreach (var path in paths) {
            if (!File.Exists(path)) {
                Console.WriteLine($"error: file {paths} does not exist");
                hasErrors = true;
                continue;
            }

            var syntaxTree = SyntaxTree.Load(path);
            syntaxTrees.Add(syntaxTree);
        }

        if (hasErrors)
            return;

        var compilation = new Compilation([.. syntaxTrees]);
        var result = compilation.Evaluate([]);

        if (result.Diagnostics.Any())
            Console.Error.WriteDiagnostics(result.Diagnostics);
        else
            if (result.Result != null)
            Console.WriteLine(result.Result);
    }

    private static IEnumerable<string> GetFilePaths(IEnumerable<string> args) {
        var result = new SortedSet<string>();
        
        foreach (var path in args) {
            if (Directory.Exists(path)) {
                result.UnionWith(Directory.EnumerateFiles(path, "*.sg",  SearchOption.AllDirectories));
            }
            else {
                result.Add(path);
            }
        }

        return result;
    }
}