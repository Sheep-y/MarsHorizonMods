# ﻿Deep Space Slots #
## A mod of Mars Horizon ##

*Secondary mission teams when you need them.*

This is an unofficial mod of Mars Horizon (2020 game).
It unlocks additional deep space slots for long duration missions, freeing up slots for more missions to help maintain game pace.

All old saves are compatible.
New saves made with this mod will still work when mod is disabled.


# Installation #

A mod loader is required.
Supports BepInEx v5, Simple Mars Horizon Mod Loader, Unity Mod Manager, and Unity Doorstop.

* **BepInEx** : Extract package to `BepInEx\plugins` folder.
* **Simple Mars Horizon Mod Loader** : Extract package to `Mods` folder.
* **Unity Mod Manager** : Drag and drop package or use the Install button.
* **Unity Doorstop** : Edit `doorstop_config.ini`, set `targetAssembly` to path of mod dll.

Folder/File locations are relative to game root.
To unintall this mod, simply delete the files.


# Default Mechanic #

By default, up to 3 deep space slots can be unlocked by these actions:

1. Launch The Grand Tour milestone mission and reach at least phase 2.
2. Construct the Space Science Library building. (Era 3 reward research)
3. Construct the Deep Space Network building.

Once unlocked, missions that meet the following requirements will be automatically
moved to the deep space network slots, below modular space station (if any).

1. Take at least 30 months to complete, and has at least 3 phases.
2. At least 6 months and 2 phases have passed since launch.

This means, new deep space missions will not be transferred immediately.
A mission with a long phases 2 can occupy a normal slot for years.

The conditions are configurable (see below).

The slot unlocked by The Grand Tour is permanent and can be used by other deep space missions, even if the tour later fails.
The slots unlocked by buildings will be gone if the buildings are cleared.

On a 1920x1080 screen, a UI scale of 86% or less is required to fit all three slots on the screen.


# Configuration #

On first launch, the mod will create `DeepSpaceSlots.ini` and `DeepSpaceSlots.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to adjust the number of slots and requirement of missions.
You can also key slots to other buildings, missions, and/or researches, if you know their in-game id.
Each setting have a short description that explains its effects.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages supported.

This mod does not change saves or game files.
If a game is saved with mission(s) in deep space slot(s), and is later loaded without the mod,
all missions will still be up and running, but you cannot launch new mission until the number
of missions drops to vanilla level.  The same may also happen if mod config is changed mid-game.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `DeepSpaceSlots.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod is not loaded.
Please follow mod loader's troubleshoot section.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log and/or game log (`Player.log`).

Errors normally have "Error" or "Exception" in the message.
If you do find errors, you usually need a programmer.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.  If it is not configured right, such as typos, the mod may not work as expected.

If you delete the config file `DeepSpaceSlots.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default values.


# License #

GPL v3.  The src folder contains source code and license.