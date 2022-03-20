# Zhant #
## A mod of Mars Horizon ##

*Because Simplified Chinese is not our Chinese*

This is an unofficial UI mod of the space agency management game Mars Horizon (2020).
It converts the Simplified Chinese language into Traditional Chinese, while tweaking the translations here and there.

Games that are not set to Chinese should not be affected.  The additional fonts won't even load in that case.

This mod does not change saves or game mechanics.


# Behaviour #

When installed, and when the game is set to Chinese, this mod will translate all Simplified Chinese into Traditional Chinese.
In additional, new fallback fonts will be loaded to support the new characters.

When the game is set to non-Chinese, it should do nothing other than to wait for game language changes.

Unlike most of my mods, this mod is not configurable.


# Installation #

1. Download latest release from GitHub or NexusMods.  Extract with 7-zip or similar tools.

2. Exit game if it is running.

3. Copy or move `version.dll`, `doorstop_config.ini`, and the `Mods` folder into game's root folder.

4. If you have other mods, you may need to overwrite some files.  If the other mods are from me (Sheepy), you can safely overwrite them.

5. That is all.  Launch the game and enjoy.

The mod has no special requirements.  It runs as part of the game.
The src folder contains source code and licenses.


# Game Compatibility #

The mod is developed and tested on Mars Horizon version 1.4.1, GOG.com,
It works only on the Microsoft .Net build of the game, i.e. on Windows or Wine,
but should be otherwise compatible with Steam and Epic.

The mod depends on the LCMapString Win32 API for basic translation.
So if it doesn't work on Wine, that may be the reason.

This mod does not change save games or game files, but if you modify vehicle names and save them,
the traditional chinese characters may fail to display when the mod is deactivated.
The save would still works normally.

If the game natively support Traditional Chinese in the future,
the mod would still be activated when it is set to Simplified Chinese, but not on Traditional.


# Mod Compatibility #

This mod is compatible with my other mods: Deep Space Slots, Mission Control, PayloadQA, and Skip Animations.
They use the same modding tools, so you will need to overwrite some files.

If this mod is installed last, it may be loaded as the first mod (as directed by `doorstop_config.ini`).
In this case, it will load all `MH_*.dll` in same folder and call the first `public static Main()`.

When you rename or disable a mod, please make sure `doorstop_config.ini` still points to a valid mod.


# Troubleshoot #

If the mod doesn't work, there are a few things you can try:

## Check Mod Is Loaded

Find `Zhant.log` in `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`.

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


# Uninstall #

To remove the mod, rename or delete `MH_Zhant.dll` from the `Mods` folder under game root.

If you are not using other mods, you may also remove `doorstop_config.ini` and `version.dll` from game root.

If you are using other mods, please make sure `doorstop_config.ini` points to an existing mod, or refer to that mod's instructions.
When in doubt, overwrite with the ini from that mod.

You may also want to remove the `src` folder, if exists, which is distributed with the mod for legality.
The mod does not modify game files, so there is no need to Verify Files.


# License #

GPL v3.  Bundled libraries are either MIT or public domain; licenses in src folder.