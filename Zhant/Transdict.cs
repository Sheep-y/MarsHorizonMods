using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static ZyMod.ModComponent;

namespace ZyMod.MarsHorizon.Zhant {
   internal class Transdict {
      internal static readonly SortedDictionary< string, string > whole = new SortedDictionary<string, string>();
      internal static string[] part;

      internal const string VerPrefix = "// 版本號 ";
      internal static string WholeCsv => Path.Combine( RootMod.AppDataDir, "Zhant-Dict-Whole.csv" );
      internal static string PartCsv  => Path.Combine( RootMod.AppDataDir, "Zhant-Dict-Terms.csv" );

      internal static void LoadDicts () {
         whole.Clear();
         if ( ! ReadCsv( WholeCsv, ReadWhole ) )
            LoadWholeDefaults();

         var list = new List< string >( 512 );
         if ( ReadCsv( PartCsv, ( a, b ) => { list.Add( a ); list.Add( b ); } ) )
            part = list.ToArray();
         else
            LoadPartDefaults();
      }

      private static bool ReadCsv ( string csv, Action< string, string > OnRow, Action< int, int > CheckVersion = null ) {
         if ( ! File.Exists( csv ) ) return false;
         var buf = new StringBuilder();
         int i = 0, ver = 0;
         Info( "Reading {0}", csv );
         using ( var r = new StreamReader( csv ) ) {
            if ( ! r.TryReadCsvRow( out var row, buf ) ) return false;
            var cells = row.Take( 3 ).ToArray();
            if ( cells.Length <= 3 || cells[ 2 ].StartsWith( VerPrefix ) || ! int.TryParse( cells[ 2 ].Substring( VerPrefix.Length ), out ver ) )
               return false;
            while ( r.TryReadCsvRow( out row, buf ) ) {
               i++;
               cells = row.Take( 2 ).ToArray();
               OnRow( cells[ 0 ], cells[ 1 ] );
            }
         }
         Info( "{0} rows read, version {1}", i, ver );
         CheckVersion?.Invoke( ver, i );
         return true;
      }

      private static void ReadWhole ( string key, string val ) {
         if ( ! whole.ContainsKey( key ) )
            whole.Add( key, val );
         else
            Warn( "Ignoring duplicate key for whole text translation", key, val );
      }

