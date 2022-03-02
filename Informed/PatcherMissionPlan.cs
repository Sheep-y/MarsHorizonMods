using Astronautica;
using Astronautica.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using static Astronautica.Data;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMissionPlan : ModPatcher {

      internal void Apply () {
         if ( config.show_planet_launch_window ) {
            TryPatch( typeof( MissionSelectSidebarScreen ), "SetState", postfix: nameof( AddLaunchWindowButton ) );
            TryPatch( typeof( MissionSelectSidebarToggle ), "SetMission", prefix: nameof( SetLaunchWindowButton ) );
            TryPatch( typeof( MissionSelectSidebarToggle ), "OnClick", prefix: nameof( ShowLaunchWindow ) );
            TryPatch( typeof( MissionSelectSidebarToggle ), "OnPointerClick", prefix: nameof( BlockLaunchWindowDblClick ) );
            TryPatch( typeof( CalendarScreen ), "IHeaderData.GetTitle", postfix: nameof( SetCalendarTitle ) );
         }
         if ( config.show_mission_expiry )
            TryPatch( typeof( MissionBriefingObjectives ), "SetPhaseDetails", postfix: nameof( ShowMissionExpiry ) );
      }

      private const string LaunchWindowGuid = "e019269c-9b9b-4ee3-ac1c-ee5c35f0e4f6";
      private static readonly MissionTemplateInstance windowTemplate = new MissionTemplateInstance( new MissionTemplate{
         id = "Calendar_LaunchWindow_Title",
         defaultPayloads = new string[0],
         phases = new MissionTemplate.Phase[0],
         planetaryBody = PlanetaryBody.None
      } ){ isRequestMission = true };

      private static void AddLaunchWindowButton ( MissionSelectSidebarScreen __instance, SimplePooler<MissionSelectSidebarGroup> ___missionGroupPooler ) { try {
         Info( "Adding launch window button to {0} mission list.", MissionControl.PlanetaryBody );
         windowTemplate.template.planetaryBody = MissionControl.PlanetaryBody;
         var mission = new Mission( __instance.agency.missions.First() ){ guid = LaunchWindowGuid, launchTurn = 0, vehicle = new Vehicle(), templateInstance = windowTemplate, missionState = Mission.EState.Successful };
         var group = ___missionGroupPooler.Get();
         group.Setup( "Title_Calendar" );
         group.AddMission( mission );
      } catch ( Exception x ) { Err( x ); } }

      private static bool SetLaunchWindowButton ( MissionSelectSidebarToggle __instance, Mission mission, AutoLocalise ___missionNameText, Image ___missionIcon, Toggle ___toggle ) { try {
         if ( mission.guid != LaunchWindowGuid ) return true;
         Fine( "Configuring launch window sidebar button" );
         var type = typeof( MissionSelectSidebarToggle );
         ___missionNameText.tag = "Calendar_LaunchWindow_Title";
         __instance.name = "Calendar_LaunchWindow_Sidebar_Toggle";
         ___missionIcon.gameObject.SetActive( false );
         ___toggle.onValueChanged.RemoveAllListeners();
         ___toggle.isOn = false;
         ___toggle.onValueChanged.AddListener( ( isOn ) => { if ( isOn ) ShowLaunchWindow( mission ); } );
         type.Property( "Mission" ).SetValue( __instance, mission );
         type.Method( "SetMissionState" ).Run( __instance, mission, MissionSelectSidebarToggle.EState.None );
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static bool ShowLaunchWindow ( Mission mission ) { try {
         if ( mission.guid != LaunchWindowGuid ) return true;
         Fine( "Launch Window Button clicked for {0} to {1}", mission.template.originBody, mission.template.planetaryBody );
         var controller = Controller.Instance;
         void Back () => controller.gameUI.SetViewState( controller.clientViewer.stateMissionControlMissionSelect, true );
         controller.clientViewer.EnterCalendarScheduleState( mission, Back, Back );
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static bool BlockLaunchWindowDblClick ( MissionSelectSidebarToggle __instance ) => __instance.Mission.guid != LaunchWindowGuid;

      private static void SetCalendarTitle ( CalendarScreen __instance, ref string __result, ref bool localise ) { try {
         if ( __instance.Mission?.guid != LaunchWindowGuid ) return;
         localise = false;
         var template = __instance.Mission.template;
         __result = Localise( "Name_Body_" + template.originBody ) + " ⮞ " + Localise( "Name_Body_" + template.planetaryBody );
      } catch ( Exception x ) { Err( x ); } }

      private static void ShowMissionExpiry ( MissionBriefingObjectives __instance, Mission mission ) { try {
         Info( mission.missionState, mission.planState );
      } catch ( Exception x ) { Err( x ); } }
   }
}