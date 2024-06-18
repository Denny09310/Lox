namespace Lox;

internal class LoxFunction(Statement.Function declaration, Environment closure, bool isInitializer) : ICallable
{
    public int Arity { get => declaration.Parameters.Count; }

    public LoxFunction Bind(LoxInstance instance)
    {
        Environment environment = new(closure);
        environment.Define("this", instance);

        return new LoxFunction(declaration, environment, isInitializer);
    }

    public object? Call(Interpreter interpreter, IList<object?> arguments)
    {
        Environment environment = new(closure);

        for (int i = 0; i < declaration.Parameters.Count; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (ReturnException returnValue)
        {
            if (isInitializer) return closure.GetAt(0, "this");

            return returnValue.Value;
        }

        if (isInitializer) return closure.GetAt(0, "this");

        return null;
    }

    public override string ToString()
    {
        return "<fn " + declaration.Name.Lexeme + ">";
    }
}