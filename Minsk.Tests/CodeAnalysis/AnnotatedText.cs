using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Minsk.CodeAnalysis.Text;

namespace Minsk.Tests.CodeAnalysis
{
    internal sealed class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);
            var textBuilder = new StringBuilder();
            var markers = ImmutableArray.CreateBuilder<TextSpan>();
            var startStack = new Stack<int>();
            var position = 0;
            foreach (var c in text)
            {
                if (c == '[')
                {
                    startStack.Push(position);
                }
                else if (c == ']')
                {
                    if (startStack.Count == 0)
                        throw new ArgumentException("Too many ']' in text", nameof(text));
                    var start = startStack.Pop();
                    markers.Add(TextSpan.FromBounds(start, position));
                }
                else
                {
                    textBuilder.Append(c);
                    position++;

                }

            }

            if (startStack.Count != 0)
            {
                throw new ArgumentException("Too few ']' in text", nameof(text));
            }

            return new AnnotatedText(textBuilder.ToString(), markers.ToImmutable());
        }

        public static string Unindent(string text)
        {
            var lines = UnindentLines(text);

            return string.Join(Environment.NewLine, lines);
        }

        public static string[] UnindentLines(string text)
        {
            var lines = new List<string>();
            using (var stringReader = new StringReader(text))
            {
                string line = null;
                while ((line = stringReader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            var minIndent = int.MaxValue;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }
                var indentation = line.Length - line.TrimStart().Length;
                minIndent = Math.Min(minIndent, indentation);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0) continue;

                lines[i] = lines[i].Substring(minIndent);
            }
            while (lines.Count > 0 && lines[0].Length == 0)
            {
                lines.RemoveAt(0);
            }

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return lines.ToArray();
        }
    }
}