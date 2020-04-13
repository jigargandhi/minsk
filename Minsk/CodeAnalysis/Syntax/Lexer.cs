using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly SourceText _text;
        private int _position;

        private int _start;

        private SyntaxKind _kind;

        private object _value;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        public Lexer(SourceText text)
        {
            _text = text;

        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _text.Length)
            {
                return '\0';
            }
            else
            {
                return _text[index];
            }
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _position++;
                    break;
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _position++;
                    break;
                case '*':
                    _kind = SyntaxKind.MultiplyToken;
                    _position++;
                    break;
                case '/':
                    _kind = SyntaxKind.DivideToken;
                    _position++;
                    break;
                case '(':
                    _kind = SyntaxKind.OpenParenthesisToken;
                    _position++;
                    break;
                case ')':
                    _kind = SyntaxKind.CloseParenthesisToken;
                    _position++;
                    break;
                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    _position++;
                    break;
                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    _position++;
                    break;
                case '!':
                    _position++;
                    if (Current == '=')
                    {
                        _kind = SyntaxKind.BangEqualsToken;
                        _position++;
                    }
                    else
                    {
                        _kind = SyntaxKind.BangToken;
                    }
                    break;

                case '&':
                    if (Lookahead == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position += 2;
                    }
                    break;
                case '|':
                    if (Lookahead == '|')
                    {
                        _kind = SyntaxKind.PipePipeToken;
                        _position += 2;
                    }
                    break;
                case '=':
                    _position++;
                    if (Current == '=')
                    {
                        _kind = SyntaxKind.EqualsEqualsToken;
                        _position++;
                    }
                    else
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadNumberToken();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    ReadWhitespace();
                    break;
                default:
                    {
                        if (char.IsLetter(Current))
                        {
                            ReadIdentifierOrKeyword();
                        }
                        else if (char.IsWhiteSpace(Current))
                        {
                            ReadWhitespace();
                        }
                        else
                        {
                            _diagnostics.ReportBadCharacter(_position, Current);
                            _position++;
                        }
                    }
                    break;
            }

            var text = SyntaxFacts.GetText(_kind);
            if (text == null)
            {
                text = _text.ToString(_start, _position - _start);
            }
            return new SyntaxToken(_kind, _start, text, _value);
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
            {
                Next();
            }
            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
            switch (_kind)
            {               
                case SyntaxKind.LetKeyword:
                case SyntaxKind.VarKeyword:
                case SyntaxKind.Identifier:
                    _value = text;
                    break;
                case SyntaxKind.TrueKeyword:
                    _value = true;
                    break;
                case SyntaxKind.FalseKeyword:
                    _value = false;
                    break;
                default:
                    throw new Exception("Unknown identifier");
            }
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(Current))
            {
                Next();
            }
            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
            {
                Next();
            }
            var length = _position - _start;
            var text = _text.ToString(_start, length);
            if (!int.TryParse(text, out int value))
            {
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, typeof(int));
            }
            _kind = SyntaxKind.LiteralToken;
            _value = value;
        }
    }
}