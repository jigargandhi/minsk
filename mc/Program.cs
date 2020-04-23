using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;
using Minsk.CodeAnalysis.Text;

namespace Minsk
{
    internal abstract class Repl
    {
        private readonly StringBuilder _textBuilder = new StringBuilder();
        public void Run()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (_textBuilder.Length == 0)
                {
                    Console.Write("» ");
                }
                else
                {
                    Console.Write("· ");
                }
                Console.ResetColor();
                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);


                if (_textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    else if (input.StartsWith("#"))
                    {
                        EvaluateMetaCommand(input);
                        continue;
                    }
                }
                _textBuilder.AppendLine(input);
                var text = _textBuilder.ToString();
                if (!IsCompletedSubmission(text))
                    continue;

                EvaluateSubmission(text);
                _textBuilder.Clear();
            }
        }

        protected virtual void EvaluateMetaCommand(string input)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Invalid command {input}");
            Console.ResetColor();
        }
        protected abstract bool IsCompletedSubmission(string text);

        protected abstract void EvaluateSubmission(string text);


    }

    internal sealed class MinskRepl : Repl
    {
        private Compilation _previous;
        
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();
        private bool _showTree = false;
        private bool _showProgram = false;

        protected override void EvaluateMetaCommand(string input)
        {
            if (input == "#showTree")
            {
                _showTree = !_showTree;
                Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
            }
            else if (input == "#showProgram")
            {
                _showProgram = !_showProgram;
                Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
            }
            else if (input == "#cls")
            {
                Console.Clear();
            }
            else if (input == "#reset")
            {
                _previous = null;
                _variables.Clear();
            }
            else
            {
                base.EvaluateMetaCommand(input);
            }
        }

        protected override bool IsCompletedSubmission(string text)
        {

            if (string.IsNullOrEmpty(text))
                return false;
            var syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Diagnostics.Any())
                return false;

            return true;
        }

        protected override void EvaluateSubmission(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = _previous == null ?
                              new Compilation(syntaxTree) :
                              _previous.ContinueWith(syntaxTree);

            var color = Console.ForegroundColor;
            if (_showTree)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                syntaxTree.Root.WriteTo(Console.Out);
                Console.ForegroundColor = color;
            }

            if (_showProgram)
            {
                compilation.EmitTree(Console.Out);
            }
            var evaluationResult = compilation.Evaluate(_variables);
            var diagnostics = evaluationResult.Diagnostics;
            if (!diagnostics.Any())
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var result = evaluationResult.Value;
                Console.WriteLine(result);
                Console.ResetColor();
                _previous = compilation;

            }
            else
            {
                foreach (var diagnostic in diagnostics)
                {
                    var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                    var lineNumber = lineIndex + 1;
                    var line = syntaxTree.Text.Lines[lineIndex];
                    var character = diagnostic.Span.Start - line.Start + 1;

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"({lineNumber}, {character}): ");
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();

                    var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                    var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                    var prefix = syntaxTree.Text.ToString(prefixSpan);
                    var error = syntaxTree.Text.ToString(diagnostic.Span);
                    var suffix = syntaxTree.Text.ToString(suffixSpan);

                    Console.Write("    ");
                    Console.Write(prefix);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(error);
                    Console.ResetColor();
                    Console.Write(suffix);
                    Console.WriteLine();
                }
                Console.ForegroundColor = color;

            }
        }

    }
    internal static class Program
    {
        public static void Main()
        {
            var repl = new MinskRepl();
            repl.Run();
        }
    }
}
