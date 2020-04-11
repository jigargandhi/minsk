using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis
{
    public class Compilation
    {
        public Compilation(SyntaxTree syntax)
        {
            Syntax = syntax;
        }

        public SyntaxTree Syntax { get; }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var binder = new Binder(variables);
            var boundExpression = binder.BindExpression(Syntax.Root);
            var evaluator = new Evaluator(boundExpression, variables);
            var value = evaluator.Evaluate();
            var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }

            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}