using HarmonyLib;
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

      private static IEnumerable< CodeInstruction > ReplaceGetTypes ( IEnumerable< CodeInstruction > codes ) {
         var count = 0;
         foreach ( var code in codes ) {
            if ( code.opcode == OpCodes.Callvirt && code.operand is MethodBase m &&
                      m.Name == "GetTypes" && m.DeclaringType == typeof( Assembly ) && m.GetParameters().Length == 0 ) {
               count++;
               code.opcode = OpCodes.Call;
               code.operand = typeof( MarsHorizonModLoader ).Method( "GetTypes" );
            }
            yield return code;
         }
         Info( "Safeguarded {0} count(s) of Assembly.GetTypes().", count );
      }
   }
}
