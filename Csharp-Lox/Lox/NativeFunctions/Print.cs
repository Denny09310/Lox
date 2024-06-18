namespace Lox.NativeFunctions;

internal class Print : ICallable
{
    public int Arity { get; } = 1;

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        Console.WriteLine(Stringify(arguments[0]));
        return null;
    }

    private static string Stringify(object? obj)
    {
        if (obj == null) return "nil";

        if (obj is double)
        {
            string? text = obj.ToString();
            if (!string.IsNullOrEmpty(text) && text.EndsWith(".0"))
            {
                text = text[0..(text.Length - 2)];
            }
            return text ?? string.Empty;
        }

        return obj.ToString() ?? string.Empty;
    }
}