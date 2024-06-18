
namespace Lox;

public abstract class Statement
{
    public interface IVisitor<out T> 
    {
        T Visit(Block statement);
        T Visit(Class statement);
        T Visit(Inline statement);
        T Visit(Function statement);
        T Visit(If statement);
        T Visit(Return statement);
        T Visit(Var statement);
        T Visit(While statement);
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
    public sealed class Class(Token name, Expression.Variable? superclass, IEnumerable<Statement.Function> methods) : Statement
    {
        public Token Name{ get; } = name;
        public Expression.Variable? Superclass{ get; } = superclass;
        public IEnumerable<Statement.Function> Methods{ get; } = methods;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Inline(Expression body) : Statement
    {
        public Expression Body{ get; } = body;
         
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
    public sealed class If(Expression condition, Statement thenbranch, Statement? elsebranch) : Statement
    {
        public Expression Condition{ get; } = condition;
        public Statement ThenBranch{ get; } = thenbranch;
        public Statement? ElseBranch{ get; } = elsebranch;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Return(Token keyword, Expression? value) : Statement
    {
        public Token Keyword{ get; } = keyword;
        public Expression? Value{ get; } = value;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class Var(Token name, Expression? initializer) : Statement
    {
        public Token Name{ get; } = name;
        public Expression? Initializer{ get; } = initializer;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    public sealed class While(Expression condition, Statement body) : Statement
    {
        public Expression Condition{ get; } = condition;
        public Statement Body{ get; } = body;
         
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

