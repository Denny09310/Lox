








 
using System.Collections.Generic;


namespace Lox;

public abstract class Expr
{
    public interface IVisitor<T> 
    {
T Visit(Assign _assign);
        T Visit(Binary _binary);
        T Visit(Call _call);
        T Visit(Get _get);
        T Visit(Grouping _grouping);
        T Visit(Literal _literal);
        T Visit(Logical _logical);
        T Visit(Set _set);
        T Visit(Super _super);
        T Visit(This _this);
        T Visit(Prefix _prefix);
        T Visit(Postfix _postfix);
        T Visit(Conditional _conditional);
        T Visit(Variable _variable);

    }

	
    /// <summary>
    /// Base function for visiting our trees.
    /// </summary> 
    public abstract T Accept<T>(IVisitor<T> visitor);

public sealed class Assign(Token name, Expr value) : Expr
    {
        public Token Name{ get; } = name;
        public Expr Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Binary(Expr left, Token opp, Expr right) : Expr
    {
        public Expr Left{ get; } = left;
        public Token Opp{ get; } = opp;
        public Expr Right{ get; } = right;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Call(Expr callee, Token paren, List<Expr> arguments) : Expr
    {
        public Expr Callee{ get; } = callee;
        public Token Paren{ get; } = paren;
        public List<Expr> Arguments{ get; } = arguments;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Get(Expr target, Token name) : Expr
    {
        public Expr Target{ get; } = target;
        public Token Name{ get; } = name;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Grouping(Expr expression) : Expr
    {
        public Expr Expression{ get; } = expression;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Literal(object value) : Expr
    {
        public object Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Logical(Expr left, Token opp, Expr right) : Expr
    {
        public Expr Left{ get; } = left;
        public Token Opp{ get; } = opp;
        public Expr Right{ get; } = right;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Set(Expr target, Token name, Expr value) : Expr
    {
        public Expr Target{ get; } = target;
        public Token Name{ get; } = name;
        public Expr Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Super(Token keyword, Token method) : Expr
    {
        public Token Keyword{ get; } = keyword;
        public Token Method{ get; } = method;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class This(Token keyword) : Expr
    {
        public Token Keyword{ get; } = keyword;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Prefix(Token opp, Expr right) : Expr
    {
        public Token Opp{ get; } = opp;
        public Expr Right{ get; } = right;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Postfix(Token opp, Expr left) : Expr
    {
        public Token Opp{ get; } = opp;
        public Expr Left{ get; } = left;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Conditional(Expr expression, Expr thenbranch, Expr elsebranch) : Expr
    {
        public Expr Expression{ get; } = expression;
        public Expr ThenBranch{ get; } = thenbranch;
        public Expr ElseBranch{ get; } = elsebranch;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Variable(Token name) : Expr
    {
        public Token Name{ get; } = name;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}

