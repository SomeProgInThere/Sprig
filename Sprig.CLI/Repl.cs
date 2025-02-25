using System.Collections.ObjectModel;

using Sprig.Code;
using Sprig.Code.Syntax;

namespace Sprig.CLI;

internal sealed class Repl {

    public void Run() {
        ReplExtensions.ColorPrint($"\n{ReplExtensions.StartupMessage}\n", ConsoleColor.DarkCyan);
        showParsing = false;
        showBinding = false;

        while (true) {
            var source = EditSource();
            if (string.IsNullOrEmpty(source))
                continue;
            
            if (!source.Contains(Environment.NewLine) && source.StartsWith('!'))
                EvaluateMetaCommands(source);
            else
                EvaluateSource(source);

            sourceHistory.Add(source);
            sourceHistoryIndex = 0;
        }
    }

    private string EditSource() {
        done = false;
        var document = new ObservableCollection<string>() { string.Empty };
        var view = new SourceView(document);
        
        while (!done) {
            var key = Console.ReadKey(true);
            HandleKey(key, document, view);
        }

        view.CurrentLine = document.Count - 1;
        view.CurrentChar = document[view.CurrentLine].Length;

        Console.WriteLine();
        return string.Join(Environment.NewLine, document);
    }

    private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SourceView view) {
        if (key.Modifiers == default) {
            switch (key.Key) {
                case ConsoleKey.Enter:
                    ReplExtensions.HandleEnter(document, view, ref done);
                    break;

                case ConsoleKey.Tab:
                    ReplExtensions.HandleTab(document, view);
                    break;

                case ConsoleKey.Backspace:
                    ReplExtensions.HandleBackspace(document, view);
                    break;

                case ConsoleKey.Delete:
                    ReplExtensions.HandleDelete(document, view);
                    break;

                case ConsoleKey.LeftArrow:
                    if (view.CurrentChar > 0)
                        view.CurrentChar--;
                    break;
                
                case ConsoleKey.RightArrow:
                    if (view.CurrentChar <= document[view.CurrentLine].Length - 1)
                        view.CurrentChar++;
                    break;
                
                case ConsoleKey.UpArrow:
                    if (view.CurrentLine > 0)
                        view.CurrentLine--;
                    break;
                
                case ConsoleKey.DownArrow:
                    if (view.CurrentLine < document.Count - 1) 
                        view.CurrentLine++;
                    break;

                case ConsoleKey.Escape:
                    document[view.CurrentChar] = string.Empty;
                    view.CurrentChar = 0;
                    break;

                case ConsoleKey.PageUp:
                    HandlePreviousHistory(document, view);
                    break;

                case ConsoleKey.PageDown:
                    HandleNextHistory(document, view);
                    break;

                case ConsoleKey.Home:
                    view.CurrentChar = 0;
                    break;

                case ConsoleKey.End:
                    view.CurrentChar = document[view.CurrentLine].Length;
                    break;
            }
        }

        else if (key.Modifiers == ConsoleModifiers.Control) {
            switch (key.Key) {
                case ConsoleKey.Enter:
                    ReplExtensions.InsertLine(document, view);
                    break;
            }
        }

        if (key.KeyChar >= ' ')
            ReplExtensions.HandleTyping(document, view, key.KeyChar.ToString());
    }

    private void HandleNextHistory(ObservableCollection<string> document, SourceView view) {
        sourceHistoryIndex++;
        if (sourceHistoryIndex > sourceHistory.Count - 1)
            sourceHistoryIndex = 0;

        UpdateDocumentFromHistory(document, view);
    }

    private void HandlePreviousHistory(ObservableCollection<string> document, SourceView view) {
        sourceHistoryIndex--;
        if (sourceHistoryIndex < 0)
            sourceHistoryIndex = sourceHistory.Count - 1;

        UpdateDocumentFromHistory(document, view);
    }

    private void UpdateDocumentFromHistory(ObservableCollection<string> document, SourceView view) {
        document.Clear();
        var historySource = sourceHistory[sourceHistoryIndex];
        var lines = historySource.Split(Environment.NewLine);

        foreach (var line in lines)
            document.Add(line);

        view.CurrentLine = document.Count - 1;
        view.CurrentChar = document[view.CurrentLine].Length;
    }

    private void EvaluateMetaCommands(string input) {
        Console.ForegroundColor = ConsoleColor.DarkGray;

        switch (input) {
            case "!exit":
                Console.WriteLine("Terminated\n");
                Environment.Exit(0);
                break;
            
            case "!clear":
                Console.Clear();
                break;
            
            case "!parsing":
                showParsing = !showParsing;
                Console.WriteLine(showParsing ? "Info: Showing parse tree\n" : "Info: Parse tree hidden\n");
                break;
            
            case "!binding":
                showBinding = !showBinding;
                Console.WriteLine(showBinding ? "Info: Showing bound tree\n" : "Info: Bound tree hidden\n");
                break;
            
            case "!reset": 
                previous = null;
                Console.WriteLine("Info: environment reset done\n");
                break;

            case "!help":
                ReplExtensions.DisplayCommands();
                break;

            default:
                ReplExtensions.ColorPrint($"Error: Invaild command '{input}'\n\n", ConsoleColor.Red);
                break;
        }

        Console.ResetColor();
    }

    private void EvaluateSource(string source) {    
        var syntaxTree = SyntaxTree.Parse(source);
        var compilation = previous is null ? new Compilation(syntaxTree) : previous.ContinueWith(syntaxTree);

        if (showParsing)
            syntaxTree.Root.WriteTo(Console.Out);

        if (showBinding)
            compilation.EmitTree(Console.Out);

        var result = compilation.Evaluate(variables);
        
        if (!result.Diagnostics.Any()) {
            ReplExtensions.ColorPrint($"{result.Result}\n", ConsoleColor.Blue);
            previous = compilation;
        }
        else {
            ReplExtensions.DisplayDiagnostics(syntaxTree, result);
        }
    }

    private Compilation? previous;
    private readonly Dictionary<VariableSymbol, object> variables = [];
    
    private readonly List<string> sourceHistory = [];
    private int sourceHistoryIndex;

    private bool done;
    private bool showParsing;
    private bool showBinding;
}