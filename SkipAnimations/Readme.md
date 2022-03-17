# Skip Animations #
## A mod of Mars Horizon ##

*Mars Horizon on Steroid.*

This is an unofficial UI mod of the space agency management game Mars Horizon (2020).
It reduces or eliminates many delays and screen fades, auto-skip normal launch and normal minigame actions, bypass seen cutscenes (reset on launch), and skips a few common cutscenes by default.

**Because of speed up, game UI may go out of alignment during mini-game.  Immediately save your game, quit, and relaunch.**
You can try to finish the mini-game, but the corruption will get worse when you return from mission and will affect save screen.
In the end, you still save a lot more time than the time spend on relaunch, esp. if you auto-resolve.

The game may also crash during mini-game, but it's something that also happens on vanilla.  So I guess that's not me.

This mod does not change game mechanics.
Support all game languages.

## The Story ##

This is my first Mars Horizon mod.
The animations appeal to my 5yo son, but he is not a seasoned gamer and he has time to waste.
He also didn't pay for the new multi-button trackball after the old one dies from excessive clicks within the first few hours of this game, which by the way is a lot more expensive than the game.

I sincerely hope this mod can save your pointer device in time.

He hates the mod, though.  Rocket rocket rocket rocket go launch! (cue Storybots.)
Did I mention I buy this game without discount for him?  Thanks James Webb Space Telescope.

Yes.  We'd like to see James Webb added to the game.  With full unfold animation please. :D

P.S. While the story is true, speed up cheats and mods are common because they speed up development.
Game and mod development invariably involves relaunching and redoing the same things again and again,
and cutting short the flow has a very positive effect on our mental health.


# Default Behaviour #

This mod provides the following features.

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


# Installation #

1. Download latest release from GitHub or NexusMods.  Extract with 7-zip or similar tools.

2. Exit game if it is running.

3. Copy or move `version.dll`, `doorstop_config.ini`, and the `Mods` folder into game's root folder.

4. If you have other mods, you may need to overwrite some files.  If the other mods are from me (Sheepy), you can safely overwrite them.

5. That is all.  Launch the game and enjoy.

The mod has no special requirements.  It runs as part of the game.
The src folder contains source code and licenses.


# Configuration #

On first launch, the mod will create `Informed.ini` and `Informed.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to disable or adjust various features.
Each setting have a short description that explains its effects.

If you can't see file extensions, the one with a little gear in its icon is the config file.
You may want to google how to reveal file extensions for good.


# Game Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
It works only on the Microsoft .Net build of the game, i.e. on Windows or Wine,
but should be otherwise compatible with Steam and Epic.

This mod does not change save games or game files.

Due to the number of screen it mods and the number of information it pulls,
each non-minor game updates have a considerable chance of breaking something.
Good luck!


# Mod Compatibility #

This mod is compatible with my other mods: Deep Space Slots, Mission Control, and Skip Animations.
They use the same modding tools, so you will need to overwrite some files.

If this mod is installed last, it may be loaded as the first mod (as directed by `doorstop_config.ini`).
In this case, it will load all `MH_*.dll` in same folder and call the first `public static Main()`.

When you rename or disable a mod, please make sure `doorstop_config.ini` still points to a valid mod.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `Informed.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod is not loaded at all.

Please check that `version.dll` exists at game folder,
and that `doorstop_config.ini` in the same folder is pointing to the right dll and path.
When in doubt, reinstall this mod and overwrite the files.

Double-clicking `doorstop_config` should open the file in notepad, and the line `targetAssembly` is the first mod to be loaded.
Any other mods will be depending on it loading them.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log.
Errors in this mods are contained, so they may not break the game, but will be recorded in mod log.

Errors normally have "Error" or "Exception" in the message, and are usually logged differently from normal messages.
If you do find errors, chances are it will need to be fixed by a programmer modder.  Mod is open source.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.
So, if the mod is configured to not work,
like if there is a typo on a line causing it to be read as false,
well, the mod is doing its job as written.

If you delete the config file `Informed.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default config, which is working at time of release.


# Uninstall #

To remove the mod, rename or delete `MH_Informed.dll` from the `Mods` folder under game root.

If you are not using other mods, you may also remove `doorstop_config.ini` and `version.dll` from game root.

If you are using other mods, please make sure `doorstop_config.ini` points to an existing mod, or refer to that mod's instructions.
When in doubt, overwrite with the ini from that mod.

You may also want to remove the `src` folder, if exists, which is distributed with the mod for legality.
The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain; licenses in src folder.