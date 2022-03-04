using Astronautica;
using Astronautica.View;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.MissionControl {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "MissionControl";
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         var config = ModPatcher.config;
         config.Load();
         new PatcherSimulation().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static Config config = new Config();
      internal static string Localise ( string tag, params string[] vars ) {
         Dictionary< string, string > variables = null;
         if ( vars?.Length > 0 ) {
            variables = new Dictionary< string, string >();
            for ( var i = 0 ; i + 1 < vars.Length ; i += 2 )
               variables.Add( vars[ i ], vars[ i + 1 ] );
         }
         return ScriptableObjectSingleton<Localisation>.instance.Localise( tag, variables );
      }
      internal static Client activeClient => Controller.Instance?.activeClient;
      internal static ClientViewer clientViewer => Controller.Instance?.clientViewer;
      internal static Simulation simulation => activeClient.simulation;
   }

   public class Config : IniConfig {
      [ Config( "Weight of Jupiter mission.  Set to 0 to disable." ) ]
      public int jupiter_mission_weight = 1;
      [ Config( "Weight of Saturn mission.  Set to 0 to disable." ) ]
      public int saturn_mission_weight = 1;
      [ Config( "Weight of Uranus mission.  Set to 0 to disable." ) ]
      public int uranus_mission_weight = 1;
      [ Config( "Weight of Neptune mission.  Set to 0 to disable." ) ]
      public int neptune_mission_weight = 1;
      [ Config( "Weight of Pluto mission.  Set to 0 to disable." ) ]
      public int pluto_mission_weight = 1;
   }

   internal class PatcherSimulation : ModPatcher {
      internal void Apply () {
         TryPatch( typeof( Simulation ).Method( "AgencyTryGenerateRequestMissionType" ), transpiler: nameof( InterceptMissionTypes ) );
      }

      private static IEnumerable< CodeInstruction > InterceptMissionTypes ( IEnumerable< CodeInstruction > codes ) {
         var list = codes.ToList();
         if ( list.Count <= 20 ) { Error( "Method too short. Aborting."); return codes; }
         var pos = list.Count - 20;
         CodeInstruction label = list[ pos ], ldloc = list[ pos + 1 ], count = list[ pos + 2 ];
         var cTxt = count.ToString();
         if ( label.opcode == OpCodes.Blt &&
              ldloc.opcode == OpCodes.Ldloc_1 &&
              count.opcode == OpCodes.Callvirt && cTxt.Contains( ".RequestMissionType>" ) && cTxt.EndsWith( "Count()" ) ) {
            Info( "CIL found, intercepting mission type list" );
            list.Insert( pos + 1, new CodeInstruction( OpCodes.Ldloc_1 ) );
            list.Insert( pos + 2, new CodeInstruction( OpCodes.Call, typeof( PatcherSimulation ).Method( "ProcessMissionTemplate" ) ) );
         } else {
            Error( "CIL mismatch.  Patch failure. {0} {1} {2}", label, ldloc, count );
            if ( RootMod.Log.LogLevel == System.Diagnostics.TraceLevel.Verbose )
               foreach ( var code in list )
                  Fine( code );
         }
         return list;
      }

      public static void ProcessMissionTemplate ( List< Data.MissionTemplate.RequestMissionType > list ) {
         Info( list.Count );
         foreach ( var m in list ) {
            Info( "{3} {0} Era[{1}] Tech[{2}]", m.type,
               string.Join( ",", m.eraDependencies.Select( e => e + "" ) ),
               string.Join( ",", m.researchDependencies.Select( e => e + "" ) ),
               m.weighting );
         }
      }
   }
}