using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Core;
using Varwin.Data;
using Varwin.Public;

namespace Varwin.Editor
{
    public class VarwinPackageInfo : IJsonSerializable
    {
        public string Name;
        public string Config;
        public LocalizationDictionary ViewPaths;
        public LocalizationDictionary ThumbnailPaths;
        public string[] VarwinObjects;
        public string[] VarwinScenes;
        public string[] VarwinResources;

        public VarwinPackageInfo()
        {
        }
        
        public VarwinPackageInfo(VarwinPackageDescriptor varwinPackageDescriptor)
        {
            Name = varwinPackageDescriptor.name;
            Config = GenerateInstallJson(varwinPackageDescriptor);
            ViewPaths = GetImagesPaths(varwinPackageDescriptor.ViewImage, varwinPackageDescriptor.View);
            ThumbnailPaths = GetImagesPaths(varwinPackageDescriptor.ThumbnailImage, varwinPackageDescriptor.Thumbnail);
            VarwinObjects = varwinPackageDescriptor.Objects?.Select(x => x.GetComponent<VarwinObjectDescriptor>()).Select(x => x.Name).ToArray();
            VarwinScenes = varwinPackageDescriptor.SceneTemplates?.Select(x => Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(x.Value))).ToArray();
            VarwinResources = varwinPackageDescriptor.Resources?.Select(AssetDatabase.GetAssetPath).ToArray();
        }

        private string GenerateInstallJson(VarwinPackageDescriptor varwinPackageDescriptor)
        {
            string builtAt = $"{DateTimeOffset.UtcNow:s}Z";

            if (DateTimeOffset.TryParse(varwinPackageDescriptor.BuiltAt, out DateTimeOffset builtAtDateTimeOffset))
            {
                builtAt = $"{builtAtDateTimeOffset.UtcDateTime:s}Z";
            }

            var installJson = new InstallJson
            {
                Name = varwinPackageDescriptor.Name.ToI18N(),
                Description = varwinPackageDescriptor.Description.ToI18N(),
                Guid = varwinPackageDescriptor.Guid,
                RootGuid = varwinPackageDescriptor.RootGuid,
                Locked = varwinPackageDescriptor.Locked,
                ViewImage = GetI18NImagesPaths("view", varwinPackageDescriptor.ViewImage),
                ThumbnailImage = GetI18NImagesPaths("thumbnail", varwinPackageDescriptor.ThumbnailImage),
                Author = new JsonAuthor
                {
                    Email = varwinPackageDescriptor.AuthorEmail,
                    Name = varwinPackageDescriptor.AuthorName,
                    Url = varwinPackageDescriptor.AuthorUrl,
                },
                License = new JsonLicense
                {
                    Code = varwinPackageDescriptor.LicenseCode,
                    Version = varwinPackageDescriptor.LicenseVersion,
                },
                BuiltAt = builtAt,
                SdkVersion = VarwinVersionInfo.VersionNumber,
            };
            
            var jsonSerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            string jsonConfig = JsonConvert.SerializeObject(installJson, Formatting.None, jsonSerializerSettings);

            return jsonConfig;
        }

        private LocalizationDictionary GetImagesPaths(LocalizedDictionary<Texture2D> viewImage, Texture2D view)
        {
            var result = new LocalizationDictionary();
            if (view)
            {
                result.Add(new LocalizationString(Language.English, AssetDatabase.GetAssetPath(view)));
                return result;
            }
            
            if (viewImage != null)
            {
                foreach (var language in viewImage.GetLanguages())
                {
                    var image = viewImage.GetValue(language);
                    result.Add(new LocalizationString(language, image ? AssetDatabase.GetAssetPath(image) : null));
                }

                if (!result.Contains(SystemLanguage.English))
                {
                    result.Add(new LocalizationString(Language.English, null));
                }
                
                return result;
            }
            
            result.Add(new LocalizationString(Language.English, null));
            return result;
        }
        
        private I18n GetI18NImagesPaths(string prefix, LocalizedDictionary<Texture2D> images)
        {
            if (images == null)
            {
                return new I18n
                {
                    en = $"{prefix}.jpg"
                };
            }
            
            var result = new I18n();
            foreach (var language in images.GetLanguages())
            {
                var locale = language.GetCode();
                result.SetLocale(locale, $"{prefix}{(language == Language.English ? "" : $"_{locale}")}.jpg");
            }
            
            if (string.IsNullOrWhiteSpace(result.en))
            {
                result.en = $"{prefix}.jpg";
            }

            return result;
        }

        private class InstallJson : IJsonSerializable
        {
            public I18n Name;
            public I18n Description;
            public I18n ViewImage;
            public I18n ThumbnailImage;
            public bool Locked;
            public string Guid;
            public string RootGuid;
            public JsonAuthor Author;
            public JsonLicense License;
            public string BuiltAt;
            public string SdkVersion;
        }
    }
}