using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using static ZyMod.ModComponent;

// Common base mod for Mars Horizon.  Find and initialize the correct class, set log and config path and name.
namespace ZyMod.MarsHorizon {

   internal abstract class MarsHorizonPatcher : Patcher {
      internal abstract void Apply();
      internal virtual void Unapply() { } // May be called multiple times.
      internal virtual void Unload() { } // Does not call Unapply. MarsHorizonMod.Unload will call Unapply. 
   }

   public abstract class MarsHorizonMod : RootMod {

      internal static bool configLoaded = true;

      internal static readonly Dictionary< Type, MarsHorizonPatcher > patchers = new Dictionary< Type, MarsHorizonPatcher >();

      internal static void ActivatePatcher ( Type type ) {
         if ( patchers.TryGetValue( type, out var result ) ) { result.Apply(); return; }
         Fine( "Creating {0}.", type.Name );
         if ( ! ( Activator.CreateInstance( type ) is MarsHorizonPatcher patcher ) ) throw new ArgumentException();
         patchers.Add( type, patcher );
         patcher.Apply();
      }

      internal static void Apply () => ( instance as MarsHorizonMod )?.OnGameAssemblyLoaded( null );

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
         if ( Log != null ) lock ( sync ) {
            Log.Flush();
            Log.LogLevel = TraceLevel.Off;
            Log = null;
         }
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

   internal class BepInUtil {
      internal void Setup ( BaseUnityPlugin mod, BaseConfig config = null ) {
         SetLogger( typeof( BaseUnityPlugin ).Property( "Logger" ).GetValue( mod ) as ManualLogSource );
         BindConfig( mod.Config, config );
         if ( mod.Info.Location != null ) Info( mod.Info.Location );
      }

      private Dictionary< string, object > configBindings;

      internal void BindConfig ( ConfigFile Config, BaseConfig conf ) { try {
         if ( Config == null || conf == null ) return;
         var type = conf.GetType();
         var fields = typeof( BaseConfig ).Method( "_ListFields" ).Run( conf, conf ) as IEnumerable< FieldInfo >;
         var bind = typeof( ConfigFile ).Methods( "Bind" ).First( e =>
            e.GetParameters().Length == 4 && e.GetParameters()[ 3 ].ParameterType == typeof( string ) );
         var section = "";
         configBindings?.Clear();
         Info( "Creating BepInEx config bindings." );
         foreach ( var f in fields ) {
            if ( f.Name == "config_version" ) continue;
            var tags = f.GetCustomAttributes( true ).OfType<ConfigAttribute>();
            ConfigAttribute sec = tags.FirstOrDefault( e => e.Comment?.EndsWith( "]" ) == true ), desc = tags.LastOrDefault();
            if ( sec?.Comment.Contains( '[' ) == true ) section = sec.Comment.Split( '[' )[ 1 ].Trim( ']' );
            var defVal = f.GetValue( conf );

            var b = bind.MakeGenericMethod( f.FieldType ).Run( Config, section, f.Name, defVal, desc?.Comment );
            if ( TryGetBoxedValue( b, out var val ) ) {
               if ( ! Equals( val, defVal ) ) f.SetValue( conf, val );
               Fine( "Config {0} = {1}", f.Name, val );
            } else Warn( "Cannot read {0}", f.Name );

            b.GetType().GetEvent( "SettingChanged" )?.AddEventHandler( b, GetOnChangeListener( f, conf ) );
            if ( configBindings == null ) configBindings = new Dictionary< string, object >();
            configBindings.Add( f.Name, b );
         }
         if ( configBindings != null && configBindings.Count > 0 )
            Config.ConfigReloaded += GetOnReloadListener( conf );
         MarsHorizonMod.configLoaded = true;
      } catch ( Exception x ) { Err( x ); } }

      private bool TryGetBoxedValue ( object binding, out object value ) {
         var box = binding.GetType().Property( "BoxedValue" );
         if ( box == null ) { value = null; return false; }
         value = box.GetValue( binding );
         return true;
      }

      internal EventHandler GetOnChangeListener ( FieldInfo field, BaseConfig conf ) {
         return ( _, evt ) => {
            var e = evt as SettingChangedEventArgs;
            if ( e == null || e.ChangedSetting == null ) return;
            var val = e.ChangedSetting.BoxedValue;
            Info( "Config {0} changed to {1}.", field, val );
            field.SetValue( conf, val );
            MarsHorizonMod.ReApply();
         };
      }

      internal EventHandler GetOnReloadListener ( BaseConfig conf ) {
         return ( _, evt ) => {
            var type = conf.GetType();
            Info( "Config reloaded." );
            foreach ( var b in configBindings ) {
               if ( ! TryGetBoxedValue( b, out var val ) ) continue;
               Info( "  {0} = {1}", b.Key, val );
               type.Field( b.Key ).SetValue( conf, val );
            }
            MarsHorizonMod.ReApply();
         };
      }

      internal static void SetLogger ( ManualLogSource logger ) {
         if ( logger == null ) { ModComponent.Logger = null; return; }
         ModComponent.Logger = ( TraceLevel lv, object msg, object[] arg ) => {
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

   internal static class UMMUtil {
      internal static void Init ( UnityModManager.ModEntry modEntry, Type modType ) {
         ModDir = modEntry.Path;
         modType.Method( "Main" ).RunStatic();
         modEntry.OnToggle = ( _, on ) => { if ( on ) MarsHorizonMod.Apply(); else MarsHorizonMod.Unapply(); return true; };
         modEntry.OnUnload = ( _ ) => MarsHorizonMod.Unload();
      }
   }
}