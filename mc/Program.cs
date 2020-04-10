using System;
using System.Collections.Generic;
using System.Linq;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk
{
    internal static class Program
    {
        private static void Main()
        {
            bool showTree = false;
            var variables = new Dictionary<VariableSymbol, object>();

            while (true)
            {
                Console.Write("> ");

                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return;

                if (line == "#showtree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees" : "Not showing parse trees");
                    continue;
                }
                else if (line == "#cls")
                {
                    Console.Clear();
                    continue;
                }
                var syntaxTree = SyntaxTree.Parse(line);
                var compilation = new Compilation(syntaxTree);
                var evaluationResult = compilation.Evaluate(variables);
                var diagnostics = evaluationResult.Diagnostics;
                var color = Console.ForegroundColor;
                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ForegroundColor = color;
                }
                if (!diagnostics.Any())
                {
                    var result = evaluationResult.Value;
                    Console.WriteLine(result);
                }
                else
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        //var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = line.Substring(diagnostic.Span.End);

                        Console.Write("    ");
                        Console.Write(suffix);

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        //Console.Write(error);
                        Console.ResetColor();
                        Console.Write(suffix);
                        Console.WriteLine();
                    }
                    Console.ForegroundColor = color;
                }
            }
        }
    }
}
