using System.Collections.Generic;
using Minsk.CodeAnalysis.Syntax;
using Xunit;

namespace Minsk.Tests.CodeAnalysis.Syntax
{
    public class ParserTest
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parse_BinaryExpression_HonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
        {
            var op1Precedence = op1.GetBinaryOperatorPrecedence();
            var op2Precedence = op2.GetBinaryOperatorPrecedence();
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = ParseExpression(text);
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

        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parse_UnaryExpression_HonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
        {
            var unaryPrecedence = unaryKind.GetUnaryOperatorPrecedence();
            var binaryPrecedence = binaryKind.GetBinaryOperatorPrecedence();
            var unaryText = SyntaxFacts.GetText(unaryKind);
            var binaryText = SyntaxFacts.GetText(binaryKind);
            var text = $"{unaryText} a {binaryText} b";
            var expression = ParseExpression(text);
            if (unaryPrecedence >= binaryPrecedence)
            {

                //     binary
                //     /   \ 
                //  unary   b
                //    |
                //    a
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);

                    enumerator.AssertNode(SyntaxKind.UnaryExpression);
                    enumerator.AssertToken(unaryKind, unaryText);

                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "a");

                    enumerator.AssertToken(binaryKind, binaryText);

                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "b");

                }
            }
            else
            {
                //  unary
                //    |  
                //  binary
                //  /   \
                // a     b
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.UnaryExpression);
                    enumerator.AssertToken(unaryKind, unaryText);

                    enumerator.AssertNode(SyntaxKind.BinaryExpression);

                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "a");

                    enumerator.AssertToken(binaryKind, binaryText);

                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.Identifier, "b");
                }
            }
        }

        private static ExpressionSyntax ParseExpression(string text)
        {
            var tree = SyntaxTree.Parse(text);
            var root = tree.Root;
            var statement = root.Statement;
            return Assert.IsType<ExpressionStatementSyntax>(statement).Expression;
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

        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach (var unary in SyntaxFacts.GetUnaryOperatorKinds())
            {
                foreach (var binary in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { unary, binary };
                }
            }
        }
    }
}