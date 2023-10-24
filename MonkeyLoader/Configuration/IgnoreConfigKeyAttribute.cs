// Adapted from the NeosModLoader project.

using System;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Marks a field of type <see cref="ConfigKey{T}"/> on a class deriving from <see cref="ConfigSection"/>
    /// to be excluded from the fields returned by <see cref="ConfigSection.GetAutoConfigKeys"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreConfigKeyAttribute : MonkeyLoaderAttribute
    { }
}