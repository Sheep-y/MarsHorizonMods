using Astronautica;
using Astronautica.View;
using Astronautica.View.MissionNotifications;
using HarmonyLib;
using System;
using System.Collections;
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
         if ( config.auto_pass_normal_launch )
            TryPatch( typeof( LaunchEventsScreen ), "EndLaunchCinematics", postfix: nameof( BypassNormalLaunch ) );
         if ( config.auto_pass_empty_levelup )
            TryPatch( typeof( LaunchEventsScreen ), "PartLevellingSequence", postfix: nameof( BypassNoLevelUp ) );
         if ( config.auto_pass_normal_action )
            TryPatch( typeof( MissionGameplayScreen ), "SpawnEventPopup", postfix: nameof( BypassNormalAction ) );

         TryPatch( typeof( LaunchEventsScreen ), "Continue", nameof( LogC1 ), nameof( LogC2 ) );
         //TryPatch( typeof( LaunchEventsScreen ), "Continue", nameof( LogMode ) );
         TryPatch( typeof( LaunchEventsScreen ), "EndLaunchCinematics", nameof( LogELC1 ) , nameof( LogELC2 ) );
         TryPatch( typeof( LaunchEventsScreen ), "LaunchReportTweens", nameof( LogLRT1 ), nameof( LogLRT1 ) );
         TryPatch( typeof( LaunchEventsScreen ), "PartLevellingSequence", nameof( LogLv1 ), nameof( LogLv2 ) );
         TryPatch( typeof( LaunchEventsScreen ), "RefreshEvents", nameof( LogRE1 ), nameof( LogRE2 ) );
         TryPatch( typeof( LaunchEventsScreen ), "RefreshButtonState", nameof( LogRBS1 ), nameof( LogRBS2 ) );
         TryPatch( typeof( LaunchEventsScreen ), "SetupEventsReport", nameof( LogSER1 ), nameof( LogSER2 ) );
      }

      private static void LogC1 ( LaunchEventsScreen __instance ) => DoLog( "Continue 1", __instance );
      private static void LogC2 ( LaunchEventsScreen __instance ) => DoLog( "Continue 2", __instance );
      private static void LogMode ( LaunchEventsScreen __instance ) => __instance.StartCoroutine( Ping( __instance ) );
      private static IEnumerator Ping ( LaunchEventsScreen __instance )  {
         while ( true ) {
            DoLog( "Ping", __instance );
            yield return null;
         }
      }
      private static void LogELC1 ( LaunchEventsScreen __instance ) => DoLog( "EndLaunchCinematics 1", __instance );
      private static void LogELC2 ( LaunchEventsScreen __instance ) => DoLog( "EndLaunchCinematics 2", __instance );
      private static void LogLRT1 ( LaunchEventsScreen __instance ) => DoLog( "LaunchReportTweens 1", __instance );
      private static void LogLRT2 ( LaunchEventsScreen __instance ) => DoLog( "LaunchReportTweens 2", __instance );
      private static void LogLv1 ( LaunchEventsScreen __instance ) => DoLog( "PartLevellingSequence 1", __instance );
      private static void LogLv2 ( LaunchEventsScreen __instance ) => DoLog( "PartLevellingSequence 2", __instance );
      private static void LogRE1 ( LaunchEventsScreen __instance ) => DoLog( "RefreshEvents 1", __instance );
      private static void LogRE2 ( LaunchEventsScreen __instance ) => DoLog( "RefreshEvents 2", __instance );
      private static void LogRBS1 ( LaunchEventsScreen __instance ) => DoLog( "RefreshButtonState 1", __instance );
      private static void LogRBS2 ( LaunchEventsScreen __instance ) => DoLog( "RefreshButtonState 2", __instance );
      private static void LogSER1 ( LaunchEventsScreen __instance ) => DoLog( "SetupEventsReport 1", __instance );
      private static void LogSER2 ( LaunchEventsScreen __instance ) => DoLog( "SetupEventsReport 2", __instance );
      private static void DoLog ( string msg, LaunchEventsScreen ___instance ) => Info( "{0} - {1} {2}", msg, ___instance.mode, typeof( LaunchEventsScreen ).Field( "activeTween" ).GetValue( ___instance ) ?? "null" );

      private static void BypassFullScreenNotices ( NotificationCache missionNotifications ) { try {
         if ( missionNotifications == null ) return;
         var i = 0;
         while ( missionNotifications.GetNext( out _ ) != null ) i++;
         if ( i > 0 ) Info( "Skipped {0} full screen notifications.", i );
      } catch ( Exception x ) { Err( x ); } }

      private static void BypassPopupNotices ( ClientViewer __instance ) { try {
         int i = 0, turn = __instance.client.simulation.universe.turn;
         foreach ( var e in __instance.agency.notifications )
            if ( ! e.resolved && e.turn == turn && e.type != Data.Notification.EType.MarsPreparationUnlocked ) {
               e.resolved = true;
               i++;
            }
         if ( i > 0 ) Info( "Skipped {0} popup notifications.", i );
      } catch ( Exception x ) { Err( x ); } }

      private static void BypassNormalLaunch ( LaunchEventsScreen __instance, string ___selectedEventId ) { try {
         Fine( "Launch mode {0}, event {1}", __instance.mode, __instance.Sim.GetLaunchEvent( ___selectedEventId ).type );
         if ( __instance.Sim.GetLaunchEvent( ___selectedEventId ).type != Mission.LaunchResult.LaunchEvent.Type.None ) return;
         __instance.StartCoroutine( BypassLanuchReport( __instance, "uneventful launch" ) );
      } catch ( Exception x ) { Err( x ); } }

      private static void BypassNoLevelUp ( LaunchEventsScreen __instance, AutoLocalise ___partExperienceText ) { try {
         Fine( "Launch mode {0}, level up tag {1}", __instance.mode, ___partExperienceText.tag );
         if ( ! string.Equals( ___partExperienceText.tag, "Launch_Screen_Experience_None_Desc", StringComparison.InvariantCultureIgnoreCase ) ) return;
         __instance.StartCoroutine( BypassLanuchReport( __instance, "empty level up report" ) );
      } catch ( Exception x ) { Err( x ); } }

      private static IEnumerator BypassLanuchReport ( LaunchEventsScreen __instance, string msg ) {
         var type = typeof( LaunchEventsScreen );
         var tweenField = type.Field( "activeTween" );
         while ( tweenField.GetValue( __instance ) == null ) yield return null;
         Fine( "Screen switch started." );
         while ( tweenField.GetValue( __instance ) != null ) yield return null;
         Fine( "Screen switch ended." );
         if ( ( type.Field( "autoResolveButton" ).GetValue( __instance ) as Button )?.gameObject.activeSelf == true ) {
            Fine( "Auto resolve avaliable.  Aborting bypass." );
            yield break;
         }
         Info( "Auto bypassing {0}.", msg );
         type.Method( "Continue" ).Run( __instance );
      }

      private static void BypassNormalAction ( Data.MissionEvent @event, Button ___ignoreButton ) { try {
         if ( @event != null ) return;
         Task.Run( async () => { try {
            await Task.Delay( 50 );
            Info( "Auto-bypassing uneventful action." );
            ___ignoreButton.OnPointerClick( new PointerEventData( EventSystem.current ) );
            //ExecuteEvents.Execute( ___ignoreButton.gameObject, new PointerEventData( EventSystem.current ), ExecuteEvents.pointerClickHandler );
         } catch ( Exception x ) { Err( x ); } } );
      } catch ( Exception x ) { Err( x ); } }
   }
}