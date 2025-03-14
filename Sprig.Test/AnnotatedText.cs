
using System.Collections.Immutable;
using System.Text;
using Sprig.Codegen.Text;

namespace Sprig.Tests;

public class AnnotatedText(string source, ImmutableArray<TextSpan> spans) {

    public static AnnotatedText Parse(string source) {
        source = Unindent(source);

        var sourceBuilder = new StringBuilder();
        var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
        var blockStart = new Stack<int>();

        var position = 0;
        foreach (var literal in source) {
            
            if (literal == '[')
                blockStart.Push(position);

            else if (literal == ']') {
                if (blockStart.Count == 0)
                    throw new ArgumentException("Unwanted ']' in source", nameof(source));
                
                var start = blockStart.Pop();
                var end = position;
                var span = TextSpan.CreateFromBounds(start, end);

                spanBuilder.Add(span);
            }

            else {
                position++;
                sourceBuilder.Append(literal);
            }
        }

        if (blockStart.Count != 0)
            throw new ArgumentException("Missing ']' in source", nameof(source));    
        
        return new(sourceBuilder.ToString(), spanBuilder.ToImmutable());
    }

    public static string[] UnindentLines(string source) {
        var lines = new List<string>();

        using (var reader = new StringReader(source)) {
            string? line;
            while ((line = reader.ReadLine()) != null)
                lines.Add(line);
        }

        var minIndents = int.MaxValue;
        for (var i = 0; i < lines.Count; i++) {
            var line = lines[i];
            
            if (line.Trim().Length == 0) {
                lines[i] = string.Empty;
                continue;
            }

            var indent = line.Length - line.TrimStart().Length;
            minIndents = Math.Min(minIndents, indent);
        }

        for (var i = 0; i < lines.Count; i++) {
            if (lines[i].Length == 0)
                continue;

            lines[i] = lines[i][minIndents..];
        }

        while (lines.Count > 0 && lines[0].Length == 0)
            lines.RemoveAt(0);

        while (lines.Count > 0 && lines[^1].Length == 0)
            lines.RemoveAt(lines.Count - 1);
 
        return [..lines];
    }

    private static string Unindent(string source) {
        var lines = UnindentLines(source);
        return string.Join(Environment.NewLine, lines);
    }

    public string Source { get; } = source;
    public ImmutableArray<TextSpan> Spans { get; } = spans;
}