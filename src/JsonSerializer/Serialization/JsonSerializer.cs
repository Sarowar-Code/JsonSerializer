using System.Reflection;
using JsonSerializer.Exceptions;

namespace JsonSerializer.Serialization;

public class JsonSerializer
{
    public string Serialize(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        var type = value.GetType();

        if (type == typeof(string))
        {
            return QuoteString((string)value);
        }

        if (type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) || type == typeof(long))
        {
            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
        }

        if (type == typeof(bool))
        {
            return (bool)value ? "true" : "false";
        }

        if (type.IsArray)
        {
            return SerializeArray((Array)value);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return SerializeList((System.Collections.IEnumerable)value);
        }

        if (type.IsClass || type.IsValueType)
        {
            return SerializeObject(value);
        }

        throw new JsonException("Unsupported type for serialization: " + type.FullName);
    }

    private string SerializeArray(Array array)
    {
        var items = new List<string>();

        foreach (var item in array)
        {
            items.Add(Serialize(item));
        }

        return "[" + string.Join(",", items) + "]";
    }

    private string SerializeList(System.Collections.IEnumerable items)
    {
        var result = new List<string>();

        foreach (var item in items)
        {
            result.Add(Serialize(item));
        }

        return "[" + string.Join(",", result) + "]";
    }

    private string SerializeObject(object value)
    {
        var properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var result = new List<string>();

        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(value);
            result.Add(QuoteString(property.Name) + ":" + Serialize(propertyValue));
        }

        return "{" + string.Join(",", result) + "}";
    }

    private string QuoteString(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f");

        return "\"" + escaped + "\"";
    }
}
