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
            if(value is DateTime)
            {
                var epoc = new DateTime(1970, 1, 1);
                var delta = ((DateTime)value) - epoc;
                if(delta.TotalSeconds < 0)
                {
                    //Unix epoc starts January 1st, 1970
                    throw new ArgumentOutOfRangeException("writer");
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
                throw new Exception(
                    String.Format("Unexpected token parsing date. Expected String, got {0}.",
                    reader.TokenType));
            }

            var date = new DateTime(1970, 1, 1);

            long val;
            if (long.TryParse((string)reader.Value, out val))
            {
                var ticks = val;
                date = date.AddSeconds(ticks);
            }

            return date.ToString("f");

        }
    }
}
