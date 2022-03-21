# ﻿Mission Control #
## A mod of Mars Horizon ##

*No outer planet requests please, thank you.*

This is an unofficial mechanic mod of the space agency management game Mars Horizon (2020).
It adjust mission weightings, tweak milestone challenge requirement and rewards, and replace the RNG used to generate and auto resolve missions.


# Installation #

1. Setup Mars Horizon Mod Loader.
2. Download latest mod release from GitHub or NexusMods.  Open with 7-zip or similiar tools.
3. Extract (Drag and Drop) `MH_MissionControl.dll` into the game's `Mods` folder.
4. Launch the game and enjoy.


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


# Configuration #

On first launch, the mod will create `MissionControl.ini` and `MissionControl.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to adjust the number of slots and requirement of missions.
You can also key slots to other buildings, missions, and/or researches, if you know their in-game id.
Each setting have a short description that explains its effects.

If you can't see file extensions, the one with a little gear in its icon is the config file.
You may want to google how to reveal file extensions for good.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be otherwise compatible with Steam and Epic (Windows only).
All game languages are supported.

This mod does not change save games or game files.
If a save is made with mission(s) on the deep space slots, and is later loaded without the mod,
all missions will still be up and running. But you won't be able to launch new mission until the number of missions drops back to vanilla level.

This mod is compatible with my other mods: Deep Space Slots, Informed, Payload QA, Skip Animation, and Zhant.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `MissionControl.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

If the log exists, delete it and re-launch the game.

If the log does not exists, or is not recreated after relaunch, the mod is not loaded.
Please follow mod loader's troubleshoot section.

## Check Mod Errors

If you get the log but mod is still not working, read the mod log and/or game log (`Player.log`).

Errors normally have "Error" or "Exception" in the message, and are usually logged differently from normal messages.
If you do find errors, chances are it will need to be fixed by a programmer modder.  Mod is open source.
Resetting the config may help, though.  Which brings us to...

## Check Mod Config

The mod is configurable.  If it is not configured right, such as typos, the mod may not work as expected.

If you delete the config file `MissionControl.ini` from `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`,
the mod will recreate it with default values.


# Uninstall #

To remove the mod, rename or delete `MH_MissionControl.dll` from the game's `Mods` folder.

If you are not using other mods, you may also remove the mod loader.
See the mod loader's instruction for details.

The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain.

The src folder contains source code and licenses for legality.