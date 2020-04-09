using System;
using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MultiplyToken:
                case SyntaxKind.DivideToken:
                    return 5;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                    return 3;
                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;
                case SyntaxKind.PipePipeToken:
                    return 1;
                default:
                    return 0;
            }
        }

        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                    return 6;
                default:
                    return 0;
            }
        }

        internal static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                default:
                    return SyntaxKind.Identifier;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken: return "+";
                case SyntaxKind.MinusToken: return "-";
                case SyntaxKind.MultiplyToken: return "*";
                case SyntaxKind.DivideToken: return "/";
                case SyntaxKind.OpenParenthesisToken: return "(";
                case SyntaxKind.CloseParenthesisToken: return ")";
                case SyntaxKind.AmpersandAmpersandToken: return "&&";
                case SyntaxKind.PipePipeToken: return "||";
                case SyntaxKind.EqualsEqualsToken: return "==";
                case SyntaxKind.EqualsToken: return "=";
                case SyntaxKind.BangEqualsToken: return "!=";
                case SyntaxKind.TrueKeyword: return "true";
                case SyntaxKind.FalseKeyword: return "false";
                case SyntaxKind.BangToken: return "!";
                default: return null;
            }
        }

        

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var values = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in values)
            {
                if (kind.GetBinaryOperatorPrecedence() > 0)
                {
                    yield return kind;
                }
            }
        }

        public static IEnumerable<object> GetUnaryOperatorKinds()
        {
            var values = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in values)
            {
                if (kind.GetUnaryOperatorPrecedence() > 0)
                {
                    yield return kind;
                }
            }
        }
    }
}