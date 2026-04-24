using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackendAPI.Converters
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats = 
        { 
            "dd/MM/yyyy", 
            "dd/MM/yyyy HH:mm:ss", 
            "yyyy-MM-ddTHH:mm:ss.fffZ", 
            "yyyy-MM-ddTHH:mm:ssZ", 
            "yyyy-MM-dd"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (DateTime.TryParseExact(value, _formats, null, System.Globalization.DateTimeStyles.None, out var date))
            {
                return date;
            }
            return DateTime.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("dd/MM/yyyy HH:mm:ss"));
        }
    }
}