namespace JsonSerializer;

public static class JsonLibrary
{
    public static string Serialize(object? value)
    {
        return new Serialization.JsonSerializer().Serialize(value);
    }

    public static T Deserialize<T>(string json)
    {
        return new Deserialization.JsonDeserializer().Deserialize<T>(json);
    }
}
