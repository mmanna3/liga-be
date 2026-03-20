using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api._Config;

/// <summary>
/// Permite deserializar DateOnly desde ISO 8601 con hora (ej: "2026-03-28T03:00:00.000Z")
/// además del formato estándar "yyyy-MM-dd".
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        if (string.IsNullOrEmpty(s))
            return default;
        if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, out var date))
            return date;
        // Fallback para ISO 8601 con componente de hora (ej: "2026-03-21T03:00:00.000Z")
        return DateOnly.FromDateTime(DateTime.Parse(s, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind));
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
