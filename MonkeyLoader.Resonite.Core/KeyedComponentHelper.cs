using FrooxEngine;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonkeyLoader.Resonite
{
    /// <summary>
    /// Contains extension methods to deal with <see cref="World.Keys">keyed</see>
    /// <see cref="Component"/>s in <see cref="World"/>s.
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.Integration")]
    public static class KeyedComponentHelper
    {
        /// <summary>
        /// <see cref="World.KeyOwner">Gets</see> or <see cref="World.RequestKey">creates</see>
        /// a keyed <typeparamref name="T"/> component, based on the situation and parameters.
        /// </summary>
        /// <returns>The existing or newly created <typeparamref name="T"/> component.</returns>
        /// <exception cref="InvalidOperationException">
        /// When an incompatible component is associated with the given <paramref name="key"/>
        /// and <paramref name="replaceExisting"/> was <see langword="false"/>.
        /// </exception>
        /// <inheritdoc cref="TryGetKeyedComponentOrCreate{T}"/>
        public static T GetKeyedComponentOrCreate<T>(this Slot slot, string key, Action<T>? onCreated = null,
                int version = 0, bool replaceExisting = false, bool updateExisting = false)
            where T : Component, new()
        {
            if (!slot.TryGetKeyedComponentOrCreate(key, out var component, onCreated, version, replaceExisting, updateExisting))
                throw new InvalidOperationException("An incompatible component is stored under the key " + key + $"\nExisting: {component}");

            return component;
        }

        /// <summary>
        /// Tries to <see cref="World.KeyOwner">get</see> or <see cref="World.RequestKey">create</see>
        /// a keyed <typeparamref name="T"/> component, based on the situation and parameters.
        /// </summary>
        /// <remarks><para>
        /// This behavior was adapted from <see cref="ContainerWorker{C}.GetComponentOrAttach{T}(Predicate{T})"/>.
        /// </para><para>
        /// Consider that these component-associations will be saved with the <see cref="World"/>.
        /// Make sure that this is really necessary for what you're attempting to do.
        /// </para></remarks>
        /// <typeparam name="T">The type of the component to get or create.</typeparam>
        /// <param name="slot">The slot to attach the <typeparamref name="T"/> component to when necessary.</param>
        /// <param name="key">The unique key (to) associate(d) with the <typeparamref name="T"/> component.</param>
        /// <param name="component">
        /// The <typeparamref name="T"/> component associated with the given <paramref name="key"/> if this call returns <see langword="true"/>;
        /// otherwise, <see langword="null"/> when an incompatible component is associated with it
        /// and <paramref name="replaceExisting"/> was <see langword="false"/>.
        /// </param>
        /// <param name="onCreated">
        /// The optional configuration action to call when the <typeparamref name="T"/> component had to be created,
        /// or when its saved version is lower than the given <paramref name="version"/> number.
        /// </param>
        /// <param name="version">The version number to associate with the <typeparamref name="T"/> component after this call.</param>
        /// <param name="replaceExisting">Whether to replace a different component associated with the given <paramref name="key"/> with this one.</param>
        /// <param name="updateExisting">
        /// Whether to call <paramref name="onCreated"/> even if the <typeparamref name="T"/> component already exists.<br/>
        /// If the given <paramref name="version"/> number is greater than zero and the saved version is lower, this will always be done.
        /// </param>
        /// <returns><see langword="true"/> if the keyed <typeparamref name="T"/> component was found or created; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetKeyedComponentOrCreate<T>(this Slot slot, string key, [NotNullWhen(true)] out T? component,
                Action<T>? onCreated = null, int version = 0, bool replaceExisting = false, bool updateExisting = false)
            where T : Component, new()
        {
            var keyedComponent = slot.World.KeyOwner(key);

            if (keyedComponent is not null)
            {
                if (keyedComponent is T typedComponent)
                {
                    if (version > 0 && slot.World.KeyVersion(key) < version)
                    {
                        updateExisting = true;
                        slot.World.RequestKey(typedComponent, key, version, false);
                    }

                    if (updateExisting)
                        onCreated?.Invoke(typedComponent);

                    component = typedComponent;
                    return true;
                }

                if (!replaceExisting)
                {
                    component = null;
                    return false;
                }
            }

            component = slot.AttachComponent<T>();

            slot.World.RequestKey(component, key, version, false);
            onCreated?.Invoke(component);

            return true;
        }
    }
}