using JsonSerializer.Exceptions;

namespace JsonSerializer.Lexer;

public class JsonLexer(string input)
{
    private readonly string _input = input ?? throw new ArgumentNullException(nameof(input));
    private int _position;

    public List<JsonToken> Tokenize()
    {
        var tokens = new List<JsonToken>();

        while (_position < _input.Length)
        {
            var character = _input[_position];

            if (char.IsWhiteSpace(character))
            {
                _position++;
                continue;
            }

            if (character == '{')
            {
                tokens.Add(new JsonToken(JsonTokenType.LeftBrace, character.ToString()));
                _position++;
                continue;
            }

            if (character == '}')
            {
                tokens.Add(new JsonToken(JsonTokenType.RightBrace, character.ToString()));
                _position++;
                continue;
            }

            if (character == '[')
            {
                tokens.Add(new JsonToken(JsonTokenType.LeftBracket, character.ToString()));
                _position++;
                continue;
            }

            if (character == ']')
            {
                tokens.Add(new JsonToken(JsonTokenType.RightBracket, character.ToString()));
                _position++;
                continue;
            }

            if (character == ':')
            {
                tokens.Add(new JsonToken(JsonTokenType.Colon, character.ToString()));
                _position++;
                continue;
            }

            if (character == ',')
            {
                tokens.Add(new JsonToken(JsonTokenType.Comma, character.ToString()));
                _position++;
                continue;
            }

            if (character == '"')
            {
                tokens.Add(ReadString());
                continue;
            }

            if (char.IsDigit(character) || character == '-')
            {
                tokens.Add(ReadNumber());
                continue;
            }

            if (character == 't' || character == 'f')
            {
                tokens.Add(ReadBoolean());
                continue;
            }

            if (character == 'n')
            {
                tokens.Add(ReadNull());
                continue;
            }

            throw new JsonException("Unexpected character in JSON input: " + character);
        }

        tokens.Add(new JsonToken(JsonTokenType.EndOfFile, string.Empty));
        return tokens;
    }

    private JsonToken ReadString()
    {
        _position++; // skip opening quote
        var builder = new System.Text.StringBuilder();

        while (_position < _input.Length)
        {
            var current = _input[_position];

            if (current == '"')
            {
                _position++;
                return new JsonToken(JsonTokenType.String, builder.ToString());
            }

            if (current == '\\')
            {
                _position++;

                if (_position >= _input.Length)
                {
                    throw new JsonException("Invalid escape sequence in JSON string.");
                }

                var escape = _input[_position];

                switch (escape)
                {
                    case '"':
                        builder.Append('"');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    case '/':
                        builder.Append('/');
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    default:
                        throw new JsonException("Unsupported escape sequence: \\" + escape);
                }

                _position++;
                continue;
            }

            builder.Append(current);
            _position++;
        }

        throw new JsonException("Unterminated string in JSON input.");
    }

    private JsonToken ReadNumber()
    {
        var start = _position;

        if (_input[_position] == '-')
        {
            _position++;
        }

        while (_position < _input.Length && char.IsDigit(_input[_position]))
        {
            _position++;
        }

        if (_position < _input.Length && _input[_position] == '.')
        {
            _position++;

            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }
        }

        if (_position < _input.Length && (_input[_position] == 'e' || _input[_position] == 'E'))
        {
            _position++;

            if (_position < _input.Length && (_input[_position] == '+' || _input[_position] == '-'))
            {
                _position++;
            }

            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }
        }

        var text = _input.Substring(start, _position - start);
        return new JsonToken(JsonTokenType.Number, text);
    }

    private JsonToken ReadBoolean()
    {
        if (_input.Substring(_position).StartsWith("true", StringComparison.Ordinal))
        {
            _position += 4;
            return new JsonToken(JsonTokenType.Boolean, "true");
        }

        if (_input.Substring(_position).StartsWith("false", StringComparison.Ordinal))
        {
            _position += 5;
            return new JsonToken(JsonTokenType.Boolean, "false");
        }

        throw new JsonException("Invalid boolean value.");
    }

    private JsonToken ReadNull()
    {
        if (_input.Substring(_position).StartsWith("null", StringComparison.Ordinal))
        {
            _position += 4;
            return new JsonToken(JsonTokenType.Null, "null");
        }

        throw new JsonException("Invalid null value.");
    }
}
