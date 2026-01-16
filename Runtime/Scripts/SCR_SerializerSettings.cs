using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Core
{
    using static CoreUtility;

    public class SerializerSettings : MonoBehaviour
    {
        private class JsonConverterGuid : JsonConverter<Guid>
        {
            public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
            {
                writer.WriteValue(Convert.ToBase64String(value.ToByteArray()));
            }
            public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                string base64 = (string)reader.Value;
                return new Guid(Convert.FromBase64String(base64));
            }
        }
        private class JsonConverterGuidNull : JsonConverter<Guid?>
        {
            public override void WriteJson(JsonWriter writer, Guid? value, JsonSerializer serializer)
            {
                if (value.HasValue)
                {
                    writer.WriteValue(Convert.ToBase64String(value.Value.ToByteArray()));
                }
                else
                {
                    writer.WriteNull();
                }
            }
            public override Guid? ReadJson(JsonReader reader, Type objectType, Guid? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                string base64 = (string)reader.Value;
                return new Guid(Convert.FromBase64String(base64));
            }
        }


        public static readonly JsonSerializerSettings SETTINGS = new()
        {
            TypeNameHandling = TypeNameHandling.Auto, // See JsonConverter<T>, override ReadJson(), override WriteJson()
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            Converters = new JsonConverter[] { new JsonConverterGuid(), new JsonConverterGuidNull() },
        };
    }
}
