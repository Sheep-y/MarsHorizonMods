﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
         if ( config.skip_intro || config.skip_all_cinematics || config.skip_seen_cinematics || config.SkipCinematics.Count > 0 )
            new CinematicPatcher().Apply();
         if ( config.remove_delays || config.skip_screen_fades || config.fast_launch || config.fast_mission )
            new AnimationPatcher().Apply();
      }
   }

   public class Config : IniConfig {

      [ Config( "[Cinematic]\r\n; Skip intro.  Default True." ) ]
      public bool skip_intro = true;
      [ Config( "Force skip all non-intro cinematics (mission control, launch, mission payload).  Override other cinematic skip settings if True.  Default False." ) ]
      public bool skip_all_cinematics = false;
      [ Config( "Add newly seen cinematics to skip_cinmatics (below).  Default true." ) ]
      public bool skip_seen_cinematics = true;
      [ Config( "Skip these cinematics, comma seprated.  Set to empty to reset.  Default starts with mission controls, launches, and earth flybys." ) ]
      public string skip_cinematics = "Earth_Launch_Failure,Earth_Launch_Failure_Large,Earth_Launch_Failure_Medium,Earth_Launch_Failure_Small,Earth_Launch_Intro,Earth_Launch_Intro_Large,Earth_Launch_Intro_Medium,Earth_Launch_Intro_Small,Earth_Launch_Outro,Earth_Launch_Success,Earth_Launch_Success_Large,Earth_Launch_Success_Medium,Earth_Launch_Success_Small,MissionControl_Intro,MissionControl_Success_Generic,MissionControl_Success_Milestone";

      [ Config( "[Animation]\r\n; Remove or reduce assorted delays between screens and actions.  Default true." ) ]
      public bool remove_delays = true;
      [ Config( "Skip screen fadings.  Default true." ) ]
      public bool skip_screen_fades = true;
      [ Config( "Skip launch countdown and speed up launch animations.  Default true." ) ]
      public bool fast_launch = true;
      [ Config( "Deploy payload in background, skip payload connection, skip task popup, and speed up mini-game animations.  Default true." ) ]
      public bool fast_mission = true;

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
            Info( "{0} cinematic(s) has been seen and will be skipped.", SkipCinematics.Count );
         }
      } catch ( Exception x ) { Err( x ); } }

      internal void AddCinematic ( string name ) { try {
         lock ( SkipCinematics ) {
            if ( SkipCinematics.Contains( name ) ) return;
            SkipCinematics.Add( name );
            object[] cinematic = SkipCinematics.OrderBy( e => e ).ToArray();
            skip_cinematics = new StringBuilder().AppendCsvLine( cinematic ).ToString();
         }
         Task.Run( () => { lock ( SkipCinematics ) Save(); } );
      } catch ( Exception x ) { Err( x ); } }
   }

}