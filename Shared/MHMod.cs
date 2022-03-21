using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

// Common base mod for Mars Horizon.  Find and initialize the correct class, set log and config path and name.
namespace ZyMod.MarsHorizon {

   public abstract class MarsHorizonMod : RootMod {

      public static void Main () { try {
         foreach ( var t in GetTypes( Assembly.GetExecutingAssembly() ) ) {
            if ( t.IsNotPublic || t.IsAbstract || ! t.IsSubclassOf( typeof( MarsHorizonMod ) ) ) continue;
            var mod = Activator.CreateInstance( t ) as MarsHorizonMod;
            mod.shouldLogAssembly = false;
            mod.Initialize();
            break;
         }
      } catch ( Exception x ) { Error( x ); } }

      private static Type[] GetTypes ( Assembly asm ) { try {
         return asm.GetTypes();
      } catch ( ReflectionTypeLoadException x ) {
         return x.Types;
      } }

      protected override string GetAppDataDir () {
         var path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
         if ( string.IsNullOrEmpty( path ) ) return null;
         return Path.Combine( Directory.GetParent( path ).FullName, "LocalLow", "Auroch Digital", "Mars Horizon" );
      }

      internal static string Localise ( string tag, params string[] vars ) {
         Dictionary< string, string > variables = null;
         if ( vars?.Length > 0 ) {
            variables = new Dictionary< string, string >();
            for ( var i = 0 ; i + 1 < vars.Length ; i += 2 )
               variables.Add( vars[ i ], vars[ i + 1 ] );
         }
         return ScriptableObjectSingleton<Localisation>.instance.Localise( tag, variables );
      }
   }
}