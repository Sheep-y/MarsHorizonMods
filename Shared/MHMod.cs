using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityModManagerNet;
using static System.Reflection.BindingFlags;

// Common base mod for Mars Horizon.  Find and initialize the correct class, set log and config path and name.
namespace ZyMod.MarsHorizon {

   internal abstract class MarsHorizonPatcher : Patcher {

      private static Dictionary< Type, Dictionary< FieldInfo, object > > DefaultFieldValues;
      private IEnumerable< FieldInfo > ResetFields => GetType().GetFields( Static | NonPublic | DeclaredOnly );

      internal MarsHorizonPatcher () {
         if ( DefaultFieldValues?.ContainsKey( GetType() ) == true ) return;
         Dictionary< FieldInfo, object > vals = null;
         foreach ( var e in ResetFields ) {
            if ( e.IsInitOnly ) continue;
            if ( vals == null ) {
               vals = new Dictionary< FieldInfo, object >();
               if ( DefaultFieldValues == null ) DefaultFieldValues = new Dictionary< Type, Dictionary< FieldInfo, object > >();
               DefaultFieldValues[ GetType() ] = vals;
            }
            vals[ e ] = e.GetValue( null );
         }
      }

      internal abstract void Apply();

      internal virtual void Unapply() { try { // May be called multiple times
         Dictionary< FieldInfo, object > defVals = null;
         if ( DefaultFieldValues?.TryGetValue( GetType(), out defVals ) != true ) return;
         foreach ( var e in ResetFields ) {
            object curr = e.GetValue( null );
            var hasDef = defVals.TryGetValue( e, out var defVal );
            if ( curr != null && ! hasDef && curr.GetType().Namespace == "System.Collections.Generic" ) {
               Fine( "Reset {0}.{1} call Clear().", GetType().Name, e.Name );
               curr.GetType().Method( "Clear", 0 )?.Run( curr );
            } else if ( ! e.IsInitOnly && ! Equals( curr, defVal ) ) {
               e.SetValue( this, defVal );
               Fine( "Reset {0}.{1} to {2}", GetType().Name, e.Name, defVal ?? "null" );
            }
         }
      } catch ( Exception x ) { Err( x ); } }

      internal virtual void Unload() { } // Does not call Unapply. MarsHorizonMod.Unload will call Unapply. 
   }

