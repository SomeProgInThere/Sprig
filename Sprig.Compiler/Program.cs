using Sprig.Codegen;
using Sprig.Codegen.Syntax;
using Sprig.IO;

if (args.Length == 0) {
    Console.Error.WriteLine("usage: sprig <source-path>");
    return;
}

if (args.Length > 1) {
    Console.WriteLine("error: only one path supported");
    return;
}

var path = args.Single();

if (!File.Exists(path)) {
    Console.WriteLine($"error: file {path} does not exist");
    return;
}

var syntaxTree = SyntaxTree.Load(path);
var compilation = new Compilation(syntaxTree);

var result = compilation.Evaluate([]);

if (result.Diagnostics.Any())
    Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
else
    if (result.Result != null)
        Console.WriteLine(result.Result);