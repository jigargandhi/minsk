using System.Collections.Immutable;
using System.Text;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;
using Minsk.CodeAnalysis.Text;

namespace Minsk
{
    internal static class Program
    {
        public static void Main()
        {
            var repl = new MinskRepl();
            repl.Run();
        }
    }
}
