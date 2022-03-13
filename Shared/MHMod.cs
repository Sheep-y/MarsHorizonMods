using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ZyMod.ModHelpers;

// Sheepy's "Universal" skeleton mod and tools.  No depency other than Harmony2 / HarmonyX.
// Bootstrap, Background Logging, Roundtrip Config, Reflection, Manual Patcher with Unpatch. Reasonably well unit tested.
namespace ZyMod.MarsHorizon {

   public abstract class MarsHorizonMod : RootMod {

      public static void Main () { try {
         var first = IsFirstMod();
         foreach ( var t in GetTypes( Assembly.GetExecutingAssembly() ) ) {
            if ( t.IsNotPublic || t.IsAbstract || ! t.IsSubclassOf( typeof( MarsHorizonMod ) ) ) continue;
            var mod = Activator.CreateInstance( t ) as MarsHorizonMod;
            mod.shouldLogAssembly = first;
            mod.Initialize();
            break;
         }
         if ( first ) LoadMarsHorizonMods( GetPath( Assembly.GetExecutingAssembly() ) );
      } catch ( Exception x ) { Error( x ); } }

      private static Type[] GetTypes ( Assembly asm ) { try {
         return asm.GetTypes();
      } catch ( ReflectionTypeLoadException x ) {
         return x.Types;
      } }

      private static bool IsFirstMod () { try {
         foreach ( var asm in AppDomain.CurrentDomain.GetAssemblies() )
            if ( Path.GetFileName( GetPath( asm ) ).StartsWith( "MH_" ) && asm != Assembly.GetExecutingAssembly() ) return false;
         return true;
      } catch ( Exception ) { return true; } }

      private static void LoadMarsHorizonMods ( string selfPath ) { try {
         Fine( "Detecting mod path." );
         var path = Path.GetDirectoryName( selfPath );
         Info( "Scanning \"{0}\" for other Mars Horizon mods (MH_*.dll)", path );
         foreach ( var f in Directory.GetFiles( path, "MH_*.dll" ) ) try {
            if ( f == selfPath ) continue;
            Info( "Loading {0}", f );
            var asm = Assembly.LoadFile( f );
            if ( asm != null ) LoadMarsHorizonMod( asm );
         } catch ( Exception x ) { Warn( x ); }
      } catch ( Exception x ) { Error( x ); } }

      private static void LoadMarsHorizonMod ( Assembly asm ) {
         foreach ( var t in GetTypes( asm ) ) {
            if ( t.IsNotPublic ) continue;
            var main = t.Methods().FirstOrDefault( e => e.Name == "Main" && e.IsStatic && e.IsPublic && e.GetParameters().Length == 0 && ! e.IsGenericMethod );
            if ( main == null ) continue;
            Fine( "Calling {0} of {1}", main, main.DeclaringType.FullName );
            main.RunStatic();
            return;
         }
         Warn( "Cannot find public static void Main() from {0}", asm.CodeBase );
      }

      private static string GetPath ( Assembly asm ) => new Uri( asm.CodeBase ).LocalPath;

      protected override string GetAppDataDir () {
         var path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
         if ( string.IsNullOrEmpty( path ) ) return null;
         return Path.Combine( Directory.GetParent( path ).FullName, "LocalLow", "Auroch Digital", "Mars Horizon" );
      }

   }

}