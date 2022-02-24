using Astronautica.View;
using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class AnimationPatcher : Patcher {

      private static Config config => SkipAnimations.config;

      internal void Apply () {
         if ( config.remove_delays ) {
            TryPatch( typeof( DelayExtension ).Method( "Delay", typeof( MonoBehaviour ), typeof( float ), typeof( Action ) ), nameof( SkipMonoTimeDelays ) );
            TryPatch( typeof( ClientViewer ).Method( "CleanupCinematicCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCinematicCoroutine ) );
            TryPatch( typeof( ClientViewer ).Method( "CleanupCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCoroutine ) );
            TryPatch( typeof( LaunchEventsScreen ).Method( "SkipLaunchCo" ).MoveNext(), transpiler: nameof( NoWait_SkipLaunchCo ) );

            TryPatch( typeof( TitleScreen ).Method( "ContinueGameCo" ).MoveNext(), transpiler: nameof( NoWait_ContinueGameCo ) );
            TryPatch( typeof( AnimatorDelay ), "Start", nameof( RemoveWait_Animator ) );
            TryPatch( typeof( HUDObjectiveList ), "_Refresh", nameof( RemoveWait_ObjectiveList ) );

            TryPatch( typeof( ConstructionCompleteScreen ), "Wait", null, nameof( RemoveWait_CompleteScreen ) );
            TryPatch( typeof( LaunchDayScreen ), "Wait", null, nameof( RemoveWait_CompleteScreen ) );
            TryPatch( typeof( MissionGameplayScreen ), "WaitForSecondsSkippable", null, nameof( RemoveWait_WaitForSecondsSkippable ) );
            TryPatch( typeof( TweenSettingsExtensions ), "AppendInterval", null, nameof( RemoveWait_Tween ) );
            TryPatch( typeof( TweenSettingsExtensions ), "PrependInterval", null, nameof( RemoveWait_Tween ) );
         }
         if ( config.skip_screen_fades )
            foreach ( var m in typeof( Blackout ).Methods().Where( e => e.Name == "Fade" || e.Name == "FadeInOut" ) )
               TryPatch( m, nameof( RemoveWait_Blackout ) );
         if ( config.fast_launch ) {
            foreach ( var m in new string[] { "AstroInitialise", "InSequence", "LaunchReportSequence", "PartLevellingSequence" } )
               TryPatch( typeof( LaunchEventsScreen ), m, null, nameof( SpeedUpLaunch ) );
            TryPatch( typeof( LaunchEventsScreen ), "SkipPressed", null, nameof( SkipLaunchAnimation ) );
         }
         if ( config.fast_mission ) {
            TryPatch( typeof( MissionGameplayScreen ), "RunMissionIntroductions", nameof( SkipPayloadDeploy ) );
            TryPatch( typeof( MissionGameplayScreen ), "OnTaskIntroCompleted", null, nameof( SkipMissionIntro ) );
            TryPatch( typeof( MissionGameplayScreen ), "AstroInitialise", null, nameof( SpeedUpMission ) );
            TryPatch( typeof( MissionGameplayScreen ), "OnPhaseIntroCompleted", null, nameof( Log1 ) );
            TryPatch( typeof( MissionGameplayScreen ), "OnTaskIntroCompleted", null, nameof( Log2 ) );
            TryPatch( typeof( MissionGameplayScreen ), "OnTaskOutroCompleted", null, nameof( Log3 ) );
            TryPatch( typeof( MissionGameplayScreen ), "SkipCurrentTimeline", null, nameof( Log4 ) );
            TryPatch( typeof( MissionGameplayScreen ), "ContinueToGameplayKBAM", null, nameof( Log5 ) );
            TryPatch( typeof( MissionGameplayScene ), "PostInitialise", nameof( SpeedUpMissionEffects ) );
            TryPatch( typeof( MissionGameplayScene ), "AnimateSwooshEffects", nameof( SpeedUpMissionSwoosh ) );
            TryPatch( typeof( MissionGameplayScene ), "PlayScreenEffect", nameof( SpeedUpMissionScreenEffect ) );
         }
      }

      private static void Log1 ( MissionGameplayScreen __instance ) => Info( "OnPhaseIntroCompleted {0}", __instance.GetMode<MissionGameplayScreen.Mode>() );
      private static void Log2 ( MissionGameplayScreen __instance ) => Info( "OnTaskIntroCompleted {0}", __instance.GetMode<MissionGameplayScreen.Mode>() );
      private static void Log3 ( MissionGameplayScreen __instance ) => Info( "OnTaskOutroCompleted {0}", __instance.GetMode<MissionGameplayScreen.Mode>() );
      private static void Log4 ( MissionGameplayScreen __instance ) => Info( "SkipCurrentTimeline {0}", __instance.GetMode<MissionGameplayScreen.Mode>() );
      private static void Log5 ( MissionGameplayScreen __instance ) => Info( "ContinueToGameplayKBAM {0}", __instance.GetMode<MissionGameplayScreen.Mode>() );

      private static void SkipMonoTimeDelays ( ref float duration, MonoBehaviour behaviour, Action callback ) {
         if ( duration <= 0.1f ) return;
         Fine( "Removing {0}s delay of {1} on {2} {3}", duration, callback, behaviour?.GetType().Name, behaviour?.name );
         duration = 0f;
      }

      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCinematicCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, 0f, 2 );
      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, 0f, 2 );
      private static IEnumerable< CodeInstruction > NoWait_SkipLaunchCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( codes, 2f, 0f, 1 ), 0.5f, 0f, 1 );
      private static IEnumerable< CodeInstruction > NoWait_ContinueGameCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, 0f, 1 );

      private static void RemoveWait_Animator ( AnimatorDelay __instance ) {
         if ( __instance.maxDelay > 0 ) Fine( "Removing {0}s delay from animator {1}", __instance.maxDelay, __instance.name );
         __instance.maxDelay = 0;
      }
      private static void RemoveWait_ObjectiveList ( ref float ___listAnimWait, ref float ___listHideExtendedTime ) => ___listAnimWait = ___listHideExtendedTime = 0;
      private static void RemoveWait_Blackout ( ref float ___tweenTime, ref float ___waitTime ) {
         if ( ___waitTime > 0 ) Fine( "Removing {0}s screen fade.", ___waitTime );
         ___tweenTime = ___waitTime = 0;
      }
      private static void RemoveWait_WaitForSecondsSkippable ( ref float seconds ) => seconds = 0;
      private static void RemoveWait_Tween ( ref float interval ) => interval = 0;
      private static void RemoveWait_CompleteScreen ( ref float time ) => time = 0;

      private static void SpeedUpLaunch ( ref float ___skipSpeedUp, ref bool ___canSkipTween ) { ___canSkipTween = true; ___skipSpeedUp = 100f; }
      private static void SkipLaunchAnimation ( ref bool __result ) => __result = true;

      private static void SpeedUpMission ( MissionGameplayScreen __instance, ref float ___timelineSkipSpeedup, ref bool ___isSkippable, ref bool ___initialWait ) {
         Fine( "Mission screen initiated." );
         ___timelineSkipSpeedup = 100f;
         ___isSkippable = true;
         ___initialWait = false;
         __instance.baseReliabilityRollSpeed = 100f;
         __instance.skippedReliabilityRollSpeed = 100f;
      }
      private static void SpeedUpMissionEffects ( MissionGameplayScene __instance ) {
         typeof( MissionGameplayScene ).Field( "skipSpeed" ).SetValue( __instance, 100f );
      }
      private static void SpeedUpMissionSwoosh () {
         MissionGameplaySwooshEffect.swooshModifier = 100f;
      }
      private static void SpeedUpMissionScreenEffect ( MissionGameplayScreenEffect effect ) {
         effect.PlaybackSpeed = 100f;
      }

      private static void SkipPayloadDeploy ( MissionGameplayScreen __instance ) { try {
         Info( "Skipping mission intro." );
         var type = typeof( MissionGameplayScreen );
         var active = type.Field( "activePlayable" );
         PlayableDirector Playable () => active.GetValue( __instance ) as PlayableDirector;
         __instance.DelayWhile( () => Playable()?.state != PlayState.Playing, () => type.Method( "SkipCurrentTimeline" ).Run( __instance ) );
      } catch ( Exception x ) { Err( x ); } }

      private static void SkipMissionIntro ( MissionGameplayScreen __instance ) { try {
         typeof( MissionGameplayScreen ).Method( "ContinueToGameplayKBAM" ).Run( __instance );
      } catch ( Exception x ) { Err( x ); } }

      #region OpCode Replacement
      private static IEnumerable< CodeInstruction > ReplaceFloat ( IEnumerable< CodeInstruction > codes, float from, float to, int expected_count )
         => ReplaceOperand( codes, "ldc.r4", from, to, expected_count );

      private static IEnumerable< CodeInstruction > ReplaceOperand < T > ( IEnumerable< CodeInstruction > codes, string opcode, object from, T to, int expected_count ) {
         bool IsTarget ( CodeInstruction code ) => code.opcode.Name == opcode && Equals( code.operand, from );
         var list = codes.ToArray();
         var actual_count = list.Count( IsTarget );
         if ( actual_count != expected_count ) {
            Warn( "Mismatch when replacing {0} from {1} to {2}: Expected {3} matches, Found {4}", opcode, from, to, expected_count, actual_count );
         } else {
            foreach ( var code in list )
               if ( IsTarget( code ) ) code.operand = to;
            Fine( "Replaced {0} from {1} to {2} ({3} matches)", opcode, from, to, actual_count );
         }
         return list;
      }
      #endregion
   }
}