namespace JsonSerializer.Models;

public abstract class JsonValue
{
}

public class JsonObjectValue : JsonValue
{
    public Dictionary<string, JsonValue> Values { get; } = new();
}

public class JsonArrayValue : JsonValue
{
    public List<JsonValue> Values { get; } = [];
}

public class JsonStringValue(string value) : JsonValue
{
    public string Value { get; } = value;
}

public class JsonNumberValue(double value) : JsonValue
{
    public double Value { get; } = value;
}

public class JsonBooleanValue(bool value) : JsonValue
{
    public bool Value { get; } = value;
}

public class JsonNullValue : JsonValue
{
}
