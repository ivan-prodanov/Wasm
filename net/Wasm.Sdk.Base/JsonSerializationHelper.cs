using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wasm.Sdk
{
    public static class JsonSerializationHelper
    {
        static JsonSerializerOptions options;
        static JsonSerializationHelper()
        {
            options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new NumberToCharJsonConverter());
            options.Converters.Add(new ByteArrayConverter());
        }

        public static T Deserialize<T>(string input, string parameterName)
        {
            try
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(input, options);

                return deserializedValue;
            }
            catch (Exception)
            {
                Console.Error.WriteLine($"Failed to deserialize the parameter {parameterName} into {typeof(T)}. \nParameter value: {input}");
                throw;
            }
        }

        public static string Serialize<T>(T input, string methodName)
        {
            try
            {
                var deserializedValue = JsonSerializer.Serialize(input, options);
                return deserializedValue;
            }
            catch (Exception)
            {
                Console.Error.WriteLine($"Failed to serialize the result of method {methodName} of type {typeof(T)}. \nMethod result: {input}");
                throw;
            }
        }
    }
}