using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wasm.Sdk
{
    class NumberToCharJsonConverter : JsonConverter<char>
    {
        public NumberToCharJsonConverter() { }

        public override char Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                // Backward compatibility '.' => '.'
                return reader.GetString()[0];
            }
            else
            {
                // Forward compatibility 46 => '.'
                return (char)reader.GetByte();
            }
        }

        public override void Write(Utf8JsonWriter writer, char value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
