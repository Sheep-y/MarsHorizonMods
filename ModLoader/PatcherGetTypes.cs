using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using XNode;

namespace ZyMod.MarsHorizon.ModLoader {
   internal class PatcherGetTypes : Patcher {
      internal void PatchXNode () {
         Info( "Patching XNode.NodeDataCache.BuildCache" );
         Patch( typeof( NodeDataCache ), "BuildCache", transpiler: nameof( ReplaceGetTypes ) );
      }

      internal void PatchUMM ( Assembly asm ) {
         var type = asm.GetType( "UnityModManagerNet.UnityModManager" )?.GetNestedType( "ModEntry" );
         if ( type == null ) {
            Warn( "UnityModManager.ModEntry not found in {0}.", asm );
            //foreach ( var t in MarsHorizonModLoader.GetTypes( asm ) ) Fine( t );
            return;
         }
         Info( "Patching Unity Mod Manager." );
         Patch( type, "Load", transpiler: nameof( ReplaceGetTypes ) );
         Patch( type, "Reload", transpiler: nameof( ReplaceGetTypes ) );
      }

      private static IEnumerable< CodeInstruction > ReplaceGetTypes ( IEnumerable< CodeInstruction > codes, MethodBase __originalMethod ) {
         var count = 0;
         var target = typeof( Assembly ).Method( "GetTypes", 0 );
         foreach ( var code in codes ) {
            if ( code.opcode == OpCodes.Callvirt && Equals( code.operand, target ) ) {
               count++;
               yield return new CodeInstruction( OpCodes.Call, typeof( MarsHorizonModLoader ).Method( "GetTypes" ) );
            } else
               yield return code;
         }
         Info( "Safeguarded {0} count(s) of Assembly.GetTypes() in {1}.{2}.", count, __originalMethod.DeclaringType, __originalMethod );
      }
   }
}
