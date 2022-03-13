using Astronautica;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherBaseScreen : ModPatcher {

      internal void Apply () {
         if ( config.show_base_bonus ) {
            TryPatch( typeof( AstroViewElement ), "Refresh", postfix: nameof( ClearBaseBonus ) );
            TryPatch( typeof( BaseScreen ), "EnterPurchaseState", postfix: nameof( HideBaseBonus ) );
            TryPatch( typeof( BaseScreen ), "OnCellHovered", postfix: nameof( ShowBaseBonus ) );
         }
      }

      private static Data.Effect[] links;
      private static BaseHighlightToolTipElement tooltip;

      private static void ClearBaseBonus () => links = null;
      private static void HideBaseBonus () => tooltip?.gameObject.SetActive( false );

      private static void ShowBaseBonus ( BaseScreen __instance ) { try {
         if ( __instance.Mode != BaseScreen.EMode.Main || __instance.HoveredBuildingObject != null ) return;
         if ( __instance.HoveredBlockerObject != null ) {
            GetModifierList( __instance.highlightToolTipElement )?.FreeAll();
            return;
         }
         if ( links == null ) {
            var buildings = typeof( BaseScene ).Field( "buildingObjects" )?.GetValue( __instance.Scene ) as List<BuildingObject>;
            if ( buildings == null || buildings.Count == 0 ) return;
            RefreshBaseBonus( buildings );
         }
         if ( links?.Length > 0 ) ShowModifiers( tooltip = __instance.highlightToolTipElement, links );
      } catch ( Exception x ) { Err( x ); } }

      private static void RefreshBaseBonus ( List< BuildingObject > buildings ) { try {
         Info( "Rendering building bonuses." );
         var bonus = new Dictionary< Data.Effect.Type, int >();
         for ( var i = 0 ; i < buildings.Count ; i++ ) {
            for ( var j = i + 1 ; j < buildings.Count ; j++ ) {
               BuildingObject a = buildings[ i ], b = buildings[ j ];
               if ( ! Data.Building.TryGetAdjacencyLink( a.building, b.building, out var link, true ) ) continue;
               var effect = link.bonus.effect;
               var type = effect.type;
               bonus[ type ] = ( bonus.TryGetValue( type, out var val ) ? val : 0 ) + effect.strength;
               Fine( "{2} {3} <<< {0} <=> {1} / Total {4}", a.name, b.name, type, effect.strength, bonus[ type ] );
            }
         }
         links = bonus.Where( e => e.Value != 0 ).Select( e => new Data.Effect{ type = e.Key, strength = e.Value } ).ToArray();
      } catch ( Exception x ) { Err( x ); } }

      private static readonly MethodInfo Refresh = typeof( BaseHighlightToolTipElement ).Method( "RefreshModifierListElement" );
      private static readonly FieldInfo ModList = typeof( BaseHighlightToolTipElement ).Field( "modifierList" );
      private static readonly FieldInfo Name = typeof( BaseHighlightToolTipElement ).Field( "objectName" );
      private static readonly FieldInfo Cost = typeof( BaseHighlightToolTipElement ).Field( "clearCost" );

      private static void ShowModifiers ( BaseHighlightToolTipElement tooltip, Data.Effect[] effects ) {
         Fine( "Refreshing total building modifiers." );
         var modifierList = GetModifierList( tooltip );
         if ( Refresh == null || modifierList == null ) {
            Error( "Not found: BaseHighlightToolTipElement RefreshModifierListElement = {0}, modifierList = {2}", Refresh, modifierList );
            return;
         }
         modifierList.FreeAll();

         foreach ( var e in effects ) {
            Fine( "{0} = {1}", e.type, e.strength );
            var link = new Data.Building.AdjacencyLink{ bonus = new Data.Blueprint.AdjacencyBonus{ effect = e } };
            Refresh.Run( tooltip, link, modifierList.Get() );
         }
         tooltip.gameObject.SetActive( true );
         ( Name?.GetValue( tooltip ) as TextSetter ).text = ScriptableObjectSingleton<Localisation>.instance.Localise( "Base" );
         ( Cost?.GetValue( tooltip ) as TextSetter ).gameObject.SetActive( false );
         //( type.Field( "modifierListTitle" )?.GetValue( tooltip ) as Transform )?.gameObject.SetActive( false );
         //( type.Field( "modifierListParent" )?.GetValue( tooltip ) as Transform )?.gameObject.SetActive( true );
      }

      private static SimplePooler< BuildingModifierElement > GetModifierList ( BaseHighlightToolTipElement tooltip ) =>
         ModList?.GetValue( tooltip ) as SimplePooler< BuildingModifierElement >;
   }
}