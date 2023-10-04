// Adapted from the NeosModLoader project.

using HarmonyLib;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyLoader.Configuration
{
    /// <summary>
    /// The configuration for a mod. Each mod has zero or one configuration. The configuration object will never be reassigned once initialized.
    /// </summary>
    public class Config : IConfigDefinition
    {
        private static readonly string VALUES_JSON_KEY = "values";
        private static readonly string VERSION_JSON_KEY = "version";

        // used to keep track of mods that spam Save():
        // any mod that calls Save() for the ModConfiguration within debounceMilliseconds of the previous call to the same ModConfiguration
        // will be put into Ultimate Punishment Mode, and ALL their Save() calls, regardless of ModConfiguration, will be debounced.
        // The naughty list is global, while the actual debouncing is per-configuration.
        private static ISet<string> naughtySavers = new HashSet<string>();

        private readonly ConfigurationDefinition Definition;

        // time that save must not be called for a save to actually go through
        private int debounceMilliseconds = 3000;

        // used to keep track of the debouncers for this configuration.
        private Dictionary<string, Action<bool>> saveActionForCallee = new();

        // used to track how frequenly Save() is being called
        private Stopwatch saveTimer = new Stopwatch();

        /// <inheritdoc/>
        public ISet<ConfigKey> ConfigurationItemDefinitions => Definition.ConfigurationItemDefinitions;

        /// <inheritdoc/>
        public Mod Owner => Definition.Owner;

        /// <inheritdoc/>
        public Version Version => Definition.Version;

        internal LoadedResoniteMod LoadedResoniteMod { get; private set; }
        private bool AutoSave => Definition.AutoSave;

        private Config(LoadedResoniteMod loadedResoniteMod, ConfigurationDefinition definition)
        {
            LoadedResoniteMod = loadedResoniteMod;
            Definition = definition;
        }

        /// <summary>
        /// Get a value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the key.</returns>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        public object GetValue(ConfigKey key)
        {
            if (TryGetValue(key, out object? value))
            {
                return value!;
            }
            else
            {
                throw new KeyNotFoundException($"{key.Name} not found in {LoadedResoniteMod.ResoniteMod.Name} configuration");
            }
        }

        /// <summary>
        /// Get a value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </summary>
        /// <typeparam name="T">The type of the key's value.</typeparam>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the key.</returns>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        public T? GetValue<T>(ConfigKey<T> key)
        {
            if (TryGetValue(key, out T? value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException($"{key.Name} not found in {LoadedResoniteMod.ResoniteMod.Name} configuration");
            }
        }

        /// <summary>
        /// Checks if the given key is defined in this config.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the key is defined.</returns>
        public bool IsKeyDefined(ConfigKey key)
        {
            // if a key has a non-null defining key it's guaranteed a real key. Lets check for that.
            ConfigKey? definingKey = key.DefiningKey;
            if (definingKey != null)
            {
                return true;
            }

            // okay, the defining key was null, so lets try to get the defining key from the hashtable instead
            if (Definition.TryGetDefiningKey(key, out definingKey))
            {
                // we might as well set this now that we have the real defining key
                key.DefiningKey = definingKey;
                return true;
            }

            // there was no definition
            return false;
        }

        public TSection LoadSection<TSection>() where TSection : ConfigSection
        {
        }

        /// <summary>
        /// Persist this configuration to disk.<br/>
        /// This method is not called automatically.<br/>
        /// Default values are not automatically saved.
        /// </summary>
        public void Save() // this overload is needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)
        {
            Save(false, false);
        }

        /// <summary>
        /// Persist this configuration to disk.<br/>
        /// This method is not called automatically.
        /// </summary>
        /// <param name="saveDefaultValues">If <c>true</c>, default values will also be persisted.</param>
        public void Save(bool saveDefaultValues = false) // this overload is needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)
        {
            Save(saveDefaultValues, false);
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
            if (!Definition.TryGetDefiningKey(key, out ConfigKey? definingKey))
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedResoniteMod.ResoniteMod.Name}");
            }

            if (value == null)
            {
                if (Util.CannotBeNull(definingKey!.ValueType()))
                {
                    throw new ArgumentException($"null cannot be assigned to {definingKey.ValueType()}");
                }
            }
            else if (!definingKey!.ValueType().IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException($"{value.GetType()} cannot be assigned to {definingKey.ValueType()}");
            }

            if (!definingKey!.Validate(value))
            {
                throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{definingKey.Name}\"");
            }

            definingKey.Set(value);
            FireConfigChangedEvent(definingKey, eventLabel);
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

            if (!Definition.TryGetDefiningKey(key, out ConfigKey? definingKey))
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedResoniteMod.ResoniteMod.Name}");
            }

            if (!definingKey!.Validate(value))
            {
                throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{definingKey.Name}\"");
            }

            definingKey.Set(value);
            FireConfigChangedEvent(definingKey, eventLabel);
        }

        /// <summary>
        /// Tries to get a value, returning <c>default</c> if the key is not found.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The value if the return value is <c>true</c>, or <c>default</c> if <c>false</c>.</param>
        /// <returns><c>true</c> if the value was read successfully.</returns>
        public bool TryGetValue(ConfigKey key, out object? value)
        {
            if (!Definition.TryGetDefiningKey(key, out ConfigKey? definingKey))
            {
                // not in definition
                value = null;
                return false;
            }

            if (definingKey!.TryGetValue(out object? valueObject))
            {
                value = valueObject;
                return true;
            }
            else if (definingKey.TryComputeDefault(out value))
            {
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get a value, returning <c>default(<typeparamref name="T"/>)</c> if the key is not found.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The value if the return value is <c>true</c>, or <c>default</c> if <c>false</c>.</param>
        /// <returns><c>true</c> if the value was read successfully.</returns>
        public bool TryGetValue<T>(ConfigKey<T> key, out T? value)
        {
            if (TryGetValue(key, out object? valueObject))
            {
                value = (T)valueObject!;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Removes a configuration value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </summary>
        /// <param name="key">The key to remove the value for.</param>
        /// <returns><c>true</c> if a value was successfully found and removed, <c>false</c> if there was no value to remove.</returns>
        /// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
        public bool Unset(ConfigKey key)
        {
            if (Definition.TryGetDefiningKey(key, out ConfigKey? definingKey))
            {
                return definingKey!.Unset();
            }
            else
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedResoniteMod.ResoniteMod.Name}");
            }
        }

        internal static void EnsureDirectoryExists()
        {
            Directory.CreateDirectory(ConfigDirectory);
        }

        internal static Config? LoadConfigForMod(LoadedResoniteMod mod)
        {
            ConfigurationDefinition? definition = mod.ResoniteMod.BuildConfigurationDefinition();
            if (definition == null)
            {
                // if there's no definition, then there's nothing for us to do here
                return null;
            }

            string configFile = GetModConfigPath(mod);

            try
            {
                using StreamReader file = File.OpenText(configFile);
                using JsonTextReader reader = new(file);
                JObject json = JObject.Load(reader);
                Version version = new(json[VERSION_JSON_KEY]!.ToObject<string>(jsonSerializer));
                if (!AreVersionsCompatible(version, definition.Version))
                {
                    var handlingMode = mod.ResoniteMod.HandleIncompatibleConfigurationVersions(definition.Version, version);
                    switch (handlingMode)
                    {
                        case IncompatibleConfigHandling.Clobber:
                            Logger.WarnInternal($"{mod.ResoniteMod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}. Clobbering old config and starting fresh.");
                            return new Config(mod, definition);

                        case IncompatibleConfigHandling.ForceLoad:
                            // continue processing
                            break;

                        case IncompatibleConfigHandling.Error: // fall through to default
                        default:
                            mod.AllowSavingConfiguration = false;
                            throw new ConfigLoadException($"{mod.ResoniteMod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}");
                    }
                }
                foreach (ConfigKey key in definition.ConfigurationItemDefinitions)
                {
                    string keyName = key.Name;
                    try
                    {
                        JToken? token = json[VALUES_JSON_KEY]?[keyName];
                        if (token != null)
                        {
                            object? value = token.ToObject(key.ValueType(), jsonSerializer);
                            key.Set(value);
                        }
                    }
                    catch (Exception e)
                    {
                        // I know not what exceptions the JSON library will throw, but they must be contained
                        mod.AllowSavingConfiguration = false;
                        throw new ConfigLoadException($"Error loading {key.ValueType()} config key \"{keyName}\" for {mod.ResoniteMod.Name}", e);
                    }
                }
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

        internal static void RegisterShutdownHook(Harmony harmony)
        {
            try
            {
                MethodInfo shutdown = AccessTools.DeclaredMethod(typeof(Engine), nameof(Engine.RequestShutdown));
                if (shutdown == null)
                {
                    Logger.ErrorInternal("Could not find method Engine.Shutdown(). Will not be able to autosave configs on close!");
                    return;
                }
                MethodInfo patch = AccessTools.DeclaredMethod(typeof(Config), nameof(ShutdownHook));
                if (patch == null)
                {
                    Logger.ErrorInternal("Could not find method ModConfiguration.ShutdownHook(). Will not be able to autosave configs on close!");
                    return;
                }
                harmony.Patch(shutdown, prefix: new HarmonyMethod(patch));
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"Unexpected exception applying shutdown hook!\n{e}");
            }
        }

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

        private static bool AreVersionsCompatible(Version serializedVersion, Version currentVersion)
        {
            if (serializedVersion.Major != currentVersion.Major)
            {
                // major version differences are hard incompatible
                return false;
            }

            if (serializedVersion.Minor > currentVersion.Minor)
            {
                // if serialized config has a newer minor version than us
                // in other words, someone downgraded the mod but not the config
                // then we cannot load the config
                return false;
            }

            // none of the checks failed!
            return true;
        }

        private bool AnyValuesSet()
        {
            return ConfigurationItemDefinitions
                .Where(key => key.HasValue)
                .Any();
        }

        private void FireConfigChangedEvent(ConfigKey key, string? label)
        {
            try
            {
                OnAnyConfigurationChanged?.SafeInvoke(new ConfigChangedEvent(this, key, label));
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"An OnAnyConfigurationChanged event subscriber threw an exception:\n{e}");
            }

            try
            {
                OnThisConfigurationChanged?.SafeInvoke(new ConfigChangedEvent(this, key, label));
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"An OnThisConfigurationChanged event subscriber threw an exception:\n{e}");
            }
        }

        /// <summary>
        /// performs the actual, synchronous save
        /// </summary>
        /// <param name="saveDefaultValues">If true, default values will also be persisted</param>
        private void SaveInternal(bool saveDefaultValues = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
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

        /// <summary>
        /// Called if any config value for any mod changed.
        /// </summary>
        public static event ConfigChangedEventHandler? OnAnyConfigurationChanged;

        /// <summary>
        /// The delegate that is called for configuration change events.
        /// </summary>
        /// <param name="ConfigChangedEvent">The event containing details about the configuration change</param>
        public delegate void ConfigChangedEventHandler(ConfigChangedEvent ConfigChangedEvent);

        /// <summary>
        /// Called if one of the values in this mod's config changed.
        /// </summary>
        public event ConfigChangedEventHandler? OnThisConfigurationChanged;
    }
}