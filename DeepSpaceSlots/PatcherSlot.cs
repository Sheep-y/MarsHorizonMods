using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ZyMod.MarsHorizon.DeepSpaceSlots {

   internal class PatcherSlot : ModPatcher {
      internal override void Apply () {
         Patch( typeof( MissionSidebarScreen ), "SetState", prefix: nameof( RecalcMissionSlots ) );
         Patch( typeof( PlannedMissionsScreen ), "Setup", prefix: nameof( RecalcMissionSlots ) );
         Patch( typeof( MissionSummary ), "Setup", postfix: nameof( RecalcAfterPhase ) ); // Refresh after mission phase change
         Patch( typeof( ClientViewer ), "SetAgency", postfix: nameof( RecalcAgencySlots ) ); // Refresh after load game
         Patch( typeof( Simulation ).Method( "GetAgencyMaxMissionSlots", typeof( Agency ) ), postfix: nameof( AddMaxMissionSlots ) );
         if ( Patch( typeof( PlannedMissionsScreen ), "GetMissions", postfix: nameof( RemoveMissions ) ) != null ) {
            Patch( typeof( PlannedMissionsScreen ), "Setup", postfix: nameof( ReAddMissions ) );
            Patch( typeof( PlannedMissionsScreen ), "Setup", postfix: nameof( SelectDeepSpaceMission ) );
         }
         Patch( typeof( Simulation ).Method( "CanAgencyDestroyBuilding", 4 ), postfix: nameof( PreventDeepSpaceBuildingDestruction ) );
      }

      private static readonly HashSet< Mission > deepSpaceMissions = new HashSet< Mission >();
      private static int deepSpaceMissionSlot;

      private static IEnumerable< Mission > GetCurrentDeepSpaceMissions ( Agency agency ) { try {
         Fine( "Scanning deep space missions." );
         var now = simulation.universe.turn;
         return agency.missions.Where( ( mission ) => {
            if ( mission.missionState != Mission.EState.Active || mission.Duration < config.deep_space_require_duration ) return false;
            if ( mission.currentPhaseIndex < config.deep_space_min_phase || ( now - mission.launchTurn ) < config.deep_space_min_turn ) return false;
            if ( config.deep_space_require_crewless && mission.CrewParticipated ) return false;
            var template = mission.template;
            if ( template.PhaseCount < config.deep_space_require_phase ) return false;
            Fine( "Mission {0} launched on {1}, phase {2}/{3}, turn {4}/{5}, can transfer to deep space slot.",
               mission.LocalisationTag(), mission.launchTurn, mission.currentPhaseIndex, template.PhaseCount, now - mission.launchTurn, mission.Duration );
            return true;
            /*
            var distance = mission.template.distance;
            if ( distance <= Data.Distance.InnerPlanets ) return false;
            if ( distance > Data.Distance.OuterPlanets && distance <= Data.Distance.TMI ) return false;
            if ( distance == Data.Distance.OuterPlanets ) return mission.currentPhaseIndex > 1;
            Info( "Unknown mission distance {1} for mission {0} to {2}.  Assuming non-deep-space.", distance, mission.LocalisationTag(), mission.template.planetaryBody );
            return false;
            */
         } );
      } catch ( Exception x ) { return Err( x, new List<Mission>() ); } }

      private static int GetDeepSpaceMissionSlots ( Agency agency ) { try {
         int b = GetBuildingSlots( agency ), m = GetMissionSlots( agency ), t = GetTechSlots( agency );
         int result = b + m + t, cap = config.max_slot;
         if ( cap > 0 ) result = Math.Min( result, cap );
         Fine( "Deep space mission slots: Building {0}, Mission {1}, Research {2}, Cap {3}, Final {4}", b, m, t, cap, result );
         return result;
      } catch ( Exception x ) { return Err( x, 0 ); } }

      private static int GetBuildingSlots ( Agency agency ) { try {
         var ext = 0f;
         var result = 0;
         var c1 = ! string.IsNullOrEmpty( config.custom_building1_id ) && config.custom_building1_slot > 0;
         var c2 = ! string.IsNullOrEmpty( config.custom_building2_id ) && config.custom_building2_slot > 0;
         foreach ( var building in agency.buildings ) {
            if ( ! building.IsComplete ) continue;
            var id = building.blueprintId;
            if ( id == "Building_Library" )
               result += config.space_library_slot;
            else if ( id == "Building_DeepSpaceNetwork" )
               result += config.deep_space_network_slot;
            else if ( id == "Building_MissionControl_Expand" )
               ext += config.mission_control_ext_slot;
            else if ( c1 && id.Equals( config.custom_building1_id, StringComparison.InvariantCultureIgnoreCase ) )
               result += config.custom_building1_slot;
            else if ( c2 && id.Equals( config.custom_building2_id, StringComparison.InvariantCultureIgnoreCase ) )
               result += config.custom_building2_slot;
         }
         return result + (int) Math.Floor( ext + 0.021f ); // 0.33 x 3 = 1, 0.66 x 3 = 2
      } catch ( Exception x ) { return Err( x, 0 ); } }

      private static int GetMissionSlots ( Agency agency ) { try {
         var result = 0;
         var c1 = ! string.IsNullOrEmpty( config.custom_mission1_id ) && config.custom_mission1_slot > 0;
         var c2 = ! string.IsNullOrEmpty( config.custom_mission2_id ) && config.custom_mission2_slot > 0;
         if ( config.grand_tour_phase2_slot > 0 || c1 || c2 ) {
            foreach ( var mission in agency.missions ) {
               if ( mission.template.id == "milestone_the_grand_tour" && mission.currentPhaseIndex >= 2 )
                  result += config.grand_tour_phase2_slot;
               if ( c1 && mission.template.id == config.custom_mission1_id && mission.currentPhaseIndex >= config.custom_mission1_phase )
                  result += config.custom_mission1_slot;
               if ( c2 && mission.template.id == config.custom_mission2_id && mission.currentPhaseIndex >= config.custom_mission2_phase )
                  result += config.custom_mission2_slot;
            }
         }
         return result;
      } catch ( Exception x ) { return Err( x, 0 ); } }

      private static int GetTechSlots ( Agency agency ) { try {
         var result = 0;
         var c1 = ! string.IsNullOrEmpty( config.custom_tech1_id ) && config.custom_tech1_slot > 0;
         var c2 = ! string.IsNullOrEmpty( config.custom_tech2_id ) && config.custom_tech2_slot > 0;
         if ( c1 || c2 ) {
            var sim = simulation;
            if ( c1 && sim.HasAgencyCompletedResearch( agency, config.custom_tech1_id ) ) result += config.custom_tech1_slot;
            if ( c2 && sim.HasAgencyCompletedResearch( agency, config.custom_tech2_id ) ) result += config.custom_tech2_slot;
         }
         return result;
      } catch ( Exception x ) { return Err( x, 0 ); } }

      private static void RecalcMissionSlots ( AstroViewElement __instance ) => RecaleMissions( __instance.agency );
      private static void RecalcAfterPhase ( Mission mission ) => RecaleMissions( mission.agency );
      private static void RecalcAgencySlots ( Agency a ) => RecaleMissions( a );

      private static void RecaleMissions ( Agency agency ) { try {
         deepSpaceMissions.Clear();
         if ( agency?.isAI != false ) { deepSpaceMissionSlot = 0; return; }
         var remoteMissions = GetCurrentDeepSpaceMissions( agency );
         deepSpaceMissionSlot = GetDeepSpaceMissionSlots( agency );
         Fine( "Deep space missions: {0}, Deep space slots: {1}", remoteMissions.Count(), deepSpaceMissionSlot );
         foreach ( var mission in remoteMissions.OrderBy( e => e.launchTurn ).Take( deepSpaceMissionSlot ) )
            deepSpaceMissions.Add( mission );
      } catch ( Exception x ) { Err( x ); } }

      private static void AddMaxMissionSlots ( Agency agency, ref int __result ) { try {
         if ( agency.isAI || deepSpaceMissions.Count == 0 ) return;
         Fine( "Adding {0} mission slots", deepSpaceMissions.Count );
         __result += deepSpaceMissions.Count;
      } catch ( Exception x ) { Err( x ); } }

      private static void RemoveMissions ( ref IEnumerable< Mission > __result ) {
         if ( deepSpaceMissions.Count == 0 || __result == null ) return;
         Fine( "Migrating {0} deep space missions.", deepSpaceMissions.Count );
         __result = __result.Except( deepSpaceMissions );
      }

      private static void ReAddMissions ( PlannedMissionsScreen __instance, RectTransform ___installationsParent, SimplePooler<PlannedMissionsScreenToggle> ___installations ) { try {
         __instance.Delay( 1, () => {
            var header = ___installationsParent.GetComponentInChildren< TMPro.TextMeshProUGUI >();
            if ( header != null ) {
               if ( deepSpaceMissionSlot > 0 ) {
                  var txt = MarsHorizonMod.Localise( "MissionControl_Ongoing_Title" );
                  var inst = ___installations.Count - deepSpaceMissions.Count;
                  if ( deepSpaceMissionSlot == deepSpaceMissions.Count )
                     txt += $" ({deepSpaceMissions.Count + inst })";
                  else
                     txt += $" ({deepSpaceMissions.Count + inst }/{ deepSpaceMissionSlot + inst })";
                  header.text = txt;
               } else 
                  header.tag = "Installations";
            }
            if ( deepSpaceMissionSlot > 0 ) ___installationsParent?.gameObject?.SetActive( true );
         } );
         if ( deepSpaceMissions.Count == 0 || ___installations == null ) return;
         foreach ( var mission in deepSpaceMissions )
            ___installations.Get().SetMission( __instance, mission, MissionPlanScreen.EState.Overview );
         Info( "Migrated {0} deep space missions.", deepSpaceMissions.Count );
      } catch ( Exception x ) { Err( x ); } }

      private static void SelectDeepSpaceMission ( PlannedMissionsScreen __instance, Mission mission, SimplePooler<PlannedMissionsScreenToggle> ___installations ) { try {
         if ( mission == null ) return;
         var toggle = ___installations.Find( e => e.Mission.guid == mission.guid );
         if ( toggle != null ) __instance.Delay( 1, toggle.Select );
      } catch ( Exception x ) { Err( x ); } }

      private static void PreventDeepSpaceBuildingDestruction ( Agency agency, Data.Blueprint blueprint, ref string invalidTag, ref bool __result ) { try {
         if ( ! __result ) return;
         var id = blueprint.id;
         if ( id == "Building_Library" )
            __result = CanDestroyBuilding( agency, config.space_library_slot );
         else if ( id == "Building_DeepSpaceNetwork" )
            __result = CanDestroyBuilding( agency, config.deep_space_network_slot );
         else if ( id == "Building_MissionControl_Expand" )
            __result = CanDestroyBuilding( agency, (int) Math.Ceiling( config.mission_control_ext_slot ) );
         else if ( id.Equals( config.custom_building1_id, StringComparison.InvariantCultureIgnoreCase ) )
            __result = CanDestroyBuilding( agency, config.custom_building1_slot );
         else if ( id.Equals( config.custom_building2_id, StringComparison.InvariantCultureIgnoreCase ) )
            __result = CanDestroyBuilding( agency, config.custom_building2_slot );
         if ( ! __result )
            invalidTag = "Building_CannotRemoveBuilding_MissionSlots";
      } catch ( Exception x ) { Err( x ); } }

      private static bool CanDestroyBuilding ( Agency agency, int slot ) {
         if ( slot == 0 ) return true;
         RecaleMissions( agency );
         int remoteMissionCount = deepSpaceMissions.Count;
         if ( remoteMissionCount == 0 ) return true;
         var available = simulation.GetAgencyAvailableMissionSlots( agency );
         Fine( "Destroy building would lost {0} deep space slot.  Current have {1} deep space missions, {2} deep space slot, and {3} general free slot.", remoteMissionCount, deepSpaceMissionSlot, available );
         if ( remoteMissionCount < deepSpaceMissionSlot ) available += deepSpaceMissionSlot - remoteMissionCount;
         return slot <= available;
      }
   }
}