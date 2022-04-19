# Click Reduction #

This is an unofficial mod of Mars Horizon (2020 game).
It removes non-essential clicks and greatly speeds up gameplay.

Given the increased pace, the game *will* break sooner than normal.
**If the game UI looks or acts wrong, please immediately save, exit, and re-run the game.**
The corruption may spread to other screens if you keep playing.

Among other things, this mod skips construction reports by default.
Please pay attention to event countdowns and mission list's alert icon.


# Installation #

A mod loader is required.  The step depends on the loader:

* **BepInEx** : Extract package into a new subfolder under `BepInEx\plugins`.
* **Simple Mars Horizon Mod Loader** : Extract package into a new subfolder under `Mods`.
* **Unity Mod Manager** : Same as above, or install through the GUI.

To uninstall this mod, delete the extracted files.


## The Story ##

This is my first Mars Horizon mod.
The animations appeal to my 5yo son, whom I purchased the game for, but he has time to waste,
esp. when Hong Kong is still doing covid lockdowns in 2022.

He also didn't pay for the new multi-button trackball after the old one coincidently died
within the first few hours of this game, which by the way costs a lot more than the game.
Hope this mod can save your pointer device in time.

He hates the mod, though.  That and the absent of James Webb Space Telescope.
So, yes, we'd like to see it added to the game.  With full unfold animation please. :D


# Default Behaviour #

This mod provides the following features by default:

## Cinematic Skip ##

* Skip game intro (logos).
* Skip common cinematics.
* Uncommon cinematics are played once per game play.

The mod can be set to permanently skip seen cinematics, but I don't recommend it.
Occasional cinematics are cool.

Crew return, shuttle return, and rocket reuse cinematics are not skipped by default because they are major milestones.
Allow them to play once per session seems to be a good balance.

## General Animation Skip ##

* Blanket reduction of screen fades.
* Blanket reduction of assorted delays.
* Reduction of selected screen delays.
* Reduction of selected screen waits.

This mod blanket-modify all fades and delays at low level,
instead of manually pick them out one by one.  Too many.

However, a short delay leads to a high chance of mini-game UI corruption,
while a non-trivial delay will be felt across the game.
The default values try to strike a balance.

## Specific Animation Speedup ##

* Speedup launch result animations.
* Speedup mini-game animations.
* Speedup phase report animations.

## Auto Bypass ##

* Auto-skip launch countdown.
* Auto-skip uneventful launch and max vehicle level confirmations.
* Auto-skip mini-game intro and briefing.
* Auto-skip uneventful mini-game actions.
* Auto-skip construction reports and research reports.


# Configuration #

The mod config file on BepInEx is `BepInEx\configs\Zy.MarsHorizon.ClickReduction.cfg`.
On other mod loaders it is `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon\ClickReduction.ini`.

You can edit the ini file to disable or adjust various features and timings.

BepInEx also has a configuration manager plugin which can reconfig the mod on the fly.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages are supported.

This mod does not change save games or game files.

Due to the number of screen it mods and the number of information it pulls,
each non-minor game update have a good chance of breaking something.
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