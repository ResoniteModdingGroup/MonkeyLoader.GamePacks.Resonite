using FrooxEngine;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Configuration
{
    public interface ISharedDefiningConfigKeyWrapper : IDefiningConfigKeyWrapper
    {
        public bool AllowWriteBack { get; set; }

        public string SharedId { get; }

        public string VariableName { get; }

        public IEnumerable<User> GetSharingUsers(World world);

        public void SetupOverride(World world);
    }

    /// <summary>
    /// Represents a wrapper for an <see cref="IDefiningConfigKey{T}"/>,
    /// which makes its local value available as a shared resource in Resonite sessions.<br/>
    /// Optionally allows writing back changes from the session to the config item.
    /// </summary>
    /// <typeparam name="T">The type of the config item's value.</typeparam>
    public sealed class Shared<T> : DefiningConfigKeyWrapper<T>, ISharedDefiningConfigKeyWrapper
    {
        private readonly Lazy<string> _sharedId;
        private readonly Lazy<string> _variableName;

        /// <summary>
        /// Gets or sets whether to allow writing back changes from the session to the config item.
        /// </summary>
        public bool AllowWriteBack { get; set; }

        public T? DefaultValue { get; }
        public string SharedId => _sharedId.Value;

        public string VariableName => _variableName.Value;

        /// <summary>
        /// Creates a new wrapper for the given <paramref name="definingKey"/>,
        /// which makes its local value available as a shared resource in Resonite sessions.<br/>
        /// Optionally allows writing back changes from the session to the config item.
        /// </summary>
        /// <param name="definingKey">The defining key to wrap.</param>
        /// <param name="defaultValue">The default value for the shared config item.</param>
        /// <param name="allowWriteBack">Whether to allow writing back changes from the session to the config item.</param>
        public Shared(IDefiningConfigKey<T> definingKey, T? defaultValue = default, bool allowWriteBack = false) : base(definingKey)
        {
            DefaultValue = defaultValue;
            AllowWriteBack = allowWriteBack;
            Changed += ValueChanged;

            _sharedId = new(() => $"{SharedConfig.Identifier}.{FullId}");
            _variableName = new(() => $"World/{SharedId}");

            SharedConfig.Register(this);
        }

        public ValueCopy<T> DriveValueDirectly(IField<T> field, bool writeBack = false)
        {
            if (!AllowWriteBack && writeBack)
                throw new InvalidOperationException("Can't enable write back on a drive if it's not enabled for the config item!");

            return field.DriveFrom(GetSharedValue(field.World).Value, writeBack);
        }

        public DynamicValueVariableDriver<T> DriveValueFromVariable(IField<T> field)
            => field.DriveFromVariable(VariableName);

        public IEnumerable<User> GetSharingUsers(World world)
            => GetSharedOverride(world)._overrides
                .Select(valueOverride => valueOverride.Value.User.Target)
                .Where(user => user is not null);

        public void SetSharedDefault(T? newDefault)
        {
            if (Engine.Current.WorldManager is null)
                return;

            foreach (var world in Engine.Current.WorldManager.Worlds)
                GetSharedOverride(world).Default.Value = newDefault!;
        }

        /// <inheritdoc/>
        public void SetupOverride(World world)
            => GetSharedValue(world);

        private ValueUserOverride<T> GetSharedOverride(World world)
            => GetSharedValue(world).Value.GetUserOverride();

        private ValueField<T> GetSharedValue(World world)
            => world.GetSharedComponentOrCreate<ValueField<T>>(SharedId, SetupSharedField, 0, true, true, world.GetSharedConfigSlot);

        private void SetupSharedField(ValueField<T> field)
        {
            field.Value.SyncWithVariable(VariableName);
            field.Changed += SharedValueChanged;

            var vuo = field.Value.OverrideForUser(field.World.LocalUser, GetValue()!);
            vuo.CreateOverrideOnWrite.Value = true;
            vuo.Default.Value = DefaultValue!;
        }

        private void SharedValueChanged(IChangeable changeable)
        {
            var field = (ValueField<T>)changeable;

            if (AllowWriteBack)
                field.RunSynchronously(() => SetValue(field.Value.Value, $"{SharedConfig.WriteBackPrefix}.{field.World.Name}"));
            else
                field.RunSynchronously(() => field.Value.Value = GetValue()!);
        }

        private void ValueChanged(object sender, ConfigKeyChangedEventArgs<T> configKeyChangedEventArgs)
        {
            if (Engine.Current.WorldManager is null)
                return;

            foreach (var world in Engine.Current.WorldManager.Worlds)
                world.RunSynchronously(() => GetSharedValue(world).Value.Value = configKeyChangedEventArgs.NewValue!);
        }
    }
}