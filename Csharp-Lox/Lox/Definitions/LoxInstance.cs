namespace Lox;

internal class LoxInstance(LoxClass @class)
{
    private readonly Dictionary<string, object?> _fields = [];

    public object? Get(Token name)
    {
        if (_fields.TryGetValue(name.lexeme, out object? value))
        {
            return value;
        }

        LoxFunction? method = @class.FindMethod(name.lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
    }

    public void Set(Token name, object? value)
    {
        if (!_fields.TryAdd(name.lexeme, value))
        {
            _fields[name.lexeme] = value;
        }
    }

    public override string ToString()
    {
        return @class.Name + " instance.";
    }
}