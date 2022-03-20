﻿using Astronautica.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Zhant";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         new PatcherL10N().Apply( game );
      }
   }

   internal class PatcherL10N : Patcher {
      private static PatcherL10N me;

      internal void Apply ( Assembly game ) {
         me = this;
         TryPatch( typeof( Controller ), "OnSystemLanguageChanged", postfix: nameof( DynamicPatch ) );
         TryPatch( typeof( UserSettings ), "SetLanguage", postfix: nameof( DynamicPatch ) );
         me.TryPatch( typeof( MaterialReferenceManager ), "AddFontAssetInternal", postfix: nameof( LogFont ) );
      }

      private static ModPatch patchZh, patchFont;

      private static void DynamicPatch () { lock ( me ) { try {
         var locale = ScriptableObjectSingleton<Localisation>.instance.locale;
         Info( "Locale is {0}", locale );
         if ( locale == "zh_cn" ) {
            if ( patchZh == null ) {
               patchZh = me.TryPatch( typeof( Localisation ).Method( "Interpolate", typeof( string ), typeof( Dictionary<string, string> ) ), prefix: nameof( ToZht ) );
               patchFont = me.TryPatch( typeof( UIStateController ), "SetViewState", postfix: nameof( ZhtFont ) );
               foreach ( var f in allTMPFs ) FixFont( f );
            }
            LoadFonts();
         } else if ( patchZh != null ) {
            patchZh?.Unpatch();
            patchFont?.Unpatch();
            patchZh = patchFont = null;
         }
      } catch ( Exception x ) { Err( x ); } } }

      private static void LoadFonts () {
         if ( zhs2zht.Count != 0 || zhtTMPFs.Count != 0 ) return;
         foreach ( var v in new string[] { "Regular", "Medium", "Bold" } ) try {
            string fn = $"NotoSansHK-{v}", f = Path.Combine( ModDir, $"{fn}.otf" );
            if ( File.Exists( f ) ) {
               Info( "Loading {0}", f );
               zhtTMPFs[ v ] = TMP_FontAsset.CreateFontAsset( new Font( f ) );
               zhtTMPFs[ v ].name = fn;
            } else
               Warn( "Font {0} not found.", f );
         } catch ( Exception x ) { Err( x ); }
         TMP_Settings.fallbackFontAssets.Add( zhtTMPFs[ "Medium" ] );
      }

      private static readonly Dictionary< string, string > zhs2zht = new Dictionary< string, string >();
      private static readonly Dictionary< string, TMP_FontAsset > zhtTMPFs = new Dictionary< string, TMP_FontAsset >();
      private static readonly HashSet< TMP_FontAsset > fixedTMPFs = new HashSet< TMP_FontAsset >();
      private static readonly HashSet< TMP_FontAsset > allTMPFs = new HashSet< TMP_FontAsset >();
      private static TMP_FontAsset lastTMPF;

      private static void ToZht ( ref string text ) { try {
         if ( string.IsNullOrEmpty( text ) ) return;
         if ( zhs2zht.TryGetValue( text, out string zht ) ) { text = zht; return; }
         else {
            zht = new string( ' ', text.Length );
            LCMapString( LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, text, text.Length, zht, zht.Length );
            zht = ZhtTweaks( zht );
         }
         Fine( "{0} => {1}", text, zht );
         zhs2zht.Add( text, text = zht );
      } catch ( Exception x ) { Err( x ); } }

      private static void LogFont ( TMP_FontAsset fontAsset ) { try {
         if ( allTMPFs.Contains( fontAsset ) ) return;
         Info( "Catch {0}", fontAsset.name );
         allTMPFs.Add( fontAsset );
         if ( zhtTMPFs.Count > 0 ) FixFont( fontAsset );
      } catch ( Exception x ) { Err( x ); } }

      private static void ZhtFont ( UIViewState state ) { try {
         Fine( "Finding font assets in view state" );
         foreach ( var entry in state.entries ) { // Is there a better way to find all TMP fonts?
            if ( entry?.@object == null ) continue;
            //foreach ( var text in entry.@object.GetComponentsInChildren< Text >( true ) ) Fine( "> TXT", text.name ?? text.text, text.font );
            foreach ( var text in entry.@object.GetComponentsInChildren< TMP_Text >( true ) ) FixFont( text.font );
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void FixFont ( TMP_FontAsset fontAsset ) { try {
         if ( fontAsset == lastTMPF || fixedTMPFs.Contains( fontAsset ) ) return;
         fixedTMPFs.Add( lastTMPF = fontAsset );
         foreach ( var fb in fontAsset.fallbackFontAssetTable )
            if ( fb.name.StartsWith( "NotoSansCJKsc-" ) ) {
               var variation = fb.name.Substring( "NotoSansCJKsc-".Length ).Split( ' ' )[0];
               if ( zhtTMPFs.TryGetValue( variation, out var tc ) ) {
                  Info( "Adding {0} as fallback of {1}.", tc.name, fb.name );
                  fontAsset.fallbackFontAssetTable.Add( tc );
                  break;
               } else
                  Warn( "Cannot find font variation {0} from {1}.", variation, fb.name );
            }
      } catch ( Exception x ) { Err( x ); } }

      private static readonly string[] tweaks = new string[]{
         "游戲", "遊戲",

         "最后", "最後",
         "并", "並",
         "进", "進",
         "于", "於",
         "剩余", "剩餘",
         "加載遊戲", "載入遊戲",
         "有效載荷", "酬載",
         "正在登錄", "載入",
         "菜單", "選單",
         "采集", "採集",
         "采樣", "採樣",
         "阿麗亞娜", "亞利安",
         "大力神", "泰坦",
      };

      private static string ZhtTweaks ( string txt ) {
         switch ( txt ) {
            case "跳過當前月" : return "下一月";
            case "跳到事件" : return "下一事件";
            case "簡體中文" : return "中文";
         }
         for ( var i = 0 ; i < tweaks.Length ; i += 2 )
            //if ( txt.Contains( tweaks[ i ][ 0 ] ) )
              txt = txt.Replace( tweaks[ i ], tweaks[ i + 1 ] );
         return txt;
      }

      [ DllImport( "kernel32", CharSet = CharSet.Unicode, SetLastError = true ) ]
      private static extern int LCMapString ( int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest );
      private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
      private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
      private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
   }
}