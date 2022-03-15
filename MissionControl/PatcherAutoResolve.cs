using Astronautica.Autoresolve;
using System;
using System.Reflection;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.MissionControl {

   internal class PatcherAutoResolve: ModPatcher {
      internal void Apply () {
         TryPatch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), prefix: nameof( StandaloneAutoResolve ) );
      }

      private static readonly Random resolveRng = new Random();
      private static readonly FieldInfo ResolveRoll = typeof( AutoresolveMission ).Field( "Roll" );

      private static void StandaloneAutoResolve ( AutoresolveMission __instance ) { try {
         var oldRoll = __instance.Roll;
         ResolveRoll.SetValue( __instance, (float) resolveRng.NextDouble() );
         Info( "Auto-resolve mission roll: {0:P2} => {1:P2}, Fail {2}%, Perfect {3}%", oldRoll, __instance.Roll, __instance.FailureChance, __instance.OutstandingChance );
      } catch ( Exception x ) { Err( x ); } }
   }
}