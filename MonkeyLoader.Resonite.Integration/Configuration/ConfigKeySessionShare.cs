using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Represents a wrapper for an <see cref="IDefiningConfigKey{T}"/>,
    /// which makes its local value available as a (converted) shared resource in Resonite <see cref="World"/>s.<br/>
    /// Optionally allows writing back changes from the <see cref="World"/> to the config item.
    /// </summary>
    /// <typeparam name="TKey">The type of the config item's value.</typeparam>
    /// <typeparam name="TShared">
    /// The type of the resource shared in Resonite <see cref="World"/>s.
    /// Must be a valid generic parameter for <see cref="ValueField{T}"/> components.
    /// </typeparam>
    public class ConfigKeySessionShare<TKey, TShared> : IConfigKeySessionShare<TKey, TShared>
    {
        private readonly Converter<TShared?, TKey?> _convertToKey;
        private readonly Converter<TKey?, TShared?> _convertToShared;
        private readonly Lazy<string> _sharedId;
        private readonly Lazy<string> _variableName;
        private IDefiningConfigKey<TKey> _configKey = null!;
        private TShared? _defaultValue;

        /// <inheritdoc/>
        public bool AllowWriteBack { get; set; }

        /// <inheritdoc/>
        public TShared? DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;

                if (Engine.Current.WorldManager is null)
                    return;

                foreach (var world in Engine.Current.WorldManager.Worlds)
                    GetSharedOverride(world).Default.Value = value!;
            }
        }

        object? IConfigKeySessionShare.DefaultValue
        {
            get => DefaultValue;
            set => DefaultValue = (TShared)value!;
        }

        /// <inheritdoc/>
        public string SharedId => _sharedId.Value;

        /// <inheritdoc/>
        public string VariableName => _variableName.Value;

        /// <summary>
        /// Creates a new <see cref="IConfigKeySessionShare{T}"/> component,
        /// which makes its config key's local value available as a shared resource in Resonite sessions.<br/>
        /// Optionally allows writing back changes from the session to the config item.
        /// </summary>
        /// <param name="convertToShared">Converts the config item's value to the shared resource's.</param>
        /// <param name="convertToKey">Converts the shared resource's value to the config item's.</param>
        /// <param name="defaultValue">
        /// The default value for the shared config item for users that don't have it themselves.<br/>
        /// Gets converted to <typeparamref name="TShared"/> using <paramref name="convertToShared"/>.
        /// </param>
        /// <param name="allowWriteBack">Whether to allow writing back changes from the session to the config item.</param>
        /// <exception cref="InvalidOperationException">When <typeparamref name="TShared"/> is an invalid generic argument for <see cref="ValueField{T}"/> components.</exception>
        public ConfigKeySessionShare(Converter<TKey?, TShared?> convertToShared, Converter<TShared?, TKey?> convertToKey,
            TKey? defaultValue = default, bool allowWriteBack = false)
        {
            if (!typeof(ValueField<TShared>).IsValidGenericType(true))
                throw new InvalidOperationException("TShared must be a valid generic argument for ValueField<T> components!");

            _convertToShared = convertToShared;
            _convertToKey = convertToKey;

            _defaultValue = convertToShared(defaultValue);
            AllowWriteBack = allowWriteBack;

            _sharedId = new(() => $"{SharedConfig.Identifier}.{_configKey!.FullId}");
            _variableName = new(() => $"World/{SharedId}");
        }

        /// <inheritdoc/>
        public TKey? ConvertToKey(TShared? sharedValue) => _convertToKey(sharedValue);

        object? IConfigKeySessionShare.ConvertToKey(object? sharedValue) => _convertToKey((TShared?)sharedValue);

        /// <inheritdoc/>
        public TShared? ConvertToShared(TKey? keyValue) => _convertToShared(keyValue);

        object? IConfigKeySessionShare.ConvertToShared(object? keyValue) => _convertToShared((TKey?)keyValue);

        /// <summary>
        /// Creates a <see cref="ValueCopy{T}"/> on the given <paramref name="field"/>'s
        /// parent <see cref="Slot"/>, which drives it from the shared value.
        /// </summary>
        /// <param name="field">The field to drive with the shared value.</param>
        /// <param name="writeBack">
        /// Whether to allow changes to the driven field and propagate them back to the shared value.<br/>
        /// <see cref="AllowWriteBack">AllowWriteBack</see> must be <c>true</c> when enabling this.
        /// </param>
        /// <returns>The created <see cref="ValueCopy{T}"/> component.</returns>
        /// <exception cref="InvalidOperationException">When <paramref name="writeBack"/> is <c>true</c> and <see cref="AllowWriteBack">AllowWriteBack</see> isn't.</exception>
        public ValueCopy<TShared> Drive(IField<TShared> field, bool writeBack = false)
        {
            if (!AllowWriteBack && writeBack)
                throw new InvalidOperationException("Can't enable write back on a drive if it's not enabled for the config item!");

            return field.DriveFrom(GetSharedValue(field.World).Value, writeBack);
        }

        /// <summary>
        /// Creates a <see cref="DynamicValueVariableDriver{T}"/> on the given <paramref name="field"/>'s
        /// parent <see cref="Slot"/>, which drives it from the shared value.<br/>
        /// The driver's <see cref="DynamicValueVariableDriver{T}.DefaultValue">DefaultValue</see>
        /// is set to the shared value's <see cref="DefaultValue">DefaultValue</see>.
        /// </summary>
        /// <param name="field">The field to drive with the shared value.</param>
        /// <returns>The created <see cref="DynamicValueVariableDriver{T}"/> component.</returns>
        public DynamicValueVariableDriver<TShared> DriveFromVariable(IField<TShared> field)
        {
            // Get Shared Value to ensure that the necessary components exist
            GetSharedValue(field.World);

            var driver = field.DriveFromVariable(VariableName);
            driver.DefaultValue.Value = DefaultValue!;

            return driver;
        }

        /// <summary>
        /// Gets this shared config item's <see cref="Slot"/> under the
        /// <see cref="SharedConfig.GetSharedConfigSlot(World)">SharedConfig slot</see>.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to get the <see cref="Slot"/> for.</param>
        /// <returns>This shared config item's SharedConfig slot for the given world.</returns>
        public Slot GetSharedConfigSlot(World world)
            => world.GetSharedConfigSlot(_configKey.Config.Owner);

        /// <inheritdoc/>
        public IEnumerable<User> GetSharingUsers(World world)
            => GetSharedOverride(world)._overrides
                .Select(valueOverride => valueOverride.Value.User.Target)
                .Where(user => user is not null);

        void IComponent<IDefiningConfigKey<TKey>>.Initialize(IDefiningConfigKey<TKey> entity)
        {
            _configKey = entity;
            entity.Changed += ValueChanged;

            SharedConfig.Register(this);
        }

        /// <inheritdoc/>
        public void SetupOverride(World world)
            => world.RunSynchronously(() => GetSharedValue(world));

        /// <inheritdoc/>
        public void ShutdownOverride(World world)
            => world.RunSynchronously(() => GetSharedValue(world).Value.OnValueChange -= SharedValueChanged);

        private ValueUserOverride<TShared> GetSharedOverride(World world)
            => GetSharedValue(world).Value.GetUserOverride();

        private ValueField<TShared> GetSharedValue(World world)
            => world.GetSharedComponentOrCreate<ValueField<TShared>>(SharedId, SetupSharedField, 0, true, false, () => GetSharedConfigSlot(world));

        private void SetupSharedField(ValueField<TShared> field)
        {
            if (!field.IsDriven && EqualityComparer<TShared>.Default.Equals(field.Value, default!))
                field.Value.Value = DefaultValue!;

            field.Value.GetSyncWithVariable(VariableName);

            var vuo = field.Value.OverrideForUser(field.World.LocalUser, ConvertToShared(_configKey.GetValue())!);
            vuo.CreateOverrideOnWrite.Value = true;

            field.Value.OnValueChange += SharedValueChanged;
        }

        private void SharedValueChanged(SyncField<TShared> field)
        {
            if (!AllowWriteBack || !_configKey.TrySetValue(ConvertToKey(field.Value)!, $"{SharedConfig.WriteBackPrefix}.{field.World.GetIdentifier()}"))
            {
                field.World.RunSynchronously(() => field.Value = ConvertToShared(_configKey.GetValue())!);
            }
        }

        private void ValueChanged(object sender, ConfigKeyChangedEventArgs<TKey> configKeyChangedEventArgs)
        {
            if (Engine.Current?.WorldManager is null)
                return;

            configKeyChangedEventArgs.TryGetWorldIdentifier(out var worldIdentifier);

            foreach (var world in Engine.Current.WorldManager.Worlds.Where(world => world.GetIdentifier() != worldIdentifier))
                world.RunSynchronously(() => GetSharedValue(world).Value.Value = ConvertToShared(configKeyChangedEventArgs.NewValue)!);
        }
    }

    /// <summary>
    /// Represents a wrapper for an <see cref="IDefiningConfigKey{T}"/>,
    /// which makes its local value available as a shared resource in Resonite <see cref="World"/>s.<br/>
    /// Optionally allows writing back changes from the <see cref="World"/> to the config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    public sealed class ConfigKeySessionShare<T> : ConfigKeySessionShare<T, T>, IConfigKeySessionShare<T>
    {
        /// <summary>
        /// Creates a new <see cref="IConfigKeySessionShare{T}"/> component,
        /// which makes its config key's local value available as a shared resource in Resonite sessions.<br/>
        /// Optionally allows writing back changes from the session to the config item.
        /// </summary>
        /// <param name="defaultValue">The default value for the shared config item for users that don't have it themselves.</param>
        /// <param name="allowWriteBack">Whether to allow writing back changes from the session to the config item.</param>
        public ConfigKeySessionShare(T? defaultValue = default!, bool allowWriteBack = false)
            : base(Identity, Identity, defaultValue, allowWriteBack)
        { }

        [return: NotNullIfNotNull(nameof(item))]
        private static T? Identity(T? item) => item;
    }

    /// <summary>
    /// Defines the untyped interface for config key components,
    /// which make the key's local value available as a shared resource in Resonite sessions,
    /// and optionally allow writing back changes from the session to the config item.
    /// </summary>
    public interface IConfigKeySessionShare
    {
        /// <summary>
        /// Gets or sets whether to allow writing back changes from the session to the config item.
        /// </summary>
        public bool AllowWriteBack { get; set; }

        /// <summary>
        /// Gets or sets the default value for the shared config item for users that don't have it themselves.
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Gets the <see cref="World.RequestKey">key</see> used in <see cref="World"/>s
        /// to identify the <see cref="ValueField{T}"/> that stores the shared value.
        /// </summary>
        public string SharedId { get; }

        /// <summary>
        /// Gets the full name of the dynamic variable that is linked to the shared value.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// Converts the given value from the shared resource's type to the config item's.
        /// </summary>
        /// <remarks>
        /// May throw when the provided input isn't compatible.
        /// </remarks>
        /// <param name="sharedValue">The value suitable for the shared resource to be converted.</param>
        /// <returns>The value converted to the config item's type.</returns>
        public object? ConvertToKey(object? sharedValue);

        /// <summary>
        /// Converts the given value from the config item's type to the shared resource's.
        /// </summary>
        /// <remarks>
        /// May throw when the provided input isn't compatible.
        /// </remarks>
        /// <param name="keyValue">The value suitable for the config item to be converted.</param>
        /// <returns>The value converted to the shared resource's type.</returns>
        public object? ConvertToShared(object? keyValue);

        /// <summary>
        /// Gets a sequence of users who have defined overrides for the shared value.
        /// </summary>
        /// <param name="world">The world to get the sharing users for.</param>
        /// <returns>The users who have defined overrides.</returns>
        public IEnumerable<User> GetSharingUsers(World world);

        /// <summary>
        /// Ensures that the shared value field and overrides for
        /// this config item exist in the given <see cref="World"/>.
        /// </summary>
        /// <param name="world">The world to set up the override for.</param>
        public void SetupOverride(World world);

        /// <summary>
        /// Removes the connection between this config item
        /// and the given <see cref="World"/>'s shared value field.
        /// </summary>
        /// <param name="world">The world to remove the connection from.</param>
        public void ShutdownOverride(World world);
    }

    /// <summary>
    /// Defines the interface for config key components,
    /// which make the key's local value available as a shared resource in Resonite <see cref="World"/>s,
    /// and optionally allow writing back changes from the <see cref="World"/> to the config item.
    /// </summary>
    /// <typeparam name="TKey">The type of the config item's value.</typeparam>
    /// <typeparam name="TShared">The type of the resource shared in Resonite <see cref="World"/>s.</typeparam>
    public interface IConfigKeySessionShare<TKey, TShared> : IConfigKeyComponent<IDefiningConfigKey<TKey>>, IConfigKeySessionShare
    {
        /// <summary>
        /// Gets or sets the default value for the shared config item for users that don't have it themselves.
        /// </summary>
        public new TShared? DefaultValue { get; set; }

        /// <summary>
        /// Converts the given value from the shared resource's type to the config item's.
        /// </summary>
        /// <remarks>
        /// May throw when the provided input isn't compatible.
        /// </remarks>
        /// <param name="sharedValue">The value suitable for the shared resource to be converted.</param>
        /// <returns>The value converted to the config item's type.</returns>
        public TKey? ConvertToKey(TShared? sharedValue);

        /// <summary>
        /// Converts the given value from the config item's type to the shared resource's.
        /// </summary>
        /// <remarks>
        /// May throw when the provided input isn't compatible.
        /// </remarks>
        /// <param name="keyValue">The value suitable for the config item to be converted.</param>
        /// <returns>The value converted to the shared resource's type.</returns>
        public TShared? ConvertToShared(TKey? keyValue);
    }

    /// <summary>
    /// Defines the interface for config key components,
    /// which make the key's local value available as a shared resource in Resonite sessions,
    /// and optionally allow writing back changes from the session to the config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value and the resource shared in Resonite <see cref="World"/>.</typeparam>
    public interface IConfigKeySessionShare<T> : IConfigKeySessionShare<T, T>
    { }
}