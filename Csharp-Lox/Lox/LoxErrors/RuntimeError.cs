namespace Lox;

public class RuntimeError : LoxError
{
    public RuntimeError(string message) : base(message)
    {
    }

    public RuntimeError(string message, Exception innerException) : base(message, innerException)
    {
    }

    public RuntimeError(Token token, string message)
    {
        Token = token;
    }

    public RuntimeError() : base()
    {
    }

    public Token? Token { get; }
}