      internal static void LoadWholeDefaults () {
         Info( "Loading default whole text mappings." );
         whole.Clear();
         var list = new string[] {
            "正在登錄中...", "載入中……",
            "簡體中文",  "中文",
            "在 Discord 上<br>加入我們！",  "加入我們的<br>Discord！",
            "{total} 的 {current}", "{current}/{total}",
            "上下文", "過去、現在、未來",
            "保障", "把關者",
            "雙薪", "眼疾手快",
            "跨國公司", "多重國藉",
            "已公開", "公眾關注",
            "點擊跳過", "任意鍵跳過",
            "點擊忽略", "任意鍵關閉",
            "成本", "費用",
            "總成本", "總額",
            "維修",  "維護費用",
            "保存游戲", "儲存遊戲",
            "加載游戲", "載入遊戲",
            "任務困難", "任務難度",

            // Agency Traits
            "高效架構",  "高效建造",
            "經濟架構",  "廉價建造",
            "經濟裝配",  "廉價裝配",
            "經濟施工",  "廉價施工",
            "團隊玩家",  "團隊精神",
            "運營專家",  "行動專家",
            "未知實體",  "無名小子",
            "水星項目",  "水星計劃",
            "永不言敗",  "輸不起",
            "空間補貼",  "宇航資助",
            "擴展專業知識",  "擴展專家",
            "彈性的工作方式",  "彈性作業",
            "並非因為輕而易舉",  "知難而行",
            "薪資適中的宇航員",  "廉價宇航員",
            "發射水平 I",  "發射能手 I",
            "發射水平 II",  "發射能手 II",
            "原型機有效載荷 I",  "實驗性酬載 I",
            "原型機有效載荷 II",  "實驗性酬載 II",
            "揮發性助推器 I",  "易爆燃料 I",
            "揮發性助推器 II",  "易爆燃料 II",
            "失敗的任務支持懲罰加倍", "任務失敗所削減的支持將翻倍",
            "失敗的任務無支持懲罰", "任務失敗不會削減支持",
            "對任務失敗不給予任何懲罰", "任務失敗不會削減支持",
            "{value}% 宇航員雇用成本和維修費用", "{value}% 宇航員雇用成本和人工",
            "{value}% 建築物施工成本和維修費用", "{value}% 建築物施工成本和維護費",
            "由於發射失敗，獲得 100% 的第一階段科學獎勵", "發射失敗後，重試時的階段1科學回報增加 100%",
            "獲得里程碑排名前三位發放 <sprite name=\"Science\"> 科學獎金高達 25%", "打入里程碑前三名時獲得額外 25% <sprite name=\"Science\"> 科學回報",
            "所有與科學相關的毗鄰建築獎金增加一倍", "與科學相關的建築物毗鄰效應全部翻倍",

            // Buildings
            "{Name_{buildingId}}待完成",  "{Name_{buildingId}}完成",
            "{Name_{buildingId}}已完成",  "{Name_{buildingId}}建造完畢",
            "{limit} 的 {current}", "已建 {current}，上限 {limit}",
            "Err:509", "向勇敢的冒險家們致上敬意的好地方，也是用宇航局成就去激勵國民的最佳地方。",
            "備用電源發電機", "備用發電機",

            // Payload / Vehicle / Installation
            "{payload}已完成",  "{payload}建造完畢",
            "{vehicle}已完成",  "{vehicle}建造完畢",
            "運載火箭檢查", "載具檢查",
            "安裝", "設施",
            "起動電源", "初始電力",
            "在發射日期", "發射當日",
            "{duration} 建造時間", "建做需時 {duration}",
            "宇航員時段 +{strength}", "宇航員數量 +{strength}",
            "完成此安裝以獲得以下獎勵", "完成此設施可得獲以下回報",
            "{Name_{building}}安裝已過期", "{Name_{building}}完成使命",
            "由於發射條件不利，發射可靠性懲罰減半", "將發射條件不利導致的發射可靠性減半",
            "發射所需的{Building_Effect_{launchpad}}！", "需要{Building_Effect_{launchpad}}才能發射！",
            "在 <sprite name=Agency{agencyIcon}>{Agency_Default_Name_{agency}} 中 +{value} 聲望", "成功時跟 <sprite name=Agency{agencyIcon}>{Agency_Default_Name_{agency}} +{value} 聲望",
            "載具上面級發射後獲得了 {value} 個經驗等級", "上面級在發射後將獲得 {value} 個經驗等級",
 
            // Training
            "出生日期", "出生",
            "消防宇航員", "解聘宇航員",
            "不適用：任務的訓練", "不可用：任務訓練中",
            "基地科學獎勵", "基本科學回報",
            "基地資金獎勵", "基本資金回報",
            "基地支持獎勵", "基本支持回報",
            "基地聲望獎勵", "基本聲望回報",
            "基地有效載荷可靠性", "基本酬載可靠性",
            "基地發射可靠性", "基本發射可靠性",
            "你尚未選擇訓練簡介。此任務並無發放任何訓練獎金。", "尚未選擇任務訓練，本任務未能受益。",

            // Mission
            "積極", "正面",
            "消極", "負面",
            "{{mission}}預備下一階段！",  "{{mission}}的下一階段已就緒！",
            "{agency}已完成{{mission}}的{phase}階段",  "{agency}已完成{{mission}}的階段{phase}",
            "新的聯合任務適用於{Name_Body_{body}} ！",  "有新的{Name_Body_{body}}聯合任務",
            "新的請求任務適用於{Name_Body_{body}} ！",  "有新的{Name_Body_{body}}請求任務",
            "{agency}下個月將會推出{{mission}}！",  "{agency}將於下個月發射{{mission}}！",
            "{agency}已完成{{mission}}（{rank}）", "{agency}已達成{{mission}}（{rank}）",
            "超過 {turns} 個月獲得 {value}", "{value}，用 {turns} 個月攤分",
            "在 {duration} 內的獎勵總共為 {value}", "分 {duration} 獲得總共 {value}",
            "完成獎金將在下項工作開始時發放", "完成後的額外回報會累加到下項工作",
            "為 <size=28>{targetCount}", "/ <size=28>{targetCount}",
            "{duration} 直至發射", "{duration} 後發射",
            "修改器：臨時", "環境變化：歸位",
            "在本回合中重置", "重置本回合的",
            "宇航員恢復試驗", "宇航員回收試驗",
            "脈沖位置", "發送定位信號",
            "軌道重新校正", "軌道校正",
            "可視數據採集", "可見光譜採樣",
            "紅外光譜", "紅外光譜分析",
            "電源充電", "充電",
            "弱 Ping", "微弱信號",
            "位置外推", "航程預測",
            "低空飛行路線調整", "掠空軌道調整",
            "星鎖丟失", "丟失星光鎖定",
            "{duration} 直至下一階段", "距下一階段還有 {duration}",
         };
         for ( var i = 0 ; i < list.Length ; i += 2 )
            whole.Add( list[ i ], list[ i + 1 ] );
         WriteCsv( WholeCsv, "全文", list );
      }

