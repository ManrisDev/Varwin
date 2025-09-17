// ReSharper disable once CheckNamespace

using System;
using Newtonsoft.Json;

namespace Varwin.Data
{
    public class PrefabObjectContainer : IJsonSerializable
    {
        public PrefabObject Node { get; set; }
    }

    public class Package : IJsonSerializable
    {
        public int Id;
    }

    public class PrefabObject : IJsonSerializable
    {
        public static string EndCursor { get; set; }

        public int Id { get; set; }
        public string Guid { get; set; }
        public string RootGuid { get; set; }
        public I18n Name { get; set; }
        public bool Embedded { get; set; }
        public Config Config { get; set; }
        public string Assets { get; set; }
        public bool Paid { get; set; }
        public Package[] Packages { get; set; }
        public DateTime BuiltAt { get; set; }
        public bool LinuxReady { get; set; }
        public string InstallResource => Assets + "/install.json";
        public string BundleResource => Assets + "/bundle";
        public string BundleManifest => Assets + "/bundle.manifest";
        public string AndroidBundleResource => Assets + "/android_bundle";
        public string AndroidBundleManifest => Assets + "/android_bundle.manifest";
        public string LinuxBundleResource => Assets + "/linux_bundle";
        public string LinuxBundleManifest => Assets + "/linux_bundle.manifest";
        public string ConfigResource => Assets + "/bundle.json";
        public string IconResource => Assets + "/bundle.png";

        [JsonIgnore] public AssetInfo AssetInfo { get; set; }

        public string GetLocalizedName()
        {
            return Name.GetCurrentLocale();
        }
    }
}