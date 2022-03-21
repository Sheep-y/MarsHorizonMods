# Zhant #
## Mars Horizon 正體中文補丁 ##

This is an unofficial translation of Mars Horizon (2020 game).
It converts Simplified Chinese into Traditional Chinese.


# 安裝 #

1. 安裝 Mars Horizon Mod Loader。將內容解壓到遊戲根目錄就可以。
2. 從 GitHub 或 NexusMods 下載本補丁的最近版。用 7-zip 或類似軟件打開。
3. 將 `MH_Zhant.dll` 和三個 NotoSans 字型擋案拖放到遊戲的 `Mods` 目錄。（共四個檔案）
4. 啓動遊戲，將遊戲語言切換到中文即可。


# 相容性 #

本補丁於 Mars Horizon 的 1.4.1 GOG.com 版本上開發及測試，
理應支援 微軟視窗/Wine/Proton 下的 Steam 及 Epic。
本補丁使用 LCMapString Win32 API 進行初步的簡繁轉換，再在必要時進行額外調整。

本補丁不修改遊戲檔案及存檔。但如果你在使用補丁期間修改了載具的名稱，部分中文字可能在移除補丁後無法顯示。
存檔本身依然完好無缺，能正常運作。

本補丁相容於我做的其他 Mars Horizon 外掛，它們會顯示正體中文。


# 排錯 #

如果有問題，可以試一下排錯：

## 缺字 ##

如果顯示正體中文，但部分文字無法顯示，一般是字型缺失的問題。
可以到以下官網下載 NotoSans 正體中文字型（普通/香港皆可），解壓到 Mods 目錄。
注意 otf 檔案要直接放在 Mods 裡，不能放子目錄。

https://fonts.google.com/noto/fonts?noto.script=Hant&noto.query=sans

遊戲只使用 Regular, Medium, Bold 三種字重，其餘可刪。
不過沒刪的話，額外的字重也會照樣載入，以防萬一。


## 確認補丁掛載 ##

瀏覽 `%AppData%\..\LocalLow\Auroch Digital\Mars Horizon`，尋找 `Zhant.log`。

如檔案存在，請刪除後重啓遊戲。
如檔案不存在，或者重啓後依然不存在，那補丁沒有被掛載。
請確保 Mod Loader 正常運作（上述目錄有 `ModLoader.log`）﹐並且本補丁位處 Mods 目錄而不是根目錄。

## 檢查補丁報錯 ##

如果檔案存在，請打開看看。
有錯誤的話會被紀錄，搜尋 "Error" 或 "Exception" 可以找到。

如果真的有錯誤，那大概要編程處理。補丁是開源的。


# 反安裝 #

從遊戲的 `Mods` 目錄中刪除 `MH_Zhant.dll` 和所有 `NotoSans` 檔案。

如果你沒有使用其他外掛，也可以反安裝包括 Mod Loader 在內的所有東西。
也就是刪除遊戲目錄下的 `doorstop_config.ini`, `version.dll`, 和 `Mods` 目錄。

本補丁不修改遊戲檔案，無復驗證修復。


# 授權 #

GPL v3.  附錄的字型採用 SIL OFL v1.1 授權。
src 目錄含有源碼和授權聲明。
