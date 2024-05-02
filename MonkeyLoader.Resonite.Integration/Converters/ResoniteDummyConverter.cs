using Elements.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Converters
{
    /// <summary>
    /// "Converts" <see cref="dummy"/> values to and from json.
    /// </summary>
    public sealed class ResoniteDummyConverter : JsonConverter<dummy>
    {
        /// <inheritdoc/>
        public override dummy ReadJson(JsonReader reader, Type objectType, dummy existingValue, bool hasExistingValue, JsonSerializer serializer)
            => default;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, dummy value, JsonSerializer serializer)
            => writer.WriteNull();
    }
}