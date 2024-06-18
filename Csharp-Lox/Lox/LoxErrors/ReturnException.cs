namespace Lox;

internal class ReturnException : LoxErrorException
{
    public ReturnException() : base()
    {
    }

    public ReturnException(string message) : base(message)
    {
    }

    public ReturnException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ReturnException(object? value)
    {
        Value = value;
    }

    public object? Value { get; set; }
}