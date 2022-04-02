using Astronautica;
using Astronautica.View;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ZyMod.MarsHorizon.Informed {

   internal class PatcherSolarSystem : ModPatcher {
      internal override void Apply () {
         if ( config.show_final_funding_tier ) {
            Patch( typeof( ResourcesScreenFunding ), "Show", prefix: nameof( CheckFunding ), postfix: nameof( ClearFunding ) );
            Patch( typeof( Simulation ).Method( "GetAgencyFundingTier", typeof( Agency ), typeof( int ) ), prefix: nameof( GetFinalFundingTier ) );
         }
         if ( config.hint_dynamic_colour )
            Patch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( GetInfoIcon ) );
         if ( config.hint_available_mission )
            Patch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintAvailableMission ) );
         if ( config.hint_new_candidates )
            Patch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintNewCandidates ) );
         if ( config.hint_propose_join_mission )
            Patch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HintJointMission ) );
         if ( config.hint_spacepedia_hide )
            Patch( typeof( HUDScreenSelect ), "_Refresh", postfix: nameof( HideSpacepediaHint ) );
      }

      private static int currTier = -1;

      private static void CheckFunding () => currTier = 0;
      private static void ClearFunding () => currTier = -1;

      private static void GetFinalFundingTier ( Simulation __instance, Agency agency, ref int tier ) {
         if ( currTier < 0 ) return;
         Info( "Curr {0}, In = {1}, support = {2}", currTier, tier, agency.support );
         if ( currTier == 0 ) { currTier = tier; return; }
         if ( tier <= currTier ) return;
         try {
            var curr = tier;
            var tiers = __instance.universe.scenario.agencies[ agency.type - Agency.Type.USA ].fundingTiers;
            var final = tiers.Find( e => e.requiredSupport >= agency.support );
            if ( final != null && final.tier > tier ) {
               Info( "Upgrading Next Tier from {0} to {1} ({2}/{3}).", tier, final.tier, agency.support, final.requiredSupport );
               tier = final.tier;
            }
         } catch ( Exception x ) { Err( x ); }
      }

      private static Sprite icoInfo, icoWarn;
      private static Color clrInfo, clrWarn;
      private static readonly FieldInfo InfoIcon = typeof( SidebarOption ).Field( "infoIcon" );

      private static void GetInfoIcon ( SidebarOption ___missionsOption, SidebarOption ___spacepediaOption ) { try {
         if ( icoInfo != null ) return;
         icoInfo = GetIcon( ___spacepediaOption ).sprite;
         icoWarn = GetIcon( ___missionsOption ).sprite;
         clrInfo = GetHighlight( ___spacepediaOption ).color;
         clrWarn = GetHighlight( ___missionsOption ).color;
      } catch ( Exception x ) { Err( x ); } }

      private static void HintAvailableMission ( HUDScreenSelect __instance, SidebarOption ___missionsOption ) { try {
         var i = 0;
         if ( ! IsActive( ___missionsOption ) ) i = __instance.client.simulation.GetAgencyAvailableMissionSlots( __instance.agency );
         SetInfoState( ___missionsOption, i, "Hinting {0} available missions." );
      } catch ( Exception x ) { Err( x ); } }

      private static void HintNewCandidates ( HUDScreenSelect __instance, SidebarOption ___crewOption ) { try {
         var i = 0;
         if ( ! IsActive( ___crewOption ) )
            foreach ( var crew in __instance.agency.astronautRecruitPool )
               if ( ! GameStats.HasEntry( "CrewMember", crew.GetStatsId() ) )
                  i++;
         SetInfoState( ___crewOption, i, "Hinting {0} available candidates." );
      } catch ( Exception x ) { Err( x ); } }

      private static void HintJointMission ( HUDScreenSelect __instance, SidebarOption ___diplomacyOption ) { try {
         var i = 0;
         if ( ! IsActive( ___diplomacyOption ) &&
               __instance.client.simulation.CanAgencyGenerateJointMissionRequest( __instance.agency, out _ ) )
            i = 1;
         SetInfoState( ___diplomacyOption, i, "Hinting joint mission cooldown." );
      } catch ( Exception x ) { Err( x ); } }

      private static void SetInfoState ( SidebarOption opt, int active, string msg ) { try {
         if ( active < 0 ) active = 0;
         else Fine( msg, active );
         if ( active > 0 ) opt.SetInfoActive( active );
         if ( icoInfo != null ) {
            GetIcon( opt ).sprite = active > 0 ? icoInfo : icoWarn;
            GetHighlight( opt ).color = active > 0 ? clrInfo : clrWarn;
         }
      } catch ( Exception x ) { Err( x ); } }

      private static void HideSpacepediaHint ( SidebarOption ___spacepediaOption ) { try {
         ___spacepediaOption.SetInfoActive( 0 );
      } catch ( Exception x ) { Err( x ); } }

      private static bool IsActive ( SidebarOption opt ) {
         var result = InfoIcon?.GetValue( opt ) is GameObject obj && obj.activeSelf;
         Fine( "Solar System icon {0} is {1}", opt?.name, result ? "Active" : "Inactive" );
         return result;
      }
      private static Image GetIcon ( SidebarOption opt ) => opt.gameObject.GetComponentsInChildren< Image >( true ).First( e => e.sprite.name.StartsWith( "Spr_Icon_" ) );
      private static Image GetHighlight ( SidebarOption opt ) => opt.gameObject.GetComponentsInChildren< Image >( true ).First( e => e.sprite.name.EndsWith( "_Notification" ) );
   }
}