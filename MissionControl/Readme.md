# ﻿Mission Control #
## A mod of Mars Horizon ##

*No outer planet requests please, thank you.*

This is an unofficial mechanic mod of the space agency management game Mars Horizon (2020).
It adjust mission weightings, tweak milestone challenge requirement and rewards, and replace the RNG used to generate and auto resolve missions.

This mod does not affect saves.
Support all game languages.


# Default Mechanic #

By default, this mod perform the following adjustments.
The change only apply to new missions and challenges, not to existing ones.

## New Mission Destinations ##

When a new mission is rolled:

* Earth and moon missions are heavily biased.
* Mars, Venus, and Mercury missions have a smaller chance to come up, in this order.
* Other destinations will not come up as request / joint missions.
* The mission is rolled by a standalone .Net RNG.

The exact chance depends heavily on which milestone missions you have completed,
since the game only draw from them.

Some events may be keyed to far away request/joint missions, which by default will never come up under this mod.

## New Missions Types ##

When a new mission is rolled:

* Lucrative missions has a slightly higher base chance to come up.
* The chance is further increased when no lucrative mission is active or being requested (joint or not).
* The chance is noticably increased when you've finished all reasearches.

The exact increment is complicated, since it depends on the weighting of other missions,
which varies by mission and can optionally be adjusted by this mod.

In other words, doubling the lucrative weight does not double its chance, ok?
Note that the game will generate one mission type object for each weight.
Wonderful design.
So a large multiplier may cause the game to shutter on turn end and maybe even an Out Of Memory crash.

## New Milestone Challenges ##

When a new milestone challenge is rolled:

* Reward increased for fund type rewards.
* Tech tree with 3 or less researches remaining will be excluded from reward.
* New challenge will have a different reward type from old one if possible, but other mod exclusions take priority.

The mod can also be configured to exclude fund type rewards untill all tech rewards have been excluded.
Just set `milestone_challenge_fund_multiplier` to zero or negative.

## Auto Resolve ##

* Mission's auto-resolve now rolls from a standalone .Net RNG.

Objective experience aside, I don't have _proof_ that this game's RNG is bad,
but the game's space graphic system and messaging system both seems to reset the shared random seed.

.Net's RNG has its issues too, but should be good enough here.
But if you'd rather not mess with it, the replacements can be disabled like other features.


# Installation #

1. Download latest release from GitHub or NexusMods.  Extract with 7-zip or similar tools.

2. Exit game if it is running.

3. Copy or move `version.dll`, `doorstop_config.ini`, and the `Mods` folder into game's root folder.

4. If you have other mods, you may need to overwrite some files.  If the other mods are from me (Sheepy), you can safely overwrite them.

5. That is all.  Launch the game and enjoy.

The mod has no special requirements.  It runs as part of the game.
The src folder contains source code and licenses.


# Configuration #

On first launch, the mod will create `MissionControl.ini` and `MissionControl.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to adjust the number of slots and requirement of missions.
You can also key slots to other buildings, missions, and/or researches, if you know their in-game id.
Each setting have a short description that explains its effects.

If you can't see file extensions, the one with a little gear in its icon is the config file.
You may want to google how to reveal file extensions for good.


# Game Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
It works only on the Microsoft .Net build of the game, i.e. on Windows or Wine,
but should be otherwise compatible with Steam and Epic.

This mod does not change save games or game files.
If a save is made with mission(s) on the deep space slots, and is later loaded without the mod,
all missions will still be up and running. But you won't be able to launch new mission until the number of missions drops back to vanilla level.


# Mod Compatibility #

This mod is compatible with my other mods: Informed, Mission Control, and Skip Animations.
They use the same modding tools, so you will need to overwrite some files.

If this mod is installed last, it may be loaded as the first mod (as directed by `doorstop_config.ini`).
In this case, it will load all `MH_*.dll` in same folder and call the first `public static Main()`.

When you rename or disable a mod, please make sure `doorstop_config.ini` still points to a valid mod.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `MissionControl.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

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
like if there is a typo on a line causing it to be read as zero or negative,
well, the mod is doing its job as written.

If you delete the config file `MissionControl.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default config, which is working at time of release.


# Uninstall #

To remove the mod, rename or delete `MH_MissionControl.dll` from the `Mods` folder under game root.

If you are not using other mods, you may also remove `doorstop_config.ini` and `version.dll` from game root.

If you are using other mods, please make sure `doorstop_config.ini` points to an existing mod, or refer to that mod's instructions.
When in doubt, overwrite with the ini from that mod.

You may also want to remove the `src` folder, if exists, which is distributed with the mod for legality.
The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain; licenses in src folder.