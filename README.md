# MonkeyLoader Resonite Game Pack

<img align="right" width="128" height="128" src="./Icon.png"/>

This Game Pack for [MonkeyLoader](https://github.com/MonkeyModdingTroop/MonkeyLoader)
provides basic hooks for modding the game [Resonite](https://resonite.com/) by [Yellow Dog Man Studios](https://yellowdogman.com/).
It provides additional hooks for the beginning of initialization, when initialization is done,
and when the game shuts down.


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
