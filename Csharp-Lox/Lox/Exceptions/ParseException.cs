namespace Lox;

public class ParseException : LoxException
{
    public ParseException(string message) : base(message)
    {
    }

    public ParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ParseException() : base()
    {
    }
}