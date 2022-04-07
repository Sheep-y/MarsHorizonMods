# Mod Loader Alternatives #

This document lists the step to install BepInEx and Unity Mod Manager for Mars Horizon.

Please install only **one** mod loader.  They generally conflict with each other.

Other mod loaders that support generic Unity games can also be used on Mars Horizon,
but you need to figure out how to get it to work.


## BepInEx ##

This guide is tested on BepInEx v5.4.19.

1. Download BepInEx 5.x (x64) from `https://github.com/BepInEx/BepInEx/releases`

2. Extract contents to game root:  `winhttp.dll`, `doorstop_config.ini`, and the `BepInEx` folder.

3. Launch game, then Exit game.  You can Alt+F4 as soon as you see the logos.

4. Check that `BepInEx\config` and `BepInEx\plugins` folders have been created.

5. Place your mods in their own subfolders in the plugins folder.
For example, the "Informed" mod may be placed as `BepInEx\plugins\Informed-1-0-123\MH_Informed.dll`.

6. Optionally, install mod config manager to manage configs in-game by pressing F1:
https://github.com/BepInEx/BepInEx.ConfigurationManager

7. Mod developers may also install UnityExplorer which provides a scene inspector and REPL console on F7:
https://github.com/sinai-dev/UnityExplorer
, while ScriptEngine enables hot-reload on F6: https://github.com/BepInEx/BepInEx.Debug


## Unity Mod Manager ##

This guide is tested on Unity Mod Manager v0.24.

1. Download UMM and extract in Documents, Desktop, whatever: https://www.nexusmods.com/site/mods/21

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

3. Save file and run UMM.  Find "Mars Horizon" from game list.

4. Pick game folder if not auto-detected.  Double-check that the game is correct, not "A Dance of Fire and Ice" or other games.

5. Click "Install".  Either method works.  A `Mods` folder is created under game root.

6. Create a new subfolder under `Mods`, e.g. `Mods\Loader`.

7. Find `Mods\MH_ModLoader.dll` and `src\Info.json` from this mod loader and put them in this subfolder.
e.g. `Mods\Loader\MH_ModLoader.dll` and `Mods\Loader\Info.json`.

8. Make sure `Mods` and the loader subfolder does *not* contain `0Harmony.dll`.

9. Place mods in the loader subfolder, either together or in individual subfolders.
For example, the "Informed" mod may be placed as `Mods\Loader\Informed-1-0-123\MH_Informed.dll`.


## Other Mod Loaders ##

Programming is complicated.  Honest.

For example, BepInEx requires hard reference to it, which causes the mods to fail to
load in Unity Mod Manager because it does not properly handle ReflectionTypeLoadException,
that is why we need this mod loader to serve as a bridge between UMM and my Mars Horizon mods.
Which is possible because this mod loader does ignore unresolvable types and functions correctly
regardless of when it is loaded.

So, no, I can't feasibly support Melon Loader or Vortax or whatever new mod loader that is the
latest hot topic.