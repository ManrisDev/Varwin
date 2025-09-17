using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class IconGenerationState : BaseObjectBuildingState
    {
        public IconGenerationState(VarwinBuilder builder) : base(builder)
        {
            Label = string.Format(SdkTexts.CreatingIconStepFormat, "objects");
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = string.Format(SdkTexts.CreatingIconStepFormat, currentObjectBuildDescription.ObjectName);

            try
            {
                if (currentObjectBuildDescription.ContainedObjectDescriptor.Icon)
                {
                    var savePath = currentObjectBuildDescription.IconPath;

                    var iconResourcePath = AssetDatabase.GetAssetPath(currentObjectBuildDescription.ContainedObjectDescriptor.Icon);
                    var bytes = File.ReadAllBytes(iconResourcePath);

                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(bytes);

                    var pngBytes = texture.EncodeToPNG();
                    File.WriteAllBytes(savePath, pngBytes);

                    return;
                }

                IconBuilder.Build(currentObjectBuildDescription);
            }
            catch (Exception e)
            {
                currentObjectBuildDescription.HasError = true;
                Debug.LogError($"{string.Format(SdkTexts.ProblemWhenCreatePreview, e.Message)}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
            }
        }

        protected override void OnExit()
        {
            Builder.Serialize();
        }
    }
}