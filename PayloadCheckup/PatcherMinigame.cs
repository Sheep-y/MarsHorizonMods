using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZyMod.MarsHorizon.PayloadCheckup {

   internal class PatcherMinigame: ModPatcher {
      internal override void Apply () {
         if ( config.minigame_base_crit >= 0 || config.minigame_porportion_crit > 0 ) {
            Patch( typeof( MissionGameplayModuleElement ), "SetReliabilityBar", prefix: nameof( SetReliabilityBar ), postfix: nameof( RevertReliability ) );
            Patch( typeof( MissionGameplaySimulation ), "GetPayloadActionChances", prefix: nameof( SetReliabilityBar ), postfix: nameof( RevertReliability ) );
            Patch( typeof( MissionGameplayScreen ), "SetupReliabilityBar", prefix: nameof( SetReliabilityBar ), postfix: nameof( RevertReliability ) );
         }
      }

      internal override void Unapply () => RevertReliability();

      private static float OriginalPayloadCritChance = float.NaN;
      private static float LastReliability = float.NaN, CritChance;
      private static Data.Rules.MissionGameplayRules Rules => Data.instance.rules.missionGameplayRules;

      private static void SetReliabilityBar () { try {
         var mSim = ClientViewer.GetViewElement<MissionGameplayScreen>()?.MissionSim;
         if ( mSim == null ) return;
         SetReliability( mSim.GetPayloadReliability() / 100f );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetReliability ( float payloadReliability ) {
         if ( float.IsNaN( OriginalPayloadCritChance ) )
            OriginalPayloadCritChance = Rules.positiveEventOccurrence;
         if ( LastReliability != payloadReliability ) {
            CritChance = Math.Min( config.minigame_base_crit + config.minigame_porportion_crit * payloadReliability, payloadReliability );
            Info( "Crit chance {0:P} = {1:P} + ( Payload {2:P} x {3:P} )", CritChance, config.minigame_base_crit, payloadReliability, config.minigame_porportion_crit );
            LastReliability = payloadReliability;
         } else
            Fine( "Set crit chance to {0}", CritChance );
         Rules.positiveEventOccurrence = CritChance;
      }

      private static void RevertReliability () { try {
         if ( float.IsNaN( OriginalPayloadCritChance ) || Rules.positiveEventOccurrence == OriginalPayloadCritChance ) return;
         Fine( "Restore crit chance to {0}", OriginalPayloadCritChance );
         Rules.positiveEventOccurrence = OriginalPayloadCritChance;
      } catch ( Exception x ) { Err( x ); } }

   }
}