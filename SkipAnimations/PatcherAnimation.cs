using Astronautica.View;
using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZyMod.MarsHorizon.ClickReduction {

   internal class PatcherAnimation : ModPatcher {
      internal override void Apply () {
         if ( config.max_delay >= 0 ) {
            Patch( typeof( DelayExtension ).Method( "Delay", typeof( MonoBehaviour ), typeof( float ), typeof( Action ) ), prefix: nameof( SkipMonoTimeDelays ) );
            Patch( typeof( LaunchEventsScreen ).Method( "SkipLaunchCo" ).MoveNext(), transpiler: nameof( NoWait_SkipLaunchCo ) );
            if ( config.max_delay <= 0.5f ) {
               Patch( typeof( ClientViewer ).Method( "CleanupCinematicCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCinematicCoroutine ) );
               Patch( typeof( TitleScreen ).Method( "ContinueGameCo" ).MoveNext(), transpiler: nameof( NoWait_ContinueGameCo ) );
               Patch( typeof( AnimatorDelay ), "Start", prefix: nameof( RemoveWait_Animator ) );
            }
         }
         if ( config.remove_delays ) {
            Patch( typeof( ConstructionCompleteScreen ), "Wait", prefix: nameof( RemoveWait_CompleteScreen ) );
            Patch( typeof( LaunchDayScreen ), "Wait", prefix: nameof( RemoveWait_CompleteScreen ) );
            Patch( typeof( MissionGameplayScreen ), "WaitForSecondsSkippable", prefix: nameof( RemoveWait_WaitForSecondsSkippable ) );
            Patch( typeof( TweenSettingsExtensions ), "AppendInterval", prefix: nameof( RemoveWait_Tween ) );
            Patch( typeof( TweenSettingsExtensions ), "PrependInterval", prefix: nameof( RemoveWait_Tween ) );
         }
         if ( config.max_screen_fade >= 0 )
            foreach ( var m in typeof( Blackout ).Methods().Where( e => e.Name == "Fade" || e.Name == "FadeInOut" ) )
               Patch( m, prefix: nameof( RemoveWait_Blackout ) );
         if ( config.fast_launch ) {
            foreach ( var m in new string[] { "AstroInitialise", "InSequence", "LaunchReportSequence", "PartLevellingSequence" } )
               Patch( typeof( LaunchEventsScreen ), m, prefix: nameof( SpeedUpLaunch ) );
            Patch( typeof( LaunchEventsScreen ), "SkipPressed", postfix: nameof( SkipLaunchAnimation ) );
         }
         if ( config.skip_mission_intro )
            Patch( typeof( MissionGameplayScreen ), "RunMissionIntroductions", prefix: nameof( SkipPayloadDeploy ) );
         if ( config.fast_mission )
            Patch( typeof( MissionGameplayScreen ), "SetupReliabilityBar", postfix: nameof( SkipReliabilityFill ) );
         if ( config.swoosh_speed > 0 && config.swoosh_speed != 1 )
            Patch( typeof( MissionGameplayScene ), "AnimateSwooshEffects", prefix: nameof( SpeedUpMissionSwoosh ) );
         if ( config.fast_mission_result ) {
            Patch( typeof( MissionSummary ), "SkipOnly", transpiler: nameof( SpeedUpMissionSkip ) );
            Patch( typeof( MissionSummary ), "AnimatePhaseProgress", postfix: nameof( SpeedUpMissionSummary ), transpiler: nameof( SpeedUpPhaseProgress ) );
            Patch( typeof( MissionSummary ), "AnimateRewards", postfix: nameof( SpeedUpMissionSummary ), transpiler: nameof( SpeedUpRewards ) );
            Patch( typeof( MissionSummaryPhaseProgress ), "Animate", transpiler: nameof( SpeedUpPhaseAnimation ) );
         }
      }

      #region Remove delays
      private static void SkipMonoTimeDelays ( ref float duration, MonoBehaviour behaviour, Action callback ) {
         if ( duration <= config.max_delay ) return;
         Fine( "Removing {0}s delay of {1} on {2} {3}", duration, callback, behaviour?.GetType().Name, behaviour?.name );
         duration = config.max_delay;
      }
      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCinematicCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, config.max_delay, 2 );
      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, config.max_delay, 2 );
      private static IEnumerable< CodeInstruction > NoWait_SkipLaunchCo ( IEnumerable< CodeInstruction > codes ) {
         if ( config.max_delay < 2f ) codes = ReplaceFloat( codes, 2f, config.max_delay, 1 );
         if ( config.max_delay < 0.5f ) codes = ReplaceFloat( codes, 0.5f, config.max_delay, 1 );
         return codes;
      }
      private static IEnumerable< CodeInstruction > NoWait_ContinueGameCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, config.max_delay, 1 );
      private static void RemoveWait_Animator ( AnimatorDelay __instance ) {
         if ( __instance.maxDelay > config.max_delay ) Fine( "Removing {0}s delay from animator {1}", __instance.maxDelay, __instance.name );
         __instance.maxDelay = config.max_delay;
      }

      private static void RemoveWait_Blackout ( ref float ___tweenTime, ref float ___waitTime ) {
         if ( ___waitTime > config.max_screen_fade ) Fine( "Reduce {0}s screen fade to {1}.", ___waitTime, config.max_screen_fade );
         ___tweenTime = ___waitTime = config.max_screen_fade;
      }
      private static void RemoveWait_CompleteScreen ( ref float time ) => time = 0;
      private static void RemoveWait_WaitForSecondsSkippable ( ref float seconds ) => seconds = 0;
      private static void RemoveWait_Tween ( ref float interval ) => interval = 0;
      #endregion

      private static void SpeedUpLaunch ( ref float ___skipSpeedUp, ref bool ___canSkipTween ) { ___canSkipTween = true; ___skipSpeedUp = 100f; }
      private static void SkipLaunchAnimation ( ref bool __result ) => __result = true;

      private static bool SkipPayloadDeploy ( MissionGameplayScreen __instance ) { try {
         typeof( MissionGameplayScreen ).Method( "ContinueToGameplayKBAM" ).Run( __instance );
         typeof( MissionGameplayScreen ).Method( "OnTaskOutroCompleted" ).Invoke( __instance, new object[]{ null } );
         return false;
      } catch ( Exception x ) { return Err( x, true ); } }

      #region Fast Mission
      private static void SkipReliabilityFill ( RectTransform ___rollArea, TextSetter ___reliabilityText, float ___reliabilityResultTargetValue ) { try {
         ___rollArea.anchorMax = new Vector2( ___reliabilityResultTargetValue, 1f );
         ___reliabilityText.text = ___reliabilityText.text.Replace( " 0%", Math.Round( ___reliabilityResultTargetValue * 100 ) + "%" );
      } catch ( Exception x ) { Err( x ); } }
      private static void SpeedUpMissionSwoosh () => MissionGameplaySwooshEffect.swooshModifier = config.swoosh_speed;

      private static IEnumerable< CodeInstruction > SpeedUpMissionSkip ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 5f, 50f, 1 );
      private static IEnumerable< CodeInstruction > SpeedUpPhaseAnimation ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.75f, 0.2f, 1 );
      private static IEnumerable< CodeInstruction > SpeedUpPhaseProgress ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( codes, 0.75f, 0.2f, 1 ),  0.5f, 0.2f, 1 );
      private static IEnumerable< CodeInstruction > SpeedUpRewards ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( ReplaceFloat( codes, 0.5f, 0.2f, 3 ), 1f, 0.2f, 3 ), 1.5f, 0.2f, 1 );
      private static void SpeedUpMissionSummary ( Tween __result ) => __result.timeScale = 100f;
      #endregion

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