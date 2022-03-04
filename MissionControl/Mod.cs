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
      internal static Client activeClient => Controller.Instance?.activeClient;
      internal static ClientViewer clientViewer => Controller.Instance?.clientViewer;
      internal static Simulation simulation => activeClient.simulation;
   }

   public class Config : IniConfig {
      [ Config( "Chance of new request mission for player.  Game default 0.25 (for 25%).  Set to -1 to not change (default)." ) ]
      public float player_request_mission_chance = -1;
      [ Config( "Chance of new request mission for AI.  Ditto." ) ]
      public float ai_request_mission_chance = -1;
      [ Config( "Limit weight changes to player agency only.  Default True." ) ]
      public bool reweight_only_player_agency = true;

      [ Config( "\r\n[Variations]\r\n; Multiply lucartive variation chances.  Default 2.4.  Set to 1 to not change." ) ]
      public float lucrative_weight_multiplier = 2.4f;
      [ Config( "Try divide all variation weight by this amount to reduce work.  Affects lucrative_weight_multiplier accuracy.  Default 10.  Set to 1 to not change." ) ]
      public int variation_weight_divider = 10;

      [ Config( "\r\n[Earth and Moon]\r\n; Weight of each uncrewed Earth mission.  Set to 0 to eliminate, -1 to not change (100).  Same for all below" ) ]
      public int earth_uncrewed_mission_weight = 30;
      public int earth_crewed_mission_weight = 20;
      public int moon_uncrewed_mission_weight = 20;
      public int moon_crewed_mission_weight = 10;
      public int space_station_mission_weight = 10;

      [ Config( "\r\n[Inner Planets]" ) ]
      public int venus_mission_weight = 3;
      public int mercury_mission_weight = 2;

      [ Config( "\r\n[Outter Planets]" ) ]
      public int mars_mission_weight = 5;
      public int jupiter_mission_weight = 0;
      public int saturn_mission_weight = 0;
      public int uranus_mission_weight = 0;
      public int neptune_mission_weight = 0;
      public int pluto_mission_weight = 0;

      [ Config( "\r\n[Others]" ) ]
      public int other_mission_weight = 0;

   }

   internal class PatcherSimulation : ModPatcher {
      internal void Apply () {
         TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateMissionRequestMessage" ), prefix: nameof( TrackNewMissionBefore ), postfix: nameof( TrackNewMissionAfter ) );
         TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateRequestMissionType" ), prefix: nameof( SetMissionWeight ) );
      }

      private static float origChance = -1f;
      private static readonly Dictionary< Data.MissionTemplate, int > origWeight = new Dictionary< Data.MissionTemplate, int >();

      private static void TrackNewMissionBefore ( Simulation __instance, Agency agency, bool forceGenerate ) { try {
         origWeight.Clear();
         if ( forceGenerate ) {
            Info( "{0} is forcing a new mission.", agency.NameLocalised );
            return;
         }
         var chance = agency.isAI ? config.ai_request_mission_chance : config.player_request_mission_chance;
         var rules = __instance.gamedata.rules;
         if ( origChance < 0 ) origChance = rules.requestGenerationChance;
         if ( chance < 0 || chance > 1 ) chance = origChance;
         rules.requestGenerationChance = chance;
         RootMod.Log?.Write( agency.isAI ? TraceLevel.Verbose : TraceLevel.Info,
            "{0} checking new mission.  Current count {2}/{3}, cooldown {4}, chance {1}.", agency.NameLocalised,
            1 - rules.requestGenerationChance, agency.RequestMissionCount, __instance.gamedata.GetEraRequestLimit( agency.era ), agency.turnsUntilNextMissionRequest );
      } catch ( Exception x ) { Err( x ); } }

      private static void TrackNewMissionAfter ( Agency agency, NetMessages.MissionRequest message, bool __result ) { try {
         if ( ! canMod( agency ) ) return;
         if ( origWeight.Count > 0 ) {
            Fine( "Restoring {0} weights.", origWeight.Count );
            foreach ( var pair in origWeight ) pair.Key.requestWeighting = pair.Value;
            origWeight.Clear();
         }
         if ( ! __result ) { Fine( "No new mission." ); return; }
         Info( "New mission {0} {1} {2}.  Next mission after {3}+ turns.", message.milestoneMissionID, message.requestMissionType, message.isJointMission ? "(Join)" : "", message.turnsUntilNextMissionRequest );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetMissionWeight ( Agency agency, Data.MissionTemplate missionTemplate ) { try {
         if ( !canMod( agency ) ) return;
         var m = missionTemplate;
         var weight = GetMissionWeight( m );
         if ( weight < 0 ) weight = m.requestWeighting;
         Fine( "Mission {0}, weight {1}{2}", m.primaryMilestone, m.requestWeighting, weight == m.requestWeighting ? "" : $" => {weight}" );
         if ( weight != m.requestWeighting ) {
            origWeight[ m ] = m.requestWeighting;
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
         var divider = config.variation_weight_divider;
         if ( divider != 1f  ) divider = m.requestMissionTypes.All( e => e.weighting % divider == 0 ) ? divider : 1;
         foreach ( var t in m.requestMissionTypes ) {
            var tWeight = t.weighting;
            if ( t.type == Data.MissionTemplate.RequestMissionType.EType.Lucrative ) tWeight = (int) Math.Round( tWeight * config.lucrative_weight_multiplier );
            if ( divider != 1 ) tWeight /= divider;
            Fine( "   > Variation {0}, weight {1}{2}", t.type, t.weighting, t.weighting == tWeight ? "" : $" => {tWeight}" );
            t.weighting = tWeight;
         }
      }

      private static bool canMod ( Agency agency ) => ! config.reweight_only_player_agency || ! agency.isAI;
   }
}