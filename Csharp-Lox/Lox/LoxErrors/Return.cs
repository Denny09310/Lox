namespace Lox;

internal class Return : LoxError
{
    public Return() : base()
    {
    }

    public Return(string message) : base(message)
    {
    }

    public Return(string message, Exception innerException) : base(message, innerException)
    {
    }

    public Return(object? value)
    {
        Value = value;
    }

    public object? Value { get; set; }
}