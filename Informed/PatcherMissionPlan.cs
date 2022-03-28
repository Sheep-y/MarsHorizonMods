using Astronautica;
using Astronautica.View;
using System;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using static Astronautica.Data;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMissionPlan : ModPatcher {
      internal void Apply () {
         if ( config.show_planet_launch_window ) {
            Patch( typeof( MissionSelectSidebarScreen ), "SetState", postfix: nameof( AddLaunchWindowButton ) );
            Patch( typeof( MissionSelectSidebarToggle ), "SetMission", prefix: nameof( SetLaunchWindowButton ) );
            Patch( typeof( MissionSelectSidebarToggle ), "OnClick", prefix: nameof( ShowLaunchWindow ) );
            Patch( typeof( MissionSelectSidebarToggle ), "OnPointerClick", prefix: nameof( BlockLaunchWindowDblClick ) );
            Patch( typeof( CalendarScreen ), "IHeaderData.GetTitle", postfix: nameof( SetCalendarTitle ) );
         }
         if ( config.show_mission_expiry || config.show_mission_payload )
            Patch( typeof( MissionSidebarResearchRequirements ), "SetMission", postfix: nameof( ShowMissionExpiry ) );
      }

      private const string LaunchWindowGuid = "e019269c-9b9b-4ee3-ac1c-ee5c35f0e4f6";
      private static readonly MissionTemplateInstance windowTemplate = new MissionTemplateInstance( new MissionTemplate{
         id = "Calendar_LaunchWindow_Title",
         defaultPayloads = new string[0],
         phases = new MissionTemplate.Phase[0],
         planetaryBody = PlanetaryBody.None
      } ){ isRequestMission = true };

      private static void AddLaunchWindowButton ( MissionSelectSidebarScreen __instance, SimplePooler<MissionSelectSidebarGroup> ___missionGroupPooler ) { try {
         var first_mission = __instance.agency.missions.FirstOrDefault();
         if ( first_mission == null ) { Fine( "No mission.  Launch Window button not added." ); return; }
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
         ___missionIcon.gameObject.SetActive( false ); // Spr_Icon_MissionPlan_Calendar
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
         void Back () => Controller.Instance.gameUI.SetViewState( clientViewer.stateMissionControlMissionSelect, true );
         clientViewer.EnterCalendarScheduleState( mission, Back, Back );
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static bool BlockLaunchWindowDblClick ( MissionSelectSidebarToggle __instance ) => __instance.Mission.guid != LaunchWindowGuid;

      private static void SetCalendarTitle ( CalendarScreen __instance, ref string __result, ref bool localise ) { try {
         if ( __instance.Mission?.guid != LaunchWindowGuid ) return;
         localise = false;
         var template = __instance.Mission.template;
         __result = Localise( "Name_Body_" + template.originBody ) + " -> " + Localise( "Name_Body_" + template.planetaryBody );
      } catch ( Exception x ) { Err( x ); } }

      private static void ShowMissionExpiry ( Mission mission, AutoLocalise ___descriptionText ) { try {
         var buf = new StringBuilder();
         if ( config.show_mission_expiry && mission.IsRequestMission ) {
            var tInstance = mission.templateInstance;
            var remaining = tInstance.lifespan + tInstance.turnAdded - simulation.universe.turn;
            Fine( "{0} will expires in {1} turns", mission.template.id, remaining );
            if ( remaining >= 0 )
               buf.Append( "\n\n" ).Append( Localise( "Mission_Summary_Turns_Remaining", "turns", remaining.ToString() ) );
         }
         if ( config.show_mission_payload && mission.missionState == Mission.EState.Planning && ! mission.template.IsSoundingRocking ) {
            var agency = mission.agency;
            var template = mission.template;
            var minCrew = template.minCrew;
            if ( mission.IsRequestMission || simulation.HasAgencyCompletedResearch( agency, template.id ) ) {
               var payloads = simulation.GetAgencyMissionPayloads( agency, mission );
               Fine( "Found {0} payloads for {1}", payloads?.Count().ToString() ?? "null", template.id );
               if ( payloads != null && payloads.Length > 0 )
                  buf.Append( "\n\n" ).Append( Localise( "Mission_Setup_Category_Payload_Selected" ) );
                  foreach ( var payload in payloads ) {
                     if ( ! template.TryGetPayloadLocalisationDescriptionOverride( payload.id, mission.requestMissionContext, out var pname ) ) pname = $"Name_{payload.id}";
                     var time = simulation.GetAgencyPayloadBuildTime( agency, payload );
                     buf.AppendFormat( "\n{0} <size=85%> {1} {2}  {3:n0}kg", Localise( pname ), time, Localise( time > 1 ? "Month_Plural" : "Month_Singular" ), payload.weight );
                     if ( minCrew > 0 ) {
                        buf.AppendFormat( "  <sprite name=\"Payload_Crew\"> {0}", minCrew );
                        if ( minCrew != payload.maxCrew ) buf.AppendFormat( "-{0}", payload.maxCrew );
                     }
                     buf.Append( "</size>" );
                     if ( ! simulation.HasAgencyUnlockedPayload( agency, payload ) ) buf.Append( " <sprite name=\"WarningScience\"/>" );
                  }
            }
         }
         if ( buf.Length > 2 )
            ___descriptionText.text = Localise( ___descriptionText.tag ) + buf.ToString();
      } catch ( Exception x ) { Err( x ); } }
   }
}