using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.DeepSpaceSlots {

   internal class PatcherSlot : ModPatcher {
      internal void Apply () {
         TryPatch( typeof( MissionSidebarScreen ), "SetState", prefix: nameof( RecaleMissionSlots ) );
         TryPatch( typeof( PlannedMissionsScreen ), "Setup", prefix: nameof( RecaleMissionSlots ) );
         TryPatch( typeof( MissionSummary ), "Setup", postfix: nameof( RecaleAfterPhase ) );
         TryPatch( typeof( Simulation ).Method( "GetAgencyMaxMissionSlots", typeof( Agency ) ), postfix: nameof( AddMaxMissionSlots ) );
         if ( TryPatch( typeof( PlannedMissionsScreen ), "GetMissions", postfix: nameof( RemoveMissions ) ) != null )
            TryPatch( typeof( PlannedMissionsScreen ), "Setup", postfix: nameof( ReAddMissions ) );
         TryPatch( typeof( Simulation ).Methods( "CanAgencyDestroyBuilding" ).FirstOrDefault( e => e.GetParameters().Length >= 4 ), postfix: nameof( PreventDeepSpaceBuildingDestruction ) );
      }

      private static readonly HashSet< Mission > deepSpaceMissions = new HashSet< Mission >();
      private static int deepSpaceMissionSlot;

      private static IEnumerable< Mission > GetCurrentDeepSpaceMissions ( Agency agency ) { try {
         Fine( "Scanning deep space missions." );
         return agency.missions.Where( ( mission ) => {
            if ( mission.missionState != Mission.EState.Active || mission.Duration < 30 ) return false;
            var template = mission.template;
            if ( template.PhaseCount <= 2 || template.phases.Skip( 2 ).Sum( e => e.turnDelay ) < 6 ) return false;
            var result = mission.currentPhaseIndex > 1;
            Fine( "Mission {0} launched on {1}, duration {4}, phase {2}/{3}, {5} transfer to deep space network.",
               mission.LocalisationTag(), mission.launchTurn, mission.currentPhaseIndex, template.PhaseCount, mission.Duration, result ? "can" : "future" );
            return result;
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
         var ext = 0f;
         var result = 0;
         foreach ( var building in agency.buildings ) {
            if ( ! building.IsComplete ) continue;
            var id = building.blueprintId;
            if ( id == "Building_Library" )
               result += config.space_library_slot;
            else if ( id == "Building_DeepSpaceNetwork" )
               result += config.deep_space_network_slot;
            else if ( id == "Building_MissionControl_Expand" )
               ext += config.mission_control_ext_slot;
            else if ( id.Equals( config.custom_building1_id, StringComparison.InvariantCultureIgnoreCase ) )
               result += config.custom_building1_slot;
            else if ( id.Equals( config.custom_building2_id, StringComparison.InvariantCultureIgnoreCase ) )
               result += config.custom_building2_slot;
         }
         Fine( "Total deep space slot from buildings: {0} + {1}.", result, ext );
         if ( config.grand_tour_phase2_slot > 0 ) {
            foreach ( var mission in agency.missions ) {
               if ( mission.template.id == "milestone_the_grand_tour" && mission.currentPhaseIndex >= 2 ) {
                  result += config.grand_tour_phase2_slot;
                  break;
               }
            }
         }
         return result + (int) Math.Floor( ext + 0.021f ); // 0.33 x 3 = 1, 0.66 x 3 = 2
      } catch ( Exception x ) { return Err( x, 0 ); } }

      private static void RecaleMissionSlots ( AstroViewElement __instance ) => RecaleMissions( __instance.agency );
      private static void RecaleAfterPhase ( Mission mission ) => RecaleMissions( mission.agency );

      private static void RecaleMissions ( Agency agency ) { try {
         deepSpaceMissions.Clear();
         if ( agency.isAI ) { deepSpaceMissionSlot = 0; return; }
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
         var header = ___installationsParent.GetComponentInChildren< TMPro.TextMeshProUGUI >();
         if ( header != null ) {
            var txt = MarsHorizonMod.Localise( deepSpaceMissions.Count == 0 ? "Installations" : "MissionControl_Ongoing_Title" );
            if ( deepSpaceMissions.Count > 0 ) txt += $" ({deepSpaceMissions.Count + ___installations.Count })";
            header.text = txt;
         }
         if ( deepSpaceMissions.Count == 0 || ___installations == null ) return;
         foreach ( var mission in deepSpaceMissions )
            ___installations.Get().SetMission( __instance, mission, MissionPlanScreen.EState.Overview );
         ___installationsParent?.gameObject?.SetActive( true );
         Info( "Migrated {0} deep space missions.", deepSpaceMissions.Count );
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
         var available = Controller.Instance?.activeClient.simulation.GetAgencyAvailableMissionSlots( agency );
         Fine( "Destroy building would lost {0} deep space slot.  Current have {1} deep space missions, {2} deep space slot, and {3} general free slot.", remoteMissionCount, deepSpaceMissionSlot, available );
         if ( remoteMissionCount < deepSpaceMissionSlot ) available += deepSpaceMissionSlot - remoteMissionCount;
         return slot <= available;
      }
   }
}