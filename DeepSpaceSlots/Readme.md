# ﻿Deep Space Slots #
## A mod of Mars Horizon ##

*Secondary mission teams when you need them.*

This is an unofficial mechanic mod of the space agency management game Mars Horizon (2020).
It unlocks additional deep space slots for long duration missions, freeing up slots for more missions to help maintain game pace.

All old saves are compatible.
New saves made with this mod will still work when mod is disabled.
Support all game languages.


# Default Mechanic #

Up to 3 deep space slots can be unlocked by these actions:

1. Launch The Grand Tour milestone mission and reach at least phase 2.
2. Construct the Space Science Library building. (Era 3 reward research)
3. Construct the Deep Space Network building.

Once unlocked, missions that meet these requirements will be automatically moved to the deep space network slots, below modular space station (if any).

1. Take at least 30 months to complete, over 3 phases or more.
2. At least 6 months and 2 phases have passed since launch.

This means, new deep space missions will not be transferred immediately (as your operators run tests, calculations, calibrations, softwared patches.)
For some missions with a long phases 2, it can take years for the mission to transfer.

The conditions are configurable (see below).

The slot granted by The Grand Tour is permanent and can be used by other deep space missions, even if the tour fails at a later stage.
The slot granted by buildings will be gone if the buildings are removed.

A UI scale of 86% or less is required to fit all three slots on the screen without overlapps.


# Installation #

1. Download latest release from GitHub or NexusMods.  Extract with 7-zip or similar tools.

2. Exit game if it is running.

3. Copy or move `version.dll`, `doorstop_config.ini`, and the `Mods` folder into game's root folder.

4. If you have other mods, you may need to overwrite some files.  If the other mods are from me (Sheepy), you can safely overwrite them.

5. That is all.  Launch the game and enjoy.

The mod has no special requirements.  It runs as part of the game.
The src folder contains source code and licenses.


# Configuration #

On first launch, the mod will create `DeepSpaceSlots.ini` and `DeepSpaceSlots.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to adjust the number of slots and requirement of missions.
You can also key slots to other buildings, missions, and/or researches, if you know their in-game id.
Each setting have a short description that explains its effects.

If you can't see file extensions, the one with a little gear in its icon is the config file.
You may want to google how to reveal file extensions for good.


# Game Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
It works only on the Microsoft .Net build of the game, i.e. on Windows or Wine,
but should be otherwise compatible with Steam and Epic.

This mod does not change save games or game files.
If a save is made with mission(s) on the deep space slots, and is later loaded without the mod,
all missions will still be up and running. But you won't be able to launch new mission until the number of missions drops back to vanilla level.


# Mod Compatibility #

This mod is compatible with my other mods: Informed, Mission Control, and Skip Animations.
They use the same modding tools, so you will need to overwrite some files.

If this mod is installed last, it may be loaded as the first mod (as directed by `doorstop_config.ini`).
In this case, it will load all `MH_*.dll` in same folder and call the first `public static Main()`.

When you rename or disable a mod, please make sure `doorstop_config.ini` still points to a valid mod.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `DeepSpaceSlots.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod is not loaded at all.

Please check that `version.dll` exists at game folder,
and that `doorstop_config.ini` in the same folder is pointing to the right dll and path.
When in doubt, reinstall this mod and overwrite the files.

Double-clicking `doorstop_config` should open the file in notepad, and the line `targetAssembly` is the first mod to be loaded.
Any other mods will be depending on it loading them.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log.
Errors in this mods are contained, so they may not break the game, but will be recorded in mod log.

Errors normally have "Error" or "Exception" in the message, and are usually logged differently from normal messages.
If you do find errors, chances are it will need to be fixed by a programmer modder.  Mod is open source.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.
So, if the mod is configured to not work,
like if there is a typo on a line causing it to be read as zero or negative,
well, the mod is doing its job as written.

If you delete the config file `DeepSpaceSlots.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default config, which is working at time of release.


# Uninstall #

To remove the mod, rename or delete `MH_DeepSpaceSlots.dll` from the `Mods` folder under game root.

If you are not using other mods, you may also remove `doorstop_config.ini` and `version.dll` from game root.

If you are using other mods, please make sure `doorstop_config.ini` points to an existing mod, or refer to that mod's instructions.
When in doubt, overwrite with the ini from that mod.

You may also want to remove the `src` folder, if exists, which is distributed with the mod for legality.
The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain; licenses in src folder.