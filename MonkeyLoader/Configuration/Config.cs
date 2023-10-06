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
        private static readonly string VALUES_JSON_KEY = "values";
        private static readonly string VERSION_JSON_KEY = "version";

        // used to keep track of mods that spam Save():
        // any mod that calls Save() for the ModConfiguration within debounceMilliseconds of the previous call to the same ModConfiguration
        // will be put into Ultimate Punishment Mode, and ALL their Save() calls, regardless of ModConfiguration, will be debounced.
        // The naughty list is global, while the actual debouncing is per-configuration.
        private static ISet<string> naughtySavers = new HashSet<string>();

        // this is a ridiculous hack because HashSet.TryGetValue doesn't exist in .NET 4.6.2
        private readonly Dictionary<ConfigKey, ConfigKey> configurationItemDefinitionsSelfMap = new();

        private readonly JObject? loadedConfigSections;

        // used to keep track of the debouncers for this configuration.
        private readonly Dictionary<string, Action<bool>> saveActionForCallee = new();

        // used to track how frequenly Save() is being called
        private readonly Stopwatch saveTimer = new();

        private readonly HashSet<ConfigSection> sections = new();

        // time that save must not be called for a save to actually go through
        private int debounceMilliseconds = 3000;

        /// <summary>
        /// Gets the set of configuration keys defined in this configuration definition.
        /// </summary>
        // clone the collection because I don't trust giving public API users shallow copies one bit
        public ISet<ConfigKey> ConfigurationItemDefinitions
            => new HashSet<ConfigKey>(configurationItemDefinitionsSelfMap.Keys);

        /// <summary>
        /// Gets the logger used by this config.
        /// </summary>
        public MonkeyLogger Logger { get; }

        /// <summary>
        /// Gets the mod that owns this config.
        /// </summary>
        public Mod Mod { get; }

        /// <summary>
        /// Gets all loaded sections of this config.
        /// </summary>
        public IEnumerable<ConfigSection> Sections
        {
            get
            {
                foreach (var section in sections)
                    yield return section;
            }
        }

        internal Config(Mod mod)
        {
            Mod = mod;
            Logger = new MonkeyLogger(mod.Logger, "Config");

            loadedConfigSections = loadConfig();
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
                throwKeyNotFound(key);

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
                throwKeyNotFound(key);

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
            if (sections.Contains(section))
                throw new ConfigLoadException($"Attempted to load section [{section.Name}] twice!");

            if (loadedConfigSections is not null)
            {
                if (loadedConfigSections[section.Name] is not JObject sectionObject)
                    Logger.Warn(() => $"Section [{section.Name}] didn't appear in the loaded config!");
                else
                    section.LoadSection(sectionObject);
            }

            foreach (var key in section.Keys)
                configurationItemDefinitionsSelfMap.Add(key, key);

            return section;
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
                throwKeyNotFound(key);

            if (value is null)
            {
                if (Util.CannotBeNull(definingKey!.ValueType))
                    throwArgumentException($"Cannot assign null to key [{definingKey.Name}] of type [{definingKey.ValueType}]!", nameof(value));
            }
            else if (!definingKey.ValueType.IsAssignableFrom(value.GetType()))
            {
                throwArgumentException($"Cannot assign [{value.GetType()}] to key [{definingKey.Name}] of type [{definingKey.ValueType}]!", nameof(value));
            }

            if (!definingKey.Validate(value))
                throwArgumentException($"Cannot assign invalid value \"{value}\" to key [{definingKey.Name}] of type [{definingKey.ValueType}]!", nameof(value));

            var oldValue = GetValue(key);
            definingKey.Set(value);

            fireConfigChangedEvents(definingKey, oldValue, eventLabel);
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
                throwKeyNotFound(key);

            if (!definingKey.Validate(value))
                throwArgumentException($"Cannot assign invalid value \"{value}\" to key [{definingKey.Name}] of type [{definingKey.ValueType}]!", nameof(value));

            var oldValue = GetValue(key);
            definingKey.Set(value);

            fireConfigChangedEvents(definingKey, oldValue, eventLabel);
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
            if (configurationItemDefinitionsSelfMap.TryGetValue(key, out definingKey))
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
                throwKeyNotFound(key);

            return definingKey.Unset();
        }

        internal void EnsureDirectoryExists() => Directory.CreateDirectory(Mod.Loader.Locations.Configs);

        /// <summary>
        /// Checks if the given key is the defining key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the key is the defining key.</returns>
        internal bool IsKeyDefiningKey(ConfigKey key)
        {
            // a key is the defining key if and only if its DefiningKey property references itself
            return ReferenceEquals(key, key.DefiningKey); // this is safe because we'll throw a NRE if key is null
        }

        /// <summary>
        /// Asynchronously persists this configuration to disk.
        /// </summary>
        /// <param name="saveDefaultValues">If <c>true</c>, default values will also be persisted.</param>
        /// <param name="immediate">If <c>true</c>, skip the debouncing and save immediately.</param>
        internal void Save(bool saveDefaultValues = false, bool immediate = false)
        {
            Thread thread = Thread.CurrentThread;
            ResoniteMod? callee = Util.ExecutingMod(new(1));
            Action<bool>? saveAction = null;

            // get saved state for this callee
            if (callee != null && naughtySavers.Contains(callee.Name) && !saveActionForCallee.TryGetValue(callee.Name, out saveAction))
            {
                // handle case where the callee was marked as naughty from a different ModConfiguration being spammed
                saveAction = Util.Debounce<bool>(SaveInternal, debounceMilliseconds);
                saveActionForCallee.Add(callee.Name, saveAction);
            }

            if (saveTimer.IsRunning)
            {
                float elapsedMillis = saveTimer.ElapsedMilliseconds;
                saveTimer.Restart();
                if (elapsedMillis < debounceMilliseconds)
                {
                    Logger.WarnInternal($"ModConfiguration.Save({saveDefaultValues}) called for \"{LoadedResoniteMod.ResoniteMod.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\". Last called {elapsedMillis / 1000f}s ago. This is very recent! Do not spam calls to ModConfiguration.Save()! All Save() calls by this mod are now subject to a {debounceMilliseconds}ms debouncing delay.");
                    if (saveAction == null && callee != null)
                    {
                        // congrats, you've switched into Ultimate Punishment Mode where now I don't trust you and your Save() calls get debounced
                        saveAction = Util.Debounce<bool>(SaveInternal, debounceMilliseconds);
                        saveActionForCallee.Add(callee.Name, saveAction);
                        naughtySavers.Add(callee.Name);
                    }
                }
                else
                {
                    Logger.DebugFuncInternal(() => $"ModConfiguration.Save({saveDefaultValues}) called for \"{LoadedResoniteMod.ResoniteMod.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\". Last called {elapsedMillis / 1000f}s ago.");
                }
            }
            else
            {
                saveTimer.Start();
                Logger.DebugFuncInternal(() => $"ModConfiguration.Save({saveDefaultValues}) called for \"{LoadedResoniteMod.ResoniteMod.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\"");
            }

            // prevent saving if we've determined something is amiss with the configuration
            if (!LoadedResoniteMod.AllowSavingConfiguration)
            {
                Logger.WarnInternal($"ModConfiguration for {LoadedResoniteMod.ResoniteMod.Name} will NOT be saved due to a safety check failing. This is probably due to you downgrading a mod.");
                return;
            }

            if (immediate || saveAction == null)
            {
                // infrequent callers get to save immediately
                Task.Run(() => SaveInternal(saveDefaultValues));
            }
            else
            {
                // bad callers get debounced
                saveAction(saveDefaultValues);
            }
        }

        private bool anyValuesSet() => ConfigurationItemDefinitions.Where(key => key.HasValue).Any();

        private void fireConfigChangedEvents(ConfigKey key, object? oldValue, string? label)
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

            Mod.Loader.FireConfigChangedEvent(configChangedEvent);
        }

        private JObject? loadConfig()
        {
            if (!File.Exists(Mod.ConfigPath))
                return null;

            try
            {
                using var file = File.OpenText(Mod.ConfigPath);
                using var reader = new JsonTextReader(file);
                var json = JObject.Load(reader);

                json.Properties;
            }
            catch (FileNotFoundException)
            {
                // return early
                return new Config(mod, definition);
            }
            catch (Exception e)
            {
                // I know not what exceptions the JSON library will throw, but they must be contained
                mod.AllowSavingConfiguration = false;
                throw new ConfigLoadException($"Error loading config for {mod.ResoniteMod.Name}", e);
            }

            return new Config(mod, definition);
        }

        /// <summary>
        /// performs the actual, synchronous save
        /// </summary>
        /// <param name="saveDefaultValues">If true, default values will also be persisted</param>
        private void SaveInternal(bool saveDefaultValues = false)
        {
            var stopwatch = Stopwatch.StartNew();
            JObject json = new()
            {
                [VERSION_JSON_KEY] = JToken.FromObject(Definition.Version.ToString(), jsonSerializer)
            };

            JObject valueMap = new();
            foreach (ConfigKey key in ConfigurationItemDefinitions)
            {
                if (key.TryGetValue(out object? value))
                {
                    // I don't need to typecheck this as there's no way to sneak a bad type past my Set() API
                    valueMap[key.Name] = value == null ? null : JToken.FromObject(value, jsonSerializer);
                }
                else if (saveDefaultValues && key.TryComputeDefault(out object? defaultValue))
                {
                    // I don't need to typecheck this as there's no way to sneak a bad type past my computeDefault API
                    // like and say defaultValue can't be null because the Json.Net
                    valueMap[key.Name] = defaultValue == null ? null : JToken.FromObject(defaultValue, jsonSerializer);
                }
            }

            json[VALUES_JSON_KEY] = valueMap;

            string configFile = GetModConfigPath(LoadedResoniteMod);
            using FileStream file = File.OpenWrite(configFile);
            using StreamWriter streamWriter = new(file);
            using JsonTextWriter jsonTextWriter = new(streamWriter);
            json.WriteTo(jsonTextWriter);

            // I actually cannot believe I have to truncate the file myself
            file.SetLength(file.Position);
            jsonTextWriter.Flush();

            Logger.DebugFuncInternal(() => $"Saved ModConfiguration for \"{LoadedResoniteMod.ResoniteMod.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
        }

        [DoesNotReturn]
        private void throwArgumentException(string message, string paramName)
            => throw new ArgumentException(message, paramName);

        [DoesNotReturn]
        private void throwKeyNotFound(ConfigKey key)
            => throw new KeyNotFoundException($"Key [{key.Name}] not found in config for [{Mod.Id}]!");

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