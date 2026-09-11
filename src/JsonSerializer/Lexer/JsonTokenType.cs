namespace JsonSerializer.Lexer;

public enum JsonTokenType
{
    EndOfFile,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Colon,
    Comma,
    String,
    Number,
    Boolean,
    Null,
    Unknown
}
