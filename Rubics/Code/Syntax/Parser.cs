
namespace Rubics.Code.Syntax;

internal sealed class Parser {

    public Parser(string source) {
        var tokens = new List<Token>();
        var lexer = new Lexer(source);
        Token token;

        do {
            token = lexer.Lex();
            if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                tokens.Add(token);    
        } 
        while (token.Kind != SyntaxKind.EndOfFileToken);
        
        this.tokens = [..tokens];
    }

    public SyntaxTree Parse() {
        var expression = ParseAssignmentExpression();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new SyntaxTree(expression, endOfFileToken, diagnostics);
    }

    private Expression ParseAssignmentExpression() {
        if (Current.Kind == SyntaxKind.IdentifierToken && Next.Kind == SyntaxKind.EqualsToken) {
            var identifierToken = NextToken();
            var operatorToken = NextToken();
            var right = ParseAssignmentExpression();

            return new AssignmentExpression(identifierToken, operatorToken, right);
        }

        return ParseBinaryExpression();
    }

    public Diagnostics Diagnostics => diagnostics;

    private Expression ParseBinaryExpression(int parentPrecedence = 0) {
        Expression left;

        var unaryPrecedence = Current.Kind.UnaryOperatorPrecedence();
        if (unaryPrecedence != 0) {
            
            var operatorToken = NextToken();
            var operand = ParseBinaryExpression(unaryPrecedence);
            left = new UnaryExpression(operand, operatorToken);
        }
        else {
            left = ParsePrimaryExpression();
        }

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
        switch (Current.Kind) {
            case SyntaxKind.OpenParenthesisToken: {
                var left = NextToken();
                var expression = ParseBinaryExpression();
                var right = MatchToken(SyntaxKind.ClosedParenthesisToken);

                return new ParenthesizedExpression(left, right, expression);
            }

            case SyntaxKind.TrueKeyword:
            case SyntaxKind.FalseKeyword: {
                var keywordToken = NextToken();
                var value = keywordToken.Kind == SyntaxKind.TrueKeyword;
            
                return new LiteralExpression(keywordToken, value);
            }

            case SyntaxKind.IdentifierToken: {
                var identifierToken = NextToken();
                return new NameExpression(identifierToken);
            }

            default: {
                var numberToken = MatchToken(SyntaxKind.NumberToken);
                return new LiteralExpression(numberToken);
            }
        }
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

    private readonly Token[] tokens = [];
    private readonly Diagnostics diagnostics = [];
    private int position;
};
