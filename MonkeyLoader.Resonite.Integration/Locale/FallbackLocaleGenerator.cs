using Elements.Assets;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Locale
{
    public sealed class FallbackLocaleGenerator : ResoniteAsyncEventHandlerMonkey<FallbackLocaleGenerator, LocaleLoadingEvent>
    {
        private const string LocaleCode = "en";
        private static readonly HashSet<Mapper> _mappers = new();

        /// <inheritdoc/>
        public override int Priority => -4096;

        public static bool AddMapper(Func<Mod, string> mapKey, Func<Mod, string> mapMessage)
            => _mappers.Add(new(mapKey, mapMessage));

        public static bool RemoveMapper(Func<Mod, string> mapKey)
            => _mappers.Remove(new(mapKey, mapKey));

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
                foreach (var mapper in _mappers)
                {
                    var key = mapper.MapKey(mod);

                    if (!messages.ContainsKey(key))
                        messages.Add(key, new LocaleResource.Message(LocaleCode, mapper.MapMessage(mod)));
                }
            }

            return Task.CompletedTask;
        }

        private sealed class Mapper
        {
            public Func<Mod, string> MapKey { get; }

            public Func<Mod, string> MapMessage { get; }

            public Mapper(Func<Mod, string> mapKey, Func<Mod, string> mapMessage)
            {
                MapKey = mapKey;
                MapMessage = mapMessage;
            }

            public override bool Equals(object obj)
                => ReferenceEquals(this, obj)
                || (obj is Mapper otherMapper && MapKey(Mod) == otherMapper.MapKey(Mod));

            public override int GetHashCode() => MapKey(Mod).GetHashCode();
        }
    }
}