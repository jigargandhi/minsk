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
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreaterOrEqualsToken:
                case SyntaxKind.LessToken:
                case SyntaxKind.GreaterToken:
                    return 3;
                case SyntaxKind.AmpersandAmpersandToken:
                case SyntaxKind.AmpersandToken:
                    return 2;
                case SyntaxKind.PipePipeToken:
                case SyntaxKind.PipeToken:
                case SyntaxKind.HatToken:
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
                case SyntaxKind.TildeToken:
                    return 6;
                default:
                    return 0;
            }
        }

        internal static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "for":
                    return SyntaxKind.ForKeyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "let":
                    return SyntaxKind.LetKeyword;
                case "to":
                    return SyntaxKind.ToKeyword;
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "var":
                    return SyntaxKind.VarKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;
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
                case SyntaxKind.AmpersandToken: return "&";
                case SyntaxKind.TildeToken: return "~";
                case SyntaxKind.HatToken: return "^";
                case SyntaxKind.PipePipeToken: return "||";
                case SyntaxKind.PipeToken: return "|";
                case SyntaxKind.EqualsEqualsToken: return "==";
                case SyntaxKind.EqualsToken: return "=";
                case SyntaxKind.BangEqualsToken: return "!=";
                case SyntaxKind.LessToken: return "<";
                case SyntaxKind.LessOrEqualsToken: return "<=";
                case SyntaxKind.GreaterToken: return ">";
                case SyntaxKind.GreaterOrEqualsToken: return ">=";
                case SyntaxKind.TrueKeyword: return "true";
                case SyntaxKind.FalseKeyword: return "false";
                case SyntaxKind.LetKeyword: return "let";
                case SyntaxKind.VarKeyword: return "var";
                case SyntaxKind.BangToken: return "!";
                case SyntaxKind.OpenBraceToken: return "{";
                case SyntaxKind.CloseBraceToken: return "}";
                case SyntaxKind.IfKeyword: return "if";
                case SyntaxKind.ElseKeyword: return "else";
                case SyntaxKind.WhileKeyword: return "while";
                case SyntaxKind.ForKeyword: return "for";
                case SyntaxKind.ToKeyword: return "to";
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