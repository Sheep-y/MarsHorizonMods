using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class CinematicPatcher : Patcher {

      private static Config config => SkipAnimations.config;

      internal void Apply () {
         if ( config.skip_intro )
            Patch( typeof( SplashDelayScene ), "Start", nameof( SkipSplash ) );
         if ( config.skip_all_cinematics || config.skip_seen_cinematics || config.SkipCinematics.Count > 0 ) {
            IsSkippableField = typeof( CinematicSceneController ).Field( "isSkippable" );
            if ( IsSkippableField != null )
               Patch( typeof( CinematicSceneController ), "GetInputDownSkip", null, nameof( SkipCinmatic ) );
            Patch( typeof( LaunchEventsScreen ), "SkipPressed", null, nameof( SkipLaunchCountdown ) );
         }
      }

      private static void SkipSplash () {
         Fine( "Waiting for splash screen." );
         Task.Run( async () => { try {
            int delay = 8, wait_count = 15_000 / delay;
            while ( SplashScreen.isFinished && wait_count-- > 0 ) await Task.Delay( delay );
            Info( SplashScreen.isFinished ? "Splash screen skip timeout" : "Skipping splash screen." );
            SplashScreen.Stop( SplashScreen.StopBehavior.StopImmediate );
         } catch ( Exception x ) { Err( x ); } } );
      }

      private static string lastCinematic;

      private static bool ShouldSkip ( string id ) {
         if ( config.skip_all_cinematics || config.SkipCinematics.Contains( id ) ) {
            if ( lastCinematic != id ) Info( "Skipping cinematic {0}", lastCinematic = id );
            return true;
         }
         if ( ! config.skip_seen_cinematics ) return false;
         Info( "Adding {0} to seen cinematics.", id );
         config.AddCinematic( id );
         return false;
      }

      private static FieldInfo IsSkippableField;
      private static HashSet< string > NonSkippable = new HashSet< string >();

      private static void SkipCinmatic ( ref bool __result, CinematicSceneController __instance ) { try {
         if ( __result ) return;
         var id = __instance.CurrentCinematicBindingType.ToString();
         if ( ! (bool) IsSkippableField.GetValue( __instance ) ) {
            if ( ! NonSkippable.Contains( id ) ) {
               Fine( "Non-skippable cinematic: {0}", id );
               NonSkippable.Add( id );
            }
            return;
         }
         __result = ShouldSkip( id );
      } catch ( Exception x ) { Err( x ); } }

      private static void SkipLaunchCountdown ( object __instance, ref bool __result ) {
         __result = ShouldSkip( "$" + __instance.GetType().Name );
      }

   }
}