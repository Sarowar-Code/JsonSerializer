namespace JsonSerializer.Lexer;

public class JsonToken(JsonTokenType type, string text)
{
    public JsonTokenType Type { get; } = type;
    public string Text { get; } = text;

}