   public abstract class MarsHorizonMod : RootMod {

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
         Fine( "Unapplying individual patches." );
         foreach ( var p in patchers.Values ) p.Unapply();
         return true;
      } catch ( Exception x ) { return Err( x, false ); } }

      internal static void ReApply () { if ( Unapply() ) Apply(); }

      internal static bool Unload () { try {
         Unapply();
         Info( "Unloading." );
         foreach ( var p in patchers.Values ) p.Unload();
         patchers.Clear();
         lock ( sync ) if ( ZyLog != null ) {
            ZyLog.Flush();
            ZyLog.LogLevel = TraceLevel.Off;
            ZyLog = null;
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

   internal class BepInUtil : Patcher {
      internal static void Setup ( BaseUnityPlugin mod, BaseConfig config = null ) {
         lock( sync ) { BepInUtil.mod = mod; ModPath = mod.Info.Location; } // ModPath is null when loaded by ScriptEngine
         SetLogger( typeof( BaseUnityPlugin ).Property( "Logger" ).GetValue( mod ) as ManualLogSource );
         if ( config != null ) new BepInUtil().PatchConfig( config );
      }

      private static void SetLogger ( ManualLogSource logger ) {
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

      private void PatchConfig ( BaseConfig conf ) {
         modConfig = conf;
         Type bcType = typeof( BaseConfig ), strType = typeof( string );
         Patch( bcType.Method( "OnLoading", strType ), prefix: nameof( LoadFromBep ) );
         Patch( bcType.Method( "OnSaving", strType ), prefix: nameof( SaveToBep ) );
      }

      internal static void Unbind () {
         MarsHorizonMod.Unload();
         bindings?.Clear();
         mod = null;
      }

      private static BaseUnityPlugin mod;
      private static BaseConfig modConfig;
      private static Dictionary< FieldInfo, ConfigEntryBase > bindings;

      internal static bool LoadFromBep ( ref bool __result ) { try { // Replaces BaseConfig.OnLoading
         __result = false;
         lock ( sync ) if ( bindings == null ) {
            var bind = typeof( ConfigFile ).Methods( "Bind" ).First( e =>
               e.GetParameters().Length == 4 && e.GetParameters()[ 3 ].ParameterType == typeof( string ) );
            var fields = typeof( BaseConfig ).Method( "_ListFields" ).Run( modConfig, modConfig ) as IEnumerable< FieldInfo >;
            var section = "General";
            Fine( "Creating BepInEx config bindings." );
            foreach ( var f in fields ) BindConfigField( mod.Config, f, bind, ref section );
            if ( bindings == null ) return false;
            mod.Config.ConfigReloaded += ReloadConfig;
         }
         LoadConfig( false );
         ValidateConfig();
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }

      private static void BindConfigField ( ConfigFile Config, FieldInfo f, MethodInfo bind, ref string section ) {
         var tags = f.GetCustomAttributes( true ).OfType<ConfigAttribute>();
         ConfigAttribute sec = tags.FirstOrDefault( e => e.Comment?.Contains( "[" ) == true ), desc = tags.LastOrDefault();
         if ( sec != null ) section = sec.Comment.Split( '[' )[ 1 ].Trim( ']' );
         var defVal = f.GetValue( modConfig );
         var b = bind.MakeGenericMethod( f.FieldType ).Run( Config, section, f.Name, defVal, desc?.Comment ?? "" ) as ConfigEntryBase;
         if ( b == null ) return;
         b.GetType().GetEvent( "SettingChanged" )?.AddEventHandler( b, (EventHandler) ReloadConfigField );
         if ( bindings == null ) bindings = new Dictionary< FieldInfo, ConfigEntryBase >();
         bindings.Add( f, b );
      }

      internal static void ReloadConfigField ( object _, object evt ) { // Listen to ConfigEntry.SettingChanged
         if ( ! ( evt is SettingChangedEventArgs e ) || e.ChangedSetting == null ) return;
         var b = e.ChangedSetting;
         lock ( sync ) {
            var field = bindings.Keys.FirstOrDefault( f => f.Name == b.Definition.Key );
            object bVal = b.BoxedValue, myVal = field?.GetValue( modConfig );
            if ( field == null || Equals( bVal, myVal ) ) return;
            Info( "Config {0} changed to {1} by BepInEx.", field.Name, bVal );
            field.SetValue( modConfig, bVal );
         }
         ScheduleReapply();
      }

      internal static void ReloadConfig ( object _, object evt ) => LoadConfig( true ); // Listen to Config.ConfigReloaded
      internal static void LoadConfig ( bool reapply ) {
         Info( "Syncing config through BepInEx from {0}.", mod.Config.ConfigFilePath );
         lock ( sync ) foreach ( var b in bindings ) {
            object bVal = b.Value.BoxedValue, myVal = b.Key.GetValue( modConfig );
            var same = Equals( bVal, myVal );
            if ( ! same || ! reapply ) Fine( "Config {0} = {1}", b.Key.Name, bVal );
            if ( same ) continue;
            b.Key.SetValue( modConfig, bVal );
         }
         if ( reapply ) ScheduleReapply();
      }

      private static Task ReapplyMod;

      private static void ScheduleReapply () { lock ( sync ) { // Called after config is changed or reloaded.
         if ( ReapplyMod != null || mod == null ) return;
         ReapplyMod = Task.Run( async () => {
            await Task.Delay( 2000 ); // BepInEx.ConfigurationManager will refresh config in realtime.
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
         typeof( BaseConfig ).Method( "OnLoad", typeof( string ) )?.Run( modConfig, mod?.Config.ConfigFilePath );
         SyncToBep();
      }

      private static void SyncToBep () { // Called after OnLoad fixed things, or on BaseConfig.Save
         lock ( sync ) foreach ( var b in bindings ) {
            object bVal = b.Value.BoxedValue, myVal = b.Key.GetValue( modConfig );
            if ( Equals( bVal, myVal ) ) continue;
            Fine( "Sync Config {0} ({1}) to BepInEx ({2}).", b.Key.Name, myVal, bVal );
            b.Value.BoxedValue = myVal;
         }
      }
 
      private static bool SaveToBep ( ref bool __result ) { try { // Replaces BaseConfig.OnSaving
         __result = false;
         SyncToBep();
         return false;
      } catch ( Exception x ) { return Err( x, false ); } }
   }

   internal class UMMUtil : ModComponent {
      internal static void Init ( object modEntry, Type modType ) {
         var mod = modEntry as UnityModManager.ModEntry;
         if ( mod != null ) lock ( sync ) ModDir = mod.Path;
         modType.Method( "Main" ).RunStatic();
         if ( mod == null ) return;
         mod.OnToggle = ( _, on ) => { if ( on ) MarsHorizonMod.Apply(); else MarsHorizonMod.Unapply(); return true; };
         mod.OnUnload = ( _ ) => MarsHorizonMod.Unload();
      }
   }
}