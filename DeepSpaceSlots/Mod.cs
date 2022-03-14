using System;
using System.Reflection;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.DeepSpaceSlots {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "DeepSpaceSlots";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         new PatcherSlot().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
   }

   public class Config : IniConfig {
      [ Config( "[Built-In]" ) ]
      [ Config( "How many deep space mission slot does Deep Space Network provide.  Default 1." ) ]
      public byte deep_space_network_slot = 1;

      [ Config( "How many deep space mission slot does Space Library provide.  Default 1." ) ]
      public byte space_library_slot = 1;

      [ Config( "How many deep space mission slot does each Missoin Control Expansion provide.  Default 0.  Can be fraction, e.g. 0.34 for 1 slot per 3 expansions." ) ]
      public float mission_control_ext_slot = 0;

      [ Config( "How many deep space mission slot does completing Grand Tour phase 2 provide.  Default 1." ) ]
      public byte grand_tour_phase2_slot = 1;

      [ Config( "[Custom]" ) ]
      [ Config( "Custom buliding to provide deep space mission slots." ) ]
      public string custom_building1_id = "";
      [ Config( "How many deep space mission slot does the custom building provide." ) ]
      public byte custom_building1_slot = 1;

      [ Config( "Custom buliding to provide deep space mission slots." ) ]
      public string custom_building2_id = "";
      [ Config( "How many deep space mission slot does the custom building provide." ) ]
      public byte custom_building2_slot = 1;

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200314;
   }
}