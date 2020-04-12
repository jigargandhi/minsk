using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Binding;

namespace Minsk.CodeAnalysis
{
    class Evaluator
    {
        private readonly BoundStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue = null;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            this._root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }
        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)node);
                    break;
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }

        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.Statements)
            {
                EvaluateStatement(statement);
            }
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
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
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
                default:
                    throw new Exception($"Unexpected unary operator {us.Kind}");
            }
        }
    }
}