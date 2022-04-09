# Simple Mars Horizon Mod Loader #

This is an unofficial mod loader of the Mars Horizon (2020 game).

Alternatively, you may use BepInEx or Unity Mod Loader to load my mods.
BepInEx is recommended for its easy setup and optional in-game mod config UI.
Please refer to `Alternatives.md` for instructions of both.

Disclaimer: You mod the game at your own risk.


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

## Bootstrapper ##

This loader comes with `doorstop_config.ini` and `winhttp.dll`.
They are Unity Doorstop, responsible for loading `Mods\MH_ModLoader.dll` as the game runs.

## Mod Loader ##

`MH_ModLoader.dll` is the mod loader.
It will find all other `MH_*.dll` in same folder, up to a depth of 3 subfolders.
Each will be loaded into memory, and have the first `public static Main()` on a top-level public class be invoked.

This mod loader does not attempt to detect or avoid mods that are already loaded.

In addition, the loader will patch the game and Unity Mod Manager (if used) to make them more
tolerant to multi-loader mods.

## Harmony ##

The mod loader comes with Harmony 2.
It is used to temporary modify the game, without changing game files.

HarmonyX 2 can also be dropped in as a replacement.


# Compatibility #

The mod loader is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.


# Troubleshoot #

If the mod loader doesn't work, or if none of the mod works, there are a few things you can try.

Before you try it, though, make sure Window Explorer is showing file extensions.

## Game Fails to Launch ##

1. Moves the `Mods` folder to Recycle Bin.
2. If the game can launch now, one of the mods is causing the problem.  Try narrow it down.
3. If the game still does not launch, move `winhttp.dll` and `version.dll` (if any) to Recycle Bin.  Only one should exists, by the way.
4. If the game can launch now, that could mean the dll build (32/64bit) is incorrect.
Get the correct one from Unity Doorstop's release page.
5. If still not work, do a cold reboot first.  Then put the platform in offline mode and try again, in case it is a sync issue.
7. If the game still can't launch, Delete all files in the game folder (important), Verify Files, and make sure the unmodded game runs ok.

The last step is because most mod loaders will add things into the game,
which Verify Files will not remove for you.
This mod loader is proves that having extra files can affect a program.

## Check Mod Loader Is Loaded ##

Find `ModLoader.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod loader is not loaded.
Please check that:

1. `doorstop_config.ini` and `winhttp.dll` both exists on the game's root folder.
2. `doorstop_config.ini` contains the line `targetAssembly=Mods/MH_ModLoader.dll`
3. The `Mods` folder exists under game root, and contains `MH_ModLoader.dll` and `0Harmony.dll`.

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

If there are other mod logs, you may also check them for errors.

## Reset Mod Config ##

My mods place their config files (*.ini) in the same folder as mod loader log.

Deleting the ini will reset the config, which may correct configuration issues.


# Uninstall #

Removing this mod loader will inactivate all mods.

Delete `doorstop_config.ini`, `winhttp.dll`, and `Mods\MH_ModLoader.dll` from game root.

The mod loader does not modify game files, so there is no need to Verify Files.


# To Mod Authors #

One of the aim of this loader is that you can write a plain, simple mod
straight out of Harmony tutorial, not tied to any loaders, and have it loads.

Of course my mods aren't like that.  But I want to leave the options open.

Good luck modding!

# License #

GPL v3.  Bundled libraries are either MIT or public domain.

The src folder contains source code and license.

https://github.com/Sheep-y/MarsHorizonMods/
