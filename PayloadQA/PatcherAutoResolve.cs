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
         TryPatch( typeof( Simulation ).Method( "GetAgencyAutoResolveChance" )
            , postfix: config.payload_power_auto_bonus > 0 || config.payload_specialise_auto_bonus > 0
               ? nameof( AddPayloadBonus ) : nameof( TrackPayload ) );
         if ( config.payload_power_auto_crit_bonus > 0 )
            TryPatch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), postfix: nameof( AddCritBonus ) );
      }

      private static PayloadVariant.Type currentVariant;

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
         var payload = GetPayloadVariant( mission, out currentVariant );
         if ( payload == null ) return;
         var bonus = GetPayloadBonus( mission, currentVariant );
         if ( bonus == 0 ) return;
         Info( "Adding {0}% to auto-resolve success rate for {1} payload.", bonus, currentVariant );
         modifiers.Add( new ValueModifier( EModifierSource.ConstructionTrait, bonus, payload.id, EPolarity.Positive, Agency.Type.None ) );
         if ( __result >= 100 ) return;
         __result = Math.Min( 99, __result + bonus );
      } catch ( Exception x ) { Err( x ); } }

      private static int GetPayloadBonus ( Mission mission, PayloadVariant.Type type ) {
         Fine( "Mission payload is {0}.", type );
         var result = 0;
         switch ( type ) {
            case Comms:
            case Navigation:
            case Observation:
               result = Math.Max( 0, (int) config.payload_specialise_auto_bonus );
               break;
            case Power :
               result = Math.Max( 0, (int) config.payload_power_auto_bonus );
               break;
         }
         if ( mission.CrewParticipated ) {
            int crew = mission.participatingAstronauts?.Length ?? 0, min = mission.template.minCrew;
            Fine( "Min crew {0}, assigned {1}.", min, crew );
            if ( crew > min )
               return Math.Max( 0, config.payload_extra_crew_auto_bonus * ( crew - min ) );
         }
         return result;
      }

      private static void TrackPayload ( Mission mission ) { try {
         GetPayloadVariant( mission, out currentVariant );
      } catch ( Exception x ) { Err( x ); } }

      private static void AddCritBonus ( AutoresolveMission __instance, bool isMarsFinalMission, ref AutoresolveMission.EResult __result ) { try {
         if ( currentVariant != Power || isMarsFinalMission ) return;
         var oldChance = __instance.OutstandingChance;
         var newChance = Math.Min( 100 - __instance.FailureChance, oldChance + config.payload_power_auto_crit_bonus );
         Info( "Changing payload Outstanding chance from {0}% to {1}%.", oldChance, newChance );
         typeof( AutoresolveMission ).Property( "OutstandingChance" ).SetValue( __instance, newChance );
         if ( __result != AutoresolveMission.EResult.Success ) return;
         var reverseRoll = ( 1 - __instance.Roll ) * 100f;
         if ( reverseRoll <= newChance ) {
            Info( "Upgrading auto-resolve roll ({0:P}) to Outstanding ({1}%) due to power bonus.", __instance.Roll, newChance );
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
         Info( "Auto-resolve: Roll {0:P2} => {1:P2}, Fail {2}%, Outstanding {3}%", oldRoll, __instance.Roll, __instance.FailureChance, __instance.OutstandingChance );
      } catch ( Exception x ) { Err( x ); } }
   }
}