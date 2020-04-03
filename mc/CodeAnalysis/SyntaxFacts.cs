namespace Minsk.CodeAnalysis
{
    internal static class SyntaxFacts
    {        
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MultiplyToken:
                case SyntaxKind.DivideToken:
                    return 2;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}