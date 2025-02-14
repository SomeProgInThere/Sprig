
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
        diagnostics = lexer.Diagnostics;
    }

    public CompilationUnit ParseCompilationUnit() {
        var statment = ParseStatement();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new(statment, endOfFileToken);
    }

    public Diagnostics Diagnostics => diagnostics;

    private Statement ParseStatement() {
        return Current.Kind switch {
            SyntaxKind.OpenBraceToken => ParseBlockStatement(),
            
            SyntaxKind.LetKeyword or 
            SyntaxKind.VarKeyword => ParseVariableDeclaration(),

            SyntaxKind.IfKeyword => ParseIfStatement(), 
            SyntaxKind.WhileKeyword => ParseWhileStatement(),
            
            _ => ParseExpressionStatement(),
        };
    }

    private Statement ParseBlockStatement() {        
        var statements = ImmutableArray.CreateBuilder<Statement>();
        var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);

        while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.ClosedBraceToken) {
            var statement = ParseStatement();
            statements.Add(statement);
        }

        var closedBraceToken = MatchToken(SyntaxKind.ClosedBraceToken);
        
        return new BlockStatement(openBraceToken, statements.ToImmutable(), closedBraceToken);
    }

    private Statement ParseVariableDeclaration() {
        var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
        
        var keyword = MatchToken(expected);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equalsToken = MatchToken(SyntaxKind.EqualsToken);
        
        var initializer = ParseAssignmentExpression();
        return new VariableDeclarationStatement(keyword, identifier, equalsToken, initializer);
    }
    
    private Statement ParseExpressionStatement() {
        var expression = ParseAssignmentExpression();
        return new ExpressionStatement(expression);
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

    private Statement ParseIfStatement() {
        var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
        var condition = ParseAssignmentExpression();
        var body = ParseStatement();

        ElseClause? elseClause = null;

        if (Current.Kind == SyntaxKind.ElseKeyword) {
            var elseKeyword = NextToken();
            var elseBody = ParseStatement();
            elseClause = new ElseClause(elseKeyword, elseBody);
        }

        return new IfStatement(ifKeyword, condition, body, elseClause);
    }

    private Statement ParseWhileStatement() {
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseAssignmentExpression();
        var body = ParseStatement();

        return new WhileStatement(whileKeyword, condition, body);
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
    private int position;
};
