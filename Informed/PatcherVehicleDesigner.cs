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
using UnityEngine;
using static Astronautica.Data;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherVehicleDesigner : ModPatcher {

      internal void Apply () {
         if ( config.show_contractor_effects_on_button && eVal != null ) {
            TryPatch( typeof( VehicleDesignerContractorUpgradeInfo ), "Initialise", postfix: nameof( TrackContractorUpgradeInfo ) );
            TryPatch( typeof( ContractorUpgradeListItem<Data.Contractor> ), "Setup", prefix: nameof( TrackDesignerState ) );
            TryPatch( typeof( ContractorListItem ), "GetName", postfix: nameof( AddContractorEffects ) );
         }
         if ( config.launch_window_hint_before_ready > 0 || config.launch_window_hint_after_ready > 0 ) {
            if ( TryPatch( typeof( VehicleDesignerTooltipManager ), "Awake", postfix: nameof( TrackTooltip ) ) != null &&
                 TryPatch( typeof( VehicleDesignerVehicleStats ), "RefreshCostTimeInfo", postfix: nameof( TrackMission ) ) != null ) {
               if ( config.always_show_launch_window )
                  TryPatch( typeof( VehicleDesignerTooltipManager ), "Show", postfix: nameof( ShowLaunchCalendar ) );
               TryPatch( typeof( VehicleDesignerTooltipManager ), "Hide", postfix: nameof( ShowLaunchCalendar ) );
            }
         }
      }

      #region Contractor
      private static VehicleDesignerContractorUpgradeInfo upgradeInfo;
      private static VehicleDesignerState designerState;
      private static readonly MethodInfo eVal = typeof( VehicleDesignerState ).Method( "GetContractorEffectValue", typeof( Contractor.Effect ) );
      private static readonly MethodInfo eIco = typeof( VehicleDesignerContractorUpgradeInfo ).Method( "GetContractorEffectIcon", typeof( Contractor.Effect ) );

      private static void TrackDesignerState ( VehicleDesignerState state ) => designerState = state;
      private static void TrackContractorUpgradeInfo ( VehicleDesignerContractorUpgradeInfo __instance ) => upgradeInfo = __instance;
      private static void AddContractorEffects ( Contractor ___data, ref string __result ) { try {
         if ( ___data?.effects == null || ___data.effects.Length == 0 || designerState == null ) return;
         var buf = new StringBuilder( __result ).Append( "\n" );
         foreach ( var effect in ___data.effects ) {
            var val = (float) eVal.Run( designerState, effect );
            var ico = upgradeInfo != null ? eIco?.Run( upgradeInfo, effect ) as Sprite : null;
            if ( ico != null ) buf.Append( "<sprite name=\"" ).Append( ico.name ).Append( "\"/> " );
            buf.AppendFormat( "<color={0}>{1}{2:0%}</color>", effect.IsEffectNegative() ? "#FF6666" : "#00FF00", val >= 0 ? "+" : "", val );
            buf.Append( "   " );
         }
         Fine( "{0} => {1}", __result, buf );
         __result = buf.ToString().Trim();
      } catch ( Exception x ) { Err( x ); manager = null; } }
      #endregion

      private static int buildTime = -1;
      private static Mission mission;
      private static IEnumerable< string > cachedPre, cachedPost;
      private static VehicleDesignerTooltipManager manager;
      private static AutoLocalise tooltipHeader;
      private static SimplePooler< VehicleDesignerTooltip > tooltipPooler;
      private static SimplePooler< VehicleDesignerTooltip > warningTooltipPooler;
      private static readonly MethodInfo Display = typeof( VehicleDesignerTooltipManager ).Method( "Display", typeof( bool ), typeof( bool ) );
      private static readonly MethodInfo FreeAll = typeof( SimplePooler< VehicleDesignerTooltip > ).Method( "FreeAll", new Type[0] );
      private static readonly MethodInfo Get = typeof( SimplePooler< VehicleDesignerTooltip > ).Method( "Get", new Type[0] );
      private static readonly FieldInfo Tooltip = typeof( VehicleDesignerTooltip ).Field( "tooltip" );

      private static void TrackTooltip () { try {
         Type type = typeof( VehicleDesignerTooltipManager );
         manager = type.Field( "instance" )?.GetValue( null ) as VehicleDesignerTooltipManager;
         if ( manager == null ) { Error( "Vehicle tooltip not found." ); return; }
         Fine( "Vehicle tooltip acquired" );
         tooltipHeader = type.Field( "tooltipHeader" ).GetValue( manager ) as AutoLocalise;
         tooltipPooler = type.Field( "tooltipPooler" ).GetValue( manager ) as SimplePooler< VehicleDesignerTooltip >;
         warningTooltipPooler = type.Field( "warningTooltipPooler" ).GetValue( manager ) as SimplePooler< VehicleDesignerTooltip >;
         if ( tooltipPooler == null || warningTooltipPooler == null || Display == null || FreeAll == null || Get == null || Tooltip == null ) {
            Error( "Something is missing: tooltip = {0}, warnings = {1}, Display = {2}, FreeAll = {3}, Get = {4}, ToolTip = {5}", tooltipPooler, warningTooltipPooler, Display, FreeAll, Get, Tooltip );
            manager = null;
         }
      } catch ( Exception x ) { Err( x ); manager = null; } }

      private static void TrackMission ( VehicleDesignerState state ) {
         if ( manager == null ) return;
         var vehicle = state.CurrentVehicle;
         if ( mission != state.Mission ) {
            mission = state.Mission;
            Fine( "Set vehicle destination to {0}.  Vehicle is {1}.", mission?.template?.planetaryBody, vehicle.isUnderConstruction ? "being built" : "built" );
            buildTime = -1;
         }
         cachedPre = cachedPost = null;
         var fitTime = vehicle.isUnderConstruction ?  vehicle.combinedBuildFitTime : state.GetVehicleBuildOrRefitTime( out _, true );
         if ( buildTime == fitTime || fitTime < 0 ) return;
         Fine( "Update vehicle build time to {0}", fitTime );
         buildTime = fitTime;
         ShowLaunchCalendar();
      }

      private static void ShowLaunchCalendar () { try {
            if ( manager == null || buildTime < 0 || mission == null ) return;
            Fine( "Refreshing vehicle designer tooltips." );
            if ( tooltipHeader != null ) tooltipHeader.tag = "Name_Body_" + mission.template.planetaryBody;
            FreeAll.Run( tooltipPooler );
            FreeAll.Run( warningTooltipPooler );
            if ( cachedPre == null ) GetLaunchCalendar( out cachedPre, out cachedPost );
            if ( cachedPre.Any() ) NewTooltip( "Building_Filter_UnderConstruction", "Title_Vehicle_Construction_Warning", string.Join( "\n", cachedPre ) );
            if ( cachedPost.Any() ) NewTooltip( "Calendar_LaunchWindow_Title", "TP_Calendar_1_Content", string.Join( "\n", cachedPost ) );
            Display.Run( manager, true, false );
         } catch ( Exception x ) { Err( x ); } }

      private static void NewTooltip ( string header, string placeholder, string content ) {
         var tooltip = Get.Run( tooltipPooler ) as VehicleDesignerTooltip;
         tooltip.Set( header, placeholder, true );
         ( Tooltip.GetValue( tooltip ) as AutoLocalise ).text = content;
      }

      private static void GetLaunchCalendar ( out IEnumerable< string > pre, out IEnumerable< string > post ) {
         List< string > preList = new List< string >(), postList = new List< string >();
         pre = preList; post = postList;
         if ( buildTime < 0 ) return;
         var buf = new StringBuilder();
         var sim = simulation;
         var agency = activeClient.agency;
         var destination = mission.template.planetaryBody;
         int nowTurn = sim.universe.turn, doneTurn = nowTurn + buildTime;
         int fromTurn = Math.Max( doneTurn - config.launch_window_hint_before_ready + 1, nowTurn ), toTurn = doneTurn + config.launch_window_hint_after_ready;
         Info( "Current turn {0}, build time {1}.  Calculating launch window for {4} from {2} to {3}", nowTurn, buildTime, fromTurn, toTurn, destination );
         var win = sim.GetAgencyLaunchWindow( agency, destination );
         for ( var i = fromTurn ; i <= toTurn ; i++ ) {
            Data.Date date = Data.instance.GetDate( i );
            var prediction = sim.GetAgencyLaunchRecommendation( agency, win, i, null, new ConstructionTrait[] { mission.Payload.ConstructionTrait } );
            string colour = config.invalid_colour;
            if ( prediction == Data.LaunchRecommendation.SubOptimal ) colour = config.suboptimal_colour;
            else if ( prediction == Data.LaunchRecommendation.Optimal ) colour = config.optimal_colour;
            if ( ! string.IsNullOrEmpty( colour ) ) buf.Append( "<color=" ).Append( colour ).Append( ">" );
            buf.Append( date.year ).Append( ' ' ).Append( Localise( $"Month_{date.month}_Short" ) ).Append( " - " ).Append( prediction );
            if ( ! string.IsNullOrEmpty( colour ) ) buf.Append( "</color>" );
            if ( i <= doneTurn ) preList.Add( buf.ToString() ); else postList.Add( buf.ToString() );
            buf.Clear();
         }
      }

   }
}