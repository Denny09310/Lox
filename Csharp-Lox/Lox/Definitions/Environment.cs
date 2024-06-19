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
    private Dictionary<string, object?> Values => _values;

    public Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.Enclosing!;
        }

        return environment;
    }

    public void Assign(Token name, object? value)
    {
        if (Values.ContainsKey(name.Lexeme))
        {
            Values[name.Lexeme] = value;
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
        Environment ancestor = Ancestor(distance);
        if (ancestor.Values.ContainsKey(name.Lexeme))
        {
            ancestor.Values[name.Lexeme] = value;
        }
        else
        {
            throw new RuntimeException(name, "Undefined variable '" + name.Lexeme + "' at distance " + distance + ".");
        }
    }

    //TODO: Handle trying to define a variable that is already defined.
    public void Define(string name, object? value)
    {
        Values.Add(name, value);
    }

    public object? Get(Token name)
    {
        if (Values.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        if (Enclosing != null) return Enclosing.Get(name);

        throw new RuntimeException(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public object? GetAt(int distance, string name)
    {
        Environment ancestor = Ancestor(distance);
        if (ancestor.Values.TryGetValue(name, out object? value))
        {
            return value;
        }
        throw new RuntimeException(new Token(SyntaxKind.IDENTIFIER, name, null, 0),
            "Undefined variable '" + name + "' at distance " + distance + ".");
    }
}