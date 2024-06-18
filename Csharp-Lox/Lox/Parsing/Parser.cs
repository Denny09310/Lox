namespace Lox;

public class Parser
{
    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    /// <summary>
    /// Parses the tokens and returns a list of statements to be executed.
    /// </summary>
    /// <returns>list of parsed statements</returns>
    public List<Statement> Parse()
    {
        List<Statement> statements = [];
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private static ParseError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }

    private Expr Addition()
    {
        Expr expr = Multiplication();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token opp = Previous();
            Expr right = Multiplication();
            expr = new Expr.Binary(expr, opp, right);
        }

        return expr;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.AND))
        {
            Token opp = Previous();
            Expr right = Equality();

            expr = new Expr.Logical(expr, opp, right);
        }

        return expr;
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.EQUAL))
        {
            Token equal = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable v)
            {
                Token name = v.Name;
                return new Expr.Assign(name, value);
            }
            else if (expr is Expr.Get get)
            {
                return new Expr.Set(get.Target, get.Name, value);
            }

            Error(equal, "Invalid assignment target.");
        }

        return expr;
    }

    /// <summary>
    /// Parses and returns a block of statements.
    /// </summary>
    /// <returns></returns>
    private List<Statement> Block()
    {
        List<Statement> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.DOT))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;

        return Peek().type == type;
    }

    private bool CheckNext(TokenType type)
    {
        if (IsAtEnd()) return false;
        if (tokens[current + 1].type == TokenType.EOF) return false;
        return tokens[current + 1].type == type;
    }

    private Statement.Class ClassDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");

        Expr.Variable? superclass = null;
        if (Match(TokenType.LESS))
        {
            Consume(TokenType.IDENTIFIER, "Expect superclass name.");
            superclass = new Expr.Variable(Previous());
        }

        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

        List<Statement.Function> methods = [];
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add((Statement.Function)Function("method"));
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

        return new Statement.Class(name, superclass, methods);
    }

    private Expr Comparison()
    {
        Expr expr = Addition();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token opp = Previous();
            Expr right = Addition();
            expr = new Expr.Binary(expr, opp, right);
        }

        return expr;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    /// <summary>
    /// Parses and returns a variable or function declaration, if one if there
    /// </summary>
    /// <returns></returns>
    private Statement Declaration()
    {
        try
        {
            if (Match(TokenType.FUN)) return Function("function");
            if (Match(TokenType.CLASS)) return ClassDeclaration();
            if (Match(TokenType.VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError error)
        {
            Synchronize();
            if (error is null) return null;
            return null;
        }
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token opp = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, opp, right);
        }

        return expr;
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Statement ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");

        return new Statement.Expression(expr);
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255) Error(Peek(), "Cannot have more than 255 arguments.");
                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    /// <summary>
    /// Parses and returns a 'for' statement.
    /// </summary>
    /// <returns></returns>
    private Statement ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
        Statement initializer;
        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after for condition.");

        Expr increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        Statement body = Statement();

        if (increment != null)
        {
            body = new Statement.Block([body, new Statement.Expression(increment)]);
        }

        if (condition == null) condition = new Expr.Literal(true);
        body = new Statement.While(condition, body);

        if (initializer != null)
        {
            body = new Statement.Block([initializer, body]);
        }

        return body;
    }

    private Statement Function(string kind)
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + "name.");

        List<Token> parameters = [];
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Cannot have more than 255 parameters.");
                }

                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
        List<Statement> body = Block();

        return new Statement.Function(name, parameters, body);
    }

    /// <summary>
    /// Parses and returns an 'If' statement.
    /// </summary>
    /// <returns></returns>
    private Statement IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Statement thenBranch = Statement();
        Statement elseBranch = null;

        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new Statement.If(condition, thenBranch, elseBranch);
    }

    private bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool MatchNext(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (CheckNext(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Expr Multiplication()
    {
        Expr expr = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token opp = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, opp, right);
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.OR))
        {
            Token opp = Previous();
            Expr right = And();

            expr = new Expr.Logical(expr, opp, right);
        }

        return expr;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Expr Postfix()
    {
        if (MatchNext(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
        {
            Token opp = Peek();
            current--;
            Expr left = Primary();
            Advance();
            return new Expr.Postfix(opp, left);
        }

        return Call();
    }

    private Expr Prefix()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token opp = Previous();
            Expr right = Prefix();
            return new Expr.Prefix(opp, right);
        }

        return Call();
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }

    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.Literal(Previous().literal);
        }

        if (Match(TokenType.SUPER))
        {
            Token keyword = Previous();
            Consume(TokenType.DOT, "Expect '.' after 'super'.");
            Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name after '.'");

            return new Expr.Super(keyword, method);
        }

        if (Match(TokenType.THIS)) return new Expr.This(Previous());

        if (Match(TokenType.IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Statement PrintStatement()
    {
        Expr value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");

        return new Statement.Print(value);
    }

    private Statement ReturnStatement()
    {
        Token keyword = Previous();
        Expr value = null;

        if (!Check(TokenType.SEMICOLON))
        {
            value = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");

        return new Statement.Return(keyword, value);
    }

    /// <summary>
    /// Determines the next statement, and calls the appropriate constructing function.
    /// </summary>
    /// <returns></returns>
    private Statement Statement()
    {
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.PRINT)) return PrintStatement();
        if (Match(TokenType.RETURN)) return ReturnStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Statement.Block(Block());

        return ExpressionStatement();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON) return;

            switch (Peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }

    private Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token opp = Previous();
            Expr right = Unary();
            return new Expr.Prefix(opp, right);
        }

        return Postfix();
    }

    private Statement VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Statement.Var(name, initializer);
    }

    private Statement WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");

        Statement body = Statement();

        return new Statement.While(condition, body);
    }
}