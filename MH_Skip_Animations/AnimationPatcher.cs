using Astronautica.View;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class AnimationPatcher : Patcher {

      private static Config config => SkipAnimations.config;

      internal void Apply () {
         //Patch( typeof( MonoBehaviour ).Method( "StartCoroutine", typeof( IEnumerator ) ), nameof( LogRoutine ) );
         if ( config.remove_delay ) {
            TryPatch( typeof( DelayExtension ).Method( "Delay", typeof( MonoBehaviour ), typeof( float ), typeof( Action ) ), nameof( SkipMonoTimeDelays ) );
            RemoveWaitForSeconds();
         }

         if ( LaunchSpeed != null )
            TryPatch( typeof( LaunchEventsScreen ), "AstroInitialise", null, nameof( SetLaunchSpeed ) );
         TryPatch( typeof( LaunchEventsScreen ), "SkipPressed", null, nameof( SkipLaunchAnimation ) );

         if ( MissionSpeed != null )
            TryPatch( typeof( MissionGameplayScene ), "PostInitialise", null, nameof( SetMissionSpeed ) );
         TryPatch( typeof( MissionGameplayScene ), "SkipPressed", null, nameof( SkipMissionAnimation ) );
      }

      private static void SkipMonoTimeDelays ( ref float duration, MonoBehaviour behaviour, Action callback ) {
         if ( duration <= 0.1f ) return;
         Fine( "Removing {0}s delay of {1} on {2} {3}", duration, callback, behaviour.GetType().Name, behaviour.name );
         duration = 0f;
      }

      private void RemoveWaitForSeconds () {
         TryPatch( typeof( ClientViewer ).Method( "CleanupCinematicCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCinematicCoroutine ) );
         TryPatch( typeof( ClientViewer ).Method( "CleanupCoroutine" ).MoveNext(), transpiler: nameof( NoWait_ClientViewer_CleanupCoroutine ) );
         TryPatch( typeof( LaunchEventsScreen ).Method( "SkipLaunchCo" ).MoveNext(), transpiler: nameof( NoWait_SkipLaunchCo ) );
         TryPatch( typeof( TitleScreen ).Method( "ContinueGameCo" ).MoveNext(), transpiler: nameof( NoWait_ContinueGameCo ) );
         TryPatch( typeof( AnimatorDelay ), "Start", nameof( RemoveWait_Animator ) );
         TryPatch( typeof( HUDObjectiveList ), "_Refresh", nameof( RemoveWait_ObjectiveList ) );
         foreach ( var m in typeof( Blackout ).Methods().Where( e => e.Name == "Fade" || e.Name == "FadeInOut" ) )
            TryPatch( m, nameof( RemoveWait_Blackout ) );
      }

      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCinematicCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, 0f, 2 );
      private static IEnumerable< CodeInstruction > NoWait_ClientViewer_CleanupCoroutine ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, 0f, 2 );
      private static IEnumerable< CodeInstruction > NoWait_SkipLaunchCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( ReplaceFloat( codes, 2f, 0f, 1 ), 0.5f, 0f, 1 );
      private static IEnumerable< CodeInstruction > NoWait_ContinueGameCo ( IEnumerable< CodeInstruction > codes )
         => ReplaceFloat( codes, 0.5f, 0f, 1 );

      private static void RemoveWait_Animator ( AnimatorDelay __instance ) => __instance.maxDelay = 0;
      private static void RemoveWait_ObjectiveList ( ref float ___listAnimWait, ref float ___listHideExtendedTime ) => ___listAnimWait = ___listHideExtendedTime = 0f;
      private static void RemoveWait_Blackout ( Blackout __instance ) => __instance.tweenTime = __instance.waitTime = 0f;

      private static FieldInfo LaunchSpeed = typeof( MissionGameplayScene ).Field( "skipSpeedUp" );
      private static void SetLaunchSpeed ( object __instance ) {
         Info( "Increasing launch screen skip speed" );
         LaunchSpeed?.SetValue( __instance, 100 );
      }

      private static void SkipLaunchAnimation ( object __instance, ref bool __result ) {
         __result = true;
      }

      private static FieldInfo MissionSpeed = typeof( MissionGameplayScene ).Field( "timelineSkipSpeedup" );
      private static void SetMissionSpeed ( object __instance ) {
         Info( "Increasing puzzle screen skip speed" );
         MissionSpeed?.SetValue( __instance, 100 );
      }

      private static void SkipMissionAnimation ( ref bool __result ) {
         __result = true;
      }

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