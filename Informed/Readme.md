# Informed #
## A mod of Mars Horizon ##

*Your personal Mars Horizon assistant.*

This is an unofficial UI mod of the space agency management game Mars Horizon (2020).
It provides covenience access to information that are otherwise difficult to get or compare,
such as payload options, launch windows, or total building synergies.

This mod does not change game mechanics.
Support all game languages.


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