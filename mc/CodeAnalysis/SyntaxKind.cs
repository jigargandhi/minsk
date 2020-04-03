namespace Minsk.CodeAnalysis
{
    public enum SyntaxKind
    {
        LiteralToken,
        WhitespaceToken,
        PlusToken,
        MinusToken,
        MultiplyToken,
        DivideToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,

        // Expression
        BinaryExpression,
        LiteralExpression,
        ParenthesizedExpression
    }
}