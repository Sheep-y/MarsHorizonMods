# Simple Mars Horizon Mod Loader #

This is an unofficial mod loader of the Mars Horizon (2020 game).

Alternatively, you may also use BepInEx and Unity Mod Loader for my mods.
BepInEx is recommended for its easy setup and optional in-game mod config UI.
Please refer to `Alternatives.md` for instructions.


# Installation #

These steps are for Simple Mars Horizon Mod Loader, not BepInEx or other loaders. 
**Please use only one mod loader.**

1. Extract (Drag and Drop) `doorstop_config.ini`, `winhttp.dll`, and the `Mods` folder into the game's root folder.

2. Launch the game, then quit the game.  You may Alt+F4 as soon as you see the logos.

3. Goto `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`, check that `ModLoader.log` has a last modified time that is the same as when you launched the game.

If the log exists and time is correct, the mod loader is correctly installed.
Extract your mods into the `Mods` folder.


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
It is used by the loader and most mods to temporary change the game, without changing game files.

HarmonyX 2 can also be dropped in as a replacement.


# Compatibility #

The mod loader is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.


# Troubleshoot #

If the mod loader doesn't work, or if none of the mod works, there are a few things you can try.

Before you try it, though, make sure Window Explorer is showing file extensions.

## Check Mod Loader Is Loaded ##

Find `ModLoader.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod loader is not loaded.
Please check that:

1. `doorstop_config.ini` and `winhttp.dll` both exists on the game's root folder.
2. `doorstop_config.ini` contains the line `targetAssembly=Mods/MH_ModLoader.dll`
3. The `Mods` folder exists under game root, and contains `MH_ModLoader.dll` and `0Harmony.dll`.
4. Check that game is 64bit.  If it is not, replace winhttp.dll with 32bit doorstop dll.  Otherwise, make sure it is 64bit.
5. Delete all files in the game folder (important), Verify Files, and make sure the unmodded game runs ok.
Then reinstall this mod loader, and mods.

If you did it all, and the log is still not created on game launch,
the trouble is probably beyond the scope of this readme.

## Check Game Is Moddable ##

If the game does not contains `Mars Horizon_Data\Managed\Assembly-CSharp.dll`, it is not a .Net bulid and is not moddable.
At least not with this mod loader.

As of writing, the game seems to only supports Windows.
But just in case, Game Pass is generally non-moddable, Linux are gerenally ok,
while Mac is... complicated.  Get a PC.

## Check Mods Location ##

Mods must be dlls starting with `MH_` and must be placed in the same folder as this mod loader,
or in a subfolder up to 3 levels deep.  `MH_` must be uppercase.

Some mods may require additional resources, please refer to each mod's instructions.

## Check Mod Logs ##

Read the mod loader log (see above) to see what mods are detected, and whether they were
loaded without error.  If a mod is not detected, make sure it is in the right name and place.

Also check game log (Player.log) to see whether the game ran into any errors.

Initilisation errors, if any, can only be fixed by that mod.

## Restart PC ##

A cold boot never hurts, as does Verify Files.


# Uninstall #

Removing this mod loader will inactivate all mods.

Delete `doorstop_config.ini`, `winhttp.dll`, and `Mods\MH_ModLoader.dll` from game root.

The mod loader does not modify game files, so there is no need to Verify Files.


# To Mod Authors #

Historical reason aside, there are three main reasons for this mod loader to exists.

First is I need a place on NexusMods to explain how to setup different mod loaders anyway.

Second is to provide an option for modders who don't want to tie their mods to
a specific loader, and the third is to provide an option for those who want to use UMM.

See, if the game does gain official mod support in the future, there is a high chance
it won't integrate BepInEx or UMM.  If a mod depends on them, it will fail.

My mods do support multiple loaders, but they are not simple.
Not all modders will be comfortable with them.


# License #

GPL v3.  Bundled libraries are either MIT or public domain.

The src folder contains source code and license.

https://github.com/Sheep-y/MarsHorizonMods/
