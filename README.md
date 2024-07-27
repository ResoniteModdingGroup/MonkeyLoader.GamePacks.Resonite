# MonkeyLoader Resonite GamePack

<img align="right" width="128" height="128" src="./Icon.png"/>

This Game Pack for [MonkeyLoader](https://github.com/MonkeyModdingTroop/MonkeyLoader)
provides basic hooks for modding the game [Resonite](https://resonite.com/) by [Yellow Dog Man Studios](https://yellowdogman.com/).
It provides additional hooks for the beginning of initialization, when initialization is done,
and when the game shuts down.

## Installation

1. Download `MonkeyLoader-v...+Resonite-v....zip` from the [latest Resonite GamePack release](https://github.com/ResoniteModdingGroup/MonkeyLoader.GamePacks.Resonite/releases/latest)
2. Extract the zip into Resonite's install folder (`C:\Program Files (x86)\Steam\steamapps\common\Resonite`)
3. Remove RML's `-LoadAssembly "..."` launch arguments from Steam if you had set it up previously

### Linux

- Native: Change the steam launch options to `./run_monkeyloader.sh %command%`
- Wine / Proton: Using winetricks / protontricks, add `winhttp` to the native libraries


## Feature Overview

On the user side, this Game Pack makes the initial loading process of Resonite
more detailed, showing the different phases - especially each Monkey being executed.
Additionally, it integrates the MonkeyLoader config system with
Resonite's overhauled Settings UI to allow ingame configuration changes.
Further, it provides easy ways for mods to integrate localisation into them,
making it easier than ever to have mods that support multiple languages.

On the developer side, there's a lot of features as well,
which are covered in detail in the [online documentation](https://resonitemoddinggroup.github.io/MonkeyLoader.GamePacks.Resonite).
In addition to the extra hooks, a variety of additions are provided
to make the development of MonkeyLoader mods for Resonite a smooth experience.

* Game Feature Definitions
* Enum Json Converter
* System for sharing configuration items in a session
* Locale loading event system
    * Automatic locale loading from mod files
    * Fallback locale generation event to programmatically generate messages
* Worker Inspector build events
    * Add custom elements to the header
    * Add custom elements to the body
* Mod-provided UI and item helpers
    * Setup a local action for buttons that is triggerable by anyone
    * Nine Slice defined by two float4
    * Shorthand to destroy a slot when the local user leaves
* Various new Monkeys setup for Resonite...
    * (Configured)ResoniteMonkey
* ... and the event system
    * (Configured)ResoniteEventHandlerMonkey
    * (Configured)ResoniteAsyncEventHandlerMonkey
    * ResoniteInspectorMonkey for custom worker inspector elements

Additional built-in features for users:

* OpenLinkedDynamicVariableSpace
    * Extra buttons on the headers of dynamic variable components to open the linked dynamic variable space
* SyncArrayEditor
    * Arrays in inspectors will be editable using a proxy list
* ModSettingStandaloneFacet
    * Individual mod settings can be pulled out of the dash settings as standalone facets and placed anywhere in userspace. They will keep working even after restarts.


## Contributing

Issues can and should be opened here instead of the mods' issue trackers if they're designed for RML, and work with it, but not with this gamepack.
The GitHub issues can also be used for feature requests.

For code contributions, getting started is a bit involved due to [Resonite-Issues#456](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/456).
The short summary of it is:

1. [Setup a private nuget feed](https://github.com/MonkeyModdingTroop/ReferencePackageGenerator).
2. [Generate the game's reference assemblies](https://github.com/MonkeyModdingTroop/ReferencePackageGenerator).
3. Add the nuget feeds (`nuget sources Add -Name ... -Source ...`, local and either <https://pkg.munally.com/MonkeyModdingTroop/index.json> & <https://pkg.munally.com/ResoniteModdingGroup/index.json>)
4. Run `dotnet build`, or build with your IDE of preference.

The long version is that you'll probably want to set it up privately on GitHub NuGet packages.
Though this isn't legal advice and you should check that [Resonite's TOS](https://resonite.com/policies/TermsOfService.html) allows it.
The feeds can also be directly used from GitHub, though that requires authentication using a PAT.
