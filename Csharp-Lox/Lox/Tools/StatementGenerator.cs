



namespace Lox;

public abstract class Statement
{
    public interface IVisitor<T> 
    {
T Visit(Block _block);
        T Visit(Class _class);
        T Visit(Expression _expression);
        T Visit(Function _function);
        T Visit(If _if);
        T Visit(Print _print);
        T Visit(Return _return);
        T Visit(Var _var);
        T Visit(While _while);

    }

	
    /// <summary>
    /// Base function for visiting our trees.
    /// </summary> 
    public abstract T Accept<T>(IVisitor<T> visitor);

public sealed class Block(IList<Statement> statements) : Statement
    {
        public IList<Statement> Statements{ get; } = statements;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Class(Token name, Expr.Variable? superclass, IEnumerable<Statement.Function> methods) : Statement
    {
        public Token Name{ get; } = name;
        public Expr.Variable? Superclass{ get; } = superclass;
        public IEnumerable<Statement.Function> Methods{ get; } = methods;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Expression(Expr body) : Statement
    {
        public Expr Body{ get; } = body;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Function(Token name, IList<Token> parameters, IList<Statement> body) : Statement
    {
        public Token Name{ get; } = name;
        public IList<Token> Parameters{ get; } = parameters;
        public IList<Statement> Body{ get; } = body;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class If(Expr condition, Statement thenbranch, Statement elsebranch) : Statement
    {
        public Expr Condition{ get; } = condition;
        public Statement ThenBranch{ get; } = thenbranch;
        public Statement ElseBranch{ get; } = elsebranch;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Print(Expr body) : Statement
    {
        public Expr Body{ get; } = body;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Return(Token keyword, Expr value) : Statement
    {
        public Token Keyword{ get; } = keyword;
        public Expr Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Var(Token name, Expr initializer) : Statement
    {
        public Token Name{ get; } = name;
        public Expr Initializer{ get; } = initializer;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class While(Expr condition, Statement body) : Statement
    {
        public Expr Condition{ get; } = condition;
        public Statement Body{ get; } = body;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}

