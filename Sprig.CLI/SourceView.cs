using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Sprig.Code.Syntax;

namespace Sprig.CLI;

public class SourceView {

    public SourceView(ObservableCollection<string> sourceDocument) {
        this.sourceDocument = sourceDocument;
        this.sourceDocument.CollectionChanged += SourceDocumentChanged;
        cursorTop = Console.CursorTop;

        RenderDocument();
    }

    public int CurrentLine { 
        get => currentLine; 
        set {
            if (currentLine != value) {
                currentLine = value;
                currentChar = Math.Min(sourceDocument[currentLine].Length, currentChar);

                UpdateCursorPosition();
            }
        } 
    }
    
    public int CurrentChar { 
        get => currentChar; 
        set { 
            if (currentChar != value) {
                currentChar = value;
                UpdateCursorPosition();
            }
        }
    }

    private void SourceDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        RenderDocument();
    }

    private void RenderDocument() {
        Console.CursorVisible = false;
        var lineCount = 0;

        foreach (var line in sourceDocument) {
            Console.SetCursorPosition(0, cursorTop + lineCount);

            if (lineCount == 0)
                ColorPrint("» ", ConsoleColor.Cyan);
            else
                ColorPrint(". ", ConsoleColor.DarkGray);

            RenderLine(line);
            Console.WriteLine(new string(' ', Console.WindowWidth - line.Length));
            lineCount++;
        }

        var blankLineCount = renderedLineCount - lineCount;
        if (blankLineCount > 0) {
            var blankLine = new string(' ', Console.WindowWidth);
            
            for (var i = 0; i < blankLineCount + 1; i++) {
                Console.SetCursorPosition(0, cursorTop + lineCount + i);
                Console.WriteLine(blankLine);
            }
        }

        renderedLineCount = lineCount;
        Console.CursorVisible = true;

        UpdateCursorPosition();
    }

    private static void RenderLine(string line) {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in tokens) {
            var isKeyword = token.Kind.ToString().EndsWith("Keyword");
            var isNumber = token.Kind == SyntaxKind.NumberToken;

            if (isKeyword)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (isNumber)
                Console.ForegroundColor = ConsoleColor.Magenta;

            Console.Write(token.Literal);
            Console.ResetColor();
        }
    }

    private void UpdateCursorPosition() {
        Console.CursorTop = cursorTop + CurrentLine;
        Console.CursorLeft = 2 + CurrentChar;
    }

    private static void ColorPrint(string value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }

    private readonly ObservableCollection<string> sourceDocument = [];
    private int renderedLineCount;
    private int currentLine;
    private int currentChar;
    private readonly int cursorTop;
}