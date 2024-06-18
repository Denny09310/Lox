namespace Lox;

internal interface ICallable
{
    public int Arity { get; }

    object? Call(Interpreter interpreter, IList<object?> arguments);
}