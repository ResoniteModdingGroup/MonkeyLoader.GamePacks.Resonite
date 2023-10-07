using Newtonsoft.Json;
using System;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Indicates that a <see cref="JsonConverter"/> or <see cref="JsonConverter{T}"/> derived class
    /// in a game tooling library isn't to be included in the list of converters for the <see cref="MonkeyLoader.JsonSerializer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreJsonConverterAttribute : MonkeyLoaderAttribute
    { }
}