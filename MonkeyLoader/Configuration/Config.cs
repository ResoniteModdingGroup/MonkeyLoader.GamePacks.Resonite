// Adapted from the NeosModLoader project.

using HarmonyLib;
using MonkeyLoader.Logging;
using MonkeyLoader.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// The configuration for a mod. Each mod has exactly one configuration.<br/>
    /// The configuration object will never be reassigned once initialized.
    /// </summary>
    public sealed class Config
    {
        private const string OwnerKey = "Owner";
        private const string SectionsKey = "Sections";

        // this is a ridiculous hack because HashSet.TryGetValue doesn't exist in .NET 4.6.2
        private readonly Dictionary<ConfigKey, ConfigKey> _configurationItemDefinitionsSelfMap = new();

        private readonly JObject _loadedConfig;

        private readonly HashSet<ConfigSection> _sections = new();

        /// <summary>
        /// Gets the set of configuration keys defined in this configuration definition.
        /// </summary>
        // clone the collection because I don't trust giving public API users shallow copies one bit
        public ISet<ConfigKey> ConfigurationItemDefinitions
            => new HashSet<ConfigKey>(_configurationItemDefinitionsSelfMap.Keys);

        /// <summary>
        /// Gets the logger used by this config.
        /// </summary>
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Gets the mod that owns this config.
        /// </summary>
        public IConfigOwner Owner { get; }

        /// <summary>
        /// Gets all loaded sections of this config.
        /// </summary>
        public IEnumerable<ConfigSection> Sections
        {
            get
            {
                foreach (var section in _sections)
                    yield return section;
            }
        }

        internal Config(IConfigOwner owner)
        {
            Owner = owner;
            Logger = new MonkeyLogger(owner.Logger, "Config");

            _loadedConfig = LoadConfig();
            if (_loadedConfig[OwnerKey]?.ToObject<string>() != Path.GetFileNameWithoutExtension(Owner.ConfigPath))
                throw new ConfigLoadException("Config malformed! Recorded owner must match the loader!");

            if (_loadedConfig[SectionsKey] is not JObject)
            {
                Logger.Warn(() => "Could not find \"Sections\" object - created it!");
                _loadedConfig[SectionsKey] = new JObject();
            }
        }

        /// <summary>
        /// Get a value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the key.</returns>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        public object? GetValue(ConfigKey key)
        {
            if (!TryGetValue(key, out var value))
                ThrowKeyNotFound(key);

            return value;
        }

        /// <summary>
        /// Get a value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </summary>
        /// <typeparam name="T">The type of the key's value.</typeparam>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the key.</returns>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        public T GetValue<T>(ConfigKey<T> key)
        {
            if (!TryGetValue(key, out var value))
                ThrowKeyNotFound(key);

            return value;
        }

        /// <summary>
        /// Checks if the given key is defined in this config.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the key is defined.</returns>
        public bool IsKeyDefined(ConfigKey key) => TryGetDefiningKey(key, out _);

        /// <summary>
        /// Loads a section with a parameterless constructor based on its type.<br/>
        /// Every section can only be loaded once.
        /// </summary>
        /// <typeparam name="TSection">The type of the section to load.</typeparam>
        /// <returns>The loaded section.</returns>
        /// <exception cref="ConfigLoadException">If section has already been loaded, or something goes wrong while loading.</exception>
        public TSection LoadSection<TSection>() where TSection : ConfigSection, new()
        {
            var section = new TSection();
            section.Config = this;

            return LoadSection(section);
        }

        /// <summary>
        /// Loads the given section.<br/>
        /// Every section can only be loaded once.
        /// </summary>
        /// <typeparam name="TSection">The type of the section to load.</typeparam>
        /// <returns>The loaded section.</returns>
        /// <exception cref="ConfigLoadException">If section has already been loaded, or something goes wrong while loading.</exception>
        public TSection LoadSection<TSection>(TSection section) where TSection : ConfigSection
        {
            if (_sections.Contains(section))
                throw new ConfigLoadException($"Attempted to load section [{section.Name}] twice!");

            if (_loadedConfig[SectionsKey]![section.Name] is not JObject sectionObject)
                Logger.Warn(() => $"Section [{section.Name}] didn't appear in the loaded config - using defaults!");
            else
                section.LoadSection(sectionObject, Owner.Loader.JsonSerializer);

            foreach (var key in section.Keys)
                _configurationItemDefinitionsSelfMap.Add(key, key);

            _sections.Add(section);

            return section;
        }

        /// <summary>
        /// Persists this configuration to disk.
        /// </summary>
        public void Save()
        {
            if (!_sections.Any())
            {
                Logger.Info(() => "Skipping save - no Config Keys!");
                return;
            }

            var sectionsJson = (JObject)_loadedConfig[SectionsKey]!;
            var stopwatch = Stopwatch.StartNew();

            lock (_loadedConfig)
            {
                foreach (var section in _sections)
                {
                    try
                    {
                        var sectionJson = section.Save(Owner.Loader.JsonSerializer);

                        if (section is not null)
                            sectionsJson[section.Name] = sectionJson;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(() => ex.Format($"Exception while serializing section [{section.Name}] - skipping it!"));
                    }
                }

                try
                {
                    using var file = File.OpenWrite(Owner.ConfigPath);
                    using var streamWriter = new StreamWriter(file);
                    using var jsonTextWriter = new JsonTextWriter(streamWriter);
                    jsonTextWriter.Formatting = Formatting.Indented;
                    _loadedConfig.WriteTo(jsonTextWriter);

                    // I actually cannot believe I have to truncate the file myself
                    file.SetLength(file.Position);
                    jsonTextWriter.Flush();

                    Logger.Info(() => $"Saved config in {stopwatch.ElapsedMilliseconds}ms!");
                }
                catch (Exception ex)
                {
                    Logger.Error(() => ex.Format($"Exception while saving config!"));
                }
            }
        }

        /// <summary>
        /// Sets a configuration value for the given key, throwing a <see cref="KeyNotFoundException"/> if the key is not found
        /// or an <see cref="ArgumentException"/> if the value is not valid for it.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="eventLabel">A custom label you may assign to this change event.</param>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        /// <exception cref="ArgumentException">The new value is not valid for the given key.</exception>
        public void Set(ConfigKey key, object? value, string? eventLabel = null)
        {
            if (!TryGetDefiningKey(key, out ConfigKey? definingKey))
                ThrowKeyNotFound(key);

            if (value is null)
            {
                if (Util.CannotBeNull(definingKey!.ValueType))
                    ThrowArgumentException($"Cannot assign null to key [{definingKey.Name}] of type [{definingKey.ValueType}] from section [{key.Section.Name}]!", nameof(value));
            }
            else if (!definingKey.ValueType.IsAssignableFrom(value.GetType()))
            {
                ThrowArgumentException($"Cannot assign [{value.GetType()}] to key [{definingKey.Name}] of type [{definingKey.ValueType}] from section [{key.Section.Name}]!", nameof(value));
            }

            if (!definingKey.Validate(value))
                ThrowArgumentException($"Cannot assign invalid value \"{value}\" to key [{definingKey.Name}] of type [{definingKey.ValueType}] from section [{key.Section.Name}]!", nameof(value));

            var oldValue = GetValue(key);
            definingKey.Set(value);

            FireConfigChangedEvents(definingKey, oldValue, eventLabel);
        }

        /// <summary>
        /// Sets a configuration value for the given key, throwing a <see cref="KeyNotFoundException"/> if the key is not found
        /// or an <see cref="ArgumentException"/> if the value is not valid for it.
        /// </summary>
        /// <typeparam name="T">The type of the key's value.</typeparam>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="eventLabel">A custom label you may assign to this change event.</param>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        /// <exception cref="ArgumentException">The new value is not valid for the given key.</exception>
        public void Set<T>(ConfigKey<T> key, T value, string? eventLabel = null)
        {
            // the reason we don't fall back to untyped Set() here is so we can skip the type check

            if (!TryGetDefiningKey(key, out ConfigKey? definingKey))
                ThrowKeyNotFound(key);

            if (!definingKey.Validate(value))
                ThrowArgumentException($"Cannot assign invalid value \"{value}\" to key [{definingKey.Name}] of type [{definingKey.ValueType}] from section [{key.Section.Name}]!", nameof(value));

            var oldValue = GetValue(key);
            definingKey.Set(value);

            FireConfigChangedEvents(definingKey, oldValue, eventLabel);
        }

        /// <summary>
        /// Tries to get the defining key in this config for the given key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="definingKey">The defining key in this config when this returns <c>true</c>, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the key is defined in this config.</returns>
        public bool TryGetDefiningKey(ConfigKey key, [NotNullWhen(true)] out ConfigKey? definingKey)
        {
            if (key.DefiningKey != null)
            {
                // we've already cached the defining key
                definingKey = key.DefiningKey;
                return true;
            }

            // first time we've seen this key instance: we need to hit the map
            if (_configurationItemDefinitionsSelfMap.TryGetValue(key, out definingKey))
            {
                // initialize the cache for this key
                key.DefiningKey = definingKey;
                return true;
            }

            // not a real key
            definingKey = null;
            return false;
        }

        /// <summary>
        /// Tries to get a value, returning <c>default</c> if the key is not found.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The value if the return value is <c>true</c>, or <c>default</c> if <c>false</c>.</param>
        /// <returns><c>true</c> if the value was read successfully.</returns>
        public bool TryGetValue(ConfigKey key, out object? value)
        {
            if (!TryGetDefiningKey(key, out ConfigKey? definingKey))
            {
                // not in definition
                value = null;
                return false;
            }

            if (definingKey.TryGetValue(out value))
                return true;

            if (definingKey.TryComputeDefault(out value))
                return true;

            value = null;
            return false;
        }

        /// <summary>
        /// Tries to get a value, returning <c>default(<typeparamref name="T"/>)</c> if the key is not found.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The value if the return value is <c>true</c>, or <c>default</c> if <c>false</c>.</param>
        /// <returns><c>true</c> if the value was read successfully.</returns>
        public bool TryGetValue<T>(ConfigKey<T> key, [NotNullWhen(true)] out T? value)
        {
            if (TryGetValue(key, out object? valueObject))
            {
                value = (T)valueObject!;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Removes a key's value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </summary>
        /// <param name="key">The key to remove the value for.</param>
        /// <returns><c>true</c> if a value was successfully found and removed, <c>false</c> if there was no value to remove.</returns>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        public bool Unset(ConfigKey key)
        {
            if (!TryGetDefiningKey(key, out var definingKey))
                ThrowKeyNotFound(key);

            return definingKey.Unset();
        }

        internal void EnsureDirectoryExists() => Directory.CreateDirectory(Owner.Loader.Locations.Configs);

        /// <summary>
        /// Checks if the given key is the defining key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the key is the defining key.</returns>
            // a key is the defining key if and only if its DefiningKey property references itself
        internal bool IsKeyDefiningKey(ConfigKey key) => ReferenceEquals(key, key.DefiningKey);

        private bool AnyValuesSet() => ConfigurationItemDefinitions.Any(key => key.HasValue);

        private void FireConfigChangedEvents(ConfigKey key, object? oldValue, string? label)
        {
            var configChangedEvent = new ConfigChangedEvent(this, key, oldValue, label);

            try
            {
                OnChanged?.TryInvokeAll(configChangedEvent);
            }
            catch (AggregateException ex)
            {
                Logger.Error(() => ex.Format("Some OnThisConfigurationChanged event subscriber threw an exception:"));
            }

            Owner.Loader.FireConfigChangedEvent(configChangedEvent);
        }

        private JObject LoadConfig()
        {
            if (File.Exists(Owner.ConfigPath))
            {
                try
                {
                    using var file = File.OpenText(Owner.ConfigPath);
                    using var reader = new JsonTextReader(file);

                    return JObject.Load(reader);
                }
                catch (Exception ex)
                {
                    // I know not what exceptions the JSON library will throw, but they must be contained
                    throw new ConfigLoadException($"Error loading config!", ex);
                }
            }

            return new JObject()
            {
                [OwnerKey] = Path.GetFileNameWithoutExtension(Owner.ConfigPath),
                [SectionsKey] = new JObject()
            };
        }

        [DoesNotReturn]
        private void ThrowArgumentException(string message, string paramName)
            => throw new ArgumentException(message, paramName);

        [DoesNotReturn]
        private void ThrowKeyNotFound(ConfigKey key)
            => throw new KeyNotFoundException($"Key [{key.Name}] not found in config!");

        /// <summary>
        /// The delegate that is called for configuration change events.
        /// </summary>
        /// <param name="configChangedEvent">The event containing details about the configuration change</param>
        public delegate void ConfigChangedEventHandler(ConfigChangedEvent configChangedEvent);

        /// <summary>
        /// Called when the value of one of the keys of this config gets changed.
        /// </summary>
        public event ConfigChangedEventHandler? OnChanged;
    }
}