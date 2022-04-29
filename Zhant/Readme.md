# Zhant #
## Mars Horizon 正體中文補丁 ##

This is an unofficial translation of Mars Horizon (2020 game).
It converts Simplified Chinese into Traditional Chinese, and improves the translations.


# 安裝 #

你需要先安裝一個外掛加載器（Mod Loader）。安裝方法視乎使用甚麼加載器：

* **BepInEx** : 於 `BepInEx\plugins` 新建子目錄並將內容解壓到其下。
* **Simple Mars Horizon Mod Loader** : 將於 `Mods` 新建子目錄並將內容解壓到其下。
* **Unity Mod Manager** : 同上，或使用 UMM 的圖型用戶介面進行安裝。

啓動遊戲後，到遊戲選項將遊戲語言切換到中文即可。


# 功能 #

當遊戲語言為中文時，或當 `dynamic_patch` 設置為 False 時，本補丁會：

1. 加載正體中文字型。
2. 當遊戲顯示文字時，先進行繁簡字符轉換，
3. 然後依照 全文對譯表 進行全文匹配轉換。
4. 如無匹配，則依照 字詞對譯表 進行字詞轉換。

## 字型加載步驟 ##

1. 載入同目錄中的 `NotoSans(CJKtc|CJKhk|TC|HK)-*.otf`。
2. 在當中尋找 Medium 字重，加到全域預設字型列表。
3. 順序載入 `TW-Sung.ttf`, `HanaMinA.ttf`, `HanaMinB.ttf`, `CODE2000.TTF`, `unifont.ttf`, 及 `unifont_upper.ttf`，加到全域字型列表。
4. 當遊戲切換畫面時，為文字項匹配相應字重的正體字型。


# 設置 #

當使用 BepInEx 時，設定檔為 `BepInEx\configs\Zy.MarsHorizon.Zhant.cfg`。
當使用其他加載器時則為 `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon\Zhant.ini`.

BepInEx 有一個設定外掛，能在遊戲中設置補丁，但大部分功能皆需要重啓遊戲才會生效。

另外，補丁會在上述的 AppData 目錄下建立 `Zhant-Dict-Whole.csv` 和 `Zhant-Dict-Whole.csv` 兩個檔案，
分別是 全文對譯表 和 字詞對譯表。前者有優先。修改此兩表可以補完或修正補丁的額外翻譯。



# 相容性 #

本補丁於 Mars Horizon 的 1.4.1 GOG.com 版本上開發及測試，
理應支援 微軟視窗/Wine/Proton 下的 Steam 及 Epic。

本補丁不修改遊戲檔案及存檔。但如果你在使用補丁期間修改了載具或存檔名稱，部分中文字可能在移除補丁後無法顯示。
存檔本身依然完好，能正常讀取及重命名。

本補丁相容於我做的其他 Mars Horizon 外掛，它們會顯示正體中文。


# 排錯 #

如果有問題，可以試一下排錯：

## 字型缺失 ##

如果顯示正體中文，但大量文字無法顯示，一般是字型缺失的問題。
可以到以下官網下載 NotoSans 正體中文字型（普通/香港皆可），解壓到 Mods 目錄。
注意 otf 檔案要跟補丁的 dll 放在同一目錄裡，不能放子目錄。

https://fonts.google.com/noto/fonts?noto.script=Hant&noto.query=sans

遊戲只使用 Regular, Medium, Bold 三種字重，其餘可刪。
不過沒刪的話，額外的字重也會照樣載入記憶體，以防萬一。

## 字型無法載入 ##

部分動態外掛加載器，例如 BepInEx 的 ScriptEngine，會導致補丁無法找到自己的位置，導致無法載入字型。

## 字型素材損毀 ##

也有一種情況是一開始沒有缺字﹐但一段時間後突然大量缺字，包括本來能顯示的字。
這意味著渲染到素材的中文字太大，塞爆了素材。調大素材或調小渲染大小都可以延後問題的發生。
另外，素材是臨時生成的。只要重啓遊戲就會重設素材。

## 確認補丁掛載 ##

如使用 BepInEx，閱讀 `LogOutput.log` 可以知道補丁是否被正確載入。

否則，請看 `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon` 是否有 `Zhant.log`。
如檔案存在，請刪除後重啓遊戲。

如檔案不存在，或者重啓後依然不存在，或 BepInEx 沒有掛載補丁，那要先解決這個問題。請參閱加載器的指引。

## 檢查補丁報錯 ##

如果檔案存在，請打開看看。
有錯誤的話會被紀錄，搜尋 "Error" 或 "Exception" 可以找到。

如果真的有錯誤，那大概要編程處理。補丁是開源的。

## 檢查補丁設定 ##

補丁是可以設定的。如設定有誤，補丁可能無法正常運作。

只要刪除設定檔及對譯表，即可恢復原廠設定。


# 授權 #

GPL v3。字型皆採用 SIL OFL v1.1 授權。
src 目錄含有源碼和授權聲明。

https://github.com/Sheep-y/MarsHorizonMods/