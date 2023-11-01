using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyLoader.Meta
{
    /// <summary>
    /// Specifies where and how to search for mods.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ModLoadingLocation
    {
        private Regex[] _ignorePatterns;

        /// <summary>
        /// Gets the regex patterns that exclude a mod from being loaded if any match.<br/>
        /// Patterns are matched case-insensitive.
        /// </summary>
        public IEnumerable<Regex> IgnorePatterns
        {
            get => _ignorePatterns.AsSafeEnumerable();
            set => _ignorePatterns = value.ToArray();
        }

        /// <summary>
        /// Gets the regex patterns that exclude a mod from being loaded if any match as strings.<br/>
        /// Patterns are matched case-insensitive.
        /// </summary>
        [JsonProperty("IgnorePatterns")]
        public IEnumerable<string> IgnorePatternsStrings
        {
            get => _ignorePatterns.Select(regex => regex.ToString());
            set => _ignorePatterns = value.Select(pattern => new Regex(pattern)).ToArray();
        }

        /// <summary>
        /// Gets the root folder to search.
        /// </summary>
        [JsonProperty("Path")]
        public string Path { get; }

        /// <summary>
        /// Gets whether nested folders get searched too.
        /// </summary>
        [JsonProperty("Recursive")]
        public bool Recursive { get; }

        /// <summary>
        /// Creates a new <see cref="ModLoadingLocation"/> with the given specification.
        /// </summary>
        /// <param name="path">The root folder to search.</param>
        /// <param name="recursive">Whether to search nested folders too.</param>
        /// <param name="ignorePatterns">Regular expression patterns that exclude a mod from being loaded if any match.<br/>
        /// Patterns are matched case-insensitive.</param>
        [JsonConstructor]
        public ModLoadingLocation(string path, bool recursive, params string[] ignorePatterns)
            : this(path, recursive, ignorePatterns.Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase)))
        { }

        /// <summary>
        /// Creates a new <see cref="ModLoadingLocation"/> with the given specification.
        /// </summary>
        /// <param name="path">The root folder to search.</param>
        /// <param name="recursive">Whether to search nested folders too.</param>
        /// <param name="ignorePatterns">Regular expression patterns that exclude a mod from being loaded if any match.<br/>
        /// Patterns are matched case-insensitive.</param>
        public ModLoadingLocation(string path, bool recursive, IEnumerable<string> ignorePatterns)
            : this(path, recursive, ignorePatterns.Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase)))
        { }

        private ModLoadingLocation(string path, bool recursive, IEnumerable<Regex> ignorePatterns)
        {
            Path = path;
            Recursive = recursive;
            this._ignorePatterns = ignorePatterns.ToArray();
        }

        /// <summary>
        /// Conducts a search based on the specifications of this loading location.
        /// </summary>
        /// <returns>The full names (including paths) of all files that satisfy the specifications.</returns>
        public IEnumerable<string> Search()
            => Directory.EnumerateFiles(Path, NuGetPackageMod.SearchPattern, Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(path => !_ignorePatterns.Any(pattern => pattern.IsMatch(path)));

        /// <inheritdoc/>
        public override string ToString()
            => $"[Recursive: {Recursive}, Path: {Path}, Excluding: {{ {string.Join(" ", _ignorePatterns.Select(p => p.ToString()))} }}]";
    }
}