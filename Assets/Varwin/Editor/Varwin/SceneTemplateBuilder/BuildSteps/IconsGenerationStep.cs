using System.IO;
using Varwin.Editor.PreviewGenerators;

namespace Varwin.SceneTemplateBuilding
{
    public class IconsGenerationStep : BaseSceneTemplateBuildStep
    {
        public IconsGenerationStep(SceneTemplateBuilder builder) : base(builder)
        {
        }
        
        public override void Update()
        {
            base.Update();
            
            var bundlePath = $"{Builder.DestinationFolder}/bundle.png";
            CreateIcon(bundlePath, Builder.PreviewCamera.GetTexturePngBytes(ImageSize.Square256));
            Builder.SceneTemplatePackingPaths.Add(bundlePath);
            
            var thumbnailPath = $"{Builder.DestinationFolder}/thumbnail.jpg";
            CreateIcon(thumbnailPath, Builder.PreviewCamera.GetTextureJpgBytes(ImageSize.MarketplaceLibrary));
            Builder.SceneTemplatePackingPaths.Add(thumbnailPath);
            
            var viewPath = $"{Builder.DestinationFolder}/view.jpg";
            CreateIcon(viewPath, Builder.PreviewCamera.GetTextureJpgBytes(ImageSize.FullHD));
            Builder.SceneTemplatePackingPaths.Add(viewPath);
        }

        private static void CreateIcon(string path, byte[] icon)
        {
            File.WriteAllBytes(path, icon);
        }
    }
}