using Varwin.Data;

namespace Varwin.SceneTemplateBuilding
{
    public class SceneTemplateInstallConfig : IJsonSerializable
    {
        public I18n Name;
        public I18n Description;
        public string Guid;
        public string RootGuid;
        public JsonAuthor Author;
        public JsonLicense License;
        public string BuiltAt;
        public bool SourcesIncluded;
        public bool MobileReady;
        public bool LinuxReady;
        public string SdkVersion;
        public string UnityVersion;
        public I18n Changelog;
    }
}