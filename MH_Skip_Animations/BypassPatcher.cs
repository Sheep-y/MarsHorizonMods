using Astronautica;
using Astronautica.View;
using Astronautica.View.MissionNotifications;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class BypassPatcher : Patcher {

      private static Config config => SkipAnimations.config;

      internal void Apply () {
         if ( config.bypass_fullscreen_notices )
            TryPatch( typeof( ClientViewer ).Method( "ShowMissionNotifications", typeof( NotificationCache ), typeof( bool ) ), nameof( BypassFullScreenNotices ) );
         if ( config.bypass_popups_notices )
            TryPatch( typeof( ClientViewer ), "UpdateNotifications", nameof( BypassPopupNotices ) );
         if ( config.auto_pass_normal_actions )
            TryPatch( typeof( MissionGameplayScreen ), "SpawnEventPopup", postfix: nameof( BypassNormalAction ) );
      }

      private static void BypassFullScreenNotices ( NotificationCache missionNotifications ) { try {
         if ( missionNotifications == null ) return;
         while ( missionNotifications.GetNext( out _ ) != null ) ;
      } catch ( Exception x ) { Err( x ); } }

      private static void BypassPopupNotices ( ClientViewer __instance ) { try {
         foreach ( var e in __instance.agency.notifications )
            if ( e.type != Data.Notification.EType.MarsPreparationUnlocked ) 
               e.resolved = true;
      } catch ( Exception x ) { Err( x ); } }

      private static void BypassNormalAction ( Data.MissionEvent @event, Button ___ignoreButton ) { try {
         if ( @event != null ) return;
         Task.Run( async () => {
            await Task.Delay( 50 );
            ___ignoreButton.OnPointerClick( new PointerEventData( EventSystem.current ) );
         } );
      } catch ( Exception x ) { Err( x ); } }
   }
}