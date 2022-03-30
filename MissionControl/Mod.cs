using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModManagerNet;

namespace ZyMod.MarsHorizon.MissionControl {
   [ BepInPlugin( "Zy.MarsHorizon.MissionControl", "Mission Control", "0.0.2022.0326" ) ]
   public class BIE_Mod : BaseUnityPlugin {
      private void Awake() { BepInUtil.Setup( this, ModPatcher.config ); Mod.Main(); }
      private void OnDestroy() => BepInUtil.Unbind();
   }

   [ EnableReloading ] public static class UMM_Mod {
      public static void Load ( UnityModManager.ModEntry entry ) => UMMUtil.Init( entry, typeof( Mod ) );
   }

   public class Mod : MarsHorizonMod {
      public static void Main () => new Mod().Initialize();
      protected override string GetModName () => "MissionControl";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         if ( ! configLoaded ) config.Load();
         ActivatePatcher( typeof( PatcherMissionSim ) );
         if ( config.milestone_challenge_fund_multiplier != 1 || config.milestone_challenge_research_highpass >= 0 || config.milestone_challenge_no_duplicate_reward )
            ActivatePatcher( typeof( PatcherMilestoneSim ) );
      }
   }

   internal abstract class ModPatcher : MarsHorizonPatcher {
      internal static readonly Config config = new Config();
   }

   internal class Config : IniConfig {
      [ Config( "Apply milestone challenge filtes and mission weight change to player only, and not to AI.  Default True." ) ]
      public bool change_only_player_agency = true;

      [ Config( "\r\n[Milestone]" ) ]
      [ Config( "(This mod only affects new milestone challenges, not existing challengs.)" ) ]
      [ Config( "Fund multiplier of milestone challenges.  Default 1.5 for 150%.  Set to 0 or negative to prevent fund rewards from showing up." ) ]
      public float milestone_challenge_fund_multiplier = 1.5f;
      [ Config( "Disable research cost milestone challenges at this many research left or less in that tree.  Default 3.  Set to -1 to never disable, or 100 to always disable.  When all tech trees are blocked, reward will always be fund.  If milestone_challenge_fund_multiplier is negative, a positive value will be used." ) ]
      public int milestone_challenge_research_highpass = 3;
      [ Config( "The game was designed to not give you the same challenge reward type as the completed one, but a bug prevents it.  This option restores the behaviour whenever feasible." ) ]
      public bool milestone_challenge_no_duplicate_reward = true;

      [ Config( "\r\n[Mission]" ) ]
      [ Config( "Chance of new request mission for player.  Game default 0.25 (for 25%).  Set to -1 to not change (default)." ) ]
      public float player_request_mission_chance = -1;
      [ Config( "Chance of new request mission for AI.  Ditto." ) ]
      public float ai_request_mission_chance = -1;
      [ Config( "Use a standalone random number generator to decide new player missions.  Default True." ) ]
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
      [ Config( "Multiply lucartive variation chances (baseline).  Default 1.8.  Set to 1 to not change." ) ]
      public float lucrative_weight_multiplier = 1.8f;
      [ Config( "Multiply lucartive variation chances when none exists (multiplied).  Default 2.  Set to 1 to not change." ) ]
      public float lucrative_weight_multiplier_opening = 2;
      [ Config( "Multiply lucartive variation chances when all researches are done (multiplied).  Default 5.  Set to 1 to not change." ) ]
      public float lucrative_weight_multiplier_full_tech = 5;
      [ Config( "Multiply publicised variation chances.  Default 1.  Set to 1 to not change." ) ]
      public float publicised_weight_multiplier = 1;
      [ Config( "Multiply test variation chances.  Default 1.  Set to 1 to not change." ) ]
      public float test_weight_multiplier = 1;
      [ Config( "Try divide all variation weight by this amount to save cpu.  Affects multiplier accuracy; set log level to fine to see exact weights.  Default 10.  Set to 1 to not change." ) ]
      public int variation_weight_divider = 10;

      [ Config( "\r\n[Earth and Moon]" ) ]
      [ Config( "Weight of each uncrewed Earth mission.  Set to 0 to eliminate, -1 to not change (100).  Same for all below" ) ]
      [ Config() ] public int earth_uncrewed_mission_weight = 40;
      [ Config() ] public int earth_crewed_mission_weight = 30;
      [ Config() ] public int moon_uncrewed_mission_weight = 30;
      [ Config() ] public int moon_crewed_mission_weight = 12;
      [ Config() ] public int space_station_mission_weight = 20;

      [ Config( "[Inner Planets]" ) ]
      [ Config() ] public int venus_mission_weight = 6;
      [ Config() ] public int mercury_mission_weight = 4;
      [ Config() ] public int mars_mission_weight = 10;

      [ Config( "[Outter Planets]" ) ]
      [ Config() ] public int jupiter_mission_weight = 1;
      [ Config() ] public int saturn_mission_weight = 1;
      [ Config() ] public int uranus_mission_weight = 1;
      [ Config() ] public int neptune_mission_weight = 1;
      [ Config() ] public int pluto_mission_weight = 1;

      [ Config( "[Others Destinations]" ) ]
      public int other_mission_weight = 1;

      [ Config( "\r\n" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200304;

      protected override void OnLoad ( string _ ) {
         if ( challenging_weight_multiplier < 0 ) challenging_weight_multiplier = 1;
         if ( experimental_weight_multiplier < 0 ) experimental_weight_multiplier = 1;
         if ( lucrative_weight_multiplier < 0 ) lucrative_weight_multiplier = 1;
         if ( lucrative_weight_multiplier_opening < 0 ) lucrative_weight_multiplier_opening = 1;
         if ( lucrative_weight_multiplier_full_tech < 0 ) lucrative_weight_multiplier_full_tech = 1;
         if ( publicised_weight_multiplier < 0 ) publicised_weight_multiplier = 1;
         if ( test_weight_multiplier < 0 ) test_weight_multiplier = 1;
         if ( variation_weight_divider < 0 ) variation_weight_divider = 1;
      }
   }
}