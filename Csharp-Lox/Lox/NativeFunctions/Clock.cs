namespace Lox.NativeFunctions;

internal class Clock : ICallable
{
    public int Arity { get; }

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        return DateTime.Now.Second + DateTime.Now.Millisecond / 1000.0;
    }
}