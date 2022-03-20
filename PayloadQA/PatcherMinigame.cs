using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Reflection;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.PayloadQA {

   internal class PatcherMinigame: ModPatcher {
      internal void Apply () {
         if ( config.minigame_base_crit >= 0 || config.minigame_porportion_crit > 0 )
            TryPatch( typeof( MissionGameplaySimulation ), "GetPayloadActionChances", postfix: nameof( SetBasePayloadReliability ) );
      }

      private static float BaseReliabilityDiff = float.NaN;

      private static void SetBasePayloadReliability ( ref Tuple< float, float > __result, float payloadReliability ) { try {
         if ( float.IsNaN( BaseReliabilityDiff ) ) {
            var orig = Data.instance.rules.missionGameplayRules.positiveEventOccurrence;
            BaseReliabilityDiff = config.minigame_base_crit - orig;
            Info( "Default base payload crit chance = {0:P}.  Modded base crit chacne = {1:P}.  Difference = {2:P}", orig, config.minigame_base_crit, BaseReliabilityDiff );
         }
         float oldNonCrit = __result.Item1, oldFail = __result.Item2;
         float newNonCrit = oldNonCrit, newFail = oldFail + BaseReliabilityDiff;
         if ( config.minigame_porportion_crit > 0 ) oldNonCrit += payloadReliability * config.minigame_porportion_crit;
         Fine( "Fail chance {0:P} => {1:P}, Crit chance {2:P} => {3:P}", oldFail, newFail, 1 - oldNonCrit, 1 - newNonCrit );
         if ( oldNonCrit == newNonCrit && oldFail == newFail ) return;
         __result = new Tuple< float, float > ( newNonCrit, newFail );
      } catch ( Exception x ) { Err( x ); } }
   }
}