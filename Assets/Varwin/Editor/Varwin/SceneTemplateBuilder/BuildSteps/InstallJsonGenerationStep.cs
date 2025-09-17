using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class InstallJsonGenerationStep : BaseSceneTemplateBuildStep
    {
        public InstallJsonGenerationStep(SceneTemplateBuilder builder) : base(builder)
        {
        }
        
        public override void Update()
        {
            base.Update();

            var descriptor = Builder.WorldDescriptor;
            
            var builtAt = $"{DateTimeOffset.UtcNow:s}Z";
            if (DateTimeOffset.TryParse(descriptor.BuiltAt, out var builtAtDateTimeOffset))
            {
                builtAt = $"{builtAtDateTimeOffset.UtcDateTime:s}Z";
            }
            
            var installConfig = new SceneTemplateInstallConfig
            {
                Name = descriptor.LocalizedName.ToI18N(),
                Description = descriptor.LocalizedDescription.ToI18N(),
                Guid = descriptor.Guid,
                RootGuid = descriptor.RootGuid,
                Author = new()
                {
                    Name = descriptor.AuthorName,
                    Email = descriptor.AuthorEmail,
                    Url = descriptor.AuthorUrl,
                },
                BuiltAt = builtAt,
                License = new()
                {
                    Code = descriptor.LicenseCode,
                    Version = descriptor.LicenseVersion,
                },
                SourcesIncluded = descriptor.SourcesIncluded,
                MobileReady = SdkSettings.Features.Mobile.Enabled && descriptor.MobileReady,
                LinuxReady = SdkSettings.Features.Linux && BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64),
                SdkVersion = VarwinVersionInfo.VersionNumber,
                UnityVersion = Application.unityVersion,
                Changelog = new()
                {
                    en = SdkSettings.Features.Changelog ? descriptor.Changelog : string.Empty,
                    ru = SdkSettings.Features.Changelog ? descriptor.Changelog : string.Empty
                }
            };

            if (installConfig.Name == null || installConfig.Name.IsEmpty())
            {
                installConfig.Name = new()
                {
                    en = Builder.Scene.name,
                    ru = Builder.Scene.name
                };
            }

            var installJson = JsonConvert.SerializeObject(installConfig, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore
            });
            
            var installJsonPath = $"{Builder.DestinationFolder}/install.json";
            File.WriteAllText(installJsonPath, installJson);
            Builder.SceneTemplatePackingPaths.Add(installJsonPath);
        }
    }
}