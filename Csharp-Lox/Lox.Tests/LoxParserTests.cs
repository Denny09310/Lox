namespace Lox.Tests;

public class LoxParserTests
{
    [Fact]
    public void LoxParseExpression()
    {
        const string filepath = @".\Files\expression.lox";
        int result = Lox.RunFile(filepath);
        Assert.Equal(0, result);
    }
}