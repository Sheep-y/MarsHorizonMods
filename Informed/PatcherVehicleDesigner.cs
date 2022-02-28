using Astronautica;
using Astronautica.View;
using Astronautica.View.VehicleDesigner;
using Astronautica.View.VehicleDesigner.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherVehicleDesigner : ModPatcher {

      internal void Apply () {
         TryPatch( typeof( VehicleDesignerTooltipManager ), "Set", postfix: nameof( LogSet ) );
         TryPatch( typeof( VehicleDesignerTooltipManager ), "SetState", postfix: nameof( LogState ) );

         if ( TryPatch( typeof( VehicleDesignerTooltipManager ), "Awake", postfix: nameof( TrackTooltip ) ) != null
           && TryPatch( typeof( VehicleDesignerState ), "GetVehicleBuildOrRefitTime", postfix: nameof( TrackBuildTime ) ) != null ) {
            TryPatch( typeof( VehicleDesignerTooltipManager ), "Hide", postfix: nameof( ShowLaunchCalendar ) );
         }
         //TryPatch( typeof( VehicleDesignerTooltipEvent ), "OnPointerEnter", prefix: nameof( LogEnter ) );
         //TryPatch( typeof( VehicleDesignerTooltip ).Method( "Set", typeof( string ), typeof( string ), typeof( bool ) ), postfix: nameof( Test ) );
      }

      private static int buildTime;
      private static VehicleDesignerTooltipManager manager;
      private static SimplePooler< VehicleDesignerTooltip > tooltipPooler;
      private static SimplePooler< VehicleDesignerTooltip > warningTooltipPooler;
      private static MethodInfo Display = typeof( VehicleDesignerTooltipManager ).Method( "Display", typeof( bool ), typeof( bool ) );
      private static MethodInfo FreeAll = typeof( SimplePooler< VehicleDesignerTooltip > ).Method( "FreeAll", new Type[0] );
      private static MethodInfo Get = typeof( SimplePooler< VehicleDesignerTooltip > ).Method( "Get", new Type[0] );

      private static void TrackTooltip () { try {
         Type type = typeof( VehicleDesignerTooltipManager );
         manager = type.Field( "instance" )?.GetValue( null ) as VehicleDesignerTooltipManager;
         if ( manager == null ) { Error( "Vehicle tooltip not found." ); return; }
         Fine( "Vehicle tooltip acquired" );
         tooltipPooler = type.Field( "tooltipPooler" ).GetValue( manager ) as SimplePooler< VehicleDesignerTooltip >;
         warningTooltipPooler = type.Field( "warningTooltipPooler" ).GetValue( manager ) as SimplePooler< VehicleDesignerTooltip >;
         if ( tooltipPooler == null || warningTooltipPooler == null || Display == null || FreeAll == null || Get == null ) {
            Error( "Something is missing: tooltip = {0}, warnings = {1}, Display = {2}, FreeAll = {3}, Get = {4}", tooltipPooler, warningTooltipPooler, Display, FreeAll, Get );
            manager = null;
         }
      } catch ( Exception x ) { Err( x ); manager = null; } }

      private static void TrackBuildTime ( int __result ) {
         if ( manager == null || buildTime == __result ) return;
         buildTime = __result;
         Fine( "Update vehicle bulid time to {0}", __result );
      }

      private static void ShowLaunchCalendar () { try {
         if ( manager == null ) return;
         Fine( "Erasing vehicle designer tooltips." );
         FreeAll.Run( tooltipPooler );
         FreeAll.Run( warningTooltipPooler );
         Fine( "Adding launch window tooltips." );
         ( Get.Run( tooltipPooler ) as VehicleDesignerTooltip )?.Set( "Calendar_LaunchWindow_Title", buildTime.ToString(), true );
         Display.Run( manager, true, false );
      } catch ( Exception x ) { Err( x ); manager = null; } }

      private static void LogEnter ( object __instance ) {
         Info( "Enter {0}", ( __instance.GetType().Field( "tooltips" )?.GetValue( __instance ) as VehicleDesignerTooltipData )?.header ?? "null" );
      }

      private static void LogSet ( VehicleDesignerTooltipData data ) { try {
         if ( data == null ) return;
         Info( "Tooptip Set {0}", data.tooltips?.Length ?? 0 );
         if ( data.tooltips == null ) return;
         foreach ( var tip in data.tooltips )
            Info( tip.validation, tip.tooltipHeader, tip.tooltip, tip.invalidTooltip );
      } catch ( Exception x ) { Err( x ); } }

      private static void LogState ( VehicleDesignerState state ) { try {
         if ( state?.CurrentVehicle == null ) return;
         Info( "Tooltip Vehicle state {0} / {1}", state.CurrentVehicle.buildTime, state.CurrentVehicle.combinedBuildFitTime );
      } catch ( Exception x ) { Err( x ); } }

      private static void Test ( VehicleDesignerTooltip __instance ) { try {
         ( __instance.GetType().Field( "tooltip" ).GetValue( __instance ) as AutoLocalise ).text = buildTime.ToString();
      } catch ( Exception x ) { Err( x ); } }
   }
}