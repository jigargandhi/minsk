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

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(BoundExpression root)
        {
            if (root is BoundLiteralExpression numberToken)
            {
                return (int)numberToken.Value;
            }

            if (root is BoundBinaryExpression bt)
            {
                var left = EvaluateExpression(bt.Left);
                var right = EvaluateExpression(bt.Right);
                if (bt.OperatorKind == BoundBinaryOperatorKind.Addition)
                {
                    return left + right;
                }
                else if (bt.OperatorKind == BoundBinaryOperatorKind.Subtraction)
                {
                    return left - right;
                }
                else if (bt.OperatorKind == BoundBinaryOperatorKind.Multiplication)
                {
                    return left * right;
                }
                else if (bt.OperatorKind == BoundBinaryOperatorKind.Division)
                {
                    return left / right;
                }
                else
                {
                    throw new Exception($"Expected a binary operator {bt.OperatorKind}");
                }

            }

            if (root is BoundUnaryExpression us)
            {
                var operand = EvaluateExpression(us.Operand);
                switch (us.OperatorKind)
                {
                    case BoundUnaryOperatorKind.Negation:
                        return -operand;
                    case BoundUnaryOperatorKind.Identity:
                        return operand;
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