using System.Text;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Text;

namespace Sprig.Codegen.Syntax;

internal sealed class Lexer(SyntaxTree syntaxTree) {

    public SyntaxToken Lex() {

        start = position;
        kind = SyntaxKind.BadTokenTrivia;
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
            case '+': kind = SyntaxKind.PlusToken;              position++; break;
            case '-': kind = SyntaxKind.MinusToken;             position++; break;
            case '*': kind = SyntaxKind.StarToken;              position++; break;
            case '^': kind = SyntaxKind.CircumflexToken;        position++; break;
            case '%': kind = SyntaxKind.PercentToken;           position++; break;

            case '/':
                if (Next == '/')
                    ReadSinglelineComment();
                else if (Next == '*')
                    ReadMultilineComment();

                else {
                    kind = SyntaxKind.SlashToken; 
                    position++;
                }
                break;

            case '!':
                SetKind(ref kind, ref position, '=', SyntaxKind.BangToken, SyntaxKind.BangEqualsToken);
                break;

            case '=':
                SetKind(ref kind, ref position, '=', SyntaxKind.EqualsToken, SyntaxKind.DoubleEqualsToken);
                break;

            case '<':
                SetKind(
                    ref kind, ref position, '<', '=', 
                    SyntaxKind.RightArrowToken, SyntaxKind.DoubleRightArrowToken, SyntaxKind.RightArrowEqualsToken
                );
                break;

            case '>':
                SetKind(
                    ref kind, ref position, '>', '=', 
                    SyntaxKind.LeftArrowToken, SyntaxKind.DoubleLeftArrowToken, SyntaxKind.LeftArrowEqualsToken
                );
                break;

            case '&':
                SetKind(
                    ref kind, ref position, '&', SyntaxKind.AmpersandToken, SyntaxKind.DoubleAmpersandToken
                );
                break;
            
            case '|':
                SetKind(ref kind, ref position, '|', SyntaxKind.PipeToken, SyntaxKind.DoublePipeToken);
                break;

            case '.':
                SetKind(ref kind, ref position, '.', SyntaxKind.DotToken, SyntaxKind.DoubleDotToken);
                break;

            case '"':
                ReadString();
                break;

            case '_':
                ReadIdentifierOrKeywordToken();
                break;

            default:
                if (char.IsDigit(Current))
                    ReadNumberToken();
                
                else if (char.IsLetter(Current))
                    ReadIdentifierOrKeywordToken();
                
                else if (char.IsWhiteSpace(Current))
                    ReadWhitespaceToken();
                
                else {
                    var span = new TextSpan(position, 1);
                    var location = new TextLocation(syntaxTree.Source, span);
                    diagnostics.ReportBadCharacter(location, Current);
                    position++;
                }

            break;
        }
        
        var length = position - start;
        var text = kind.Text() ?? syntaxTree.Source.ToString(start, length);
        return new SyntaxToken(syntaxTree, kind, start, text, value);
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

    private void ReadSinglelineComment() {
        position += 2;
        var done = false;

        while (!done) {
            switch (Current) {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                
                default:
                    position++;
                    break;
            }
        }

        kind = SyntaxKind.SinglelineCommentTrivia;
    }

    private void ReadMultilineComment() {
        position += 2;
        var done = false;

        while (!done) {
            switch (Current) {
                case '\0':
                    var span = new TextSpan(start, 1);
                    var location = new TextLocation(syntaxTree.Source, span);
                    diagnostics.ReportUnterminatedComment(location);
                    done = true;
                    break;
                
                case '*':
                    if (Next == '/') {
                        done = true;
                        position ++;
                    }
                    
                    position++;
                    break;

                default:
                    position++;
                    break;
            }
        }

        kind = SyntaxKind.MultilineCommentTrivia;
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
                    var location = new TextLocation(syntaxTree.Source, span);
                    diagnostics.ReportUnterminatedString(location);
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
        
        kind = SyntaxKind.WhitespaceTrivia;
    }

    private void ReadNumberToken() {
        var isDecimal = false;
        while (char.IsDigit(Current) || Current == '.') {
            if (Current == '.') {
                if (Next == '.') {
                    break;
                }

                if (isDecimal)
                    break;
                isDecimal = true;
            }

            position++;
        }
            
        var length = position - start;
        var text = syntaxTree.Source.ToString(start, length);

        if (!isDecimal) {
            if (!int.TryParse(text, out var intResult)) {
                var span = new TextSpan(start, length);
                var location = new TextLocation(syntaxTree.Source, span);
                diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int32);
            }
            value = intResult;
        }
        else {
            if (!double.TryParse(text, out var doubleResult)) {
                var span = new TextSpan(start, length);
                var location = new TextLocation(syntaxTree.Source, span);
                diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Double);
            }
            value = doubleResult;
        }

        kind = SyntaxKind.NumberToken;
    }

    private void ReadIdentifierOrKeywordToken() {
        while (char.IsLetter(Current) || Current == '_')
            position++;
        
        var length = position - start;
        var text = syntaxTree.Source.ToString(start, length);
        kind = text.KeywordKind();
    }
    
    public Diagnostics Diagnostics => diagnostics;

    private char Current => Peek(0);
    private char Next => Peek(1);

    private char Peek(int offset) {
        var index = position + offset;
        if (index >= syntaxTree.Source.Length) 
            return '\0';
        return syntaxTree.Source[index];
    }

    private int position;
    private int start;
    private object? value;

    private SyntaxKind kind;
    private readonly Diagnostics diagnostics = [];
}