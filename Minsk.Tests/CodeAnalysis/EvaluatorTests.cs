using System;
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
        [InlineData("3<4", true)]
        [InlineData("5<4", false)]
        [InlineData("3<=3", true)]
        [InlineData("4<=3", false)]
        [InlineData("4>3", true)]
        [InlineData("3>4", false)]
        [InlineData("4>=4", true)]
        [InlineData("4>=5", false)]
        [InlineData("-1 == -1", true)]
        [InlineData("2 != 2", false)]
        [InlineData("true == false", false)]
        [InlineData("true == true", true)]
        [InlineData("true != true", false)]
        [InlineData("true && false", false)]
        [InlineData("true || false", true)]
        [InlineData("{var a=10 {a=10*a } }", 100)]
        [InlineData("{var a = 0 if a==0 a=10 a}", 10)]
        [InlineData("{var a = 0 if a==5 a = 5 a=10 a}", 10)]
        [InlineData("{var a = 0 if a==20 a = 5 else a= 10 a}", 10)]
        [InlineData("{var a = 20 if a==20 a = 5 else a= 10 a}", 5)]
        [InlineData("{var a = 25 while a>20 a = a-1 a}", 20)]
        [InlineData("{var result = 0 for i = 1 to 10 { result = result + i } result}", 55)]
        public void Evaluate_Performs_CorrectEvaluation(string text, object expectedResult)
        {
            AssertValue(text, expectedResult);
        }

        [Fact]
        public void Evaulator_VariableDeclaration_Reports_Redeclaration()
        {
            var text = @"
            {
                var x = 0
                var y = 100
                {
                    var x = 10
                }
                var [x] = 5
            }
            ";

            var diagnostics = @"
                Variable 'x' is already declared
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaulator_Name_Reports_Undefined()
        {
            var text = @"[x] + 10";

            var diagnostics = @"
                Variable 'x' doesn't exist
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaulator_Assigned_Reports_Undefined()
        {
            var text = @"[x] = 10";

            var diagnostics = @"
                Variable 'x' doesn't exist
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaulator_Assigned_Reports_CannotAssign()
        {
            var text = @"
            {
                let x =10
                x [=] 0
            }";

            var diagnostics = @"
                Variable 'x' is readonly and cannot be assigned to
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaulator_Assigned_Reports_CannotConvert()
        {
            var text = @"
            {
                var x = 10
                x = [true]
            }";

            var diagnostics = @"
                Cannot convert type from 'System.Boolean' to 'System.Int32'
            ";
            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaulator_IfStatement_Reports_CannotConvert()
        {
            var text = @"
            {
                if [10]
                    var x = 10
            }";

            var diagnostics = @"
                Cannot convert type from 'System.Int32' to 'System.Boolean'
            ";
            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaulator_WhileStatement_Reports_CannotConvert()
        {
            var text = @"
            {
                while [10]
                    var x = 10
            }";

            var diagnostics = @"
                Cannot convert type from 'System.Int32' to 'System.Boolean'
            ";
            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaulator_ForStatement_Reports_CannotConvert_LowerBound()
        {
            var text = @"
            {
                var result = 0
                for i = [false] to 10
                    result = result + i
            }";

            var diagnostics = @"
                Cannot convert type from 'System.Boolean' to 'System.Int32'
            ";
            AssertDiagnostics(text, diagnostics);
        }
        [Fact]
        public void Evaulator_ForStatement_Reports_CannotConvert_UpperBound()
        {
            var text = @"
            {
                var result = 0
                for i = 0 to [true]
                    result = result + i
            }";

            var diagnostics = @"
               Cannot convert type from 'System.Boolean' to 'System.Int32'
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaulator_Unary_Reports_Undefined()
        {
            var text = @"[+]true";

            var diagnostics = @"
                Unary operator '+' is not defined for type 'System.Boolean'
            ";
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaulator_Binary_Reports_Undefined()
        {
            var text = @"10 [*] true";

            var diagnostics = @"
                Binary operator '*' is not defined for type 'System.Int32' and 'System.Boolean'
            ";
            AssertDiagnostics(text, diagnostics);
        }

        private void AssertValue(string text, object expectedResult)
        {
            var expression = SyntaxTree.Parse(text);
            Compilation c = new Compilation(expression);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = c.Evaluate(variables);

            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedResult, result.Value);
        }

        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var results = compilation.Evaluate(new Dictionary<VariableSymbol, object>());
            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
            {
                throw new Exception("ERROR: must mark as many spans as there are expected diagnostics");
            }
            Assert.Equal(expectedDiagnostics.Length, results.Diagnostics.Length);
            for (var i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = results.Diagnostics[i].Message;

                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = results.Diagnostics[i].Span;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Equal(expectedSpan, actualSpan);

            }
        }
    }
}