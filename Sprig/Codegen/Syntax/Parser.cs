using System.Collections.Immutable;
using Sprig.Codegen.Source;

namespace Sprig.Codegen.Syntax;

internal sealed class Parser {

    public Parser(SourceText sourceText) {
        var sourceTokens = new List<SyntaxToken>();
        var lexer = new Lexer(sourceText);
        SyntaxToken token;

        do {
            token = lexer.Lex();
            if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                sourceTokens.Add(token);    
        } 
        while (token.Kind != SyntaxKind.EndOfFileToken);
           
        tokens = [..sourceTokens];
        source = sourceText;
        Diagnostics = lexer.Diagnostics;
    }

    public CompilationUnit ParseCompilationUnit() {
        var members = ParseMembers();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new CompilationUnit(members, endOfFileToken);
    }

    public Diagnostics Diagnostics { get; }

    private ImmutableArray<Member> ParseMembers() {
        var members = ImmutableArray.CreateBuilder<Member>();

        while (Current.Kind != SyntaxKind.EndOfFileToken) {
            var startToken = Current;
            Member? member;
            
            if (Current.Kind == SyntaxKind.FuncKeyword)
                member = ParseFunctionHeader();
            else {
                var statement = ParseStatement();
                member = new GlobalStatment(statement);
            }

            members.Add(member);
            if (Current == startToken)
                NextToken();
        }

        return members.ToImmutable();
    }

    private Member ParseFunctionHeader() {
        var funcKeyword = MatchToken(SyntaxKind.FuncKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);

        var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameters();
        var closedParenthesisToken = MatchToken(SyntaxKind.ClosedParenthesisToken);

        var returnType = ParseTypeClause();
        var body = (BlockStatement)ParseBlockStatement();

        return new FunctionHeader(
            funcKeyword, 
            identifier, 
            openParenthesisToken, 
            parameters, 
            closedParenthesisToken, 
            returnType, 
            body
        );
    }

    private Statement ParseStatement() => Current.Kind switch {
        SyntaxKind.OpenBraceToken   => ParseBlockStatement(),
        SyntaxKind.LetKeyword or 
        SyntaxKind.VarKeyword       => ParseVariableDeclarationStatement(),
        SyntaxKind.IfKeyword        => ParseIfStatement(),
        SyntaxKind.WhileKeyword     => ParseWhileStatement(),
        SyntaxKind.DoKeyword        => ParseDoWhileStatement(),
        SyntaxKind.ForKeyword       => ParseForStatement(),
        SyntaxKind.BreakKeyword     => ParseBreakStatement(),
        SyntaxKind.ContinueKeyword  => ParseContinueStatement(),
        SyntaxKind.ReturnKeyword    => ParseReturnStatement(),
        
        _ => ParseExpressionStatement(),
    };

    private Statement ParseReturnStatement() {
        var keyword = MatchToken(SyntaxKind.ReturnKeyword);
        
        var keywordLine = source.GetLineIndex(keyword.Span.Start);
        var currentLine = source.GetLineIndex(Current.Span.Start);
        var isEof = Current.Kind == SyntaxKind.EndOfFileToken;
        var isSameLine = !isEof && keywordLine == currentLine;

        var expression = isSameLine ? ParseAssignmentExpression() : null;
        return new ReturnStatement(keyword, expression);
    }

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

    private Statement ParseVariableDeclarationStatement() {
        var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;

        var keyword = MatchToken(expected);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeClause = ParseTypeClause();
        var equalsToken = MatchToken(SyntaxKind.EqualsToken);

        var initializer = ParseAssignmentExpression();
        return new VariableDeclarationStatement(keyword, identifier, typeClause, equalsToken, initializer);
    }

    private TypeClause? ParseTypeClause() {
        if (Current.Kind != SyntaxKind.ColonToken)
            return null;
    
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var typeIdentifier = MatchToken(SyntaxKind.IdentifierToken);
        return new TypeClause(colonToken, typeIdentifier);
    }

    private Statement ParseIfStatement()
    {
        var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
        var condition = ParseAssignmentExpression();
        var body = ParseStatement();
        ElseClause? elseClause = ParseElseClause();

        return new IfStatement(ifKeyword, condition, body, elseClause);
    }

    private ElseClause? ParseElseClause() {        
        ElseClause? clause = null;
        if (Current.Kind == SyntaxKind.ElseKeyword) {
            var elseKeyword = NextToken();
            var elseBody = ParseStatement();

            clause = new ElseClause(elseKeyword, elseBody);
            orderedClauses.Push(clause);
        }
        else {
            orderedClauses.Push(clause);
            if (orderedClauses.Count > 0 && orderedClauses.Peek() != null) {
                return orderedClauses.Pop();
            }
        }

        return clause;
    }

