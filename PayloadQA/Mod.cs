using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.PayloadQA {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "PayloadQA";
      public static void Main () => new Mod { shouldLogAssembly = false }.Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.payload_specialise_auto_bonus > 0 || config.payload_extra_crew_auto_bonus > 0 || config.payload_power_auto_bonus > 0 || config.payload_power_auto_crit_bonus > 0 || config.standalone_resolve_rng )
            new PatcherAutoResolve().Apply();
         if ( config.minigame_base_crit >= 0 || config.minigame_porportion_crit > 0 )
            new PatcherMinigame().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static readonly Config config = new Config();
   }

   internal class Config : IniConfig {
      [ Config( "\r\n[Auto-Resolve]" ) ]
      [ Config( "Auto-resolve success chance bonus provided by specialized payload (nav, comm etc).  Set to 0 to disable." ) ]
      public byte payload_specialise_auto_bonus = 10;
      [ Config( "Auto-resolve success chance bonus provided by each extra crew member.  Set to 0 to disable." ) ]
      public byte payload_extra_crew_auto_bonus = 10;
      [ Config( "Auto-resolve success chance bonus provided by power variant payload.  Set to 0 to disable." ) ]
      public byte payload_power_auto_bonus = 0;
      [ Config( "Auto-resolve outstanding chance bonus provided by power variant payload.  Set to 0 to disable." ) ]
      public byte payload_power_auto_crit_bonus = 10;
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