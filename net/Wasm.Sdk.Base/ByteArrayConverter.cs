using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wasm.Sdk
{
    public class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var byteList = new List<byte>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.Number:
                            var b = reader.BytesConsumed;
                            byteList.Add(reader.GetByte());
                            break;
                        case JsonTokenType.EndArray:
                            return byteList.ToArray();
                        case JsonTokenType.Comment:
                            // skip
                            break;
                        default:
                            throw new Exception(
                            string.Format(
                                "Unexpected token when reading bytes: {0}",
                                reader.TokenType));
                    }
                }

                throw new Exception("Unexpected end when reading bytes.");
            }
            else
            {
                throw new Exception(
                    string.Format(
                        "Unexpected token parsing binary. "
                        + "Expected StartArray, got {0}.",
                        reader.TokenType));
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            byte[] byteArray,
            JsonSerializerOptions options)
        {
            if (byteArray == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Compose an array.
            writer.WriteStartArray();

            for (var i = 0; i < byteArray.Length; i++)
            {
                writer.WriteNumberValue(byteArray[i]);
            }

            writer.WriteEndArray();
        }
    }
}
