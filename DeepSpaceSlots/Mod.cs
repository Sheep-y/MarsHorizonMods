using Astronautica.View;
using BepInEx;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace ZyMod.MarsHorizon.DeepSpaceSlots {
   [ BepInPlugin( "Zy.MarsHorizon.DeepSpaceSlots", "Deep Space Slots", "0.0.2022.0326" ) ]
   public class BIE_Mod : BaseUnityPlugin {
      private void Awake() { BepInUtil.Setup( this, ModPatcher.config ); Mod.Main(); }
      private void OnDestroy() => BepInUtil.Unbind();
   }

   [ EnableReloading ] public static class UMM_Mod {
      public static void Load ( UnityModManager.ModEntry entry ) => UMMUtil.Init( entry, typeof( Mod ) );
   }

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "DeepSpaceSlots";
      public static void Main () => new Mod().Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         ModPatcher.config.Load();
         ActivatePatcher( typeof( PatcherSlot ) );
      }
   }

   internal abstract class ModPatcher : MarsHorizonPatcher {
      internal static readonly Config config = new Config();
      internal static Simulation simulation => Controller.Instance?.activeClient.simulation;
   }

   internal class Config : IniConfig {
      [ Config( "[Qualification]" ) ]
      [ Config( "As the number of mission destination may increase in the future, " ) ]
      [ Config( "Min. total duration (months) for a mission to be qualifed as deep space." ) ]
      public byte deep_space_require_duration = 30;
      [ Config( "Min. total phases for a mission to be qualified as deep space." ) ]
      public byte deep_space_require_phase = 3;
      [ Config( "Exclude crewed missions from deep space slot.  Default False." ) ]
      public bool deep_space_require_crewless = false;
      [ Config( "Min. phase for a mission to be trasferred to deep space slot." ) ]
      public byte deep_space_min_phase = 2;
      [ Config( "Min. months for a mission to be trasferred to deep space slot." ) ]
      public byte deep_space_min_turn = 6;

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
      [ Config( "Custom buliding id to provide deep space mission slots." ) ]
      public string custom_building1_id = "";
      [ Config( "How many deep space mission slot does the custom building provide." ) ]
      public byte custom_building1_slot = 1;
      [ Config( "Custom buliding id to provide deep space mission slots." ) ]
      public string custom_building2_id = "";
      [ Config( "How many deep space mission slot does the custom building provide." ) ]
      public byte custom_building2_slot = 1;

      [ Config( "Custom mission id to provide deep space mission slots.  Can be same with custom mission 2 or grand tour." ) ]
      public string custom_mission1_id = "";
      [ Config( "Which phase does the mission needs to clear to provide the slot." ) ]
      public byte custom_mission1_phase = 2;
      [ Config( "How many deep space mission slot does the custom building provide." ) ]
      public byte custom_mission1_slot = 1;
      [ Config( "Custom mission id to provide deep space mission slots.  Can be same with custom mission 1 or grand tour." ) ]
      public string custom_mission2_id = "";
      [ Config( "Which phase does the mission needs to clear to provide the slot." ) ]
      public byte custom_mission2_phase = 2;
      [ Config( "How many deep space mission slot does the custom building provide." ) ]
      public byte custom_mission2_slot = 1;

      [ Config( "Custom research id to provide deep space mission slots." ) ]
      public string custom_tech1_id = "";
      [ Config( "How many deep space mission slot does the custom research provide." ) ]
      public byte custom_tech1_slot = 1;
      [ Config( "Custom research id to provide deep space mission slots." ) ]
      public string custom_tech2_id = "";
      [ Config( "How many deep space mission slot does the custom research provide." ) ]
      public byte custom_tech2_slot = 1;

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200314;
   }
}