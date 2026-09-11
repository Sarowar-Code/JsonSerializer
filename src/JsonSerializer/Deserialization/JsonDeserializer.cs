using System.Globalization;
using System.Reflection;
using JsonSerializer.Exceptions;
using JsonSerializer.Lexer;
using JsonSerializer.Models;
using JsonSerializer.Parsing;

namespace JsonSerializer.Deserialization;

public class JsonDeserializer
{
    public T Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException("JSON input cannot be empty.");
        }

        var lexer = new JsonLexer(json);
        var tokens = lexer.Tokenize();
        var parser = new JsonParser(tokens);
        var parsed = parser.Parse();

        return ConvertValue<T>(parsed);
    }

    private T ConvertValue<T>(JsonValue value)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)((JsonStringValue)value).Value;
        }

        if (typeof(T) == typeof(int))
        {
            return (T)(object)Convert.ToInt32(((JsonNumberValue)value).Value);
        }

        if (typeof(T) == typeof(double))
        {
            return (T)(object)((JsonNumberValue)value).Value;
        }

        if (typeof(T) == typeof(bool))
        {
            return (T)(object)((JsonBooleanValue)value).Value;
        }

        if (typeof(T) == typeof(List<int>))
        {
            var array = (JsonArrayValue)value;
            var list = new List<int>();
            foreach (var item in array.Values)
            {
                list.Add(Convert.ToInt32(((JsonNumberValue)item).Value));
            }
            return (T)(object)list;
        }

        if (typeof(T) == typeof(List<double>))
        {
            var array = (JsonArrayValue)value;
            var list = new List<double>();
            foreach (var item in array.Values)
            {
                list.Add(((JsonNumberValue)item).Value);
            }
            return (T)(object)list;
        }

        if (typeof(T) == typeof(List<string>))
        {
            var array = (JsonArrayValue)value;
            var list = new List<string>();
            foreach (var item in array.Values)
            {
                list.Add(((JsonStringValue)item).Value);
            }
            return (T)(object)list;
        }

        if (typeof(T) == typeof(List<bool>))
        {
            var array = (JsonArrayValue)value;
            var list = new List<bool>();
            foreach (var item in array.Values)
            {
                list.Add(((JsonBooleanValue)item).Value);
            }
            return (T)(object)list;
        }

        object result = Activator.CreateInstance(typeof(T))!;

        if (value is JsonObjectValue obj)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!obj.Values.TryGetValue(property.Name, out var propertyValue))
                {
                    continue;
                }

                if (propertyValue is JsonNullValue)
                {
                    property.SetValue(result, null);
                    continue;
                }

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(result, ((JsonStringValue)propertyValue).Value);
                }
                else if (property.PropertyType == typeof(int))
                {
                    property.SetValue(result, Convert.ToInt32(((JsonNumberValue)propertyValue).Value));
                }
                else if (property.PropertyType == typeof(double))
                {
                    property.SetValue(result, ((JsonNumberValue)propertyValue).Value);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(result, ((JsonBooleanValue)propertyValue).Value);
                }
                else if (property.PropertyType.IsArray)
                {
                    property.SetValue(result, ConvertArray(property.PropertyType, propertyValue));
                }
                else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    property.SetValue(result, ConvertList(property.PropertyType, propertyValue));
                }
            }

            return (T)result;
        }

        throw new JsonException("Unsupported type for deserialization: " + typeof(T).FullName);
    }

    private object ConvertArray(Type elementType, JsonValue value)
    {
        var arrayValue = (JsonArrayValue)value;
        var array = Array.CreateInstance(elementType.GetElementType()!, arrayValue.Values.Count);

        for (var i = 0; i < arrayValue.Values.Count; i++)
        {
            var item = arrayValue.Values[i];
            if (item is JsonNumberValue number)
            {
                array.SetValue(Convert.ChangeType(number.Value, elementType.GetElementType()!, CultureInfo.InvariantCulture), i);
            }
            else if (item is JsonStringValue text)
            {
                array.SetValue(text.Value, i);
            }
            else if (item is JsonBooleanValue boolean)
            {
                array.SetValue(boolean.Value, i);
            }
        }

        return array;
    }

    private object ConvertList(Type listType, JsonValue value)
    {
        var arrayValue = (JsonArrayValue)value;
        var elementType = listType.GetGenericArguments()[0];
        var list = Activator.CreateInstance(listType)!;

        var addMethod = listType.GetMethod("Add")!;
        foreach (var item in arrayValue.Values)
        {
            if (elementType == typeof(int) && item is JsonNumberValue number)
            {
                addMethod.Invoke(list, [Convert.ToInt32(number.Value)]);
            }
            else if (elementType == typeof(double) && item is JsonNumberValue number2)
            {
                addMethod.Invoke(list, [number2.Value]);
            }
            else if (elementType == typeof(string) && item is JsonStringValue text)
            {
                addMethod.Invoke(list, [text.Value]);
            }
            else if (elementType == typeof(bool) && item is JsonBooleanValue boolean)
            {
                addMethod.Invoke(list, [boolean.Value]);
            }
        }

        return list;
    }
}
