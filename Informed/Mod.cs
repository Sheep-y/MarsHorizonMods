using Astronautica.View;
using BepInEx;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace ZyMod.MarsHorizon.Informed {
   [ BepInPlugin( "Zy.MarsHorizon.Informed", "Informed", "1.0.2" ) ]
   internal class BIE_Mod : BaseUnityPlugin {
      private void Awake() { BepInUtil.Setup( this, ModPatcher.config ); Mod.Main(); }
      private void OnDestroy() => BepInUtil.Unbind();
   }

   internal static class UMM_Mod {
      public static void Load ( object entry ) => UMMUtil.Init( entry, typeof( Mod ) );
   }

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Informed";
      public static void Main () => new Mod().Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.show_base_bonus )
            ActivatePatcher( typeof( PatcherBaseScreen ) );
         if ( config.show_planet_launch_window || config.show_mission_expiry || config.show_mission_payload )
            ActivatePatcher( typeof( PatcherMissionPlan ) );
         if ( config.show_supplement_in_booster_description )
            ActivatePatcher( typeof( PatcherResearchScreen ) );
         if ( config.launch_window_hint_before_ready > 0 || config.launch_window_hint_after_ready > 0 || config.show_contractor_effects_on_button )
            ActivatePatcher( typeof( PatcherVehicleDesigner ) );
         if ( config.hint_available_mission || config.hint_new_candidates || config.hint_propose_join_mission || config.hint_spacepedia_hide || config.show_final_funding_tier )
            ActivatePatcher( typeof( PatcherSolarSystem ) );
      }
   }

   internal abstract class ModPatcher : MarsHorizonPatcher {
      internal static readonly Config config = new Config();
      internal static string Localise ( string tag, params string[] vars ) => MarsHorizonMod.Localise( tag, vars );
      internal static Client activeClient => Controller.Instance.activeClient;
      internal static ClientViewer clientViewer => Controller.Instance.clientViewer;
      internal static Simulation simulation => activeClient.simulation;
   }

   internal class Config : IniConfig {
      [ Config( "\r\n[Vehicle Designer]" ) ]
      [ Config( "On vehicle designer screen, show launch window up to this many months before vehicle is ready.  Default 2.  0 to not show.  Max 6." ) ]
      public byte launch_window_hint_before_ready = 2;
      [ Config( "On vehicle designer screen, show launch window this many months after vehicle is ready.  Default 10.  0 to not show.  Max 24." ) ]
      public byte launch_window_hint_after_ready = 10;
      [ Config( "Replace all left-hand-side hint with launch window if true.  If false, game will show vehicle part / upgrade / contractor hint on hover as normal.  Default true." ) ]
      public bool always_show_launch_window = true;
      [ Config( "Hint text colour for invalid launch dates.  Set to empty to not change (white).  Default #FFBBBB" ) ]
      public string invalid_colour = "#FFBBBB";
      [ Config( "Hint text colour for suboptimal launch dates.  Set to empty to not change (white).  Default #EEDDDD" ) ]
      public string suboptimal_colour = "#EEDDDD";
      [ Config( "Hint text colour for optimal launch dates.  Set to empty to not change (white).  Default #BBFFBB" ) ]
      public string optimal_colour = "#BBFFBB";
      [ Config( "Show contractor effects on contractor buttons.  Default True." ) ]
      public bool show_contractor_effects_on_button = true;

      [ Config( "\r\n[Planet Mission Screen]" ) ]
      [ Config( "Add a Launch Window button on planetery body mission list.  Default True." ) ]
      public bool show_planet_launch_window = true;
      [ Config( "Show expiry countdown for request and joint missions.  Default True." ) ]
      public bool show_mission_expiry = true;
      [ Config( "Show payload(s) time and weight of researched but unplanned mission.  Default True." ) ]
      public bool show_mission_payload = true;

      [ Config( "\r\n[Solar System]" ) ]
      [ Config( "Show an icon next to mission button when a slot is available.  Default True." ) ]
      public bool hint_available_mission = true;
      [ Config( "Show an icon next to crew button when new candidates are available.  Default True." ) ]
      public bool hint_new_candidates = true;
      [ Config( "Show an icon next to diplomacy button when joint mission can be proposed.  Default True." ) ]
      public bool hint_propose_join_mission = true;
      [ Config( "Hide spacepedia alert icon.  Default True." ) ]
      public bool hint_spacepedia_hide = true;
      [ Config( "Toggle highlight colour and icon to distinguish vanilla notices (warnings) and mod notice (info).  Default True." ) ]
      public bool hint_dynamic_colour = true;

      [ Config( "\r\n[Misc]" ) ]
      [ Config( "Show base bonus on base screen, when not in build/edit/clear mode.  Default True." ) ]
      public bool show_base_bonus = true;
      [ Config( "Show which supplements can be installed on booster on research screen.  Default True." ) ]
      public bool show_supplement_in_booster_description = true;
      [ Config( "Show the final tier on next funding, instead of current tier + 1.  Default True." ) ]
      public bool show_final_funding_tier = true;

      [ Config( "\r\n[Ω]" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20220330;

      protected override void OnLoad ( string _ ) {
         launch_window_hint_before_ready = Math.Min( launch_window_hint_before_ready, (byte) 6 );
         launch_window_hint_after_ready  = Math.Min( launch_window_hint_after_ready, (byte) 24 );
         if ( config_version < 20220330 ) { // Added: show_final_funding_tier
            config_version = 20220330;
            Task.Run( Save );
         }
      }
   }

}