using FrooxEngine;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    /// <summary>
    /// Defines the untyped interface for <see cref="IDefiningConfigKey{T}"/> wrappers,
    /// which make its local value available as a shared resource in Resonite sessions,
    /// and optionally allow writing back changes from the session to the config item.
    /// </summary>
    public interface ISharedDefiningConfigKey : IDefiningConfigKeyWrapper
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
        /// Gets a sequence of users who have defined overrides for the shared value.
        /// </summary>
        /// <param name="world">The world to get the sharing users for.</param>
        /// <returns>The users who have defined overrides.</returns>
        public IEnumerable<User> GetSharingUsers(World world);

        /// <summary>
        /// Ensures that the shared value field and overrides for
        /// this config item exist in the given <see cref="World"/>.
        /// </summary>
        /// <param name="world"></param>
        public void SetupOverride(World world);
    }

    /// <summary>
    /// Represents a wrapper for an <see cref="IDefiningConfigKey{T}"/>,
    /// which makes its local value available as a shared resource in Resonite sessions.<br/>
    /// Optionally allows writing back changes from the session to the config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    public sealed class SharedDefiningConfigKey<T> : DefiningConfigKeyWrapper<T>, ISharedDefiningConfigKey
    {
        private readonly ConditionalWeakTable<World, World> _didWorldSetup = new();
        private readonly Lazy<string> _sharedId;
        private readonly Lazy<string> _variableName;
        private T? _defaultValue;

        /// <inheritdoc/>
        public bool AllowWriteBack { get; set; }

        object? ISharedDefiningConfigKey.DefaultValue
        {
            get => DefaultValue;
            set => DefaultValue = (T?)value;
        }

        /// <summary>
        /// Gets or sets the default value for the shared config item for users that don't have it themselves.
        /// </summary>
        public T? DefaultValue
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

        /// <inheritdoc/>
        public string SharedId => _sharedId.Value;

        /// <inheritdoc/>
        public string VariableName => _variableName.Value;

        /// <summary>
        /// Creates a new wrapper for the given <paramref name="definingKey"/>,
        /// which makes its local value available as a shared resource in Resonite sessions.<br/>
        /// Optionally allows writing back changes from the session to the config item.
        /// </summary>
        /// <param name="definingKey">The defining key to wrap.</param>
        /// <param name="defaultValue">The default value for the shared config item for users that don't have it themselves.</param>
        /// <param name="allowWriteBack">Whether to allow writing back changes from the session to the config item.</param>
        public SharedDefiningConfigKey(IDefiningConfigKey<T> definingKey,
            T? defaultValue = default, bool allowWriteBack = false) : base(definingKey)
        {
            _defaultValue = defaultValue;
            AllowWriteBack = allowWriteBack;
            Changed += ValueChanged;

            _sharedId = new(() => $"{SharedConfig.Identifier}.{FullId}");
            _variableName = new(() => $"World/{SharedId}");

            SharedConfig.Register(this);
        }

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
        public ValueCopy<T> Drive(IField<T> field, bool writeBack = false)
        {
            if (!AllowWriteBack && writeBack)
                throw new InvalidOperationException("Can't enable write back on a drive if it's not enabled for the config item!");

            return field.DriveFrom(GetSharedValue(field.World).Value, writeBack);
        }

        /// <summary>
        /// Creates a <see cref="DynamicValueVariableDriver{T}"/> on the given <paramref name="field"/>'s
        /// parent <see cref="Slot"/>, which drives it from the shared value.
        /// </summary>
        /// <param name="field">The field to drive with the shared value.</param>
        /// <returns>The created <see cref="DynamicValueVariableDriver{T}"/> component.</returns>
        public DynamicValueVariableDriver<T> DriveFromVariable(IField<T> field)
            => field.DriveFromVariable(VariableName);

        /// <summary>
        /// Gets this shared config item's <see cref="Slot"/> under the
        /// <see cref="SharedConfig.GetSharedConfigSlot(World)">SharedConfig slot</see>.
        /// </summary>
        /// <param name="world">The <see cref="World"/> to get the <see cref="Slot"/> for.</param>
        /// <returns>This shared config item's SharedConfig slot for the given world.</returns>
        public Slot GetSharedConfigSlot(World world)
            => world.GetSharedConfigSlot(Config.Owner);

        /// <inheritdoc/>
        public IEnumerable<User> GetSharingUsers(World world)
            => GetSharedOverride(world)._overrides
                .Select(valueOverride => valueOverride.Value.User.Target)
                .Where(user => user is not null);

        /// <inheritdoc/>
        public void SetupOverride(World world)
            => world.RunSynchronously(() => GetSharedValue(world));

        private ValueUserOverride<T> GetSharedOverride(World world)
            => GetSharedValue(world).Value.GetUserOverride();

        private ValueField<T> GetSharedValue(World world)
            => world.GetSharedComponentOrCreate<ValueField<T>>(SharedId, SetupSharedField, 0, true, true, () => GetSharedConfigSlot(world));

        private void SetupSharedField(ValueField<T> field)
        {
            if (_didWorldSetup.TryGetValue(field.World, out _))
                return;

            if (!field.IsDriven && EqualityComparer<T>.Default.Equals(field.Value, default!))
                field.Value.Value = DefaultValue!;

            field.Value.GetSyncWithVariable(VariableName);

            var vuo = field.Value.OverrideForUser(field.World.LocalUser, GetValue()!);
            vuo.CreateOverrideOnWrite.Value = true;

            field.Value.OnValueChange += SharedValueChanged;

            _didWorldSetup.TryAdd(field.World, field.World);
        }

        private void SharedValueChanged(SyncField<T> field)
        {
            if (AllowWriteBack)
                SetValue(field.Value, $"{SharedConfig.WriteBackPrefix}.{field.World.GetIdentifier()}");
            else
                field.World.RunSynchronously(() => field.Value = GetValue()!);
        }

        private void ValueChanged(object sender, ConfigKeyChangedEventArgs<T> configKeyChangedEventArgs)
        {
            if (Engine.Current?.WorldManager is null)
                return;

            configKeyChangedEventArgs.TryGetWorldIdentifier(out var worldIdentifier);

            foreach (var world in Engine.Current.WorldManager.Worlds.Where(world => world.GetIdentifier() != worldIdentifier))
                world.RunSynchronously(() => GetSharedValue(world).Value.Value = configKeyChangedEventArgs.NewValue!);
        }
    }
}