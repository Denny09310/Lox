namespace Lox;

public abstract class LoxException : Exception
{
    protected LoxException() : base()
    {
    }

    protected LoxException(string message) : base(message)
    {
    }

    protected LoxException(string message, Exception innerException) : base(message, innerException)
    {
    }
}