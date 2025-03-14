using Sprig.Codegen;
using Sprig.Codegen.Syntax;
using Sprig.IO;

internal class Program {
    private static int Main(string[] args) {
        // if (args.Length == 0) {
        //     Console.Error.WriteLine("usage: twig <source-path>");
        //     return 1;
        // }

        var path = "C:/Users/calla/Dev/Sprig/samples/test.sg";
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;

        if (!File.Exists(path)) {
            Console.Error.WriteLine($"error: file {path} does not exist");
            hasErrors = true;
        }

        if (hasErrors)
            return 1;

        var syntaxTree = SyntaxTree.Load(path);
        syntaxTrees.Add(syntaxTree);

        var compilation = new Compilation([..syntaxTrees]);
        var result = compilation.Evaluate([]);

        if (result.Diagnostics.Any())
            Console.Error.WriteDiagnostics(result.Diagnostics);
        else
            if (result.Result != null)
            Console.WriteLine(result.Result);

        return 0;
    }
}