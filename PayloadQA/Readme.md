# Payload QA #
## A mod of Mars Horizon ##

*Payload variation that matters.*

This is an unofficial mod of Mars Horizon (2020 game).
It adjusts auto-resolve success/crit rate by payload variation,
and manual action crit rate by payload reliability.


# Installation #

A mod loader is required.
Supports BepInEx v5, Simple Mars Horizon Mod Loader, Unity Mod Manager, and Unity Doorstop.

* **BepInEx** : Extract package to `BepInEx\plugins` folder.
* **Simple Mars Horizon Mod Loader** : Extract package to `Mods` folder.
* **Unity Mod Manager** : Drag and drop package or use the Install button.
* **Unity Doorstop** : Edit `doorstop_config.ini`, set `targetAssembly` to path of mod dll.

Folder/File locations are relative to game root.
To unintall this mod, simply delete the files.


# Default Behaviour #

This mod provides the following features by default:

## Mission Auto-Resolve ##

* Specialised payloads (Nav, Comm, and Observe ) now enjoys improved auto-resolve success chance.
* Extra crew beyond the min. requirement now improves auto-resolve success chance.
* Power payload now improves auto-resolve outstanding chance.
* Make auto-resolve rolls from a standalone RNG.

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


# License #

GPL v3.  The src folder contains complete source code and license.