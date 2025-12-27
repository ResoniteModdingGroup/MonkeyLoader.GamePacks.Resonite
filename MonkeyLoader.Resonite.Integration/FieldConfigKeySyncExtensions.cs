using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods for <see cref="IField{T}">fields</see>
    /// that allow syncing them with <see cref="IDefiningConfigKey{T}"/>s.
    /// </summary>
    public static class FieldConfigKeySyncExtensions
    {
        /// <summary>
        /// The prefix for the <see cref="IDefiningConfigKey{T}.SetValue(T, string?)">SetValue</see>
        /// <c>eventLabel</c> used when the cause is a change in the synchronized <see cref="IField{T}">field</see>'s shared value.
        /// </summary>
        /// <remarks>
        /// The actually passed label has the following format:
        /// <c>$"{<see cref="WriteBackPrefix"/>}.{<see cref="ValueField{T}">field</see>.<see cref="World"/>.<see cref="SharedConfig.GetIdentifier">GetIdentifier</see>()}"</c>
        /// </remarks>
        public const string WriteBackPrefix = "SyncedField.WriteBack";

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s <see cref="IValue{T}.Value">Value</see> with that of the <paramref name="configKey"/>.
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
        /// It defaults to: <c>$"{<see cref="WriteBackPrefix">WriteBackPrefix</see>}.{<paramref name="field"/>.<see cref="IWorldElement.World">World</see>.<see cref="SharedConfig.GetIdentifier">GetIdentifier</see>()}"</c>
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
            => field.SyncWithConfigKeyUntyped(configKey, eventLabel, allowWriteBack);

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s nullable
        /// <see cref="IValue{T}.Value">Value</see> with that of the <paramref name="configKey"/>.
        /// </summary>
        /// <inheritdoc cref="SyncWithConfigKey{T}(IField{T}, IDefiningConfigKey{T}, string?, bool)"/>
        public static Action<IChangeable> SyncWithConfigKey<T>(this IField<T?> field,
                IDefiningConfigKey<T?> configKey, string? eventLabel = null, bool allowWriteBack = true)
            where T : struct
            => field.SyncWithConfigKeyUntyped(configKey, eventLabel, allowWriteBack);

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s boolean
        /// <see cref="IValue{T}.Value">Value</see> with whether the <paramref name="configKey"/>'s
        /// value has the <paramref name="referenceValue"/> flag set.<br/>
        /// When the <paramref name="configKey"/> value is <c>null</c>,
        /// the <paramref name="field"/>'s will always be set to false.
        /// </summary>
        /// <param name="referenceValue">The flag that is checked for in the <paramref name="configKey"/>'s value.</param>
        /// <inheritdoc cref="SyncWithConfigKey{T}(IField{T}, IDefiningConfigKey{T}, string?, bool)"/>
        public static Action<IChangeable> SyncWithConfigKeyEnumFlag<T>(this IField<bool> field,
                IDefiningConfigKey<T?> configKey, T referenceValue, string? eventLabel = null, bool allowWriteBack = true)
            where T : unmanaged, Enum
            => field.SyncWithConfigKeyEnumFlagUntyped(configKey, referenceValue, eventLabel, allowWriteBack);

#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

        /// <summary>
        /// Synchronizes the <paramref name="field"/>'s boolean
        /// <see cref="IValue{T}.Value">Value</see> with whether the <paramref name="configKey"/>'s
        /// value has the <paramref name="referenceValue"/> flag set.
        /// </summary>
        /// <inheritdoc cref="SyncWithConfigKeyEnumFlag{T}(IField{bool}, IDefiningConfigKey{T?}, T, string?, bool)"/>
        public static Action<IChangeable> SyncWithConfigKeyEnumFlag<T>(this IField<bool> field,
                IDefiningConfigKey<T> configKey, T referenceValue, string? eventLabel = null, bool allowWriteBack = true)
            where T : unmanaged, Enum
            => field.SyncWithConfigKeyEnumFlagUntyped(configKey, referenceValue, eventLabel, allowWriteBack);

        /// <summary><para>
        /// Synchronizes the <paramref name="field"/>'s boolean
        /// <see cref="IValue{T}.Value">Value</see> with whether the <paramref name="configKey"/>'s
        /// value has the <paramref name="referenceValue"/> flag set.<br/>
        /// If the <paramref name="configKey"/> value is <c>null</c>,
        /// the <paramref name="field"/>'s will always be set to false.
        /// </para><para><i>
        /// This method is mainly public for reflection based use,
        /// to simplify handling nullable values.<br/>
        /// In theory, the <paramref name="configKey"/> could be any (nullable) enum or even integer type.
        /// </i></para></summary>
        /// <inheritdoc cref="SyncWithConfigKeyEnumFlag{T}(IField{bool}, IDefiningConfigKey{T?}, T, string?, bool)"/>
        public static Action<IChangeable> SyncWithConfigKeyEnumFlagUntyped<T>(this IField<bool> field,
                IDefiningConfigKey configKey, T referenceValue, string? eventLabel = null, bool allowWriteBack = true)
            where T : unmanaged, Enum
        {
            var longReferenceValue = Convert.ToInt64(referenceValue);
            var isZeroReference = longReferenceValue == 0;

            var enumType = typeof(T);
            eventLabel ??= field.GetWriteBackEventLabel();

            if (configKey.GetValue() is null)
            {
                field.Value = false;
            }
            else
            {
                var currentValue = Convert.ToInt64(configKey.GetValue());

                field.Value = isZeroReference
                    ? currentValue == 0
                    : (currentValue & longReferenceValue) == longReferenceValue;
            }

            void FieldChanged(IChangeable changeable)
            {
                var currentValue = Convert.ToInt64(configKey.GetValue() ?? default(T));

                var newValue = field.Value
                    ? currentValue | longReferenceValue
                    : currentValue & ~longReferenceValue;

                // If writeback allowed, sensible, and successful: return
                if (!isZeroReference && allowWriteBack && configKey.TrySetValue(Enum.ToObject(enumType, newValue), eventLabel))
                    return;

                // Otherwise reset field value for current state
                field.World.RunSynchronously(() =>
                {
                    var currentValue = Convert.ToInt64(configKey.GetValue());
                    var newValue = isZeroReference
                        ? currentValue == 0
                        : (currentValue & longReferenceValue) == longReferenceValue;

                    field.SetWithoutChangedHandler(newValue, FieldChanged);
                });
            }

            void KeyChanged(object sender, IConfigKeyChangedEventArgs changedEvent)
            {
                if (field.FilterWorldElement() is null)
                {
                    configKey.Changed -= KeyChanged;
                    return;
                }

                if (changedEvent.NewValue is null)
                {
                    field.World.RunSynchronously(() => field.SetWithoutChangedHandler(false, FieldChanged));
                    return;
                }

                var newValue = Convert.ToInt64(changedEvent.NewValue);
                var isPartialCombinedValue = (newValue & longReferenceValue) != 0;

                field.World.RunSynchronously(() =>
                {
                    var newFieldValue = isZeroReference
                        ? newValue == 0
                        : (newValue & longReferenceValue) == longReferenceValue;

                    field.SetWithoutChangedHandler(newFieldValue, FieldChanged);
                });
            }

            SetupChangedHandlers(field, FieldChanged, configKey, KeyChanged);

            return FieldChanged;
        }

        /// <summary><para>
        /// Synchronizes the <paramref name="field"/>'s (nullable)
        /// <see cref="IValue{T}.Value">Value</see> with that of the <paramref name="configKey"/>.
        /// </para><para><i>
        /// This method is mainly public for reflection based use,
        /// to simplify handling nullable values.
        /// </i></para></summary>
        /// <inheritdoc cref="SyncWithConfigKey{T}(IField{T}, IDefiningConfigKey{T}, string?, bool)"/>
        public static Action<IChangeable> SyncWithConfigKeyUntyped<T>(this IField<T> field,
            IDefiningConfigKey configKey, string? eventLabel = null, bool allowWriteBack = true)
        {
            field.Value = (T)(configKey.GetValue() ?? default(T)!);
            eventLabel ??= field.GetWriteBackEventLabel();

            void FieldChanged(IChangeable _)
            {
                // If writeback allowed, sensible, and successful: return
                if (Equals(field.Value, configKey.GetValue())
                 || ((Nullable.GetUnderlyingType(configKey.ValueType)?.IsEnum ?? false) && configKey.GetValue() is null && Equals(field.Value, default(T)))
                 || (allowWriteBack && configKey.TrySetValue(field.Value, eventLabel)))
                    return;

                // Otherwise reset field value for current state
                field.World.RunSynchronously(() => field.Value = (T)(configKey.GetValue() ?? default(T)!));
            }

            void KeyChanged(object sender, IConfigKeyChangedEventArgs args)
            {
                if (Equals(field.Value, configKey.GetValue()))
                    return;

                field.World.RunSynchronously(() => field.Value = (T)(configKey.GetValue() ?? default(T)!));
            }

            SetupChangedHandlers(field, FieldChanged, configKey, KeyChanged);

            return FieldChanged;
        }

        /// <summary>
        /// Synchronizes this <paramref name="field"/>'s <see cref="IValue{T}.Value">Value</see> with
        /// the <see cref="Nullable{T}.HasValue"/> of the <paramref name="configKey"/>.<br/>
        /// When the field is toggled to <c>true</c> or set to <c>false</c>,
        /// the <see cref="Nullable{T}.Value"/> is set to <c>default(<typeparamref name="T"/>)</c>.
        /// </summary>
        /// <inheritdoc cref="SyncWithConfigKey{T}(IField{T}, IDefiningConfigKey{T}, string?, bool)"/>
        public static Action<IChangeable> SyncWithNullableConfigKeyHasValue<T>(this IField<bool> field,
                IDefiningConfigKey<T?> configKey, string? eventLabel = null, bool allowWriteBack = true)
            where T : struct
        {
            field.Value = configKey.GetValue().HasValue;
            eventLabel ??= field.GetWriteBackEventLabel();

            void FieldChanged(IChangeable _)
            {
                T? newValue = field.Value ? default(T) : null;

                if (field.Value == configKey.GetValue().HasValue || (allowWriteBack && configKey.TrySetValue(newValue, eventLabel)))
                    return;

                field.World.RunSynchronously(() => field.SetWithoutChangedHandler(configKey.GetValue().HasValue, FieldChanged));
            }

            void KeyChanged(object sender, ConfigKeyChangedEventArgs<T?> args)
            {
                if (field.Value == configKey.GetValue().HasValue)
                    return;

                field.World.RunSynchronously(() => field.SetWithoutChangedHandler(configKey.GetValue().HasValue, FieldChanged));
            }

            SetupChangedHandlers(field, FieldChanged, configKey, KeyChanged);

            return FieldChanged;
        }

        private static string GetWriteBackEventLabel(this IField field)
            => $"{WriteBackPrefix}.{field.World.GetIdentifier()}";

        private static void SetupChangedHandlers<T>(IField field, Action<IChangeable> fieldChangedHandler,
            IDefiningConfigKey<T> configKey, ConfigKeyChangedEventHandler<T> configKeyChangedHandler)
        {
            var parent = field.FindNearestParent<Component>();

            void ParentDestroyedHandler(IDestroyable _)
            {
                parent.Destroyed -= ParentDestroyedHandler;

                field.Changed -= fieldChangedHandler;
                configKey.Changed -= configKeyChangedHandler;
            }

            field.Changed += fieldChangedHandler;
            configKey.Changed += configKeyChangedHandler;
            parent.Destroyed += ParentDestroyedHandler;
        }

        private static void SetupChangedHandlers(IField field, Action<IChangeable> fieldChangedHandler,
            IDefiningConfigKey configKey, ConfigKeyChangedEventHandler configKeyChangedHandler)
        {
            var parent = field.FindNearestParent<Component>();

            void ParentDestroyedHandler(IDestroyable _)
            {
                parent.Destroyed -= ParentDestroyedHandler;

                field.Changed -= fieldChangedHandler;
                configKey.Changed -= configKeyChangedHandler;
            }

            field.Changed += fieldChangedHandler;
            configKey.Changed += configKeyChangedHandler;
            parent.Destroyed += ParentDestroyedHandler;
        }

        private static void SetWithoutChangedHandler<T>(this IField<T> field, T value, Action<IChangeable> changedHandler)
        {
            field.Changed -= changedHandler;
            field.Value = value;
            field.Changed += changedHandler;
        }
    }
}