# Main Differences to RML

Many seasoned Resonite mod developers are probably more familiar with
[ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) (RML), rather than MonkeyLoader.
While their purpose is the same, there are some important differences between the two,
which will be discussed on this page.


## Mod Packaging

For RML, there's a one-to-one relation between loaded DLLs and mods,
such that each DLL must only contain one `ResoniteMod` derived class.
For MonkeyLoader, this is different:
Each mod correlates to one loaded NuGet packages (.nupkg) by default,
which are actually zip files and as such can contain multiple DLLs,
resources such as textures and localizations, as well as metadata.

A mod thus can consist of multiple DLLs, which can each have any number of Monkeys inside of them.
In terms of compartmentalization, a Monkey is roughly equivalent to a ResoniteMod -
patching errors in one won't affect the others unless they specifically depend on each other.

For the end user, this way of packaging mods can simplify things,
as even mods with extra files can be shipped in a single file
with no extra work, rather than having to unzip something manually.


## Automatic Patching Order

By utilizing the metadata provided in NuGet packages,
MonkeyLoader can automatically determine the order in which mods
and their monkeys have to be loaded.
Monkeys will be loaded in the topological (dependency) order of the mods they belong to,
with those with larger impacts on features coming before those with smaller impacts.


## Configuration

Together with the more compartmentalized approach to patching,
MonkeyLoader's approach to configuration is more split up as well.
As described on the [Configuration](configuration.md) page,
each Monkey can define its own section in the mod's config file,
which is treated separately from the others.

Further, the configuration values don't have to be acquired by
passing the ConfigKey to the Config class - rather, the values
can be get and set on each key directly, with only access by other
mods having to go through the Config class to get the defining key
or directly get/set it.
To this end, a separate non-defining <xref:MonkeyLoader.Configuration.ConfigKey`1>
class has been introduced, which makes it clear, that the purpose of it is to access other mod's defining keys.


## Pre-Patching

MonkeyLoader uses [Doorstop](https://github.com/NeighTools/UnityDoorstop)
to get loaded before even Unity does, rather than relying on Resonite's
plugin system to be loaded.
As such, it doesn't require a command line argument to be loaded,
and also doesn't need to hack around the game's compatibility hashing
to allow multiplayer compatibility.

The main benefit of this however, is that MonkeyLoader supports pre-patching.
This means, the IL code of Assemblies can be modified using [Mono.Cecil](https://github.com/jbevain/cecil/)
before they're even loaded into memory.
Due to their nature, they're incompatible with being hot-reloaded.

Pre-patching makes it possible, to do the things that Harmony can't do.
Most importantly, it allows adding members to types and
allows patching generic types and methods without faffing around with
the intricacies of the runtime - or it just failing outright.

Supporting this was an additional consideration for the multi-DLL support using NuGet packages.
Because, to not trigger the automatical loading of dependencies,
pre-patchers must not reference game assemblies in any way.
Doing so is however practically a necessity for regular Harmony patching,
which necessitates multiple DLLs per mod,
if pre-patchers and patchers are to be associated into one mod.


## Debugging Support

By using Doorstop, MonkeyLoader also gains the ability to fully support debugging.
Simply set `debug_enabled=true` in the `doorstop.ini`,
connect Visual Studio (or your IDE of choice) by using "Attach Unity Debugger",
and enjoy easy debugging of all your methods.
Breakpoints, step into, step over, and until jump back all work as expected,
allowing to see live values too.

For the best experience, make sure to build in Debug mode,
otherwise local variables and especially sequence points
will be optimized out by the compiler.
Further, be aware that hot-reloading and debugging don't work well together -
especially since IDEs usually don't allow building while debugging.


## Hot-Reloading

MonkeyLoader supports hot-reloading out of the box,
with no extra libraries or effort necessary if a patcher
doesn't go out of its way to break the conventions for Monkeys.

RML requires an [extra library](https://github.com/Nytra/ResoniteHotReloadLib)
for this, adding extra development effort to use this
almost exclusively development focused feature.

Hot-reloading is not supported for pre-patchers by their nature.


## Feature and Update Mentality

For MonkeyLoader and the Resonite Game Pack, the approach to including features is different than that of other mod loaders.
If certain code is useful to many mods, or encourages good practice for
elements added by mods, it is fine to include it in the base installation.
The same thing applies to features that every user would want,
such as editing the loaded mods' configuration ingame - which is included in this Game Pack.

To give some examples for features on the development side:
* Hot-Reloading
* Debugging Support
* Event System
  * Locale Loading
  * Injecting custom Worker Inspector elements
* Shared Configs in Sessions

Of course, backwards compatibility is still a concern,
however MonkeyLoader has the benefit of being designed from scratch with
the experience from RML, allowing for a (hopefully) more extensible approach.

Further, it is planned to essentialy add auto-updating for mods,
based on searching customizable NuGet feeds for updates.
This would make even updates from breaking changes rather painless.

To serve this purpose, all of MonkeyLoader and its Game Packs
already makes heavy use of NuGet to manage all dependencies.
This extends to the game dependencies, referring to them as NuGet packages 
from a local feed to remove the reliance on Resonite's install location.
While the reliance is unavoidable for the automatic copying process of build artifacts,
this process can also be carried out manually when necessary
and should not affect the build itself.
