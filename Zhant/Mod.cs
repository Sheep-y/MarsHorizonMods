using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModManagerNet;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {

   [ BepInPlugin( "Zy.MarsHorizon.Zhant", "Traditional Chinese", "0.0.2022.0326" ) ]
   public class Plugin : BaseUnityPlugin {
      private void Awake() => new Mod().Initialize();
      public void OnDestroy() => Mod.Unload();
   }

   [EnableReloading]
   public static class UMM_Mod {
      private static Mod mod;

      public static void Load ( UnityModManager.ModEntry modEntry ) {
         mod = new Mod();
         Mod.ModDir = modEntry.Path;
         mod.Initialize();
         modEntry.OnToggle = OnOff;
         modEntry.OnUnload = ( _ ) => Mod.Unload();
      }

      private static bool OnOff ( UnityModManager.ModEntry modEntry, bool active ) {
         Info( "UMM - Switching {0}.", active ? "On" : "Off" );
         if ( ! active ) Mod.patchers.Values.FirstOrDefault()?.UnpatchAll();
         else Mod.Load();
         return true;
      }
   }

   public class Mod : MarsHorizonMod {
      internal static string ModDir;
      protected override string GetModName () => "Zhant";
      public static void Main () => new Mod().Initialize();
      internal static readonly Dictionary< Type, ModPatcher > patchers = new Dictionary< Type, ModPatcher >();
      protected override void OnGameAssemblyLoaded ( Assembly game ) => Load();
      internal static void Load () {
         ModPatcher.config.Load();
         if ( ! patchers.ContainsKey( typeof( PatcherL10N ) ) )
            patchers.Add( typeof( PatcherL10N ), new PatcherL10N() );
         foreach ( var p in patchers.Values ) p.Apply();
      }
      internal static bool Unload () { try {
         Info( "Unloading." );
         patchers.Values.FirstOrDefault()?.UnpatchAll();
         foreach ( var p in patchers.Values ) p.Unload();
         Log.Close();
         return true;
      } catch ( Exception x ) { return Err( x, false ); } }
   }

   internal abstract class ModPatcher : Patcher {
      internal static readonly Config config = new Config();
      internal abstract void Apply();
      internal virtual void Unload() { }
   }

   internal class Config : IniConfig {
      [ Config( "是否在偵察到使用中文時才載入。設成 False 的話會無視遊戲的語言設定，恒常轉換所有語言。預設 True。" ) ]
      public bool dynamic_patch = true;

      [ Config( "\r\n[渲染]" ) ]
      [ Config( "SDF 素材地圖高度。過小會容易溢出導致缺字，過大則降低效能。預設 8192。最低 512，最高 16384。" ) ]
      public uint atlas_height = 8192;
      [ Config( "SDF 素材地圖寬度。同上" ) ]
      public uint atlas_width = 8192;
      [ Config( "一般字型渲染大小。越大越精緻，但速度越慢，遊戲引擎越易放棄。預設 40。" ) ]
      public uint sample_size_normal = 40;
      [ Config( "粗幼字型渲染大小。同上。預設 80。" ) ]
      public uint sample_size_other = 80;
      [ Config( "渲染間距比率。過小出現字框，過大浪費素材。預設 0.1 即 10%。" ) ]
      public float padding_ratio = 0.1f;

      [ Config( "\r\n" ) ]
      [ Config( "本設定檔的版本號。敬請勿動。" ) ]
      public int config_version = 20200322;

      public override void Load ( object subject, string path ) {
         base.Load( subject, path );
         if ( ! ( subject is Config conf ) ) return;
         conf.atlas_height = Math.Min( Math.Max( 512, conf.atlas_height ), 16384 );
         conf.atlas_width = Math.Min( Math.Max( 512, conf.atlas_width ), 16384 );
         conf.sample_size_other = Math.Max( 8, conf.sample_size_other );
         conf.sample_size_normal = Math.Max( 8, conf.sample_size_normal );
         if ( ! Rational( conf.padding_ratio ) || conf.padding_ratio < 0 )
            conf.padding_ratio = 0.1f;
      }
   }

}