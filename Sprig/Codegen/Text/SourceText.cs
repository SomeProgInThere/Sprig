using System.Collections.Immutable;

namespace Sprig.Codegen.Text;

public sealed class SourceText {

    public static SourceText FromString(string text, string fileName = "") {
        return new(text, fileName);
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

    public char this[int index] => Text[index];
    public int Length => Text.Length;

    public string ToString(int start, int length) => Text.Substring(start, length);

    public string ToString(TextSpan span) => Text.Substring(span.Start, span.Length);

    public override string ToString() => Text;

    private SourceText(string text, string fileName) {
        Text = text;
        FileName = fileName;
        Lines = ParseLines(this, text);
    }

    private static ImmutableArray<TextLine> ParseLines(SourceText source, string text) {
        var result = ImmutableArray.CreateBuilder<TextLine>();
        var position = 0;
        var start = 0;

        while (position < source.Length) {
            var lineBreakWidth = GetLineBreakWidth(text, position);
            if (lineBreakWidth == 0)
                position++;
            else {
                AddLine(result, source, position, start, lineBreakWidth);
                position += lineBreakWidth;
                start = position;
            }
        }

        if (position >= start)
            AddLine(result, source, position, start, 0);

        return result.ToImmutable();
    }

    private static int GetLineBreakWidth(string text, int position) {
        var current = text[position];
        var next = position + 1 >= text.Length ? '\0' : text[position + 1];

        if (current == '\r' && next == '\n')
            return 2;
        
        if (current == '\r' || next == '\n')
            return 1;
         
        return 0;
    }

    private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText source, int position, int start, int lineBreakWidth) {
        var length = position - start;
        var lengthWithLineBreak = length + lineBreakWidth;
        var line = new TextLine(source, start, length, lengthWithLineBreak);

        result.Add(line);
    }
    
    private string Text { get; }
    public string FileName { get; }
}