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

        public EvaluationResult Evaluate()
        {
            var binder = new Binder();
            var boundExpression = binder.BindExpression(Syntax.Root);
            var evaluator = new Evaluator(boundExpression);
            var value = evaluator.Evaluate();
            var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics);
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }

            return new EvaluationResult(Enumerable.Empty<Diagnostic>(), value);
        }
    }
}