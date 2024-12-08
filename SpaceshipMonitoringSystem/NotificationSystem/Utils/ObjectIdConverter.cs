using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace NotificationSystem.Utils;

public class ObjectIdConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(ObjectId);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Activator.CreateInstance<ObjectIdConverterInner>();
    }
}

public class ObjectIdConverterInner : JsonConverter<ObjectId>
{
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new Exception($"Unexpected token parsing objectId. Expected string, got {reader.TokenType}");
        }
        
        var value = reader.GetString();
        
        return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
    }

    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString() ?? string.Empty);
    }
}