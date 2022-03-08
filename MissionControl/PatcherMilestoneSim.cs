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
         TryPatch( typeof( Simulation ), "SetMilestoneChallengeReward", postfix: nameof( AdjustMilestoneChallengeFund ) );
      }

      private static bool isNewChallenge = true;
      private static readonly HashSet< RewardType > blocks = new HashSet< RewardType >();

      private static void FilterMilestoneChallenge ( Simulation __instance, MilestoneChallenge mChallenge, Agency agency, ref int rValue ) { try {
         if ( agency.isAI || mChallenge == null ) return;
         if ( isNewChallenge ) RefreshMilestoneChallenge( __instance, agency );
         agency.activeMilestoneChallenge = null; // Avoid triggering type duplication change
         Fine( "New milestone challenge is {0}.", (RewardType) rValue );
            if ( blocks.Contains( (RewardType) rValue ) ) {
            var list = Enum.GetValues( typeof( RewardType ) ).OfType< RewardType >().Except( blocks ).ToList();
            Fine( "Valid reward(s): {0}", (Func<string>) ( () => string.Join( ", ", list.Select( e => e.ToString() ) ) ) );
            if ( list.Count == 0 ) throw new InvalidOperationException( "Empty type list." );
            rValue = (int) list.RandomElement();
            Info( "Reroll. New milestone challenge is now {0}.", (RewardType) rValue );
         }
         mChallenge.Reward = (RewardType) rValue; // Make sure everything aligns
      } catch ( Exception x ) { Err( x ); } }

      private static void AdjustMilestoneChallengeFund ( Agency agency, MilestoneChallenge mChallenge ) { try {
         if ( agency.activeMilestoneChallenge == null ) agency.activeMilestoneChallenge = mChallenge;
         var multiplier = config.milestone_challenge_fund_multiplier;
         if ( agency.isAI || mChallenge == null || mChallenge.FundsReward == 0 || multiplier == 1 || multiplier == -1 || multiplier == 0 ) return;
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
            Info( "Blocking fund type milestone rewards: milestone_challenge_fund_multiplier <= 0." );
            blocks.Add( RewardType.Funds );
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