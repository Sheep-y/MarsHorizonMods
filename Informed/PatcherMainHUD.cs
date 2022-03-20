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
         if ( ! IsActive( ___missionsOption ) ) i = __instance.client.simulation.GetAgencyAvailableMissionSlots( __instance.agency );
         SetInfoState( ___missionsOption, i, "Hinting {0} available missions." );
      } catch ( Exception x ) { Err( x ); } }

      private static void HintNewCandidates ( HUDScreenSelect __instance, SidebarOption ___crewOption ) { try {
         var i = 0;
         if ( ! IsActive( ___crewOption ) )
            foreach ( var crew in __instance.agency.astronautRecruitPool )
               if ( ! GameStats.HasEntry( "CrewMember", crew.GetStatsId() ) )
                  i++;
         SetInfoState( ___crewOption, i, "Hinting {0} available candidates." );
      } catch ( Exception x ) { Err( x ); } }

      private static void HintJointMission ( HUDScreenSelect __instance, SidebarOption ___diplomacyOption ) { try {
         var i = 0;
         if ( ! IsActive( ___diplomacyOption ) &&
               __instance.client.simulation.CanAgencyGenerateJointMissionRequest( __instance.agency, out _ ) )
            i = 1;
         SetInfoState( ___diplomacyOption, i, "Hinting joint mission cooldown." );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetInfoState ( SidebarOption opt, int active, string msg ) { try {
         if ( active < 0 ) active = 0;
         else Fine( msg, active );
         opt.SetInfoActive( active );
         if ( icoInfo != null ) {
            GetIcon( opt ).sprite = active > 0 ? icoInfo : icoWarn;
            GetHighlight( opt ).color = active > 0 ? clrInfo : clrWarn;
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