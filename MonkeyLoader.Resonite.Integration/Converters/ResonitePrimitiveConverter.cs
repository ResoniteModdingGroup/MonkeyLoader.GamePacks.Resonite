// Adapted from the NeosModLoader project.

using Elements.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonkeyLoader.Resonite.Converters
{
    /// <summary>
    /// Converts a Resonite Primitive to and from json.
    /// </summary>
    public sealed class ResonitePrimitiveConverter : JsonConverter
    {
        private static readonly Dictionary<Type, CoderMethods> _coderMethods = [];
        private static readonly Assembly _primitivesAssembly = typeof(colorX).Assembly;

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsNullable())
                objectType = Nullable.GetUnderlyingType(objectType)!;

            // handle all non-enum Resonite Primitives
            return !objectType.IsEnum && _primitivesAssembly.Equals(objectType.Assembly)
                && Coder.IsEnginePrimitive(objectType) && GetCoderMethods(objectType).SupportsStringCoding;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (objectType.IsNullable() && reader.TokenType == JsonToken.Null)
                return null!;

            if (reader.Value is not string serialized)
                throw new ArgumentException($"Could not deserialize primitive type [{objectType}] from reader type [{reader?.Value?.GetType()}]!");

            // use Resonite's built-in decoding if the value was serialized as a string
            return GetCoderMethods(objectType).Decode(serialized);
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var serialized = GetCoderMethods(value!.GetType()).Encode(value);

            writer.WriteValue(serialized);
        }

        private static CoderMethods GetCoderMethods(Type type)
        {
            if (!_coderMethods.TryGetValue(type, out var coderMethods))
            {
                coderMethods = new CoderMethods(type);
                _coderMethods.Add(type, coderMethods);
            }

            return coderMethods;
        }

        private readonly struct CoderMethods
        {
            private static readonly Type _coderType = typeof(Coder<>);
            private static readonly string _decodeFromString = nameof(Coder<colorX>.DecodeFromString);
            private static readonly string _encodeToString = nameof(Coder<colorX>.EncodeToString);
            private static readonly string _supportsStringCodingString = nameof(Coder<colorX>.SupportsStringCoding);

            private readonly MethodInfo _decode;
            private readonly MethodInfo _encode;

            public bool SupportsStringCoding { get; }

            public CoderMethods(Type type)
            {
                var specificCoder = _coderType.MakeGenericType(type);

                _encode = specificCoder.GetMethod(_encodeToString)!;
                _decode = specificCoder.GetMethod(_decodeFromString)!;
                SupportsStringCoding = (bool)specificCoder.GetProperty(_supportsStringCodingString)!.GetValue(null)!;
            }

            public object Decode(string serialized)
                => _decode.Invoke(null, [serialized])!;

            public string Encode(object? value)
                => (string)_encode.Invoke(null, [value])!;
        }
    }
}