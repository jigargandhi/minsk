namespace Minsk.CodeAnalysis
{
    public enum SyntaxKind
    {
        NumberToken,
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
        NumberExpression,
        ParenthesizedExpression
    }
}