
namespace Rubics.Code.Syntax;

internal sealed class Lexer(string source) {

    public Token Lex() {
        if (position >= source.Length)
            return new Token(SyntaxKind.EndOfFileToken, position, "\0");

        if (char.IsDigit(Current)) {
            
            var start = position;
            while (char.IsDigit(Current))
                position++;
            
            var length = position - start;
            var literal = source.Substring(start, length);
            
            if (!int.TryParse(literal, out var value))
                diagnostics.ReportInvalidNumber(new TextSpan(position, length), literal, typeof(int));
            return new Token(SyntaxKind.NumberToken, start, literal, value);
        }

        if (char.IsWhiteSpace(Current)) {
            
            var start = position;
            while (char.IsWhiteSpace(Current))
                position++;
            
            var length = position - start;
            var literal = source.Substring(start, length);

            return new Token(SyntaxKind.WhitespaceToken, start, literal);
        }

        if (char.IsLetter(Current)) {
            
            var start = position;
            while (char.IsLetter(Current))
                position++;
            
            var length = position - start;
            var literal = source.Substring(start, length);
            var kind = SyntaxKindExtensions.GetKeywordKind(literal);

            return new Token(kind, start, literal);
        }
    
        switch (Current) {
            case '+': return new Token(SyntaxKind.PlusToken                 , position++, "+");
            case '-': return new Token(SyntaxKind.MinusToken                , position++, "-");
            case '*': return new Token(SyntaxKind.StarToken                 , position++, "*");
            case '/': return new Token(SyntaxKind.SlashToken                , position++, "/");
            case '%': return new Token(SyntaxKind.PercentToken              , position++, "%");
            case '(': return new Token(SyntaxKind.OpenParenthesisToken      , position++, "("); 
            case ')': return new Token(SyntaxKind.ClosedParenthesisToken    , position++, ")");
            
            case '!':
                if (Next == '=')
                    return new Token(SyntaxKind.BangEqualsToken, ref position, 2, "!=");
                else
                    return new Token(SyntaxKind.BangToken, position++, "!");
            case '=':
                if (Next == '=')
                    return new Token(SyntaxKind.DoubleEqualsToken, ref position, 2, "==");
                else
                    return new Token(SyntaxKind.EqualsToken, position++, "=");
            case '&':
                if (Next == '&')
                    return new Token(SyntaxKind.DoubleAmpersandToken, ref position, 2, "&&");
                break;
            case '|':
                if (Next == '|')
                    return new Token(SyntaxKind.DoublePipeToken, ref position, 2, "||");
                break;
        }
        
        diagnostics.ReportBadCharacter(position, Current);
        return new Token(SyntaxKind.BadToken, position++, source.Substring(position - 1, 1));
    }

    public Diagnostics Diagnostics => diagnostics;

    private char Current => Peek(0);
    private char Next => Peek(1);

    private char Peek(int offset) {
        var index = position + offset;
        if (index >= source.Length) 
            return '\0';
        return source[index];
    }

    private int position;
    private readonly Diagnostics diagnostics = [];
}
