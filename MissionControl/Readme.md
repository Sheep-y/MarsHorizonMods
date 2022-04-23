# ﻿Mission Control #

This is an unofficial mod of Mars Horizon (2020 game).
It adjust mission weightings, tweak milestone challenge requirement and rewards, and replace the RNG used to generate and auto resolve missions.

The default configuration gives you a greater freedom to do non-Earth planet milestones,
since they have a much reduced chance to induce request missions.


# Installation #

A mod loader is required.  The step depends on the loader:

* **BepInEx** : Extract package into a new subfolder under `BepInEx\plugins`.
* **Simple Mars Horizon Mod Loader** : Extract package into a new subfolder under `Mods`.
* **Unity Mod Manager** : Same as above, or install through the GUI.

To uninstall this mod, delete the extracted files.


# Default Mechanic #

This mod provides the following features by default.
They only apply to new missions and challenges, not to existing ones, and only when mod is enabled.

## Reweight New Mission Destinations ##

When a new mission is rolled:

* Earth and moon missions are heavily biased.
* Mars, Venus, and Mercury missions have a smaller chance to come up, in this order.
* Other destinations will not come up as request / joint missions.
* The mission is rolled from a standalone RNG.

The exact chance depends on what milestones you have completed, since the game only draws from them.
Some events may be keyed to specific request/joint missions, which will be harder to encounter under this mod.

## Reweight New Mission Types ##

When a new mission is rolled:

* Lucrative missions has a higher chance to come up.
* The chance is further increased when there are no lucrative missions around.
* The chance is further increased when you've finished all reasearches.
* The type is rolled from a standalone RNG.

The exact chance boost is complicated and varies from mission to mission.

## More Relevant New Challenge ##

When a new milestone challenge is rolled:

* Reward increased for fund type rewards.
* Tech tree with 3 or less researches remaining will be excluded from reward pool.
* New challenge will have a different reward from the old one, if still possible after tech tree exclusion.

# Configuration #

The mod config file on BepInEx is `BepInEx\configs\Zy.MarsHorizon.MissionControl.cfg`.
On other mod loaders it is `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon\MissionControl.ini`.

You can edit the config file to disable some features or adjust the numbers.

BepInEx also has a configuration manager plugin which can reconfig the mod on the fly.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages are supported.

This mod does not change save games or game files.
If a save is made with mission(s) on the deep space slots, and is later loaded without the mod,
all missions will still be up and running. But you won't be able to launch new mission until the number of missions drops back to vanilla level.


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