using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Reflection;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.PayloadQA {

   internal class PatcherMinigame: ModPatcher {
      internal void Apply () {
         if ( config.minigame_base_crit >= 0 || config.minigame_porportion_crit > 0 ) {
            TryPatch( typeof( MissionGameplayModuleElement ), "SetReliabilityBar", prefix: nameof( SetReliabilityBar ), postfix: nameof( RevertReliability ) );
            TryPatch( typeof( MissionGameplaySimulation ), "GetPayloadActionChances", prefix: nameof( SetReliabilityBar ), postfix: nameof( RevertReliability ) );
            TryPatch( typeof( MissionGameplayScreen ), "SetupReliabilityBar", prefix: nameof( SetReliabilityBar ), postfix: nameof( RevertReliability ) );
         }
      }

      private static float OriginalPayloadCritChance = float.NaN;
      private static float LastReliability = float.NaN, LastCritChance;

      private static void SetReliabilityBar () { try {
         var mSim = ClientViewer.GetViewElement<MissionGameplayScreen>()?.MissionSim;
         if ( mSim == null ) return;
         SetReliability( mSim.GetPayloadReliability() / 100f );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetReliability ( float payloadReliability ) {
         if ( float.IsNaN( OriginalPayloadCritChance ) ) OriginalPayloadCritChance = Data.instance.rules.missionGameplayRules.positiveEventOccurrence;
         var crit = LastCritChance;
         if ( LastReliability != payloadReliability ) {
            crit = config.minigame_base_crit + config.minigame_porportion_crit * payloadReliability;
            if ( crit > payloadReliability ) crit = payloadReliability;
            Fine( "Crit chance {0:P} = {1:P} + ( Payload {2:P} x {3:P} )", crit, config.minigame_base_crit, payloadReliability, config.minigame_porportion_crit );
            LastReliability = payloadReliability;
            LastCritChance = crit;
         }
         Data.instance.rules.missionGameplayRules.positiveEventOccurrence = crit;
      }

      private static void RevertReliability () { try {
         if ( ! float.IsNaN( OriginalPayloadCritChance ) )
            Data.instance.rules.missionGameplayRules.positiveEventOccurrence = OriginalPayloadCritChance;
      } catch ( Exception x ) { Err( x ); } }

   }
}