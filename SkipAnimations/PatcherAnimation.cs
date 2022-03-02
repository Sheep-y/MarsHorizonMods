using Astronautica.View;
using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class PatcherAnimation : ModPatcher {

      internal void Apply () {
         if ( config.remove_delays ) {
            TryPatch( typeof( DelayExtension ).Method( "Delay", typeof( MonoBehaviour ), typeof( float ), typeof( Action ) ), nameof( SkipMonoTimeDelays ) );
            TryPatch( typeof( ClientViewer ).Method( "CleanupCinematicCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCinematicCoroutine ) );
            TryPatch( typeof( ClientViewer ).Method( "CleanupCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCoroutine ) );
            TryPatch( typeof( LaunchEventsScreen ).Method( "SkipLaunchCo" ).MoveNext(), transpiler: nameof( NoWait_SkipLaunchCo ) );

            TryPatch( typeof( TitleScreen ).Method( "ContinueGameCo" ).MoveNext(), transpiler: nameof( NoWait_ContinueGameCo ) );
            TryPatch( typeof( AnimatorDelay ), "Start", nameof( RemoveWait_Animator ) );
            TryPatch( typeof( HUDObjectiveList ), "_Refresh", nameof( RemoveWait_ObjectiveList ) );

            TryPatch( typeof( ConstructionCompleteScreen ), "Wait", nameof( RemoveWait_CompleteScreen ) );
            TryPatch( typeof( LaunchDayScreen ), "Wait", nameof( RemoveWait_CompleteScreen ) );
            TryPatch( typeof( MissionGameplayScreen ), "WaitForSecondsSkippable", nameof( RemoveWait_WaitForSecondsSkippable ) );
            TryPatch( typeof( TweenSettingsExtensions ), "AppendInterval", nameof( RemoveWait_Tween ) );
            TryPatch( typeof( TweenSettingsExtensions ), "PrependInterval", nameof( RemoveWait_Tween ) );
            foreach ( var m in typeof( DOTweenModuleUI ).Methods().Where( e => e.Name.StartsWith( "DO" ) ) )
               TryPatch( m, nameof( RemoveWait_TweenDo ) );
         }
         if ( config.skip_screen_fade )
            foreach ( var m in typeof( Blackout ).Methods().Where( e => e.Name == "Fade" || e.Name == "FadeInOut" ) )
               TryPatch( m, nameof( RemoveWait_Blackout ) );
         if ( config.fast_launch ) {
            foreach ( var m in new string[] { "AstroInitialise", "InSequence", "LaunchReportSequence", "PartLevellingSequence" } )
               TryPatch( typeof( LaunchEventsScreen ), m, nameof( SpeedUpLaunch ) );
            TryPatch( typeof( LaunchEventsScreen ), "SkipPressed", postfix: nameof( SkipLaunchAnimation ) );
         }
         if ( config.skip_mission_intro )
            TryPatch( typeof( MissionGameplayScreen ), "RunMissionIntroductions", nameof( SkipPayloadDeploy ) );
         if ( config.fast_mission ) {
            TryPatch( typeof( MissionGameplayScreen ), "AstroInitialise", postfix: nameof( SpeedUpMission ) );
            TryPatch( typeof( MissionGameplayScreen ), "ReliabilityRollAnim", nameof( SkipReliabilityFill ) );
            TryPatch( typeof( MissionGameplayScene ), "PostInitialise", nameof( SpeedUpMissionEffects ) );
            TryPatch( typeof( MissionGameplayScene ), "AnimateSwooshEffects", nameof( SpeedUpMissionSwoosh ) );
            TryPatch( typeof( MissionGameplayScene ), "PlayScreenEffect", nameof( SpeedUpMissionScreenEffect ) );
            TryPatch( typeof( MissionGameplayActionResourceElement ).Method( "Show", typeof( bool ) ), nameof( SkipResourceTween ) );
         }
         if ( config.fast_mission_result ) {
            TryPatch( typeof( MissionSummary ).Method( "SkipOnly" ), transpiler: nameof( SpeedUpMissionSkip ) );
            TryPatch( typeof( MissionSummary ).Method( "AnimatePhaseProgress" ), postfix: nameof( SpeedUpMissionSummary ), transpiler: nameof( SpeedUpPhaseProgress ) );
            TryPatch( typeof( MissionSummary ).Method( "AnimateRewards" ), postfix: nameof( SpeedUpMissionSummary ) );
         }
      }

      #region Remove delays
      private const float DELAY = 0.05f;

      private static void SkipMonoTimeDelays ( ref float duration, MonoBehaviour behaviour, Action callback ) {
         if ( duration <= 0.1f ) return;
         Fine( "Removing {0}s delay of {1} on {2} {3}", duration, callback, behaviour?.GetType().Name, behaviour?.name );
         duration = DELAY;
      }

      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCinematicCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, DELAY, 2 );
      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, DELAY, 2 );
      private static IEnumerable< CodeInstruction > NoWait_SkipLaunchCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( codes, 2f, 0f, 1 ), 0.5f, DELAY, 1 );
      private static IEnumerable< CodeInstruction > NoWait_ContinueGameCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, DELAY, 1 );

      private static void RemoveWait_Animator ( AnimatorDelay __instance ) {
         if ( __instance.maxDelay > 0 ) Fine( "Removing {0}s delay from animator {1}", __instance.maxDelay, __instance.name );
         __instance.maxDelay = DELAY;
      }
      private static void RemoveWait_ObjectiveList ( ref float ___listAnimWait, ref float ___listHideExtendedTime ) => ___listAnimWait = ___listHideExtendedTime = 0;
      private static void RemoveWait_Blackout ( ref float ___tweenTime, ref float ___waitTime ) {
         if ( ___waitTime > 0 ) Fine( "Removing {0}s screen fade.", ___waitTime );
         ___tweenTime = ___waitTime = 0f;
      }
      private static void RemoveWait_CompleteScreen ( ref float time ) => time = DELAY;
      private static void RemoveWait_WaitForSecondsSkippable ( ref float seconds ) => seconds = DELAY;
      private static void RemoveWait_Tween ( ref float interval ) => interval = DELAY;
      private static void RemoveWait_TweenDo ( ref float duration ) => duration = 0f;
      #endregion

      private static void SpeedUpLaunch ( ref float ___skipSpeedUp, ref bool ___canSkipTween ) { ___canSkipTween = true; ___skipSpeedUp = 100f; }
      private static void SkipLaunchAnimation ( ref bool __result ) => __result = true;

      private static bool SkipPayloadDeploy ( MissionGameplayScreen __instance ) { try {
         typeof( MissionGameplayScreen ).Method( "ContinueToGameplayKBAM" ).Run( __instance );
         typeof( MissionGameplayScreen ).Method( "OnTaskOutroCompleted" ).Invoke( __instance, new object[]{ null } );
         return false;
      } catch ( Exception x ) { return Err( x, true ); } }

      #region Fast Mission
      private static void SpeedUpMission ( MissionGameplayScreen __instance, ref float ___timelineSkipSpeedup, ref bool ___isSkippable, ref bool ___initialWait ) {
         Fine( "Mission screen initiated." );
         ___timelineSkipSpeedup = 50f;
         ___isSkippable = true;
         ___initialWait = false;
         __instance.baseReliabilityRollSpeed = 100f;
         __instance.skippedReliabilityRollSpeed = 100f;
      }
      private static void SpeedUpMissionEffects ( MissionGameplayScene __instance ) => typeof( MissionGameplayScene ).Field( "skipSpeed" ).SetValue( __instance, 100f );
      private static void SpeedUpMissionSwoosh () => MissionGameplaySwooshEffect.swooshModifier = 5f;
      private static void SpeedUpMissionScreenEffect ( MissionGameplayScreenEffect effect ) => effect.PlaybackSpeed = 10f;
      private static void SkipReliabilityFill ( RectTransform ___rollArea, float ___reliabilityResultTargetValue ) => ___rollArea.anchorMax = new Vector2( ___reliabilityResultTargetValue - 0.02f, 1f );
      private static void SkipResourceTween ( ref bool tween ) => tween = false;
      #endregion

      private static IEnumerable< CodeInstruction > SpeedUpMissionSkip ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 5f, 50f, 1 );
      private static IEnumerable< CodeInstruction > SpeedUpPhaseProgress ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( codes, 0.75f, 0.1f, 1 ),  0.5f, 0.1f, 1 );
      private static IEnumerable< CodeInstruction > SpeedUpRewards ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( ReplaceFloat( codes, 0.5f, 0.05f, 3 ), 1f, 0.1f, 3 ), 1.5f, 0.15f, 1 );
      private static void SpeedUpMissionSummary ( Tween __result ) => __result.timeScale = 100f;
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