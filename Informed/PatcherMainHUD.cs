using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMainHUD : ModPatcher {

      internal void Apply () {
         TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( GetInfoIcon ) );
         if ( config.hint_available_mission )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintAvailableMission ) );
         if ( config.hint_propose_join_mission )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintJointMission ) );
         if ( config.hint_spacepedia_hide )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HideSpacepediaHint ) );
            //TryPatch( typeof( SpacepediaScreen ), "MarkAllRead", postfix: nameof( FixSpacepediaMarkAll ) );
      }

      private static Sprite icoInfo, icoMission, icoDiplomacy;
      private static readonly FieldInfo InfoIcon = typeof( SidebarOption ).Field( "infoIcon" );

      private static void GetInfoIcon ( SidebarOption ___missionsOption, SidebarOption ___diplomacyOption, SidebarOption ___spacepediaOption ) { try {
         if ( icoInfo != null ) return;
         icoInfo = GetIcon( ___spacepediaOption ).sprite;
         icoMission = GetIcon( ___missionsOption ).sprite;
         icoDiplomacy = GetIcon( ___diplomacyOption ).sprite;
      } catch ( Exception x ) { Err( x ); } }

      private static void HintAvailableMission ( HUDScreenSelect __instance, SidebarOption ___missionsOption ) { try {
         GetIcon( ___missionsOption ).sprite = icoMission;
         if ( IsActive( ___missionsOption ) ) return;
         var i = __instance.client.simulation.GetAgencyAvailableMissionSlots( __instance.agency );
         if ( i > 0 ) {
            ___missionsOption.SetInfoActive( i );
            GetIcon( ___missionsOption ).sprite = icoInfo;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void HintJointMission ( HUDScreenSelect __instance, SidebarOption ___diplomacyOption ) { try {
         GetIcon( ___diplomacyOption ).sprite = icoDiplomacy;
         if ( IsActive( ___diplomacyOption ) ) return;
         if ( __instance.client.simulation.CanAgencyGenerateJointMissionRequest( __instance.agency, out _ ) ) {
            ___diplomacyOption.SetInfoActive( 1 );
            GetIcon( ___diplomacyOption ).sprite = icoInfo;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void HideSpacepediaHint ( SidebarOption ___spacepediaOption ) { try {
         //if ( IsActive( ___spacepediaOption ) ) return;
         ___spacepediaOption.SetInfoActive( 0 );
      } catch ( Exception x ) { Err( x ); } }

      private static bool IsActive ( SidebarOption opt ) => ( InfoIcon?.GetValue( opt ) is GameObject obj && obj.activeSelf );
      private static Image GetIcon ( SidebarOption opt ) => opt.gameObject.GetComponentsInChildren< Image >( true ).First( e => e.sprite.name.StartsWith( "Spr_Icon_" ) );
   }
}