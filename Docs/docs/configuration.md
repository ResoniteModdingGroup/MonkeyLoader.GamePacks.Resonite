# Configuration

As with the Monkeys, the configuration is also compartmentalized for different patchers.
To create the config for a Monkey, you must create a type that derives from <xref:MonkeyLoader.Configuration.ConfigSection>.

```csharp
namespace YourMod
{
    internal sealed class YourConfig : ConfigSection
    {
        private readonly DefiningConfigKey<int> _intKey = new("Int", "A random number.", () => 4);
        private readonly DefiningConfigKey<string> _nameKey = new("Name", "Your name!", () => 100_000);
        private readonly DefiningConfigKey<HashSet<string>> _friendsKey = new("Friends", "The internal collection of your friends.", () => new(), internalAccessOnly: true);

        public override Version Version { get; } = new(1, 0, 0);
        public override string Description => "Contains options for my mod.";
        
        public int Int => _intKey.GetValue();
        
        public string Name => _nameKey.GetValue()!;
        
        public HashSet<string> Friends => _friendsKey.GetValue()!;
    }
}
```

You must override the <xref:MonkeyLoader.Configuration.ConfigSection.Description>
and <xref:MonkeyLoader.Configuration.ConfigSection.Version> properties.
The Version should ideally follow the rules of semantic versioning,
while the Description is the default text which will be displayed
to help users of your mod understand, what can be configured here.

To actually allow configuration, you have to create [DefiningConfigKeys](xref:MonkeyLoader.Configuration.DefiningConfigKey\`1).
These will typically be the ones provided by MonkeyLoader or the Game Packs,
but they can be anything that implements the interface
<xref:MonkeyLoader.Configuration.IDefiningConfigKey`1>.
They can be instance or static fields, and will automatically be collected for loading and saving,
unless decorated with the <xref:MonkeyLoader.Configuration.IgnoreConfigKeyAttribute>.

Within the definition of the config keys, you must provide a name unique to your mod.
Additionally, you can provide a default description and default value,
which are both majorly recommended. Once accessed, the initialized default value will stick around.
Further, you can flag whether only your mod should access the key, and even add a validation function for new values.

Proxying the access to the ConfigKeys through properties is not necessary -
but makes it clear which operations are available and allowed for other mods.

The configuration can be manually loaded by using one of the Mod.Config <xref:MonkeyLoader.Configuration.Config.LoadSection``1>
methods, but typically you should use the `Configured` version of a Monkey.

```csharp
namespace YourMod
{
    [HarmonyPatchCategory(nameof(YourPatcher))]
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.OnAttach))]
    internal sealed class YourPatcher : ConfiguredResoniteMonkey<YourPatcher, YourConfig>
    {
        protected override IEnumerable<IFeaturePatch> GetFeaturePatches()
        {
            yield return new FeaturePatch<ProtofluxTool>(PatchCompatibility.HookOnly);
        }

        private static void Postfix()
        {
            Logger.Info(() => $"My name is {ConfigSection.Name} and I have {ConfigSection.Friends.Count} friends!");
        }
    }
}
```

When you do that, your config section will automatically be loaded when the Monkey
is first [loaded](xref:MonkeyLoader.Resonite.ResoniteMonkey\`1.OnLoaded) - as long
as you don't override the OnLoaded method without a `base.OnLoaded()` call at the start.

The loaded config section is then available through the
<xref:MonkeyLoader.Resonite.ConfiguredResoniteMonkey`2.ConfigSection> property.

The whole Mod config is saved when the game shuts down,
but you can manually save it by calling the `Mod.Config.Save()` method.
Default values are always persisted if they have been accessed, so that they won't change between versions
and users can find and edit them in the .json file storing the configuration.