namespace Lox;

internal class Scanner(string source)
{
    private readonly Dictionary<string, TokenType> keywords = new()
    {
        { "and", TokenType.AND },
        { "class", TokenType.CLASS },
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "for", TokenType.FOR },
        { "fun", TokenType.FUN },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "or", TokenType.OR },
        { "print", TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super", TokenType.SUPER },
        { "this", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR },
        { "while", TokenType.WHILE }
    };

    private readonly List<Token> _tokens = [];

    private int _current = 0;
    private int _line = 1;
    private int _start = 0;

    /// <summary>
    /// Scans and parses the tokens.
    /// </summary>
    /// <returns>List of parsed tokens</returns>
    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    /// <summary>
    /// Adds the token.
    /// </summary>
    /// <param name="type">The token type.</param>
    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    /// <summary>
    /// Adds the token.
    /// </summary>
    /// <param name="type">The token type.</param>
    /// <param name="literal">The token literal.</param>
    private void AddToken(TokenType type, object? literal)
    {
        string text = source[_start.._current];
        _tokens.Add(new Token(type, text, literal, _line));
    }

    /// <summary>
    /// Consumes the current character and advances the index.
    /// </summary>
    /// <returns>Character consumed</returns>
    private char Advance()
    {
        _current++;
        return source[_current - 1];
    }

    /// <summary>
    /// Parses and adds an identifier OR keyword token to the list.
    /// </summary>
    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        string text = source[_start.._current];

        if (!keywords.TryGetValue(text, out TokenType type))
        {
            type = TokenType.IDENTIFIER;
        }

        AddToken(type);
    }

    /// <summary>
    /// Determines whether the specified char is alpha.
    /// </summary>
    /// <param name="c">The char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is alpha; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c == '_');
    }

    /// <summary>
    /// Determines whether the specifed char is alpha numeric.
    /// </summary>
    /// <param name="c">The char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is alpha numeric; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    /// <summary>
    /// Determines whether [is at end] of source.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if [is at end]; otherwise, <c>false</c>.
    /// </returns>
    private bool IsAtEnd()
    {
        return _current >= source.Length;
    }

    /// <summary>
    /// Determines whether the specified char is a digit.
    /// </summary>
    /// <param name="c">The char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is digit; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    /// <summary>
    /// Parses and adds a string token.
    /// </summary>
    private void Lox_string()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        //Unterminated string
        if (IsAtEnd())
        {
            Lox.Error(_line, "Unterminated string.");
            return;
        }

        //consume closing "
        Advance();

        string value = source.Substring(_start + 1, _current - _start - 2);
        AddToken(TokenType.STRING, value);
    }

    /// <summary>
    /// If the current character matches the expected character, then consume it and return true.
    /// </summary>
    /// <param name="expected">The expected character.</param>
    /// <returns>true if match, false otherwise</returns>
    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[_current] != expected) return false;

        _current++;
        return true;
    }

    /// <summary>
    /// Parses and adds a number token to the list.
    /// </summary>
    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        //look for decimal part
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            //consume the '.'
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.NUMBER, double.Parse(source[_start.._current]));
    }

    /// <summary>
    /// Peeks at the next character, without advancing the counter.
    /// </summary>
    /// <returns></returns>
    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return source[_current];
    }

    /// <summary>
    /// Peeks at the next, next character, without advancing the counter.
    /// </summary>
    /// <returns></returns>
    private char PeekNext()
    {
        if (_current + 1 >= source.Length) return '\0';
        return source[_current + 1];
    }

    /// <summary>
    /// Scans the next token and adds it to the list.
    /// </summary>
    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(Match('-') ? TokenType.MINUS_MINUS : TokenType.MINUS); break;
            case '+': AddToken(Match('+') ? TokenType.PLUS_PLUS : TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;

            case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }
                break;

            case ' ':
            case '\r':
            case '\t':
                //ignore whitespace
                break;

            case '\n':
                _line++;
                break;

            case '"': Lox_string(); break;

            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Lox.Error(_line, "Unexpected character.");
                }

                break;
        }
    }
}