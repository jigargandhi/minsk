using System.Collections.Generic;
using Minsk.CodeAnalysis.Syntax;
using Xunit;

namespace Minsk.Tests.CodeAnalysis.Syntax
{
    public class ParserTest
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parse_ParseBinaryExpression_HonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
        {
            var op1Precedence = op1.GetBinaryOperatorPrecedence();
            var op2Precedence = op2.GetBinaryOperatorPrecedence();
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = SyntaxTree.Parse(text).Root;
            if (op1Precedence >= op2Precedence)
            {

                //     op2
                //    /   \
                //   op1   c
                //  /   \
                // a     b
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);

                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "a");
                    enumerator.AssertToken(op1, op1Text);

                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "b");
                    enumerator.AssertToken(op2, op2Text);

                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "c");
                }
            }
            else
            {
                //   op1 
                //  /   \
                // a     op2
                //      /   \
                //     b     c
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "a");
                    enumerator.AssertToken(op1, op1Text);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "b");
                    enumerator.AssertToken(op2, op2Text);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "c");
                }
            }
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperatorKinds())
            {
                foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { op1, op2 };
                }
            }
        }
    }
}