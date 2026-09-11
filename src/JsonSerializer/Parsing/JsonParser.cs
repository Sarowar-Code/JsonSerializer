using JsonSerializer.Exceptions;
using JsonSerializer.Lexer;
using JsonSerializer.Models;

namespace JsonSerializer.Parsing;

public class JsonParser(List<JsonToken> tokens)
{
    private int _position;

    public JsonValue Parse()
    {
        var value = ParseValue();
        Expect(JsonTokenType.EndOfFile);
        return value;
    }

    private JsonValue ParseValue()
    {
        var token = Peek();

        if (token == null)
        {
            throw new JsonException("Unexpected end of JSON input.");
        }

        switch (token.Type)
        {
            case JsonTokenType.LeftBrace:
                return ParseObject();
            case JsonTokenType.LeftBracket:
                return ParseArray();
            case JsonTokenType.String:
                var stringToken = Read();
                if (stringToken == null)
                {
                    throw new JsonException("Unexpected end of JSON input.");
                }
                return new JsonStringValue(stringToken.Text);
            case JsonTokenType.Number:
                var numberToken = Read();
                if (numberToken == null)
                {
                    throw new JsonException("Unexpected end of JSON input.");
                }
                return new JsonNumberValue(double.Parse(numberToken.Text));
            case JsonTokenType.Boolean:
                var booleanToken = Read();
                if (booleanToken == null)
                {
                    throw new JsonException("Unexpected end of JSON input.");
                }
                return new JsonBooleanValue(bool.Parse(booleanToken.Text));
            case JsonTokenType.Null:
                Read();
                return new JsonNullValue();
            default:
                throw new JsonException("Unexpected token while parsing JSON value: " + token.Type);
        }
    }

    private JsonObjectValue ParseObject()
    {
        var result = new JsonObjectValue();
        Expect(JsonTokenType.LeftBrace);

        if (Match(JsonTokenType.RightBrace))
        {
            return result;
        }

        while (true)
        {
            var keyToken = Expect(JsonTokenType.String);
            Expect(JsonTokenType.Colon);
            var value = ParseValue();

            result.Values[keyToken.Text] = value;

            if (Match(JsonTokenType.Comma))
            {
                continue;
            }

            Expect(JsonTokenType.RightBrace);
            return result;
        }
    }

    private JsonArrayValue ParseArray()
    {
        var result = new JsonArrayValue();
        Expect(JsonTokenType.LeftBracket);

        if (Match(JsonTokenType.RightBracket))
        {
            return result;
        }

        while (true)
        {
            result.Values.Add(ParseValue());

            if (Match(JsonTokenType.Comma))
            {
                continue;
            }

            Expect(JsonTokenType.RightBracket);
            return result;
        }
    }

    private JsonToken Expect(JsonTokenType type)
    {
        var token = Peek();

        if (token == null || token.Type != type)
        {
            throw new JsonException("Expected token " + type + ", but found " + (token == null ? "end of input" : token.Type.ToString()));
        }

        _position++;
        return token;
    }

    private bool Match(JsonTokenType type)
    {
        var token = Peek();

        if (token != null && token.Type == type)
        {
            _position++;
            return true;
        }

        return false;
    }

    private JsonToken? Read()
    {
        if (_position >= tokens.Count)
        {
            return null;
        }

        var token = tokens[_position];
        _position++;
        return token;
    }

    private JsonToken? Peek()
    {
        if (_position >= tokens.Count)
        {
            return null;
        }

        return tokens[_position];
    }
}
