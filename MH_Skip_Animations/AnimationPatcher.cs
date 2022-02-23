using Astronautica.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   internal class AnimationPatcher : Patcher {

      private static Config config => SkipAnimations.config;

      internal void Apply () {
         if ( RootMod.Log.LogLevel == System.Diagnostics.TraceLevel.Verbose )
            Patch( typeof( MonoBehaviour ).Method( "StartCoroutine", typeof( IEnumerator ) ), nameof( LogRoutine ) );
      }

      private static void LogRoutine ( IEnumerator routine ) {
         var name = routine.GetType().FullName;
         if ( name.EndsWith( ".ColorTween]" ) ) return;
         Fine( "Routine {0}", name );
      }
   }
}