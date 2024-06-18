namespace Lox;

internal class Resolver(Interpreter interpreter) : Expr.IVisitor<object?>, Statement.IVisitor<object?>
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
        if (!scope.TryAdd(name.lexeme, false))
        {
            Lox.Error(name, "Variable with this name already declared in this scope.");
        }
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;

        if (_scopes.Peek().ContainsKey(name.lexeme))
        {
            _scopes.Peek()[name.lexeme] = true;
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

    private void Resolve(Expr expr)
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

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes.ToArray()[i].ContainsKey(name.lexeme))
            {
                interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }

        //Not found locally.  Assume global.
    }

    #endregion "Private Helpers"

    #region "Statement Visitors"

    public object? Visit(Statement.Block _block)
    {
        BeginScope();
        Resolve(_block.Statements);
        EndScope();

        return null;
    }

    public object? Visit(Statement.Class _class)
    {
        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.CLASS;

        Declare(_class.Name);
        Define(_class.Name);

        if ((_class.Superclass != null) &&
            _class.Name.lexeme.Equals(_class.Superclass.Name.lexeme))
        {
            Lox.Error(_class.Superclass.Name, "A class cannot inherit from itself.");
        }

        if (_class.Superclass != null)
        {
            _currentClass = ClassType.SUBCLASS;
            Resolve(_class.Superclass);
        }

        if (_class.Superclass != null)
        {
            BeginScope();
            _scopes.Peek().Add("super", true);
        }

        BeginScope();
        _scopes.Peek().Add("this", true);

        foreach (Statement.Function method in _class.Methods)
        {
            FunctionType declaration = (method.Name.lexeme.Equals("init")) ? FunctionType.INITIALIZER : FunctionType.METHOD;
            ResolveFunction(method, declaration);
        }

        EndScope();

        if (_class.Superclass != null) EndScope();

        _currentClass = enclosingClass;
        return null;
    }

    public object? Visit(Statement.Expression _expression)
    {
        Resolve(_expression.Body);

        return null;
    }

    public object? Visit(Statement.Function _function)
    {
        Declare(_function.Name);
        Define(_function.Name);

        ResolveFunction(_function, FunctionType.FUNCTION);

        return null;
    }

    public object? Visit(Statement.If _if)
    {
        Resolve(_if.Condition);
        Resolve(_if.ThenBranch);
        if (_if.ElseBranch != null) Resolve(_if.ElseBranch);

        return null;
    }

    public object? Visit(Statement.Print _print)
    {
        Resolve(_print.Body);

        return null;
    }

    public object? Visit(Statement.Return _return)
    {
        if (_currentFunction == FunctionType.NONE)
        {
            Lox.Error(_return.Keyword, "Cannot return from top-level code.");
        }

        if (_return.Value != null)
        {
            if (_currentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(_return.Keyword, "Cannot return a value from an initializer.");
            }
            Resolve(_return.Value);
        }

        return null;
    }

    public object? Visit(Statement.Var @var)
    {
        Declare(@var.Name);
        if (@var.Initializer != null)
        {
            Resolve(@var.Initializer);
        }
        Define(@var.Name);

        return null;
    }

    public object? Visit(Statement.While _while)
    {
        Resolve(_while.Condition);
        Resolve(_while.Body);

        return null;
    }

    #endregion "Statement Visitors"

    #region "Expression Visitors"

    public object? Visit(Expr.Assign _assign)
    {
        Resolve(_assign.Value);
        ResolveLocal(_assign, _assign.Name);

        return null;
    }

    public object? Visit(Expr.Binary _binary)
    {
        Resolve(_binary.Left);
        Resolve(_binary.Right);

        return null;
    }

    public object? Visit(Expr.Call _call)
    {
        Resolve(_call.Callee);

        foreach (Expr argument in _call.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? Visit(Expr.Get _get)
    {
        Resolve(_get.Target);

        return null;
    }

    public object? Visit(Expr.Grouping _grouping)
    {
        Resolve(_grouping.Expression);

        return null;
    }

    public object? Visit(Expr.Literal _literal)
    {
        return null;
    }

    public object? Visit(Expr.Logical _logical)
    {
        Resolve(_logical.Left);
        Resolve(_logical.Right);

        return null;
    }

    public object? Visit(Expr.Set _set)
    {
        Resolve(_set.Value);
        Resolve(_set.Target);

        return null;
    }

    public object? Visit(Expr.Super _super)
    {
        if (_currentClass == ClassType.NONE)
        {
            Lox.Error(_super.Keyword, "Cannot use 'super' outside of a class.");
        }
        else if (_currentClass != ClassType.SUBCLASS)
        {
            Lox.Error(_super.Keyword, "Cannot use 'super' in a class with no superclass.");
        }

        ResolveLocal(_super, _super.Keyword);

        return null;
    }

    public object? Visit(Expr.This _this)
    {
        ResolveLocal(_this, _this.Keyword);

        return null;
    }

    public object? Visit(Expr.Prefix _prefix)
    {
        Resolve(_prefix.Right);

        return null;
    }

    public object? Visit(Expr.Postfix _postfix)
    {
        Resolve(_postfix.Left);

        return null;
    }

    public object? Visit(Expr.Conditional _conditional)
    {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Variable _variable)
    {
        if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(_variable.Name.lexeme, out bool value) && value == false)
        {
            Lox.Error(_variable.Name, "Cannot read local variable in its own initializer.");
        }

        ResolveLocal(_variable, _variable.Name);

        return null;
    }

    #endregion "Expression Visitors"
}