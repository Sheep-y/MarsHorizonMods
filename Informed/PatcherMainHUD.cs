using Astronautica;
using Astronautica.View;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMainHUD : ModPatcher {
      internal void Apply () {
         if ( config.hint_dynamic_colour )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( GetInfoIcon ) );
         if ( config.hint_available_mission )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintAvailableMission ) );
         if ( config.hint_new_candidates )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintNewCandidates ) );
         if ( config.hint_propose_join_mission )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintJointMission ) );
         if ( config.hint_spacepedia_hide )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HideSpacepediaHint ) );
      }

      private static Sprite icoInfo, icoWarn;
      private static Color clrInfo, clrWarn;
      private static readonly FieldInfo InfoIcon = typeof( SidebarOption ).Field( "infoIcon" );

      private static void GetInfoIcon ( SidebarOption ___missionsOption, SidebarOption ___spacepediaOption ) { try {
         if ( icoInfo != null ) return;
         icoInfo = GetIcon( ___spacepediaOption ).sprite;
         icoWarn = GetIcon( ___missionsOption ).sprite;
         clrInfo = GetHighlight( ___spacepediaOption ).color;
         clrWarn = GetHighlight( ___missionsOption ).color;
      } catch ( Exception x ) { Err( x ); } }

      private static void HintAvailableMission ( HUDScreenSelect __instance, SidebarOption ___missionsOption ) { try {
         var i = 0;
         var showInfo = ! IsActive( ___missionsOption );
         if ( showInfo ) {
            i = __instance.client.simulation.GetAgencyAvailableMissionSlots( __instance.agency );
            showInfo = i > 0;
         }
         if ( showInfo ) {
            Fine( "Hinting {0} available missions.", i );
            ___missionsOption.SetInfoActive( i );
         }
         if ( icoInfo != null ) {
            GetIcon( ___missionsOption ).sprite = showInfo ? icoInfo : icoWarn;
            GetHighlight( ___missionsOption ).color = showInfo ? clrInfo : clrWarn;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void HintNewCandidates ( HUDScreenSelect __instance, SidebarOption ___crewOption ) { try {
         var i = 0;
         var showInfo = ! IsActive( ___crewOption );
         if ( showInfo ) {
            foreach ( var crew in __instance.agency.astronautRecruitPool )
               if ( ! GameStats.HasEntry( "CrewMember", crew.GetStatsId() ) ) i++;
            showInfo = i > 0;
         }
         if ( showInfo ) {
            Fine( "Hinting {0} available Candidates.", i );
            ___crewOption.SetInfoActive( i );
         }
         if ( icoInfo != null ) {
            GetIcon( ___crewOption ).sprite = showInfo ? icoInfo : icoWarn;
            GetHighlight( ___crewOption ).color = showInfo ? clrInfo : clrWarn;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void HintJointMission ( HUDScreenSelect __instance, SidebarOption ___diplomacyOption ) { try {
         var showInfo = ! IsActive( ___diplomacyOption );
         if ( showInfo )
            showInfo = __instance.client.simulation.CanAgencyGenerateJointMissionRequest( __instance.agency, out _ );
         if ( showInfo ) {
            Fine( "Hinting joint mission cooldown." );
            ___diplomacyOption.SetInfoActive( 1 );
         }
         if ( icoInfo != null ) {
            GetIcon( ___diplomacyOption ).sprite = showInfo ? icoInfo : icoWarn;
            GetHighlight( ___diplomacyOption ).color = showInfo ? clrInfo : clrWarn;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void HideSpacepediaHint ( SidebarOption ___spacepediaOption ) { try {
         ___spacepediaOption.SetInfoActive( 0 );
      } catch ( Exception x ) { Err( x ); } }

      private static bool IsActive ( SidebarOption opt ) => ( InfoIcon?.GetValue( opt ) is GameObject obj && obj.activeSelf );
      private static Image GetIcon ( SidebarOption opt ) => opt.gameObject.GetComponentsInChildren< Image >( true ).First( e => e.sprite.name.StartsWith( "Spr_Icon_" ) );
      private static Image GetHighlight ( SidebarOption opt ) => opt.gameObject.GetComponentsInChildren< Image >( true ).First( e => e.sprite.name.EndsWith( "_Notification" ) );
   }
}