namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        LogicalAnd,
        LogicalOr,

        Equals,

        NotEquals,
        Less,
        Greater,
        LessOrEquals,
        GreaterOrEquals,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor
    }

}