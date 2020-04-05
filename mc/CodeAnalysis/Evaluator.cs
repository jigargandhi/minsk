using System;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis
{
    class Evaluator
    {
        private readonly BoundExpression _root;

        public Evaluator(BoundExpression root)
        {
            this._root = root;
        }

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression root)
        {
            if (root is BoundLiteralExpression boundLiteralExpression)
            {
                return boundLiteralExpression.Value;
            }

            if (root is BoundBinaryExpression bt)
            {
                var left = EvaluateExpression(bt.Left);
                var right = EvaluateExpression(bt.Right);
                switch (bt.OperatorKind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return (int)left + (int)right;
                    case BoundBinaryOperatorKind.Subtraction:
                        return (int)left - (int)right;
                    case BoundBinaryOperatorKind.Multiplication:
                        return (int)left * (int)right;
                    case BoundBinaryOperatorKind.Division:
                        return (int)left / (int)right;
                    case BoundBinaryOperatorKind.LogicalAnd:
                        return (bool)left && (bool)right;
                    case BoundBinaryOperatorKind.LogicalOr:
                        return (bool)left || (bool)right;
                    default:
                        throw new Exception($"Expected a binary operator {bt.OperatorKind}");
                }

            }

            if (root is BoundUnaryExpression us)
            {
                var operand = EvaluateExpression(us.Operand);
                switch (us.OperatorKind)
                {
                    case BoundUnaryOperatorKind.Negation:
                        return -(int)operand;
                    case BoundUnaryOperatorKind.Identity:
                        return (int)operand;
                    case BoundUnaryOperatorKind.LogicalNegation:
                        return !(bool)operand;
                    default:
                        throw new Exception($"Unexpected unary operator {us.Kind}");
                }
            }

            // if (root is ParenthesizedExpressionSyntax parenthesized)
            // {
            //     return EvaluateExpression(parenthesized.Expression);
            // }
            throw new Exception($"unexpected node {root.Kind}");

        }
    }
}