using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {
   internal static class Transdict {
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
            "跳過當前月",  "下一月",
            "跳到事件",  "下一事件",
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
            "上下文",  "過去、現在、未來",
            "雙薪",  "心靈手巧",
            "已公開",  "公眾關注",
            "點擊跳過",  "任意鍵跳過",
            "點擊忽略",  "任意鍵關閉",

            // Agent traits
            "維修費用",  "恒常開支",
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
            "并非因為輕而易舉",  "知難而行",
            "薪資適中的宇航員",  "廉價宇航員",
            "發射水平 I",  "發射能手 I",
            "發射水平 II",  "發射能手 II",
            "原型機有效載荷 I",  "實驗性酬載 I",
            "原型機有效載荷 II",  "實驗性酬載 II",
            "揮發性助推器 I",  "易爆燃料 I",
            "揮發性助推器 II",  "易爆燃料 II",
            "失敗的任務支持懲罰加倍",  "任務失敗的支持度懲罰翻倍",
         };
         for ( var i = 0 ; i < list.Length ; i += 2 )
            whole.Add( list[ i ], list[ i + 1 ] );
         WriteCsv( WholeCsv, "全文", list );
      }

      internal static void LoadPartDefaults () {
         Info( "Loading default term mappings." );
         part = new string[] {
            "游戲", "遊戲",
            "游客", "遊客",

            "獎金", "額外",
            "適用獎勵", "獎勵",
            "適用期限已至", "期限已至",
            "所有適用的研究", "的所有研究",
            "不適用", "不可使用",
            "適用於", "可以用於",
            "適用", "可以使用",
            "獎勵", "回報",
            "完成回報", "完成獎勵",
            "額外：", "額外獎勵：",

            "没有任何有效任務", "没有任何進行中的任務",
            "有效任務", "任務列表",
            "表面棲息地", "地面居所",
            "變軌彈道", "轉移航道",
            "準備狀態", "技術指標",
            "中途操控", "中途軌道調整",
            "上下文動作", "情景動作",
            "試驗飛行員", "試飛員",
            "航天地面指揮中心", "任務控制中心",
            "模塊空間站的增加，", "模塊空間站的擴充，",

            "后", "後",
            "并", "並",
            "进", "進",
            "于", "於",
            "愿", "願",
            "筑", "築",
            "丟失", "失去",
            "剩余", "剩餘",
            "加載", "載入",
            "明了", "明瞭",
            "菜單", "選單",
            "采集", "採集",
            "采樣", "採樣",
            "采用", "採用",
            "有效載荷", "酬載",
            "正在登錄", "載入",
            "阿麗亞娜", "亞利安",
            "你是否確定", "是否",
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
