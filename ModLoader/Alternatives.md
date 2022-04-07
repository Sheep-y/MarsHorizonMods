# Mod Loader Alternatives #

Below are the steps to install BepInEx (recommended) and Unity Mod Manager for Mars Horizon.

Please install only **one** mod loader.  They generally conflict with each other.


## BepInEx ##

This guide is tested on BepInEx v5.4.19.

1. Download BepInEx 5.x (x64) from https://github.com/BepInEx/BepInEx/releases

2. Extract contents to game root:  `winhttp.dll`, `doorstop_config.ini`, and the `BepInEx` folder.

3. Launch game, then Exit game.  You can Alt+F4 as soon as you see the logos.

4. Check that `BepInEx\config` and `BepInEx\plugins` folders have been created.

5. Place your mods in their own subfolders in the plugins folder.
For example, the "Informed" mod may be placed as `BepInEx\plugins\Informed-1-0-123\MH_Informed.dll`.

6. Optionally, install mod config manager to change mod configs in-game by pressing F1:
https://github.com/BepInEx/BepInEx.ConfigurationManager

7. Mod developers may also install UnityExplorer which provides a scene inspector and REPL console on F7:
https://github.com/sinai-dev/UnityExplorer
, while ScriptEngine enables hot-reload on F6: https://github.com/BepInEx/BepInEx.Debug


## Unity Mod Manager ##

This guide is tested on Unity Mod Manager v0.24.

1. Download UMM and extract in a folder of your choice: https://www.nexusmods.com/site/mods/21

2. Open `UnityModManagerConfigLocal.xml` with a text editor, and add these lines *before* `</Config>`.

```
	<GameInfo Name="Mars Horizon">
		<Folder>Mars Horizon</Folder>
		<ModsDirectory>Mods</ModsDirectory>
		<ModInfo>Info.json</ModInfo>
		<GameExe>Mars Horizon.exe</GameExe>
		<EntryPoint>[Assembly-CSharp.dll]SplashDelayScene.cctor:Before</EntryPoint>
		<GameVersionPoint>[UnityEngine.CoreModule.dll]UnityEngine.Application.version</GameVersionPoint>
		<StartingPoint>[Assembly-CSharp.dll]SplashDelayScene.Start:Before</StartingPoint>
		<UIStartingPoint>[Assembly-CSharp.dll]Astronautica.View.TitleScreen.AstroInitialise:After</UIStartingPoint>
		<MinimalManagerVersion>0.24</MinimalManagerVersion>
	</GameInfo>
```

3. Save file and run UMM.  Find "Mars Horizon" from game list.  If Mars Horizon is not found, you messed up the last step!

4. Pick game folder if not auto-detected.  Double-check that the game is correct, not "A Dance of Fire and Ice" or other games.

5. Click "Install".  Either method works.  A `Mods` folder is created under game root.

6. Create a new subfolder under `Mods`, e.g. `Mods\Loader`.

7. Find `Mods\MH_ModLoader.dll` and `src\Info.json` from this mod loader and put them in this subfolder.
e.g. `Mods\Loader\MH_ModLoader.dll` and `Mods\Loader\Info.json`.

8. Make sure `Mods` and the loader subfolder does *not* contain `0Harmony.dll`.

9. Place mods in their own subfolder under the loader subfolder.
For example, the "Informed" mod may be placed as `Mods\Loader\Informed-1-0-123\MH_Informed.dll`.

This mod loader is required as a bridge because Unity Mod Manager does not correctly handle
`ReflectionTypeLoadException` as of version 0.24.2.  Until it fixes that, it cannot load
BepInEx compatible mods, and they shall not be installed through the GUI.