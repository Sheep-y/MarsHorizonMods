using Astronautica;
using Astronautica.Autoresolve;
using Astronautica.Model.Modifiers;
using Astronautica.Model.PayloadVariation;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Astronautica.Model.PayloadVariation.PayloadVariant.Type;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.PayloadQA {

   internal class PatcherAutoResolve: ModPatcher {
      internal void Apply () {
         if ( config.standalone_resolve_rng )
            TryPatch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), prefix: nameof( StandaloneAutoResolve ), postfix: nameof( LogAutoResolve ) );
         if ( config.payload_power_auto_bonus > 0 || config.payload_specialise_auto_bonus > 0 )
            TryPatch( typeof( Simulation ).Method( "GetAgencyAutoResolveChance" ), postfix: nameof( AddPayloadBonus ) );
         if ( config.payload_power_auto_bonus_crit > 0 ) {
            TryPatch( typeof( Simulation ).Method( "GetAgencyAutoResolveChance" ), postfix: nameof( TrackPayload ) );
            TryPatch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), postfix: nameof( AddCritBonus ) );
         }
      }

      private static Data.Payload GetPayloadVariant ( Mission mission, out PayloadVariant.Type variant ) {
         variant = Crew;
         var payload = mission.payload;
         if ( mission.template.IsSoundingRocking || payload == null ) return null;
         var i = payload.variantIndex;
         var list = payload.variants;
         if ( i < 0 || i >= list.Length ) return null;
         variant = list[ i ].type;
         return payload;
      }

      private static void AddPayloadBonus ( Mission mission, List< ValueModifier > modifiers, ref int __result ) { try {
         var payload = GetPayloadVariant( mission, out var type );
         if ( payload == null ) return;
         var bonus = GetPayloadBonus( type );
         if ( bonus == 0 ) return;
         Info( "Adding {0}% to auto-resolve success rate for {1} payload.", bonus, type );
         modifiers.Add( new ValueModifier( EModifierSource.ConstructionTrait, bonus, payload.id, EPolarity.Positive, Agency.Type.None ) );
         if ( __result >= 100 ) return;
         __result = Math.Min( 99, __result + bonus );
      } catch ( Exception x ) { Err( x ); } }

      private static int GetPayloadBonus ( PayloadVariant.Type type ) {
         switch ( type ) {
            case Comms:
            case Navigation:
            case Observation:
               return Math.Max( 0, (int) config.payload_specialise_auto_bonus );
            case Power:
               return Math.Max( 0, (int) config.payload_power_auto_bonus );
         }
         return 0;
      }

      private static PayloadVariant.Type currentVariant;

      private static void TrackPayload ( Mission mission ) { try {
         GetPayloadVariant( mission, out currentVariant );
      } catch ( Exception x ) { Err( x ); } }

      private static void AddCritBonus ( AutoresolveMission __instance, bool isMarsFinalMission, ref AutoresolveMission.EResult __result ) { try {
         if ( currentVariant != Power || isMarsFinalMission ) return;
         var oldChance = __instance.OutstandingChance;
         var newChance = Math.Min( 100 - __instance.FailureChance, oldChance + config.payload_power_auto_bonus_crit );
         typeof( AutoresolveMission ).Property( "OutstandingChance" ).SetValue( __instance, newChance );
         if ( __result != AutoresolveMission.EResult.Success ) return;
         var reverseRoll = ( 1 - __instance.Roll ) * 100f;
         if ( reverseRoll >= newChance ) {
            Info( "Correcting auto-resolve roll {0} to Outstanding ({1}%) due to power bonus.", __instance.Roll, newChance );
            __result = AutoresolveMission.EResult.OutstandingSuccess;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static readonly Random resolveRng = new Random();
      private static readonly PropertyInfo ResolveRoll = typeof( AutoresolveMission ).Property( "Roll" );
      private static float oldRoll;

      private static void StandaloneAutoResolve ( AutoresolveMission __instance ) { try {
         oldRoll = __instance.Roll;
         ResolveRoll?.SetValue( __instance, (float) resolveRng.NextDouble() );
      } catch ( Exception x ) { Err( x ); } }

      private static void LogAutoResolve ( AutoresolveMission __instance ) { try {
         Info( "Auto-resolve mission roll: {0:P2} => {1:P2}, Fail {2}%, Perfect {3}%", oldRoll, __instance.Roll, __instance.FailureChance, __instance.OutstandingChance );
      } catch ( Exception x ) { Err( x ); } }
   }
}