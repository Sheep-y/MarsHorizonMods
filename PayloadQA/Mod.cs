using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.PayloadQA {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "PayloadQA";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         new PatcherAutoResolve().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static readonly Config config = new Config();
   }

   public class Config : IniConfig {
      [ Config( "\r\n[Auto-Resolve]" ) ]
      [ Config( "Auto-resolve bonus success chance provided by specialized payload (nav, comm etc).  Set to 0 to disable." ) ]
      public byte payload_specialise_auto_bonus = 10;
      [ Config( "Auto-resolve bonus success chance provided by power variant payload.  Set to 0 to disable." ) ]
      public byte payload_power_auto_bonus = 0;
      [ Config( "Auto-resolve bonus crit success chance provided by power variant payload.  Set to 0 to disable." ) ]
      public byte payload_power_auto_bonus_crit = 10;
      [ Config( "Use a standalone random number generator to decide auto resolve.  Default True." ) ]
      public bool standalone_resolve_rng = true;

      [ Config( "\r\n" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200304;
   }
}