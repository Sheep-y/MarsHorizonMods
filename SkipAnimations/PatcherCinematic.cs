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

   internal class PatcherCinematic : ModPatcher {

      internal void Apply () {
         if ( config.skip_intro )
            TryPatch( typeof( SplashDelayScene ), "Start", nameof( SkipSplash ) );
         if ( config.skip_all_cinematic || config.skip_seen_cinematic || config.skip_seen_cinematic_until_exit || config.SkipCinematics.Count > 0 )
            TryPatch( typeof( CinematicSceneController ), "GetInputDownSkip", postfix: nameof( SkipCinmatic ) );
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
         if ( lastCinematic == id ) return false;
         lastCinematic = id;
         if ( config.SkipCinematics.Contains( id ) || config.skip_all_cinematic || TempSkip.Contains( id ) ) {
            Info( "Skipping cinematic {0}", id );
            return true;
         }
         if ( config.skip_seen_cinematic_until_exit ) {
            Info( "Adding {0} to temporary skip list until exit game.", id );
            TempSkip.Add( id );
         }
         if ( config.skip_seen_cinematic ) {
            Info( "Adding {0} to seen cinematics.", id );
            config.AddCinematic( id );
         } else
            Info( "Allowing cinematic {0}", id );
         return false;
      }

      private static HashSet< string > NonSkippable = new HashSet< string >();
      private static HashSet< string > TempSkip = new HashSet< string >();

      private static void SkipCinmatic ( ref bool __result, CinematicSceneController __instance, bool ___isSkippable ) { try {
         if ( __result ) return;
         var id = __instance.CurrentCinematicBindingType.ToString();
         if ( ! ___isSkippable ) {
            if ( ! NonSkippable.Contains( id ) ) {
               Fine( "Non-skippable cinematic: {0}", id );
               NonSkippable.Add( id );
            }
            return;
         }
         __result = ShouldSkip( id );
      } catch ( Exception x ) { Err( x ); } }

   }
}