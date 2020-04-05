using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new List<string>();

        public IEnumerable<string> Diagnostics => _diagnostics;
        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected expression {syntax.Kind}");
            }
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {

            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.Add($"Unary operator {syntax.OperatorToken.Text} is not defined for type {boundOperand.Type}");
                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {

            var boundLeftOperand = BindExpression(syntax.Left);
            var boundRightOperand = BindExpression(syntax.Right);
            var boundBinaryOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeftOperand.Type, boundRightOperand.Type);

            if (boundBinaryOperator == null)
            {
                _diagnostics.Add($"Binary operator {syntax.OperatorToken.Text} is not defined for type {boundLeftOperand.Type} and {boundRightOperand.Type}.");
                return boundLeftOperand;
            }
            return new BoundBinaryExpression(boundLeftOperand, boundBinaryOperator, boundRightOperand);
        }
    }

}