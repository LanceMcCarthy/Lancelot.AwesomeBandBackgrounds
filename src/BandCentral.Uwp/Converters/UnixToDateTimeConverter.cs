using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BandCentral.Uwp.Converters
{
    public class UnixToDateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long ticks;
            if(value is DateTime time)
            {
                var epoc = new DateTime(1970, 1, 1);
                var delta = time - epoc;
                if(delta.TotalSeconds < 0)
                {
                    //Unix epoc starts January 1st, 1970
                    throw new ArgumentOutOfRangeException(nameof(writer));
                }
                ticks = (long)delta.TotalSeconds;
            }
            else
            {
                throw new Exception("Expected date object value.");
            }
            writer.WriteValue(ticks);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType != JsonToken.String)
            {
                throw new Exception($"Unexpected token parsing date. Expected String, got {reader.TokenType}.");
            }

            var date = new DateTime(1970, 1, 1);

            if (long.TryParse((string)reader.Value, out var val))
            {
                var ticks = val;
                date = date.AddSeconds(ticks);
            }

            return date.ToString("f");

        }
    }
}
