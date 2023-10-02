using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Indicates that a <see cref="JsonConverter{T}"/> or <see cref="JsonConverterFactory"/> derived class
    /// in a game tooling library isn't to be included in the list of converters for the <see cref="JsonSerializerOptions"/>
    /// used by the <see cref="ConfigManager"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreJsonConverterAttribute : MonkeyLoaderAttribute
    { }
}