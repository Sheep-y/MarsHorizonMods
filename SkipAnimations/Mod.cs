using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace ZyMod.MarsHorizon.SkipAnimations {
   [ BepInPlugin( "Zy.MarsHorizon.SkipAnimations", "Skip Animations", "0.0.2022.0326" ) ]
   public class BIE_Mod : BaseUnityPlugin {
      private void Awake() { BepInUtil.Setup( this, ModPatcher.config ); Mod.Main(); }
      private void OnDestroy() => BepInUtil.Unbind();
   }

   [ EnableReloading ] public static class UMM_Mod {
      public static void Load ( UnityModManager.ModEntry entry ) => UMMUtil.Init( entry, typeof( Mod ) );
   }

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "SkipAnimations";
      public static void Main () => new Mod().Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         if ( config.skip_intro || config.skip_all_cinematic || config.skip_seen_cinematic || config.skip_seen_cinematic_until_exit || config.SkipCinematics.Count > 0 )
            ActivatePatcher( typeof( PatcherCinematic ) );
         if ( config.max_delay >= 0 || config.remove_delays || config.max_screen_fade >= 0 || config.skip_mission_intro ||
               config.fast_launch || config.fast_mission || config.fast_mission_result )
            ActivatePatcher( typeof( PatcherAnimation ) );
         if ( config.bypass_fullscreen_notices || config.bypass_popups_notices || config.auto_pass_normal_action )
            ActivatePatcher( typeof( PatcherBypass ) );
      }
   }

   internal abstract class ModPatcher : MarsHorizonPatcher {
      internal static readonly Config config = new Config();
   }

   internal class Config : IniConfig {
      [ Config( "\r\n[Cinematic]" ) ]
      [ Config( "Skip intro.  Default True." ) ]
      public bool skip_intro = true;
      [ Config( "Force skip all non-intro cinematics (mission control, launch, mission payload).  Override other cinematic skip settings if True.  Default False." ) ]
      public bool skip_all_cinematic = false;
      [ Config( "Add newly seen cinematics to skip_cinmatics (below).  Default False." ) ]
      public bool skip_seen_cinematic = false;
      [ Config( "Skip seen cinematics until game is closed.  Default True." ) ]
      public bool skip_seen_cinematic_until_exit = true;
      [ Config( "Skip these cinematics, comma seprated.  Set to empty (blank) to disable.  Set to \"default\" (without quotes) to reset.  Default skip mission controls, launches, lands, earth flybys, and general sat failure." ) ]
      public string skip_cinematics = "Earth_Launch_Failure,Earth_Launch_Failure_Large,Earth_Launch_Failure_Medium,Earth_Launch_Failure_Small,Earth_Launch_Intro,Earth_Launch_Intro_Large,Earth_Launch_Intro_Medium,Earth_Launch_Intro_Small,Earth_Launch_Outro,Earth_Launch_Success,Earth_Launch_Success_Large,Earth_Launch_Success_Medium,Earth_Launch_Success_Small,MissionControl_Intro,MissionControl_Success_Generic,MissionControl_Success_Milestone,Space_Generic_Failure";

      [ Config( "\r\n[Animation]" ) ]
      [ Config( "Cap assorted screen and action delays.  Set to -1 to disable." ) ]
      public float max_delay = 0.4f;
      [ Config( "Max screen fading duration.  Set to -1 to disable." ) ]
      public float max_screen_fade = 0.1f;
      [ Config( "Remove or reduce assorted screen delays.  Default True." ) ]
      public bool remove_delays = true;
      [ Config( "Deploy payload in background, skip payload connection, and skip task popup.  Default True." ) ]
      public bool skip_mission_intro = true;
      [ Config( "Skip launch countdown and speed up launch animations.  Default True." ) ]
      public bool fast_launch = true;
      [ Config( "Speed up mini-game animations such as reliablilty bar.  Default True." ) ]
      public bool fast_mission = true;
      [ Config( "Speed up mini-game report animations.  Default True." ) ]
      public bool fast_mission_result = true;
      [ Config( "Mini-game resource swoosh speed.  Set to 1, 0, or -1 to disable." ) ]
      public float swoosh_speed = 2f;

      [ Config( "\r\n[Bypass]" ) ]
      [ Config( "Bypass full screen notifications (construction complete and launch ready).  Default True." ) ]
      public bool bypass_fullscreen_notices = true;
      [ Config( "Bypass run of the mill popups such as research complete or mission phase.  Default True." ) ]
      public bool bypass_popups_notices = true;
      [ Config( "Automatically continue uneventful launches.  Default True." ) ]
      public bool auto_pass_normal_launch = true;
      [ Config( "Automatically continue empty rocket parts level up report.  Default True." ) ]
      public bool auto_pass_empty_levelup = true;
      [ Config( "Automatically continue uneventful mission actions.  Default True." ) ]
      public bool auto_pass_normal_action = true;

      [ Config( "\r\n" ) ]
      [ Config( "Version of this mod config file.  Do not change." ) ]
      public int config_version = 20200223;

      internal readonly HashSet< string > SkipCinematics = new HashSet< string >();

      protected override void OnLoad ( string _ ) { try {
         if ( string.Equals( skip_cinematics, "default", StringComparison.InvariantCultureIgnoreCase ) ) {
            skip_cinematics = new Config().skip_cinematics;
            Task.Run( Save );
         }
         lock ( SkipCinematics ) {
            SkipCinematics.Clear();
            if ( ! string.IsNullOrEmpty( skip_cinematics ) )
               foreach ( var e in new StringReader( skip_cinematics ).ReadCsvRow() )
                  if ( ! string.IsNullOrWhiteSpace( e ) )
                     SkipCinematics.Add( e.Trim() );
            Info( "{0} cinematic(s) has been seen and will be skipped.", SkipCinematics.Count );
         }
      } catch ( Exception x ) { Err( x ); } }

      public override void Save ( object subject, string path ) { lock ( SkipCinematics ) base.Save( subject, path ); }

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