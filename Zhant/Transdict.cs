using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZyMod.MarsHorizon.Zhant {
   internal class Transdict : LogAccess {
      internal static readonly SortedDictionary< string, string > whole = new SortedDictionary<string, string>();
      internal static string[] part;

      internal const string VerPrefix = "// 版本號 ";
      internal static string WholeCsv => Path.Combine( RootMod.AppDataDir, "Zhant-Dict-Whole.csv" );
      internal static string PartCsv  => Path.Combine( RootMod.AppDataDir, "Zhant-Dict-Terms.csv" );

      internal static void LoadDicts () {
         if ( Environment.UserName == "Sheepy" ) {
            LoadWholeDefaults();
            LoadPartDefaults();
         }

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
            "{Name_{buildingId}}待完成",  "{Name_{buildingId}}完成",
            "{agency}已完成{{mission}}的{phase}階段",  "{agency}已完成{{mission}}的階段{phase}",
            "新的聯合任務適用於{Name_Body_{body}} ！",  "有新的{Name_Body_{body}}聯合任務",
            "新的請求任務適用於{Name_Body_{body}} ！",  "有新的{Name_Body_{body}}請求任務",
            "{agency}下個月將會推出{{mission}}！",  "{agency}將於下個月發射{{mission}}！",
            "{{mission}}預備下一階段！",  "{{mission}}的下一階段已就緒！",
            "{Name_{buildingId}}已完成",  "{Name_{buildingId}}建造完畢",
            "{payload}已完成",  "{payload}建造完畢",
            "{vehicle}已完成",  "{vehicle}建造完畢",
            "上下文", "過去、現在、未來",
            "雙薪", "眼疾手快",
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

            // Payload / Vehicle / Installation
            "運載火箭檢查", "載具檢查",
            "由於發射條件不利，發射可靠性懲罰減半", "將發射條件不利導致的發射可靠性減半",
            "宇航員時段 +{strength}", "宇航員數量 +{strength}",
            "安裝", "設施",
            "完成此安裝以獲得以下獎勵", "完成此設施可得獲以下回報",
            "{Name_{building}}安裝已過期", "{Name_{building}}完成使命",
            "發射所需的{Building_Effect_{launchpad}}！", "需要{Building_Effect_{launchpad}}才能發射！",
            "起動電源", "初始電力",
 
            // Training
            "基地科學獎勵", "基本科學回報",
            "基地資金獎勵", "基本資金回報",
            "基地支持獎勵", "基本支持回報",
            "基地聲望獎勵", "基本聲望回報",
            "基地有效載荷可靠性", "基本酬載可靠性",
            "基地發射可靠性", "基本發射可靠性",
            "你尚未選擇訓練簡介。此任務並無發放任何訓練獎金。", "尚未選擇任務訓練，本任務未能受益。",

            // Mission
            "完成獎金將在下項工作開始時發放", "完成後的額外回報會累加到下項工作",
            "在本回合中重置", "重置本回合的",
            "{agency}已完成{{mission}}（{rank}）", "{agency}已達成{{mission}}（{rank}）",

            // Other
            "備用電源發電機", "備用發電機",
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
            "表面棲息地", "地面居所",
            "變軌彈道", "轉移航道",
            "準備狀態", "技術指標",
            "中途操控", "中途軌道調整",
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
      } catch ( Exception x ) {
         Error( x );
         try { File.Delete( file ); } catch ( Exception ) { }
      } }
   }
}
