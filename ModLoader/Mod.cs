using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZyMod.MarsHorizon.ModLoader {
   public class MarsHorizonModLoader : RootMod {
      protected override string GetModName () => "ModLoader";

      public static void Main () => new MarsHorizonModLoader().Initialize();

      protected override void OnGameAssemblyLoaded ( Assembly game ) { try {
         // As of March 2022: Game version 1.4.1.0.  Unity version 2019.4.9f1.
         Info( "Game version {0}.  Unity version {1}.", Application.version, Application.unityVersion );
         PatchUMM();
         Fine( "Detecting mod path." );
         var selfPath = GetPath( Assembly.GetExecutingAssembly() );
         var path = Path.GetDirectoryName( selfPath );
         Fine( "Mod Loader = {0}", selfPath );
         if ( path.EndsWith( ".cache" ) ) path = new Regex( "\\.\\d+\\.cache$" ).Replace( path, "" ); // UMM
         Info( "Scanning \"{0}\" for Mars Horizon mods (MH_*.dll).", path );
         FindMods( selfPath, path, 0 );
      } catch ( Exception x ) { Error( x ); } }

      private static void FindMods ( string selfPath, string path, int depth ) {
         if ( depth >= 3 ) return;

         var files = Directory.GetFiles( path, "MH_*.dll" );
         Array.Sort( files );
         Fine( "Found {0} mod files in {1}", files.Length, path );
         foreach ( var f in files ) try {
            if ( f == selfPath ) continue;
            Info( "Loading {0}", f );
            var asm = Assembly.LoadFile( f );
            if ( asm != null ) LoadMarsHorizonMod( asm );
         } catch ( Exception x ) { Warn( x ); }

         files = Directory.GetDirectories( path );
         Array.Sort( files );
         foreach ( var f in files ) FindMods( selfPath, f, depth + 1 );
      }

      internal static Type[] GetTypes ( Assembly asm ) { try {
         return asm.GetTypes();
      } catch ( ReflectionTypeLoadException x ) {
         return x.Types.Where( t => t != null ).ToArray();
      } }

      private static bool PatchedXNode;

      private static void LoadMarsHorizonMod ( Assembly asm ) {
         foreach ( var t in GetTypes( asm ) ) {
            if ( t.IsNotPublic ) continue;
            var main = t.Methods().FirstOrDefault( e => e.Name == "Main" && e.IsStatic && e.IsPublic && e.GetParameters().Length == 0 && ! e.IsGenericMethod );
            if ( main == null ) continue;
            if ( ! PatchedXNode ) PatchXNode();
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

      private static void PatchXNode () { try {
         PatchedXNode = true;
         new PatcherGetTypes().PatchXNode();
      } catch ( Exception x ) { Error( x ); } }

      private static void PatchUMM () { try {
         var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( e => e.GetName().Name.StartsWith( "UnityModManager" ) );
         if ( asm != null ) new PatcherGetTypes().PatchUMM( asm );
      } catch ( Exception x ) { Error( x ); } }
   }
}