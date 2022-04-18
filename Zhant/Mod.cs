using BepInEx;
using System;
using System.Reflection;
using UnityModManagerNet;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {
   [ BepInPlugin( "Zy.MarsHorizon.Zhant", "Traditional Chinese", "1.0" ) ]
   internal class BIE_Mod : BaseUnityPlugin {
      private void Awake() { BepInUtil.Setup( this, ModPatcher.config ); Mod.Main(); }
      private void OnDestroy() => BepInUtil.Unbind();
   }

   internal static class UMM_Mod {
      public static void Load ( object entry ) => UMMUtil.Init( entry, typeof( Mod ) );
   }

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Zhant";
      public static void Main () => new Mod().Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         ModPatcher.config.Load();
         ActivatePatcher( typeof( PatcherL10N ) );
      }
   }

   internal abstract class ModPatcher : MarsHorizonPatcher {
      internal static readonly Config config = new Config();
   }

   internal class Config : IniConfig {
      [ Config( "是否在偵察到使用中文時才載入。設成 False 的話會無視遊戲的語言設定，恒常轉換所有語言。預設 True。" ) ]
      public bool dynamic_patch = true;

      [ Config( "\r\n[渲染]" ) ]
      [ Config( "SDF 素材地圖高度。過小會容易溢出導致素材失效，過大則浪費資源。預設 8192。最低 512，最高 16384。" ) ]
      public int atlas_height = 8192;
      [ Config( "SDF 素材地圖寬度。同上" ) ]
      public int atlas_width = 8192;
      [ Config( "一般字型渲染大小。越大越精緻，但速度越慢，越易溢出導致素材失效。預設 40。" ) ]
      public int sample_size_normal = 40;
      [ Config( "粗體字型渲染大小。同上。預設 80。" ) ]
      public int sample_size_other = 80;
      [ Config( "渲染間距比率。過小出現字框，過大浪費素材。預設 0.1 即 10%。" ) ]
      public float padding_ratio = 0.1f;

      [ Config( "\r\n[Ɩnternal]" ) ]
      [ Config( "本設定檔的版本號。敬請勿動。" ) ]
      public int config_version = 20220322;

      protected override void OnLoad ( string _ ) {
         atlas_height = Math.Min( Math.Max( 512, atlas_height ), 16384 );
         atlas_width = Math.Min( Math.Max( 512, atlas_width ), 16384 );
         sample_size_other = Math.Min( Math.Max( 8, sample_size_other ), atlas_height / 16 );
         sample_size_normal = Math.Min( Math.Max( 8, sample_size_normal ), atlas_width / 16 );
         if ( ! Rational( padding_ratio ) || padding_ratio < 0 )
            padding_ratio = 0.1f;
         else
            padding_ratio = Math.Min( padding_ratio, 3 );
      }
   }

}