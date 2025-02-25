using Sprig.Code.Source;

namespace Sprig.Code.Syntax;

internal sealed class Lexer(SourceText source) {

    public Token Lex() {

        start = position;
        kind = SyntaxKind.BadToken;
        value = null;
        
        switch (Current) {
        case '\0': kind = SyntaxKind.EndOfFileToken; break;
        
        case '(': kind = SyntaxKind.OpenParenthesisToken;   position++; break; 
        case ')': kind = SyntaxKind.ClosedParenthesisToken; position++; break;
        case '{': kind = SyntaxKind.OpenBraceToken;         position++; break; 
        case '}': kind = SyntaxKind.ClosedBraceToken;       position++; break;
        case '~': kind = SyntaxKind.TildeToken;             position++; break;

        case '+':
            SetKind(
                ref kind, ref position, 
                '=', '+', 
                SyntaxKind.PlusToken, SyntaxKind.PlusEqualsToken, SyntaxKind.DoublePlusToken
            );
            break;

        case '-':
            SetKind(
                ref kind, ref position, 
                '=', '-', 
                SyntaxKind.MinusToken, SyntaxKind.MinusEqualsToken, SyntaxKind.DoubleMinusToken
            );
            break;

        case '*':
            SetKind(
                ref kind, ref position, 
                '=', '*', 
                SyntaxKind.StarToken, SyntaxKind.StarEqualsToken, SyntaxKind.DoubleStarToken
            );
            break;

        case '/':
            SetKind(
                ref kind, ref position, 
                '=', '/', 
                SyntaxKind.SlashToken, SyntaxKind.SlashEqualsToken, SyntaxKind.DoubleSlashToken
            );
            break;

        case '%':
            SetKind(
                ref kind, ref position, 
                '=', 
                SyntaxKind.PercentToken, SyntaxKind.PercentEqualsToken
            );
            break;

        case '!':
            SetKind(
                ref kind, ref position, 
                '=', SyntaxKind.BangToken, SyntaxKind.BangEqualsToken
            );
            break;

        case '=':
            SetKind(
                ref kind, ref position, 
                '=', SyntaxKind.EqualsToken, SyntaxKind.DoubleEqualsToken
            );
            break;

        case '^':
            SetKind(
                ref kind, ref position, 
                '=', SyntaxKind.CircumflexToken, SyntaxKind.CircumflexEqualsToken
            );
            break;

        case '<':
            SetKind(
                ref kind, ref position, 
                '<', '=', 
                SyntaxKind.RightArrowToken, SyntaxKind.DoubleRightArrowToken, SyntaxKind.RightArrowEqualsToken
            );
            break;

        case '>':
            SetKind(
                ref kind, ref position, 
                '>', '=', 
                SyntaxKind.LeftArrowToken, SyntaxKind.DoubleLeftArrowToken, SyntaxKind.LeftArrowEqualsToken
            );
            break;

        case '&':
            SetKind(
                ref kind, ref position, 
                '&', '=', 
                SyntaxKind.AmpersandToken, SyntaxKind.DoubleAmpersandToken, SyntaxKind.AmpersandEqualsToken
            );
            break;
        
        case '|':
            SetKind(
                ref kind, ref position, 
                '|', '=', 
                SyntaxKind.PipeToken, SyntaxKind.DoublePipeToken, SyntaxKind.PipeEqualsToken
            );
            break;

        case '.':
            SetKind(
                ref kind, ref position,
                '.', 
                SyntaxKind.DotToken, SyntaxKind.DoubleDotToken
            );
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
        var literal = kind.GetLiteral() ?? source.ToString(start, length);
        return new Token(kind, start, literal, value);
    }

    private void SetKind(ref SyntaxKind kind, ref int position, char l1, SyntaxKind k1, SyntaxKind k2) {
        if (Next == l1) { kind = k2; position+= 2; return; }
        kind = k1; position++;
    }

    private void SetKind(ref SyntaxKind kind, ref int position, char l1, char l2, SyntaxKind k1, SyntaxKind k2, SyntaxKind k3) {
        if (Next == l1) { kind = k2; position += 2; return; }
        if (Next == l2) { kind = k3; position += 2; return; }
        kind = k1; position++;
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
        var literal = source.ToString(start, length);
            
        if (!int.TryParse(literal, out var result))
            diagnostics.ReportInvalidNumber(new TextSpan(start, length), literal, typeof(int));
                
        value = result;
        kind = SyntaxKind.NumberToken;
    }

    private void ReadIdentifierOrKeywordToken() {
        while (char.IsLetter(Current))
            position++;
            
        var length = position - start;
        var literal = source.ToString(start, length);
        kind = literal.GetKeywordKind();
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
    private int start;
    private SyntaxKind kind;
    private object? value;

    private readonly Diagnostics diagnostics = [];
}