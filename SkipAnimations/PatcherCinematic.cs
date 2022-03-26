using Astronautica.View;
using Astronautica.View.Cinematics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class PatcherCinematic : ModPatcher {
      internal void Apply () {
         if ( config.skip_intro )
            //TryPatch( typeof( SplashDelayScene ), "Start", prefix: nameof( SkipSplash ) );
            SkipSplash();
         if ( config.skip_all_cinematic || config.skip_seen_cinematic || config.skip_seen_cinematic_until_exit || config.SkipCinematics.Count > 0 )
            TryPatch( typeof( CinematicSceneController ), "GetInputDownSkip", postfix: nameof( SkipCinmatic ) );
         if ( Environment.UserName == "Sheepy" )
            TryPatch( typeof( CinematicSetupCreator ).Method( "TryGetValidSetup", typeof( CinematicSetup.ECategory ), typeof( CinematicData ), typeof( CinematicSetup ).MakeByRefType() ), postfix: nameof( LogCinematic ) );
      }

      private static void SkipSplash () {
         Fine( "Waiting for splash screen." );
         Task.Run( async () => { try {
            int delay = 8, wait_count = 30_000 / delay;
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

      private static readonly HashSet< string > NonSkippable = new HashSet< string >();
      private static readonly HashSet< string > TempSkip = new HashSet< string >();

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

      // Trying to tackle the "Can't find valid cinematic setup" error.
      private static void LogCinematic ( CinematicSetup.ECategory category, CinematicData cinematicData, CinematicSetup cinematicSetup, bool __result ) { try {
         var c = cinematicData;
         Fine( "{4} cinematic {2}:{3} for {0} {1}", category, c, cinematicSetup?.name, cinematicSetup?.SceneName, __result ? "Found" : "Not Found" );
         Fine( "Agency {0}, cId {1}, planet {2}, launchpad {3}, launch result {4}, mission {5}, mission state {6}, model {7}" +
               ", next planet {8}, payload {9}, phase {10}, task {11}, isShuttle {12}, isReusable {13}" +
               ", mission context {14}, crew {15}"
            , c.agency?.type, c.cinematicAssetId, c.currentPlanetaryBody, c.launchRequirement, c.launchState, c.mission?.template?.id, c.missionState, c.modelName
            , c.nextPlanetaryBody, c.payload?.id, c.phaseIndex, c.taskIndex, c.vehicle?.isShuttle, c.vehicle?.isReusable
            , c.mission.requestMissionContext?.localisationReference, c.mission?.template?.minCrew );
      } catch ( Exception x ) { Err( x ); } }
   }
}