using Astronautica;
using Astronautica.Autoresolve;
using Astronautica.Model.Modifiers;
using Astronautica.Model.PayloadVariation;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Astronautica.Model.PayloadVariation.PayloadVariant.Type;

namespace ZyMod.MarsHorizon.PayloadCheckup {

   internal class PatcherAutoResolve: ModPatcher {
      internal override void Apply () {
         if ( config.standalone_resolve_rng )
            Patch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), prefix: nameof( StandaloneAutoResolve ), postfix: nameof( LogAutoResolve ) );
         Patch( typeof( Simulation ).Method( "GetAgencyAutoResolveChance" )
            , postfix: config.special_payload_ar_bonus != 0 ? nameof( AddPayloadBonus ) : nameof( GetPayloadVariant ) );
         if ( config.power_payload_ar_crit > 0 )
            Patch( typeof( AutoresolveMission ).Method( "CalculateSuccess" ), postfix: nameof( AddCritBonus ) );
      }

      private static PayloadVariant.Type currentVariant;

      private static void GetPayloadVariant ( Mission mission ) { try {
         currentVariant = Crew;
         var payload = mission.payload;
         if ( mission.template.IsSoundingRocking || payload == null ) return;
         var i = payload.variantIndex;
         var list = payload.variants;
         if ( i < 0 || i >= list.Length ) return;
         currentVariant = list[ i ].type;
      } catch ( Exception x ) { Err( x ); } }

      private static void AddPayloadBonus ( Mission mission, List< ValueModifier > modifiers, ref int __result ) { try {
         GetPayloadVariant( mission );
         if ( mission.payload == null ) return;
         var bonus = GetPayloadBonus( mission, currentVariant );
         if ( bonus == 0 ) return;
         var orig = __result;
         modifiers.Add( new ValueModifier( EModifierSource.ConstructionTrait, bonus, mission.payload.id,
            bonus > 0 ? EPolarity.Positive : EPolarity.Negative, Agency.Type.None ) );
         __result = Math.Max( 1, Math.Min( __result + modifiers.Sum( e => e.ModifiedValue ), 99 ) );
         Info( "Added {0}% to auto-resolve success rate for {1} payload, total {2} => {3}.", bonus, currentVariant, orig, __result );
      } catch ( Exception x ) { Err( x ); } }

      private static int GetPayloadBonus ( Mission mission, PayloadVariant.Type type ) {
         Fine( "Mission payload is {0}.", type );
         var result = 0;
         switch ( type ) {
            case Comms :
            case Navigation :
            case Observation : return config.special_payload_ar_bonus;
            case Power : return config.power_payload_ar_bonus;
         }
         if ( mission.CrewParticipated ) {
            int crew = mission.participatingAstronauts?.Length ?? 0, min = mission.template.minCrew;
            Fine( "Min crew {0}, assigned {1}.", min, crew );
            if ( crew > min ) return config.extra_crew_ar_bonus * ( crew - min );
         }
         return result;
      }

      private static void AddCritBonus ( AutoresolveMission __instance, bool isMarsFinalMission, ref AutoresolveMission.EResult __result ) { try {
         if ( currentVariant != Power || isMarsFinalMission ) return;
         int oldChance = __instance.OutstandingChance, nonFailChance = 100 - __instance.FailureChance;
         var newChance = Math.Min( nonFailChance, Math.Max( oldChance + config.power_payload_ar_crit, 0 ) );
         if ( newChance == oldChance ) return;
         typeof( AutoresolveMission ).Property( "OutstandingChance" ).SetValue( __instance, newChance );
         typeof( AutoresolveMission ).Property( "SuccessChance" ).SetValue( __instance, nonFailChance - newChance );
         Info( "Changing auto-resolve Outstanding chance from {0}% to {1}% due to payload. Fail {2}%, Success {3}%", oldChance, newChance, __instance.FailureChance, __instance.SuccessChance );
         if ( __result != AutoresolveMission.EResult.Success ) return;
         var reverseRoll = ( 1 - __instance.Roll ) * 100f;
         if ( reverseRoll <= newChance ) {
            Info( "Upgrading auto-resolve roll ({0:P}) to Outstanding ({1}%) due to payload bonus.", __instance.Roll, newChance );
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