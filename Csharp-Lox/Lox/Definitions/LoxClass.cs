namespace Lox;

internal class LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods) : ICallable
{
    public int Arity
    {
        get
        {
            LoxFunction? initializer = FindMethod("init");
            return (initializer == null) ? 0 : initializer.Arity;
        }
    }

    public string Name { get; } = name;
    public LoxClass Superclass { get; } = superclass;

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        LoxInstance instance = new(this);
        LoxFunction? initializer = FindMethod("init");
        initializer?.Bind(instance).Call(interpreter, arguments);

        return instance;
    }

    public LoxFunction? FindMethod(string name)
    {
        if (methods.TryGetValue(name, out LoxFunction? value))
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