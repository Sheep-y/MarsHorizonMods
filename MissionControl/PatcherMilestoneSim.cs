using Astronautica;
using Astronautica.Model.MilestoneChallenge;
using Astronautica.TechTrees;
using Astronautica.View;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static Astronautica.Model.MilestoneChallenge.MilestoneChallenge;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.MissionControl {

   internal class PatcherMilestoneSim: ModPatcher {
      internal void Apply () {
         TryPatch( typeof( Simulation ), "GetNewMilestoneChallenge", postfix: nameof( ClearMilestoneChecks ) );
         TryPatch( typeof( Simulation ), "SetMilestoneChallengeReward", prefix: nameof( FilterMilestoneChallenge ) );
         if ( config.milestone_challenge_fund_multiplier != 1 && config.milestone_challenge_fund_multiplier != 0 && config.milestone_challenge_fund_multiplier != -1 )
            TryPatch( typeof( Simulation ), "SetMilestoneChallengeReward", postfix: nameof( AdjustMilestoneChallengeFund ) );
      }

      private static bool isNewChallenge = true;
      private static readonly HashSet< RewardType > blocks = new HashSet< RewardType >();

      private static void FilterMilestoneChallenge ( Simulation __instance, MilestoneChallenge mChallenge, Agency agency, ref int rValue ) { try {
         if ( agency.isAI || mChallenge == null ) return;
         if ( isNewChallenge ) RefreshMilestoneChallenge( __instance, agency );
         var oldChallenge = agency.activeMilestoneChallenge;
         var oldVal = oldChallenge == null ? -1 : (int) oldChallenge.Reward;
         Fine( "Last challenge was {0}.  New challenge will be {1}.", oldChallenge?.Reward.ToString() ?? "null", (RewardType) rValue );
         var rTypes = Enum.GetValues( typeof( RewardType ) );
         if ( blocks.Contains( (RewardType) rValue ) ) {
            var list = rTypes.OfType< RewardType >().Except( blocks ).Where( e => (int) e != oldVal ).ToList();
            Fine( "Valid type(s): {0}", (Func<string>) ( () => string.Join( ", ", list.Select( e => e.ToString() ) ) ) );
            if ( list.Count == 0 ) { Error( new InvalidOperationException( "Empty type list." ) ); return; }
            var newVal = list.RandomElement();
            Info( "Reroll. New challenge will now be {0}.", newVal );
            rValue = (int) newVal;
         }
         // The increment code seems to always get triggered.
         Fine( "Reducing  {0} {1}", rValue, mChallenge.Reward );
         rValue = rValue == 0 ? rTypes.Length - 1 : rValue - 1;
         mChallenge.Reward = (RewardType) rValue;
      } catch ( Exception x ) { Err( x ); } }

      private static void AdjustMilestoneChallengeFund ( Agency agency, MilestoneChallenge mChallenge ) { try {
         Fine( mChallenge.Reward );
         if ( agency.isAI || mChallenge == null || mChallenge.FundsReward == 0 ) return;
         var newFund = (int) Math.Round( mChallenge.FundsReward * Math.Abs( config.milestone_challenge_fund_multiplier ) );
         Info( "Fund rewards of {0}: {1} => {2}", mChallenge.Id, mChallenge.FundsReward, newFund );
         mChallenge.FundsReward = newFund;
      } catch ( Exception x ) { Err( x ); } }

      private static void RefreshMilestoneChallenge ( Simulation sim, Agency agency ) { try {
         Fine( "New milestone challenge." );
         isNewChallenge = false;
         var highpass = config.milestone_challenge_research_highpass;
         if ( highpass >= 0 ) {
            blocks.Clear();
            var researchCountByType = new Dictionary< TechTree.Type, int >( 3 );
            foreach ( var id in agency.researchProgress.Keys ) {
               var type = sim.GetTechTreeType( sim.GetNodeFromResearch( id, true ) );
               if ( ! researchCountByType.TryGetValue( type, out var count ) ) count = 0;
               researchCountByType[ type ] = count + 1;
            }
            foreach ( var row in researchCountByType ) if ( row.Key != TechTree.Type.None ) Fine( "{0} = {1} total", row.Key, row.Value );
            if ( ShouldBlock( agency.missionResearchCompletedCount, researchCountByType, TechTree.Type.Missions ) )
               blocks.Add( RewardType.MissionResearch );
            if ( ShouldBlock( agency.baseResearchCompletedCount, researchCountByType, TechTree.Type.Base ) )
               blocks.Add( RewardType.BaseResearch );
            if ( ShouldBlock( agency.vehicleResearchCompletedCount, researchCountByType, TechTree.Type.Vehicles ) )
               blocks.Add( RewardType.VehicleResearch );
         }
         if ( config.milestone_challenge_fund_multiplier <= 0 && blocks.Count < 3 ) {
            Info( "Blocking fund type milestone rewards: milestone_challenge_fund_multiplier = 0." );
            blocks.Add( RewardType.Funds );
         }
         var current = agency.activeMilestoneChallenge.Reward;
         if ( blocks.Count == 3 && agency.activeMilestoneChallenge != null && ! blocks.Contains( current ) ) {
            agency.activeMilestoneChallenge.Reward = blocks.RandomElement();
            Fine( "Current reward type changed from {0} to {1} due to blockage.", current, agency.activeMilestoneChallenge.Reward );
         }
      } catch ( Exception x ) { Err( x ); } }

      private static bool ShouldBlock ( int completed, Dictionary< TechTree.Type, int > total, TechTree.Type type ) {
         var result = completed + config.milestone_challenge_research_highpass>= total[ type ];
         RootMod.Log.Write( result ? TraceLevel.Info : TraceLevel.Verbose,
            "{0} {1} type milestone rewards: {2}/{3}", result ? "Blocking" : "Allowing", type, completed, total[ type ] );
         return result;
      }

      private static void ClearMilestoneChecks () => isNewChallenge = true;
   }
}