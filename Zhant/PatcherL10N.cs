using Astronautica.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ZyMod.MarsHorizon.Zhant {
   internal class PatcherL10N : ModPatcher {
      private static PatcherL10N me;

      internal override void Apply () {
         me = this;
         if ( config.dynamic_patch ) {
            Patch( typeof( UserSettings ), "SetLanguage", prefix: nameof( DynamicPatch ) );
            if ( Controller.Instance?.settings?.general?.language == UserSettings.Language.Chinese )
               DynamicPatch( UserSettings.Language.Chinese );
         } else
            ApplyZhPatches();
      }

      internal override void UnpatchAll () {
         base.UnpatchAll();
         patchZh = patchFont = null;
      }

      internal override void Unload () {
         zhs2zht.Clear();
         zhtTMPFs.Clear();
         fixedTMPFs.Clear();
         lastTMPF = null;
         Transdict.whole.Clear();
         Transdict.part = null;
      }

      private static ModPatch patchZh, patchFont;
      private const string SC_prefix = "NotoSansCJKsc-";
      private static readonly string[] fallback_fonts = new string[]{ "TW-Sung.ttf", "HanaMinA.ttf", "HanaMinB.ttf", "CODE2000.TTF", "unifont.ttf" };
      private static readonly string[] variations = new string[] { "Thin", "Light", "Regular", "Medium", "Bold", "Black" };

      private static void DynamicPatch ( UserSettings.Language language ) { lock ( me ) { try {
         Info( "Locale is {0}", language );
         if ( language == UserSettings.Language.Chinese ) {
            ApplyZhPatches();
         } else if ( patchZh != null ) {
            patchZh?.Unpatch();
            patchFont?.Unpatch();
            patchZh = patchFont = null;
         }
      } catch ( Exception x ) { Err( x ); } } }

      private static void ApplyZhPatches () {
         if ( patchZh == null )
            patchZh = me.Patch( typeof( Localisation ).Method( "Interpolate", typeof( string ), typeof( Dictionary<string, string> ) ), prefix: nameof( ToZht ) );
         if ( patchFont == null )
            patchFont = me.Patch( typeof( UIStateController ), "SetViewState", postfix: nameof( SetZhtFont ) );
         Transdict.LoadDicts();
         LoadFonts();
      }

      private static void LoadFonts () {
         if ( zhtTMPFs.Count != 0 ) return;
         foreach ( var v in variations ) {
            if ( LoadFont( $"NotoSansCJKtc-{v}", v ) || LoadFont( $"NotoSansCJKhk-{v}", v )
                 || LoadFont( $"NotoSansTC-{v}", v ) || LoadFont( $"NotoSansHK-{v}", v ) );
         }
         var fbList = TMP_Settings.fallbackFontAssets;
         if ( zhtTMPFs.Count != 0 ) {
            var weight = FindFontWeight( fbList, out var i ) ?? "Medium";
            if ( ! zhtTMPFs.TryGetValue( weight, out var tc ) ) tc = zhtTMPFs.First().Value;
            AddToFallback( tc, fbList, "global fallback" );
         }
         var has_fallback = false;
         foreach ( var file in fallback_fonts ) {
            if ( ! LoadFont( file, file ) ) continue;
            has_fallback = true;
            AddToFallback( zhtTMPFs[ file ], fbList, "global fallback" );
         }
         if ( ! has_fallback ) Info( "Fallback font(s) not found", fallback_fonts );
      }

      private static bool LoadFont ( string fn, string v ) { try {
         if ( zhtTMPFs.ContainsKey( v ) ) return true;
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
         if ( font == null ) return;
         if ( fb?.Any( e => e.name == font.name ) != false ) {
            Info( "{1} is null or already contains {0}", fname, font.name );
            return;
         }
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
         zht = ZhtTweaks( ref raw );
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
         string result = null;
         for ( i = 0 ; i < list.Count ; i++ ) { var fb = list[ i ];
            var fname = fb?.name;
            if ( fname?.StartsWith( "NotoSans" ) != true ) continue;
            if ( IsTC( fname ) ) return null;
            if ( fname.StartsWith( SC_prefix ) )
               result = fb.name.Substring( SC_prefix.Length ).Split( ' ' )[0];
         }
         return result;
      } catch ( Exception x ) { return Err< string >( x, null ); } }

      private static bool IsTC ( string name )
         => name.StartsWith( "NotoSansCJKtc-" ) || name.StartsWith( "NotoSansCJKhk-" ) || name.StartsWith( "NotoSansTC-" ) || name.StartsWith( "NotoSansHK-" );

      private static readonly char[] charMap = new char[] {
         '后','後',  '并','並',  '进','進',  '于','於',  '愿','願',  '筑','築',  '范','範',  '涂','塗',  '余','餘',
      };

      private static string ZhtTweaks ( ref string txt ) {
         var buf = new StringBuilder( txt );
         for ( var i = 0 ; i < charMap.Length ; i += 2 )
            buf.Replace( charMap[ i ], charMap[ i + 1 ] );
         txt = buf.ToString();
         if ( Transdict.whole.TryGetValue( buf.ToString(), out var zht ) ) return zht;
         var map = Transdict.part;
         for ( var i = 0 ; i < map.Length ; i += 2 )
            buf.Replace( map[ i ], map[ i + 1 ] );
         return buf.ToString();
      }

      [ DllImport( "kernel32", CharSet = CharSet.Unicode, SetLastError = true ) ]
      private static extern int LCMapStringEx ( string lpLocaleName, uint dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest, IntPtr lpVersionInformation, IntPtr lpReserved, IntPtr sortHandle );
      private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
      private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
   }
}