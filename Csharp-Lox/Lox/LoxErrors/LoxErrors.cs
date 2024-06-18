namespace Lox;

public abstract class LoxErrorException : Exception
{
    protected LoxErrorException() : base()
    {
    }

    protected LoxErrorException(string message) : base(message)
    {
    }

    protected LoxErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ParseErrorException : LoxErrorException
{
    public ParseErrorException(string message) : base(message)
    {
    }

    public ParseErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ParseErrorException() : base()
    {
    }
}