      internal static void LoadPartDefaults () {
         Info( "Loading default term mappings." );
         part = new string[] {
            "空間", "太空",
            "有效載荷", "酬載",

            "可靠性獎金", "額外可靠性",
            "獎金", "額外",
            "任務懲罰", "失敗懲罰",
            "懲罰", "削減",
            "受到以下削減", "受到以下懲罰",
            "適用獎勵", "獎勵",
            "適用期限已至", "期限已至",
            "所有適用的研究", "的所有研究",
            "適用", "可用",
            "獎勵", "回報",
            "完成回報", "完成獎勵",
            "額外：", "額外獎勵：",
            "障礙物成本", "障礙物清除費",
            "無需成本", "無需花費",
            "研究成本", "研究所需點數",
            "建造成本", "建造費",
            "載具成本", "載具建造費",
            "酬載成本", "酬載建造費",
            "任務時段", "任務欄",
            "空閑時段", "閒置欄",
            "可進行維修作業，並", "可以用來維護載具，並",
            "維修", "經常性開支",

            "没有任何有效任務", "没有任何進行中的任務",
            "有效任務", "任務列表",
            "沒有任何任務列表。要尋找可用任務，", "沒有任務。要尋找可以規劃的任務",
            "表面棲息地", "地面居所",
            "變軌彈道", "轉移航道",
            "準備狀態", "技術指標",
            "中途操控", "中途軌道調整",
            ">總結<", ">概覽<",
            ">上下文<", ">過去、現在、未來<",
            "試驗飛行員", "試飛員",
            "航天地面指揮中心", "任務控制中心",
            "模塊空間站的增加，", "模塊空間站的擴充，",
            "對地軌道以遠", "地球軌道甚至更遠",
            "從此安裝中", "從此設施中",
            "酬載變量", "酬載特化",
            " 增加的", " 提升",
            " 改進的", " 升級",
            " 降低的", " 減低",
            "電源", "電力",
            "總是供應不足！", "總是不夠用！",
            "預備發射！", "發射就緒！",
            "按時間發放", "緩緩獲得",
            "啟動每項工作", "開始每項工作",
            "指令艙", "指揮艙",
            "星的星鎖已丟失", "系統無法鎖定星光",
            "修改器：", "環境變化：",
            "此部分已解鎖，", "此組件未解鎖，",
            "當前工作中持續存在。", "當前工作中起效，不影響後續工作。",
            "{total} 中的 {value}", "{value}/{total} ",
            "成本</b>用於{duration}", "需求<b>，尚餘{duration}",
            "{Agency_Default_Name_ {", "{Agency_Default_Name_{",
 
            "剩余", "剩餘",
            "明了", "明瞭",
            "采集", "採集",
            "采樣", "採樣",
            "采用", "採用",
            "零件", "組件",
            "輕松", "輕鬆",
            "游戲", "遊戲",
            "游客", "遊客",
            "占用", "佔用",
            "盡管", "儘管",
            "跟蹤", "追蹤",
            "細致", "細緻",

            "禁用", "停用",
            "丟失", "失去",
            "菜單", "選單",
            "屏幕", "畫面",
            "阿麗亞娜", "亞利安",
            "大力神", "泰坦",
            "旅行者號", "航行者號",
         };
         WriteCsv( PartCsv, "字詞", part );
      }

      private static void WriteCsv ( string file, string header, string[] content ) { try {
         using ( var w = new StreamWriter( file, false, Encoding.UTF8 ) ) {
            w.WriteCsvLine( header, "替換成", "（版本號 " + new Config().config_version + "）" );
            for ( var i = 0 ; i < content.Length ; i += 2 )
               w.WriteCsvLine( content[ i ], content[ i + 1 ] );
         }
      } catch ( SystemException x ) {
         Error( x );
         try { File.Delete( file ); } catch ( SystemException ) { }
      } }
   }
}
