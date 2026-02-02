using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESsample.Banking.API.Infrastructure.Converters;

public class SystemTypeJsonConverter : JsonConverter<Type>
{
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? typeName = reader.GetString() ?? throw new JsonException("Type name is null in the JSON.");

        Type? type = Type.GetType(typeName);

        return type ?? throw new JsonException($"Unable to resolve type: {typeName}");
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        writer.WriteStringValue(value.Name);
    }
}
