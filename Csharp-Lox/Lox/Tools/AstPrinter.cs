using System.Text;

namespace Lox;

public class AstPrinter : Expr.IVisitor<string?>
{
    public string? Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string? Visit(Expr.Assign _assign)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Binary _binary)
    {
        return Parenthesize(_binary.Opp.lexeme, _binary.Left, _binary.Right);
    }

    public string? Visit(Expr.Call _call)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Get _get)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Grouping _grouping)
    {
        return Parenthesize("group", _grouping.Expression);
    }

    public string? Visit(Expr.Literal _literal)
    {
        if (_literal.Value == null) return "nil";
        return _literal.Value.ToString();
    }

    public string? Visit(Expr.Logical _logical)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Set _set)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Super _super)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.This _this)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Prefix _prefix)
    {
        return Parenthesize(_prefix.Opp.lexeme, _prefix.Right);
    }

    public string? Visit(Expr.Postfix _postfix)
    {
        return Parenthesize(_postfix.Opp.lexeme);
    }

    public string? Visit(Expr.Conditional _conditional)
    {
        throw new NotImplementedException();
    }

    public string? Visit(Expr.Variable _variable)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        StringBuilder builder = new();

        builder.Append('(').Append(name);
        foreach (Expr expr in exprs)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }
}