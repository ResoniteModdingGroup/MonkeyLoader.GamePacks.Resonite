using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;

namespace MonkeyLoader.NuGet
{
    /// <summary>
    /// Represents a NuGet feed source.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public sealed class NuGetSource
    {
        /// <summary>
        /// Gets whether this NuGet source uses authentication.<br/>
        /// <c>true</c> when <see cref="Username">Username</see> and <see cref="Password">Password</see> aren't empty.
        /// </summary>
        public bool Authenticated => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

        /// <summary>
        /// Gets the name of this source, e.g. "Official NuGet Feed".
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; }

        /// <summary>
        /// Gets the password to use to authenticate with this NuGet source.<br/>
        /// Should be <c>null</c> when no authentication is required.
        /// </summary>
        [field: JsonProperty("Password")]
        public string? Password { get; }

        /// <summary>
        /// Gets the URI of this NuGet source's feed.<br/>
        /// Usually ends with <c>/v3/index.json</c>
        /// </summary>
        [JsonProperty("SourceUri")]
        public Uri SourceUri { get; }

        /// <summary>
        /// Gets the username to use to authenticate with this NuGet source.<br/>
        /// Should be <c>null</c> when no authentication is required.
        /// </summary>
        [field: JsonProperty("Username")]
        public string? Username { get; }

        /// <summary>
        /// Creates a new authenticated <see cref="NuGetSource"/> instance with the given details.
        /// </summary>
        /// <param name="name">The nice name of the source.</param>
        /// <param name="sourceUri">The URI of the source's feed. Usually ends with <c>/v3/index.json</c></param>
        /// <param name="username">The username to use to authenticate with this source.</param>
        /// <param name="password">The password to use to authenticate with this source.</param>
        public NuGetSource(string name, Uri sourceUri, string username, string password) : this(name, sourceUri)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Creates a new unauthenticated <see cref="NuGetSource"/> instance with the given details.
        /// </summary>
        /// <param name="name">The nice name of the source.</param>
        /// <param name="sourceUri">The URI of the source's feed. Usually ends with <c>/v3/index.json</c></param>
        [JsonConstructor]
        public NuGetSource(string name, Uri sourceUri)
        {
            Name = name;
            SourceUri = sourceUri;
        }

        /// <summary>
        /// Gets a <see cref="PackageSource"/> created from this NuGet source.
        /// </summary>
        /// <returns>The package source based on this.</returns>
        public PackageSource GetPackageSource()
        {
            var source = new PackageSource(SourceUri.AbsoluteUri, Name);

            if (Authenticated)
                source.Credentials = new(SourceUri.AbsolutePath, Username, Password, true, string.Empty);

            return source;
        }

        /// <summary>
        /// Gets a <see cref="SourceRepository"/> created from this NuGet source.
        /// </summary>
        /// <returns>The source repositor based on this.</returns>
        public SourceRepository GetSourceRepository() => Repository.Factory.GetCoreV3(GetPackageSource());
    }
}