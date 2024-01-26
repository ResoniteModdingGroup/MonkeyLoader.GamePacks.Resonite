# MonkeyLoader

*A convenience and extendability focused mod loader using NuGet packages.*

## What's this?

MonkeyLoader is a mod loader that aims to combine easy usability for developers and users,
while also offering a feature rich platform.
It started with me wanting to use the break in mod compatibility, caused by the release of Resonite,
to develop something with support for pre-patching (modifying assemblies before they're loaded),
without relying on the game's plugin system, and with a focus on improving dependency resolution.

To start with this, I asked other active developers in the general modding community,
what they would like to see as features in a new mod loader.
It became clear, that main requests were:
* Being able to use it as a library from a custom launcher
* Automatic load order and patch sorting
* Easy distribution of assets with mods, without messy files
* Dependency resolution capabilities

Using NuGet packages (.nupkg) files to distribute and load mods from came as
a natural conclusion from the last two points.
As fancy zip files, they make it easy to bundle assets (like debug symbols, 3D models, textures, ...) for the mod into one file,
without creating an enormous and opaque DLL file.
Further, they include metadata that can be read without having to load the DLLs,
which allows checking whether all dependencies are available and otherwise restoring them.
For mod distribution and dependency resolution, custom NuGet feeds can be used:
Mod authors can have their own feeds for mods, and community groups could set up feeds for "vetted" builds as well.
Other general libraries can be loaded from custom feeds or the general nuget.org one too.


## Features

Below you can find an overview of the (planned) features.
Help is always wanted and appreciated, feel free to ask here, or on Telegram or Discord.

* Fully usable as a library by custom launchers / wrappers
* Multiple, configurable loading locations for mods
* Add mods and their dependencies from customizable NuGet feeds (not yet)
* Pre-patching of assemblies - modify the IL that gets loaded
* Game specific integration through GamePacks
  * Extra hooks, JsonConverters, feature lists, etc.
* Configuration system with config files for each mod
* Load mods in topological order (respecting dependencies)
  * Otherwise smartly order mods' patchers based on which features they affect how much


## How it Works

This is the order of operations that MonkeyLoader executes on startup:

* Catalogue all assemblies of the game
* Load the metadata of all GamePacks and mods 
* Load and run the pre-patchers of GamePacks, then of mods
* Save all modified assemblies to disk and load all assemblies of the game
* Load and run the patchers of GamePacks, then of mods

After that, the game can proceed with its initialization and startup.
GamePacks may provide extra hooks for patchers that need some things to already be set up.
For example the Unity pack ads a hook for patching after the first scene has been loaded -
otherwise, the dynamic binding of Unity engine methods fails and everything breaks.


## Creating a Mod

Someone from the Resonite community has created a [Template](https://github.com/mpmxyz/ResoniteSampleMod),
which covers both the regular [RML](https://github.com/resonite-modding-group/ResoniteModLoader)
and MonkeyLoader.

Otherwise, the general steps are:

* Create a class library in Visual Studio, targeting the .net version of the game
* Reference MonkeyLoader from NuGet (eventually, for now from your install)
* Reference the GamePacks you need from NuGet (eventually, for now from your install)
* Make sure to split up pre-patcher and patcher assemblies
  * Pre-patchers _must not_ reference any game assemblies directly, or pre-patching won't work
* (Automatically) create a .nupkg with your assemblies and place it into the ./MonkeyLoader/Mods/ folder (by default)
* When you now start the game, your mod should be loaded and patches should be applied

For more detail, you can take a look into the Resonite.Integration for now.
I will update this readme with an actual MVP eventually and link to other examples.
