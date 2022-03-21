# Zhant #
## Mars Horizon 正體中文補丁 ##

This is an unofficial translation of Mars Horizon (2020 game).
It converts the Simplified Chinese language into Traditional Chinese.
Original translation mistakes are corrected whenever I can.

# 安裝 #

1. 從 GitHub 或 NexusMods 下載最近版。用 7-zip 或類似軟件解壓。

2. 如果遊戲正在運行，先離開遊戲。

3. 解壓後，將 `version.dll`, `doorstop_config.ini`, 及 `Mods` 目錄搬到遊戲的根目錄。

4. 如有其他外掛，你可能需要覆蓋其中的一些檔案。如果其他外掛是我寫的(Sheepy)，可以安心覆蓋。

5. 啓動遊戲，並將遊戲語言設定成 简体中文。補丁會在遊戲載入中文數據時激活，轉換文字並載入額外字型。

本補丁沒有任何特殊要求。src 目錄裡有源碼和使用授權協議，一般不需要閱讀。


# 遊戲相容性 #

本補丁於 Mars Horizon 的 1.4.1 GOG.com 版本上開發及測試。
理論上支援所有 Microsoft .Net 平台，即支援 Steam 及 Epic，只要是視窗版.
本補丁使用 LCMapString Win32 API 進行初步的簡繁轉換，再在必要時進行少量手工調整。

本補丁不修改遊戲檔案及存檔，但如果你在使用補丁期間修改了載具的名稱，名稱中的部分中文字可能會在移除補丁後無法顯示。
存檔本身依然完好無缺，能正常運作。


# 外掛相容性 #

本補丁支援我寫的其他 Mars Horizon 外掛。由於它們使用相同的修改工具，安裝時需要覆蓋一些檔案。

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