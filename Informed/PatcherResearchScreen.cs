using Astronautica;
using Astronautica.Tutorial.Tooltips;
using Astronautica.View;
using System;
using System.Linq;
using System.Text;
using static Astronautica.Data;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherResearchScreen : ModPatcher {

      internal void Apply () {
         if ( config.show_supplement_in_booster_description ) {
            TryPatch( typeof( TutorialTooltipStats ), "SetupResearchBooster", prefix: nameof( SetSupplementList ), postfix: nameof( ClearSupplementList ) );
            TryPatch( typeof( TutorialTooltipStatItem ), "Set", postfix: nameof( AppendSupplementStats ) );
         }
      }

      private static string key_cap;
      private static string[] sups;
      private static int cap;
      private static ReusableSimplePooler< TutorialTooltipStatItem > statItemPooler;

      private static VehiclePart FindPart ( string id ) => simulation.gamedata.vehicleParts.FirstOrDefault( ( p ) => p.id == id );

      private static void SetSupplementList ( Research research, ReusableSimplePooler< TutorialTooltipStatItem > ___statItemPooler ) { try {
         var part = FindPart( research.id );
         sups = part?.validSupplementaries;
         if ( sups == null ) return;
         if ( sups.Length == 0 ) { sups = null; return; }
         statItemPooler = ___statItemPooler;
         cap = part.capacity;
         key_cap = Localise( "Vehicle_Select_Part_Effect_Capacity" );
         Fine( "Found {1} boosters for {0}.", research.id, sups.Length );
      } catch ( Exception x ) { Err( x ); } }

      private static void ClearSupplementList () => sups = null;

      private static void AppendSupplementStats ( string key ) { try {
         if ( sups == null || key != key_cap ) return;
         Info( "Adding {0} boosters to stat list", sups.Length );
         foreach ( var id in sups )
            statItemPooler.Reuse().Set( " + " + Localise( $"Name_{id}" ) + " <sprite name=\"WarningScience\"/>", Data.instance.FormatWeight( FindPart( id ).capacity + cap ) );
         sups = null;
      } catch ( Exception x ) { Err( x ); } }
   }
}