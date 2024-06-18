namespace Lox;

public class Environment
{
    private readonly Environment? _enclosing;
    private readonly Dictionary<string, object?> _values = [];

    public Environment()
    { }

    public Environment(Environment enclosing)
    {
        _enclosing = enclosing;
    }

    public Environment? Enclosing => _enclosing;

    public Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            if (environment.Enclosing == null) return environment;
            environment = environment.Enclosing;
        }

        return environment;
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeException(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)._values.Add(name.Lexeme, value);
    }

    //TODO: Handle trying to define a variable that is already defined.
    public void Define(string name, object? value)
    {
        _values.Add(name, value);
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        if (Enclosing != null) return Enclosing.Get(name);

        throw new RuntimeException(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)._values[name];
    }
}