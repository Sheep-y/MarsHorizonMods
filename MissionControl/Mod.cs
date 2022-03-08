using Astronautica;
using Astronautica.View;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.MissionControl {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "MissionControl";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.milestone_challenge_fund_multiplier != 1 || config.milestone_challenge_research_highpass >= 0 )
            new PatcherMilestoneSim().Apply();
         new PatcherMissionSim().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
      internal static string Localise ( string tag, params string[] vars ) {
         Dictionary< string, string > variables = null;
         if ( vars?.Length > 0 ) {
            variables = new Dictionary< string, string >();
            for ( var i = 0 ; i + 1 < vars.Length ; i += 2 )
               variables.Add( vars[ i ], vars[ i + 1 ] );
         }
         return ScriptableObjectSingleton<Localisation>.instance.Localise( tag, variables );
      }
   }

   public class Config : IniConfig {
      [ Config( "\r\n[Milestone]" ) ]
      [ Config( "Fund multiplier of milestone challenges.  Default 1.5 for 150%.  Set to 0 or negative to prevent fund rewards from showing up." ) ]
      public float milestone_challenge_fund_multiplier = 1.5f;
      [ Config( "Disable research cost milestone challenges at this many research left or less in that tree.  Default 3.  Set to -1 to never disable, or 100 to always disable.  On trigger, if milestone_challenge_fund_multiplier is negative, a positive value will be used." ) ]
      public int milestone_challenge_research_highpass = 3;

      [ Config( "\r\n[Mission]" ) ]
      [ Config( "Chance of new request mission for player.  Game default 0.25 (for 25%).  Set to -1 to not change (default)." ) ]
      public float player_request_mission_chance = -1;
      [ Config( "Chance of new request mission for AI.  Ditto." ) ]
      public float ai_request_mission_chance = -1;
      [ Config( "Limit weight changes to player agency only.  Default True." ) ]
      public bool reweight_only_player_agency = true;
      [ Config( "Use a standalone random number generator for new missions.  Also apply to AI.  Default True." ) ]
      public bool standalone_mission_rng = true;

      [ Config( "\r\n[Joint]" ) ]
      [ Config( "Section note: join mission happens only to player agency.  Does not affect AI." ) ]
      [ Config( "Base join mission chance.  Game default 0.1 (for 10%).  Set to -1 to not change (default)." ) ]
      public float joint_mission_chance = -1;
      [ Config( "Diplomacy Office bonus chance (added).  Game default 0.05 (for +5%).  Set to -1 to not change (default)." ) ]
      public float diplomacy_office_bonus_chance = -1;
      [ Config( "Team Player bonus chance (multiplied).  Game default 1 (for 100% bonus).  Set to -1 to not change (default)." ) ]
      public float joint_trait_multiplier = -1;

      [ Config( "\r\n[Variations]" ) ]
      [ Config( "Multiply challenging variation (all level) chances.  Default 1.  Set to 1 to not change." ) ]
      public float challenging_weight_multiplier = 1;
      [ Config( "Multiply experimental variation chances.  Default 1.  Set to 1 to not change." ) ]
      public float experimental_weight_multiplier = 1;
      [ Config( "Multiply lucartive variation chances.  Default 2.4.  Set to 1 to not change." ) ]
      public float lucrative_weight_multiplier = 2.4f;
      [ Config( "Multiply publicised variation chances.  Default 1.  Set to 1 to not change." ) ]
      public float publicised_weight_multiplier = 1;
      [ Config( "Multiply test variation chances.  Default 1.  Set to 1 to not change." ) ]
      public float test_weight_multiplier = 1;
      [ Config( "Try divide all variation weight by this amount to save cpu.  Affects multiplier accuracy, set log level to fine to see exact weight.  Default 10.  Set to 1 to not change." ) ]
      public int variation_weight_divider = 10;

      [ Config( "\r\n[Earth and Moon]" ) ]
      [ Config( "Weight of each uncrewed Earth mission.  Set to 0 to eliminate, -1 to not change (100).  Same for all below" ) ]
      [ Config() ] public int earth_uncrewed_mission_weight = 30;
      [ Config() ] public int earth_crewed_mission_weight = 20;
      [ Config() ] public int moon_uncrewed_mission_weight = 20;
      [ Config() ] public int moon_crewed_mission_weight = 10;
      [ Config() ] public int space_station_mission_weight = 10;

      [ Config( "\r\n[Inner Planets]" ) ]
      [ Config() ] public int venus_mission_weight = 3;
      [ Config() ] public int mercury_mission_weight = 2;

      [ Config( "\r\n[Outter Planets]" ) ]
      [ Config() ] public int mars_mission_weight = 5;
      [ Config() ] public int jupiter_mission_weight = 0;
      [ Config() ] public int saturn_mission_weight = 0;
      [ Config() ] public int uranus_mission_weight = 0;
      [ Config() ] public int neptune_mission_weight = 0;
      [ Config() ] public int pluto_mission_weight = 0;

      [ Config( "\r\n[Others]" ) ]
      public int other_mission_weight = 0;

      [ Config( "\r\n" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200304;
   }
}