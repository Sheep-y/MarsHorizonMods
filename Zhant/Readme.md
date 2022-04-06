# Zhant #
## Mars Horizon 正體中文補丁 ##

This is an unofficial translation of Mars Horizon (2020 game).
It converts Simplified Chinese into Traditional Chinese, and improves the translantions.

Alternatively, set `dynamic_patch` to False to expand the character support in all languages.
GNU Unifont is bundled to support most languages and symbols.


# 安裝 #

你需要先安裝一個外掛加載器（Mod Loader）。
本補丁支援 BepInEx 5，Simple Mars Horizon Mod Loader，及 Unity Mod Loader。

* **BepInEx** : 將內容解壓到 `BepInEx\plugins` 目錄。
* **Simple Mars Horizon Mod Loader** : 將內容解壓到 `Mods` 目錄。
* **Unity Mod Manager** : 將封包拖放進去，或使用 Install 按鍵。

啓動遊戲後，到遊戲選項將遊戲語言切換到中文即可。

Folder/File locations are relative to game root.
To unintall this mod, delete the extracted files.

# 相容性 #

本補丁於 Mars Horizon 的 1.4.1 GOG.com 版本上開發及測試，
理應支援 微軟視窗/Wine/Proton 下的 Steam 及 Epic。

本補丁不修改遊戲檔案及存檔。但如果你在使用補丁期間修改了載具或存檔名稱，部分中文字可能在移除補丁後無法顯示。
存檔本身依然完好，能正常讀取及重命名。

本補丁相容於我做的其他 Mars Horizon 外掛，它們會顯示正體中文。


# 排錯 #

如果有問題，可以試一下排錯：

## 缺字 ##

如果顯示正體中文，但大量文字無法顯示，一般是字型缺失的問題。
可以到以下官網下載 NotoSans 正體中文字型（普通/香港皆可），解壓到 Mods 目錄。
注意 otf 檔案要直接放在 Mods 目錄裡，不能放子目錄。

https://fonts.google.com/noto/fonts?noto.script=Hant&noto.query=sans

遊戲只使用 Regular, Medium, Bold 三種字重，其餘可刪。
不過沒刪的話，額外的字重也會照樣載入記憶體，以防萬一。

也有一種情況是一開始沒有問題﹐但一段時間後漸漸出現缺字，包括本來能顯示的字。
這意味著渲染過的中文字太多，塞爆了字型的素材圖。萬一真的遇上，只要重啓遊戲就會重設素材。

## 確認補丁掛載 ##

瀏覽 `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`，尋找 `Zhant.log`。

如檔案存在，請刪除後重啓遊戲。
如檔案不存在，或者重啓後依然不存在，那補丁沒有被掛載。
請確保 Mod Loader 正常運作（上述目錄有 `ModLoader.log`）﹐並且本補丁位處 Mods 目錄而不是根目錄。

## 檢查補丁報錯 ##

如果檔案存在，請打開看看。
有錯誤的話會被紀錄，搜尋 "Error" 或 "Exception" 可以找到。

如果真的有錯誤，那大概要編程處理。補丁是開源的。


# 授權 #

GPL v3.  字型皆採用 SIL OFL v1.1 授權。
src 目錄含有源碼和授權聲明。
