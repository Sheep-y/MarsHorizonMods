using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ZyMod.MarsHorizon.PayloadQA {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "PayloadQA";
      public static void Main () => new Mod().Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.special_payload_ar_bonus > 0 || config.extra_crew_ar_bonus > 0 || config.power_payload_ar_crit > 0 || config.standalone_resolve_rng )
            ActivatePatcher( typeof( PatcherAutoResolve ) );
         if ( config.minigame_base_crit >= 0 || config.minigame_porportion_crit > 0 )
            ActivatePatcher( typeof( PatcherMinigame ) );
      }
   }

   internal abstract class ModPatcher : MarsHorizonPatcher {
      internal static readonly Config config = new Config();
   }

   internal class Config : IniConfig {
      [ Config( "\r\n[Auto-Resolve]" ) ]
      [ Config( "Auto-resolve success chance bonus provided by specialized payload (nav, comm etc).  Set to 0 to disable." ) ]
      public byte special_payload_ar_bonus = 10;
      [ Config( "Auto-resolve success chance bonus provided by each extra crew member.  Set to 0 to disable." ) ]
      public byte extra_crew_ar_bonus = 10;
      [ Config( "Auto-resolve outstanding chance bonus provided by power variant payload.  Set to 0 to disable." ) ]
      public byte power_payload_ar_crit = 10;
      [ Config( "Use a standalone random number generator to auto resolve.  Default True." ) ]
      public bool standalone_resolve_rng = true;

      [ Config( "\r\n[Mini-game]" ) ]
      [ Config( "Base critical success chance.  Game default at 0.1 for 10%.  Set to -1 to not change." ) ]
      public float minigame_base_crit = 0;
      [ Config( "Crit chance as proportional to payload reliability.  Mod default 0.2 for 20%, e.g. 50% payload reliability = 10% crit." ) ]
      public float minigame_porportion_crit = 0.2f;

      [ Config( "\r\n" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200320;
   }
}