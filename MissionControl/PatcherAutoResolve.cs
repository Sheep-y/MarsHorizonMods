using Astronautica.Autoresolve;
using System;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.MissionControl {

   internal class PatcherAutoResolve: ModPatcher {
      internal void Apply () {
         TryPatch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), postfix: nameof( TrackMissionAutoResolve ) );
      }

      private static void TrackMissionAutoResolve ( AutoresolveMission __instance, AutoresolveMission.EResult __result ) { try {
         Info( "Auto-resolve mission roll: {0:P2}/{1}% = {2}", __instance.Roll, __instance.Chance, __result );
      } catch ( Exception x ) { Err( x ); } }
   }
}