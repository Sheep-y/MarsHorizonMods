# Informed #
## A mod of Mars Horizon ##

*Your personal Mars Horizon assistant.*

This is an unofficial UI mod of Mars Horizon (2020 game).
It provides convenience access to information that are otherwise difficult to get or compare,
such as payload options, launch windows, or total building synergies.


# Installation #

1. Setup Mars Horizon Mod Loader.
2. Download latest mod release from GitHub or NexusMods.  Open with 7-zip or similiar tools.
3. Extract (Drag and Drop) `MH_Informed.dll` into the game's `Mods` folder.
4. Launch the game and enjoy.


# Default Behaviour #

This mod provides the following features.
Features can be individually disabled by mod config (see below).

## Solar System Screen ##

* Show info icon next to Mission when a mission slot is availble.
* Show info icon next to Crew when new candidates are availble.
* Show info icon next to Diplomacy when a joint mission can be proposed.
* Spacepedia will now never show an icon.

Hiding spacepedia notice is _way_ simpler, and _way_ more reliable, than trying to fix the mark all button in-game.
If you insist, a simple google should show you a simple json fix.

## Planet Screen ##

* A "Launch Window" button is added to the bottom of milestone/request mission list.
* The expiry countdown is displayed on the right when you select a mission.
* The potential payload(s) is displayed on the right when you select a mission, alongside their build times, weights, and crews.

For milestone missions, the mission itself must be researched to see the payload list.
The payload build time already factors in all applicable bonus.

## Vehicle Design Screen ##

* After you hover out of any vehicle option on the left, the launch window around the point of vehicle completion will be listed.
* The bonus and penalty of each contractor is displayed under their name for easy comparison.

I couldn't find a way to show launch window without mouse movement, sorry about that.
Contractor bonus already factor in applicable multipliers.

The number of months and their colours can be changed in mod config.

## Research Screen ##

* Booster research node will now show all possible supplements and the combined capacity of each.

If the supplement has not been researched, a warning icon will be added.

## Base Screen ##

* Show total building synergies when the screen is not in build/move/clear mode, and when no building is being hovered.

Only synergies count.  Building effects are not included on the list.


# Configuration #

On first launch, the mod will create `Informed.ini` and `Informed.log` in the game's user data folder,
i.e. %AppData%\..\LocalLow\Auroch Digital\Mars Horizon

You can edit the ini file to disable or adjust various features.
Each setting have a short description that explains its effects.

If you can't see file extensions, the one with a little gear in its icon is the config file.
You may want to google how to reveal file extensions for good.


# Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
but should be otherwise compatible with Steam and Epic (Windows only).
All game languages are supported.

This mod does not change save games or game files.

Due to the number of screen it mods and the number of information it pulls,
each non-minor game updates have a considerable chance of breaking something.
Good luck!

This mod is compatible with my other mods: Deep Space Slots, Mission Control, Payload QA, Skip Animation, and Zhant.


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