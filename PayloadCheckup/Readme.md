# Payload Checkup #

This is an unofficial mod of Mars Horizon (2020 game).
It adjusts auto-resolve success rate by payload variation,
and adjusts mini-game action outstanding rate by payload reliability.


# Installation #

A mod loader is required.  The step depends on the loader:

* **BepInEx** : Extract package into a new subfolder under `BepInEx\plugins`.
* **Simple Mars Horizon Mod Loader** : Extract package into a new subfolder under `Mods`.
* **Unity Mod Manager** : Same as above, or install through the GUI.

To uninstall this mod, delete the extracted files.


# Default Behaviour #

This mod provides the following features by default:

## Mission Auto-Resolve ##

* Specialised payloads (Nav, Comm, and Observe) now have improved auto-resolve success chance.
* Extra crew beyond the min. requirement now improves auto-resolve success chance.
* Power payload now improves auto-resolve outstanding chance.
* Make auto-resolve rolls from a standalone RNG.

## Mission Mini-Game ##

* The base outstanding rate is now proportional to payload reliability.


# Configuration #

The mod config file on BepInEx is `BepInEx\configs\Zy.MarsHorizon.PayloadCheckup.cfg`.
On other mod loaders it is `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon\PayloadCheckup.ini`.

You can edit the config file to disable or adjust some features.
Except when using BepInEx's configuration manager, the changes will take effect on game's next launch.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages are supported.

This mod does not change save games or game files.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

If using BepInEx, read `LogOutput.log` to check that this mod is loaded.

Otherwise, find `ClickReduction.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.
If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, or the mod is not loaded by BepInEx,
you need to fix that first.  Please follow the mod loader's troubleshoot instructions.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log and/or game log (`Player.log`).

Errors normally have "Error" or "Exception" in the message.
If you do find errors, you usually need a programmer.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.  If it is not configured right, the mod may not work as expected.

If you delete the config file, the mod will recreate it with default values.


# License #

GPL v3.  The src folder contains source code and license.

https://github.com/Sheep-y/MarsHorizonMods/