# Localization

The Resonite Integration for MonkeyLoader fully supports localized mods.  
This means, that mods can adapt any content they add to the User's selected locale.

To avoid collisions of locale keys, mods should prefix their keys with their Id.
To this end, there is extension methods on the <xref:MonkeyLoader.Meta.Mod> class and others,
which allow the easy creation of these keys as plain `strings` or directly as `LocaleString`s.

Together with the UIBuilder extension methods for mod-backed buttons,
creating a fancy localized button is as simple as writing the following:

```csharp
var ui = new UIBuilder(root);
RadiantUI_Constants.SetupEditorStyle(builder);

ui.LocalActionButton(Mod.GetLocaleString("MyButton"), () => { /* Do something */ });
```

If your mod's Id was `MyMod`, this would use `MyMod.MyButton` as the locale key for the `LocaleString` passed to the button.

The Integration currently adds three ways in which mods can provide the necessary locale data.  
These will be explained in the later sections, but first a primer on how Resonite loads locales.


## What Resonite Does

When the User changes their locale setting, the `LocaleResource` asset's data is reloaded.
To do this, a maximum of three locales are checked: the specific selected locale,
that specific locale's main language, and finally `en` as the final fallback.

```csharp
var localeCodes = new List<string>();
localeCodes.AddUnique(variant.LocaleCode);
localeCodes.AddUnique(LocaleResource.GetMainLanguage(variant.LocaleCode));
localeCodes.AddUnique("en");
```

For each unique locale, Resonite's `/Locale/` folder is checked for a file with the name `[locale].json`.
They consist of their `localeCode`, an array of `authors` and finally a dictionary mapping locale keys to their messages.
To give an example, the file `en-gb.json` looks like this:

```json
{
    "localeCode": "en-gb",
    "authors": [ "Nammi", "Enverex", "atomicwave", "CyberZott" ],
    "messages": {
        "Undo.ChangeColor": "Change Colour",
        ...
    }
}
```

Each locale file is loaded and its contents are additively added to the loaded `LocaleData`.
This means, that any message keys already loaded from an earlier file won't be overridden by later ones.
As such, the `en-gb.json` file would only need to define the keys that are different to the `en.json` file.
Similarly, a partial translation into another language would automatically be filled in by the English messages.


## What the Integration Adds

The locale loading process described above is postfixed by the integration pack and extended.
As such, vanilla messages could be modified, but should generally only be added to.  
Additionally, the integration enables the use of custom locales that aren't known by the runtime,
and ensures that the locale author listing appears neat.

The integration offers the following options, in order of their execution:


### Automatic Locale File Loading

Any content files of a mod that match the pattern `Locale/[locale].json` are
considered for loading if they fit the requested locale.
Their messages will be additively added to the already loaded locale data just like vanilla files.

To this extent, the template mod already contains sample locale files.
If you're adding your own, the easiest way is to add a `Locale` folder
to your project and include the following in your `.csproj` file:

```xml
<ItemGroup>
  <None Include="Locale\*.json" Pack="true" PackagePath="content/Locale/" />
</ItemGroup>
```


### Locale Loading Event

The <xref:MonkeyLoader.Resonite.Locale.LocaleLoadingEvent> uses MonkeyLoader's event source/handler system
to let the different handlers add locale data as needed.
All the other options listed here are extensions of this process.

To use it, you must register an <xref:MonkeyLoader.Events.IAsyncEventHandler`1>.  
This is explained on the [Event System](event-system.md) page.

The event data contains everything necessary to load whatever locale content.  
As an example, this is what the automatic file system loader's structure looks like:

```csharp
internal sealed class FileSystemLocaleLoader : ResoniteAsyncEventHandlerMonkey<FileSystemLocaleLoader, LocaleLoadingEvent>
{
    /// <inheritdoc/>
    public override int Priority => HarmonyLib.Priority.First;

    protected override bool AppliesTo(LocaleLoadingEvent eventData) => true;

    protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

    /// <summary>
    /// Handles the given locale loading event by checking every loaded mod's
    /// <see cref="Mod.FileSystem">FileSystem</see> for matching <c>Locale/[localeCode].json</c> files.
    /// </summary>
    /// <inheritdoc/>
    protected override async Task Handle(LocaleLoadingEvent eventData)
    {
        // The actual code
    }
}
```


### Fallback Locale Generation Event

The <xref:MonkeyLoader.Resonite.Locale.FallbackLocaleGenerationEvent> uses MonkeyLoader's event source/handler system as well.
It's a second level to the Locale Loading Event above, which gets called only as the last option for the final locale `en`.

The intended use is, to allow mods that add locale keys based on other mods
to fill these with their programmatically set defaults, or based on other intrinsic data.  
This allows mods to define those keys themselves to be compatible with the added system,
while at the same time allowing the system itself to provide a useful fallback.

To use it, you must register an <xref:MonkeyLoader.Events.IAsyncEventHandler`1>.  
This is explained on the [Event System](event-system.md) page.

The prime example for this is the <xref:MonkeyLoader.Resonite.Configuration.SettingsDataFeedInjector>,
which uses this system to fill all the locale keys used by the MonkeyLoader Settings
with the values programmatically defined for the configuration keys.  
Mods can still provide their own localization for them, but these are the default.

```csharp
internal sealed class SettingsDataFeedInjector : ResoniteAsyncEventHandlerMonkey<SettingsDataFeedInjector, FallbackLocaleGenerationEvent>
{
    public override int Priority => HarmonyLib.Priority.Normal;

    protected override bool AppliesTo(FallbackLocaleGenerationEvent eventData) => true;

    protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

    protected override Task Handle(FallbackLocaleGenerationEvent eventData)
    {
        foreach (var configSection in Mod.Loader.Config.Sections)
        {
            eventData.AddMessage($"{configSection.FullId}.Name", configSection.Name);

            foreach (var configKey in configSection.Keys)
            {
                eventData.AddMessage(configKey.GetLocaleKey("Name"), configKey.Id);
                eventData.AddMessage(configKey.GetLocaleKey("Description"), configKey.Description ?? "No Description");
            }
        }
        
        // And more for every mod
            
        return Task.CompletedTask;
    }
}
```
