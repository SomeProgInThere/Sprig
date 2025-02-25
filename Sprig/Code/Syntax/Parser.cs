using System.Collections.Immutable;

using Sprig.Code.Source;

namespace Sprig.Code.Syntax;

internal sealed class Parser {

    public Parser(SourceText sourceText) {
        var tokens = new List<SyntaxToken>();
        var lexer = new Lexer(sourceText);
        SyntaxToken token;

        do {
            token = lexer.Lex();
            if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                tokens.Add(token);    
        } 
        while (token.Kind != SyntaxKind.EndOfFileToken);
           
        this.tokens = [..tokens];
        Diagnostics = lexer.Diagnostics;
    }

    public CompilationUnit ParseCompilationUnit() {
        var statement = ParseStatement();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new CompilationUnit(statement, endOfFileToken);
    }

    public Diagnostics Diagnostics { get; }

    private Statement ParseStatement() => Current.Kind switch {
        SyntaxKind.OpenBraceToken   => ParseBlockStatement(),
        SyntaxKind.LetKeyword or 
        SyntaxKind.VarKeyword       => ParseVariableDeclaration(),
        SyntaxKind.IfKeyword        => ParseIfStatement(),
        SyntaxKind.WhileKeyword     => ParseWhileStatement(),
        SyntaxKind.ForKeyword       => ParseForStatement(),
        SyntaxKind.IdentifierToken  => ParseAssignOperationStatement(),
        
        _ => ParseExpressionStatement(),
    };

    private Statement ParseBlockStatement() {        
        var statements = ImmutableArray.CreateBuilder<Statement>();
        var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken); 

        while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.ClosedBraceToken) {
            var startToken = Current;

            var statement = ParseStatement();
            statements.Add(statement);

            if (Current == startToken)
                NextToken();
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

    private Statement ParseAssignOperationStatement() {
        var isAssignOperator = Next.Kind switch {
            SyntaxKind.PlusEqualsToken          or
            SyntaxKind.MinusEqualsToken         or
            SyntaxKind.StarEqualsToken          or
            SyntaxKind.SlashEqualsToken         or
            SyntaxKind.PercentEqualsToken       or
            SyntaxKind.AmpersandEqualsToken     or
            SyntaxKind.PipeEqualsToken          or
            SyntaxKind.CircumflexEqualsToken    => true,
            _ => false,
        };
        
        if (isAssignOperator) {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var assignOperatorToken = NextToken();
            var expression = ParseAssignmentExpression();
            
            return new AssignOperationStatement(identifier, assignOperatorToken, expression);
        }

        return ParseExpressionStatement();
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
            orderedClauses.Push(elseClause);
        } 
        else {
            orderedClauses.Push(elseClause);
            if (orderedClauses.Count > 0 && orderedClauses.Peek() != null) {
                return new IfStatement(ifKeyword, condition, body, orderedClauses.Pop());
            }
        }
        
        return new IfStatement(ifKeyword, condition, body, elseClause);
    }

    private Statement ParseWhileStatement() {
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseAssignmentExpression();
        var body = ParseStatement();

        return new WhileStatement(whileKeyword, condition, body);
    }

    private Statement ParseForStatement() {
        var forKeyword = MatchToken(SyntaxKind.ForKeyword);
        var identifierToken = NextToken();
        var inKeyword = MatchToken(SyntaxKind.InKeyword);
        
        var range = ParseRangeExpression();
        var body = ParseStatement();

        return new ForStatement(forKeyword, identifierToken, inKeyword, range, body);
    }

    private Statement ParseExpressionStatement() {
        var expression = ParseAssignmentExpression();
        return new ExpressionStatement(expression);
    }

    private Expression ParseAssignmentExpression() {
        if (Current.Kind == SyntaxKind.IdentifierToken) {

            if (Next.Kind == SyntaxKind.EqualsToken) {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();

                return new AssignmentExpression(identifierToken, operatorToken, right);
            }

            if (Next.Kind == SyntaxKind.DoublePlusToken || Next.Kind == SyntaxKind.DoubleMinusToken) {
                var operand = ParsePrimaryExpression();
                var operatorToken = NextToken();
                return new UnaryExpression(operand, operatorToken);
            }
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
        return Current.Kind switch {
            SyntaxKind.OpenParenthesisToken                     => ParseParenthesizedExpression(),
            SyntaxKind.FalseKeyword or SyntaxKind.TrueKeyword   => ParseBooleanLiteral(),
            SyntaxKind.NumberToken                              => ParseNumberLiteral(),
            SyntaxKind.IdentifierToken or _                     => ParseNameExpression(),
        };
    }

    private Expression ParseParenthesizedExpression() {
        var left = MatchToken(SyntaxKind.OpenParenthesisToken);
        var expression = ParseAssignmentExpression();
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

    private Expression ParseRangeExpression() {
        var lower = ParsePrimaryExpression();
        var rangeToken = MatchToken(SyntaxKind.DoubleDotToken);
        var upper = ParsePrimaryExpression();

        return new RangeExpression(lower, rangeToken, upper);
    }

    private SyntaxToken Peek(int offset) {
        var index = position + offset;
        return index >= tokens.Length ? tokens[^1] : tokens[index];
    }

    private SyntaxToken Current => Peek(0);
    private SyntaxToken Next => Peek(1);

    private SyntaxToken NextToken() {
        var current = Current;
        position++;
        return current;
    }

    private SyntaxToken MatchToken(SyntaxKind kind) {
        if (Current.Kind == kind)
            return NextToken();
        
        Diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
        return new SyntaxToken(kind, Current.Position, "\0");
    }

    private readonly Stack<ElseClause?> orderedClauses = new();
    private readonly ImmutableArray<SyntaxToken> tokens;

    private int position;
};
