
using System.Collections.Immutable;
using Rubics.Code.Source;

namespace Rubics.Code.Syntax;

internal sealed class Parser {

    public Parser(SourceText sourceText) {
        var tokens = new List<Token>();
        var lexer = new Lexer(sourceText);
        Token token;

        do {
            token = lexer.Lex();
            if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                tokens.Add(token);    
        } 
        while (token.Kind != SyntaxKind.EndOfFileToken);
           
        this.tokens = [..tokens];
        this.sourceText = sourceText;
        diagnostics = lexer.Diagnostics;
    }

    public CompilationUnit ParseCompilationUnit() {
        var expression = ParseAssignmentExpression();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new(expression, endOfFileToken);
    }

    public Diagnostics Diagnostics => diagnostics;

    private Expression ParseAssignmentExpression() {
        if (Current.Kind == SyntaxKind.IdentifierToken && Next.Kind == SyntaxKind.EqualsToken) {
            var identifierToken = NextToken();
            var operatorToken = NextToken();
            var right = ParseAssignmentExpression();

            return new AssignmentExpression(identifierToken, operatorToken, right);
        }

        return ParseBinaryExpression();
    }

    private Expression ParseBinaryExpression(int parentPrecedence = 0) {
        Expression left;
        var unaryPrecedence = Current.Kind.UnaryOperatorPrecedence();

        if (unaryPrecedence != 0) {
            var operatorToken = NextToken();
            var operand = ParseBinaryExpression(unaryPrecedence);
            left = new UnaryExpression(operand, operatorToken);
        }
        else 
            left = ParsePrimaryExpression();

        while (true) {
            var binaryPrecedence = Current.Kind.BinaryOperatorPrecedence();
            if (binaryPrecedence == 0 || binaryPrecedence <= parentPrecedence)
                break;

            var operatorToken = NextToken();
            var right = ParseBinaryExpression(binaryPrecedence);
            left = new BinaryExpression(left, right, operatorToken);
        }

        return left;
    }

    private Expression ParsePrimaryExpression() {
        return Current.Kind switch {
            SyntaxKind.OpenParenthesisToken                     => ParseParenthesizedExpression(),
            SyntaxKind.FalseKeyword or SyntaxKind.TrueKeyword   => ParseBooleanLiteral(),
            SyntaxKind.NumberToken                              => ParseNumberLiteral(),
            SyntaxKind.IdentifierToken or _                     => ParseNameExpression(),
        };
    }

    private Expression ParseParenthesizedExpression() {
        var left = MatchToken(SyntaxKind.OpenParenthesisToken);
        var expression = ParseBinaryExpression();
        var right = MatchToken(SyntaxKind.ClosedParenthesisToken);

        return new ParenthesizedExpression(left, right, expression);
    }

    private Expression ParseBooleanLiteral() {
        var value = Current.Kind == SyntaxKind.TrueKeyword;
        var keywordToken = value ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
        return new LiteralExpression(keywordToken, value);
    }

    private Expression ParseNameExpression() {
        var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpression(identifierToken);
    }

    private Expression ParseNumberLiteral() {
        var numberToken = MatchToken(SyntaxKind.NumberToken);
        return new LiteralExpression(numberToken);
    }

    private Token Peek(int offset) {
        var index = position + offset;
        if (index >= tokens.Length)
            return tokens[^1];
        
        return tokens[index];
    }

    private Token Current => Peek(0);
    private Token Next => Peek(1);

    private Token NextToken() {
        var current = Current;
        position++;
        return current;
    }

    private Token MatchToken(SyntaxKind kind) {
        if (Current.Kind == kind)
            return NextToken();
        
        diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
        return new Token(kind, Current.Position, "\0");
    }

    private readonly ImmutableArray<Token> tokens = [];
    private readonly Diagnostics diagnostics = [];
    private readonly SourceText sourceText;
    private int position;
};
