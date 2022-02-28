using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Informed";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.show_base_bonus )
            new PatcherBaseScreen().Apply();
         if ( config.launch_window_hint_before_ready > 0 && config.launch_window_hint_before_ready > 0 )
            new PatcherVehicleDesigner().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
   }

   public class Config : IniConfig {
      [ Config( "Show base bonus on base screen, when not in build/edit/clear mode.  Default true." ) ]
      public bool show_base_bonus = true;

      [ Config( "Show launch window on vehicle designer screen a fixed number of months before vehicle is ready.  Default 2.  0 to not show.  Max 6." ) ]
      public byte launch_window_hint_before_ready = 2;
      [ Config( "Show launch window on vehicle designer screen a fixed number of months after vehicle is ready.  Default 12.  0 to not show.  Max 36." ) ]
      public byte launch_window_hint_after_ready = 12;

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200226;

      public override void Load ( object subject, string path ) {
         base.Load( subject, path );
         if ( ! ( subject is Config conf ) ) return;
         conf.launch_window_hint_before_ready = Math.Min( conf.launch_window_hint_before_ready, (byte) 6 );
         conf.launch_window_hint_after_ready  = Math.Min( conf.launch_window_hint_after_ready, (byte) 36 );
      }
   }

}