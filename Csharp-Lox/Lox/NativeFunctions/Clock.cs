namespace Lox.NativeFunctions;

internal class Clock : ICallable
{
    public int Arity
    { get { return 0; } set { } }

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        return DateTime.Now.Second + DateTime.Now.Millisecond / 1000.0;
    }
}