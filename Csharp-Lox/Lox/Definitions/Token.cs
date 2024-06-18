namespace Lox;

public class Token
{
    public readonly string lexeme;
    public readonly int line;
    public readonly object literal;
    public readonly TokenType type;

    public Token(TokenType type, string lexeme, object literal, int line)
    {
        this.type = type;
        this.lexeme = lexeme;
        this.literal = literal;
        this.line = line;
    }

    public override string ToString()
    {
        return type + " " + lexeme + " " + literal;
    }
}