using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

// Common base mod for Mars Horizon.  Find and initialize the correct class, set log and config path and name.
namespace ZyMod.MarsHorizon {

   internal abstract class MarsHorizonPatcher : Patcher {
      internal abstract void Apply();
      internal virtual void Unapply() { } // May be called multiple times.
      internal virtual void Unload() { } // Does not call Unapply. MarsHorizonMod.Unload will call Unapply. 
   }

   public abstract class MarsHorizonMod : RootMod {

      internal static bool configLoaded;

      internal static readonly Dictionary< Type, MarsHorizonPatcher > patchers = new Dictionary< Type, MarsHorizonPatcher >();

      internal static void ActivatePatcher ( Type type ) {
         if ( patchers.TryGetValue( type, out var result ) ) { result.Apply(); return; }
         Fine( "Creating {0}.", type.Name );
         if ( ! ( Activator.CreateInstance( type ) is MarsHorizonPatcher patcher ) ) throw new ArgumentException();
         patchers.Add( type, patcher );
         patcher.Apply();
      }

      internal static void Apply () { try {
         var mod = instance as MarsHorizonMod;
         lock ( sync ) mod?.OnGameAssemblyLoaded( null );
         mod?.CountPatches();
      } catch ( Exception x ) { Err( x ); } }

      internal static bool Unapply () { try {
         patchers.Values.FirstOrDefault( e => e.harmony != null )?.UnpatchAll();
         foreach ( var p in patchers.Values ) p.Unapply();
         return true;
      } catch ( Exception x ) { return Err( x, false ); } }

      internal static void ReApply () { if ( Unapply() ) Apply(); }

      internal static bool Unload () { try {
         Unapply();
         Info( "Unloading." );
         foreach ( var p in patchers.Values ) p.Unload();
         patchers.Clear();
         lock ( sync ) if ( Log != null ) {
            Log.Flush();
            Log.LogLevel = TraceLevel.Off;
            Log = null;
         }
         Logger = null;
         return true;
      } catch ( Exception x ) { return Err( x, false ); } }

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

   internal class BepInUtil : ModComponent {
      internal static void Setup ( BaseUnityPlugin mod, BaseConfig config = null ) {
         lock( sync ) { BepInUtil.mod = mod; ModPath = mod.Info.Location; } // ModPath is null when loaded by ScriptEngine
         SetLogger( typeof( BaseUnityPlugin ).Property( "Logger" ).GetValue( mod ) as ManualLogSource );
         BindConfig( mod.Config, config );
      }

      internal static void Unbind () {
         MarsHorizonMod.Unload();
         bindings?.Clear();
         mod = null;
      }

      private static BaseUnityPlugin mod;
      private static BaseConfig modConfig;
      private static Dictionary< FieldInfo, object > bindings;

      internal static void BindConfig ( ConfigFile Config, BaseConfig conf ) { lock ( sync ) try {
         if ( Config == null || conf == null ) return;
         modConfig = conf;
         Type type = conf.GetType(), strT = typeof( string );
         var fields = typeof( BaseConfig ).Method( "_ListFields" ).Run( conf, conf ) as IEnumerable< FieldInfo >;
         var bind = typeof( ConfigFile ).Methods( "Bind" ).First( e =>
            e.GetParameters().Length == 4 && e.GetParameters()[ 3 ].ParameterType == typeof( string ) );
         var section = "";
         bindings?.Clear();
         Info( "Loading config through BepInEx from {0}.", Config.ConfigFilePath );
         foreach ( var f in fields ) BindConfigField( Config, f, bind, ref section );
         new Harmony( ModName + ".Bep" ).Patch( type.Method( "OnSaving", strT ) ?? typeof( BaseConfig ).Method( "OnSaving", strT ),
            prefix: new HarmonyMethod( typeof( BepInUtil ).Method( nameof( SaveToBep ) ) ) );
         ValidateConfig();
         if ( bindings?.Count > 0 ) Config.ConfigReloaded += GetOnReloadListener();
         MarsHorizonMod.configLoaded = true;
      } catch ( Exception x ) { Err( x ); } }

