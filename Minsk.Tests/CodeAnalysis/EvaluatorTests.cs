using System.Collections.Generic;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Syntax;
using Xunit;

namespace Minsk.Tests.CodeAnalysis
{
    public class EvaluatorTests
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        [InlineData("1 + 2", 3)]
        [InlineData("3 - 2", 1)]
        [InlineData("3 * 2", 6)]
        [InlineData("4 / 2", 2)]
        [InlineData("(10)", 10)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("-1 == -1", true)]
        [InlineData("2 != 2", false)]
        [InlineData("true == false", false)]
        [InlineData("true == true", true)]
        [InlineData("true != true", false)]
        [InlineData("true && false", false)]
        [InlineData("true || false", true)]
        [InlineData("{var a=10 {a=10*a } }", 100)]
        public void Evaluate_Performs_CorrectEvaluation(string text, object expectedResult)
        {
            var expression = SyntaxTree.Parse(text);
            Compilation c = new Compilation(expression);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = c.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedResult, result.Value);
        }
    }
}