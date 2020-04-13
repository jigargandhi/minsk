using System;
using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Text
{
    public sealed class SourceText
    {
        private readonly string _text;

        public ImmutableArray<TextLine> Lines { get; }

        public char this[int index] => _text[index];

        public int Length => _text.Length;
        public SourceText(string text)
        {
            Lines = ParseLines(this, text);
            _text = text;
        }
        public int GetLineIndex(int position)
        {
            var lower = 0;
            var upper = Lines.Length - 1;
            while (lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Start;
                if (start == position)
                {
                    return index;
                }

                if (position < start)
                {
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }
            }
            return lower - 1;
        }

        private ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();
            var lineStart = 0;
            var position = 0;
            while (position < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, position);
                if (lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(result, sourceText, lineStart, position, lineBreakWidth);
                    position += lineBreakWidth;
                    lineStart = position;

                    continue;
                }
            }
            if (position >= lineStart)
            {
                AddLine(result, sourceText, lineStart, position, 0);
            }
            return result.ToImmutable();
        }

        private void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int lineStart, int position, int lineBreakWidth)
        {
            var lineLength = position - lineStart;
            var lineLengthIncludingBreakWidth = lineLength + lineBreakWidth;
            var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingBreakWidth);
            result.Add(line);
        }

        private int GetLineBreakWidth(string text, int position)
        {
            var c = text[position];
            var lookahead = position + 1 >= text.Length ? '\0' : text[position + 1];
            if (c == '\r' && lookahead == '\n')
                return 2;

            if (c == '\r' || c == '\n')
                return 1;
            else
                return 0;
        }

        public static SourceText From(string text)
        {
            return new SourceText(text);
        }

        public override string ToString() => _text;

        public string ToString(int start, int length) => _text.Substring(start, length);

        public string ToString(TextSpan span) => ToString(span.Start, span.Length);
    }
}