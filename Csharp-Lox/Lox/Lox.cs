namespace Lox;

public static class Lox
{
    private static readonly Interpreter _interpreter = new();

    private static bool _hadError = false;
    private static bool _hadRuntimeError = false;

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.Kind == SyntaxKind.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    public static void Exit(int code)
    {
        System.Environment.Exit(code);
    }

    public static int RunFile(string path)
    {
        using StreamReader sr = new(path);

        Run(sr.ReadToEnd());

        if (_hadError) return 65;
        if (_hadRuntimeError) return 70;

        return 0;
    }

    public static void RunPrompt()
    {
        const string ExitCommand = "exit";
        while (true)
        {
            Console.Write("> ");
            var source = Console.ReadLine();

            if (source == null) continue;
            if (source == ExitCommand) break;

            Run(source);
            _hadError = false;
        }
    }

    public static void RuntimeError(RuntimeException runtimeError)
    {
        Console.Error.WriteLine(runtimeError.Message + "\n[line " + runtimeError.Token?.Line + "]");
        _hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine("[line " + line.ToString() + "] Error" + where + ": " + message);
        _hadError = true;
    }

    private static void Run(string source)
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new(tokens);
        IList<Statement> statements = parser.Parse();

        //stop if there was a syntax error.
        if (_hadError) return;

        Resolver resolver = new(_interpreter);
        resolver.Resolve(statements);

        //stop if there was a resolution error.
        if (_hadError) return;

        _interpreter.Interpret(statements);
    }
}