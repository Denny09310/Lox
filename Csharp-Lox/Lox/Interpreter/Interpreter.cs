using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Lox;

internal class Interpreter : Expression.IVisitor<object?>, Statement.IVisitor<object?>
{
    private readonly Dictionary<Expression, int> _locals = [];
    private Environment _environment;

    public Interpreter()
    {
        Globals = new Environment();
        _environment = Globals;
        DefineNativeFunctions();
    }

    public Environment Globals { get; }

    public void ExecuteBlock(IEnumerable<Statement> statements, Environment environment)
    {
        Environment previous = _environment;

        try
        {
            _environment = environment;

            foreach (Statement statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public void Interpret(List<Statement> statements)
    {
        try
        {
            foreach (Statement statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeException err)
        {
            Lox.RuntimeError(err);
        }
    }

    public void Resolve(Expression expr, int depth)
    {
        _locals.Add(expr, depth);
    }

    public object? Visit(Expression.Assign expression)
    {
        object? value = Evaluate(expression.Value);

        if (_locals.TryGetValue(expression, out int distance))
        {
            _environment.AssignAt(distance, expression.Name, value);
        }
        else
        {
            Globals.Assign(expression.Name, value);
        }

        return value;
    }

    public object? Visit(Expression.Binary expression)
    {
        object? left = Evaluate(expression.Left);
        object? right = Evaluate(expression.Right);

        switch (expression.Opp.Kind)
        {
            case SyntaxKind.BANG_EQUAL:
                CheckNumberOperand(expression.Opp, right);
                return !IsEqual(left, right);

            case SyntaxKind.EQUAL_EQUAL:
                CheckNumberOperand(expression.Opp, right);
                return IsEqual(left, right);

            case SyntaxKind.GREATER:
                CheckNumberOperands(expression.Opp, left, right);
                return (double)left > (double)right;

            case SyntaxKind.GREATER_EQUAL:
                CheckNumberOperands(expression.Opp, left, right);
                return (double)left >= (double)right;

            case SyntaxKind.LESS:
                CheckNumberOperands(expression.Opp, left, right);
                return (double)left < (double)right;

            case SyntaxKind.LESS_EQUAL:
                CheckNumberOperands(expression.Opp, left, right);
                return (double)left <= (double)right;

            case SyntaxKind.MINUS:
                CheckNumberOperands(expression.Opp, left, right);
                return (double)left - (double)right;

            case SyntaxKind.PLUS:
                if (left is double l1 && right is double r1) return l1 + r1;
                if (left is string l2 && right is string r2) return l2 + r2;
                if (left is string l3 && right is double r3) return l3 + r3.ToString();
                throw new RuntimeException(expression.Opp, "Operands must be two numbers or two strings.");

            case SyntaxKind.SLASH:
                CheckNumberOperands(expression.Opp, left, right);
                if (((double)right).Equals(0)) throw new RuntimeException(expression.Opp, "Cannot divide by zero.");
                return (double)left / (double)right;

            case SyntaxKind.STAR:
                CheckNumberOperands(expression.Opp, left, right);
                return (double)left * (double)right;
        }

        throw new UnreachableException();
    }

    public object? Visit(Expression.Call expression)
    {
        object? callee = Evaluate(expression.Callee);

        List<object?> arguments = [];
        foreach (Expression argument in expression.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ICallable function)
        {
            throw new RuntimeException(expression.Paren, "Can only call functions and classes.");
        }

        if (arguments.Count != function.Arity)
        {
            throw new RuntimeException(expression.Paren, "Expected " + function.Arity + " arguments, but got " + arguments.Count + ".");
        }

        return function.Call(this, arguments);
    }

    public object? Visit(Expression.Get expression)
    {
        object? target = Evaluate(expression.Target);
        if (target is InstanceDefinition instance)
        {
            return instance.Get(expression.Name);
        }

        throw new RuntimeException(expression.Name, "Only instances have properties.");
    }

    public object? Visit(Expression.Grouping expression)
    {
        return Evaluate(expression.Expression);
    }

    public object? Visit(Expression.Literal expression)
    {
        return expression.Value;
    }

    public object? Visit(Expression.Logical expression)
    {
        object? left = Evaluate(expression.Left);

        if (expression.Opp.Kind == SyntaxKind.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expression.Right);
    }

    public object? Visit(Expression.Set expression)
    {
        object? target = Evaluate(expression.Target);

        if (target is not InstanceDefinition instance)
        {
            throw new RuntimeException(expression.Name, "Only instances have fields.");
        }

        object? value = Evaluate(expression.Value);
        instance.Set(expression.Name, value);

        return value;
    }

    public object? Visit(Expression.Super expression)
    {
        int distance = _locals[expression];

        if (_environment.GetAt(distance, "super") is not ClassDefinition superclass)
        {
            throw new RuntimeException(expression.Keyword, $"The class does not inherit {expression.Method.Lexeme}");
        }

        if (_environment.GetAt(distance, "super") is not InstanceDefinition instance)
        {
            throw new RuntimeException(expression.Keyword, $"The class does not inherit {expression.Method.Lexeme}");
        }

        FunctionDefinition method = superclass.FindMethod(expression.Method.Lexeme)
            ?? throw new RuntimeException(expression.Method, "Undefined property '" + expression.Method.Lexeme + "'.");

        return method.Bind(instance);
    }

    public object? Visit(Expression.This expression)
    {
        return LookUpVariable(expression.Keyword, expression);
    }

    public object? Visit(Expression.Prefix expression)
    {
        object? right = Evaluate(expression.Right);

        switch (expression.Opp.Kind)
        {
            case SyntaxKind.MINUS:
                CheckNumberOperand(expression.Opp, right);
                return -(double)right;

            case SyntaxKind.BANG:
                return !IsTruthy(right);

            default:
                break;
        }

        throw new UnreachableException();
    }

    public object? Visit(Expression.Conditional expression)
    {
        throw new NotImplementedException();
    }

    public object? Visit(Expression.Variable expression)
    {
        return LookUpVariable(expression.Name, expression);
    }

    public object? Visit(Statement.Inline statement)
    {
        Evaluate(statement.Body);

        return null;
    }

    public object? Visit(Statement.Var statement)
    {
        object? value = null;
        if (statement.Initializer != null)
        {
            value = Evaluate(statement.Initializer);
        }

        _environment.Define(statement.Name.Lexeme, value);
        return null;
    }

    public object? Visit(Statement.Block statement)
    {
        ExecuteBlock(statement.Statements, new Environment(_environment));
        return null;
    }

    public object? Visit(Statement.If statement)
    {
        if (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.ThenBranch);
        }
        else if (statement.ElseBranch != null)
        {
            Execute(statement.ElseBranch);
        }
        return null;
    }

    public object? Visit(Statement.While statement)
    {
        while (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.Body);
        }

        return null;
    }

    public object? Visit(Statement.Function statement)
    {
        FunctionDefinition func = new(statement, _environment, false);
        _environment.Define(statement.Name.Lexeme, func);

        return null;
    }

    public object? Visit(Statement.Return statement)
    {
        object? value = null;

        if (statement.Value != null) value = Evaluate(statement.Value);

        throw new ReturnException(value);
    }

    public object? Visit(Statement.Class statement)
    {
        object? superclass = null;
        if (statement.Superclass != null)
        {
            superclass = Evaluate(statement.Superclass);
            if (superclass is not ClassDefinition)
            {
                throw new RuntimeException(statement.Superclass.Name, "Superclass must be a class.");
            }
        }

        _environment.Define(statement.Name.Lexeme, null);

        if (superclass != null)
        {
            _environment = new Environment(_environment);
            _environment.Define("super", superclass);
        }

        Dictionary<string, FunctionDefinition> methods = [];
        foreach (Statement.Function method in statement.Methods)
        {
            FunctionDefinition function = new(method, _environment);
            methods.Add(method.Name.Lexeme, function);
        }

        if (statement.Constructor is Statement.Function constructor)
        {
            FunctionDefinition function = new(constructor, _environment, constructor: true);
            methods.Add(constructor.Name.Lexeme, function);
        }

        ClassDefinition? superklass = superclass as ClassDefinition;
        ClassDefinition @class = new(statement.Name.Lexeme, superklass, methods);

        if (superklass?.Superclass != null) _environment = _environment.Enclosing!;
        _environment.Assign(statement.Name, @class);

        return null;
    }

    public object? Visit(Expression.Postfix expression)
    {
        object? left = Evaluate(expression.Left);

        CheckNumberOperand(expression.Opp, left);

        switch (expression.Opp.Kind)
        {
            case SyntaxKind.PLUS_PLUS:
                return (double)left + 1;

            case SyntaxKind.MINUS_MINUS:
                return (double)left - 1;

            default:
                break;
        }

        return null;
    }

    private static void CheckNumberOperand(Token opp, [NotNull] object? operand)
    {
        if (operand is double) return;

        throw new RuntimeException(opp, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token opp, [NotNull] object? left, [NotNull] object? right)
    {
        if (left is double && right is double) return;

        throw new RuntimeException(opp, "Operands must be numbers.");
    }

    private static bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private static bool IsTruthy(object? obj)
    {
        if (obj == null) return false;
        if (obj is bool @bool) return @bool;
        return true;
    }

    private void DefineNativeFunctions()
    {
        Globals.Define("clock", new NativeFunctions.Clock());
        Globals.Define("exit", new NativeFunctions.Exit());
        Globals.Define("print", new NativeFunctions.Print());
    }

    private object? Evaluate(Expression expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Statement stmt)
    {
        stmt.Accept(this);
    }

    private object? LookUpVariable(Token name, Expression expr)
    {
        if (_locals.TryGetValue(expr, out int distance))
        {
            return _environment.GetAt(distance, name.Lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }
}