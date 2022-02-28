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
      protected override string GetModName () => "DumpData";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.dump_building_link )
            new BuildingDumper().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
   }

   public class Config : IniConfig {
      [ Config( "Dump building associations to building_link.csv" ) ]
      public bool dump_building_link = true;

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200226;
   }

   internal class BuildingDumper : ModPatcher {
      internal void Apply () {
      }

      private static void DumpLinks () {
      }

      private static void DumpBuildings () {
      }
   }

}