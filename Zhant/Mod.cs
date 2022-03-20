using Astronautica.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Zhant";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         new PatcherL10N().Apply();
      }
   }

   internal class PatcherL10N : Patcher {
      private static PatcherL10N me;

      internal void Apply () {
         me = this;
         TryPatch( typeof( Controller ), "OnSystemLanguageChanged", postfix: nameof( DynamicPatch ) );
         TryPatch( typeof( UserSettings ), "SetLanguage", postfix: nameof( DynamicPatch ) );
      }

      private static ModPatch patch;

      private static void DynamicPatch () { lock ( me ) {
         var locale = ScriptableObjectSingleton<Localisation>.instance.locale;
         Info( "Locale is {0}", locale );
         if ( locale == "zh_cn" ) {
            if ( patch == null )
               patch = me.TryPatch( typeof( Localisation ).Method( "Interpolate", typeof( string ), typeof( Dictionary< string, string > ) ), postfix: nameof( ToZht ) );
         } else if ( patch != null ) {
            patch.Unpatch();
            patch = null;
         }
      } }

      private static readonly Dictionary< string, string > zhs2zht = new Dictionary<string, string>();
      
      private static void ToZht ( ref string text ) { try {
         if ( string.IsNullOrEmpty( text ) ) return;
         if ( zhs2zht.TryGetValue( text, out string zht ) ) { text = zht; return; }
         if ( text == "简体中文" ) zht = "中文";
         else {
            zht = new string( ' ', text.Length );
            LCMapString( LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, text, text.Length, zht, zht.Length );
            zht = ZhtTweaks( zht );
         }
         Fine( "{0} => {1} ({2} chars)", text, zht, zht.Length );
         zhs2zht.Add( text, text = zht );
      } catch ( Exception x ) { Err( x ); } }
      private static readonly string[] tweaks = new string[]{
         "游戲", "遊戲",

         "于", "於",
         "剩余", "剩餘",
         "加載遊戲", "載入遊戲",
         "有效載荷", "酬載",
         "菜單", "選單",
         "采集", "采集",
      };

      private static string ZhtTweaks ( string txt ) {
         switch ( txt ) {
            case "跳過當前月" : return "下一月";
            case "跳到事件" : return "下一事件";
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