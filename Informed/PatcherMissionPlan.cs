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
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMissionPlan : ModPatcher {

      internal void Apply () {
         TryPatch( typeof( MissionSelectSidebarScreen ), "SetState", postfix: nameof( AddLaunchWindowButton ) );
         TryPatch( typeof( MissionSelectSidebarToggle ), "SetMission", prefix: nameof( SetLaunchWindowButton ) );
         TryPatch( typeof( MissionSelectSidebarToggle ), "OnClick", prefix: nameof( ShowLaunchWindow ) );
      }

      private const string LaunchWindowGuid = "e019269c-9b9b-4ee3-ac1c-ee5c35f0e4f6";

      private static void AddLaunchWindowButton ( MissionSelectSidebarScreen __instance, SimplePooler<MissionSelectSidebarGroup> ___missionGroupPooler ) { try {
         Fine( "Adding the launch window button in mission select screen." );
         var group = ___missionGroupPooler.Get();
         group.Setup( "Title_Calendar" );
         var mission = new Mission( __instance.agency.missions.First() ){ guid = LaunchWindowGuid };
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
         ___toggle.onValueChanged.AddListener( ( isOn ) => {
            if ( ! isOn ) return;
            Fine( "Launch Window Button clicked for {0}", MissionControl.PlanetaryBody );
         } );
         type.Property( "Mission" ).SetValue( __instance, mission );
         type.Method( "SetMissionState" ).Run( __instance, mission, MissionSelectSidebarToggle.EState.None );
         Fine( "Configured launch window sidebar button" );
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static bool ShowLaunchWindow ( Mission mission ) => mission.guid != LaunchWindowGuid;
   }

   //internal class LaunchWindowMission : Mission {}
}