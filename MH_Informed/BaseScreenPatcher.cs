using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class BaseScreenPatcher : ModPatcher {

      internal void Apply () {
         if ( config.show_base_bonus )
            TryPatch( typeof( BaseScreen ), "OnCellHovered", postfix: nameof( ShowBaseBonus ) );
      }

      private static void ShowBaseBonus ( BaseScreen __instance ) { try {
         if ( __instance.Mode != BaseScreen.EMode.Main ) return;
         if ( __instance.HoveredBuildingObject != null || __instance.HoveredBuildingObject != null ) return;
         var buildings = typeof( BaseScene ).Field( "buildingObjects" )?.GetValue( __instance.Scene ) as List<BuildingObject>;
         if ( buildings == null || buildings.Count == 0 ) return;

         var bonus = new Dictionary< Data.Effect.Type, int >();
         for ( var i = 0 ; i < buildings.Count ; i++ ) {
            for ( var j = i + 1 ; j < buildings.Count ; j++ ) {
               BuildingObject a = buildings[ i ], b = buildings[ j ];
               if ( ! Data.Building.TryGetAdjacencyLink( a.building, b.building, out var link, true ) ) continue;
               var effect = link.bonus.effect;
               var type = effect.type;
               bonus[ type ] = ( bonus.TryGetValue( type, out var val ) ? val : 0 ) + effect.strength;
            }
         }
         var links = bonus.Where( e => e.Value != 0 ).Select( e => new Data.Effect{ type = e.Key, strength = e.Value } ).ToArray();
         if ( links.Length == 0 ) return;
         ShowModifiers( __instance.highlightToolTipElement, links );
      } catch ( Exception x ) { Err( x ); } }

      private static void ShowModifiers ( BaseHighlightToolTipElement tooltip, Data.Effect[] effects ) {
         var type = typeof( BaseHighlightToolTipElement );
         var refresh = type.Method( "RefreshModifierListElement" );
         var modifierList = type.Field( "modifierList" )?.GetValue( tooltip ) as SimplePooler<BuildingModifierElement>;
         if ( refresh == null || modifierList == null ) {
            Error( "BaseScreen.highlightToolTipElement.(RefreshModifierListElement|modifierList|parent) not found or type mismatch." );
            return;
         }
         modifierList.FreeAll();

         foreach ( var e in effects ) {
            var link = new Data.Building.AdjacencyLink{ bonus = new Data.Blueprint.AdjacencyBonus{ effect = e } };
            refresh.Run( tooltip, link, modifierList.Get() );
         }
         tooltip.gameObject.SetActive( true );
         //( type.Field( "modifierListTitle" )?.GetValue( tooltip ) as Transform )?.gameObject.SetActive( false );
         ( type.Field( "modifierListParent" )?.GetValue( tooltip ) as Transform )?.gameObject.SetActive( true );
      }
   }
}