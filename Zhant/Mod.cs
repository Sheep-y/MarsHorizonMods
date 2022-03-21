using Astronautica.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Zhant";
      public static void Main () => new Mod { shouldLogAssembly = false }.Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) => new PatcherL10N().Apply();
   }

   internal class PatcherL10N : Patcher {
      private static PatcherL10N me;

      internal void Apply () {
         me = this;
         TryPatch( typeof( UserSettings ), "SetLanguage", prefix: nameof( DynamicPatch ) );
      }

      private static ModPatch patchZh, patchFont;

      private static void DynamicPatch ( UserSettings.Language language ) { lock ( me ) { try {
         Info( "Locale is {0}", language );
         if ( language == UserSettings.Language.Chinese ) {
            if ( patchZh == null ) {
               patchZh = me.TryPatch( typeof( Localisation ).Method( "Interpolate", typeof( string ), typeof( Dictionary<string, string> ) ), prefix: nameof( ToZht ) );
               patchFont = me.TryPatch( typeof( UIStateController ), "SetViewState", postfix: nameof( ZhtFont ) );
            }
            LoadFonts();
         } else if ( patchZh != null ) {
            patchZh?.Unpatch();
            patchFont?.Unpatch();
            patchZh = patchFont = null;
         }
      } catch ( Exception x ) { Err( x ); } } }

      private static void LoadFonts () {
         if ( zhtTMPFs.Count != 0 ) return;
         foreach ( var v in new string[] { "Thin", "Light", "Regular", "Medium", "Bold", "Black" } )  {
            if ( ! LoadFont( $"NotoSansTC-{v}", v ) )
               LoadFont( $"NotoSansHK-{v}", v );
         }
         TMP_Settings.fallbackFontAssets.Insert( 0, zhtTMPFs[ "Medium" ] );
      }

      private static bool LoadFont ( string fn, string v ) { try {
         var f = Path.Combine( ModDir, $"{fn}.otf" );
         if ( File.Exists( f ) ) {
            Info( "Loading {0}", f );
            var tmpf = zhtTMPFs[ v ] = TMP_FontAsset.CreateFontAsset( new Font( f ), 24, 7, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA_HINTED, 8192, 8192 );
            tmpf.name = fn;
            LogFont( tmpf );
            return true;
         }
         Info( "Not Found: {0}.", f );
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static readonly Dictionary< string, string > zhs2zht = new Dictionary< string, string >();
      private static readonly Dictionary< string, TMP_FontAsset > zhtTMPFs = new Dictionary< string, TMP_FontAsset >();

      private static void ToZht ( ref string text ) { try {
         if ( string.IsNullOrEmpty( text ) ) return;
         if ( zhs2zht.TryGetValue( text, out string zht ) ) { text = zht; return; }
         else {
            zht = new string( ' ', text.Length );
            LCMapStringEx( "zh", LCMAP_TRADITIONAL_CHINESE, text, text.Length, zht, zht.Length, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero );
            zht = ZhtTweaks( zht );
         }
         Fine( "{0} => {1}", text, zht );
         zhs2zht.Add( text, text = zht );
      } catch ( Exception x ) { Err( x ); } }

      private static bool Co;

      private static void ZhtFont ( UIViewState state ) { try {
         Fine( "Finding font assets in view state" );
         foreach ( var entry in state.entries ) { // Is there a better way to find all TMP fonts?
            if ( entry?.@object == null ) continue;
            //foreach ( var text in entry.@object.GetComponentsInChildren< Text >( true ) ) Info( "> TXT", text.name ?? text.text, text.font );
            foreach ( var text in entry.@object.GetComponentsInChildren< TMP_Text >( true ) )
               FixFont( text );
         }
         if ( ! Co ) {
            Co = true;
            Controller.Instance.StartCoroutine( FixAllFonts() );
         }
      } catch ( Exception x ) { Err( x ); } }

      private static IEnumerator FixAllFonts () {
         while ( true ) {
            yield return new WaitForSecondsRealtime( 3 );
            foreach ( var root in SceneManager.GetActiveScene().GetRootGameObjects() )
               foreach ( var text in root.GetComponentsInChildren< TMP_Text >() )
                  FixFont( text );
         }
      }

      private static void FixFont ( TMP_Text text ) { try {
         var font = text?.font;
         if ( font == null || zhtTMPFs.ContainsValue( font ) ) return;
         var weight = FindFontWeight( font, out _ );
         if ( weight != null && zhtTMPFs.TryGetValue( weight, out var tc ) ) {
            Info( "Replacing {1} with {0}.", tc.name, font.name );
            text.font = tc;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void LogFont ( TMP_FontAsset fb ) {
         Info( "Atlas {0}x{1}, padding {2}, render {3}, pop {4}", fb.atlasWidth, fb.atlasHeight, fb.atlasPadding, fb.atlasRenderMode, fb.atlasPopulationMode );
      }

      private static string FindFontWeight ( TMP_FontAsset fontAsset, out int i ) { i = 0; try {
         var list = fontAsset?.fallbackFontAssetTable;
         if ( list == null || list.Count == 0 ) return null;
         for ( ; i < list.Count ; i++ ) { var fb = list[ i ];
            var fname = fb?.name;
            if ( fname?.StartsWith( "NotoSans" ) != true ) continue;
            if ( fname.StartsWith( "NotoSansHK-" ) || fname.StartsWith( "NotoSansTC-" ) ) return null;
            if ( fname.StartsWith( "NotoSansCJKsc-" ) ) {
               LogFont( fb );
               return fb.name.Substring( "NotoSansCJKsc-".Length ).Split( ' ' )[0];
            }
         }
         return null;
      } catch ( Exception x ) { return Err< string >( x, null ); } }

      private static readonly string[] tweaks = new string[]{
         "游戲", "遊戲",

         "適用獎勵", "獎勵",
         "適用期限已至", "期限已至",
         "所有適用的研究", "的所有研究",
         "不適用", "不可使用",
         "適用於", "可以用於",
         "適用", "可以使用",
         "没有任何有效任務", "没有任何進行中的任務",
         "有效任務", "任務列表",

         "表面棲息地", "地面居所",
         "變軌彈道", "轉移航道",
         "準備狀態", "技術指標",
         "中途操控", "中途軌道調整",

         "最后", "最後",
         "并", "並",
         "进", "進",
         "于", "於",
         "剩余", "剩餘",
         "加載", "載入",
         "有效載荷", "酬載",
         "正在登錄", "載入", // ^
         "菜單", "選單",
         "采集", "採集",
         "采樣", "採樣",
         "阿麗亞娜", "亞利安",
         "大力神", "泰坦",
         "旅行者號", "航行者號",
      };

      private static string ZhtTweaks ( string txt ) {
         for ( var i = 0 ; i < tweaks.Length ; i += 2 )
            //if ( txt.Contains( tweaks[ i ][ 0 ] ) )
              txt = txt.Replace( tweaks[ i ], tweaks[ i + 1 ] );
         switch ( txt ) {
            case "跳過當前月" : return "下一月";
            case "跳到事件" : return "下一事件";
            case "簡體中文" : return "中文";
            case "在 Discord 上<br>加入我們！" : return "加入我們的<br>Discord！";
            case "{Name_{buildingId}}待完成" : return "{Name_{buildingId}}完成";
            case "{agency}已完成{{mission}}的{phase}階段" : return "{agency}已完成{{mission}}的階段{phase}";
            case "新的聯合任務適用於{Name_Body_{body}} ！" : return "有新的{Name_Body_{body}}聯合任務";
            case "新的請求任務適用於{Name_Body_{body}} ！" : return "有新的{Name_Body_{body}}請求任務";
            case "{agency}下個月將會推出{{mission}}！" : return "{agency}將於下個月發射{{mission}}！";
            case "{{mission}}預備下一階段！" : return "{{mission}}的下一階段已就緒！";
            case "{Name_{buildingId}}已完成" : return "{Name_{buildingId}}建造完畢";
            case "{payload}已完成" : return "{payload}建造完畢";
            case "{vehicle}已完成" : return "{vehicle}建造完畢";
         }
         return txt;
      }

      [ DllImport( "kernel32", CharSet = CharSet.Unicode, SetLastError = true ) ]
      private static extern int LCMapStringEx ( string lpLocaleName, uint dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest, IntPtr lpVersionInformation, IntPtr lpReserved, IntPtr sortHandle );
      private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
      private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
   }
}