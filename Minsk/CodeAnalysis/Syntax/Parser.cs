using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private readonly SourceText _text;
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private int _position;
        public Parser(SourceText text)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.BadToken &&
                token.Kind != SyntaxKind.WhitespaceToken)
                {
                    tokens.Add(token);
                }

            } while (token.Kind != SyntaxKind.EndOfFileToken);
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
            _text = text;
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
            {
                return _tokens[_tokens.Length - 1];
            }

            return _tokens[index];

        }

        private SyntaxToken Current => Peek(0);

        /// <summary> 
        /// Returns the current token and increments the parser to next token
        /// </summary>
        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;

        }

        public DiagnosticBag Diagnostics => _diagnostics;

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var statement = ParseStatement();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(statement, endOfFileToken);
        }

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.VarKeyword:
                case SyntaxKind.LetKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private StatementSyntax ParseBlockStatement()
        {
            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            var statementBuilder = ImmutableArray.CreateBuilder<StatementSyntax>();
            while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var statement = ParseStatement();
                statementBuilder.Add(statement);
            }
            var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
            return new BlockStatementSyntax(openBraceToken, statementBuilder.ToImmutable(), closeBraceToken);
        }      

        private StatementSyntax ParseVariableDeclaration()
        {
            var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.Identifier);
            var equalsToken = MatchToken(SyntaxKind.EqualsToken);
            var expression = ParseExpression();
            return new VariableDeclarationSyntax(keyword, identifier, equalsToken, expression);
        }
        
        private StatementSyntax ParseIfStatement()
        {
            var keyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatementSyntax(keyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if(Current.Kind!=SyntaxKind.ElseKeyword)
                return null;
            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);
        }
                
        private StatementSyntax ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementSyntax(keyword, condition, body);
        }


        private StatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(expression);
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }
                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            return left;
        }


        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesizedExpression();
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteralExpression();
                case SyntaxKind.LiteralToken:
                    return ParseNumberLiteral();
                case SyntaxKind.Identifier:
                default:
                    return ParseNameExpression();

            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteralExpression()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTrue);
        }
        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.LiteralToken);
            return new LiteralExpressionSyntax(numberToken);
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.Identifier);
            return new NameExpressionSyntax(identifierToken);
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpression();
        }
        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.Identifier &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
                // right = ParseAssignmentExpression
            }
            return ParseBinaryExpression();
        }
    }
}