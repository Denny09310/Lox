namespace Lox;

public class Parser(IList<Token> tokens)
{
    private int _current = 0;

    /// <summary>
    /// Parses the tokens and returns a list of statements to be executed.
    /// </summary>
    /// <returns>list of parsed statements</returns>
    public List<Statement> Parse()
    {
        List<Statement> statements = [];
        while (!IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration == null) continue;
            statements.Add(declaration);
        }

        return statements;
    }

    private static ParseErrorException Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseErrorException();
    }

    private Expression Addition()
    {
        Expression expr = Multiplication();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token opp = Previous();
            Expression right = Multiplication();
            expr = new Expression.Binary(expr, opp, right);
        }

        return expr;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Expression And()
    {
        Expression expr = Equality();

        while (Match(TokenType.AND))
        {
            Token opp = Previous();
            Expression right = Equality();

            expr = new Expression.Logical(expr, opp, right);
        }

        return expr;
    }

    private Expression Assignment()
    {
        Expression expr = Or();

        if (Match(TokenType.EQUAL))
        {
            Token equal = Previous();
            Expression value = Assignment();

            if (expr is Expression.Variable v)
            {
                Token name = v.Name;
                return new Expression.Assign(name, value);
            }
            else if (expr is Expression.Get get)
            {
                return new Expression.Set(get.Target, get.Name, value);
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
            var declaration = Declaration();
            if (declaration == null) continue;
            statements.Add(declaration);
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expression Call()
    {
        Expression expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.DOT))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Expression.Get(expr, name);
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

        return Peek().Type == type;
    }

    private bool CheckNext(TokenType type)
    {
        if (IsAtEnd()) return false;
        if (tokens[_current + 1].Type == TokenType.EOF) return false;
        return tokens[_current + 1].Type == type;
    }

    private Statement.Class ClassDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");

        Expression.Variable? superclass = null;
        if (Match(TokenType.LESS))
        {
            Consume(TokenType.IDENTIFIER, "Expect superclass name.");
            superclass = new Expression.Variable(Previous());
        }

        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

        List<Statement.Function> methods = [];
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

        return new Statement.Class(name, superclass, methods);
    }

    private Expression Comparison()
    {
        Expression expr = Addition();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token opp = Previous();
            Expression right = Addition();
            expr = new Expression.Binary(expr, opp, right);
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
    private Statement? Declaration()
    {
        try
        {
            if (Match(TokenType.FUN)) return Function("function");
            if (Match(TokenType.CLASS)) return ClassDeclaration();
            if (Match(TokenType.VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseErrorException)
        {
            Synchronize();
            return null;
        }
    }

    private Expression Equality()
    {
        Expression expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token opp = Previous();
            Expression right = Comparison();
            expr = new Expression.Binary(expr, opp, right);
        }

        return expr;
    }

    private Expression Expression()
    {
        return Assignment();
    }

    private Statement.Inline ExpressionStatement()
    {
        Expression expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");

        return new Statement.Inline(expr);
    }

    private Expression.Call FinishCall(Expression callee)
    {
        List<Expression> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255) Error(Peek(), "Cannot have more than 255 arguments.");
                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expression.Call(callee, paren, arguments);
    }

    /// <summary>
    /// Parses and returns a 'for' statement.
    /// </summary>
    /// <returns></returns>
    private Statement ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
        Statement? initializer;
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

        Expression? condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after for condition.");

        Expression? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        Statement body = Statement();

        if (increment != null)
        {
            body = new Statement.Block([body, new Statement.Inline(increment)]);
        }

        condition ??= new Expression.Literal(true);
        body = new Statement.While(condition, body);

        if (initializer != null)
        {
            body = new Statement.Block([initializer, body]);
        }

        return body;
    }

    private Statement.Function Function(string kind)
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
    private Statement.If IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expression condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Statement thenBranch = Statement();
        Statement? elseBranch = null;

        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new Statement.If(condition, thenBranch, elseBranch);
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private bool Match(params TokenType[] types)
    {
        if (Array.Exists(types, Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool MatchNext(params TokenType[] types)
    {
        if (Array.Exists(types, CheckNext))
        {
            Advance();
            return true;
        }

        return false;
    }

    private Expression Multiplication()
    {
        Expression expr = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token opp = Previous();
            Expression right = Unary();
            expr = new Expression.Binary(expr, opp, right);
        }

        return expr;
    }

    private Expression Or()
    {
        Expression expr = And();

        while (Match(TokenType.OR))
        {
            Token opp = Previous();
            Expression right = And();

            expr = new Expression.Logical(expr, opp, right);
        }

        return expr;
    }

    private Token Peek()
    {
        return tokens[_current];
    }

    private Expression Postfix()
    {
        if (MatchNext(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
        {
            Token opp = Peek();
            _current--;
            Expression left = Primary();
            Advance();
            return new Expression.Postfix(opp, left);
        }

        return Call();
    }

    private Expression Prefix()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token opp = Previous();
            Expression right = Prefix();
            return new Expression.Prefix(opp, right);
        }

        return Call();
    }

    private Token Previous()
    {
        return tokens[_current - 1];
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE)) return new Expression.Literal(false);
        if (Match(TokenType.TRUE)) return new Expression.Literal(true);
        if (Match(TokenType.NIL)) return new Expression.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expression.Literal(Previous().Literal);
        }

        if (Match(TokenType.SUPER))
        {
            Token keyword = Previous();
            Consume(TokenType.DOT, "Expect '.' after 'super'.");
            Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name after '.'");

            return new Expression.Super(keyword, method);
        }

        if (Match(TokenType.THIS)) return new Expression.This(Previous());

        if (Match(TokenType.IDENTIFIER))
        {
            return new Expression.Variable(Previous());
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            Expression expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expression.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Statement.Print PrintStatement()
    {
        Expression value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");

        return new Statement.Print(value);
    }

    private Statement.Return ReturnStatement()
    {
        Token keyword = Previous();
        Expression? value = null;

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
            if (Previous().Type == TokenType.SEMICOLON) return;

            switch (Peek().Type)
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

    private Expression Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token opp = Previous();
            Expression right = Unary();
            return new Expression.Prefix(opp, right);
        }

        return Postfix();
    }

    private Statement.Var VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expression? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Statement.Var(name, initializer);
    }

    private Statement.While WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expression condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");

        Statement body = Statement();

        return new Statement.While(condition, body);
    }
}