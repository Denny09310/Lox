namespace Lox;

internal class LoxInstance(LoxClass @class)
{
    private readonly Dictionary<string, object?> _fields = [];

    public object? Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        LoxFunction? method = @class.FindMethod(name.Lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeException(name, "Undefined property '" + name.Lexeme + "'.");
    }

    public void Set(Token name, object? value)
    {
        if (!_fields.TryAdd(name.Lexeme, value))
        {
            _fields[name.Lexeme] = value;
        }
    }

    public override string ToString()
    {
        return @class.Name + " instance.";
    }
}