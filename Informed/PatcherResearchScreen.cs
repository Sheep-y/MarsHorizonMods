using Astronautica;
using Astronautica.Tutorial.Tooltips;
using Astronautica.View;
using Astronautica.View.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
         var agent = activeClient.agency;
         foreach ( var id in sups ) {
            var part = FindPart( id );
            string icon = null;
            if ( ! agent.HasCompletedResearch( id ) ) icon = agent.activeResearch?.id == id ? "Spr_Icon_Screen_Research_64x64" : "WarningScience";
            if ( icon != null ) icon = $" <sprite name=\"{icon}\"/>";
            statItemPooler.Reuse().Set( " + " + Localise( $"Name_{id}" ) + icon, Data.instance.FormatWeight( part.capacity + cap ) );
         }
         sups = null;
      } catch ( Exception x ) { Err( x ); } }
   }
}