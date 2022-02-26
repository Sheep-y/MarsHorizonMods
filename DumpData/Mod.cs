using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.DumpData {

   public class Mod : MarsHorizonMod {
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.dump_localization )
            new LocalizationDumper().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
   }

   public class Config : IniConfig {
      [ Config( "Dump localisation data to {language_code}_{hash}.csv" ) ]
      public bool dump_localization = true;

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200226;
   }

   internal class LocalizationDumper : ModPatcher {
      internal void Apply () {
         TryPatch( typeof( Localisation ), "ReadCSV", postfix: nameof( DumpLangCSV ) );
         TryPatch( typeof( Localisation ), "ReadCSVCO", postfix: nameof( DumpLangCSV ) );
      }

      private static void DumpLangCSV ( string csv ) {
         Fine( "Captured localisation data ({0} char), queuing background write.", csv.Length );
         Task.Run( () => { try {
            var file = Path.Combine( Mod.AppDataDir, "lang-" + csv.GetHashCode() + ".csv" );
            Info( "Writing {1} characters to {0}", file, csv.Length );
            File.WriteAllText( file, csv );
         } catch ( Exception x ) { Err( x ); } } );
      }
   }

}