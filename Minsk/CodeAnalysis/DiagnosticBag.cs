using System;
using System.Collections;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Syntax;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan textSpan, string text, Type type)
        {
            var message = $"The number {text} isn't a valid {type}";
            Report(textSpan, message);
        }

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        internal void ReportBadCharacter(int position, char character)
        {
            var span = new TextSpan(position, 1);
            var message = $"Bad character input '{character}'";
            Report(span, message);
        }

        internal void ReportUnexpectedToken(TextSpan span, SyntaxKind actual, SyntaxKind expected)
        {
            var message = $"Unexpected token '{actual}' expected {expected}";
            Report(span, message);
        }

        internal void ReportUndefinedUnaryOperator(TextSpan span, string text, Type type)
        {
            var message = $"Unary operator {text} is not defined for type {type}";
            Report(span, message);
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string text, Type leftType, Type rightType)
        {
            var message = $"Binary operator {text} is not defined for type {leftType} and {rightType}.";
            Report(span, message);
        }

        internal void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variable {name} doesn't exist";
            Report(span, message);
        }

        internal void ReportCannotConvert(TextSpan span, Type fromType, Type toType)
        {
            var message = $"Cannot convert type from '{fromType}' to '{toType}'";
            Report(span, message);
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is already declared";
            Report(span, message);
        }

        internal void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is readonly and cannot be assigned to";
            Report(span, message);
        }
    }
}