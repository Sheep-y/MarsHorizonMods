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
         new PatcherSimulation().Apply();
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

   internal class PatcherSimulation : ModPatcher {
      internal void Apply () {
         TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateMissionRequestMessage" ), prefix: nameof( TrackNewMissionBefore ), postfix: nameof( TrackNewMissionAfter ) );
         TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateRequestMissionType" ), prefix: nameof( SetMissionWeight ), postfix: nameof( LogMissionRemoval ) );
         TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateRequestMissionContext" ), postfix: nameof( LogMissionRemoval ) );
         if ( config.joint_mission_chance >= 0 || config.diplomacy_office_bonus_chance >= 0 )
            TryPatch( typeof( Simulation ).Method( "GetAgencyRollJointMission" ), prefix: nameof( SetJointMissionChance ) );
         if ( config.joint_trait_multiplier >= 0 )
            TryPatch( typeof( Agency ).Method( "GetAgencyTraits" ), postfix: nameof( SetTraitMissionChance ) );
         if ( config.standalone_mission_rng ) {
            TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateMissionRequestMessage" ), prefix: nameof( TrackPlayerNewMission ) );
            TryPatch( typeof( LINQExtensions ).Method( "RandomElement" ).MakeGenericMethod( typeof( Data.RequestMissionData ) ), postfix: nameof( RollRandomMission ) );
         }
      }

      private static float origChance = -1f;
      private static readonly Dictionary< Data.MissionTemplate, int > origWeightM = new Dictionary< Data.MissionTemplate, int >();
      private static readonly Dictionary< Data.MissionTemplate.RequestMissionType, int > origWeightT = new Dictionary< Data.MissionTemplate.RequestMissionType, int >();

      private static Data.MissionTemplate lastMission = null;
      private static bool allowed = false;

      private static void TrackNewMissionBefore ( Simulation __instance, Agency agency, bool forceGenerate ) { try {
         origWeightM.Clear();
         lastMission = null;
         if ( forceGenerate ) { Info( "{0} is forcing a new mission.", agency.NameLocalised ); return; }
         var rules = __instance.gamedata.rules;
         if ( origChance < 0 ) origChance = rules.requestGenerationChance;
         var chance = agency.isAI ? config.ai_request_mission_chance : config.player_request_mission_chance;
         rules.requestGenerationChance = ( chance < 0 || chance > 1 ) ? origChance : chance;
         RootMod.Log?.Write( agency.isAI ? TraceLevel.Verbose : TraceLevel.Info,
            "{0} checking new mission.  Current count {2}/{3}, cooldown {4}, chance {1}.", agency.NameLocalised,
            1 - rules.requestGenerationChance, agency.RequestMissionCount, __instance.gamedata.GetEraRequestLimit( agency.era ), agency.turnsUntilNextMissionRequest );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetJointMissionChance ( Simulation __instance, Agency agency, Agency selectedAgency ) { try {
         Fine( "Rolling joint mission between {0} and {1}.", agency.NameLocalised, selectedAgency?.NameLocalised ?? "{Any}" );
         var rules = __instance.gamedata.rules;
         if ( config.joint_mission_chance >= 0 && config.joint_mission_chance != rules.jointMissionChance ) {
            Info( "Setting base joint mission chance from {0} to {1}.", rules.jointMissionChance, config.joint_mission_chance );
            rules.jointMissionChance = config.joint_mission_chance;
         }
         if ( config.diplomacy_office_bonus_chance >= 0 && config.diplomacy_office_bonus_chance != rules.diplomacyOfficeIncrease ) {
            Info( "Setting Diplomacy Office joint mission bonus chance from {0} to {1}.", rules.diplomacyOfficeIncrease, config.diplomacy_office_bonus_chance );
            rules.diplomacyOfficeIncrease = config.diplomacy_office_bonus_chance;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void SetTraitMissionChance ( Data.AgencyTrait.EAgencyTrait traitType, List< Data.AgencyTrait > __result ) { try {
         if ( traitType != Data.AgencyTrait.EAgencyTrait.JointMissionSpawnRate || __result == null ) return;
         for ( var i = 0 ; i < __result.Count ; i++ ) {
            var trait = __result[ i ];
            if ( config.joint_trait_multiplier < 0 || config.joint_trait_multiplier == trait.value ) return;
            Info( "Setting trait {0} bonus chance from {1} to {2}.", Localise( trait.LocalisationTitleTag ), trait.value, config.joint_trait_multiplier );
            trait.value = config.joint_trait_multiplier;
            __result[ i ] = trait;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void TrackNewMissionAfter ( NetMessages.MissionRequest message, bool __result ) { try {
         lastMission = null;
         if ( origWeightM.Count > 0 || origWeightT.Count > 0 ) {
            Fine( "Restoring {0}+{1} weights.", origWeightM.Count, origWeightT.Count );
            foreach ( var pair in origWeightM ) pair.Key.requestWeighting = pair.Value;
            foreach ( var pair in origWeightT ) pair.Key.weighting = pair.Value;
            origWeightM.Clear();
            origWeightT.Clear();
         }
         if ( ! __result ) { Fine( "No new mission." ); return; }
         Info( "New mission {0} {1} {2}.  Next mission after {3}+ turns.", message.milestoneMissionID, message.requestMissionType, message.isJointMission ? "(Join)" : "", message.turnsUntilNextMissionRequest );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetMissionWeight ( Agency agency, Data.MissionTemplate missionTemplate ) { try {
         if ( ( config.reweight_only_player_agency && agency.isAI ) || missionTemplate == lastMission ) return;
         var m = lastMission = missionTemplate;
         allowed = true;
         var weight = GetMissionWeight( m );
         if ( weight < 0 ) weight = m.requestWeighting;
         Fine( "Mission {0}, weight {1}{2}.", m.primaryMilestone, m.requestWeighting, weight == m.requestWeighting ? "" : $" => {weight}" );
         if ( weight != m.requestWeighting ) {
            origWeightM[ m ] = m.requestWeighting;
            m.requestWeighting = weight;
         }
         SetVariationWeights( m );
      } catch ( Exception x ) { Err( x ); } }

      private static int GetMissionWeight ( Data.MissionTemplate m ) {
         switch ( m.planetaryBody ) {
            case Data.PlanetaryBody.Earth:
               if ( m.crewRemainOnStation ) return config.space_station_mission_weight;
               return m.IsCrewed ? config.earth_crewed_mission_weight : config.earth_uncrewed_mission_weight;
            case Data.PlanetaryBody.Moon:
               return m.IsCrewed ? config.moon_crewed_mission_weight : config.moon_uncrewed_mission_weight;
            case Data.PlanetaryBody.Mercury: return config.mercury_mission_weight;
            case Data.PlanetaryBody.Venus: return config.venus_mission_weight;
            case Data.PlanetaryBody.Mars: return config.mars_mission_weight;
            case Data.PlanetaryBody.Jupiter: return config.jupiter_mission_weight;
            case Data.PlanetaryBody.Saturn: return config.saturn_mission_weight;
            case Data.PlanetaryBody.Uranus: return config.uranus_mission_weight;
            case Data.PlanetaryBody.Neptune: return config.neptune_mission_weight;
            case Data.PlanetaryBody.Pluto: return config.pluto_mission_weight;
            default: return config.other_mission_weight;
         }
      }

      private static void SetVariationWeights ( Data.MissionTemplate m ) {
         if ( m.requestWeighting == 0 ) return;
         var divider = config.variation_weight_divider;
         if ( divider != 1f  ) divider = m.requestMissionTypes.All( e => e.weighting % divider == 0 ) ? divider : 1;
         foreach ( var t in m.requestMissionTypes ) {
            var tWeight = t.weighting;
            var multiplier = GetVariationWeight( t );
            if ( multiplier != 1 ) tWeight = (int) Math.Round( tWeight * multiplier );
            if ( divider != 1 ) tWeight /= divider;
            Fine( "   > Variation {0}, weight {1}{2}", t.type, t.weighting, t.weighting == tWeight ? "" : $" => {tWeight}" );
            if ( t.weighting != tWeight ) {
               origWeightT[ t ] = t.weighting;
               t.weighting = tWeight;
            }
         }
      }

      private static float GetVariationWeight ( Data.MissionTemplate.RequestMissionType t ) {
         switch ( t.type ) {
            case Data.MissionTemplate.RequestMissionType.EType.Challenging_1 :
            case Data.MissionTemplate.RequestMissionType.EType.Challenging_2 :
            case Data.MissionTemplate.RequestMissionType.EType.Challenging_3 : return config.challenging_weight_multiplier;
            case Data.MissionTemplate.RequestMissionType.EType.EfficiencyTest :
            case Data.MissionTemplate.RequestMissionType.EType.MarsTechTest : return config.test_weight_multiplier;
            case Data.MissionTemplate.RequestMissionType.EType.ExperimentalFuel :
            case Data.MissionTemplate.RequestMissionType.EType.ExperimentalPayload : return config.experimental_weight_multiplier;
            case Data.MissionTemplate.RequestMissionType.EType.Publicised : return config.publicised_weight_multiplier;
            case Data.MissionTemplate.RequestMissionType.EType.Lucrative : return config.lucrative_weight_multiplier;
         }
         return 1f;
      }

      private static void LogMissionRemoval ( bool __result ) {
         if ( lastMission == null || __result == allowed ) return;
         Fine( allowed ? "X Mission removed due to era, research, or ongoing mission." : "X ... then restored after forced generation." );
         allowed = __result;
      }

      private static bool isAI = true;
      private static void TrackPlayerNewMission ( Agency agency ) => isAI = ! agency.isAI;

      private static Random missionRNG;
      private static void RollRandomMission ( IEnumerable< Data.RequestMissionData > sequence, ref Data.RequestMissionData  __result ) { try {
         if ( isAI ) return;
         if ( missionRNG == null ) missionRNG = new Random();
         List<Data.RequestMissionData> list = sequence.ToList();
         var i = missionRNG.Next( 0, list.Count );
         Info( "Unity rolled {0}-{1} ({2}) out of {3}.  RNG rolled {4} ({5}).",
            list.IndexOf( __result ), list.LastIndexOf( __result ), __result.templateInstance.template.id, list.Count,
            i, list[ i ].templateInstance.template.id );
         __result = list[ i ];
      } catch ( Exception x ) { Err( x ); } }
   }
}