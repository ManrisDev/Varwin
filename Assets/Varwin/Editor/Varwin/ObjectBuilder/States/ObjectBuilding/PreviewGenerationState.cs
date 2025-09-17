using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class PreviewGenerationState : BaseObjectBuildingState
    {
        public PreviewGenerationState(VarwinBuilder builder) : base(builder)
        {
            Label = string.Format(SdkTexts.CreatePreviewStep, "objects");
        }

        protected override void OnEnter()
        {
            if (Directory.Exists(VarwinBuildingPath.ObjectPreviews))
            {
                return;
            }
            
            try
            {
                Directory.CreateDirectory(VarwinBuildingPath.ObjectPreviews);
            }
            catch
            {
                string message = string.Format(SdkTexts.CannotCreateDirectoryFormat, VarwinBuildingPath.ObjectPreviews);
                Debug.LogError(message);
                EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle, message, "OK");
            }
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = string.Format(SdkTexts.CreatePreviewStep, currentObjectBuildDescription.ObjectName);
            
            try
            {
                GenerateSpritesheet(currentObjectBuildDescription);
                GenerateView(currentObjectBuildDescription);
                GenerateThumbnail(currentObjectBuildDescription);
            }
            catch (Exception e)
            {
                currentObjectBuildDescription.HasError = true;
                Debug.LogError($"{string.Format(SdkTexts.ProblemWhenCreatePreview, e.Message)}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
            }
        }

        private static void GenerateThumbnail(ObjectBuildDescription currentObjectBuildDescription)
        {
            if (currentObjectBuildDescription.ContainedObjectDescriptor.ThumbnailImage)
            {
                var savePath = ThumbnailBuilder.GetExportPath(null, VarwinBuildingPath.ObjectPreviews, currentObjectBuildDescription.RootGuid);

                var iconResourcePath = AssetDatabase.GetAssetPath(currentObjectBuildDescription.ContainedObjectDescriptor.ThumbnailImage);
                var bytes = File.ReadAllBytes(iconResourcePath);

                var texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);

                var pngBytes = texture.EncodeToJPG();
                File.WriteAllBytes(savePath, pngBytes);

                return;
            }
            
            ThumbnailBuilder.Build(currentObjectBuildDescription.ContainedObjectDescriptor, VarwinBuildingPath.ObjectPreviews, currentObjectBuildDescription);
        }

        private static void GenerateView(ObjectBuildDescription currentObjectBuildDescription)
        {
            if (currentObjectBuildDescription.ContainedObjectDescriptor.ViewImage)
            {
                var savePath = ViewBuilder.GetExportPath(null, VarwinBuildingPath.ObjectPreviews, currentObjectBuildDescription.RootGuid);

                var iconResourcePath = AssetDatabase.GetAssetPath(currentObjectBuildDescription.ContainedObjectDescriptor.ViewImage);
                var bytes = File.ReadAllBytes(iconResourcePath);

                var texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);

                var pngBytes = texture.EncodeToJPG();
                File.WriteAllBytes(savePath, pngBytes);

                return;
            }

            ViewBuilder.Build(currentObjectBuildDescription.ContainedObjectDescriptor,
                VarwinBuildingPath.ObjectPreviews,
                currentObjectBuildDescription
            );
        }

        private static void GenerateSpritesheet(ObjectBuildDescription currentObjectBuildDescription)
        {
            if (currentObjectBuildDescription.ContainedObjectDescriptor.SpritesheetImage)
            {
                var savePath = SpritesheetBuilder.GetExportPath(null, VarwinBuildingPath.ObjectPreviews, currentObjectBuildDescription.RootGuid);

                var iconResourcePath = AssetDatabase.GetAssetPath(currentObjectBuildDescription.ContainedObjectDescriptor.SpritesheetImage);
                var bytes = File.ReadAllBytes(iconResourcePath);

                var texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);

                var pngBytes = texture.EncodeToJPG();
                File.WriteAllBytes(savePath, pngBytes);

                return;
            }

            SpritesheetBuilder.Build(currentObjectBuildDescription.ContainedObjectDescriptor, VarwinBuildingPath.ObjectPreviews, currentObjectBuildDescription);
        }
    }
}