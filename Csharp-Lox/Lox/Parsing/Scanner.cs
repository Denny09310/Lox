namespace Lox;

internal class Scanner(string source)
{
    private readonly Dictionary<string, SyntaxKind> keywords = new()
    {
        { "and", SyntaxKind.AND },
        { "class", SyntaxKind.CLASS },
        { "else", SyntaxKind.ELSE },
        { "false", SyntaxKind.FALSE },
        { "for", SyntaxKind.FOR },
        { "fun", SyntaxKind.FUN },
        { "if", SyntaxKind.IF },
        { "nil", SyntaxKind.NIL },
        { "or", SyntaxKind.OR },
        { "return", SyntaxKind.RETURN },
        { "super", SyntaxKind.SUPER },
        { "this", SyntaxKind.THIS },
        { "true", SyntaxKind.TRUE },
        { "var", SyntaxKind.VAR },
        { "while", SyntaxKind.WHILE }
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

        _tokens.Add(new Token(SyntaxKind.EOF, "", null, _line));
        return _tokens;
    }

    /// <summary>
    /// Adds the token.
    /// </summary>
    /// <param name="type">The token type.</param>
    private void AddToken(SyntaxKind type)
    {
        AddToken(type, null);
    }

    /// <summary>
    /// Adds the token.
    /// </summary>
    /// <param name="type">The token type.</param>
    /// <param name="literal">The token literal.</param>
    private void AddToken(SyntaxKind type, object? literal)
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

        if (!keywords.TryGetValue(text, out SyntaxKind type))
        {
            type = SyntaxKind.IDENTIFIER;
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
        AddToken(SyntaxKind.STRING, value);
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

        AddToken(SyntaxKind.NUMBER, double.Parse(source[_start.._current]));
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
            case '(': AddToken(SyntaxKind.LEFT_PAREN); break;
            case ')': AddToken(SyntaxKind.RIGHT_PAREN); break;
            case '{': AddToken(SyntaxKind.LEFT_BRACE); break;
            case '}': AddToken(SyntaxKind.RIGHT_BRACE); break;
            case ',': AddToken(SyntaxKind.COMMA); break;
            case '.': AddToken(SyntaxKind.DOT); break;
            case '-': AddToken(Match('-') ? SyntaxKind.MINUS_MINUS : SyntaxKind.MINUS); break;
            case '+': AddToken(Match('+') ? SyntaxKind.PLUS_PLUS : SyntaxKind.PLUS); break;
            case ';': AddToken(SyntaxKind.SEMICOLON); break;
            case '*': AddToken(SyntaxKind.STAR); break;

            case '!': AddToken(Match('=') ? SyntaxKind.BANG_EQUAL : SyntaxKind.BANG); break;
            case '=': AddToken(Match('=') ? SyntaxKind.EQUAL_EQUAL : SyntaxKind.EQUAL); break;
            case '<': AddToken(Match('=') ? SyntaxKind.LESS_EQUAL : SyntaxKind.LESS); break;
            case '>': AddToken(Match('=') ? SyntaxKind.GREATER_EQUAL : SyntaxKind.GREATER); break;

            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(SyntaxKind.SLASH);
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