using System.Collections.Immutable;

namespace Sprig.Codegen.Source;

public sealed class SourceText {

    public static SourceText FromString(string source, string fileName = "") {
        return new(source, fileName);
    }

    public ImmutableArray<TextLine> Lines { get; }

    public int GetLineIndex(int position) {
        var lower = 0;
        var upper = Lines.Length - 1;

        while (lower <= upper) {
            var index = (lower + upper) / 2;
            var start = Lines[index].Start;

            if (position == start) 
                return index;
            
            if (start > position)
                upper = index - 1;
            else
                lower = index + 1;
        }

        return lower - 1;
    }

    public char this[int index] => Source[index];
    public int Length => Source.Length;

    public string ToString(int start, int length) => Source.Substring(start, length);

    public string ToString(TextSpan span) => Source.Substring(span.Start, span.Length);

    public override string ToString() => Source;

    private SourceText(string source, string fileName) {
        Source = source;
        FileName = fileName;
        Lines = ParseLines(this, source);
    }

    private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string source) {
        var result = ImmutableArray.CreateBuilder<TextLine>();
        var position = 0;
        var start = 0;

        while (position < source.Length) {
            var lineBreakWidth = GetLineBreakWidth(source, position);
            if (lineBreakWidth == 0)
                position++;
            else {
                AddLine(result, sourceText, position, start, lineBreakWidth);
                position += lineBreakWidth;
                start = position;
            }
        }

        if (position >= start)
            AddLine(result, sourceText, position, start, 0);

        return result.ToImmutable();
    }

    private static int GetLineBreakWidth(string source, int position) {
        var current = source[position];
        var next = position + 1 >= source.Length ? '\0' : source[position + 1];

        if (current == '\r' && next == '\n')
            return 2;
        
        if (current == '\r' || next == '\n')
            return 1;
         
        return 0;
    }

    private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int start, int lineBreakWidth) {
        var length = position - start;
        var lengthWithLineBreak = length + lineBreakWidth;
        var line = new TextLine(sourceText, start, length, lengthWithLineBreak);

        result.Add(line);
    }
    
    private string Source { get; }
    public string FileName { get; }
}