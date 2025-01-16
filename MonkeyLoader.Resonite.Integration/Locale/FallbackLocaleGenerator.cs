using MonkeyLoader.Events;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Locale
{
    /// <summary>
    /// Subscribes to the <see cref="LocaleLoadingEvent"/> and generates the <see cref="FallbackLocaleGenerationEvent"/>
    /// when it's loading the final fallback locale to inject programmatically generated keys.
    /// </summary>
    public sealed class FallbackLocaleGenerator : ResoniteAsyncEventHandlerMonkey<FallbackLocaleGenerator, LocaleLoadingEvent>,
        IAsyncEventSource<FallbackLocaleGenerationEvent>
    {
        /// <summary>
        /// The fallback locale code.
        /// </summary>
        public const string LocaleCode = "en";

        private static AsyncEventDispatching<FallbackLocaleGenerationEvent>? _generateFallbackMessages;

        /// <inheritdoc/>
        public override int Priority => -4096;

        /// <inheritdoc/>
        protected override bool AppliesTo(LocaleLoadingEvent eventData)
            => base.AppliesTo(eventData) && eventData.LocaleCode == LocaleCode;

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => [];

        /// <inheritdoc/>
        protected override async Task Handle(LocaleLoadingEvent eventData)
        {
            var generatorEventData = new FallbackLocaleGenerationEvent(eventData.LocaleResource._formatMessages);

            await (_generateFallbackMessages?.Invoke(generatorEventData) ?? Task.CompletedTask);
        }

        /// <inheritdoc/>
        protected override bool OnEngineReady()
        {
            Mod.RegisterEventSource(this);

            return base.OnEngineReady();
        }

        event AsyncEventDispatching<FallbackLocaleGenerationEvent>? IAsyncEventSource<FallbackLocaleGenerationEvent>.Dispatching
        {
            add => _generateFallbackMessages += value;
            remove => _generateFallbackMessages -= value;
        }
    }
}