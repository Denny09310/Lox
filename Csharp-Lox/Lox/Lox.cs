namespace Lox;

public static class Lox
{
    private static readonly Interpreter interpreter = new();

    private static bool hadError = false;
    private static bool hadRuntimeError = false;

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.lexeme + "'", message);
        }
    }

    public static void Exit(int code)
    {
        System.Environment.Exit(code);
    }

    public static int RunFile(string path)
    {
        StreamReader sr = new(path);

        Run(sr.ReadToEnd());

        if (hadError) return 65;
        if (hadRuntimeError) return 70;

        return 0;
    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var source = Console.ReadLine();
            if (source == null) continue;
            Run(source);
            hadError = false;
        }
    }

    public static void RuntimeError(RuntimeError runtimeError)
    {
        Console.Error.WriteLine(runtimeError.Message + "\n[line " + runtimeError.Token?.line + "]");
        hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine("[line " + line.ToString() + "] Error" + where + ": " + message);
        hadError = true;
    }

    private static void Run(string source)
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new(tokens);
        List<Statement> statements = parser.Parse();

        //stop if there was a syntax error.
        if (hadError) return;

        Resolver resolver = new(interpreter);
        resolver.Resolve(statements);

        //stop if there was a resolution error.
        if (hadError) return;

        interpreter.Interpret(statements);
    }
}