using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherMainHUD : ModPatcher {

      internal void Apply () {
         if ( config.hint_available_mission )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintAvailableMission ) );
         if ( config.hint_propose_join_mission )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintJointMission ) );
         if ( config.hint_spacepedia_hide )
            TryPatch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HideSpacepediaHint ) );
            //TryPatch( typeof( SpacepediaScreen ), "MarkAllRead", postfix: nameof( FixSpacepediaMarkAll ) );
      }

      private static FieldInfo WarningCG = typeof( SidebarOption ).Field( "warningCg" );

      private static void HintAvailableMission ( HUDScreenSelect __instance, SidebarOption ___missionsOption ) { try {
         if ( HasWarning( ___missionsOption ) ) return;
         var i = __instance.client.simulation.GetAgencyAvailableMissionSlots( __instance.agency );
         if ( i > 0 ) ___missionsOption.SetInfoActive( i );
      } catch ( Exception x ) { Err( x ); } }

      private static void HintJointMission ( HUDScreenSelect __instance, SidebarOption ___diplomacyOption ) { try {
         if ( HasWarning( ___diplomacyOption ) ) return;
         if ( __instance.client.simulation.CanAgencyGenerateJointMissionRequest( __instance.agency, out _ ) )
            ___diplomacyOption.SetInfoActive( 1 );
      } catch ( Exception x ) { Err( x ); } }

      private static void HideSpacepediaHint ( SidebarOption ___spacepediaOption ) { try {
         if ( HasWarning( ___spacepediaOption ) ) return;
         ___spacepediaOption.SetInfoActive( 0 );
      } catch ( Exception x ) { Err( x ); } }

      private static bool HasWarning ( SidebarOption opt ) => ( WarningCG?.GetValue( opt ) is Component cg ) && cg.gameObject.activeSelf;
   }
}