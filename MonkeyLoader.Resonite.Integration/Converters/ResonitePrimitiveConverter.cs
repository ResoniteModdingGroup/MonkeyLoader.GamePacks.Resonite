// Adapted from the NeosModLoader project.

using Elements.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ResoniteModLoader.JsonConverters
{
    /// <summary>
    /// Converts a Resonite Primitive to and from json.
    /// </summary>
    public sealed class ResonitePrimitiveConverter : JsonConverter
    {
        private static readonly Dictionary<Type, MethodInfo> encodeMethods = new();
        private static readonly Assembly primitivesAssembly = typeof(colorX).Assembly;

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            // handle all non-enum Resonite Primitives
            return !objectType.IsEnum && primitivesAssembly.Equals(objectType.Assembly) && Coder.IsEnginePrimitive(objectType);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string serialized)
            {
                // use Resonite's built-in decoding if the value was serialized as a string
                return typeof(Coder<>).MakeGenericType(objectType).GetMethod("DecodeFromString").Invoke(null, new object[] { serialized });
            }

            throw new ArgumentException($"Could not deserialize primitive type [{objectType}] from reader type [{reader?.Value?.GetType()}]!");
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (!encodeMethods.TryGetValue(value!.GetType(), out var encodeMethod))
            {
                encodeMethod = typeof(Coder<>).MakeGenericType(value!.GetType()).GetMethod(nameof(Coder<colorX>.EncodeToString));
                encodeMethods.Add(value!.GetType(), encodeMethod);
            }

            var serialized = (string)encodeMethod.Invoke(null, new[] { value });

            writer.WriteValue(serialized);
        }
    }
}