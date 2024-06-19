namespace Lox;

internal class ClassDefinition(string name, ClassDefinition? superclass, Dictionary<string, FunctionDefinition> methods) : ICallable
{
    public int Arity
    {
        get
        {
            FunctionDefinition? initializer = FindMethod("constructor");
            return (initializer == null) ? 0 : initializer.Arity;
        }
    }

    public string Name { get; } = name;
    public ClassDefinition? Superclass { get; } = superclass;

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        InstanceDefinition instance = new(this);
        FunctionDefinition? initializer = FindMethod("constructor");
        initializer?.Bind(instance).Call(interpreter, arguments);

        return instance;
    }

    public FunctionDefinition? FindMethod(string name)
    {
        if (methods.TryGetValue(name, out FunctionDefinition? value))
        {
            return value;
        }

        if (Superclass != null)
        {
            return Superclass.FindMethod(name);
        }

        return null;
    }

    public override string ToString()
    {
        return Name;
    }
}