using JsonSerializer.Deserialization;
using JsonSerializer.Lexer;
using JsonSerializer.Models;
using JsonSerializer.Parsing;

LexerTests.Run();
ParserTests.Run();
SerializationTests.Run();
DeserializationTests.Run();

Console.WriteLine("All tests passed.");

public static class LexerTests
{
    public static void Run()
    {
        AssertToken("123", JsonTokenType.Number, "123");
        AssertToken("\"hello\"", JsonTokenType.String, "hello");
        AssertToken("true", JsonTokenType.Boolean, "true");
        AssertToken("null", JsonTokenType.Null, "null");

        var tokens = new JsonLexer(" { \"name\" : \"John\" , [ ] } ").Tokenize();
        AssertTokenTypes(
            tokens,
            JsonTokenType.LeftBrace,
            JsonTokenType.String,
            JsonTokenType.Colon,
            JsonTokenType.String,
            JsonTokenType.Comma,
            JsonTokenType.LeftBracket,
            JsonTokenType.RightBracket,
            JsonTokenType.RightBrace,
            JsonTokenType.EndOfFile);

        var escapedString = new JsonLexer("\"line1\\nline2\\\"quote\"").Tokenize()[0];
        if (escapedString.Text != "line1\nline2\"quote")
        {
            throw new Exception("Lexer failed to decode escaped characters.");
        }
    }

    private static void AssertToken(string json, JsonTokenType expectedType, string expectedText)
    {
        var tokens = new JsonLexer(json).Tokenize();
        var token = tokens[0];

        if (token.Type != expectedType || token.Text != expectedText)
        {
            throw new Exception($"Lexer test failed for '{json}'. Expected {expectedType} ({expectedText}), got {token.Type} ({token.Text}).");
        }
    }

    private static void AssertTokenTypes(List<JsonToken> tokens, params JsonTokenType[] expectedTypes)
    {
        if (tokens.Count != expectedTypes.Length)
        {
            throw new Exception($"Expected {expectedTypes.Length} tokens, got {tokens.Count}.");
        }

        for (var i = 0; i < expectedTypes.Length; i++)
        {
            if (tokens[i].Type != expectedTypes[i])
            {
                throw new Exception($"Expected token {i} to be {expectedTypes[i]}, got {tokens[i].Type}.");
            }
        }
    }
}

public static class ParserTests
{
    public static void Run()
    {
        var emptyObject = new JsonParser(new JsonLexer("{}").Tokenize()).Parse();
        if (emptyObject is not JsonObjectValue)
            throw new Exception("Parser failed for empty object.");

        var objectValue = new JsonParser(new JsonLexer("{\"name\":\"John\"}").Tokenize()).Parse();
        if (objectValue is not JsonObjectValue)
            throw new Exception("Parser failed for object.");

        var arrayValue = new JsonParser(new JsonLexer("[1,2,3]").Tokenize()).Parse();
        if (arrayValue is not JsonArrayValue)
            throw new Exception("Parser failed for array.");

        var nestedValue = new JsonParser(new JsonLexer("{\"active\":true,\"items\":[1,null,\"two\"]}").Tokenize()).Parse();
        if (nestedValue is not JsonObjectValue nestedObject ||
            nestedObject.Values["active"] is not JsonBooleanValue { Value: true } ||
            nestedObject.Values["items"] is not JsonArrayValue { Values.Count: 3 })
        {
            throw new Exception("Parser failed for nested values.");
        }
    }
}

public static class SerializationTests
{
    public static void Run()
    {
        var serializer = new JsonSerializer.Serialization.JsonSerializer();
        var person = new Person { Name = "John", Age = 25 };
        var json = serializer.Serialize(person);

        if (json != "{\"Name\":\"John\",\"Age\":25}")
        {
            throw new Exception("Serialization test failed: " + json);
        }

        AssertSerialized(serializer.Serialize("hello\n\"world"), "\"hello\\n\\\"world\"");
        AssertSerialized(serializer.Serialize(new[] { 1, 2, 3 }), "[1,2,3]");
        AssertSerialized(serializer.Serialize(true), "true");
        AssertSerialized(serializer.Serialize(null), "null");
    }

    private static void AssertSerialized(string actual, string expected)
    {
        if (actual != expected)
        {
            throw new Exception($"Serialization test failed. Expected {expected}, got {actual}.");
        }
    }
}

public static class DeserializationTests
{
    public static void Run()
    {
        var deserializer = new JsonDeserializer();
        var result = deserializer.Deserialize<Person>("{\"Name\":\"John\",\"Age\":25}");

        if (result.Name != "John" || result.Age != 25)
        {
            throw new Exception("Deserialization test failed.");
        }

        var numbers = deserializer.Deserialize<List<int>>("[1,2,3]");
        if (numbers.Count != 3 || numbers[0] != 1 || numbers[2] != 3)
        {
            throw new Exception("Deserialization of integer list failed.");
        }

        var settings = deserializer.Deserialize<Settings>("{\"Enabled\":true,\"Tags\":[\"json\",\"csharp\"]}");
        if (!settings.Enabled || settings.Tags.Count != 2 || settings.Tags[1] != "csharp")
        {
            throw new Exception("Deserialization of object collections failed.");
        }
    }
}

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Settings
{
    public bool Enabled { get; set; }
    public List<string> Tags { get; set; } = [];
}
