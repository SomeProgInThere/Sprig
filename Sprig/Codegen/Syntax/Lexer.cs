using System.Text;
using Sprig.Codegen.Source;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.Syntax;

internal sealed class Lexer(SourceText source) {

    public SyntaxToken Lex() {

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
            case ',': kind = SyntaxKind.CommaToken;             position++; break;
            case ':': kind = SyntaxKind.ColonToken;             position++; break;

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

            case '"':
                ReadString();
                break;

            default:
                if (char.IsDigit(Current))
                    ReadNumberToken();
                
                else if (char.IsLetter(Current))
                    ReadIdentifierOrKeywordToken();
                
                else if (char.IsWhiteSpace(Current))
                    ReadWhitespaceToken();
                
                else {
                    diagnostics.ReportBadCharacter(new TextSpan(position, 1), Current);
                    position++;
                }

                break;
        }
        
        var length = position - start;
        var literal = kind.Literal() ?? source.ToString(start, length);
        return new SyntaxToken(kind, start, literal, value);
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

    private void ReadString() {
        position++;
        var builder = new StringBuilder();
        var done = false;

        while (!done) {
            switch (Current) {
                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(start, 1);
                    diagnostics.ReportUnterminatedString(span);
                    done = true;
                    break;

                case '"':
                    if (Next == '"') {
                        builder.Append(Current);
                        position += 2;
                    }
                    else {
                        position++;
                        done = true;
                    }
                    break;

                default:
                    builder.Append(Current);
                    position++;
                    break;
            }
        }

        kind = SyntaxKind.StringToken;
        value = builder.ToString();
    }

    private void ReadWhitespaceToken() {
        while (char.IsWhiteSpace(Current))
            position++;
        
        kind = SyntaxKind.WhitespaceToken;
    }

    private void ReadNumberToken() {
        var isFloat = false;
        while (char.IsDigit(Current) || Current == '.') {
            if (Current == '.') {
                if (Next == '.') {
                    break;
                }

                if (isFloat)
                    break;
                isFloat = true;
            }
            position++;
        }
            
        var length = position - start;
        var literal = source.ToString(start, length);

        if (!isFloat) {
            if (!int.TryParse(literal, out var intResult))
                diagnostics.ReportInvalidNumber(new TextSpan(start, length), literal, TypeSymbol.Int);
            value = intResult;
        }
        else {
            if (!float.TryParse(literal, out var floatResult))
                diagnostics.ReportInvalidNumber(new TextSpan(start, length), literal, TypeSymbol.Float);
            value = floatResult;
        }

        kind = SyntaxKind.NumberToken;
    }

    private void ReadIdentifierOrKeywordToken() {
        while (char.IsLetter(Current))
            position++;
            
        var length = position - start;
        var literal = source.ToString(start, length);
        kind = literal.KeywordKind();
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
    private object? value;

    private SyntaxKind kind;
    private readonly Diagnostics diagnostics = [];
}