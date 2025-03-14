using Sprig.Codegen;
using Sprig.Codegen.Syntax;
using Sprig.IO;

internal class Program {
    private static int Main(string[] args) {
        if (args.Length == 0) {
            Console.Error.WriteLine("usage: twig <source-path>");
            return 1;
        }

        var paths = GetFilePaths(args);
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;

        foreach (var path in paths) {
            if (!File.Exists(path)) {
                Console.Error.WriteLine($"error: file {paths} does not exist");
                hasErrors = true;
                continue;
            }

            var syntaxTree = SyntaxTree.Load(path);
            syntaxTrees.Add(syntaxTree);
        }

        if (hasErrors)
            return 1;

        var compilation = new Compilation([..syntaxTrees]);
        var result = compilation.Evaluate([]);

        if (result.Diagnostics.Any())
            Console.Error.WriteDiagnostics(result.Diagnostics);
        else
            if (result.Result != null)
            Console.WriteLine(result.Result);

        return 0;
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