# Skip Animations #

*Mars Horizon on Adrenaline.*

This is an unofficial mod of Mars Horizon (2020 game).
It reduces or eliminates many delays and screen fades, auto-skip normal launch and normal minigame actions, bypass seen cutscenes (reset on launch), and skips a few common cutscenes by default.

**If the game UI became misaligned during mini-game, immediately save, quit, and relaunch.**
If you try to finish the mini-game, the corruption will spread to other screens including save screen.

The game may also crash during mini-game, but it's something that also happens on vanilla.  So I guess that's not me.


# Installation #

A mod loader is required.
Supports BepInEx v5, Simple Mars Horizon Mod Loader, Unity Mod Manager, or Unity Doorstop.

* **BepInEx** : Extract package content into `BepInEx\plugins` folder.
* **Simple Mars Horizon Mod Loader** : Extract package into `Mods` folder.
* **Unity Mod Manager** : Drag and drop package or use the Install button.
* **Unity Doorstop** : Edit `doorstop_config.ini`, set `targetAssembly` to path of mod dll.

Folder/File locations are relative to game root.
To unintall this mod, delete the extracted files.


## The Story ##

This is my first Mars Horizon mod.
The animations appeal to my 5yo son, but he is not a seasoned gamer and he has time to waste,
esp. when we are still doing covid lockdown here in Hong Kong on 2022.

He also didn't pay for the new multi-button trackball after the old one dies from excessive clicks within the first few hours of this game,
which by the way is a lot more expensive than this game.

I sincerely hope this mod can save your pointer device in time.

He hates the mod, though.  Rocket rocket rocket rocket go launch! (cue Storybots.)
Did I mention I buy this game for him?  Thanks James Webb Space Telescope.

Yes.  We'd like to see James Webb added to the game.  With full unfold animation please. :D


# Default Behaviour #

This mod provides the following features by default:

## Cinematic Skip ##

* Skip game intro (logos).
* Skip mission control in/out cinematics, launch cinematics, general earth success/fail cinematics.
* Other cinematics will be played once per game launch.  Restarting the game will clear the memory.

The skip list can be configured.  The defaults are the ones I'm already sick of after the first few hours.

You can set the mod to permantely skip seen cinematics, which will add their ids to the skip list,
but I tried it and don't recommend it.  Occasional cinematics are cool.

Crew return, shuttle return, and rocket reuse cinematics are not skipped by default because they are major milestones.
Skip them once per session seems to be acceptable tradeoff.

## General Animation Skip ##

* Blanket removal of screen fades.
* Blanket reduction of assorted delays.
* Reduction of selected screen delays.
* Removal of selected screen waits.

Because there are too many delays in this game,
this mod use a carpet bombing approach to remove fades and delays.

Unfortunately, a short delay leads to a high chance of mini-game UI corruption,
while a non-trivial delay will be felt across the game.

So, this mod let you control the delay.
Don't worry; it will only shorten delays, not increase them.

## Specific Animation Speedup ##

* Speedup launch result animations.
* Speedup mini-game animations.
* Speedup phase report animations.

## Auto Bypass ##

* Auto-skip launch countdown.
* Auto-skip uneventful launch and max vehicle level up.
* Auto-skip mini-game intro and briefing.
* Auto-skip uneventful mini-game actions.
* Auto-skip construction reports and research report.


# Configuration #

The mod config file on BepInEx is `BepInEx\configs\Zy.MarsHorizon.DeepSpaceSlots.cfg`.
On other mod loaders it is `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon\DeepSpaceSlots.ini`.

You can edit the ini file to disable or adjust various features and timings.

BepInEx also has a configuration manager plugin which can modify the parameters on the fly.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be compatible with Steam and Epic on Windows/Wine/Proton.
All game languages are supported.

This mod does not change save games or game files.

Due to the number of screen it mods and the number of information it pulls,
each non-minor game updates have a good chance of breaking something.
Good luck!


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

If using BepInEx, read its log to be sure that mod is loaded.
Otherwise, find `DeepSpaceSlots.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

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

GPL v3.  The src folder contains complete source code and license.