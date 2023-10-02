// Adapted from the NeosModLoader project.

using System;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// Represents an <see cref="Exception"/> encountered while loading a configuration file.
    /// </summary>
    public class ConfigLoadException : Exception
    {
        internal ConfigLoadException(string message) : base(message)
        { }

        internal ConfigLoadException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}