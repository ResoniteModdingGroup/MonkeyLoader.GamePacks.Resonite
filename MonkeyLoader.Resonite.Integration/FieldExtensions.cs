using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods for <see cref="IField{T}">fields</see>
    /// and other <see cref="IWorldElement">world elements</see>
    /// </summary>
    public static class FieldExtensions
    {
        /// <summary>
        /// The prefix for the <see cref="IDefiningConfigKey{T}.SetValue(T, string?)">SetValue</see>
        /// <c>eventLabel</c> used when the cause is a change in the synchronized <see cref="IField{T}">field</see>'s shared value.
        /// </summary>
        /// <remarks>
        /// The actually passed label has the following format:
        /// <c>$"{<see cref="WriteBackPrefix"/>}.{<see cref="ValueField{T}">field</see>.<see cref="World"/>.<see cref="GetIdentifier">GetIdentifier</see>()}"</c>
        /// </remarks>
        public const string WriteBackPrefix = "SyncedField.WriteBack";

        /// <summary>
        /// Creates a label describing the <paramref name="target"/> reference as a <see cref="RefEditor"/> would.
        /// </summary>
        /// <param name="target">The reference to label.</param>
        /// <returns>A label for the <paramref name="target"/> reference if it is not <c>null</c>; otherwise, <c>&lt;i&gt;null&lt;/i&gt;</c>.</returns>
        public static string GetReferenceLabel(this IWorldElement? target)
        {
            if (target is null)
                return "<i>null</i>";

            if (target is Slot targetSlot)
                return $"{targetSlot.Name} ({target.ReferenceID})";

            var component = target.FindNearestParent<Component>();
            var slot = component?.Slot ?? target.FindNearestParent<Slot>();

            var arg = (component is not null && component != target) ? ("on " + component.Name + " on " + slot.Name) : ((slot is null) ? "" : ("on " + slot.Name));
            return (target is not SyncElement syncElement) ? $"{target.Name ?? target.GetType().Name} {arg} ({target.ReferenceID})" : $"{syncElement.NameWithPath} {arg} ({target.ReferenceID})";
        }

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s <see cref="IValue{T}.Value"/> with that of the <paramref name="configKey"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <paramref name="allowWriteBack"/> is <c>true</c>,
        /// changes to the <paramref name="field"/> get written back to the config key.<br/>
        /// If <paramref name="allowWriteBack"/> is <c>false</c> or
        /// <see cref="IDefiningConfigKey{T}.TrySetValue">setting the value</see> fails,
        /// the <paramref name="field"/>'s value is reset back to the config key's value.
        /// </para>
        /// <para>
        /// The <paramref name="eventLabel"/> is passed when
        /// <see cref="IDefiningConfigKey{T}.TrySetValue">setting the value</see> of the config key.<br/>
        /// It defaults to: <c>$"SyncedField.WriteBack.{<paramref name="field"/>.<see cref="IWorldElement.World">World</see>.<see cref="SharedConfig.GetIdentifier">GetIdentifier</see>()}"</c>
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the field's value.</typeparam>
        /// <param name="field">The field to synchronize with.</param>
        /// <param name="configKey">The config key to synchronize with.</param>
        /// <param name="eventLabel">
        /// Optional override for the event label passed when writing back a change of the field.<br/>
        /// Defaults to: <c>$"SyncedField.WriteBack.{<paramref name="field"/>.<see cref="IWorldElement.World">World</see>.<see cref="SharedConfig.GetIdentifier">GetIdentifier</see>()}"</c>
        /// </param>
        /// <param name="allowWriteBack">Whether changes of the <paramref name="field"/> should be written back to the config key.</param>
        /// <returns>The delegate subscribed to the <paramref name="field"/>'s <see cref="IChangeable.Changed">Changed</see> event.</returns>
        public static Action<IChangeable> SyncWithConfigKey<T>(this IField<T> field,
            IDefiningConfigKey<T> configKey, string? eventLabel = null, bool allowWriteBack = true)
        {
            field.Value = configKey.GetValue()!;
            eventLabel ??= field.GetWriteBackEventLabel();

            var parent = field.FindNearestParent<Component>();

            void ParentDestroyedHandler(IDestroyable _)
            {
                parent.Destroyed -= ParentDestroyedHandler;

                field.Changed -= FieldChangedHandler;
                configKey.Changed -= ConfigKeyChangedHandler;
            }

            void FieldChangedHandler(IChangeable _)
            {
                if (Equals(field.Value, configKey.GetValue()) && (!allowWriteBack || !configKey.TrySetValue(field.Value, eventLabel)))
                    field.World.RunSynchronously(() => field.Value = configKey.GetValue()!);
            }

            void ConfigKeyChangedHandler(object sender, ConfigKeyChangedEventArgs<T> args)
            {
                if (!Equals(field.Value, configKey.GetValue()))
                    field.World.RunSynchronously(() => field.Value = configKey.GetValue()!);
            }

            field.Changed += FieldChangedHandler;
            configKey.Changed += ConfigKeyChangedHandler;
            parent.Destroyed += ParentDestroyedHandler;

            return FieldChangedHandler;
        }

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s <see cref="IValue{T}.Value"/> with
        /// the <see cref="Nullable{T}.HasValue"/> of the <paramref name="configKey"/>.<br/>
        /// When the field is toggled to <c>true</c> or set to <c>false</c>,
        /// the <see cref="Nullable{T}.Value"/> is set to <c>default(<typeparamref name="T"/>)</c>.
        /// </summary>
        /// <inheritdoc cref="SyncWithConfigKey{T}(IField{T}, IDefiningConfigKey{T}, string?, bool)"/>
        public static Action<IChangeable> SyncWithNullableConfigKeyHasValue<T>(this IField<bool> field,
            IDefiningConfigKey<T?> configKey, string? eventLabel = null, bool allowWriteBack = true)
            where T : unmanaged
        {
            field.Value = configKey.GetValue().HasValue;
            eventLabel ??= field.GetWriteBackEventLabel();

            var parent = field.FindNearestParent<Component>();

            void ParentDestroyedHandler(IDestroyable _)
            {
                parent.Destroyed -= ParentDestroyedHandler;

                field.Changed -= FieldChangedHandler;
                configKey.Changed -= ConfigKeyChangedHandler;
            }

            void FieldChangedHandler(IChangeable _)
            {
                T? newValue = field.Value ? default : null;
                var keyValue = configKey.GetValue().HasValue;

                if (field.Value != keyValue && (!allowWriteBack || !configKey.TrySetValue(newValue, eventLabel)))
                    field.World.RunSynchronously(() => field.Value = keyValue);
            }

            void ConfigKeyChangedHandler(object sender, ConfigKeyChangedEventArgs<T?> args)
            {
                var keyValue = configKey.GetValue().HasValue;

                if (field.Value != keyValue)
                    field.World.RunSynchronously(() => field.Value = keyValue);
            }

            field.Changed += FieldChangedHandler;
            configKey.Changed += ConfigKeyChangedHandler;
            parent.Destroyed += ParentDestroyedHandler;

            return FieldChangedHandler;
        }

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s <see cref="IValue{T}.Value"/> with
        /// the <see cref="Nullable{T}.Value"/> of the <paramref name="configKey"/>.<br/>
        /// When the <see cref="Nullable{T}.HasValue"/> is <c>false</c>,
        /// the <paramref name="field"/> is set to <c>default(<typeparamref name="T"/>)</c>.
        /// </summary>
        /// <inheritdoc cref="SyncWithConfigKey{T}(IField{T}, IDefiningConfigKey{T}, string?, bool)"/>
        public static Action<IChangeable> SyncWithNullableConfigKeyValue<T>(this IField<T> field,
                IDefiningConfigKey<T?> configKey, string? eventLabel = null, bool allowWriteBack = true)
            where T : unmanaged
        {
            // support for nullables requires this to never be null
            field.Value = configKey.GetValue() ?? default;
            eventLabel ??= field.GetWriteBackEventLabel();

            var parent = field.FindNearestParent<Component>();

            void ParentDestroyedHandler(IDestroyable _)
            {
                parent.Destroyed -= ParentDestroyedHandler;

                field.Changed -= FieldChangedHandler;
                configKey.Changed -= ConfigKeyChangedHandler;
            }

            void FieldChangedHandler(IChangeable _)
            {
                var keyValue = configKey.GetValue() ?? default;

                if (!Equals(field.Value, keyValue) && (!allowWriteBack || !configKey.TrySetValue(field.Value, eventLabel)))
                    field.World.RunSynchronously(() => field.Value = keyValue);
            }

            void ConfigKeyChangedHandler(object sender, ConfigKeyChangedEventArgs<T?> args)
            {
                var keyValue = configKey.GetValue() ?? default;

                if (!Equals(field.Value, keyValue))
                    field.World.RunSynchronously(() => field.Value = keyValue);
            }

            field.Changed += FieldChangedHandler;
            configKey.Changed += ConfigKeyChangedHandler;
            parent.Destroyed += ParentDestroyedHandler;

            return FieldChangedHandler;
        }

        private static string GetWriteBackEventLabel(this IField field)
            => $"SyncedField.WriteBack.{field.World.GetIdentifier()}";
    }
}