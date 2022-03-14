using Astronautica.View;
using System;
using System.Collections.Generic;
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
         if ( config.show_planet_launch_window || config.show_mission_expiry )
            new PatcherMissionPlan().Apply();
         if ( config.show_supplement_in_booster_description )
            new PatcherResearchScreen().Apply();
         if ( config.launch_window_hint_before_ready > 0 && config.launch_window_hint_before_ready > 0 || config.show_contractor_effects_on_button )
            new PatcherVehicleDesigner().Apply();
         if ( config.hint_available_mission || config.hint_propose_join_mission || config.hint_spacepedia_hide )
            new PatcherMainHUD().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
      internal static string Localise ( string tag, params string[] vars ) => MarsHorizonMod.Localise( tag, vars );
      internal static Client activeClient => Controller.Instance?.activeClient;
      internal static ClientViewer clientViewer => Controller.Instance?.clientViewer;
      internal static Simulation simulation => activeClient.simulation;
   }

   public class Config : IniConfig {
      [ Config( "Show base bonus on base screen, when not in build/edit/clear mode.  Default True." ) ]
      public bool show_base_bonus = true;
      [ Config( "Add a Launch Window button on planetery body mission list.  Default True." ) ]
      public bool show_planet_launch_window = true;
      [ Config( "Show expiry countdown for request and joint missions.  Default True." ) ]
      public bool show_mission_expiry = true;
      [ Config( "Show payload(s) time and weight of researched but unplanned mission.  Default True." ) ]
      public bool show_mission_payload = true;
      [ Config( "Show which supplements can be installed on booster on research screen.  Default True." ) ]
      public bool show_supplement_in_booster_description = true;

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

      [ Config( "\r\n[Solar System]" ) ]
      [ Config( "Show alert icon next to mission button when a slot is available.  Default True." ) ]
      public bool hint_available_mission = true;
      [ Config( "Show alert icon next to diplomacy button when joint mission can be proposed.  Default True." ) ]
      public bool hint_propose_join_mission = true;
      [ Config( "Hide spacepedia alert icon.  Default True." ) ]
      public bool hint_spacepedia_hide = true;

      [ Config( "\r\n" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200226;

      public override void Load ( object subject, string path ) {
         base.Load( subject, path );
         if ( ! ( subject is Config conf ) ) return;
         conf.launch_window_hint_before_ready = Math.Min( conf.launch_window_hint_before_ready, (byte) 6 );
         conf.launch_window_hint_after_ready  = Math.Min( conf.launch_window_hint_after_ready, (byte) 24 );
      }
   }

}