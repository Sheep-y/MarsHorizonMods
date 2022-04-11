using Astronautica;
using Astronautica.Model.MilestoneChallenge;
using Astronautica.TechTrees;
using Astronautica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static Astronautica.Model.MilestoneChallenge.MilestoneChallenge;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.MissionControl {

   internal class PatcherMilestoneSim: ModPatcher {
      internal override void Apply () {
         Patch( typeof( Simulation ), "GetNewMilestoneChallenge", prefix: nameof( CaptureCurrentReward ), postfix: nameof( ClearMilestoneChecks ) );
         Patch( typeof( Simulation ), "SetMilestoneChallengeReward", prefix: nameof( FilterMilestoneChallenge ) );
         Patch( typeof( Simulation ), "SetMilestoneChallengeReward", postfix: nameof( AdjustMilestoneChallengeFund ) );
      }

      private static bool isNewChallenge = true;
      private static int lastReward = -1;
      private static readonly HashSet< RewardType > blocks = new HashSet< RewardType >();

      private static void CaptureCurrentReward ( Agency agency )
         => lastReward = agency?.activeMilestoneChallenge == null ? -1 : (int) agency.activeMilestoneChallenge.Reward;

      private static void FilterMilestoneChallenge ( Simulation __instance, MilestoneChallenge mChallenge, Agency agency, ref int rValue ) { try {
         if ( mChallenge == null || agency == null ) return;
         if ( config.change_only_player_agency && agency.isAI ) return;
         if ( isNewChallenge ) RefreshMilestoneChallenge( __instance, agency );
         agency.activeMilestoneChallenge = null; // Avoid triggering type duplication change
         Fine( "{0} new milestone challenge rewards {1}.", agency.NameLocalised, (RewardType) rValue );
         if ( blocks.Contains( (RewardType) rValue ) ) {
            var list = Enum.GetValues( typeof( RewardType ) ).OfType< RewardType >().Except( blocks ).ToList();
            Fine( "Valid reward(s): {0}", (Func<string>) ( () => string.Join( ", ", list.Select( e => e.ToString() ) ) ) );
            if ( list.Count == 0 ) throw new InvalidOperationException( "Empty reward type list." );
            rValue = (int) list.RandomElement();
            Info( "Reroll. New milestone challenge now rewards {0}.", (RewardType) rValue );
         }
         mChallenge.Reward = (RewardType) rValue; // Make sure everything aligns
      } catch ( Exception x ) { Err( x ); } }

      private static void AdjustMilestoneChallengeFund ( Agency agency, MilestoneChallenge mChallenge ) { try {
         if ( agency == null ) return;
         if ( agency.activeMilestoneChallenge == null ) agency.activeMilestoneChallenge = mChallenge;
         if ( config.change_only_player_agency && agency.isAI ) return;
         var multiplier = Math.Abs( config.milestone_challenge_fund_multiplier );
         if ( mChallenge == null || mChallenge.FundsReward == 0 || ! Rational( multiplier ) || multiplier == 1 || multiplier == 0 ) return;
         var newFund = (int) Math.Round( mChallenge.FundsReward * multiplier );
         Info( "Fund rewards of {0}: {1} => {2}", mChallenge.Id, mChallenge.FundsReward, newFund );
         mChallenge.FundsReward = newFund;
      } catch ( Exception x ) { Err( x ); } }

      private static void RefreshMilestoneChallenge ( Simulation sim, Agency agency ) { try {
         isNewChallenge = false;
         Fine( "New milestone challenge." );
         var highpass = config.milestone_challenge_research_highpass;
         if ( highpass >= 0 && sim != null ) {
            blocks.Clear();
            var researchCountByType = new Dictionary< TechTree.Type, int >( 3 );
            foreach ( var id in agency.researchProgress.Keys ) {
               var type = sim.GetTechTreeType( sim.GetNodeFromResearch( id, true ) );
               if ( ! researchCountByType.TryGetValue( type, out var count ) ) count = 0;
               researchCountByType[ type ] = count + 1;
            }
            foreach ( var row in researchCountByType ) if ( row.Key != TechTree.Type.None ) Fine( "{0} research = {1} total", row.Key, row.Value );
            if ( ShouldBlock( agency.missionResearchCompletedCount, researchCountByType, TechTree.Type.Missions ) )
               blocks.Add( RewardType.MissionResearch );
            if ( ShouldBlock( agency.baseResearchCompletedCount, researchCountByType, TechTree.Type.Base ) )
               blocks.Add( RewardType.BaseResearch );
            if ( ShouldBlock( agency.vehicleResearchCompletedCount, researchCountByType, TechTree.Type.Vehicles ) )
               blocks.Add( RewardType.VehicleResearch );
         }
         if ( config.milestone_challenge_fund_multiplier <= 0 && blocks.Count < 3 ) {
            Info( "Blocking Fund type milestone rewards: milestone_challenge_fund_multiplier <= 0" );
            blocks.Add( RewardType.Funds );
         }
         if ( config.milestone_challenge_no_duplicate_reward && blocks.Count < 3 && lastReward >= 0 ) {
               Info( "Blocking {0} type milestone rewards: milestone_challenge_no_duplicate_reward = true", (RewardType) lastReward );
               blocks.Add( (RewardType) lastReward );
         }
      } catch ( Exception x ) { Err( x ); } }

      private static bool ShouldBlock ( int completed, Dictionary< TechTree.Type, int > total, TechTree.Type type ) {
         if ( ! total.TryGetValue( type, out var count ) ) return false;
         var result = completed + config.milestone_challenge_research_highpass >= count;
         Log( result ? TraceLevel.Info : TraceLevel.Verbose,
            "{0} {1} type milestone rewards: {2}/{3}", result ? "Blocking" : "Allowing", type, completed, count );
         return result;
      }

      private static void ClearMilestoneChecks () => isNewChallenge = true;
   }
}