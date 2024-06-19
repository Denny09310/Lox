using System.Text;

namespace Lox;

public class AbstractSyntaxTreeBuilder : Expression.IVisitor<string?>
{
    public string? Print(Expression expr)
    {
        return expr.Accept(this);
    }

    public string? Visit(Expression.Assign expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Binary expression)
    {
        return Parenthesize(expression.Opp.Lexeme, expression.Left, expression.Right);
    }

    public string? Visit(Expression.Call expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Get expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Grouping expression)
    {
        return Parenthesize("group", expression.Expression);
    }

    public string? Visit(Expression.Literal expression)
    {
        if (expression.Value == null) return "nil";
        return expression.Value.ToString();
    }

    public string? Visit(Expression.Logical expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Set expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Super expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.This expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Prefix expression)
    {
        return Parenthesize(expression.Opp.Lexeme, expression.Right);
    }

    public string? Visit(Expression.Postfix expression)
    {
        return Parenthesize(expression.Opp.Lexeme);
    }

    public string? Visit(Expression.Conditional expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Variable expression)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expression.Ternary expression)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string name, params Expression[] exprs)
    {
        StringBuilder builder = new();

        builder.Append('(').Append(name);
        foreach (Expression expr in exprs)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }
}