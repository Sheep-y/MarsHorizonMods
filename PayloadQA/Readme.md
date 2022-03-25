# Payload QA #
## A mod of Mars Horizon ##

*Payload variation that matters.*

This is an unofficial mod of Mars Horizon (2020 game).
It adjusts auto-resolve success/crit rate by payload variation,
and manual action crit rate by payload reliability.


# Installation #

1. Setup Mars Horizon Mod Loader.
2. Download latest mod release from GitHub or NexusMods.  Open with 7-zip.
3. Extract (Drag and Drop) `MH_SkipAnimations.dll` into the game's `Mods` folder.
4. Launch the game and enjoy.


# Default Behaviour #

This mod provides the following features by default:

## Mission Auto-Resolve ##

* Specialised payloads (Nav, Comm etc.) now enjoys improved auto-resolve success rate, but still lower than a normal payload.
* Power payload retain the same success rate, but have improved critical success chance.
* Auto-Resolve now rolls from a standalone RNG.

## Mission Mini-Game ##

* The base critical success rate is now proportional to payload reliability.


# Configuration #

On first launch, the mod will create `PayloadQA.ini` and `PayloadQA.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to disable or adjust various features.
Each setting have a short description that explains its effects.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages are supported.

This mod does not change save games or game files.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `PayloadQA.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod is not loaded.
Please follow mod loader's troubleshoot section.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log and/or game log (`Player.log`).

Errors normally have "Error" or "Exception" in the messag.
If you do find errors, you usually need a programmer.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.  If it is not configured right, such as typos, the mod may not work as expected.

If you delete `PayloadQA.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default values.


# Uninstall #

To remove the mod, rename or delete `MH_PayloadQA.dll` from the game's `Mods` folder.

If you are not using other mods, you may also remove the mod loader.
Please refer to the mod loader's instructions.

The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  The src folder contains complete source code and license.