using System;
using System.Collections.Generic;
using System.Linq;
using Minsk.CodeAnalysis.Syntax;
using Xunit;

namespace Minsk.Tests.CodeAnalysis.Syntax
{
    public class LexerTest
    {
        [Fact]
        public void Lexer_Tests_AllTokens()
        {
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                                              .Cast<SyntaxKind>()
                                              .Where(t => t.ToString().EndsWith("Keyword") ||
                                                    t.ToString().EndsWith("Token"))
                                              .ToList();
            var testedTokenKinds = GetTokens().Concat(GetSeparators()).Select(t => t.kind);
            var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.BadToken);
            untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
            untestedTokenKinds.ExceptWith(testedTokenKinds);

            Assert.Empty(untestedTokenKinds);
        }

        [Theory()]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
        {
            var text = t1Text + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(t1Kind, tokens[0].Kind);
            Assert.Equal(t2Kind, tokens[1].Kind);
            Assert.Equal(t1Text, tokens[0].Text);
            Assert.Equal(t2Text, tokens[1].Text);
        }

        [Theory()]
        [MemberData(nameof(GetTokenPairsWithSeparatorsData))]
        public void Lexer_Lexes_TokenPairsWithSeparators(SyntaxKind t1Kind, string t1Text, SyntaxKind separator, string separatorText, SyntaxKind t2Kind, string t2Text)
        {
            var text = t1Text + separatorText + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(t1Kind, tokens[0].Kind);
            Assert.Equal(t2Kind, tokens[2].Kind);
            Assert.Equal(t1Text, tokens[0].Text);
            Assert.Equal(t2Text, tokens[2].Text);
            Assert.Equal(separator, tokens[1].Kind);
            Assert.Equal(separatorText, tokens[1].Text);
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var t in GetTokens().Concat(GetSeparators()))
            {
                yield return new object[] { t.kind, t.text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var t in GetTokenPairs())
            {
                yield return new object[] { t.t1kind, t.t1text, t.t2kind, t.t2text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorsData()
        {
            foreach (var t in GetTokenPairsWithSeparators())
            {
                yield return new object[] { t.t1kind, t.t1text, t.separator, t.separatorText, t.t2kind, t.t2text };
            }
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            var fixedTokens = Enum.GetValues(typeof(SyntaxKind))
                                  .Cast<SyntaxKind>()
                                  .Select(k => (kind: k, text: SyntaxFacts.GetText(k)))
                                  .Where(p => p.text != null);

            var dyanmicTokens = new[]{
                (SyntaxKind.Identifier,"a"),
                (SyntaxKind.Identifier,"abcd"),

                (SyntaxKind.LiteralToken, "1"),
                (SyntaxKind.LiteralToken, "123"),
            };

            return fixedTokens.Concat(dyanmicTokens);
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[]{
                (SyntaxKind.WhitespaceToken," "),
                (SyntaxKind.WhitespaceToken,"   "),
                (SyntaxKind.WhitespaceToken,"\r"),
                (SyntaxKind.WhitespaceToken,"\n"),
                (SyntaxKind.WhitespaceToken,"\r\n"),
            };
        }

        private static IEnumerable<(SyntaxKind t1kind, string t1text, SyntaxKind t2kind, string t2text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (!RequiresSeparator(t1.kind, t2.kind))
                        yield return (t1.kind, t1.text, t2.kind, t2.text);
                }
            }
        }
        private static IEnumerable<(SyntaxKind t1kind, string t1text, SyntaxKind separator, string separatorText, SyntaxKind t2kind, string t2text)> GetTokenPairsWithSeparators()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (RequiresSeparator(t1.kind, t2.kind))
                    {
                        foreach (var s in GetSeparators())
                        {
                            yield return (t1.kind, t1.text, s.kind, s.text, t2.kind, t2.text);
                        }
                    }
                }
            }
        }

        private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
        {
            var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword", StringComparison.CurrentCultureIgnoreCase);
            var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword", StringComparison.CurrentCultureIgnoreCase);
            if (t1Kind == SyntaxKind.Identifier || t2Kind == SyntaxKind.Identifier)
                return true;

            if (t1IsKeyword && t2IsKeyword)
                return true;


            if (t1IsKeyword && t2Kind == SyntaxKind.Identifier)
                return true;

            if (t2IsKeyword && t1Kind == SyntaxKind.Identifier)
                return true;

            if (t1Kind == SyntaxKind.LiteralToken && t2Kind == SyntaxKind.LiteralToken)
                return true;

            if (t1Kind == SyntaxKind.BangToken && (t2Kind == SyntaxKind.EqualsToken || t2Kind == SyntaxKind.EqualsEqualsToken))
                return true;

            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsEqualsToken)
                return true;
            
            if (t1Kind == SyntaxKind.AmpersandToken && t2Kind == SyntaxKind.AmpersandToken)
                return true;

             if (t1Kind == SyntaxKind.AmpersandToken && t2Kind == SyntaxKind.AmpersandAmpersandToken)
                return true;
            
            if (t1Kind == SyntaxKind.AmpersandAmpersandToken && t2Kind == SyntaxKind.AmpersandToken)
                return true;
            
            if (t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipePipeToken)
                return true;
            
            if (t1Kind == SyntaxKind.PipePipeToken && t2Kind == SyntaxKind.PipeToken)
                return true;
                
            if (t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipeToken)
                return true;

            return false;
        }
    }
}