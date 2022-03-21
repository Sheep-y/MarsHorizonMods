# ﻿Deep Space Slots #
## A mod of Mars Horizon ##

*Secondary mission teams when you need them.*

This is an unofficial mechanic mod of the space agency management game Mars Horizon (2020).
It unlocks additional deep space slots for long duration missions, freeing up slots for more missions to help maintain game pace.

All old saves are compatible.
New saves made with this mod will still work when mod is disabled.


# Installation #

1. Setup Mars Horizon Mod Loader.
2. Download latest mod release from GitHub or NexusMods.  Open with 7-zip or similiar tools.
3. Extract (Drag and Drop) `MH_DeepSpaceSlots.dll` into the game's `Mods` folder.
4. Launch the game and enjoy.


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


# Configuration #

On first launch, the mod will create `DeepSpaceSlots.ini` and `DeepSpaceSlots.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to adjust the number of slots and requirement of missions.
You can also key slots to other buildings, missions, and/or researches, if you know their in-game id.
Each setting have a short description that explains its effects.

If you can't see file extensions, the one with a little gear in its icon is the config file.
You may want to google how to reveal file extensions for good.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be otherwise compatible with Steam and Epic (Windows only).
All game languages are supported.

This mod does not change save games or game files.
If a save is made with mission(s) on the deep space slots, and is later loaded without the mod,
all missions will still be up and running. But you won't be able to launch new mission until the number of missions drops back to vanilla level.
The same also happens if the mod config is changed leading to less missions to be transferred.

This mod is compatible with my other mods: Deep Space Slots, Informed, Mission Control, Payload QA, Skip Animation, and Zhant.



# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `DeepSpaceSlots.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod is not loaded.
Please follow mod loader's troubleshoot section.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log and/or game log (`Player.log`).

Errors normally have "Error" or "Exception" in the message, and are usually logged differently from normal messages.
If you do find errors, chances are it will need to be fixed by a programmer modder.  Mod is open source.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.  If it is not configured right, such as typos, the mod may not work as expected.

If you delete the config file `DeepSpaceSlots.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default values.


# Uninstall #

To remove the mod, rename or delete `MH_DeepSpaceSlots.dll` from the game's `Mods` folder.

If you are not using other mods, you may also remove the mod loader.
See the mod loader's instruction for details.

The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain.

The src folder contains source code and licenses for legality.