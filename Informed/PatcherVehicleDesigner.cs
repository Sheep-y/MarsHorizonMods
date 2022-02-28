using Astronautica;
using Astronautica.Model.ConstructionTraits;
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
         TryPatch( typeof( VehicleDesignerTooltipManager ), "SetState", postfix: nameof( LogState ) );

         if ( TryPatch( typeof( VehicleDesignerTooltipManager ), "Awake", postfix: nameof( TrackTooltip ) ) != null ) {
            TryPatch( typeof( VehicleDesignerScreen ), "Setup", postfix: nameof( TrackMission ) );
            TryPatch( typeof( VehicleDesignerState ), "GetVehicleBuildOrRefitTime", postfix: nameof( TrackBuildTime ) );
            TryPatch( typeof( VehicleDesignerTooltipManager ), "Hide", postfix: nameof( ShowLaunchCalendar ) );
         }
      }

      private static int buildTime = -1;
      private static Client client;
      private static Mission mission;
      private static string cachedLaunchWindow;
      private static VehicleDesignerTooltipManager manager;
      private static SimplePooler< VehicleDesignerTooltip > tooltipPooler;
      private static SimplePooler< VehicleDesignerTooltip > warningTooltipPooler;
      private static MethodInfo Display = typeof( VehicleDesignerTooltipManager ).Method( "Display", typeof( bool ), typeof( bool ) );
      private static MethodInfo FreeAll = typeof( SimplePooler< VehicleDesignerTooltip > ).Method( "FreeAll", new Type[0] );
      private static MethodInfo Get = typeof( SimplePooler< VehicleDesignerTooltip > ).Method( "Get", new Type[0] );
      private static FieldInfo Tooltip = typeof( VehicleDesignerTooltip ).Field( "tooltip" );

      private static void TrackTooltip () { try {
         Type type = typeof( VehicleDesignerTooltipManager );
         manager = type.Field( "instance" )?.GetValue( null ) as VehicleDesignerTooltipManager;
         if ( manager == null ) { Error( "Vehicle tooltip not found." ); return; }
         Fine( "Vehicle tooltip acquired" );
         tooltipPooler = type.Field( "tooltipPooler" ).GetValue( manager ) as SimplePooler< VehicleDesignerTooltip >;
         warningTooltipPooler = type.Field( "warningTooltipPooler" ).GetValue( manager ) as SimplePooler< VehicleDesignerTooltip >;
         if ( tooltipPooler == null || warningTooltipPooler == null || Display == null || FreeAll == null || Get == null || Tooltip == null ) {
            Error( "Something is missing: tooltip = {0}, warnings = {1}, Display = {2}, FreeAll = {3}, Get = {4}, ToolTip = {5}", tooltipPooler, warningTooltipPooler, Display, FreeAll, Get, Tooltip );
            manager = null;
         }
      } catch ( Exception x ) { Err( x ); manager = null; } }

      private static void TrackMission ( VehicleDesignerScreen __instance, Mission mission ) {
         if ( PatcherVehicleDesigner.mission == mission ) return;
         Fine( "Set vehicle calendar destination {0}", mission?.template?.planetaryBody );
         PatcherVehicleDesigner.mission = mission;
         client = __instance.client;
         cachedLaunchWindow = null;
      }

      private static void TrackBuildTime ( int __result ) {
         if ( manager == null || buildTime == __result ) return;
         Fine( "Update vehicle bulid time to {0}", __result );
         buildTime = __result;
         cachedLaunchWindow = null;
         ShowLaunchCalendar();
      }

      private static void ShowLaunchCalendar () { try {
         if ( manager == null || buildTime < 0 || client == null || mission == null ) return;
         Fine( "Refreshing vehicle designer tooltips." );
         FreeAll.Run( tooltipPooler );
         FreeAll.Run( warningTooltipPooler );
         var tooltip = Get.Run( tooltipPooler ) as VehicleDesignerTooltip;
         tooltip?.Set( "Calendar_LaunchWindow_Title", "TP_Calendar_1_Content", true );
         ( Tooltip.GetValue( tooltip ) as AutoLocalise ).text = GetLaunchCalendar();
         Display.Run( manager, true, false );
      } catch ( Exception x ) { Err( x ); } }

      private static string GetLaunchCalendar () {
         if ( cachedLaunchWindow != null ) return cachedLaunchWindow;
         var buf = new StringBuilder();
         var sim = client.simulation;
         var agency = client.agency;
         var destination = mission.template.planetaryBody;
         int nowTurn = sim.universe.turn, doneTurn = nowTurn + buildTime, fromTurn = doneTurn - 1, toTurn = doneTurn + 5;
         Info( "Current turn {0}, build time {1}.  Calculating launch window for {4} from {2} to {3}", nowTurn, buildTime, fromTurn, toTurn, destination );
         var win = sim.GetAgencyLaunchWindow( agency, destination );
         for ( var i = fromTurn ; i <= doneTurn ; i++ ) {
            Data.Date date = Data.instance.GetDate( i );
            buf.Append( date.year ).Append( ' ' ).Append( ScriptableObjectSingleton<Localisation>.instance.Localise( "Month_" + date.month ) ).Append( ' ' )
               .Append( sim.GetAgencyLaunchRecommendation( agency, win, i, null, new ConstructionTrait[] { mission.Payload.ConstructionTrait } ) )
               .Append( "\n" );
         }
         return cachedLaunchWindow = buf.ToString();
      }

      private static void LogState ( VehicleDesignerState state ) { try {
         if ( state?.CurrentVehicle == null ) return;
         Info( "Tooltip Vehicle state {0} / {1}", state.CurrentVehicle.buildTime, state.CurrentVehicle.combinedBuildFitTime );
      } catch ( Exception x ) { Err( x ); } }
   }
}