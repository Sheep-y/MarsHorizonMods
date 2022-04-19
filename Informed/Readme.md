# Informed #

This is an unofficial mod of Mars Horizon (2020 game).
It provides convenience access to information that are otherwise difficult to get or compare,
such as payload options, launch windows, or total building synergies.


# Installation #

A mod loader is required.  The step depends on the loader:

* **BepInEx** : Extract package into a new subfolder under `BepInEx\plugins`.
* **Simple Mars Horizon Mod Loader** : Extract package into a new subfolder under `Mods`.
* **Unity Mod Manager** : Same as above, or install through the GUI.

To uninstall this mod, delete the extracted files.


# Default Behaviour #

This mod provides the following features by default:

## Solar System Screen ##

* Show info icon next to Mission when a mission slot is available.
* Show info icon next to Crew when new candidates are available.
* Show info icon next to Diplomacy when a joint mission can be proposed.
* Spacepedia will now never show an icon.

## Planet Screen ##

* A "Launch Window" button is added to the bottom of milestone/request mission list.
* The expiry countdown is displayed on the right when you select a mission.
* The potential payload(s) is displayed on the right when you select a mission.

For milestone missions, the mission itself must be researched to see the payload list.
The payload build time already factors in all applicable bonus.

## Vehicle Design Screen ##

* When you hover any option on the left, the launch windows around the completion date will be listed.
* Add bonus and penalty under each contractor option.

The launch window list does NOT take vehicle traits into account, just unmodified launch windows.

## Research Screen ##

* Booster mouseover now lists all possible supplements and their capacities.

## Base Screen ##

* Show total building synergies on base screen.

Only synergies count.  Building effects are not included on the list.

## Funding Screen ##

* Next Tier now shows the tier you *will* be at.


# Configuration #

The mod config file on BepInEx is `BepInEx\configs\Zy.MarsHorizon.Informed.cfg`.
On other mod loaders it is `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon\Informed.ini`.

You can edit the config file to disable or adjust some features.

BepInEx also has a configuration manager plugin which can reconfig the mod on the fly.
A restart may be required to apply some changes.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages are supported.

This mod does not change save games or game files.

Due to the number of screen it mods and the number of information it pulls,
each non-minor game update have a considerable chance of breaking something.
Good luck!


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

If using BepInEx, read `LogOutput.log` to check that this mod is loaded.

Otherwise, find `ClickReduction.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.
If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, or the mod is not loaded by BepInEx,
you need to fix that first.  Please make sure mod is placed in the correct folder.

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