namespace Lox;

public record Token(SyntaxKind Kind, string Lexeme, object? Literal, int Line)
{
    public override string ToString()
    {
        return Kind + " " + Lexeme + " " + Literal;
    }
}