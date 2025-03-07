using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Sprig.Code.Syntax;

namespace Sprig.Interpreter;

public class SourceView {

    public SourceView(ObservableCollection<string> sourceDocument) {
        this.sourceDocument = sourceDocument;
        this.sourceDocument.CollectionChanged += SourceDocumentChanged;
        cursorTop = Console.CursorTop;

        RenderDocument();
    }

    private void SourceDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        RenderDocument();
    }

    private void RenderDocument() {
        Console.CursorVisible = false;
        var lineCount = 0;

        foreach (var line in sourceDocument) {
            if (cursorTop + lineCount >= Console.WindowHeight) {
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.WriteLine();

                if (cursorTop > 0)
                    cursorTop--;
            }

            Console.SetCursorPosition(0, cursorTop + lineCount);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(lineCount == 0 ? "» " : ". ");

            Console.ResetColor();

            RenderLine(line);
            Console.Write(new string(' ', Console.WindowWidth - line.Length - 2));
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
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
            var isString = token.Kind == SyntaxKind.StringToken;

            if (isKeyword)
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (isIdentifier)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (isNumber)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (isString)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write(token.Literal);
            Console.ResetColor();
        }
    }

    private void UpdateCursorPosition() {
        Console.CursorTop = cursorTop + currentLine;
        Console.CursorLeft = 2 + currentChar;
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

    private readonly ObservableCollection<string> sourceDocument = [];

    private int renderedLineCount;
    private int currentLine;
    private int currentChar;
    private int cursorTop;
}