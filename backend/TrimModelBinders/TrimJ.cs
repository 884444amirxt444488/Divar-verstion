
using System.Text.Json;
using System.Text.Json.Serialization;

public class TrimJsonModel : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.None)
        {
            return null;
        }
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Not string");
        }

        string? Result = reader.GetString();
        if (Result == null)
        {   
            return null;
        }

        return Result.Trim();
    }
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}




