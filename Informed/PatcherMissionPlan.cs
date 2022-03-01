using Astronautica;
using Astronautica.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMissionPlan : ModPatcher {

      internal void Apply () {
         TryPatch( typeof( SolarScreen ), "EnterMissionSelectState", nameof( LogEnter ) ); // Initial switch from solar system to mission list

         // MissionControl.Open
         // MissionControl.SetMission
         // MissionSelectSidebarScreen.Setup
         // MissionSelectSidebarScreen.SetCategoryToggle
         // MissionSelectSidebarScreen.SetState

         TryPatch( typeof( MissionSelectSidebarGroup ), "AddMission", nameof( AddMission ) ); // Add mission buttons.
         TryPatch( typeof( MissionSelectSidebarToggle ), "DoSelectToggle", nameof( DoSelectToggle ) ); // Mission button clicked.

         TryPatch( typeof( MissionSelectSidebarToggle ), "OnClick", nameof( OnClick ) );
         TryPatch( typeof( MissionPlanScreen ), "SetMission", nameof( SetMission ) );
         TryPatch( typeof( MissionPlanScreen ), "SetMissionSelectState", nameof( SetMissionSelectState ) );

         //TryPatch( typeof( MissionControl ), "GetMissionPlanState", nameof( LogGetMissionPlan ) ); // 4 times initial load, 3 times per select
         //TryPatch( typeof( MissionPlanOverviewToggle ), "RefreshActionIcon", nameof( LogRefreshActionIcon ) ); // 4 times initial load, 3 times per select
      }

      private static void LogEnter () => Info( "SolarScreen.EnterMissionSelectState" );
      private static void DoSelectToggle () => Info( "MissionPlanScreen.DoSelectToggle" );
      private static void AddMission ( object __instance, Mission mission ) => Info( "{1}.AddMission {0}", mission, __instance.GetType() );
      private static void SetMission ( object __instance, Mission mission ) => Info( "{1}.SetMission {0}", mission, __instance.GetType() );
      private static void OnClick ( Mission mission ) => Info( "MissionSelectSidebarToggle.OnClick {0}", mission );
      private static void SetMissionSelectState () => Info( "MissionPlanScreen.SetMissionSelectState" );
      private static void LogGetMissionPlan ( Mission mission ) => Info( "MissionControl.GetMissionPlanState {0}", mission );
      private static void LogRefreshActionIcon ( Mission mission ) => Info( "MissionPlanOverviewToggle.RefreshActionIcon {0}", mission );
   }

}