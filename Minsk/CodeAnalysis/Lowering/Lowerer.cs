using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;
        private Lowerer()
        {

        }
        
        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement(statement);
            return Flatten(result);
        }

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }
        private LabelSymbol GenerateLabel()
        {
            var name = $"Label{++_labelCount}";
            return new LabelSymbol(name);
        }
        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = GenerateLabel();
                var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoFalse, 
                    node.ThenStatement, 
                    endLabelStatement));
                return RewriteStatement(result);
            }
            else
            {
                var endLabel = GenerateLabel();
                var elseLabel = GenerateLabel();
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var elseLabelStatement = new BoundLabelStatement(elseLabel);
                var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, true);
                var gotoEndStatement = new BoundGotoStatement(endLabel);
                var result = new BoundBlockStatement(
                    ImmutableArray.Create<BoundStatement>(
                        gotoFalse,
                        node.ThenStatement,
                        gotoEndStatement,
                        elseLabelStatement,
                        node.ElseStatement,
                        endLabelStatement));
                return RewriteStatement(result);

            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            // goto check:
            // continue:
            // <body>
            // check:
            // gotoTrue condition continue:
            // end:
            // -------------------------------------------------------
            // later try out 
            //- begin
            //- gotoFalse <condition> end
            //- <body>
            //-  goto begin
            //- end

            var endLabel = GenerateLabel();
            var checkLabel = GenerateLabel();
            var continueLabel = GenerateLabel();

            var gotoCheck = new BoundGotoStatement(checkLabel);
            var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition);

            var continueLabelStatement = new BoundLabelStatement(continueLabel);
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var result = new BoundBlockStatement(
                    ImmutableArray.Create<BoundStatement>(
                        gotoCheck,
                        continueLabelStatement,
                        node.Body,
                        checkLabelStatement,
                        gotoTrue,
                        endLabelStatement));
            return RewriteStatement(result);

        }
        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            // for <var> = <lower> to <upper>
            //  <body>
            // is rewritten to 
            //  {
            //    var <Var> = <lower>
            //  while ()
            //  
            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            var variableExpression = new BoundVariableExpression(node.Variable);

            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
                node.UpperBound);

            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(
                            SyntaxKind.PlusToken,
                            typeof(int),
                            typeof(int)),
                        new BoundLiteralExpression(1)
                    )
                )
            );

            var whileBlock = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));

            var whileStatement = new BoundWhileStatement(condition, whileBlock);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));

            return RewriteStatement(result);
        }
    }
}