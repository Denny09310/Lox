namespace Lox;

public class RuntimeException : LoxErrorException
{
    public RuntimeException(string message) : base(message)
    {
    }

    public RuntimeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public RuntimeException(Token token, string message)
    {
        Token = token;
    }

    public RuntimeException() : base()
    {
    }

    public Token? Token { get; }
}