      private static void BindConfigField ( ConfigFile Config, FieldInfo f, MethodInfo bind, ref string section ) {
         if ( f.Name == "config_version" ) return;
         var tags = f.GetCustomAttributes( true ).OfType<ConfigAttribute>();
         ConfigAttribute sec = tags.FirstOrDefault( e => e.Comment?.EndsWith( "]" ) == true ), desc = tags.LastOrDefault();
         if ( sec?.Comment.Contains( '[' ) == true ) section = sec.Comment.Split( '[' )[ 1 ].Trim( ']' );
         var defVal = f.GetValue( modConfig );

         var b = bind.MakeGenericMethod( f.FieldType ).Run( Config, section, f.Name, defVal, desc?.Comment ?? "" );
         if ( ! TryGetValues( null, b, out var val, out _ ) ) return;
         if ( ! Equals( val, defVal ) ) f.SetValue( modConfig, val );
         Fine( "Config {0} = {1}", f.Name, val );

         b.GetType().GetEvent( "SettingChanged" )?.AddEventHandler( b, GetOnChangeListener( f ) );
         if ( bindings == null ) bindings = new Dictionary< FieldInfo, object >();
         bindings.Add( f, b );
      }

      private static bool TryGetValues ( FieldInfo confField, object binding, out object bVal, out object myVal ) {
         var box = binding.GetType().Property( "BoxedValue" );
         if ( box == null ) { Warn( "Cannot get BoxedValue from {0}.", binding ); bVal = myVal = null; return false; }
         bVal = box.GetValue( binding );
         myVal = confField?.GetValue( modConfig );
         return true;
      }

      internal static EventHandler GetOnChangeListener ( FieldInfo field ) { return ( _, evt ) => {
         if ( ! ( evt is SettingChangedEventArgs e ) || e.ChangedSetting == null ) return;
         lock ( sync ) {
            object bVal = e.ChangedSetting.BoxedValue, myVal = field.GetValue( modConfig );
            if ( Equals( bVal, myVal ) ) return;
            Info( "Config {0} changed to {1} by BepInEx.", field.Name, bVal );
            field.SetValue( modConfig, bVal );
         }
         ScheduleReapply();
      }; }

      internal static EventHandler GetOnReloadListener () { return ( _, evt ) => {
         Info( "Config reloaded by BepInEx." );
         lock ( sync ) foreach ( var b in bindings ) {
            if ( ! TryGetValues( b.Key, b.Value, out var bVal, out var myVal ) || Equals( bVal, myVal ) ) continue;
            Fine( "Config {0} = {1}", b.Key.Name, bVal );
            b.Key.SetValue( modConfig, bVal );
         }
         ScheduleReapply();
      }; }

      private static Task ReapplyMod;

      private static void ScheduleReapply () { lock ( sync ) {
         if ( ReapplyMod != null || mod == null ) return;
         ReapplyMod = Task.Run( async () => {
            await Task.Delay( 50 );
            mod.StartCoroutine( ReapplyConfig() );
         } );
      } }

      private static IEnumerator ReapplyConfig () {
         lock ( sync ) ReapplyMod = null;
         ValidateConfig();
         MarsHorizonMod.ReApply();
         yield break;
      }

      private static void ValidateConfig () {
         modConfig.GetType().Method( "OnLoad", typeof( string ) )?.Run( modConfig, mod?.Config.ConfigFilePath );
         SyncToBep();
      }

      private static void SyncToBep () {
         foreach ( var b in bindings ) {
            if ( ! TryGetValues( b.Key, b.Value, out var bVal, out var myVal ) || Equals( bVal, myVal ) ) continue;
            Fine( "Sync Config {0} ({1}) to BepInEx ({2}).", b.Key.Name, myVal, bVal );
            b.Value.GetType().Property( "BoxedValue" ).SetValue( b.Value, myVal );
         }
      }
 
      private static bool SaveToBep ( ref bool __result ) { try {
         __result = false;
         lock ( sync ) SyncToBep();
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      internal static void SetLogger ( ManualLogSource logger ) {
         if ( logger == null ) return;
         Logger = ( TraceLevel lv, object msg, object[] arg ) => {
            object obj = msg is Exception ? msg : ZyLogger.DefaultFormatter( null, msg, arg );
            switch ( lv ) {
               case TraceLevel.Verbose : logger.LogDebug( obj ); break;
               case TraceLevel.Info : logger.LogInfo( obj ); break;
               case TraceLevel.Warning : logger.LogWarning( obj ); break;
               case TraceLevel.Error : logger.LogError( obj ); break;
            }
         };
      }
   }

   internal class UMMUtil : ModComponent {
      internal static void Init ( UnityModManager.ModEntry modEntry, Type modType ) {
         lock ( sync ) ModDir = modEntry.Path;
         modType.Method( "Main" ).RunStatic();
         modEntry.OnToggle = ( _, on ) => { if ( on ) MarsHorizonMod.Apply(); else MarsHorizonMod.Unapply(); return true; };
         modEntry.OnUnload = ( _ ) => MarsHorizonMod.Unload();
      }
   }
}