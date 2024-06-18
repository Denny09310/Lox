using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Lox;

internal class Interpreter : Expr.IVisitor<object?>, Statement.IVisitor<object?>
{
    private readonly Dictionary<Expr, int> _locals = [];
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
        catch (RuntimeError err)
        {
            Lox.RuntimeError(err);
        }
    }

    public void Resolve(Expr expr, int depth)
    {
        _locals.Add(expr, depth);
    }

    public object? Visit(Expr.Assign _assign)
    {
        object? value = Evaluate(_assign.Value);

        if (_locals.TryGetValue(_assign, out int distance))
        {
            _environment.AssignAt(distance, _assign.Name, value);
        }
        else
        {
            Globals.Assign(_assign.Name, value);
        }

        return value;
    }

    public object? Visit(Expr.Binary binary)
    {
        object? left = Evaluate(binary.Left);
        object? right = Evaluate(binary.Right);

        switch (binary.Opp.type)
        {
            case TokenType.BANG_EQUAL:
                CheckNumberOperand(binary.Opp, right);
                return !IsEqual(left, right);

            case TokenType.EQUAL_EQUAL:
                CheckNumberOperand(binary.Opp, right);
                return IsEqual(left, right);

            case TokenType.GREATER:
                CheckNumberOperands(binary.Opp, left, right);
                return (double)left > (double)right;

            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(binary.Opp, left, right);
                return (double)left >= (double)right;

            case TokenType.LESS:
                CheckNumberOperands(binary.Opp, left, right);
                return (double)left < (double)right;

            case TokenType.LESS_EQUAL:
                CheckNumberOperands(binary.Opp, left, right);
                return (double)left <= (double)right;

            case TokenType.MINUS:
                CheckNumberOperands(binary.Opp, left, right);
                return (double)left - (double)right;

            case TokenType.PLUS:
                if (left is double l1 && right is double r1) return l1 + r1;
                if (left is string l2 && right is string r2) return l2 + r2;
                if (left is string l3 && right is double r3) return l3 + r3.ToString();
                throw new RuntimeError(binary.Opp, "Operands must be two numbers or two strings.");

            case TokenType.SLASH:
                CheckNumberOperands(binary.Opp, left, right);
                if ((double)right == 0) throw new RuntimeError(binary.Opp, "Cannot divide by zero.");
                return (double)left / (double)right;

            case TokenType.STAR:
                CheckNumberOperands(binary.Opp, left, right);
                return (double)left * (double)right;
        }

        throw new UnreachableException();
    }

    public object? Visit(Expr.Call call)
    {
        object? callee = Evaluate(call.Callee);

        List<object?> arguments = [];
        foreach (Expr argument in call.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ICallable function)
        {
            throw new RuntimeError(call.Paren, "Can only call functions and classes.");
        }

        if (arguments.Count != function.Arity)
        {
            throw new RuntimeError(call.Paren, "Expected " + function.Arity + " arguments, but got " + arguments.Count + ".");
        }

        return function.Call(this, arguments);
    }

    public object? Visit(Expr.Get get)
    {
        object? target = Evaluate(get.Target);
        if (target is LoxInstance instance)
        {
            return instance.Get(get.Name);
        }

        throw new RuntimeError(get.Name, "Only instances have properties.");
    }

    public object? Visit(Expr.Grouping grouping)
    {
        return Evaluate(grouping.Expression);
    }

    public object? Visit(Expr.Literal literal)
    {
        return literal.Value;
    }

    public object? Visit(Expr.Logical logical)
    {
        object? left = Evaluate(logical.Left);

        if (logical.Opp.type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(logical.Right);
    }

    public object? Visit(Expr.Set set)
    {
        object? target = Evaluate(set.Target);

        if (target is not LoxInstance instance)
        {
            throw new RuntimeError(set.Name, "Only instances have fields.");
        }

        object? value = Evaluate(set.Target);
        instance.Set(set.Name, value);

        return value;
    }

    public object? Visit(Expr.Super super)
    {
        int distance = _locals[super];

        if (_environment.GetAt(distance, "super") is not LoxClass superclass)
        {
            throw new RuntimeError(super.Keyword, $"The class does not inherit {super.Method.lexeme}");
        }

        if (_environment.GetAt(distance, "super") is not LoxInstance instance)
        {
            throw new RuntimeError(super.Keyword, $"The class does not inherit {super.Method.lexeme}");
        }

        LoxFunction method = superclass.FindMethod(super.Method.lexeme)
            ?? throw new RuntimeError(super.Method, "Undefined property '" + super.Method.lexeme + "'.");

        return method.Bind(instance);
    }

    public object? Visit(Expr.This @this)
    {
        return LookUpVariable(@this.Keyword, @this);
    }

    public object? Visit(Expr.Prefix prefix)
    {
        object? right = Evaluate(prefix.Right);

        switch (prefix.Opp.type)
        {
            case TokenType.MINUS:
                CheckNumberOperand(prefix.Opp, right);
                return -(double)right;

            case TokenType.BANG:
                return !IsTruthy(right);

            default:
                break;
        }

        throw new UnreachableException();
    }

    public object? Visit(Expr.Conditional conditional)
    {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Variable variable)
    {
        return LookUpVariable(variable.Name, variable);
    }

    public object? Visit(Statement.Expression expression)
    {
        Evaluate(expression.Body);

        return null;
    }

    public object? Visit(Statement.Print print)
    {
        object? value = Evaluate(print.Body);
        Console.WriteLine(Stringify(value));

        return null;
    }

    public object? Visit(Statement.Var @var)
    {
        object? value = null;
        if (@var.Initializer != null)
        {
            value = Evaluate(@var.Initializer);
        }

        _environment.Define(@var.Name.lexeme, value);
        return null;
    }

    public object? Visit(Statement.Block block)
    {
        ExecuteBlock(block.Statements, new Environment(_environment));
        return null;
    }

    public object? Visit(Statement.If @if)
    {
        if (IsTruthy(Evaluate(@if.Condition)))
        {
            Execute(@if.ThenBranch);
        }
        else if (@if.ElseBranch != null)
        {
            Execute(@if.ElseBranch);
        }
        return null;
    }

    public object? Visit(Statement.While @while)
    {
        while (IsTruthy(Evaluate(@while.Condition)))
        {
            Execute(@while.Body);
        }

        return null;
    }

    public object? Visit(Statement.Function function)
    {
        LoxFunction func = new(function, _environment, false);
        _environment.Define(function.Name.lexeme, func);

        return null;
    }

    public object? Visit(Statement.Return @return)
    {
        object? value = null;

        if (@return.Value != null) value = Evaluate(@return.Value);

        throw new Return(value);
    }

    public object? Visit(Statement.Class @class)
    {
        if (@class.Superclass == null)
        {
            return null;
        }

        object? superclass = Evaluate(@class.Superclass);
        if (superclass is not LoxClass superklass)
        {
            throw new RuntimeError(@class.Superclass.Name, "Superclass must be a class.");
        }

        _environment.Define(@class.Name.lexeme, null);

        if (@class.Superclass != null)
        {
            _environment = new Environment(_environment);
            _environment.Define("super", superclass);
        }

        Dictionary<string, LoxFunction> methods = [];
        foreach (Statement.Function method in @class.Methods)
        {
            LoxFunction function = new(method, _environment, method.Name.lexeme.Equals("init"));
            methods.Add(method.Name.lexeme, function);
        }

        LoxClass klass = new(@class.Name.lexeme, superklass, methods);

        if (_environment.Enclosing is null)
        {
            return null;
        }

        if (superclass != null) _environment = _environment.Enclosing;

        _environment.Assign(@class.Name, klass);

        return null;
    }

    public object? Visit(Expr.Postfix _postfix)
    {
        object? left = Evaluate(_postfix.Left);

        CheckNumberOperand(_postfix.Opp, left);

        switch (_postfix.Opp.type)
        {
            case TokenType.PLUS_PLUS:
                return (double)left + 1;

            case TokenType.MINUS_MINUS:
                return (double)left - 1;

            default:
                break;
        }

        return null;
    }

    private static void CheckNumberOperand(Token opp, [NotNull] object? operand)
    {
        if (operand is double) return;

        throw new RuntimeError(opp, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token opp, [NotNull] object? left, [NotNull] object? right)
    {
        if (left is double && right is double) return;

        throw new RuntimeError(opp, "Operands must be numbers.");
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

    private static string Stringify(object? obj)
    {
        if (obj == null) return "nil";

        if (obj is double)
        {
            string? text = obj.ToString();
            if (!string.IsNullOrEmpty(text) && text.EndsWith(".0"))
            {
                text = text[0..(text.Length - 2)];
            }
            return text ?? string.Empty;
        }

        return obj.ToString() ?? string.Empty;
    }

    private void DefineNativeFunctions()
    {
        Globals.Define("clock", new NativeFunctions.Clock());
        Globals.Define("exit", new NativeFunctions.Exit());
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Statement stmt)
    {
        stmt.Accept(this);
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out int distance))
        {
            return _environment.GetAt(distance, name.lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }
}