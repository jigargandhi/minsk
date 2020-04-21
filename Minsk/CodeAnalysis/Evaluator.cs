using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Binding;

namespace Minsk.CodeAnalysis
{
    class Evaluator
    {
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue = null;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            this._root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            var labelToIndexes = new Dictionary<LabelSymbol, int>();
            for (var i = 0; i < _root.Statements.Length; i++)
            {

                if (_root.Statements[i] is BoundLabelStatement l)
                    labelToIndexes.Add(l.Label, i + 1);
            }

            var index = 0;
            while (index < _root.Statements.Length)
            {
                var s = _root.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                        index++;
                        break;

                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var conditionalGotoStatement = (BoundConditionalGotoStatement)s;
                        var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);
                        if ((condition && !conditionalGotoStatement.JumpIfFalse)
                        || (!condition && conditionalGotoStatement.JumpIfFalse))
                        {
                            index = labelToIndexes[conditionalGotoStatement.Label];
                        }
                        else
                        {
                            index++;
                        }

                        break;
                    case BoundNodeKind.GotoStatement:
                        index = labelToIndexes[((BoundGotoStatement)s).Label];
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;

                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }
        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateBoundAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression)node);
                default:
                    throw new Exception($"unexpected node {node.Kind}");
            }

        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression boundLiteralExpression)
        {
            return boundLiteralExpression.Value;
        }
        private object EvaluateBoundAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            _variables[a.Variable] = value;
            return value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            return _variables[v.Variable];
        }
        private object EvaluateBinaryExpression(BoundBinaryExpression bt)
        {
            var left = EvaluateExpression(bt.Left);
            var right = EvaluateExpression(bt.Right);
            switch (bt.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (int)left + (int)right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    return (int)left / (int)right;

                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (bt.Left.Type == typeof(int))
                        return (int)left & (int)right;
                    else
                        return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (bt.Left.Type == typeof(int))
                        return (int)left | (int)right;
                    else
                        return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (bt.Left.Type == typeof(int))
                        return (int)left ^ (int)right;
                    else
                        return (bool)left ^ (bool)right;

                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.Less:
                    return (int)left < (int)right;
                case BoundBinaryOperatorKind.LessOrEquals:
                    return (int)left <= (int)right;
                case BoundBinaryOperatorKind.Greater:
                    return (int)left > (int)right;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return (int)left >= (int)right;
                default:
                    throw new Exception($"Expected a binary operator {bt.Op.Kind}");
            }
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression us)
        {
            var operand = EvaluateExpression(us.Operand);
            switch (us.Op.Kind)
            {
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operand;
                case BoundUnaryOperatorKind.Identity:
                    return (int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int)operand;
                default:
                    throw new Exception($"Unexpected unary operator {us.Kind}");
            }
        }
    }
}