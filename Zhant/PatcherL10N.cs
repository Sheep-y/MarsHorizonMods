using Astronautica.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {
   internal class PatcherL10N : ModPatcher {
      private static PatcherL10N me;

      internal void Apply () {
         me = this;
         if ( config.dynamic_patch )
            TryPatch( typeof( UserSettings ), "SetLanguage", prefix: nameof( DynamicPatch ) );
         else
            ApplyZhPatches();
      }

      private static ModPatch patchZh, patchFont;
      private const string SC_prefix = "NotoSansCJKsc-";
      private static readonly string[] fallback_fonts = new string[]{ "TW-Sung.ttf", "HanaMinA.ttf", "HanaMinB.ttf", "CODE2000.TTF", "unifont.ttf" };
      private static readonly string[] variations = new string[] { "Thin", "Light", "Regular", "Medium", "Bold", "Black" };

      private static void DynamicPatch ( UserSettings.Language language ) { lock ( me ) { try {
         Info( "Locale is {0}", language );
         if ( language == UserSettings.Language.Chinese ) {
            if ( patchZh == null ) ApplyZhPatches();
         } else if ( patchZh != null ) {
            patchZh?.Unpatch();
            patchFont?.Unpatch();
            patchZh = patchFont = null;
         }
      } catch ( Exception x ) { Err( x ); } } }

      private static void ApplyZhPatches () {
         patchZh = me.TryPatch( typeof( Localisation ).Method( "Interpolate", typeof( string ), typeof( Dictionary<string, string> ) ), prefix: nameof( ToZht ) );
         patchFont = me.TryPatch( typeof( UIStateController ), "SetViewState", postfix: nameof( SetZhtFont ) );
         Transdict.LoadDicts();
         LoadFonts();
      }

      private static void LoadFonts () {
         if ( zhtTMPFs.Count != 0 ) return;
         foreach ( var v in variations ) {
            if ( LoadFont( $"NotoSansCJKtc-{v}", v ) || LoadFont( $"NotoSansCJKhk-{v}", v )
                 || LoadFont( $"NotoSansTC-{v}", v ) || LoadFont( $"NotoSansHK-{v}", v ) );
         }
         if ( zhtTMPFs.Count == 0 ) return;
         var weight = FindFontWeight( TMP_Settings.fallbackFontAssets, out var i ) ?? "Medium";
         if ( ! zhtTMPFs.TryGetValue( weight, out var tc ) ) tc = zhtTMPFs.First().Value;
         AddToFallback( tc, TMP_Settings.fallbackFontAssets, "global fallback" );
         var has_fallback = false;
         foreach ( var file in fallback_fonts ) {
            if ( ! LoadFont( file, file ) ) continue;
            has_fallback = true;
            AddToFallback( zhtTMPFs[ file ], TMP_Settings.fallbackFontAssets, "global fallback" );
         }
         if ( ! has_fallback ) Info( "Fallback font(s) not found", fallback_fonts );
      }

      private static bool LoadFont ( string fn, string v ) { try {
         var f = Path.Combine( ModDir, fn.EndsWith( ".ttf" ) ? fn : $"{fn}.otf" );
         if ( File.Exists( f ) ) {
            var size = (int) ( Array.IndexOf( variations, v ) >= 0 && v != "Medium" && v != "Regular" ? config.sample_size_other : config.sample_size_normal );
            var padding = (int) Math.Ceiling( size * config.padding_ratio );
            Info( "Loading {0}, size {1}, padding {2}.", f, size, padding );
            var tmpf = zhtTMPFs[ v ] = TMP_FontAsset.CreateFontAsset( new Font( f ), size, padding, GlyphRenderMode.SDFAA_HINTED, (int) config.atlas_width, (int) config.atlas_height );
            tmpf.name = fn;
            return true;
         }
         Fine( "Not Found: {0}.", f );
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static void AddToFallback ( TMP_FontAsset font, List<TMP_FontAsset> fb, string fname ) {
         if ( font == null || fb == null ) return;
         Info( "Inserting {0} at end of {1}.", font.name, fname );
         fb.Add( font );
      }

      private static readonly Dictionary< string, string > zhs2zht = new Dictionary< string, string >();
      private static readonly Dictionary< string, TMP_FontAsset > zhtTMPFs = new Dictionary< string, TMP_FontAsset >();
      private static readonly HashSet< TMP_FontAsset > fixedTMPFs = new HashSet< TMP_FontAsset >();
      private static TMP_FontAsset lastTMPF;

      private static void ToZht ( ref string text ) { try {
         if ( string.IsNullOrEmpty( text ) ) return;
         if ( zhs2zht.TryGetValue( text, out string zht ) ) { text = zht; return; }
         var raw = new string( ' ', text.Length );
         LCMapStringEx( "zh", LCMAP_TRADITIONAL_CHINESE, text, text.Length, raw, raw.Length, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero );
         zht = ZhtTweaks( raw );
         Fine( "{3} {0} => {1} => {2}", text, raw, raw == zht ? null : zht, text.Length );
         zhs2zht.Add( text, text = zht );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetZhtFont ( UIViewState state ) { try {
         Fine( "Finding font assets in view state" );
         foreach ( var entry in state.entries ) { // Is there a better way to find all TMP fonts?
            if ( entry?.@object == null ) continue;
            foreach ( var text in entry.@object.GetComponentsInChildren< TMP_Text >( true ) )
               if ( text.font != null && text.font != lastTMPF )
                  FixFont( lastTMPF = text.font );
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void FixFont ( TMP_FontAsset font ) { try {
         if ( font == null || fixedTMPFs.Contains( font ) ) return;
         fixedTMPFs.Add( font );
         var weight = FindFontWeight( font.fallbackFontAssetTable, out var i );
         if ( weight != null ) {
            if ( ! zhtTMPFs.TryGetValue( weight, out var tc ) ) {
               if ( ! zhtTMPFs.TryGetValue( "Medium", out tc ) ) {
                  Warn( "Font variation {0} not loaded.  TC fallback not added to {1}", weight, font.name );
                  return;
               } else
                  Warn( "Font variation {0} not loaded, replacing with Medium.", weight );
            }
            //font.fallbackFontAssetTable[ i ] = tc;
            AddToFallback( tc, font.fallbackFontAssetTable, font.name );
         }
      } catch ( Exception x ) { Err( x ); } }

      private static string FindFontWeight ( List< TMP_FontAsset > list, out int i ) { i = -1; try {
         if ( list == null || list.Count == 0 ) return null;
         for ( i = 0 ; i < list.Count ; i++ ) { var fb = list[ i ];
            var fname = fb?.name;
            if ( fname?.StartsWith( "NotoSans" ) != true ) continue;
            if ( fname.StartsWith( "NotoSansHK-" ) || fname.StartsWith( "NotoSansTC-" ) ) return null;
            if ( fname.StartsWith( SC_prefix ) )
               return fb.name.Substring( SC_prefix.Length ).Split( ' ' )[0];
         }
         return null;
      } catch ( Exception x ) { return Err< string >( x, null ); } }

      private static string ZhtTweaks ( string txt ) {
         if ( Transdict.whole.TryGetValue( txt, out var zht ) ) return zht;
         var map = Transdict.part;
         for ( var i = 0 ; i < map.Length ; i += 2 )
           txt = txt.Replace( map[ i ], map[ i + 1 ] );
         return txt;
      }

      [ DllImport( "kernel32", CharSet = CharSet.Unicode, SetLastError = true ) ]
      private static extern int LCMapStringEx ( string lpLocaleName, uint dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest, IntPtr lpVersionInformation, IntPtr lpReserved, IntPtr sortHandle );
      private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
      private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
   }
}