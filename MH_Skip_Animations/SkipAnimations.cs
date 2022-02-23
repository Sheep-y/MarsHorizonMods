using Astronautica.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.SkipAnimations {

   public class SkipAnimations : RootMod {

      public static void Main () => new SkipAnimations().Initialize();

      protected override string GetAppDataDir () {
         var path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
         if ( string.IsNullOrEmpty( path ) ) return null;
         return Path.Combine( Directory.GetParent( path ).FullName, "LocalLow", "Auroch Digital", "Mars Horizon" );
      }

      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         new CinematicPatcher().Apply();
      }
   }

   internal class CinematicPatcher : Patcher {

      internal void Apply () {
         IsSkippableField = typeof( CinematicSceneController ).Field( "isSkippable" );
         if ( IsSkippableField != null )
            Patch( typeof( CinematicSceneController ), "GetInputDownSkip", null, nameof( SkipCinmatic ) );
      }

      private static FieldInfo IsSkippableField;

      private static void SkipCinmatic ( ref bool __result, CinematicSceneController __instance ) {
         if ( __result ) return;
         var isSkippable = (bool) IsSkippableField.GetValue( __instance );
         if ( isSkippable ) __result = true;
      }
   }
}
