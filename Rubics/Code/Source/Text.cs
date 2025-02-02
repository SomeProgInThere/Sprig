
namespace Rubics.Code.Source;

public readonly struct TextSpan(int start, int length) {
    public static TextSpan CreateFromBounds(int start, int end) {
        var length = end - start;
        return new(start, length);
    }
    
    public int Start { get; } = start;
    public int Length { get; } = length;
    public readonly int End => Start + Length;
}

public sealed class TextLine(SourceText source, int start, int length, int lengthWithLineBreak) {
    public SourceText Source { get; } = source;

    public TextSpan Span => new(Start, Length);
    public TextSpan SpanWithLineBreak => new(Start, LengthWithLineBreak);

    public int Start { get; } = start;
    public int Length { get; } = length;
    public int LengthWithLineBreak { get; } = lengthWithLineBreak;
    public int End => Start + Length;

    public override string ToString() => Source.ToString(Span);
}