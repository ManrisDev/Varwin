using Newtonsoft.Json;
using Varwin.Public;

namespace Varwin.SceneTemplateBuilding
{
    public class WorldDescriptorData
    {
        public LocalizationDictionary LocalizedName;
        public LocalizationDictionary LocalizedDescription;

        public string Guid;
        public string RootGuid;

        public string Image;
        public string AssetBundleLabel;
        public string[] DllNames;
        public string[] AsmdefNames;

        public string AuthorName;
        public string AuthorEmail;
        public string AuthorUrl;

        public string LicenseCode;
        public string LicenseVersion;

        public string BuiltAt;
        public bool SourcesIncluded;
        public bool MobileReady;

        public string SceneGuid;

        public bool CurrentVersionWasBuilt;
        public bool CurrentVersionWasBuiltAsMobileReady;
    
        public string Changelog;

        [JsonIgnore] public WorldDescriptor WorldDescriptor;

        public WorldDescriptorData()
        {
            
        }

        public WorldDescriptorData(WorldDescriptor worldDescriptor)
        {
            WorldDescriptor = worldDescriptor;

            LocalizedName = worldDescriptor.LocalizedName;
            LocalizedDescription = worldDescriptor.LocalizedDescription;

            Guid = worldDescriptor.Guid;
            RootGuid = worldDescriptor.RootGuid;

            Image = worldDescriptor.Image;
            AssetBundleLabel = worldDescriptor.AssetBundleLabel;
            DllNames = worldDescriptor.DllNames;
            AsmdefNames = worldDescriptor.AsmdefNames;

            AuthorName = worldDescriptor.AuthorName;
            AuthorEmail = worldDescriptor.AuthorEmail;
            AuthorUrl = worldDescriptor.AuthorUrl;

            LicenseCode = worldDescriptor.LicenseCode;
            LicenseVersion = worldDescriptor.LicenseVersion;

            BuiltAt = worldDescriptor.BuiltAt;
            SourcesIncluded = worldDescriptor.SourcesIncluded;
            MobileReady = worldDescriptor.MobileReady;

            SceneGuid = worldDescriptor.SceneGuid;

            CurrentVersionWasBuilt = worldDescriptor.CurrentVersionWasBuilt;
            CurrentVersionWasBuiltAsMobileReady = worldDescriptor.CurrentVersionWasBuiltAsMobileReady;

            Changelog = worldDescriptor.Changelog;
        }
    }
}