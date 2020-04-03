using System;

namespace Minsk.CodeAnalysis
{
    class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            this._root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax root)
        {
            if (root is LiteralExpressionSyntax numberToken)
            {
                return (int)numberToken.LiteralToken.Value;
            }

            if (root is BinaryExpressionSyntax bt)
            {
                var left = EvaluateExpression(bt.Left);
                var right = EvaluateExpression(bt.Right);
                if (bt.OperatorToken.Kind == SyntaxKind.PlusToken)
                {
                    return left + right;
                }
                else if (bt.OperatorToken.Kind == SyntaxKind.MinusToken)
                {
                    return left - right;
                }
                else if (bt.OperatorToken.Kind == SyntaxKind.MultiplyToken)
                {
                    return left * right;
                }
                else if (bt.OperatorToken.Kind == SyntaxKind.DivideToken)
                {
                    return left / right;
                }
                else
                {
                    throw new Exception($"Expected a binary operator {bt.OperatorToken.Kind}");
                }

            }

            if (root is ParenthesizedExpressionSyntax parenthesized)
            {
                return EvaluateExpression(parenthesized.Expression);
            }
            throw new Exception($"unexpected node {root.Kind}");

        }
    }
}