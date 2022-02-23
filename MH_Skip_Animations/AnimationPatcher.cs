using Astronautica.View;
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
         if ( config.remove_delay )
            TryPatch( typeof( DelayExtension ).Method( "Delay", typeof( MonoBehaviour ), typeof( float ), typeof( Action ) ), nameof( SkipMonoTimeDelays ) );

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

      private static FieldInfo LaunchSpeed = typeof( MissionGameplayScene ).Field( "skipSpeedUp" );
      private static void SetLaunchSpeed ( object __instance ) {
         Info( "Increasing launch screen animation speed" );
         LaunchSpeed?.SetValue( __instance, 100 );
      }

      private static void SkipLaunchAnimation ( object __instance, ref bool __result ) {
         __result = true;
      }

      private static FieldInfo MissionSpeed = typeof( MissionGameplayScene ).Field( "timelineSkipSpeedup" );
      private static void SetMissionSpeed ( object __instance ) {
         Info( "Increasing puzzle screen animation speed" );
         MissionSpeed?.SetValue( __instance, 100 );
      }

      private static void SkipMissionAnimation ( ref bool __result ) {
         __result = true;
      }

   }
}