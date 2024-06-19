namespace Lox;

public class Parser(IList<Token> tokens)
{
    private int _current = 0;

    /// <summary>
    /// Parses the tokens and returns a list of statements to be executed.
    /// </summary>
    /// <returns>list of parsed statements</returns>
    public IList<Statement> Parse()
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

    private static ParseException Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseException();
    }

    private Expression Addition()
    {
        Expression expr = Multiplication();

        while (Match(SyntaxKind.MINUS, SyntaxKind.PLUS))
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

        while (Match(SyntaxKind.AND))
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

        if (Match(SyntaxKind.EQUAL))
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
        else if (Match(SyntaxKind.QUESTION_MARK))
        {
            expr = Ternary(expr);
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

        while (!Check(SyntaxKind.RIGHT_BRACE) && !IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration == null) continue;
            statements.Add(declaration);
        }

        Consume(SyntaxKind.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expression Call()
    {
        Expression expr = Primary();

        while (true)
        {
            if (Match(SyntaxKind.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(SyntaxKind.DOT))
            {
                Token name = Consume(SyntaxKind.IDENTIFIER, "Expect property name after '.'.");
                expr = new Expression.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private bool Check(SyntaxKind kind)
    {
        if (IsAtEnd()) return false;

        return Peek().Kind == kind;
    }

    private bool CheckNext(SyntaxKind kind)
    {
        if (IsAtEnd()) return false;
        if (tokens[_current + 1].Kind == SyntaxKind.EOF) return false;
        return tokens[_current + 1].Kind == kind;
    }

    private Statement.Class Class()
    {
        Token name = Consume(SyntaxKind.IDENTIFIER, "Expect class name.");

        Expression.Variable? superclass = null;
        if (Match(SyntaxKind.COLON))
        {
            Consume(SyntaxKind.IDENTIFIER, "Expect superclass name.");
            superclass = new Expression.Variable(Previous());
        }

        Consume(SyntaxKind.LEFT_BRACE, "Expect '{' before class body.");

        List<Statement.Function> methods = [];
        Statement.Function? constructor = null;
        while (!Check(SyntaxKind.RIGHT_BRACE) && !IsAtEnd())
        {
            if (Check(SyntaxKind.CONSTRUCTOR))
            {
                constructor = Function("method");
                continue;
            }

            methods.Add(Function("method"));
        }

        Consume(SyntaxKind.RIGHT_BRACE, "Expect '}' after class body.");

        return new Statement.Class(name, constructor, superclass, methods);
    }

    private Expression Comparison()
    {
        Expression expr = Addition();

        while (Match(SyntaxKind.GREATER, SyntaxKind.GREATER_EQUAL, SyntaxKind.LESS, SyntaxKind.LESS_EQUAL))
        {
            Token opp = Previous();
            Expression right = Addition();
            expr = new Expression.Binary(expr, opp, right);
        }

        return expr;
    }

    private Token Consume(SyntaxKind kind, string message)
    {
        if (Check(kind)) return Advance();

        throw Error(Peek(), message);
    }

    private Token Consume(SyntaxKind[] kinds, string message)
    {
        if (Array.Exists(kinds, Check)) return Advance();
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
            if (Match(SyntaxKind.FUN)) return Function("function");
            if (Match(SyntaxKind.CLASS)) return Class();
            if (Match(SyntaxKind.VAR)) return Var();

            return Statement();
        }
        catch (ParseException)
        {
            Synchronize();
            return null;
        }
    }

    private void EnsureNotTernaryOperator(Expression expression, Token keyword)
    {
        try
        {
            if (expression is Expression.Ternary)
            {
                throw Error(keyword, "Ternary operator should be used only for assignments");
            }
        }
        finally
        {
            Synchronize();
        }
    }

    private Expression Equality()
    {
        Expression expr = Comparison();

        while (Match(SyntaxKind.BANG_EQUAL, SyntaxKind.EQUAL_EQUAL))
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

    private Expression.Call FinishCall(Expression callee)
    {
        List<Expression> arguments = [];

        if (!Check(SyntaxKind.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255) Error(Peek(), "Cannot have more than 255 arguments.");
                arguments.Add(Expression());
            } while (Match(SyntaxKind.COMMA));
        }

        Token paren = Consume(SyntaxKind.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expression.Call(callee, paren, arguments);
    }

    /// <summary>
    /// Parses and returns a 'for' statement.
    /// </summary>
    /// <returns></returns>
    private Statement ForStatement()
    {
        Consume(SyntaxKind.LEFT_PAREN, "Expect '(' after 'for'.");
        Statement? initializer;
        if (Match(SyntaxKind.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(SyntaxKind.VAR))
        {
            initializer = Var();
        }
        else
        {
            initializer = Inline();
        }

        Expression? condition = null;
        if (!Check(SyntaxKind.SEMICOLON))
        {
            var semicolon = Peek();
            condition = Expression();

            EnsureNotTernaryOperator(condition, semicolon);
        }

        Consume(SyntaxKind.SEMICOLON, "Expect ';' after for condition.");

        Expression? increment = null;
        if (!Check(SyntaxKind.RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(SyntaxKind.RIGHT_PAREN, "Expect ')' after for clauses.");

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
        Token name = Consume([SyntaxKind.CONSTRUCTOR, SyntaxKind.IDENTIFIER], "Expect " + kind + " name.");

        Consume(SyntaxKind.LEFT_PAREN, "Expect '(' after " + kind + "name.");

        List<Token> parameters = [];
        if (!Check(SyntaxKind.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Cannot have more than 255 parameters.");
                }

                parameters.Add(Consume(SyntaxKind.IDENTIFIER, "Expect parameter name."));
            }
            while (Match(SyntaxKind.COMMA));
        }

        Consume(SyntaxKind.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(SyntaxKind.LEFT_BRACE, "Expect '{' before " + kind + " body.");
        List<Statement> body = Block();

        return new Statement.Function(name, parameters, body);
    }

    /// <summary>
    /// Parses and returns an 'If' statement.
    /// </summary>
    /// <returns></returns>
    private Statement.If IfStatement()
    {
        var paren = Consume(SyntaxKind.LEFT_PAREN, "Expect '(' after 'if'.");
        Expression condition = Expression();

        EnsureNotTernaryOperator(condition, paren);

        Consume(SyntaxKind.RIGHT_PAREN, "Expect ')' after if condition.");

        Statement thenBranch = Statement();
        Statement? elseBranch = null;

        if (Match(SyntaxKind.ELSE))
        {
            elseBranch = Statement();
        }

        return new Statement.If(condition, thenBranch, elseBranch);
    }

    private Statement.Inline Inline()
    {
        Expression expr = Expression();
        Consume(SyntaxKind.SEMICOLON, "Expect ';' after value.");

        return new Statement.Inline(expr);
    }

    private bool IsAtEnd()
    {
        return Peek().Kind == SyntaxKind.EOF;
    }

    private bool Match(params SyntaxKind[] types)
    {
        if (Array.Exists(types, Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool MatchNext(params SyntaxKind[] types)
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

        while (Match(SyntaxKind.SLASH, SyntaxKind.STAR))
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

        while (Match(SyntaxKind.OR))
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
        if (MatchNext(SyntaxKind.PLUS_PLUS, SyntaxKind.MINUS_MINUS))
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
        if (Match(SyntaxKind.BANG, SyntaxKind.MINUS))
        {
            Token opp = Previous();
            Expression right = Prefix();
            return new Expression.Prefix(opp, right);
        }

        return Call();
    }

    private Token Previous(int amount = 1)
    {
        return tokens[_current - amount];
    }

    private Expression Primary()
    {
        if (Match(SyntaxKind.FALSE)) return new Expression.Literal(false);
        if (Match(SyntaxKind.TRUE)) return new Expression.Literal(true);
        if (Match(SyntaxKind.NIL)) return new Expression.Literal(null);

        if (Match(SyntaxKind.NUMBER, SyntaxKind.STRING))
        {
            return new Expression.Literal(Previous().Literal);
        }

        if (Match(SyntaxKind.SUPER))
        {
            Token keyword = Previous();
            Consume(SyntaxKind.DOT, "Expect '.' after 'super'.");
            Token method = Consume([SyntaxKind.CONSTRUCTOR, SyntaxKind.IDENTIFIER], "Expect superclass method name after '.'");

            return new Expression.Super(keyword, method);
        }

        if (Match(SyntaxKind.THIS)) return new Expression.This(Previous());

        if (Match(SyntaxKind.IDENTIFIER))
        {
            return new Expression.Variable(Previous());
        }

        if (Match(SyntaxKind.LEFT_PAREN))
        {
            Expression expr = Expression();
            Consume(SyntaxKind.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expression.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Statement.Return ReturnStatement()
    {
        Token keyword = Previous();
        Expression? value = null;

        if (!Check(SyntaxKind.SEMICOLON))
        {
            value = Expression();
        }
        Consume(SyntaxKind.SEMICOLON, "Expect ';' after return value.");

        return new Statement.Return(keyword, value);
    }

    /// <summary>
    /// Determines the next statement, and calls the appropriate constructing function.
    /// </summary>
    /// <returns></returns>
    private Statement Statement()
    {
        if (Match(SyntaxKind.FOR)) return ForStatement();
        if (Match(SyntaxKind.IF)) return IfStatement();
        if (Match(SyntaxKind.RETURN)) return ReturnStatement();
        if (Match(SyntaxKind.WHILE)) return WhileStatement();
        if (Match(SyntaxKind.LEFT_BRACE)) return new Statement.Block(Block());

        return Inline();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Kind == SyntaxKind.SEMICOLON) return;

            switch (Peek().Kind)
            {
                case SyntaxKind.CLASS:
                case SyntaxKind.FUN:
                case SyntaxKind.VAR:
                case SyntaxKind.FOR:
                case SyntaxKind.IF:
                case SyntaxKind.WHILE:
                case SyntaxKind.RETURN:
                    return;
            }

            Advance();
        }
    }

    private Expression.Ternary Ternary(Expression condition)
    {
        Expression trueBranch = Expression();
        Consume(SyntaxKind.COLON, "Expect ':' after true branch of ternary operator.");
        Expression falseBranch = Assignment();
        return new Expression.Ternary(condition, trueBranch, falseBranch);
    }

    private Expression Unary()
    {
        if (Match(SyntaxKind.BANG, SyntaxKind.MINUS))
        {
            Token opp = Previous();
            Expression right = Unary();
            return new Expression.Prefix(opp, right);
        }

        return Postfix();
    }

    private Statement.Var Var()
    {
        Token name = Consume(SyntaxKind.IDENTIFIER, "Expect variable name.");

        Expression? initializer = null;
        if (Match(SyntaxKind.EQUAL))
        {
            initializer = Expression();
        }

        Consume(SyntaxKind.SEMICOLON, "Expect ';' after variable declaration.");
        return new Statement.Var(name, initializer);
    }

    private Statement.While WhileStatement()
    {
        var paren = Consume(SyntaxKind.LEFT_PAREN, "Expect '(' after 'while'.");
        Expression condition = Expression();

        EnsureNotTernaryOperator(condition, paren);
        Consume(SyntaxKind.RIGHT_PAREN, "Expect ')' after condition.");

        Statement body = Statement();

        return new Statement.While(condition, body);
    }
}