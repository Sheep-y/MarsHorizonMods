# ﻿Mission Control #

This is an unofficial mod of Mars Horizon (2020 game).
It adjust mission weightings, tweak milestone challenge requirement and rewards, and replace the RNG used to generate and auto resolve missions.

The default configuration gives you a greater freedom to do non-Earth planet milestones,
since they have a much reduced chance to be selected for request missions.


# Installation #

A mod loader is required.
Supports BepInEx v5, Simple Mars Horizon Mod Loader, and Unity Mod Manager.

* **BepInEx** : Extract package into a new subfolder under `BepInEx\plugins`.
* **Simple Mars Horizon Mod Loader** : Extract package into a new subfolder under `Mods`.
* **Unity Mod Manager** : Drag and drop package into UMM or use the Install button.

To uninstall this mod, delete the extracted files.


# Default Mechanic #

This mod provides the following features by default.
They only apply to new missions and challenges, not to existing ones.

## New Mission Destinations ##

When a new mission is rolled:

* Earth and moon missions are heavily biased.
* Mars, Venus, and Mercury missions have a smaller chance to come up, in this order.
* Other destinations will not come up as request / joint missions.
* Make new missions roll from a standalone RNG.

The exact chance depends heavily on which milestone missions you have completed,
since the game only draw from them.

Some events may be keyed to far away request/joint missions, which by default will never come up under this mod.

## New Missions Types ##

When a new mission is rolled:

* Lucrative missions has a slightly higher base chance to come up.
* The chance is further increased when no lucrative mission is active or being requested (joint or not).
* The chance is noticably increased when you've finished all reasearches.
* The roll is made from an independent RNG.

The exact boost is complicated, and depends on the weighting of other missions,
which varies by mission and can optionally be adjusted by this mod.
In other words, doubling lucrative weight does not double its chance, ok?

As for RNG, it is because the chance of getting the same mission after loading a save is way too high.
This is not a proof, but the game's space graphic system and messaging system both seems to reset the shared random seed.

## New Milestone Challenges ##

When a new milestone challenge is rolled:

* Reward increased for fund type rewards.
* Tech tree with 3 or less researches remaining will be excluded from reward.
* New challenge will have a different reward type from old one if possible, but other mod exclusions take priority.

The mod can also be configured to exclude fund type rewards untill all tech rewards have been excluded.
Just set `milestone_challenge_fund_multiplier` to zero or negative.


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

If using BepInEx, read its log to be sure that mod is loaded.
Otherwise, find `MissionControl.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

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