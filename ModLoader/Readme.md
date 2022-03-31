# Simple Mars Horizon Mod Loader #

This is an unofficial mod loader of the Mars Horizon (2020 game).

Alternatively, all my mods support BepInEx (v5), Unity Mod Loader, and Unity Doorstop (direct load) as alternatives.
Please refer to `Alternatives.md` for instructions.


# Installation #

1. Extract (Drag and Drop) `doorstop_config.ini`, `version.dll`, and the `Mods` folder into the game's root folder.
2. Launch the game, then quit the game.  You may Alt+F4 as soon as you see the logos.
3. Goto `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`, check that `ModLoader.log` has a last modified time that is the same as when you launched the game.

The last step is used to check whether the mod loader is actually installed correctly.
Since the mod loader does not modify the game, the log is the sure way to check that.


# Behaviour #

This mod loader has three parts.

## Unity Doorstop ##

This loader comes with `doorstop_config.ini` and `winhttp.dll`.
They are Unity Doorstop, and are pre-configured to run `Mods\MH_ModLoader.dll`.

## Mod Loader ##

`MH_ModLoader.dll` is the mod loader.
It will find all other `MH_*.dll` in same folder, up to a depth of 3 subfolders.
Each will be loaded into memory, and call the first `public static Main()` on a top-level public class.

This mod loader does not detect or avoid mods that are already loaded,
since only the mod itself can tell whether it has been initiated.

## Harmony ##

The mod loader comes with Harmony 2.
It is used by mods to temporary change the game, without changing game files.
The mod loader itself does not use it, but all my mods require it.

HarmonyX 2 can also be dropped in as a replacement.


# Compatibility #

The mod loader is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.

This mod loader does not change save games or game files.
Since it does not depends on game code, it is expected to be compatible with future game versions.


# Troubleshoot #

If the mod loader doesn't work, or if none of the mod works, there are a few things you can try.

Before you try it, though, make sure Window Explorer is showing file extensions.

## Check Mod Loader Is Loaded

Find `ModLoader.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod loader is not loaded.
Please check that:

1. `doorstop_config.ini` and `version.dll` both exists on the game's root folder.
2. `doorstop_config.ini` contains the line `targetAssembly=Mods/MH_ModLoader.dll`
3. The `Mods` folder exists under game root, and contains `MH_ModLoader.dll`.
4. Check that game is 64bit.  If it is not, replace version.dll with 32bit doorstop.

If everything looks ok, and the log still is not created with game launch,
the trouble is probably beyond the scope of this readme.

## Check Game Is Moddable

If the game does not contains `Mars Horizon_Data\Managed\Assembly-CSharp.dll`, it is not a .Net bulid and is not moddable.
At least not with DLLs that change code on the fly.

As of writing, the game seems to only supports Windows.
But just in case, Mac and Game Pass are generally non-moddable, while Linux are gerenally ok.

## Check Mods Location

Mods must be dlls starting with `MH_` and must be placed in the same folder as this mod loader,
by default the Mods folder under game root.  `MH` must be uppercase.

For example, the Skip Animations mod comes in form of `MH_SkipAnimations.dll`,
which should be placed in Mods as `Mods\MH_SkipAnimations.dll`.

Some mods may require additional resources, please refer to each mod's instructions.

## Check Mod Loader Log ##

The mod loader log (above) contains information on the activity of the loader.

You can read the log to see whether a mod is detected, and whether it is initiated without error.
If a mod is not detected, please make sure it is in the right name and place.

Initilisation errors, if any, can only be fixed by that mod.


## Alternative Mod Loader ##

This mod loader is dead simple.
Unless/Until Unity Mod Manager supports this game,
or unless Mars Horizon adds official mod support, all alternatives are more complicated.


# Uninstall #

Removing this mod loader will inactivate all mods, if there are no other mod loaders.

Delete `doorstop_config.ini`, `version.dll`, and `Mods\MH_ModLoader.dll` from game root.

If you are not using other mods, you may also remove the mod loader.

The mod loader does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain.

The src folder contains complete source code and license.
