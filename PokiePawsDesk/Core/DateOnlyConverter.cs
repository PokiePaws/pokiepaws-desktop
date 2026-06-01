using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokiePawsDesk.Core
{
    public class DateOnlyConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            var s = reader.GetString();
            if (s == null) return null;
            if (DateTime.TryParse(s, out var dt))
                return dt;
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}