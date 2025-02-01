
namespace Rubics.Code.Syntax;

internal sealed class Lexer(string source) {

    public Token Lex() {

        start = position;
        kind = SyntaxKind.BadToken;
        value = null;
        
        switch (Current) {
        case '\0': kind = SyntaxKind.EndOfFileToken; break;
        
        case '+': kind = SyntaxKind.PlusToken;              position++; break;
        case '-': kind = SyntaxKind.MinusToken;             position++; break;
        case '*': kind = SyntaxKind.StarToken;              position++; break;
        case '/': kind = SyntaxKind.SlashToken;             position++; break;
        case '%': kind = SyntaxKind.PercentToken;           position++; break;
        case '(': kind = SyntaxKind.OpenParenthesisToken;   position++; break; 
        case ')': kind = SyntaxKind.ClosedParenthesisToken; position++; break;
        
        case '!':
            position++;
            if (Next != '=')
                kind = SyntaxKind.BangToken;
            else {
                kind = SyntaxKind.BangEqualsToken; 
                position++;
            }
            break;

        case '=':
            position++;
            if (Next != '=') 
                kind = SyntaxKind.EqualsToken;
            else {
                kind = SyntaxKind.DoubleEqualsToken; 
                position ++;
            }
            break;

        case '&':
            position++;
            if (Next == '&')
                kind = SyntaxKind.DoubleAmpersandToken; 
            break;
        
        case '|':
            position++;
            if (Next == '|')
                kind = SyntaxKind.DoublePipeToken; 
            break;
        
        default:
            if (char.IsDigit(Current))
                ReadNumberToken();
            
            else if (char.IsLetter(Current))
                ReadIdentifierOrKeywordToken();
            
            else if (char.IsWhiteSpace(Current))
                ReadWhitespaceToken();
            
            else {
                diagnostics.ReportBadCharacter(position, Current);
                position++;
            }

            break;
        }
        
        var length = position - start;
        var literal = SyntaxKindExtensions.GetLiteral(kind);
        literal ??= source.Substring(start, length);

        return new Token(kind, start, literal, value);
    }

    private void ReadWhitespaceToken() {
        while (char.IsWhiteSpace(Current))
            position++;
        
        kind = SyntaxKind.WhitespaceToken;
    }

    private void ReadNumberToken() {
        while (char.IsDigit(Current))
            position++;
            
        var length = position - start;
        var literal = source.Substring(start, length);
            
        if (!int.TryParse(literal, out var result))
            diagnostics.ReportInvalidNumber(new TextSpan(start, length), source, typeof(int));
                
        value = result;
        kind = SyntaxKind.NumberToken;
    }

    private void ReadIdentifierOrKeywordToken() {
        while (char.IsLetter(Current))
            position++;
            
        var length = position - start;
        var literal = source.Substring(start, length);
        kind = SyntaxKindExtensions.GetKeywordKind(literal);
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

    private int position = 0;
    private int start;
    private SyntaxKind kind;
    private object? value;

    private readonly Diagnostics diagnostics = [];
}