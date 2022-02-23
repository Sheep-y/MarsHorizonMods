using Astronautica.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   public class SkipAnimations : RootMod {

      public static Config config = new Config();

      public static void Main () => new SkipAnimations().Initialize();

      protected override string GetAppDataDir () {
         var path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
         if ( string.IsNullOrEmpty( path ) ) return null;
         return Path.Combine( Directory.GetParent( path ).FullName, "LocalLow", "Auroch Digital", "Mars Horizon" );
      }

      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         config.Load();
         new CinematicPatcher().Apply();
      }
   }

   public class Config : IniConfig {

      [ Config( "[Cinematic]\r\n; Skip intro.  Default True." ) ]
      public bool skip_intro = true;
      [ Config( "Force skip all non-intro cinematics (mission control, launch, mission payload).  Override other cinematic skip settings if True.  Default False." ) ]
      public bool skip_all_cinematics = false;
      [ Config( "Add newly seen cinematics to skip_cinmatics (below).  Default True." ) ]
      public bool skip_seen_cinematics = true;
      [ Config( "Skip these cinematics, comma seprated.  Set to empty to reset.  Default starts with mission controls, launches, and earth flybys." ) ]
      public string skip_cinematics = "Earth_Launch_Failure,Earth_Launch_Failure_Large,Earth_Launch_Failure_Medium,Earth_Launch_Failure_Small,Earth_Launch_Intro,Earth_Launch_Intro_Large,Earth_Launch_Intro_Medium,Earth_Launch_Intro_Small,Earth_Launch_Outro,Earth_Launch_Success,Earth_Launch_Success_Large,Earth_Launch_Success_Medium,Earth_Launch_Success_Small,MissionControl_Intro,MissionControl_Success_Generic,MissionControl_Success_Milestone";

      [ Config( "\r\n; Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200223;

      internal HashSet< string > SkipCinematics { get; } = new HashSet< string >();

      public override void Load ( object subject, string path ) { try {
         base.Load( subject, path );
         lock ( SkipCinematics ) {
            SkipCinematics.Clear();
            if ( skip_cinematics != null )
               foreach ( var e in new StringReader( skip_cinematics ).ReadCsvRow() )
                  if ( ! string.IsNullOrWhiteSpace( e ) )
                     SkipCinematics.Add( e.Trim() );
         }
      } catch ( Exception x ) { Err( x ); } }

      internal void AddCinematic ( string name ) { try {
         lock ( SkipCinematics ) {
            if ( SkipCinematics.Contains( name ) ) return;
            SkipCinematics.Add( name );
            skip_cinematics = new StringWriter().WriteCsvLine( SkipCinematics.OrderBy( e => e ).ToArray< object >() ).ToString();
         }
         Task.Run( () => { lock ( SkipCinematics ) Save(); } );
      } catch ( Exception x ) { Err( x ); } }
   }

   internal class CinematicPatcher : Patcher {

      private static Config config => SkipAnimations.config;

      internal void Apply () {
         if ( config.skip_intro )
            Patch( typeof( SplashDelayScene ), "Start", nameof( SkipSplash ) );
         IsSkippableField = typeof( CinematicSceneController ).Field( "isSkippable" );
         if ( IsSkippableField != null )
            Patch( typeof( CinematicSceneController ), "GetInputDownSkip", null, nameof( SkipCinmatic ) );
         if ( RootMod.Log.LogLevel == System.Diagnostics.TraceLevel.Verbose )
            Patch( typeof( MonoBehaviour ).Method( "StartCoroutine", typeof( IEnumerator ) ), nameof( LogRoutine ) );
      }

      private static FieldInfo IsSkippableField;
      private static HashSet< string > NonSkippable = new HashSet< string >();

      private static void LogRoutine ( IEnumerator routine ) {
         var name = routine.GetType().FullName;
         if ( name.EndsWith( ".ColorTween]" ) ) return;
         Fine( "Routine {0}", name );
      }

      private static void SkipSplash ( ref IEnumerator __result ) {
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