    private Statement ParseWhileStatement() {
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseAssignmentExpression();
        var body = ParseStatement();

        return new WhileStatement(whileKeyword, condition, body);
    }

    
    private Statement ParseDoWhileStatement() {
        var doKeyword = MatchToken(SyntaxKind.DoKeyword);
        var body = ParseStatement();
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseAssignmentExpression();

        return new DoWhileStatement(doKeyword, body, whileKeyword, condition);
    }

    private Statement ParseForStatement() {
        var forKeyword = MatchToken(SyntaxKind.ForKeyword);
        var identifierToken = NextToken();
        var inKeyword = MatchToken(SyntaxKind.InKeyword);
        
        var range = ParseRangeExpression();
        var body = ParseStatement();

        return new ForStatement(forKeyword, identifierToken, inKeyword, range, body);
    }

    private Statement ParseBreakStatement() {
        var keyword = MatchToken(SyntaxKind.BreakKeyword);
        return new BreakStatement(keyword);
    }

    private Statement ParseContinueStatement() {
        var keyword = MatchToken(SyntaxKind.ContinueKeyword);
        return new ContinueStatement(keyword);
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
            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    private Expression ParsePrimaryExpression() {
        return Current.Kind switch {
            SyntaxKind.OpenParenthesisToken                     => ParseParenthesizedExpression(),
            SyntaxKind.FalseKeyword or SyntaxKind.TrueKeyword   => ParseBooleanLiteral(),
            SyntaxKind.NumberToken                              => ParseNumberLiteral(),
            SyntaxKind.StringToken                              => ParseStringLiteral(),
            SyntaxKind.IdentifierToken or _                     => ParseNameOrCallExpression(),
        };
    }

    private Expression ParseParenthesizedExpression() {
        var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        var expression = ParseAssignmentExpression();
        var closedParenthesisToken = MatchToken(SyntaxKind.ClosedParenthesisToken);

        return new ParenthesizedExpression(openParenthesisToken, expression, closedParenthesisToken);
    }

    private Expression ParseBooleanLiteral() {
        var value = Current.Kind == SyntaxKind.TrueKeyword;
        var keywordToken = value ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
        return new LiteralExpression(keywordToken, value);
    }

    private Expression ParseNameOrCallExpression() {
        if (Current.Kind == SyntaxKind.IdentifierToken && Next.Kind == SyntaxKind.OpenParenthesisToken) {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var arguments = ParseArguments();
            var closedParenthesisToken = MatchToken(SyntaxKind.ClosedParenthesisToken);

            return new CallExpression(identifier, openParenthesisToken, arguments, closedParenthesisToken);
        }

        var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpression(identifierToken);
    }

    private SeparatedSyntaxList<Expression> ParseArguments() {
        var nodesWithSeperators = ImmutableArray.CreateBuilder<SyntaxNode>();
        var parseNextArgument = true;

        while (
            parseNextArgument 
            && Current.Kind != SyntaxKind.ClosedParenthesisToken 
            && Current.Kind != SyntaxKind.EndOfFileToken 
        ) {
            var expression = ParseAssignmentExpression();
            nodesWithSeperators.Add(expression);

            if (Current.Kind == SyntaxKind.CommaToken) {
                var commaToken = MatchToken(SyntaxKind.CommaToken);
                nodesWithSeperators.Add(commaToken);
            }
            else {
                parseNextArgument = false;
            }
        }
        
        return new SeparatedSyntaxList<Expression>(nodesWithSeperators.ToImmutable());
    }

    private SeparatedSyntaxList<FunctionParameter> ParseParameters() {
        var parameters = ImmutableArray.CreateBuilder<SyntaxNode>();
        var parseNextParameter = true;

        while (
            parseNextParameter
            && Current.Kind != SyntaxKind.ClosedParenthesisToken
            && Current.Kind != SyntaxKind.EndOfFileToken
        ) { 
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var type = ParseTypeClause();
            var parameter = new FunctionParameter(identifier, type);

            parameters.Add(parameter);

            if (Current.Kind == SyntaxKind.CommaToken) {
                var comma = MatchToken(SyntaxKind.CommaToken);
                parameters.Add(comma);
            }
            else {
                parseNextParameter = false;
            }
        }
        
        return new SeparatedSyntaxList<FunctionParameter>(parameters.ToImmutable());    
    }

    private Expression ParseNumberLiteral() {
        var numberToken = MatchToken(SyntaxKind.NumberToken);
        return new LiteralExpression(numberToken);
    }

    private Expression ParseStringLiteral() {
        var stringToken = MatchToken(SyntaxKind.StringToken);
        return new LiteralExpression(stringToken);
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
        return new SyntaxToken(kind, Current.Position, "");
    }

    private readonly Stack<ElseClause?> orderedClauses = new();
    private readonly ImmutableArray<SyntaxToken> tokens;
    private readonly SourceText source;

    private int position;
};