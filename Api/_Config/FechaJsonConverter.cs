using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Core.DTOs;

namespace Api._Config;

/// <summary>
/// Deserializa <see cref="FechaDTO"/> (abstracto) según el discriminador <c>tipo</c>,
/// o infiriendo por <c>numero</c> / <c>instanciaId</c> si falta <c>tipo</c> (compat. payloads viejos).
/// </summary>
public class FechaJsonConverter : JsonConverter<FechaDTO>
{
    public override FechaDTO Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var inner = InnerOptions(options);

        if (TryGetStringPropertyInsensitive(root, options, "tipo", out var tipo) && tipo is not null)
        {
            return tipo switch
            {
                "todosContraTodos" => JsonSerializer.Deserialize<FechaTodosContraTodosDTO>(root.GetRawText(), inner)!,
                "eliminacionDirecta" => JsonSerializer.Deserialize<FechaEliminacionDirectaDTO>(root.GetRawText(), inner)!,
                _ => throw new JsonException($"tipo de fecha desconocido: {tipo}")
            };
        }

        if (HasPropertyInsensitive(root, options, "numero"))
            return JsonSerializer.Deserialize<FechaTodosContraTodosDTO>(root.GetRawText(), inner)!;

        if (HasPropertyInsensitive(root, options, "instanciaId"))
            return JsonSerializer.Deserialize<FechaEliminacionDirectaDTO>(root.GetRawText(), inner)!;

        if (TryDeserializeConcrete(root.GetRawText(), inner, out var inferred))
            return inferred;

        throw new JsonException("Se requiere 'tipo' o las propiedades 'numero' (todos contra todos) o 'instanciaId' (eliminación directa).");
    }

    public override void Write(Utf8JsonWriter writer, FechaDTO value, JsonSerializerOptions options)
    {
        var inner = InnerOptions(options);
        var json = JsonSerializer.Serialize(value, value.GetType(), inner);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var tipoDiscriminator = value switch
        {
            FechaTodosContraTodosDTO => "todosContraTodos",
            FechaEliminacionDirectaDTO => "eliminacionDirecta",
            _ => throw new JsonException($"Tipo de fecha no soportado: {value.GetType().Name}")
        };

        var tipoPropName = GetName(options, "tipo");
        writer.WriteStartObject();
        writer.WriteString(tipoPropName, tipoDiscriminator);
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Name == tipoPropName)
                continue;
            writer.WritePropertyName(prop.Name);
            prop.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private static JsonSerializerOptions InnerOptions(JsonSerializerOptions options)
    {
        var inner = new JsonSerializerOptions(options);
        for (var i = inner.Converters.Count - 1; i >= 0; i--)
        {
            if (inner.Converters[i] is FechaJsonConverter)
                inner.Converters.RemoveAt(i);
        }

        return inner;
    }

    private static string GetName(JsonSerializerOptions options, string name) =>
        options.PropertyNamingPolicy?.ConvertName(name) ?? name;

    private static bool HasPropertyInsensitive(JsonElement root, JsonSerializerOptions options, string logicalName)
    {
        var camel = GetName(options, logicalName);
        foreach (var prop in root.EnumerateObject())
        {
            if (string.Equals(prop.Name, logicalName, StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(prop.Name, camel, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool TryGetStringPropertyInsensitive(JsonElement root, JsonSerializerOptions options,
        string logicalName, out string? value)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (!string.Equals(prop.Name, GetName(options, logicalName), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(prop.Name, logicalName, StringComparison.OrdinalIgnoreCase))
                continue;
            value = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.GetRawText();
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryDeserializeConcrete(string json, JsonSerializerOptions inner, out FechaDTO dto)
    {
        try
        {
            var tct = JsonSerializer.Deserialize<FechaTodosContraTodosDTO>(json, inner);
            if (tct != null)
            {
                dto = tct;
                return true;
            }
        }
        catch (JsonException)
        {
            /* siguiente */
        }

        try
        {
            var ed = JsonSerializer.Deserialize<FechaEliminacionDirectaDTO>(json, inner);
            if (ed != null)
            {
                dto = ed;
                return true;
            }
        }
        catch (JsonException)
        {
            /* fallar */
        }

        dto = null!;
        return false;
    }
}
