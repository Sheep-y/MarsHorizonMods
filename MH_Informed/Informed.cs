using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   public class Informed : MarsHorizonMod {
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
   }

   public class Config : IniConfig {
      [ Config( "Show base bonus on base screen, when not in build/edit/clear mode.  Default true." ) ]
      public bool show_base_bonus = true;

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200226;
   }

}