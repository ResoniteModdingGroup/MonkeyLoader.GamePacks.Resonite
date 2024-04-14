using Elements.Assets;
using MonkeyLoader.Configuration;
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
    public sealed class FallbackLocaleGenerator : ResoniteAsyncEventHandlerMonkey<FallbackLocaleGenerator, LocaleLoadingEvent>,
        IAsyncEventSource<FallbackLocaleGenerationEvent>
    {
        private const string LocaleCode = "en";
        private static readonly HashSet<ConfigKeyMapper> _configKeyMappers = new();
        private static readonly HashSet<ConfigSectionMapper> _configSectionMappers = new();
        private static readonly HashSet<ModMapper> _modMappers = new();

        /// <inheritdoc/>
        public override int Priority => -4096;

        public static bool AddMapper(MapKey<Mod> mapKey, Func<Mod, string> mapMessage)
            => _modMappers.Add(new(mapKey, mapMessage));

        public static bool RemoveMapper(MapKey<Mod> mapKey)
            => _modMappers.Remove(new(mapKey, mapKey));

        /// <inheritdoc/>
        protected override bool AppliesTo(LocaleLoadingEvent eventData) => eventData.LocaleCode == LocaleCode;

        /// <inheritdoc/>
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        /// <inheritdoc/>
        protected override Task Handle(LocaleLoadingEvent eventData)
        {
            var messages = eventData.LocaleResource._formatMessages;

            foreach (var mod in Mod.Loader.Mods)
            {
                foreach (var mapper in _modMappers)
                {
                    var key = mapper.MapKey(mod);

                    if (!messages.ContainsKey(key))
                        messages.Add(key, new LocaleResource.Message(LocaleCode, mapper.MapMessage(mod)));
                }
            }

            return Task.CompletedTask;
        }

        public delegate string MapKey<T>(T input);

        public delegate string MapMessage<T>(T input, Dictionary<string, LocaleResource.Message> messages);

        private sealed class ConfigKeyMapper : Mapper<IDefiningConfigKey>
        {
            protected override IDefiningConfigKey TestInput => Mod.Loader.Config.Sections.First().Keys.First();

            public ConfigKeyMapper(Func<IDefiningConfigKey, string> mapKey, Func<IDefiningConfigKey, string> mapMessage)
                : base(mapKey, mapMessage) { }
        }

        private sealed class ConfigSectionMapper : Mapper<ConfigSection>
        {
            protected override ConfigSection TestInput => Mod.Loader.Locations;

            public ConfigSectionMapper(Func<ConfigSection, string> mapKey, Func<ConfigSection, string> mapMessage)
                : base(mapKey, mapMessage) { }
        }

        private abstract class Mapper<T>
        {
            public MapKey<T> MapKey { get; }
            public Func<T, string> MapMessage { get; }
            protected abstract T TestInput { get; }

            protected Mapper(MapKey<T> mapKey, Func<T, string> mapMessage)
            {
                MapKey = mapKey;
                MapMessage = mapMessage;
            }

            public override sealed bool Equals(object obj)
                => ReferenceEquals(this, obj)
                || (obj is Mapper<T> otherMapper && MapKey(TestInput) == otherMapper.MapKey(TestInput));

            public override sealed int GetHashCode() => MapKey(TestInput).GetHashCode();
        }

        private sealed class ModMapper : Mapper<Mod>
        {
            protected override Mod TestInput => Mod;

            public ModMapper(MapKey<Mod> mapKey, Func<Mod, string> mapMessage)
                : base(mapKey, mapMessage) { }
        }
    }
}