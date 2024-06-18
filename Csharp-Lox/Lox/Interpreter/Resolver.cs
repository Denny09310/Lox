namespace Lox;

internal class Resolver(Interpreter interpreter) : Expression.IVisitor<object?>, Statement.IVisitor<object?>
{
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private ClassType _currentClass = ClassType.NONE;
    private FunctionType _currentFunction = FunctionType.NONE;

    #region "Private Helpers"

    public void Resolve(IEnumerable<Statement> statements)
    {
        foreach (Statement statement in statements)
        {
            Resolve(statement);
        }
    }

    private void BeginScope()
    {
        _scopes.Push([]);
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;

        Dictionary<string, bool> scope = _scopes.Peek();
        if (!scope.TryAdd(name.Lexeme, false))
        {
            Lox.Error(name, "Variable with this name already declared in this scope.");
        }
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;

        if (_scopes.Peek().ContainsKey(name.Lexeme))
        {
            _scopes.Peek()[name.Lexeme] = true;
        }
        else
        {
            Lox.Error(name, "Variable has not been declared.");
        }
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    private void Resolve(Statement stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expression expr)
    {
        expr.Accept(this);
    }

    private void ResolveFunction(Statement.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope();
        foreach (Token param in function.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    private void ResolveLocal(Expression expr, Token name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes.ToArray()[i].ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }

        //Not found locally.  Assume global.
    }

    #endregion "Private Helpers"

    #region "Statement Visitors"

    public object? Visit(Statement.Block statement)
    {
        BeginScope();
        Resolve(statement.Statements);
        EndScope();

        return null;
    }

    public object? Visit(Statement.Class statement)
    {
        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.CLASS;

        Declare(statement.Name);
        Define(statement.Name);

        if ((statement.Superclass != null) &&
            statement.Name.Lexeme.Equals(statement.Superclass.Name.Lexeme))
        {
            Lox.Error(statement.Superclass.Name, "A class cannot inherit from itself.");
        }

        if (statement.Superclass != null)
        {
            _currentClass = ClassType.SUBCLASS;
            Resolve(statement.Superclass);
        }

        if (statement.Superclass != null)
        {
            BeginScope();
            _scopes.Peek().Add("super", true);
        }

        BeginScope();
        _scopes.Peek().Add("this", true);

        foreach (Statement.Function method in statement.Methods)
        {
            FunctionType declaration = (method.Name.Lexeme.Equals("init")) ? FunctionType.INITIALIZER : FunctionType.METHOD;
            ResolveFunction(method, declaration);
        }

        EndScope();

        if (statement.Superclass != null) EndScope();

        _currentClass = enclosingClass;
        return null;
    }

    public object? Visit(Statement.Inline statement)
    {
        Resolve(statement.Body);

        return null;
    }

    public object? Visit(Statement.Function statement)
    {
        Declare(statement.Name);
        Define(statement.Name);

        ResolveFunction(statement, FunctionType.FUNCTION);

        return null;
    }

    public object? Visit(Statement.If statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.ThenBranch);
        if (statement.ElseBranch != null) Resolve(statement.ElseBranch);

        return null;
    }

    public object? Visit(Statement.Return statement)
    {
        if (_currentFunction == FunctionType.NONE)
        {
            Lox.Error(statement.Keyword, "Cannot return from top-level code.");
        }

        if (statement.Value != null)
        {
            if (_currentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(statement.Keyword, "Cannot return a value from an initializer.");
            }
            Resolve(statement.Value);
        }

        return null;
    }

    public object? Visit(Statement.Var statement)
    {
        Declare(statement.Name);
        if (statement.Initializer != null)
        {
            Resolve(statement.Initializer);
        }
        Define(statement.Name);

        return null;
    }

    public object? Visit(Statement.While statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.Body);

        return null;
    }

    #endregion "Statement Visitors"

    #region "Expression Visitors"

    public object? Visit(Expression.Assign expression)
    {
        Resolve(expression.Value);
        ResolveLocal(expression, expression.Name);

        return null;
    }

    public object? Visit(Expression.Binary expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);

        return null;
    }

    public object? Visit(Expression.Call expression)
    {
        Resolve(expression.Callee);

        foreach (Expression argument in expression.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? Visit(Expression.Get expression)
    {
        Resolve(expression.Target);

        return null;
    }

    public object? Visit(Expression.Grouping expression)
    {
        Resolve(expression.Expression);

        return null;
    }

    public object? Visit(Expression.Literal expression)
    {
        return null;
    }

    public object? Visit(Expression.Logical expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);

        return null;
    }

    public object? Visit(Expression.Set expression)
    {
        Resolve(expression.Value);
        Resolve(expression.Target);

        return null;
    }

    public object? Visit(Expression.Super expression)
    {
        if (_currentClass == ClassType.NONE)
        {
            Lox.Error(expression.Keyword, "Cannot use 'super' outside of a class.");
        }
        else if (_currentClass != ClassType.SUBCLASS)
        {
            Lox.Error(expression.Keyword, "Cannot use 'super' in a class with no superclass.");
        }

        ResolveLocal(expression, expression.Keyword);

        return null;
    }

    public object? Visit(Expression.This expression)
    {
        ResolveLocal(expression, expression.Keyword);

        return null;
    }

    public object? Visit(Expression.Prefix expression)
    {
        Resolve(expression.Right);

        return null;
    }

    public object? Visit(Expression.Postfix expression)
    {
        Resolve(expression.Left);

        return null;
    }

    public object? Visit(Expression.Conditional expression)
    {
        throw new NotImplementedException();
    }

    public object? Visit(Expression.Variable expression)
    {
        if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expression.Name.Lexeme, out bool value) && !value)
        {
            Lox.Error(expression.Name, "Cannot read local variable in its own initializer.");
        }

        ResolveLocal(expression, expression.Name);

        return null;
    }

    #endregion "Expression Visitors"
}