 

namespace Lox;

public abstract class Expression
{
    public interface IVisitor<out T> 
    {
        T Visit(Assign expression);
        T Visit(Binary expression);
        T Visit(Ternary expression);
        T Visit(Call expression);
        T Visit(Get expression);
        T Visit(Grouping expression);
        T Visit(Literal expression);
        T Visit(Logical expression);
        T Visit(Set expression);
        T Visit(Super expression);
        T Visit(This expression);
        T Visit(Prefix expression);
        T Visit(Postfix expression);
        T Visit(Conditional expression);
        T Visit(Variable expression);
    }

	
    /// <summary>
    /// Base function for visiting our trees.
    /// </summary> 
    public abstract T Accept<T>(IVisitor<T> visitor);

    public sealed class Assign(Token name, Expression value) : Expression
    {
        public Token Name{ get; } = name;
        public Expression Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Binary(Expression left, Token opp, Expression right) : Expression
    {
        public Expression Left{ get; } = left;
        public Token Opp{ get; } = opp;
        public Expression Right{ get; } = right;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Ternary(Expression condition, Expression truebranch, Expression falsebranch) : Expression
    {
        public Expression Condition{ get; } = condition;
        public Expression TrueBranch{ get; } = truebranch;
        public Expression FalseBranch{ get; } = falsebranch;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Call(Expression callee, Token paren, List<Expression> arguments) : Expression
    {
        public Expression Callee{ get; } = callee;
        public Token Paren{ get; } = paren;
        public List<Expression> Arguments{ get; } = arguments;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Get(Expression target, Token name) : Expression
    {
        public Expression Target{ get; } = target;
        public Token Name{ get; } = name;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Grouping(Expression expression) : Expression
    {
        public Expression Expression{ get; } = expression;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Literal(object? value) : Expression
    {
        public object? Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Logical(Expression left, Token opp, Expression right) : Expression
    {
        public Expression Left{ get; } = left;
        public Token Opp{ get; } = opp;
        public Expression Right{ get; } = right;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Set(Expression target, Token name, Expression value) : Expression
    {
        public Expression Target{ get; } = target;
        public Token Name{ get; } = name;
        public Expression Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Super(Token keyword, Token method) : Expression
    {
        public Token Keyword{ get; } = keyword;
        public Token Method{ get; } = method;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class This(Token keyword) : Expression
    {
        public Token Keyword{ get; } = keyword;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Prefix(Token opp, Expression right) : Expression
    {
        public Token Opp{ get; } = opp;
        public Expression Right{ get; } = right;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Postfix(Token opp, Expression left) : Expression
    {
        public Token Opp{ get; } = opp;
        public Expression Left{ get; } = left;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Conditional(Expression expression, Expression thenbranch, Expression elsebranch) : Expression
    {
        public Expression Expression{ get; } = expression;
        public Expression ThenBranch{ get; } = thenbranch;
        public Expression ElseBranch{ get; } = elsebranch;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Variable(Token name) : Expression
    {
        public Token Name{ get; } = name;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

