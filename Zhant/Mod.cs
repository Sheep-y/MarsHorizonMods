using System;
using System.Reflection;
using System.Text;
using static ZyMod.ModHelpers;

namespace ZyMod.MarsHorizon.Zhant {

   public class Mod : MarsHorizonMod {
      protected override string GetModName () => "Zhant";
      public static void Main () => new Mod { shouldLogAssembly = false }.Initialize();
      protected override void OnGameAssemblyLoaded ( Assembly game ) {
         ModPatcher.config.Load();
         new PatcherL10N().Apply();
      }
   }

   internal class ModPatcher : Patcher {
      internal static readonly Config config = new Config();
   }

   internal class Config : IniConfig {
      [ Config( "\r\n[字型]" ) ]
      [ Config( "字型檔案目錄及名稱。預設 NotoSans??-*.otf。" ) ]
      public string font= "NotoSans??-*.otf";
      [ Config( "相對於簡體字型的位置。-1 為正體優先，0 為取代，1 為簡體優先。預設 1，因為簡體字型有預先渲染，顯示速度快，雖然渲染質素會有細微差異。" ) ]
      public int tc_index = 1;

      [ Config( "\r\n[渲染]" ) ]
      [ Config( "SDF 素材地圖高度。過小會容易溢出導致缺字，過大則降低效能。預設 8192。" ) ]
      public uint atlas_height = 8192;
      [ Config( "SDF 素材地圖寬度。同上" ) ]
      public uint atlas_width = 8192;
      [ Config( "正常字型渲染大小。越大越精緻，速度越慢。預設 40。" ) ]
      public uint sample_size_normal = 40;
      [ Config( "粗體字型渲染大小。同上。預設 80。" ) ]
      public uint sample_size_bold = 80;
      [ Config( "渲染間距比率。過小會出現字框，過大浪費素材。預設 0.1 即 10%。" ) ]
      public float padding_ratio = 0.1f;

      [ Config( "\r\n" ) ]
      [ Config( "本設定檔的版本號。敬請勿動。" ) ]
      public int config_version = 20200322;

      public override void Load ( object subject, string path ) {
         base.Load( subject, path );
         if ( ! ( subject is Config conf ) || string.IsNullOrEmpty( font ) ) return;
         conf.tc_index = Math.Max( -1, Math.Min( conf.tc_index, 1 ) );
         conf.atlas_height = Math.Max( 512, conf.atlas_height );
         conf.atlas_width = Math.Max( 512, conf.atlas_width );
         conf.sample_size_bold = Math.Max( 8, conf.atlas_width );
         conf.sample_size_normal = Math.Max( 8, conf.atlas_width );
         if ( ! Rational( conf.padding_ratio ) || conf.padding_ratio < 0 )
            conf.padding_ratio = 0.1f;
      }
   }

}