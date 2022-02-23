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
         IsSkippableField = typeof( CinematicSceneController ).Field( "isSkippable" );
         if ( IsSkippableField != null )
            Patch( typeof( CinematicSceneController ), "GetInputDownSkip", null, nameof( SkipCinmatic ) );
      }

      private static FieldInfo IsSkippableField;
      private static HashSet< string > NonSkippable = new HashSet< string >();

      private static void SkipSplash () {
         Fine( "Waiting for splash screen." );
         Task.Run( async () => { try {
            int delay = 8, wait_count = 15_000 / delay;
            while ( SplashScreen.isFinished && wait_count-- > 0 ) await Task.Delay( delay );
            Info( SplashScreen.isFinished ? "Splash screen skip timeout" : "Skipping splash screen." );
            SplashScreen.Stop( SplashScreen.StopBehavior.StopImmediate );
         } catch ( Exception x ) { Err( x ); } } );
      }

      private static void SkipCinmatic ( ref bool __result, CinematicSceneController __instance ) { try {
         if ( __result ) return;
         var id = __instance.CurrentCinematicBindingType.ToString();

         var isSkippable = (bool) IsSkippableField.GetValue( __instance );
         if ( ! isSkippable ) {
            if ( ! NonSkippable.Contains( id ) ) {
               Fine( "Non-skippable cinematic: {0}", id );
               NonSkippable.Add( id );
            }
            return;
         }

         if ( config.skip_all_cinematics || config.SkipCinematics.Contains( id ) )
            __result = true;
         else if ( config.skip_seen_cinematics ) {
            Info( "Adding {0} to seen cinematics.", id );
            config.AddCinematic( id );
         }

         if ( __result ) Info( "Skipping cinematic {0}", id );

      } catch ( Exception x ) { Err( x ); } }
   }
}