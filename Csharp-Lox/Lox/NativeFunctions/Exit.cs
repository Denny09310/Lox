namespace Lox.NativeFunctions;

internal class Exit : ICallable
{
    public int Arity { get; }

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        Lox.Exit(1);

        return null;
    }
}